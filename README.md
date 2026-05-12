# solid-fullstack-template

## Mapa README

- **Repo**
  - [Faza 1: Organization prerequisite](prerequisite-org/README.md)
    - [AWS foundation](prerequisite-org/terraform/README.md)
    - [GitHub governance](prerequisite-org/gh/README.md)
      - [GitHub App](prerequisite-org/gh/app/README.md)
      - [Administrators team](prerequisite-org/gh/team/README.md)
  - [Faza 2: Repository prerequisite](prerequisite-repo/README.md)
    - [GitHub Actions workflow](.github/workflows/README.md)
    - [Account pool](prerequisite-repo/app/README.md)
    - [Terraform prerequisite](prerequisite-repo/terraform/README.md)
      - [aws-accounts](prerequisite-repo/terraform/aws-accounts/README.md)
      - [aws-iam](prerequisite-repo/terraform/aws-iam/README.md)

  - [Wspolne: Backend API](backend/README.md)
  - [Wspolne: Config](config/README.md)
  - [Wspolne: Scripts](scripts/README.md)

## Spis tresci

- [solid-fullstack-template](#solid-fullstack-template)
  - [Spis tresci](#spis-tresci)
  - [Cel repo](#cel-repo)
  - [Trzy fazy uzywania repo](#trzy-fazy-uzywania-repo)
  - [Faza 1: bootstrap organizacji](#faza-1-bootstrap-organizacji)
  - [Faza 2: bootstrap repozytorium](#faza-2-bootstrap-repozytorium)
  - [Faza 3: korzystanie z repozytorium](#faza-3-korzystanie-z-repozytorium)
  - [Wymagania startowe](#wymagania-startowe)
  - [Najwazniejsze dokumenty](#najwazniejsze-dokumenty)

## Cel repo

Ten template pomaga wystartowac nowe repo aplikacji z gotowym podzialem na konta AWS, rolami IAM dla GitHub Actions i podstawowym governance w GitHub.

Po przejsciu bootstrapu repo ma:
- konta AWS dla srodowisk z wybranego presetu,
- role OIDC dla GitHub Actions,
- environmenty GitHub z variables/secrets,
- branche, default branch, CODEOWNERS i podstawowe rulesety ochronne.

## Trzy fazy uzywania repo

1. [Bootstrap organizacji](prerequisite-org/README.md) - jednorazowy fundament dla AWS management account i GitHub ownera.
1. [Bootstrap repozytorium](prerequisite-repo/README.md) - przygotowanie konkretnego repo z template i uruchomienie workflow bootstrapowego.
1. [Korzystanie z repozytorium](terraform/README.md) - codzienna praca po bootstrapie: runtime infrastruktura aplikacji, presety i operacje pomocnicze.

## Faza 1: bootstrap organizacji

Wejscie: [prerequisite-org/README.md](prerequisite-org/README.md)

Ta faza jest uruchamiana raz dla GitHub ownera i AWS management account.

Kolejnosc wykonywana przez `prerequisite-org/bootstrap-organization.py`:
1. Zbiera kontekst lokalny: GitHub owner, AWS region, AWS profile.
1. Uruchamia AWS foundation: [prerequisite-org/terraform/README.md](prerequisite-org/terraform/README.md).
1. Uruchamia GitHub governance: [prerequisite-org/gh/README.md](prerequisite-org/gh/README.md).

Efekt fazy:
- S3 bucket na Terraform state: `tfstate-<account-id>-<region>`,
- DynamoDB lock table: `terraform-locks`,
- GitHub [OIDC](prerequisite-org/terraform/README.md#jak-dziala-github-oidc) provider w AWS,
- rola bootstrapowa `gha-bootstrap-org`,
- GitHub App do operacji governance,
- credentials GitHub Appki zapisane w AWS SSM,
- team `administrators` dla GitHub Organization, jesli owner jest organizacja.

## Faza 2: bootstrap repozytorium

Wejscie: [prerequisite-repo/README.md](prerequisite-repo/README.md)

Ta faza jest uruchamiana dla kazdego repo utworzonego z template.

Kolejnosc:
1. `prerequisite-repo/prepare-repository.py` dopina repo do roli `gha-bootstrap-org`.
1. Ten sam skrypt tworzy environment `bootstrap` i zapisuje bootstrap variables.
1. Ten sam skrypt zapisuje GitHub App secrets na environment `bootstrap`.
1. Uruchamiasz workflow [bootstrap-repo.yml](.github/workflows/README.md).
1. Workflow tworzy konta aplikacji, role deployowe, branche, environmenty, CODEOWNERS i rulesety.

Najwazniejsze pojecia w tej fazie:
- preset z [config/presets.json](config/presets.json) wybiera konta i branche,
- `aws-accounts` tworzy OU i konta AWS,
- `resolve-targets` czyta output `account_ids` ze state `aws-accounts`,
- `aws-iam` tworzy role `gha-environment-deploy` w kontach aplikacji.

## Faza 3: korzystanie z repozytorium

Po bootstrapie repo jest gotowe do normalnego rozwoju aplikacji.

Glowne miejsca:
- [terraform/README.md](terraform/README.md) - docelowa infrastruktura runtime aplikacji,
- [config/README.md](config/README.md) - kontrakt presetow uzywanych przez bootstrap,
- [prerequisite-repo/app/README.md](prerequisite-repo/app/README.md) - account pool i archiwizacja aplikacji bez zamykania kont,
- [scripts/README.md](scripts/README.md) - skrypty pomocnicze.

## Wymagania startowe

- AWS management account z uprawnieniami do AWS Organizations.
- GitHub owner: organizacja albo konto uzytkownika.
- Uprawnienia admina do repo tworzonego z template.
- Lokalnie: `python`, `terraform`, `aws`, `gh`.

## Najwazniejsze dokumenty

- [docs/operational-overview.md](docs/operational-overview.md) - krotki status i mapa repo.
- [docs/US-1.2-SECURITY-IMPLEMENTATION-PLAN.md](docs/US-1.2-SECURITY-IMPLEMENTATION-PLAN.md) - szczegolowy plan security/governance.
- [COMMIT.md](COMMIT.md) - konwencja commit message.
- [AGENTS.md](AGENTS.md) - zasady pracy agentow w repo.
