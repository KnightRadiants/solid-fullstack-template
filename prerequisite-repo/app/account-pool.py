#!/usr/bin/env python3
"""
AWS Organizations account pool operations.

The pool is an OU named "Unused". Safe bootstrap runs can reuse accounts
from this OU before Terraform creates new ones.
"""

from __future__ import annotations

import argparse
import json
import os
import re
import subprocess
import sys
from typing import Any

DEFAULT_UNUSED_OU_NAME = "Unused"
SAFE_RESOURCE_PREFIX = "aws_organizations_account.app_safe"
DEBUG_RESOURCE_PREFIX = "aws_organizations_account.app_debug"
ACCOUNT_MANAGEMENT_SERVICE_PRINCIPAL = "account.amazonaws.com"
ANSI_RED = "\033[91m"
ANSI_RESET = "\033[0m"


def red(text: str) -> str:
    if not sys.stderr.isatty():
        return text
    return f"{ANSI_RED}{text}{ANSI_RESET}"


def print_step(message: str) -> None:
    print(f"[INFO] {message}", file=sys.stderr, flush=True)


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


def aws_command(args: argparse.Namespace, service_args: list[str]) -> list[str]:
    command = ["aws", *service_args]
    if getattr(args, "aws_region", "").strip():
        command.extend(["--region", args.aws_region.strip()])
    if getattr(args, "aws_profile", "").strip():
        command.extend(["--profile", args.aws_profile.strip()])
    return command


def aws_json(args: argparse.Namespace, service_args: list[str], *, description: str = "") -> dict[str, Any]:
    output = run_command_checked(
        aws_command(args, [*service_args, "--output", "json"]),
        description=description,
    )
    if not output:
        return {}
    parsed = json.loads(output)
    if not isinstance(parsed, dict):
        raise RuntimeError(f"Expected JSON object from aws {' '.join(service_args)}.")
    return parsed


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Manage the AWS account pool OU.")
    parser.add_argument("--aws-region", default="", help="AWS region for CLI calls")
    parser.add_argument("--aws-profile", default="", help="AWS profile for CLI calls")

    subparsers = parser.add_subparsers(dest="command", required=True)

    allocate = subparsers.add_parser("allocate", help="Reuse accounts from the Unused OU for one app.")
    allocate.add_argument("--app-slug", required=True, help="Application slug")
    allocate.add_argument(
        "--environment-accounts-json",
        required=True,
        help="JSON list of environment account names, e.g. '[\"dev\",\"prod\"]'",
    )
    allocate.add_argument(
        "--bootstrap-mode",
        choices=["safe", "debug"],
        default="safe",
        help="Terraform resource family to import into",
    )
    allocate.add_argument("--debug-suffix", default="", help="Debug suffix used by the Terraform naming convention")
    allocate.add_argument("--unused-ou-name", default=DEFAULT_UNUSED_OU_NAME, help="Account pool OU name")

    archive = subparsers.add_parser("archive", help="Move one app's accounts into the Unused OU.")
    archive.add_argument("--app-slug", required=True, help="Application slug")
    archive.add_argument("--unused-ou-name", default=DEFAULT_UNUSED_OU_NAME, help="Account pool OU name")

    list_pool = subparsers.add_parser("list", help="List accounts currently in the Unused OU.")
    list_pool.add_argument("--unused-ou-name", default=DEFAULT_UNUSED_OU_NAME, help="Account pool OU name")

    return parser.parse_args()


def normalize_slug(value: str) -> str:
    normalized = value.strip().lower()
    if not re.fullmatch(r"[a-z0-9]+(?:-[a-z0-9]+)*", normalized):
        raise RuntimeError("app_slug must use kebab-case: lowercase letters, digits, hyphen.")
    return normalized


def normalize_environment(value: str) -> str:
    normalized = value.strip().lower()
    if not re.fullmatch(r"[a-z0-9-]+", normalized):
        raise RuntimeError(f"Invalid environment account name: {value!r}.")
    return normalized


def parse_environment_accounts(raw_json: str) -> list[str]:
    parsed = json.loads(raw_json)
    if not isinstance(parsed, list) or not parsed:
        raise RuntimeError("environment_accounts_json must be a non-empty JSON list.")
    return [normalize_environment(str(value)) for value in parsed]


