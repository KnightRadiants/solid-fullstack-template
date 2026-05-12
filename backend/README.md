# Backend

## Mapa README

- [Repo](../README.md)
  - [Faza 1: Organization prerequisite](../prerequisite-org/README.md)
    - [AWS foundation](../prerequisite-org/terraform/README.md)
    - [GitHub governance](../prerequisite-org/gh/README.md)
      - [GitHub App](../prerequisite-org/gh/app/README.md)
      - [Administrators team](../prerequisite-org/gh/team/README.md)
  - [Faza 2: Repository prerequisite](../prerequisite-repo/README.md)
    - [GitHub Actions workflow](../.github/workflows/README.md)
    - [Account pool](../prerequisite-repo/app/README.md)
    - [Terraform prerequisite](../prerequisite-repo/terraform/README.md)
      - [aws-accounts](../prerequisite-repo/terraform/aws-accounts/README.md)
      - [aws-iam](../prerequisite-repo/terraform/aws-iam/README.md)
  - [Faza 3: Application Terraform](../terraform/README.md)
  - **Wspolne: Backend API**
  - [Wspolne: Config](../config/README.md)
  - [Wspolne: Scripts](../scripts/README.md)

## Migrations

Uruchamiaj z katalogu `backend`.

## Add migration

```powershell
dotnet ef migrations add NAME_OF_MIGRATION `
  --project src/SolidFullstackTemplate.Infrastructure `
  --startup-project src/SolidFullstackTemplate.Api `
  --context AppDbContext `
  --output-dir Migrations
```

## Update database

```powershell
dotnet ef database update `
  --project src/SolidFullstackTemplate.Infrastructure `
  --startup-project src/SolidFullstackTemplate.Api `
  --context AppDbContext
```
