# Operational Overview

Ten plik jest krotkim przewodnikiem operacyjnym po repo i statusie wdrozenia.
Stan ma zawsze odzwierciedlac faktyczna implementacje.

## Jak korzystac z repo (kolejnosc)

1. Uruchom [prerequisite-org/README.md](../prerequisite-org/README.md) (fundament AWS + GitHub owner).
1. Uruchom [prerequisite-repo/README.md](../prerequisite-repo/README.md) dla konkretnego repo z template.
1. Uruchom workflow [bootstrap-repo.yml](../.github/workflows/bootstrap-repo.yml).
1. Po bootstrapie rozwijaj runtime infrastrukture aplikacji w [terraform/README.md](../terraform/README.md).

## Status wdrozenia security/governance (2026-04-24)

| Etap | Status | Co jest zrobione / co brakuje |
| --- | --- | --- |
| Etap 0 - prerequisite GH automation | zrealizowany | Orchestrator + app + team + idempotencja sa wdrozone. |
| Etap 1 - zamrozenie powierzchni ataku bootstrap | czesciowo | Environment gate + admin gate sa wdrozone; brakuje twardego warunku uruchamiania tylko z `main`. |
| Etap 2 - sekrety/zmienne przez environment gate | zrealizowany | Kontrakt `bootstrap` (secrets + variables) jest utrzymany przez prerequisite i workflow. |
| Etap 3 - OIDC hardening (AWS) | zrealizowany funkcjonalnie | Trust policy oparta o `repo + environment`; dalsze zaciesnianie policy mozliwe iteracyjnie. |
| Etap 4 - rulesets i branch protection | czesciowo | Rulesety i `CODEOWNERS` sa automatyzowane; required status checks pozostaja do doprecyzowania. |
| Etap 5 - owner bypass i self-merge policy | nie zrealizowany | Brakuje konfiguracji bypass actor zgodnie z planem. |
| Etap 6 - E2E security regression | nie zrealizowany | Brakuje kompletnego zestawu testow regresji i artefaktow. |

## Najwazniejsze pliki zrodlowe

- Szczegolowy plan i kryteria: [US-1.2-SECURITY-IMPLEMENTATION-PLAN.md](./US-1.2-SECURITY-IMPLEMENTATION-PLAN.md)
- Glowny workflow bootstrap: [../.github/workflows/bootstrap-repo.yml](../.github/workflows/bootstrap-repo.yml)
- Contract presetow: [../config/README.md](../config/README.md)

## Kluczowe pojecia

- Preset: wariant bootstrapu z [../config/presets.json](../config/presets.json), ktory okresla m.in. `aws_accounts` i branche repo.
- `environment_accounts`: zmienna Terraform modulu `aws-accounts`; dostaje wartosc z `aws_accounts` wybranego presetu.
- State `aws-accounts`: Terraform state modulu [../prerequisite-repo/terraform/aws-accounts](../prerequisite-repo/terraform/aws-accounts/README.md), trzymany w S3 pod kluczem `bootstrap-repo/<app_slug>.tfstate`; jest wspolny dla kont jednej aplikacji.
- State `aws-iam`: Terraform state modulu [../prerequisite-repo/terraform/aws-iam](../prerequisite-repo/terraform/aws-iam/README.md), trzymany per environment pod kluczem `bootstrap-repo/aws-iam/<app_slug>/<environment>.tfstate`.
- `account_ids`: output ze state `aws-accounts`, mapujacy srodowiska (`prod`, `dev`, `shared`, itd.) na faktyczne AWS account ID.

## Zasada utrzymania

Przy kazdej zmianie dotykajacej bootstrap/security/governance:
- zaktualizuj ten plik (`docs/operational-overview.md`),
- zaktualizuj plan szczegolowy (`docs/US-1.2-SECURITY-IMPLEMENTATION-PLAN.md`),
- upewnij sie, ze status etapow i lista "co zrobione / co otwarte" sa zgodne z kodem.
