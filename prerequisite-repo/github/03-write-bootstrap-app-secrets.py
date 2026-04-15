#!/usr/bin/env python3
"""
Repository prerequisite step 3.

Reuse the organization GitHub App credentials and write the repo bootstrap
environment secrets required by the bootstrap-repo workflow.
"""

from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path

ANSI_RED = "\033[91m"
ANSI_RESET = "\033[0m"


def red(text: str) -> str:
    if not sys.stderr.isatty():
        return text
    return f"{ANSI_RED}{text}{ANSI_RESET}"


def print_step(message: str) -> None:
    print(f"[INFO] {message}", flush=True)


def run_command(command: list[str], *, cwd: Path) -> None:
    result = subprocess.run(command, check=False, cwd=cwd)
    if result.returncode != 0:
        raise RuntimeError(f"Command failed ({result.returncode}): {' '.join(command)}")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Write GitHub App bootstrap secrets for one repository.")
    parser.add_argument("--org", required=True, help="GitHub owner")
    parser.add_argument("--repo", required=True, help="GitHub repository")
    parser.add_argument("--aws-region", required=True, help="AWS region used for SSM credential lookup")
    parser.add_argument("--aws-profile", default="", help="AWS profile used for SSM credential lookup")
    parser.add_argument("--app-name", default="", help="GitHub App name override")
    parser.add_argument("--homepage-url", default="", help="Optional app homepage URL")
    parser.add_argument("--output-dir", default="prerequisite-org/gh/app/out", help="Directory for app credentials output")
    browser_group = parser.add_mutually_exclusive_group()
    browser_group.add_argument("--open-browser", dest="open_browser", action="store_true", help="Open browser if needed")
    browser_group.add_argument("--no-open-browser", dest="open_browser", action="store_false", help="Do not open browser")
    parser.set_defaults(open_browser=True)
    parser.add_argument("--force-create-app", action="store_true", help="Force creating a new app")
    parser.add_argument(
        "--skip-team-repo-admin-grant",
        action="store_true",
        help="Skip granting administrators team admin permission on the repo",
    )
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    script_dir = Path(__file__).resolve().parent
    repo_root = script_dir.parent.parent
    governance_script = repo_root / "prerequisite-org" / "gh" / "bootstrap-github-governance.py"

    output_dir = Path(args.output_dir)
    if not output_dir.is_absolute():
        output_dir = repo_root / output_dir

    command = [
        sys.executable,
        str(governance_script),
        "--org",
        args.org,
        "--bootstrap-repo",
        args.repo,
        "--aws-region",
        args.aws_region,
        "--output-dir",
        str(output_dir.resolve()),
    ]
    if args.aws_profile.strip():
        command.extend(["--aws-profile", args.aws_profile.strip()])
    if args.app_name.strip():
        command.extend(["--app-name", args.app_name.strip()])
    if args.homepage_url.strip():
        command.extend(["--homepage-url", args.homepage_url.strip()])
    if args.open_browser:
        command.append("--open-browser")
    else:
        command.append("--no-open-browser")
    if args.force_create_app:
        command.append("--force-create-app")
    if args.skip_team_repo_admin_grant:
        command.append("--skip-team-repo-admin-grant")

    print_step(f"Writing GitHub App bootstrap secrets for repo '{args.org}/{args.repo}'.")
    run_command(command, cwd=repo_root)
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except Exception as exc:  # noqa: BLE001
        print(red(f"[ERROR] {exc}"), file=sys.stderr)
        raise SystemExit(1)
