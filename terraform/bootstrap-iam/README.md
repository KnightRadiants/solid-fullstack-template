# bootstrap-iam

Tworzy IAM OIDC role `gha-environment-deploy` w kontach utworzonych przez `bootstrap-org`.

## 1. Jak to dziala

1. Job `resolve-targets` w `bootstrap-all.yml` czyta `account_ids` ze state `bootstrap-org`
1. Buduje matrix: `environment_name -> target_account_id`
1. Job `bootstrap-iam` uruchamia ten sam kod dla kazdego konta
1. `bootstrap-iam` tworzy OIDC provider + role IAM w koncie docelowym

## 2. Co jest tworzone

- OIDC provider: `token.actions.githubusercontent.com` (w koncie docelowym)
- Rola: `gha-environment-deploy`
- Trust policy zawezona do:
  - `repo:<github_org>/<github_repo>:environment:<environment_name>`
- Policy-as-code zalezne od `environment_name` (`prod`, `dev`, `preview`, `shared`, `logging`)

## 3. Workflow bootstrap-all

Ten etap jest wykonywany tylko jako job matrix wewnatrz `bootstrap-all`.
Nie ma osobnego manualnego workflow dla `bootstrap-iam`.

## 4. Lokalny run (fallback)

1. Przejdz do katalogu:
   ```ps
   Set-Location terraform/bootstrap-iam
   ```
1. Ustaw profil i login:
   ```ps
   $env:AWS_PROFILE = "mafi-general-sso"
   aws sso login --profile $env:AWS_PROFILE
   ```
1. Przygotuj zmienne:
   ```ps
   Copy-Item terraform.tfvars.example terraform.tfvars
   ```
1. Uzupelnij `terraform.tfvars`:
   - `app_slug`
   - `environment_name`
   - `target_account_id`
   - `github_org`
   - `github_repo`
1. Uruchom:
   ```ps
   terraform init
   terraform plan -var-file="terraform.tfvars"
   terraform apply -var-file="terraform.tfvars"
   ```
