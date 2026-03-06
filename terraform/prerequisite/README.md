# Prerequisite

Ten katalog zawiera kroki wykonywane raz, zanim zaczniesz bootstrapowac kolejne repo z template.

- `aws/` - prerequisite po stronie AWS (rola OIDC dla workflow, S3 backend, DynamoDB lock table)
- `gh/` - prerequisite po stronie GitHub:
  - `gh/app/` - tworzenie GitHub App,
  - `gh/team/` - zapewnienie teamu administratorow,
  - `gh/bootstrap-gh.py` - lokalny orchestrator spinajacy app + team + bootstrap secrets/variables.

Po wykonaniu obu krokow ustawiasz Variables/Secrets i uruchamiasz `bootstrap-all`.
