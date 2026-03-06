#!/usr/bin/env python3
"""
Local orchestrator for GitHub prerequisite bootstrap.

Stage order:
1) Ensure GitHub App credentials (create once, then reuse from app/out).
2) Ensure administrators team baseline.
3) Upsert required bootstrap secrets/variables via gh CLI.
"""

from __future__ import annotations

import argparse
import json
import re
import subprocess
import sys
from pathlib import Path


def run_command(command: list[str]) -> subprocess.CompletedProcess[str]:
    return subprocess.run(command, text=True, capture_output=True, check=False)


def run_command_checked(command: list[str]) -> str:
    result = run_command(command)
    if result.returncode != 0:
        message = (
            f"Command failed ({result.returncode}): {' '.join(command)}\n"
            f"STDERR:\n{result.stderr}\nSTDOUT:\n{result.stdout}"
        )
        raise RuntimeError(message)
    return result.stdout.strip()


def slugify(value: str) -> str:
    slug = value.strip().lower()
    slug = re.sub(r"[^a-z0-9]+", "-", slug)
    slug = re.sub(r"-{2,}", "-", slug).strip("-")
    return slug


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Bootstrap GitHub prerequisite (app + team + configuration).")
    parser.add_argument("--org", required=True, help="GitHub organization")
    parser.add_argument("--bootstrap-repo", required=True, help="Repository name that receives bootstrap settings")
    parser.add_argument(
        "--scope",
        choices=["org", "repo"],
        default="org",
        help="Where to write GH_APP_* and bootstrap vars",
    )
    parser.add_argument("--app-name", default="gha-template-bootstrap", help="GitHub App name")
    parser.add_argument(
        "--app-description",
        default="Bootstrap app for template governance",
        help="GitHub App description",
    )
    parser.add_argument("--homepage-url", default="", help="Optional app homepage URL")
    parser.add_argument("--output-dir", default="app/out", help="Directory for app credentials output")
    parser.add_argument("--open-browser", action="store_true", help="Open browser for manifest flow")
    parser.add_argument("--force-create-app", action="store_true", help="Force creating a new app")

    parser.add_argument("--team-name", default="administrators", help="Administrators team name")
    parser.add_argument(
        "--team-description",
        default="Template bootstrap administrators",
        help="Administrators team description",
    )
    parser.add_argument("--team-maintainers", default="", help="Comma-separated maintainer logins")
    parser.add_argument("--team-members", default="", help="Comma-separated member logins")
    parser.add_argument(
        "--skip-team-repo-admin-grant",
        action="store_true",
        help="Skip granting admin permission on bootstrap repo to administrators team",
    )

    parser.add_argument("--aws-region", default="", help="Value for AWS_REGION variable")
    parser.add_argument("--aws-role-to-assume", default="", help="Value for AWS_ROLE_TO_ASSUME variable")
    parser.add_argument("--tf-state-bucket", default="", help="Value for TF_STATE_BUCKET variable")
    parser.add_argument("--tf-lock-table", default="", help="Value for TF_LOCK_TABLE variable")
    parser.add_argument("--tf-state-key-prefix", default="", help="Optional TF_STATE_KEY_PREFIX variable")
    return parser.parse_args()


def find_existing_credentials(output_dir: Path, app_name: str) -> tuple[Path, Path, dict] | None:
    expected_slug = slugify(app_name)
    candidates: list[tuple[float, Path, Path, dict]] = []

    for credentials_file in output_dir.glob("github-app-*.credentials.json"):
        try:
            data = json.loads(credentials_file.read_text(encoding="utf-8"))
        except json.JSONDecodeError:
            continue

        app_slug = str(data.get("slug", "")).strip().lower()
        app_display_name = str(data.get("name", "")).strip()
        app_id = data.get("id")
        if not app_id:
            continue

        if app_display_name != app_name and app_slug != expected_slug:
            continue

        private_key_file = output_dir / f"github-app-{app_id}.private-key.pem"
        if not private_key_file.exists():
            continue

        candidates.append((credentials_file.stat().st_mtime, credentials_file, private_key_file, data))

    if not candidates:
        return None

    candidates.sort(key=lambda item: item[0], reverse=True)
    _, credentials, private_key, payload = candidates[0]
    return credentials, private_key, payload


def run_app_bootstrap(
    script_path: Path,
    org: str,
    app_name: str,
    app_description: str,
    homepage_url: str,
    output_dir: Path,
    open_browser: bool,
) -> None:
    command = [
        sys.executable,
        str(script_path),
        "--org",
        org,
        "--app-name",
        app_name,
        "--description",
        app_description,
        "--output-dir",
        str(output_dir),
    ]
    if homepage_url.strip():
        command.extend(["--homepage-url", homepage_url.strip()])
    if open_browser:
        command.append("--open-browser")
    run_command_checked(command)


def split_csv(raw_value: str) -> list[str]:
    parts = [part.strip() for part in raw_value.split(",")]
    return [part for part in parts if part]


