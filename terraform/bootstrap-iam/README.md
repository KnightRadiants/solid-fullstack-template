# 1. Bootstrap IAM

Ten katalog przygotowuje warstwe IAM dla kont utworzonych przez `bootstrap-org`.
Stack dziala w modelu single-account: jeden run konfiguruje IAM w jednym koncie docelowym.
Docelowo ten stack bedzie uruchamiany w petli (matrix) dla kont wynikajacych z wybranego presetu.

Na tym etapie dodany jest szkielet stacka. Kolejnym krokiem bedzie implementacja zasobow IAM.

## 1.1. Uruchomienie lokalne (SSO)

1. Przejdź do katalogu:
    ```ps
    Set-Location terraform/bootstrap-iam
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
1. Uzupełnij wymagane zmienne w `terraform.tfvars`:
    - `app_slug`
    - `environment_name`
    - `target_account_id`
    - `github_org`
    - `github_repo`
1. Uruchom Terraform:
    ```ps
    terraform init
    terraform plan -var-file="terraform.tfvars"
    terraform apply -var-file="terraform.tfvars"
    ```

## 1.2. Następny zakres implementacji

1. Dodać moduł OIDC role dla pojedynczego konta docelowego.
1. Dodać trust policy pod `github_org` i `github_repo`.
1. Dodać workflow matrix, ktory uruchomi ten stack dla kazdego konta z outputow `bootstrap-org`.