def app_ou_name(app_slug: str) -> str:
    return f"APP-{app_slug.upper()}"


def account_debug_suffix(*, app_slug: str, debug_suffix: str) -> str:
    normalized_suffix = debug_suffix.strip().lower()
    if not normalized_suffix:
        return ""
    suffix_segment = f"-{normalized_suffix}"
    return "" if app_slug.endswith(suffix_segment) else suffix_segment


def desired_account_name(*, app_slug: str, environment_name: str, debug_suffix: str) -> str:
    suffix = account_debug_suffix(app_slug=app_slug, debug_suffix=debug_suffix)
    return f"APP-{app_slug.upper()}-{environment_name.upper()}{suffix.upper()}"


def unused_account_name(*, app_slug: str, account_name: str, account_id: str) -> str:
    base = f"UNUSED-{app_slug.upper()}-{account_name.upper()}-{account_id[-4:]}"
    return base[:50]


def terraform_resource_address(*, environment_name: str, bootstrap_mode: str) -> str:
    prefix = SAFE_RESOURCE_PREFIX if bootstrap_mode == "safe" else DEBUG_RESOURCE_PREFIX
    return f'{prefix}["{environment_name}"]'


def get_root_id(args: argparse.Namespace) -> str:
    data = aws_json(args, ["organizations", "list-roots"], description="Reading AWS Organizations root...")
    roots = data.get("Roots", [])
    if not isinstance(roots, list) or not roots:
        raise RuntimeError("Could not find AWS Organizations root.")
    root = roots[0]
    if not isinstance(root, dict) or not str(root.get("Id", "")).strip():
        raise RuntimeError("AWS Organizations root payload is invalid.")
    return str(root["Id"])


def list_ous(args: argparse.Namespace, parent_id: str) -> list[dict[str, Any]]:
    data = aws_json(
        args,
        ["organizations", "list-organizational-units-for-parent", "--parent-id", parent_id],
        description=f"Listing OUs under parent '{parent_id}'...",
    )
    ous = data.get("OrganizationalUnits", [])
    if not isinstance(ous, list):
        raise RuntimeError("Invalid OrganizationalUnits payload.")
    return [ou for ou in ous if isinstance(ou, dict)]


def find_ou(args: argparse.Namespace, *, parent_id: str, name: str) -> dict[str, Any] | None:
    matches = [ou for ou in list_ous(args, parent_id) if str(ou.get("Name", "")) == name]
    if len(matches) > 1:
        raise RuntimeError(f"Found multiple OUs named '{name}' under parent '{parent_id}'.")
    return matches[0] if matches else None


def ensure_ou(args: argparse.Namespace, *, parent_id: str, name: str) -> dict[str, Any]:
    existing = find_ou(args, parent_id=parent_id, name=name)
    if existing:
        return existing

    data = aws_json(
        args,
        ["organizations", "create-organizational-unit", "--parent-id", parent_id, "--name", name],
        description=f"Creating OU '{name}'...",
    )
    ou = data.get("OrganizationalUnit")
    if not isinstance(ou, dict):
        raise RuntimeError(f"Could not create OU '{name}'.")
    return ou


def list_accounts(args: argparse.Namespace, parent_id: str) -> list[dict[str, Any]]:
    data = aws_json(
        args,
        ["organizations", "list-accounts-for-parent", "--parent-id", parent_id],
        description=f"Listing accounts under parent '{parent_id}'...",
    )
    accounts = data.get("Accounts", [])
    if not isinstance(accounts, list):
        raise RuntimeError("Invalid Accounts payload.")
    return [account for account in accounts if isinstance(account, dict)]


def active_accounts(accounts: list[dict[str, Any]]) -> list[dict[str, Any]]:
    return [account for account in accounts if str(account.get("Status", "")).upper() == "ACTIVE"]


def account_id(account: dict[str, Any]) -> str:
    value = str(account.get("Id", "")).strip()
    if not re.fullmatch(r"\d{12}", value):
        raise RuntimeError(f"Invalid account payload: {account}")
    return value


def account_name(account: dict[str, Any]) -> str:
    return str(account.get("Name", "")).strip()


