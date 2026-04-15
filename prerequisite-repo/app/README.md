# Application account pool

Ten katalog zawiera operacje aplikacyjne poza Terraformem.

## Account pool

OU `Unused` jest pula aktywnych kont AWS, ktore nie sa aktualnie przypisane do aplikacji.

Operacje:
- `account-pool.py list` - pokazuje konta w OU `Unused`.
- `account-pool.py allocate` - uzywane przez workflow `bootstrap-repo.yml`; przenosi konta z `Unused` do OU aplikacji i zwraca importy Terraform.
- `archive-application.py` - reczne "usuniecie" aplikacji bez zamykania kont.

## Archive aplikacji

```ps1
python prerequisite-repo/app/archive-application.py `
  --app-slug todo-list `
  --aws-region eu-central-1
```

Parametr `--app-slug` jest opcjonalny w trybie interaktywnym. Bez niego skrypt odczyta OU `APP-*` z AWS Organizations i wybierze aplikacje automatycznie albo pokaze menu, jesli jest ich kilka.

Skrypt:
1. znajduje OU `APP-<APP_SLUG>`,
1. tworzy OU `Unused`, jesli jej nie ma,
1. zmienia nazwy kont na `UNUSED-*`,
1. przenosi konta do `Unused`,
1. usuwa pusta OU aplikacji.

Zmiana nazwy kont uzywa AWS Account Management API `account put-account-name`.
Skrypt sam wlacza trusted access dla Account Management (`account.amazonaws.com`), jesli jeszcze go nie ma.
Profil musi miec uprawnienia `organizations:EnableAWSServiceAccess` i `account:PutAccountName`.
