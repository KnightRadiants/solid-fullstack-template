# Preset contract

`config/presets.json` to zrodlo prawdy dla wariantow bootstrapu.

Kazdy preset definiuje:
- `aws_accounts` - jakie konta tworzy `bootstrap-org`
- `repo_branches` - jakie branche tworzy `bootstrap-gh-core`
- `default_branch` - jaka galaz ustawia `bootstrap-gh-core`
- `enable_preview_pr` - flaga pod dalsze workflow CI/CD

Konsumenci presetow:
- `bootstrap-all.yml` (job `bootstrap-org`)
- `bootstrap-all.yml` (job `bootstrap-gh-core`)
- `scripts/validate-presets.py`

Globalne reguly walidacji:
- `prod` jest zawsze wymagany
- `preview` wymaga `dev`
- `shared` jest wymagany, gdy istnieje jakiekolwiek konto poza `prod`
- `logging` jest wymagany, gdy istnieje `stage` lub `test`

Walidacja kontraktu:

```ps
python scripts/validate-presets.py
```