def rename_account(args: argparse.Namespace, *, target_account_id: str, new_name: str) -> None:
    run_command_checked(
        aws_command(
            args,
            ["account", "put-account-name", "--account-id", target_account_id, "--account-name", new_name],
        ),
        description=f"Renaming account '{target_account_id}' to '{new_name}'...",
    )


def move_account(args: argparse.Namespace, *, target_account_id: str, source_parent_id: str, destination_parent_id: str) -> None:
    run_command_checked(
        aws_command(
            args,
            [
                "organizations",
                "move-account",
                "--account-id",
                target_account_id,
                "--source-parent-id",
                source_parent_id,
                "--destination-parent-id",
                destination_parent_id,
            ],
        ),
        description=f"Moving account '{target_account_id}' to parent '{destination_parent_id}'...",
    )


def tag_account(args: argparse.Namespace, *, target_account_id: str, tags: dict[str, str]) -> None:
    tag_args = [f"Key={key},Value={value}" for key, value in tags.items()]
    run_command_checked(
        aws_command(args, ["organizations", "tag-resource", "--resource-id", target_account_id, "--tags", *tag_args]),
        description=f"Tagging account '{target_account_id}'...",
    )


def account_management_trusted_access_enabled(args: argparse.Namespace) -> bool:
    data = aws_json(
        args,
        ["organizations", "list-aws-service-access-for-organization"],
        description="Checking trusted access for AWS Account Management...",
    )
    principals = data.get("EnabledServicePrincipals", [])
    if not isinstance(principals, list):
        raise RuntimeError("Invalid EnabledServicePrincipals payload.")

    return any(
        isinstance(principal, dict)
        and str(principal.get("ServicePrincipal", "")).strip() == ACCOUNT_MANAGEMENT_SERVICE_PRINCIPAL
        for principal in principals
    )


def ensure_account_management_trusted_access(args: argparse.Namespace) -> None:
    if account_management_trusted_access_enabled(args):
        print_step("Trusted access for AWS Account Management is already enabled.")
        return

    run_command_checked(
        aws_command(
            args,
            [
                "organizations",
                "enable-aws-service-access",
                "--service-principal",
                ACCOUNT_MANAGEMENT_SERVICE_PRINCIPAL,
            ],
        ),
        description="Enabling trusted access for AWS Account Management...",
    )


def allocate(args: argparse.Namespace) -> dict[str, Any]:
    app_slug_value = normalize_slug(args.app_slug)
    environments = parse_environment_accounts(args.environment_accounts_json)
    ensure_account_management_trusted_access(args)

    root_id = get_root_id(args)
    app_ou = find_ou(args, parent_id=root_id, name=app_ou_name(app_slug_value))
    if not app_ou:
        raise RuntimeError(
            f"Application OU '{app_ou_name(app_slug_value)}' does not exist yet. "
            "Create/import the OU before allocating accounts from the pool."
        )
    unused_ou = ensure_ou(args, parent_id=root_id, name=args.unused_ou_name)
    app_ou_id = str(app_ou["Id"])
    unused_ou_id = str(unused_ou["Id"])

    app_accounts = active_accounts(list_accounts(args, app_ou_id))
    pool_accounts = active_accounts(list_accounts(args, unused_ou_id))
    app_accounts_by_name = {account_name(account): account for account in app_accounts}

    imports: list[dict[str, str]] = []
    reused: dict[str, str] = {}
    existing: dict[str, str] = {}
    missing: list[str] = []

    for environment_name in environments:
        target_name = desired_account_name(
            app_slug=app_slug_value,
            environment_name=environment_name,
            debug_suffix=args.debug_suffix,
        )
        resource_address = terraform_resource_address(
            environment_name=environment_name,
            bootstrap_mode=args.bootstrap_mode,
        )

        if target_name in app_accounts_by_name:
            target_account_id = account_id(app_accounts_by_name[target_name])
            existing[environment_name] = target_account_id
            imports.append(
                {
                    "environment": environment_name,
                    "account_id": target_account_id,
                    "resource_address": resource_address,
                    "source": "app-ou",
                }
            )
            continue

        if not pool_accounts:
            missing.append(environment_name)
            continue

        account = pool_accounts.pop(0)
        target_account_id = account_id(account)
        rename_account(args, target_account_id=target_account_id, new_name=target_name)
        move_account(
            args,
            target_account_id=target_account_id,
            source_parent_id=unused_ou_id,
            destination_parent_id=app_ou_id,
        )
        tag_account(
            args,
            target_account_id=target_account_id,
            tags={
                "ManagedBy": "Terraform",
                "App": app_slug_value,
                "Account": environment_name,
                "PoolStatus": "reused",
            },
        )
        reused[environment_name] = target_account_id
        imports.append(
            {
                "environment": environment_name,
                "account_id": target_account_id,
                "resource_address": resource_address,
                "source": "unused-ou",
            }
        )

    return {
        "app_ou_id": app_ou_id,
        "unused_ou_id": unused_ou_id,
        "existing": existing,
        "reused": reused,
        "missing": missing,
        "imports": imports,
    }


