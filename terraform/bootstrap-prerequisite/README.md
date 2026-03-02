# 1. Bootstrap Prerequisite

Ten katalog przygotowuje fundament pod automatyzacje GitHub Actions dla nowego repo:
- rola OIDC dla GitHub Actions (IAM),
- remote state backend Terraform (S3 + DynamoDB).

Na tym etapie dodany jest szkielet stacka. Kolejne kroki dopisza zasoby IAM/S3/DynamoDB.

## 1.1. Uruchomienie lokalne (SSO)

1. Przejdź do katalogu:
    ```ps
    Set-Location terraform/bootstrap-prerequisite
    ```
1. Ustaw profil:
    ```ps
    $env:AWS_PROFILE = "mafi-general-sso"
    ```
1. Zaloguj się:
    ```ps
    aws sso login --profile $env:AWS_PROFILE
    ```
1. Przygotuj plik zmiennych:
    ```ps
    Copy-Item terraform.tfvars.example terraform.tfvars
    ```
1. Uruchom Terraform:
    ```ps
    terraform init
    terraform plan -var-file="terraform.tfvars"
    terraform apply -var-file="terraform.tfvars"
    ```

## 1.2. Następny zakres implementacji

1. Dodać IAM OIDC provider i rolę `github_oidc_role_name`.
1. Dodać S3 bucket `tf_state_bucket_name`.
1. Dodać DynamoDB table `tf_lock_table_name`.
1. Wystawić outputy dla workflow (`bootstrap_role_arn`, `tf_state_bucket`, `tf_lock_table`).
