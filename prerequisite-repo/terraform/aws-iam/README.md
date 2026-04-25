# aws-iam

Breadcrumbs: [Repo](../../../README.md) > [Faza 2: bootstrap repozytorium](../../README.md) > [Terraform prerequisite](../README.md) > **aws-iam**

## Spis tresci

- [Cel modulu](#cel-modulu)
- [Kolejnosc w workflow](#kolejnosc-w-workflow)
- [Co tworzy](#co-tworzy)
- [Terraform state](#terraform-state)
- [Trust policy](#trust-policy)
- [Lokalny fallback](#lokalny-fallback)

## Cel modulu

Ten Terraform tworzy role OIDC `gha-environment-deploy` w kontach aplikacji.
Jest uruchamiany przez job `create-deploy-roles`.

## Kolejnosc w workflow

1. Job `resolve-targets` czyta `account_ids` ze state [aws-accounts](../aws-accounts/README.md).
1. Buduje matrix `environment_name -> target_account_id`.
1. Job `create-deploy-roles` uruchamia ten modul dla kazdego konta.
1. Provider AWS zaklada `OrganizationAccountAccessRole` w koncie docelowym.
1. Terraform tworzy provider OIDC, role i policy runtime.

## Co tworzy

- OIDC provider `token.actions.githubusercontent.com` w koncie docelowym.
- Role `gha-environment-deploy`.
- Policy-as-code zalezne od environmentu, np. `prod`, `dev`, `preview`, `shared`, `logging`.

## Terraform state

Workflow `create-deploy-roles` uruchamia ten modul z osobnym backend key dla kazdego environmentu:

```text
bootstrap-repo/aws-iam/<app_slug>/<environment>.tfstate
```

Dzieki temu rownolegle elementy matrixa, np. `dev` i `prod`, nie pisza do tego samego state.

## Trust policy

Trust policy jest zawezona do repo i environmentu:

```text
repo:<github_org>/<github_repo>:environment:<environment_name>
```

Dzieki temu job z environment `dev` nie powinien moc zalozyc roli `prod`.

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
