# Repository Prerequisite

Breadcrumbs: [Repo](../README.md) > **Faza 2: bootstrap repozytorium**

## Spis tresci

- [Cel fazy](#cel-fazy)
- [Kolejnosc wykonywania](#kolejnosc-wykonywania)
- [Krok 1: AWS bootstrap role trust](#krok-1-aws-bootstrap-role-trust)
- [Krok 2: GitHub bootstrap variables](#krok-2-github-bootstrap-variables)
- [Krok 3: GitHub bootstrap app secrets](#krok-3-github-bootstrap-app-secrets)
- [Krok 4: workflow bootstrap-repo](#krok-4-workflow-bootstrap-repo)
- [Czym jest state aws-accounts](#czym-jest-state-aws-accounts)
- [Account pool](#account-pool)
- [Szybki start](#szybki-start)
- [Szczegoly techniczne](#szczegoly-techniczne)

## Cel fazy

Ten katalog przygotowuje konkretne repo utworzone z template.
Zaklada, ze faza 1, czyli [../prerequisite-org/README.md](../prerequisite-org/README.md), zostala juz wykonana.

Faza sklada sie z lokalnego przygotowania repo (`prepare-repository.py`) oraz uruchomienia workflow [../.github/workflows/README.md](../.github/workflows/README.md).

## Kolejnosc wykonywania

`prepare-repository.py` wykonuje kroki w tej kolejnosci:

1. Wykrywa albo przyjmuje `owner/repo`, AWS region i AWS profile.
1. Uruchamia [aws/01-attach-bootstrap-role.py](aws/01-attach-bootstrap-role.py).
1. Uruchamia [github/02-write-bootstrap-variables.py](github/02-write-bootstrap-variables.py).
1. Uruchamia [github/03-write-bootstrap-app-secrets.py](github/03-write-bootstrap-app-secrets.py).
1. Po tym recznie uruchamiasz workflow [../.github/workflows/bootstrap-repo.yml](../.github/workflows/bootstrap-repo.yml).

Tu kolejnosc jest przeplatana AWS -> GitHub -> GitHub -> GitHub Actions.

## Krok 1: AWS bootstrap role trust

`aws/01-attach-bootstrap-role.py` dopisuje do trust policy roli `gha-bootstrap-org` subject:

```text
repo:<owner>/<repo>:environment:bootstrap
```

Dzieki temu workflow z tego repo moze uzyc roli bootstrapowej tylko przez GitHub environment `bootstrap`.

## Krok 2: GitHub bootstrap variables

`github/02-write-bootstrap-variables.py` tworzy environment `bootstrap` i zapisuje variables:
- `AWS_REGION` - region workflowow i Terraform backendu,
- `AWS_ACCOUNT_ID` - ID AWS management account,
- `BOOTSTRAP_ROLE_NAME` - domyslnie `gha-bootstrap-org`,
- `TF_STATE_BUCKET` - bucket S3 na Terraform state.

Workflow moze alternatywnie uzyc `AWS_ROLE_TO_ASSUME`, jesli ustawisz je recznie na environment `bootstrap`.

## Krok 3: GitHub bootstrap app secrets

`github/03-write-bootstrap-app-secrets.py` zapisuje na environment `bootstrap` secrets:
- `GH_APP_ID`,
- `GH_APP_PRIVATE_KEY`.

Skrypt korzysta z GitHub governance z fazy 1. Jesli trzeba, reuse'uje credentials z lokalnego cache albo AWS SSM.

## Krok 4: workflow bootstrap-repo

Workflow [../.github/workflows/bootstrap-repo.yml](../.github/workflows/bootstrap-repo.yml) wykonuje joby liniowo:

1. `create-app-accounts` - tworzy OU aplikacji i konta AWS albo importuje konta z puli `Unused`.
1. `resolve-targets` - czyta output `account_ids` z Terraform state modulu `aws-accounts`.
1. `create-deploy-roles` - tworzy role `gha-environment-deploy` w kontach aplikacji.
1. `configure-github-repo` - tworzy branche, ustawia default branch i environmenty, utrzymuje `.github/CODEOWNERS` oraz rulesety ochronne dla `main` i `dev`.
1. `bind-deploy-roles` - zapisuje `AWS_ROLE_TO_ASSUME` i `AWS_REGION` na environmentach aplikacyjnych.

## Czym jest state aws-accounts

`aws-accounts` to Terraform root module w [terraform/aws-accounts](terraform/aws-accounts/README.md).
W jobie `create-app-accounts` workflow uruchamia ten modul z backendem S3:
- bucket: `TF_STATE_BUCKET` z environment `bootstrap`,
- key: `bootstrap-repo/<app_slug>.tfstate`,
- lock table: `terraform-locks`.

Ten state jest wspolny dla bootstrapu kont jednej aplikacji, a nie osobny per environment.
Zawiera stan OU aplikacji `APP-<APP_SLUG>` i kont AWS utworzonych albo zaimportowanych dla tej aplikacji, np. `dev`, `prod` i `shared`.
Po `terraform apply` modul wystawia output `account_ids`, np.:

```json
{
  "prod": "111111111111",
  "dev": "222222222222",
  "shared": "333333333333"
}
```

Job `resolve-targets` inicjalizuje ten sam backend i robi `terraform output -json`, zeby z `account_ids` zbudowac matrix dla joba `create-deploy-roles`.
Sam job `create-deploy-roles` uzywa juz osobnych state per environment:
`bootstrap-repo/aws-iam/<app_slug>/<environment>.tfstate`.

## Account pool

Konta AWS nie sa zamykane przy normalnym "usunieciu" aplikacji.
Zamiast tego mozna przeniesc je do OU `Unused`:

```ps1
python app/archive-application.py `
  --app-slug todo-list `
  --aws-region eu-central-1
```

Kolejny bezpieczny run `bootstrap-repo.yml` najpierw probuje uzyc aktywnych kont z OU `Unused`.
Jesli puli brakuje, Terraform tworzy brakujace konta standardowo.

Szczegoly: [app/README.md](app/README.md)

## Szybki start

```ps1
Set-Location prerequisite-repo

python prepare-repository.py `
  --aws-region eu-central-1
```

Jesli pominiesz `--org` albo `--repo`, skrypt sprobuje wziac ownera i repo z `git remote origin`.

## Szczegoly techniczne

- [app/README.md](app/README.md)
- [terraform/README.md](terraform/README.md)
- [terraform/aws-accounts/README.md](terraform/aws-accounts/README.md)
- [terraform/aws-iam/README.md](terraform/aws-iam/README.md)
- [../.github/workflows/README.md](../.github/workflows/README.md)
