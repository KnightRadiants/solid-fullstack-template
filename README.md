# solid-fullstack-template

## Bootstrap Flow

1. Utwórz nowe repozytorium na podstawie szablonu `SOLID-FULLSTACK-TEMPLATE`.

1. Jeśli to pierwsze repo na tym koncie AWS:
    - sklonuj repo lokalnie,
    - skonfiguruj `aws-cli` / Identity Federation (SSO),
    - uruchom lokalnie Terraform `terraform/bootstrap-prerequisite`, który tworzy:
      - rolę OIDC dla GitHub Actions (np. `gha-bootstrap-org`),
      - bucket S3 dla backendów Terraform,
      - tabelę DynamoDB dla locków Terraform.

1. W GitHub (repo lub org) ustaw zmienne z outputów `bootstrap-prerequisite`:
    - `AWS_REGION`
    - `AWS_ROLE_TO_ASSUME`
    - `TF_LOCK_TABLE`
    - `TF_STATE_BUCKET`

1. Uruchom workflow `bootstrap-org`:
    - workflow pobiera token OIDC (`id-token: write`),
    - AWS trust policy pozwala temu tokenowi przyjąć rolę z `AWS_ROLE_TO_ASSUME`,
    - workflow uruchamia Terraform i tworzy OU + konta zgodnie z presetem.

1. Uruchom workflow `bootstrap-iam-matrix`:
    - workflow czyta `account_ids` ze stanu `bootstrap-org`,
    - uruchamia `bootstrap-iam` per konto i tworzy role `gha-environment-deploy`.

1. Uruchom workflow `bootstrap-gh-core`:
    - tworzy branche repo zgodnie z presetem,
    - ustawia `default_branch` z presetu,
    - tworzy GitHub Environments zgodnie z listą `aws_accounts` z presetu.

1. Uruchom workflow `bootstrap-gh-bind`:
    - workflow czyta `account_ids` ze stanu `bootstrap-org`,
    - buduje `AWS_ROLE_TO_ASSUME` (`gha-environment-deploy`) per environment,
    - zapisuje `AWS_ROLE_TO_ASSUME` i `AWS_REGION` do GitHub Environment Variables.

1. Docelowo będzie jeden workflow orchestratora `bootstrap-all`, który uruchomi:
    - `bootstrap-org` (OU + konta),
    - `bootstrap-iam` (OIDC + role `gha-environment-deploy` w kontach member),
    - `bootstrap-gh-core` (utworzenie environments/branches/rulesets),
    - `bootstrap-gh-bind` (powiązanie outputów AWS z GitHub env vars/secrets).
