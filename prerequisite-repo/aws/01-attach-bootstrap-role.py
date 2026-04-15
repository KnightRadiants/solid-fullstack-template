#!/usr/bin/env python3
"""
Repository prerequisite step 1.

Attach one repository environment to the shared AWS bootstrap role.
This script expects the organization prerequisite to already exist.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from urllib.parse import unquote

BOOTSTRAP_ENVIRONMENT_NAME = "bootstrap"
DEFAULT_BOOTSTRAP_ROLE_NAME = "gha-bootstrap-org"
ANSI_RED = "\033[91m"
ANSI_RESET = "\033[0m"


def red(text: str) -> str:
    if not sys.stderr.isatty():
        return text
    return f"{ANSI_RED}{text}{ANSI_RESET}"


def print_step(message: str) -> None:
    print(f"[INFO] {message}", flush=True)


def run_command(command: list[str]) -> subprocess.CompletedProcess[str]:
    return subprocess.run(command, text=True, capture_output=True, check=False)


def run_command_checked(command: list[str], *, description: str = "") -> str:
    if description:
        print_step(description)
    result = run_command(command)
    if result.returncode != 0:
        raise RuntimeError(
            f"{description + chr(10) if description else ''}"
            f"Command failed ({result.returncode}): {' '.join(command)}\n"
            f"STDERR:\n{result.stderr}\nSTDOUT:\n{result.stdout}"
        )
    return result.stdout.strip()


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Allow one repo bootstrap environment to assume gha-bootstrap-org.")
    parser.add_argument("--org", required=True, help="GitHub owner")
    parser.add_argument("--repo", required=True, help="GitHub repository")
    parser.add_argument("--aws-profile", default="", help="AWS profile to use")
    return parser.parse_args()


def build_subject(*, org: str, repo: str) -> str:
    return f"repo:{org}/{repo}:environment:{BOOTSTRAP_ENVIRONMENT_NAME}"


def normalize_subject(pattern: str) -> str:
    normalized = pattern.strip()
    legacy_repo_match = re.fullmatch(r"repo:([^/]+)/([^:]+):\*", normalized)
    if legacy_repo_match:
        return build_subject(org=legacy_repo_match.group(1), repo=legacy_repo_match.group(2))

    legacy_main_match = re.fullmatch(r"repo:([^/]+)/([^:]+):ref:refs/heads/main", normalized)
    if legacy_main_match:
        return build_subject(org=legacy_main_match.group(1), repo=legacy_main_match.group(2))

    return normalized


def dedupe(values: list[str]) -> list[str]:
    seen: set[str] = set()
    result: list[str] = []
    for value in values:
        normalized = value.strip()
        if not normalized or normalized in seen:
            continue
        seen.add(normalized)
        result.append(normalized)
    return result


def list_statements(policy_document: dict[str, object]) -> list[dict[str, object]]:
    raw_statements = policy_document.get("Statement", [])
    if isinstance(raw_statements, dict):
        return [raw_statements]
    if isinstance(raw_statements, list):
        return [statement for statement in raw_statements if isinstance(statement, dict)]
    raise RuntimeError("IAM trust policy does not contain a valid Statement list.")


def is_github_oidc_statement(statement: dict[str, object]) -> bool:
    raw_action = statement.get("Action", [])
    if isinstance(raw_action, str):
        actions = [raw_action]
    elif isinstance(raw_action, list):
        actions = [str(action).strip() for action in raw_action]
    else:
        return False

    if "sts:AssumeRoleWithWebIdentity" not in actions:
        return False

    principal = statement.get("Principal", {})
    if not isinstance(principal, dict):
        return True

    federated = principal.get("Federated", [])
    if isinstance(federated, str):
        federated_values = [federated]
    elif isinstance(federated, list):
        federated_values = [str(value).strip() for value in federated]
    else:
        federated_values = []

    return not federated_values or any("token.actions.githubusercontent.com" in value for value in federated_values)


def extract_subjects(statement: dict[str, object]) -> list[str]:
    condition = statement.get("Condition", {})
    if not isinstance(condition, dict):
        return []

    subjects: list[str] = []
    for operator_value in condition.values():
        if not isinstance(operator_value, dict):
            continue
        raw_subjects = operator_value.get("token.actions.githubusercontent.com:sub")
        if isinstance(raw_subjects, str):
            subjects.append(raw_subjects)
        elif isinstance(raw_subjects, list):
            subjects.extend(str(value) for value in raw_subjects)
    return dedupe(subjects)


def load_policy() -> dict[str, object]:
    output = run_command_checked(
        [
            "aws",
            "iam",
            "get-role",
            "--role-name",
            DEFAULT_BOOTSTRAP_ROLE_NAME,
            "--query",
            "Role.AssumeRolePolicyDocument",
            "--output",
            "json",
        ],
        description=f"Reading IAM trust policy for role '{DEFAULT_BOOTSTRAP_ROLE_NAME}'...",
    )
    parsed = json.loads(output)
    if isinstance(parsed, dict):
        return parsed
    if isinstance(parsed, str):
        return json.loads(unquote(parsed))
    raise RuntimeError(f"Unsupported trust policy payload for role '{DEFAULT_BOOTSTRAP_ROLE_NAME}'.")


def update_policy_subjects(policy_document: dict[str, object], subjects: list[str]) -> dict[str, object]:
    updated = False
    for statement in list_statements(policy_document):
        if not is_github_oidc_statement(statement):
            continue

        condition = statement.setdefault("Condition", {})
        if not isinstance(condition, dict):
            raise RuntimeError("IAM trust policy contains an invalid Condition block.")

        for operator_name, operator_values in list(condition.items()):
            if operator_name == "StringLike" or not isinstance(operator_values, dict):
                continue
            operator_values.pop("token.actions.githubusercontent.com:sub", None)

        string_like = condition.setdefault("StringLike", {})
        if not isinstance(string_like, dict):
            raise RuntimeError("IAM trust policy contains an invalid StringLike block.")

        string_like["token.actions.githubusercontent.com:sub"] = subjects
        updated = True

    if not updated:
        raise RuntimeError(f"Role '{DEFAULT_BOOTSTRAP_ROLE_NAME}' does not contain a GitHub OIDC trust statement.")
    return policy_document


def main() -> int:
    args = parse_args()
    if args.aws_profile.strip():
        os.environ["AWS_PROFILE"] = args.aws_profile.strip()

    print_step(f"Attaching repo '{args.org}/{args.repo}' to AWS bootstrap role.")
    run_command_checked(["aws", "sts", "get-caller-identity"], description="Validating AWS credentials...")

    policy_document = load_policy()
    current_subjects: list[str] = []
    for statement in list_statements(policy_document):
        if is_github_oidc_statement(statement):
            current_subjects.extend(extract_subjects(statement))

    desired_subject = build_subject(org=args.org, repo=args.repo)
    desired_subjects = dedupe([normalize_subject(subject) for subject in current_subjects] + [desired_subject])

    if set(current_subjects) == set(desired_subjects) and len(current_subjects) == len(desired_subjects):
        print_step(f"Role '{DEFAULT_BOOTSTRAP_ROLE_NAME}' already allows '{desired_subject}'.")
        return 0

    updated_policy = update_policy_subjects(policy_document, desired_subjects)
    run_command_checked(
        [
            "aws",
            "iam",
            "update-assume-role-policy",
            "--role-name",
            DEFAULT_BOOTSTRAP_ROLE_NAME,
            "--policy-document",
            json.dumps(updated_policy, separators=(",", ":")),
        ],
        description=f"Updating role '{DEFAULT_BOOTSTRAP_ROLE_NAME}' trust policy...",
    )
    print_step(f"Added trust subject: {desired_subject}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:  # noqa: BLE001
        print(red(f"[ERROR] {exc}"), file=sys.stderr)
        raise SystemExit(1)
