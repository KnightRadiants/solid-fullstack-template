# Repository Prerequisite Terraform

## Mapa README

- [Repo](../../README.md)
  - [Faza 1: Organization prerequisite](../../prerequisite-org/README.md)
    - [AWS foundation](../../prerequisite-org/terraform/README.md)
    - [GitHub governance](../../prerequisite-org/gh/README.md)
      - [GitHub App](../../prerequisite-org/gh/app/README.md)
      - [Administrators team](../../prerequisite-org/gh/team/README.md)
  - [Faza 2: Repository prerequisite](../README.md)
    - [GitHub Actions workflow](../../.github/workflows/README.md)
    - [Account pool](../app/README.md)
    - **Terraform prerequisite**
      - [aws-accounts](aws-accounts/README.md)
      - [aws-iam](aws-iam/README.md)

  - [Wspolne: Backend API](../../backend/README.md)
  - [Wspolne: Config](../../config/README.md)
  - [Wspolne: Scripts](../../scripts/README.md)

## Spis tresci

- [Cel katalogu](#cel-katalogu)
- [Moduly](#moduly)
- [Kolejnosc w workflow](#kolejnosc-w-workflow)
- [Czego tutaj nie ma](#czego-tutaj-nie-ma)

## Cel katalogu

Terraform w tym katalogu przygotowuje konkretne repo do pozniejszych deploymentow.
Jest uruchamiany przez workflow [../../.github/workflows/README.md](../../.github/workflows/README.md).

## Moduly

- [aws-accounts](aws-accounts/README.md) - tworzy OU aplikacji i konta AWS Organizations.
- [aws-iam](aws-iam/README.md) - tworzy role OIDC w kontach aplikacji.

## Kolejnosc w workflow

1. `create-app-accounts` uruchamia [aws-accounts](aws-accounts/README.md).
1. `resolve-targets` czyta output `account_ids` ze state `aws-accounts`.
1. `create-deploy-roles` uruchamia [aws-iam](aws-iam/README.md) dla kazdego konta z matrixa.
1. `bind-deploy-roles` zapisuje ARN roli deployowej na environmentach GitHub.

## Czego tutaj nie ma

To nie jest jeszcze infrastruktura aplikacyjna typu VPC/RDS/ECS.
Taka infrastruktura powinna trafiac do rootowego katalogu [../../terraform/README.md](../../terraform/README.md).
