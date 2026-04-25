# Organization Prerequisite

## Mapa README

- [Repo](../README.md)
  - **Faza 1: Organization prerequisite**
    - [AWS foundation](terraform/README.md)
    - [GitHub governance](gh/README.md)
      - [GitHub App](gh/app/README.md)
      - [Administrators team](gh/team/README.md)
  - [Faza 2: Repository prerequisite](../prerequisite-repo/README.md)
    - [GitHub Actions workflow](../.github/workflows/README.md)
    - [Account pool](../prerequisite-repo/app/README.md)
    - [Terraform prerequisite](../prerequisite-repo/terraform/README.md)
      - [aws-accounts](../prerequisite-repo/terraform/aws-accounts/README.md)
      - [aws-iam](../prerequisite-repo/terraform/aws-iam/README.md)
  - [Faza 3: Application Terraform](../terraform/README.md)
  - [Wspolne: Config](../config/README.md)
  - [Wspolne: Scripts](../scripts/README.md)

## Spis tresci

- [Cel fazy](#cel-fazy)
- [Kolejnosc wykonywania](#kolejnosc-wykonywania)
- [Krok 1: kontekst lokalny](#krok-1-kontekst-lokalny)
- [Krok 2: AWS foundation](#krok-2-aws-foundation)
- [Krok 3: GitHub governance](#krok-3-github-governance)
- [Szybki start](#szybki-start)
- [Wynik fazy](#wynik-fazy)
- [Co dalej](#co-dalej)

## Cel fazy

Ten katalog przygotowuje fundament wspolny dla AWS management account i GitHub ownera.
Uruchamiasz go raz przed przygotowaniem konkretnych repo z template.

Na tym etapie nie konfigurujemy jeszcze repo aplikacji. Repo jest dopinane dopiero w [../prerequisite-repo/README.md](../prerequisite-repo/README.md).

## Kolejnosc wykonywania

`bootstrap-organization.py` wykonuje kroki w tej kolejnosci:

1. Zbiera kontekst lokalny: GitHub owner, AWS region, AWS profile.
1. Uruchamia AWS foundation z [terraform/README.md](terraform/README.md).
1. Uruchamia GitHub governance z [gh/README.md](gh/README.md).

Nie ma tu przeplatania AWS/GitHub/AWS. Kolejnosc jest prosta: najpierw AWS, potem GitHub.

## Krok 1: kontekst lokalny

Skrypt probuje ustalic:
- GitHub owner z `--org` albo z `git remote origin`,
- AWS region z `--aws-region` albo z menu,
- AWS profile z `--aws-profile`, `AWS_PROFILE` albo z menu profili w `~/.aws`.

## Krok 2: AWS foundation

Szczegoly: [terraform/README.md](terraform/README.md)

`terraform/bootstrap-aws-foundation.py`:
1. Sprawdza AWS credentials i identyfikuje management account.
1. Sprawdza, czy istnieja zasoby fundamentu.
1. Jesli nie istnieja, uruchamia Terraform.
1. Jesli istnieja wszystkie, pomija Terraform apply.
1. Jesli istnieje tylko czesc, przerywa z bledem.

Tworzone zasoby:
- S3 bucket `tfstate-<account-id>-<region>`,
- DynamoDB table `terraform-locks`,
- IAM OIDC provider `token.actions.githubusercontent.com`,
- IAM role `gha-bootstrap-org` z policy potrzebna do bootstrapu repo i account pool.

Opis przeplywu GitHub OIDC, STS i trust policy jest w [terraform/README.md#jak-dziala-github-oidc](terraform/README.md#jak-dziala-github-oidc).

## Krok 3: GitHub governance

Szczegoly: [gh/README.md](gh/README.md)

`gh/bootstrap-github-governance.py`:
1. Sprawdza GitHub ownera i wymagane scope'y `gh`.
1. Szuka istniejacych credentials GitHub Appki lokalnie i w AWS SSM.
1. Tworzy albo wybiera GitHub App.
1. Zapisuje credentials GitHub Appki w AWS SSM jako backup/fallback.
1. Dla GitHub Organization zapewnia team `administrators`.

## Szybki start

```ps1
Set-Location prerequisite-org

python bootstrap-organization.py `
  --aws-region eu-central-1
```

Opcjonalne argumenty:
- `--org` - GitHub owner,
- `--aws-profile` - AWS profile,
- `--app-name` - nazwa GitHub App,
- `--team-maintainers` - maintainerzy teamu `administrators`.

## Wynik fazy

Po tej fazie istnieja:
- S3 bucket `tfstate-<account-id>-<region>`,
- DynamoDB table `terraform-locks`,
- IAM OIDC provider `token.actions.githubusercontent.com`,
- IAM role `gha-bootstrap-org`,
- GitHub App do governance/bootstrapu,
- credentials GitHub Appki zapisane w AWS SSM,
- team `administrators` dla GitHub Organization, jesli owner jest organizacja.

## Co dalej

Dla kazdego repo utworzonego z template uruchom faze 2:
- [../prerequisite-repo/README.md](../prerequisite-repo/README.md)
