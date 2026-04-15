#!/usr/bin/env python3
"""
Archive an application without closing AWS accounts.

This is a thin user-facing wrapper around account-pool.py archive.
"""

from __future__ import annotations

import argparse
import configparser
import json
import os
import re
import subprocess
import sys
from pathlib import Path


def aws_config_dir() -> Path:
    config_path = os.environ.get("AWS_CONFIG_FILE", "")
    credentials_path = os.environ.get("AWS_SHARED_CREDENTIALS_FILE", "")

    if config_path:
        return Path(config_path).expanduser().resolve().parent
    if credentials_path:
        return Path(credentials_path).expanduser().resolve().parent
    return Path.home() / ".aws"


def normalize_profile_name(section: str, *, from_config: bool) -> str:
    if not from_config:
        return section.strip()
    if section == "default":
        return "default"
    if section.startswith("profile "):
        return section[len("profile ") :].strip()
    return section.strip()


def list_available_aws_profiles() -> list[str]:
    profiles: set[str] = set()
    files = [
        (Path(os.environ.get("AWS_CONFIG_FILE", Path.home() / ".aws" / "config")).expanduser(), True),
        (
            Path(os.environ.get("AWS_SHARED_CREDENTIALS_FILE", Path.home() / ".aws" / "credentials")).expanduser(),
            False,
        ),
    ]

    for path, from_config in files:
        if not path.exists():
            continue

        parser = configparser.RawConfigParser()
        parser.read(path, encoding="utf-8")
        for section in parser.sections():
            profile = normalize_profile_name(section, from_config=from_config)
            if profile:
                profiles.add(profile)

    return sorted(profiles, key=str.lower)


def read_menu_key() -> str:
    if os.name == "nt":
        import msvcrt

        while True:
            char = msvcrt.getwch()
            if char in ("\x00", "\xe0"):
                extended = msvcrt.getwch()
                if extended == "H":
                    return "up"
                if extended == "P":
                    return "down"
                continue
            if char == "\r":
                return "enter"
            if char == "\x03":
                raise KeyboardInterrupt
            continue

    import termios
    import tty

    fd = sys.stdin.fileno()
    old_settings = termios.tcgetattr(fd)
    try:
        tty.setraw(fd)
        char = sys.stdin.read(1)
        if char in ("\r", "\n"):
            return "enter"
        if char == "\x03":
            raise KeyboardInterrupt
        if char == "\x1b":
            next_char = sys.stdin.read(1)
            if next_char == "[":
                final_char = sys.stdin.read(1)
                if final_char == "A":
                    return "up"
                if final_char == "B":
                    return "down"
        return ""
    finally:
        termios.tcsetattr(fd, termios.TCSADRAIN, old_settings)


def render_arrow_menu(options: list[str], selected_index: int, *, redraw: bool) -> None:
    if redraw:
        sys.stdout.write(f"\x1b[{len(options)}A")
    for index, option in enumerate(options):
        prefix = "> " if index == selected_index else "  "
        sys.stdout.write(f"\r{prefix}{option}\x1b[K\n")
    sys.stdout.flush()


def select_with_arrows(prompt_text: str, options: list[str]) -> int:
    if not sys.stdin.isatty():
        raise RuntimeError(f"Missing required value: {prompt_text}")

    print(f"{prompt_text} (use arrow keys and Enter):")
    selected_index = 0
    render_arrow_menu(options, selected_index, redraw=False)

    while True:
        key = read_menu_key()
        if key == "up":
            selected_index = (selected_index - 1) % len(options)
            render_arrow_menu(options, selected_index, redraw=True)
            continue
        if key == "down":
            selected_index = (selected_index + 1) % len(options)
            render_arrow_menu(options, selected_index, redraw=True)
            continue
        if key == "enter":
            print()
            return selected_index


def prompt_for_aws_profile(profiles: list[str]) -> str:
    if not sys.stdin.isatty():
        available = ", ".join(profiles) if profiles else "none found"
        raise RuntimeError(
            "Missing AWS profile. Set AWS_PROFILE, pass --aws-profile, or run interactively. "
            f"Available profiles in {aws_config_dir()}: {available}."
        )

    print(f"AWS profile is not set. Available profiles in {aws_config_dir()}:")
    if not profiles:
        print("  No profiles found in config files.")
        while True:
            answer = input("AWS profile: ").strip()
            if answer:
                return answer
            print("Value is required.")

    selected_index = select_with_arrows("Select AWS profile", profiles)
    return profiles[selected_index]


def resolve_aws_profile(profile_arg: str) -> str:
    explicit_profile = profile_arg.strip()
    if explicit_profile:
        return explicit_profile

    env_profile = os.environ.get("AWS_PROFILE", "").strip()
    if env_profile:
        return env_profile

    profiles = list_available_aws_profiles()
    return prompt_for_aws_profile(profiles)


def prompt_if_missing(value: str, prompt_text: str) -> str:
    normalized = value.strip()
    if normalized:
        return normalized

    if not sys.stdin.isatty():
        raise RuntimeError(f"Missing required value: {prompt_text}")

    while True:
        answer = input(f"{prompt_text}: ").strip()
        if answer:
            return answer
        print("Value is required.")


