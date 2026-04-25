# aws-accounts

Breadcrumbs: [Repo](../../../README.md) > [Faza 2: bootstrap repozytorium](../../README.md) > [Terraform prerequisite](../README.md) > **aws-accounts**

## Spis tresci

- [Cel modulu](#cel-modulu)
- [Kolejnosc w workflow](#kolejnosc-w-workflow)
- [Co tworzy](#co-tworzy)
- [Skad bierze sie environment_accounts](#skad-bierze-sie-environment_accounts)
- [Terraform state i output account_ids](#terraform-state-i-output-account_ids)
- [Account pool](#account-pool)
- [Tryby](#tryby)
- [Lokalny fallback](#lokalny-fallback)

## Cel modulu

Ten Terraform tworzy OU i konta AWS Organizations dla jednej aplikacji.
Zalecane uruchomienie: workflow [../../../.github/workflows/README.md](../../../.github/workflows/README.md), job `create-app-accounts`.

## Kolejnosc w workflow

1. Workflow czyta preset z [../../../config/presets.json](../../../config/presets.json).
1. Pole `aws_accounts` z presetu trafia do zmiennej `environment_accounts`.
1. W trybie `safe` workflow najpierw przygotowuje OU aplikacji i probuje uzyc kont z OU `Unused`.
1. Terraform plan/apply tworzy albo importuje docelowe konta.
1. Modul wystawia outputy, w tym `account_ids`.

## Co tworzy

- Organizational Unit `APP-<APP_SLUG>`.
- Konta z listy `environment_accounts`.
- Outputy: `ou_id`, `ou_arn`, `ou_name`, `account_ids`, `account_arns`, `account_emails`.

## Skad bierze sie environment_accounts

W workflow `bootstrap-repo.yml` uzytkownik wybiera input `preset`, np. `minimal`, `dev-lite` albo `dev-standard`.
Preset jest definicja w [../../../config/presets.json](../../../config/presets.json).
Pole `aws_accounts` z wybranego presetu jest przekazywane do tego modulu jako zmienna Terraform `environment_accounts`.

Przyklad:

```json
"dev-lite": {
  "aws_accounts": ["prod", "dev", "shared"]
}
```

W runtime workflow zamienia to na:

```hcl
environment_accounts = ["prod", "dev", "shared"]
```

Ten modul tworzy wtedy konta o nazwach wyprowadzonych z `app_slug` i nazwy srodowiska, np.:
- `APP-TODO-LIST-PROD`
- `APP-TODO-LIST-DEV`
- `APP-TODO-LIST-SHARED`

## Terraform state i output account_ids

Workflow uruchamia ten modul z backendem S3:
- bucket: `TF_STATE_BUCKET` z environment `bootstrap`,
- key: `bootstrap-repo/<app_slug>.tfstate`,
- lock table: `terraform-locks`.

To jest jeden state dla kont jednej aplikacji.
Nie rozdzielamy go per environment, bo modul zarzadza OU aplikacji i mapa kont srodowiskowych razem.
Output `account_ids` mapuje nazwe srodowiska na faktyczny AWS account ID.
Kolejny job workflow (`resolve-targets`) czyta ten output ze state i przekazuje go dalej do tworzenia rol deployowych.
Role deployowe sa tworzone przez modul `aws-iam`, ktory dostaje oddzielny state S3 dla kazdego environmentu.

## Account pool

W trybie `safe` workflow przed `terraform plan` robi dodatkowe kroki:
1. Tworzy/importuje sama OU aplikacji.
1. Uruchamia [../../app/account-pool.py](../../app/account-pool.py) z komenda `allocate`.
1. Przenosi aktywne konta z OU `Unused` do OU aplikacji.
1. Zmienia im nazwy na docelowe.
1. Importuje je do Terraform state.

Jesli w OU `Unused` brakuje kont, Terraform tworzy brakujace konta standardowo.

Zeby "usunac" aplikacje bez zamykania kont:

```ps1
python prerequisite-repo/app/archive-application.py `
  --app-slug todo-list `
  --aws-region eu-central-1
```

## Tryby

- `bootstrap_mode = "safe"` - konta maja `prevent_destroy = true`; normalny destroy nie zamknie kont.
- `bootstrap_mode = "debug"` - `close_on_deletion = true`; tryb testowy moze zamykac konta.

## Lokalny fallback

```ps1
Set-Location prerequisite-repo/terraform/aws-accounts
$env:AWS_PROFILE = "mafi-general-sso"
aws sso login --profile $env:AWS_PROFILE

terraform init
terraform plan -var-file="terraform.tfvars"
terraform apply -var-file="terraform.tfvars"
```
