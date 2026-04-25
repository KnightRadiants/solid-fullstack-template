# Agent Working Rules

## Minimal Contracts First

- Do not add new Terraform inputs unless there is a concrete, current caller that needs to set them.
- Do not add new Terraform outputs unless there is a concrete, current consumer that reads them.
- Do not add workflow inputs/outputs "for future use".
- When in doubt, hardcode stable conventions and add configurability later only when a real need appears.
- Keep bootstrap workflows and modules opinionated and minimal; avoid optional switches that increase error surface.

## Commit Messages

- When creating commits, follow the commit message convention defined in `COMMIT.md`.

## Documentation Sync

- Keep `docs/operational-overview.md` up to date as the short operational overview of the repository.
- Keep `docs/US-1.2-SECURITY-IMPLEMENTATION-PLAN.md` up to date as the detailed security/governance plan.
- After every change in bootstrap/security/governance behavior, update both files so status ("zrealizowane" vs "otwarte") matches the real implementation.
