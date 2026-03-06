# GitHub prerequisite (one-time per org)

Ten krok wykonujesz raz na organizacje GitHub.

Etap 0 sklada sie z trzech czesci:
- `app/` - tworzenie GitHub App przez manifest flow,
- `team/` - idempotentne zapewnienie teamu `administrators`,
- `bootstrap-gh.py` - lokalny orchestrator, ktory spina oba kroki i od razu ustawia bootstrapowe secrets/variables.

## 1. Wymagania

1. Zalogowany `gh` CLI z uprawnieniami administracyjnymi do org/repo:
   ```ps1
   gh auth login
   gh auth status
   ```
1. Uprawnienia do tworzenia GitHub App i zarzadzania teamami/repo variables/secrets.

## 2. Szybki start (zalecane)

```ps1
Set-Location terraform/prerequisite/gh

python bootstrap-gh.py `
  --org KnightRadiants `
  --bootstrap-repo solid-fullstack-template-manual `
  --scope org `
  --app-name "gha-template-bootstrap" `
  --app-description "Bootstrap app for template governance" `
  --open-browser `
  --aws-region "eu-central-1" `
  --aws-role-to-assume "arn:aws:iam::123456789012:role/gha-bootstrap-org" `
  --tf-state-bucket "tfstate-123456789012-eu-central-1" `
  --tf-lock-table "terraform-locks"
```

Co zrobi orchestrator:
1. Utworzy GitHub App (albo uzyje istniejacych credentials z `app/out`, jesli juz sa).
1. Zapewni team `administrators` i maintainera.
1. Ustawi:
   - `GH_APP_ID` (secret)
   - `GH_APP_PRIVATE_KEY` (secret)
   - `AWS_REGION` (variable, jesli podano)
   - `AWS_ROLE_TO_ASSUME` (variable, jesli podano)
   - `TF_STATE_BUCKET` (variable, jesli podano)
   - `TF_LOCK_TABLE` (variable, jesli podano)
   - `TF_STATE_KEY_PREFIX` (variable, opcjonalnie)

Przy `--scope org` wartosci sa zapisywane jako org-level i ograniczone do `--bootstrap-repo` (`visibility=selected`).

## 3. Tryb reczny

- Tylko App: [app/README.md](app/README.md)
- Tylko Team: [team/README.md](team/README.md)

## 4. Instalacja appki

Po utworzeniu appki zainstaluj ja na repo, ktore beda bootstrapowane.
Jesli zmienisz permissiony appki po instalacji, zaakceptuj `Permission updates requested` w organizacji.
