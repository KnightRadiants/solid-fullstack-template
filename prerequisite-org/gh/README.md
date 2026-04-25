# GitHub Governance Prerequisite

## Mapa README

- [Repo](../../README.md)
  - [Faza 1: Organization prerequisite](../README.md)
    - [AWS foundation](../terraform/README.md)
    - **GitHub governance**
      - [GitHub App](app/README.md)
      - [Administrators team](team/README.md)
  - [Faza 2: Repository prerequisite](../../prerequisite-repo/README.md)
    - [GitHub Actions workflow](../../.github/workflows/README.md)
    - [Account pool](../../prerequisite-repo/app/README.md)
    - [Terraform prerequisite](../../prerequisite-repo/terraform/README.md)
      - [aws-accounts](../../prerequisite-repo/terraform/aws-accounts/README.md)
      - [aws-iam](../../prerequisite-repo/terraform/aws-iam/README.md)
  - [Faza 3: Application Terraform](../../terraform/README.md)
  - [Wspolne: Config](../../config/README.md)
  - [Wspolne: Scripts](../../scripts/README.md)

## Spis tresci

- [Cel katalogu](#cel-katalogu)
- [Kolejnosc wykonywania](#kolejnosc-wykonywania)
- [Krok 1: GitHub owner i scope](#krok-1-github-owner-i-scope)
- [Krok 2: GitHub App credentials](#krok-2-github-app-credentials)
- [Krok 3: team administrators](#krok-3-team-administrators)
- [Sekrety dla konkretnego repo](#sekrety-dla-konkretnego-repo)
- [Szybki start](#szybki-start)
- [Wymagania](#wymagania)
- [Szczegoly](#szczegoly)

## Cel katalogu

Ten katalog przygotowuje GitHub governance wspolne dla ownera.
Jest uruchamiany przez [../bootstrap-organization.py](../bootstrap-organization.py) po AWS foundation.

## Kolejnosc wykonywania

`bootstrap-github-governance.py`:
1. Sprawdza GitHub ownera i typ ownera (`Organization` albo `User`).
1. Sprawdza wymagane scope'y `gh`.
1. Szuka istniejacych credentials GitHub Appki w `app/out`, lokalnym cache i AWS SSM.
1. Jesli trzeba, tworzy GitHub App przez manifest flow.
1. Zapisuje `app_id` i `private_key_pem` w AWS SSM jako centralny backup/fallback.
1. Dla organizacji zapewnia team `administrators`.

## Krok 1: GitHub owner i scope

Dla GitHub Organization wymagany jest scope:

```ps1
gh auth refresh -h github.com -s admin:org
```

Dla ownera typu `User` skrypt pomija teamy, bo teamy istnieja tylko w organizacjach.

## Krok 2: GitHub App credentials

Szczegoly: [app/README.md](app/README.md)

Skrypt najpierw probuje reuse:
- `app/out`,
- lokalny cache,
- AWS SSM.

Jesli nie znajdzie pasujacych credentials, odpala manifest flow i zapisuje:
- `github-app-<APP_ID>.private-key.pem`,
- `github-app-<APP_ID>.credentials.json`.

## Krok 3: team administrators

Szczegoly: [team/README.md](team/README.md)

Dla organizacji skrypt zapewnia team `administrators`.
Ten team jest pozniej uzywany jako code owner i jako naturalna grupa adminow bootstrapu.

## Sekrety dla konkretnego repo

Environment secrets `GH_APP_ID` i `GH_APP_PRIVATE_KEY` na `bootstrap` nie sa tutaj ustawiane globalnie.
Dla konkretnego repo robi to faza 2:
- [../../prerequisite-repo/github/03-write-bootstrap-app-secrets.py](../../prerequisite-repo/github/03-write-bootstrap-app-secrets.py)

## Szybki start

```ps1
Set-Location prerequisite-org/gh

python bootstrap-github-governance.py `
  --org KnightRadiants `
  --aws-region eu-central-1 `
  --app-description "Bootstrap app for template governance"
```

## Wymagania

- `gh auth login`
- Dla GitHub Organization: scope `admin:org`
- AWS credentials, jesli ma dzialac backup/fallback przez AWS SSM

## Szczegoly

- [app/README.md](app/README.md)
- [team/README.md](team/README.md)
