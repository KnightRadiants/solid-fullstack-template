# Preset Contract

Breadcrumbs: [Repo](../README.md) > **Config**

## Spis tresci

- [Cel katalogu](#cel-katalogu)
- [Co definiuje preset](#co-definiuje-preset)
- [Konsumenci presetow](#konsumenci-presetow)
- [Kontrakt walidacji](#kontrakt-walidacji)
- [Walidacja kontraktu](#walidacja-kontraktu)

## Cel katalogu

`config/presets.json` to zrodlo prawdy dla wariantow workflow `bootstrap-repo`.
Preset wybierasz w fazie 2 podczas uruchamiania workflow [../.github/workflows/README.md](../.github/workflows/README.md).

## Co definiuje preset

Kazdy preset definiuje:
- `aws_accounts` - jakie konta AWS ma miec aplikacja,
- `repo_branches` - jakie branche tworzy job `configure-github-repo`,
- `default_branch` - jaka galaz ustawia job `configure-github-repo`,
- `enable_preview_pr` - flaga kontraktowa pod dalsze workflow CI/CD.

## Konsumenci presetow

- `.github/workflows/bootstrap-repo.yml`, job `create-app-accounts`.
- `.github/workflows/bootstrap-repo.yml`, job `configure-github-repo`.
- `scripts/validate-presets.py`.

## Kontrakt walidacji

`scripts/validate-presets.py` wymusza:
- `aws_accounts` musi byc niepusta lista unikalnych wartosci.
- `repo_branches` musi byc niepusta lista unikalnych wartosci.
- Dozwolone wartosci `aws_accounts`: `prod`, `dev`, `stage`, `test`, `preview`, `shared`, `logging`, `security`.
- `prod` jest zawsze wymagany.
- `preview` wymaga `dev`.
- `shared` jest wymagany, gdy istnieje jakiekolwiek konto poza `prod`.
- `logging` jest wymagany, gdy istnieje `stage` lub `test`.
- `dev` w `aws_accounts` wymaga brancha `dev` w `repo_branches`.
- `stage` w `aws_accounts` wymaga brancha `stage` w `repo_branches`.
- `test` w `aws_accounts` wymaga brancha `test` w `repo_branches`.
- `default_branch` musi byc niepustym stringiem i musi istniec w `repo_branches`.
- `enable_preview_pr` musi byc `true/false`; `true` wymaga konta `preview`.

## Walidacja kontraktu

```ps1
python scripts/validate-presets.py
```
