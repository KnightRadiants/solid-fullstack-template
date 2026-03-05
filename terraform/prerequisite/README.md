# Terraform Prerequisite

Ten katalog zawiera kroki wykonywane raz (one-time prerequisite), przed regularnym bootstrapem repo.

- `aws/` - prerequisite AWS (OIDC role do bootstrapu, S3 backend, DynamoDB lock table)
- `gh/` - prerequisite GitHub (GitHub App z uprawnieniami administracyjnymi, krok pólautomatyczny)
