# AWS foundation prerequisite

Ten katalog tworzy zasoby AWS wspolne dla wszystkich repo bootstrapowanych z template.

## Kolejnosc

1. `bootstrap-aws-foundation.py` sprawdza AWS credentials i identyfikuje management account.
1. Sprawdza, czy istnieja trzy zasoby fundamentu:
   S3 bucket state, DynamoDB lock table i rola `gha-bootstrap-org`.
1. Jesli nie istnieje zaden z nich, uruchamia `terraform init` i `terraform apply`.
1. Jesli istnieja wszystkie, pomija `terraform apply`.
1. Jesli istnieje tylko czesc, przerywa z bledem, zeby nie nadpisac pol-recznego stanu.

## Co tworzy Terraform

- S3 bucket `tfstate-<account-id>-<region>`.
- DynamoDB table `terraform-locks`.
- IAM OIDC provider `token.actions.githubusercontent.com`.
- IAM role `gha-bootstrap-org`.
- Inline policy tej roli zawiera tez `organizations:EnableAWSServiceAccess` i `account:PutAccountName`, potrzebne do account pool.

Trust policy dla konkretnych repo nie jest tutaj dopisywana. Robi to:
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
