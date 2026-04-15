# aws-iam

Ten Terraform tworzy role OIDC `gha-environment-deploy` w kontach aplikacji.

## Kolejnosc w workflow

1. Job `resolve-targets` czyta `account_ids` ze state `aws-accounts`.
1. Buduje matrix `environment_name -> target_account_id`.
1. Job `create-deploy-roles` uruchamia ten modul dla kazdego konta.
1. Provider AWS zaklada `OrganizationAccountAccessRole` w koncie docelowym.

## Co tworzy

- OIDC provider `token.actions.githubusercontent.com` w koncie docelowym.
- Role `gha-environment-deploy`.
- Trust policy zawezona do `repo:<github_org>/<github_repo>:environment:<environment_name>`.
- Policy-as-code zalezne od environmentu, np. `prod`, `dev`, `preview`, `shared`, `logging`.

## Lokalny fallback

```ps1
Set-Location prerequisite-repo/terraform/aws-iam
$env:AWS_PROFILE = "mafi-general-sso"
aws sso login --profile $env:AWS_PROFILE

terraform init
terraform plan -var-file="terraform.tfvars"
terraform apply -var-file="terraform.tfvars"
```

W `terraform.tfvars` musza byc ustawione:
- `app_slug`
- `environment_name`
- `target_account_id`
- `github_org`
- `github_repo`
