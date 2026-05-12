# AWS Foundation Prerequisite

## Mapa README

- [Repo](../../README.md)
  - [Faza 1: Organization prerequisite](../README.md)
    - **AWS foundation**
    - [GitHub governance](../gh/README.md)
      - [GitHub App](../gh/app/README.md)
      - [Administrators team](../gh/team/README.md)
  - [Faza 2: Repository prerequisite](../../prerequisite-repo/README.md)
    - [GitHub Actions workflow](../../.github/workflows/README.md)
    - [Account pool](../../prerequisite-repo/app/README.md)
    - [Terraform prerequisite](../../prerequisite-repo/terraform/README.md)
      - [aws-accounts](../../prerequisite-repo/terraform/aws-accounts/README.md)
      - [aws-iam](../../prerequisite-repo/terraform/aws-iam/README.md)

  - [Wspolne: Backend API](../../backend/README.md)
  - [Wspolne: Config](../../config/README.md)
  - [Wspolne: Scripts](../../scripts/README.md)

## Spis tresci

- [Cel katalogu](#cel-katalogu)
- [Kolejnosc wykonywania](#kolejnosc-wykonywania)
- [Co tworzy Terraform](#co-tworzy-terraform)
- [Jak dziala GitHub OIDC](#jak-dziala-github-oidc)
- [Czego ten krok nie robi](#czego-ten-krok-nie-robi)
- [Szybki start](#szybki-start)
- [Manualny fallback](#manualny-fallback)

## Cel katalogu

Ten katalog tworzy zasoby AWS wspolne dla wszystkich repo bootstrapowanych z template.
Jest uruchamiany przez [../bootstrap-organization.py](../bootstrap-organization.py) jako pierwszy duzy krok fazy 1.

## Kolejnosc wykonywania

`bootstrap-aws-foundation.py`:
1. Sprawdza AWS credentials i identyfikuje management account.
1. Wylicza nazwe bucketu state: `tfstate-<account-id>-<region>`.
1. Sprawdza trzy zasoby fundamentu: S3 bucket state, DynamoDB lock table i role `gha-bootstrap-org`.
1. Jesli nie istnieje zaden z nich, uruchamia `terraform init` i `terraform apply`.
1. Jesli istnieja wszystkie, pomija `terraform apply` i tylko dopina brakujace elementy inline policy.
1. Jesli istnieje tylko czesc, przerywa z bledem, zeby nie nadpisac pol-recznego stanu.

## Co tworzy Terraform

- S3 bucket `tfstate-<account-id>-<region>`.
- DynamoDB table `terraform-locks`.
- IAM OIDC provider `token.actions.githubusercontent.com`.
- IAM role `gha-bootstrap-org`.
- Inline policy roli `gha-bootstrap-org` potrzebna do Organizations, state backendu i assume role w kontach aplikacji.

Policy zawiera tez `organizations:EnableAWSServiceAccess` i `account:PutAccountName`, bo account pool potrzebuje wlaczac trusted access i zmieniac nazwy kont.

## Jak dziala GitHub OIDC

OIDC provider w AWS mowi, ze AWS ufa tokenom wystawianym przez GitHub Actions z `token.actions.githubusercontent.com`.
Samo ograniczenie repo i environmentu jest w trust policy roli, np. `gha-bootstrap-org`.

Przeplyw wyglada tak:

1. GitHub Actions job startuje w repo/environmencie.
1. Job prosi GitHuba o OIDC token.
1. GitHub wystawia krotko zyjacy JWT z claimami, np. `repo`, `environment`, `aud`, `sub`.
1. Workflow wysyla ten token do AWS STS przez `AssumeRoleWithWebIdentity`.
1. AWS sprawdza:
   - czy issuer tokenu to zaufany OIDC provider,
   - czy podpis tokenu jest poprawny,
   - czy `aud` pasuje,
   - czy `sub` pasuje do trust policy roli.
1. Jesli wszystko sie zgadza, STS wydaje tymczasowe AWS credentials dla tej roli.

W praktyce:
- OIDC provider odpowiada na pytanie: czy AWS ufa GitHubowi jako wystawcy tokenow?
- Trust policy roli odpowiada na pytanie: ktore konkretne repo i environment moga zalozyc role?
- Permissions policy roli odpowiada na pytanie: co workflow moze zrobic po zalozeniu roli?

## Czego ten krok nie robi

Ten krok nie dopisuje realnych repo aplikacji do trust policy roli `gha-bootstrap-org`.
Robi to dopiero faza 2:
- [../../prerequisite-repo/aws/01-attach-bootstrap-role.py](../../prerequisite-repo/aws/01-attach-bootstrap-role.py)

## Szybki start

```ps1
Set-Location prerequisite-org/terraform

python bootstrap-aws-foundation.py `
  --org KnightRadiants `
  --aws-region eu-central-1
```

## Manualny fallback

```ps1
Set-Location prerequisite-org/terraform
$env:AWS_PROFILE = "mafi-general-sso"
aws sso login --profile $env:AWS_PROFILE

terraform init
terraform plan -var-file="terraform.tfvars"
terraform apply -var-file="terraform.tfvars"
```