def ensure_team(
    team_script_path: Path,
    org: str,
    team_name: str,
    team_description: str,
    maintainers: str,
    members: str,
    admin_repo: str | None,
) -> None:
    command = [
        sys.executable,
        str(team_script_path),
        "--org",
        org,
        "--team-name",
        team_name,
        "--team-description",
        team_description,
    ]
    if maintainers.strip():
        command.extend(["--maintainers", maintainers.strip()])
    if members.strip():
        command.extend(["--members", members.strip()])
    if admin_repo:
        command.extend(["--admin-repos", admin_repo])
    run_command_checked(command)


def set_secret(name: str, value: str, *, scope: str, org: str, repo: str) -> None:
    command = ["gh", "secret", "set", name, "--body", value]
    if scope == "repo":
        command.extend(["--repo", f"{org}/{repo}"])
    else:
        command.extend(["--org", org, "--visibility", "selected", "--repos", repo])
    run_command_checked(command)


def set_variable(name: str, value: str, *, scope: str, org: str, repo: str) -> None:
    command = ["gh", "variable", "set", name, "--body", value]
    if scope == "repo":
        command.extend(["--repo", f"{org}/{repo}"])
    else:
        command.extend(["--org", org, "--visibility", "selected", "--repos", repo])
    run_command_checked(command)


def upsert_bootstrap_configuration(
    *,
    scope: str,
    org: str,
    repo: str,
    app_id: str,
    private_key_pem: str,
    aws_region: str,
    aws_role_to_assume: str,
    tf_state_bucket: str,
    tf_lock_table: str,
    tf_state_key_prefix: str,
) -> list[str]:
    changes: list[str] = []

    set_secret("GH_APP_ID", app_id, scope=scope, org=org, repo=repo)
    changes.append("secret GH_APP_ID")

    set_secret("GH_APP_PRIVATE_KEY", private_key_pem, scope=scope, org=org, repo=repo)
    changes.append("secret GH_APP_PRIVATE_KEY")

    variables = {
        "AWS_REGION": aws_region.strip(),
        "AWS_ROLE_TO_ASSUME": aws_role_to_assume.strip(),
        "TF_STATE_BUCKET": tf_state_bucket.strip(),
        "TF_LOCK_TABLE": tf_lock_table.strip(),
        "TF_STATE_KEY_PREFIX": tf_state_key_prefix.strip(),
    }
    for name, value in variables.items():
        if not value:
            continue
        set_variable(name, value, scope=scope, org=org, repo=repo)
        changes.append(f"variable {name}")

    return changes


def verify_cli_prerequisites() -> None:
    run_command_checked(["gh", "--version"])
    run_command_checked(["gh", "auth", "status"])


def main() -> int:
    args = parse_args()
    verify_cli_prerequisites()

    base_dir = Path(__file__).resolve().parent
    output_dir = (base_dir / args.output_dir).resolve()
    app_script = base_dir / "app" / "bootstrap-gh-app-manifest.py"
    team_script = base_dir / "team" / "bootstrap-gh-team.py"

    credentials_bundle = None if args.force_create_app else find_existing_credentials(output_dir, args.app_name)
    if credentials_bundle:
        credentials_file, private_key_file, payload = credentials_bundle
        print(f"Reusing existing app credentials: {credentials_file}")
    else:
        print("Creating GitHub App credentials via manifest flow...")
        run_app_bootstrap(
            app_script_path=app_script,
            org=args.org,
            app_name=args.app_name,
            app_description=args.app_description,
            homepage_url=args.homepage_url,
            output_dir=output_dir,
            open_browser=args.open_browser,
        )
        credentials_bundle = find_existing_credentials(output_dir, args.app_name)
        if not credentials_bundle:
            raise RuntimeError(
                "Unable to locate created credentials in output directory. "
                "Check app creation output and rerun with --force-create-app if needed."
            )
        credentials_file, private_key_file, payload = credentials_bundle

    app_id = str(payload["id"])
    private_key_pem = private_key_file.read_text(encoding="utf-8")

    grant_admin_repo = None if args.skip_team_repo_admin_grant else args.bootstrap_repo
    ensure_team(
        team_script_path=team_script,
        org=args.org,
        team_name=args.team_name,
        team_description=args.team_description,
        maintainers=args.team_maintainers,
        members=args.team_members,
        admin_repo=grant_admin_repo,
    )

    changes = upsert_bootstrap_configuration(
        scope=args.scope,
        org=args.org,
        repo=args.bootstrap_repo,
        app_id=app_id,
        private_key_pem=private_key_pem,
        aws_region=args.aws_region,
        aws_role_to_assume=args.aws_role_to_assume,
        tf_state_bucket=args.tf_state_bucket,
        tf_lock_table=args.tf_lock_table,
        tf_state_key_prefix=args.tf_state_key_prefix,
    )

    print("\nbootstrap-gh-prerequisite summary:")
    print(f"- org: {args.org}")
    print(f"- bootstrap_repo: {args.bootstrap_repo}")
    print(f"- scope: {args.scope}")
    print(f"- app_id: {app_id}")
    print(f"- app_credentials: {credentials_file}")
    print(f"- app_private_key: {private_key_file}")
    for change in changes:
        print(f"- upsert {change}")

    print("\nDone.")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:  # noqa: BLE001
        print(f"[ERROR] {exc}", file=sys.stderr)
        raise SystemExit(1)
