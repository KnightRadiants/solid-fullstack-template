# Repository prerequisite Terraform

Terraform w tym katalogu przygotowuje konkretne repo do pozniejszych deploymentow.

- `aws-accounts` tworzy OU aplikacji i konta AWS Organizations.
- `aws-iam` tworzy role OIDC w kontach aplikacji.

To nie jest jeszcze infrastruktura aplikacyjna typu VPC/RDS/ECS. Taka infrastruktura powinna trafiac do rootowego katalogu `terraform/`.
