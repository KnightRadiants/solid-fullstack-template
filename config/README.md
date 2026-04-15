# Preset contract

`config/presets.json` to zrodlo prawdy dla wariantow workflow `bootstrap-repo`.

Kazdy preset definiuje:
- `aws_accounts` - jakie konta ma miec aplikacja.
- `repo_branches` - jakie branche tworzy job `configure-github-repo`.
- `default_branch` - jaka galaz ustawia job `configure-github-repo`.
- `enable_preview_pr` - flaga pod dalsze workflow CI/CD.

Konsumenci presetow:
- `.github/workflows/bootstrap-repo.yml`, job `create-app-accounts`.
- `.github/workflows/bootstrap-repo.yml`, job `configure-github-repo`.
- `scripts/validate-presets.py`.

Globalne reguly walidacji:
- `prod` jest zawsze wymagany.
- `preview` wymaga `dev`.
- `shared` jest wymagany, gdy istnieje jakiekolwiek konto poza `prod`.
- `logging` jest wymagany, gdy istnieje `stage` lub `test`.

Walidacja kontraktu:

```ps1
python scripts/validate-presets.py
```
