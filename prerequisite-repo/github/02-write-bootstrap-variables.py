#!/usr/bin/env python3
"""
Repository prerequisite step 2.

Create the bootstrap environment and write non-secret variables used by
the bootstrap-repo workflow.
"""

from __future__ import annotations

import argparse
import os
import subprocess
import sys

BOOTSTRAP_ENVIRONMENT_NAME = "bootstrap"
DEFAULT_BOOTSTRAP_ROLE_NAME = "gha-bootstrap-org"
DEFAULT_LOCK_TABLE_NAME = "terraform-locks"
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
    parser = argparse.ArgumentParser(description="Write bootstrap environment variables for one repository.")
    parser.add_argument("--org", required=True, help="GitHub owner")
    parser.add_argument("--repo", required=True, help="GitHub repository")
    parser.add_argument("--aws-region", required=True, help="AWS region")
    parser.add_argument("--aws-profile", default="", help="AWS profile to use")
    return parser.parse_args()


def build_tf_state_bucket_name(*, aws_account_id: str, aws_region: str) -> str:
    return f"tfstate-{aws_account_id}-{aws_region}"


def ensure_bootstrap_environment(*, owner: str, repo: str) -> None:
    run_command_checked(
        [
            "gh",
            "api",
            "--method",
            "PUT",
            f"/repos/{owner}/{repo}/environments/{BOOTSTRAP_ENVIRONMENT_NAME}",
        ],
        description=f"Ensuring GitHub environment '{BOOTSTRAP_ENVIRONMENT_NAME}' exists...",
    )


def set_variable(*, owner: str, repo: str, name: str, value: str) -> None:
    run_command_checked(
        [
            "gh",
            "variable",
            "set",
            name,
            "--body",
            value,
            "--env",
            BOOTSTRAP_ENVIRONMENT_NAME,
            "--repo",
            f"{owner}/{repo}",
        ],
        description=f"Setting bootstrap environment variable '{name}'...",
    )


def main() -> int:
    args = parse_args()
    if args.aws_profile.strip():
        os.environ["AWS_PROFILE"] = args.aws_profile.strip()

    print_step(f"Writing bootstrap variables for repo '{args.org}/{args.repo}'.")
    run_command_checked(["gh", "--version"], description="Checking GitHub CLI...")
    run_command_checked(["aws", "sts", "get-caller-identity"], description="Validating AWS credentials...")

    aws_account_id = run_command_checked(
        ["aws", "sts", "get-caller-identity", "--query", "Account", "--output", "text"],
        description="Reading AWS management account ID...",
    ).strip()
    tf_state_bucket = build_tf_state_bucket_name(aws_account_id=aws_account_id, aws_region=args.aws_region)

    run_command_checked(
        ["aws", "s3api", "head-bucket", "--bucket", tf_state_bucket],
        description=f"Checking Terraform state bucket '{tf_state_bucket}'...",
    )
    run_command_checked(
        ["aws", "dynamodb", "describe-table", "--table-name", DEFAULT_LOCK_TABLE_NAME, "--region", args.aws_region],
        description=f"Checking Terraform lock table '{DEFAULT_LOCK_TABLE_NAME}'...",
    )
    run_command_checked(
        ["aws", "iam", "get-role", "--role-name", DEFAULT_BOOTSTRAP_ROLE_NAME],
        description=f"Checking bootstrap role '{DEFAULT_BOOTSTRAP_ROLE_NAME}'...",
    )

    ensure_bootstrap_environment(owner=args.org, repo=args.repo)
    variables = {
        "AWS_REGION": args.aws_region,
        "AWS_ACCOUNT_ID": aws_account_id,
        "BOOTSTRAP_ROLE_NAME": DEFAULT_BOOTSTRAP_ROLE_NAME,
        "TF_STATE_BUCKET": tf_state_bucket,
    }
    for name, value in variables.items():
        set_variable(owner=args.org, repo=args.repo, name=name, value=value)

    print_step("Bootstrap variables are ready.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:  # noqa: BLE001
        print(red(f"[ERROR] {exc}"), file=sys.stderr)
        raise SystemExit(1)
