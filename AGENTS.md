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

## README Structure

- Keep README files arranged from general to specific as the reader moves down the repository tree.
- Every README should start with a `Mapa README` section directly after the title.
- The `Mapa README` section should be a tree of the repository README files, not only a one-line breadcrumb.
- The tree should let a reader navigate from any README to any other README without guessing paths.
- In each README tree, render the current README as bold plain text without a link so the reader can see where they are.
- When adding, moving, or renaming a README, update the README tree in all README files.