def archive(args: argparse.Namespace) -> dict[str, Any]:
    app_slug_value = normalize_slug(args.app_slug)
    ensure_account_management_trusted_access(args)

    root_id = get_root_id(args)
    app_ou = find_ou(args, parent_id=root_id, name=app_ou_name(app_slug_value))
    if not app_ou:
        raise RuntimeError(f"Application OU '{app_ou_name(app_slug_value)}' does not exist.")
    unused_ou = ensure_ou(args, parent_id=root_id, name=args.unused_ou_name)

    app_ou_id = str(app_ou["Id"])
    unused_ou_id = str(unused_ou["Id"])
    accounts = active_accounts(list_accounts(args, app_ou_id))
    archived: list[dict[str, str]] = []

    for account in accounts:
        target_account_id = account_id(account)
        old_name = account_name(account)
        new_name = unused_account_name(app_slug=app_slug_value, account_name=old_name, account_id=target_account_id)
        rename_account(args, target_account_id=target_account_id, new_name=new_name)
        move_account(
            args,
            target_account_id=target_account_id,
            source_parent_id=app_ou_id,
            destination_parent_id=unused_ou_id,
        )
        tag_account(
            args,
            target_account_id=target_account_id,
            tags={
                "ManagedBy": "AccountPool",
                "PreviousApp": app_slug_value,
                "PoolStatus": "unused",
            },
        )
        archived.append({"account_id": target_account_id, "old_name": old_name, "new_name": new_name})

    remaining_accounts = active_accounts(list_accounts(args, app_ou_id))
    if not remaining_accounts:
        run_command_checked(
            aws_command(args, ["organizations", "delete-organizational-unit", "--organizational-unit-id", app_ou_id]),
            description=f"Deleting empty application OU '{app_ou_name(app_slug_value)}'...",
        )

    return {
        "app_ou_id": app_ou_id,
        "unused_ou_id": unused_ou_id,
        "archived": archived,
        "deleted_app_ou": not remaining_accounts,
    }


def list_pool(args: argparse.Namespace) -> dict[str, Any]:
    root_id = get_root_id(args)
    unused_ou = ensure_ou(args, parent_id=root_id, name=args.unused_ou_name)
    unused_ou_id = str(unused_ou["Id"])
    accounts = active_accounts(list_accounts(args, unused_ou_id))
    return {
        "unused_ou_id": unused_ou_id,
        "accounts": [
            {
                "account_id": account_id(account),
                "name": account_name(account),
                "email": str(account.get("Email", "")).strip(),
            }
            for account in accounts
        ],
    }


def main() -> int:
    args = parse_args()
    if args.aws_profile.strip():
        os.environ["AWS_PROFILE"] = args.aws_profile.strip()

    run_command_checked(["aws", "sts", "get-caller-identity"], description="Validating AWS credentials...")

    if args.command == "allocate":
        result = allocate(args)
    elif args.command == "archive":
        result = archive(args)
    elif args.command == "list":
        result = list_pool(args)
    else:
        raise RuntimeError(f"Unsupported command: {args.command}")

    print(json.dumps(result, separators=(",", ":")))
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:  # noqa: BLE001
        print(red(f"[ERROR] {exc}"), file=sys.stderr)
        raise SystemExit(1)
