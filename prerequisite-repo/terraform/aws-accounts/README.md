# aws-accounts

Ten Terraform tworzy OU i konta AWS Organizations dla jednej aplikacji.

Zalecane uruchomienie: workflow `.github/workflows/bootstrap-repo.yml`, job `create-app-accounts`.

## Co tworzy

- Organizational Unit `APP-<APP_SLUG>`.
- Konta z listy `environment_accounts`, wyliczanej z presetu.
- Outputy: `ou_id`, `ou_arn`, `ou_name`, `account_ids`, `account_arns`, `account_emails`.

## Account pool

W trybie `safe` workflow przed `terraform plan` robi dodatkowe kroki:
- tworzy/importuje sama OU aplikacji,
- uruchamia `prerequisite-repo/app/account-pool.py allocate`,
- przenosi aktywne konta z OU `Unused` do OU aplikacji,
- zmienia im nazwy na docelowe,
- importuje je do Terraform state.

Jesli w OU `Unused` brakuje kont, Terraform tworzy brakujace konta standardowo.

Zeby "usunac" aplikacje bez zamykania kont:

```ps1
python prerequisite-repo/app/archive-application.py `
  --app-slug todo-list `
  --aws-region eu-central-1
```

Skrypt przenosi konta do OU `Unused`, zmienia ich nazwy na `UNUSED-*` i usuwa pusta OU aplikacji.

## Tryby

- `bootstrap_mode = "safe"`: konta maja `prevent_destroy = true`; normalny destroy nie zamknie kont.
- `bootstrap_mode = "debug"`: `close_on_deletion = true`; tryb testowy moze zamykac konta.

## Lokalny fallback

```ps1
Set-Location prerequisite-repo/terraform/aws-accounts
$env:AWS_PROFILE = "mafi-general-sso"
aws sso login --profile $env:AWS_PROFILE

terraform init
terraform plan -var-file="terraform.tfvars"
terraform apply -var-file="terraform.tfvars"
```
