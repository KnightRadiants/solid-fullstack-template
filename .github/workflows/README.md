# Workflow bootstrap-repo

W Actions widoczny jest jeden workflow bootstrapowy:
- `bootstrap-repo.yml`

To jest trzecia faza procesu: repo ma juz environment `bootstrap`, variables, secrets i trust policy w AWS. Workflow tworzy zasoby aplikacyjne dla tego repo.

## Kolejnosc jobow

1. `create-app-accounts`
   Tworzy OU `APP-<APP_SLUG>`. W trybie `safe` najpierw probuje zaimportowac konta z account pool `Unused`, a dopiero brakujace konta tworzy przez Terraform.
1. `resolve-targets`
   Czyta `account_ids` ze state `aws-accounts` i buduje matrix srodowisk.
1. `create-deploy-roles`
   Tworzy role `gha-environment-deploy` w kontach aplikacji.
1. `configure-github-repo`
   Tworzy brakujace branche, ustawia default branch i environmenty repo.
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

## Dostep

- Wszystkie joby dzialaja na environment `bootstrap`.
- Workflow wymaga `admin` access do repo przez lokalna akcje `require-admin-access`.
- Environment `bootstrap` jest miejscem, gdzie trzymamy kontrakt prerequisite dla konkretnego repo.