def run_command(command: list[str]) -> subprocess.CompletedProcess[str]:
    return subprocess.run(command, text=True, capture_output=True, check=False)


def run_command_checked(command: list[str], *, description: str = "") -> str:
    result = run_command(command)
    if result.returncode != 0:
        raise RuntimeError(
            f"{description + chr(10) if description else ''}"
            f"Command failed ({result.returncode}): {' '.join(command)}\n"
            f"STDERR:\n{result.stderr}\nSTDOUT:\n{result.stdout}"
        )
    return result.stdout.strip()


def aws_command(*, aws_profile: str, aws_region: str, service_args: list[str]) -> list[str]:
    command = ["aws", *service_args]
    if aws_region.strip():
        command.extend(["--region", aws_region.strip()])
    command.extend(["--profile", aws_profile])
    return command


def aws_json(*, aws_profile: str, aws_region: str, service_args: list[str], description: str = "") -> dict:
    output = run_command_checked(
        aws_command(
            aws_profile=aws_profile,
            aws_region=aws_region,
            service_args=[*service_args, "--output", "json"],
        ),
        description=description,
    )
    if not output:
        return {}
    parsed = json.loads(output)
    if not isinstance(parsed, dict):
        raise RuntimeError(f"Expected JSON object from aws {' '.join(service_args)}.")
    return parsed


def normalize_app_slug_from_ou_name(ou_name: str) -> str:
    suffix = ou_name.removeprefix("APP-").strip()
    slug = suffix.lower()
    slug = re.sub(r"[^a-z0-9]+", "-", slug)
    slug = re.sub(r"-{2,}", "-", slug).strip("-")
    if not slug:
        raise RuntimeError(f"Cannot derive app slug from OU name '{ou_name}'.")
    return slug


def list_application_ous(*, aws_profile: str, aws_region: str, unused_ou_name: str) -> list[tuple[str, str]]:
    roots_payload = aws_json(
        aws_profile=aws_profile,
        aws_region=aws_region,
        service_args=["organizations", "list-roots"],
        description="Reading AWS Organizations root...",
    )
    roots = roots_payload.get("Roots", [])
    if not isinstance(roots, list) or not roots:
        raise RuntimeError("Could not find AWS Organizations root.")
    root_id = str(roots[0].get("Id", "")).strip()
    if not root_id:
        raise RuntimeError("AWS Organizations root payload is invalid.")

    ous_payload = aws_json(
        aws_profile=aws_profile,
        aws_region=aws_region,
        service_args=["organizations", "list-organizational-units-for-parent", "--parent-id", root_id],
        description="Listing application OUs...",
    )
    ous = ous_payload.get("OrganizationalUnits", [])
    if not isinstance(ous, list):
        raise RuntimeError("Invalid OrganizationalUnits payload.")

    applications: list[tuple[str, str]] = []
    for ou in ous:
        if not isinstance(ou, dict):
            continue
        ou_name = str(ou.get("Name", "")).strip()
        if not ou_name.startswith("APP-") or ou_name == unused_ou_name:
            continue
        applications.append((normalize_app_slug_from_ou_name(ou_name), ou_name))

    return sorted(applications, key=lambda item: item[1].lower())


def resolve_app_slug(*, app_slug_arg: str, aws_profile: str, aws_region: str, unused_ou_name: str) -> str:
    explicit_slug = app_slug_arg.strip()
    if explicit_slug:
        return explicit_slug

    applications = list_application_ous(
        aws_profile=aws_profile,
        aws_region=aws_region,
        unused_ou_name=unused_ou_name,
    )
    if len(applications) == 1:
        app_slug, ou_name = applications[0]
        print(f"Detected application OU '{ou_name}', using app_slug='{app_slug}'.")
        return app_slug

    if len(applications) > 1:
        options = [f"{ou_name} [{app_slug}]" for app_slug, ou_name in applications]
        selected_index = select_with_arrows("Select application to archive", options)
        return applications[selected_index][0]

    return prompt_if_missing("", "Application slug")


def parse_args() -> argparse.Namespace:
    parser = argparse.ArgumentParser(description="Archive one application into the AWS account pool.")
    parser.add_argument("--app-slug", default="", help="Application slug")
    parser.add_argument("--aws-region", default="", help="AWS region for CLI calls")
    parser.add_argument("--aws-profile", default="", help="AWS profile for CLI calls")
    parser.add_argument("--unused-ou-name", default="Unused", help="Account pool OU name")
    return parser.parse_args()


def main() -> int:
    args = parse_args()
    aws_profile = resolve_aws_profile(args.aws_profile)
    args.app_slug = resolve_app_slug(
        app_slug_arg=args.app_slug,
        aws_profile=aws_profile,
        aws_region=args.aws_region,
        unused_ou_name=args.unused_ou_name,
    )
    script = Path(__file__).resolve().parent / "account-pool.py"
    command = [sys.executable, str(script)]
    if args.aws_region.strip():
        command.extend(["--aws-region", args.aws_region.strip()])
    command.extend(["--aws-profile", aws_profile])
    command.extend(
        [
            "archive",
            "--app-slug",
            args.app_slug,
            "--unused-ou-name",
            args.unused_ou_name,
        ]
    )
    return subprocess.run(command, check=False).returncode


if __name__ == "__main__":
    raise SystemExit(main())
