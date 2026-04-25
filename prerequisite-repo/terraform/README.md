# Repository Prerequisite Terraform

Breadcrumbs: [Repo](../../README.md) > [Faza 2: bootstrap repozytorium](../README.md) > **Terraform prerequisite**

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
