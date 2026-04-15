# Repository prerequisite

Ten katalog przygotowuje konkretne repo utworzone z template. Zaklada, ze `prerequisite-org` zostal juz wykonany.

## Kolejnosc

1. `prepare-repository.py` wykrywa albo przyjmuje `owner/repo`, AWS region i AWS profile.
1. `aws/01-attach-bootstrap-role.py` dopisuje do trust policy roli `gha-bootstrap-org`:
   `repo:<owner>/<repo>:environment:bootstrap`.
1. `github/02-write-bootstrap-variables.py` tworzy repo environment `bootstrap` i zapisuje variables potrzebne przez workflow.
1. `github/03-write-bootstrap-app-secrets.py` zapisuje na environment `bootstrap` sekrety GitHub Appki.
1. Po tym uruchamiasz workflow [../.github/workflows/bootstrap-repo.yml](../.github/workflows/bootstrap-repo.yml).

## Szybki start

```ps1
Set-Location prerequisite-repo

python prepare-repository.py `
  --aws-region eu-central-1
```

Jesli pominiesz `--org` albo `--repo`, skrypt sprobuje wziac ownera i repo z `git remote origin`.

## Co jest zapisywane na environment bootstrap

Variables:
- `AWS_REGION` - region uzywany przez workflowy i Terraform backend.
- `AWS_ACCOUNT_ID` - ID AWS management account z rola bootstrapowa.
- `BOOTSTRAP_ROLE_NAME` - nazwa roli bootstrapowej, domyslnie `gha-bootstrap-org`.
- `TF_STATE_BUCKET` - bucket S3 na Terraform state.

Secrets:
- `GH_APP_ID` - ID GitHub Appki uzywanej przez workflow do operacji governance.
- `GH_APP_PRIVATE_KEY` - private key GitHub Appki, uzywany do wygenerowania installation token.

## Workflow bootstrap-repo

Workflow `bootstrap-repo.yml` wykonuje faze bootstrapu repo w GitHub Actions:
- `create-app-accounts` - tworzy OU aplikacji i konta AWS albo importuje konta z puli `Unused`.
- `resolve-targets` - czyta `account_ids` ze state `aws-accounts`.
- `create-deploy-roles` - tworzy role `gha-environment-deploy` w kontach aplikacji.
- `configure-github-repo` - tworzy branche, ustawia default branch i environmenty.
- `bind-deploy-roles` - zapisuje `AWS_ROLE_TO_ASSUME` i `AWS_REGION` na environmentach aplikacyjnych.

## Account pool

Konta AWS nie sa zamykane przy normalnym "usunieciu" aplikacji. Zamiast tego mozna przeniesc je do OU `Unused`:

```ps1
python app/archive-application.py `
  --app-slug todo-list `
  --aws-region eu-central-1
```

Jesli pominiesz `--app-slug`, skrypt odczyta rootowe OU z AWS Organizations. Gdy znajdzie jedna OU `APP-*`, uzyje jej automatycznie; gdy znajdzie kilka, pokaze menu wyboru.

Kolejny bezpieczny run `bootstrap-repo.yml` najpierw probuje uzyc aktywnych kont z OU `Unused`. Jesli puli brakuje, Terraform tworzy brakujace konta standardowo.

Szczegoly:
- [app/account-pool.py](app/account-pool.py)
- [terraform/aws-accounts/README.md](terraform/aws-accounts/README.md)
- [terraform/aws-iam/README.md](terraform/aws-iam/README.md)
