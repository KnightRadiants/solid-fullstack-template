# Organization prerequisite

Ten katalog przygotowuje fundament wspolny dla AWS management account i GitHub ownera. Uruchamiasz go raz, zanim zaczniesz przygotowywac konkretne repo z template.

## Kolejnosc

1. `bootstrap-organization.py` zbiera kontekst lokalny: GitHub owner, AWS region, AWS profile.
1. `terraform/bootstrap-aws-foundation.py` przygotowuje AWS foundation:
   S3 bucket na Terraform state, DynamoDB lock table, GitHub OIDC provider i role `gha-bootstrap-org`.
1. `gh/bootstrap-github-governance.py` przygotowuje GitHub governance:
   GitHub App, zapis credentials Appki w AWS SSM i team `administrators` dla organizacji.

Na tym etapie nie konfigurujemy jeszcze konkretnego repo. Repo jest dopinane dopiero w `prerequisite-repo`.

## Szybki start

```ps1
Set-Location prerequisite-org

python bootstrap-organization.py `
  --aws-region eu-central-1
```

Jesli pominiesz `--org`, skrypt sprobuje wziac ownera z `git remote origin`.
Jesli pominiesz `--aws-region`, skrypt pokaze menu regionow.
Jesli nie ustawisz `AWS_PROFILE` i nie podasz `--aws-profile`, skrypt wyswietli profile znalezione w `~/.aws` i poprosi o wybor.

## Wynik fazy

Po tej fazie istnieja:
- S3 bucket `tfstate-<account-id>-<region>`.
- DynamoDB table `terraform-locks`.
- IAM OIDC provider `token.actions.githubusercontent.com`.
- IAM role `gha-bootstrap-org`.
- GitHub App do governance/bootstrapu.
- Credentials GitHub Appki zapisane w AWS SSM.
- Team `administrators` dla GitHub Organization, jesli owner jest organizacja.

## Co dalej

Dla kazdego repo utworzonego z template uruchom:
- [../prerequisite-repo/README.md](../prerequisite-repo/README.md)

Szczegoly techniczne:
- [terraform/README.md](terraform/README.md)
- [gh/README.md](gh/README.md)
