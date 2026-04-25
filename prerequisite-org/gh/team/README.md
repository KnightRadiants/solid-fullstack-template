# GitHub Administrators Team

## Mapa README

- [Repo](../../../README.md)
  - [Faza 1: Organization prerequisite](../../README.md)
    - [AWS foundation](../../terraform/README.md)
    - [GitHub governance](../README.md)
      - [GitHub App](../app/README.md)
      - **Administrators team**
  - [Faza 2: Repository prerequisite](../../../prerequisite-repo/README.md)
    - [GitHub Actions workflow](../../../.github/workflows/README.md)
    - [Account pool](../../../prerequisite-repo/app/README.md)
    - [Terraform prerequisite](../../../prerequisite-repo/terraform/README.md)
      - [aws-accounts](../../../prerequisite-repo/terraform/aws-accounts/README.md)
      - [aws-iam](../../../prerequisite-repo/terraform/aws-iam/README.md)
  - [Faza 3: Application Terraform](../../../terraform/README.md)
  - [Wspolne: Config](../../../config/README.md)
  - [Wspolne: Scripts](../../../scripts/README.md)

## Spis tresci

- [Cel katalogu](#cel-katalogu)
- [Kolejnosc wykonywania](#kolejnosc-wykonywania)
- [Uruchomienie](#uruchomienie)
- [Co robi skrypt](#co-robi-skrypt)

## Cel katalogu

Ten katalog zawiera idempotentny skrypt do przygotowania teamu administratorow.
Zwykle uruchamia go [../bootstrap-github-governance.py](../bootstrap-github-governance.py).

## Kolejnosc wykonywania

`bootstrap-gh-team.py`:
1. Wylicza slug teamu z nazwy.
1. Tworzy team, jesli nie istnieje.
1. Aktualizuje opis, nazwe i privacy, jesli team juz istnieje.
1. Dopina maintainerow.
1. Dopina memberow.
1. Opcjonalnie nadaje teamowi admin access do wskazanych repo.

## Uruchomienie

```ps1
Set-Location prerequisite-org/gh
python team/bootstrap-gh-team.py `
  --org KnightRadiants `
  --team-name "administrators" `
  --team-description "Template bootstrap administrators" `
  --maintainers "MafistoPL" `
  --admin-repos "solid-fullstack-template-manual"
```

## Co robi skrypt

- Tworzy team, jesli nie istnieje.
- Aktualizuje opis/metadata teamu, jesli juz istnieje.
- Dopina maintainerow i memberow.
- Opcjonalnie nadaje teamowi admin access do wskazanych repo.
