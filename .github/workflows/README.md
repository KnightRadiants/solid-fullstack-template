# Workflow bootstrap-repo

Breadcrumbs: [Repo](../../README.md) > [Faza 2: bootstrap repozytorium](../../prerequisite-repo/README.md) > **GitHub Actions workflow**

## Spis tresci

- [Cel workflow](#cel-workflow)
- [Kolejnosc jobow](#kolejnosc-jobow)
- [Inputy](#inputy)
- [Wymagane variables na environment bootstrap](#wymagane-variables-na-environment-bootstrap)
- [Wymagane secrets na environment bootstrap](#wymagane-secrets-na-environment-bootstrap)
- [Dostep i guardy](#dostep-i-guardy)
- [Powiazane README](#powiazane-readme)

## Cel workflow

W Actions widoczny jest jeden workflow bootstrapowy:
- [bootstrap-repo.yml](bootstrap-repo.yml)

To jest druga czesc fazy 2.
Repo ma juz environment `bootstrap`, variables, secrets i trust policy w AWS.
Workflow tworzy zasoby aplikacyjne dla tego repo i dopina governance repo.

## Kolejnosc jobow

1. `guard-main-branch`
   Fail-fast guard: konczy workflow, jesli `workflow_dispatch` zostal uruchomiony spoza `refs/heads/main`.
1. `create-app-accounts`
   Tworzy OU `APP-<APP_SLUG>`. W trybie `safe` najpierw probuje zaimportowac konta z account pool `Unused`, a dopiero brakujace konta tworzy przez Terraform [aws-accounts](../../prerequisite-repo/terraform/aws-accounts/README.md).
1. `resolve-targets`
   Czyta output `account_ids` ze state `aws-accounts` i buduje matrix srodowisk.
1. `create-deploy-roles`
   Tworzy role `gha-environment-deploy` w kontach aplikacji przez [aws-iam](../../prerequisite-repo/terraform/aws-iam/README.md). Kazdy element matrixa ma osobny state S3: `bootstrap-repo/aws-iam/<app_slug>/<environment>.tfstate`.
1. `configure-github-repo`
   Tworzy brakujace branche, ustawia default branch i environmenty repo, a potem utrzymuje `.github/CODEOWNERS` oraz rulesety ochronne dla `main` i `dev`.
1. `bind-deploy-roles`
   Zapisuje na environmentach aplikacyjnych `AWS_ROLE_TO_ASSUME` i `AWS_REGION`.

## Inputy

- `app_slug` - slug aplikacji, np. `todo-list`.
- `root_email_base` - bazowy email bez aliasu `+`, np. `owner@example.com`.
- `bootstrap_mode` - `safe` albo `debug`.
- `debug_suffix` - opcjonalny suffix dla debug, np. `dbg01`.
- `preset` - `minimal`, `dev-lite`, `dev-standard`, `release`, `full-qa`.
- `aws_region` - opcjonalny override regionu.

## Wymagane variables na environment bootstrap

- `AWS_REGION`
- `AWS_ACCOUNT_ID`
- `BOOTSTRAP_ROLE_NAME`
- `TF_STATE_BUCKET`

Alternatywnie zamiast `AWS_ACCOUNT_ID` + `BOOTSTRAP_ROLE_NAME` mozesz ustawic `AWS_ROLE_TO_ASSUME`.

## Wymagane secrets na environment bootstrap

- `GH_APP_ID`
- `GH_APP_PRIVATE_KEY`

## Dostep i guardy

- Preflight job `guard-main-branch` nie uzywa sekretow ani environment gate; ma przerwac run spoza `refs/heads/main` przed approvalem.
- Wszystkie joby wykonujace bootstrap zasobow dzialaja na environment `bootstrap`.
- Workflow wymaga `admin` access do repo przez lokalna akcje [../actions/require-admin-access/action.yml](../actions/require-admin-access/action.yml).
- Workflow ma guard, ktory blokuje uruchomienie na repo zrodlowym template o nazwie `solid-fullstack-template`.
- Workflow ma twardy guard `GITHUB_REF == refs/heads/main`.
- Environment `bootstrap` jest miejscem, gdzie trzymamy kontrakt prerequisite dla konkretnego repo.

## Powiazane README

- [../../prerequisite-repo/README.md](../../prerequisite-repo/README.md)
- [../../prerequisite-repo/terraform/aws-accounts/README.md](../../prerequisite-repo/terraform/aws-accounts/README.md)
- [../../prerequisite-repo/terraform/aws-iam/README.md](../../prerequisite-repo/terraform/aws-iam/README.md)
