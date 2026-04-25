# Application Account Pool

Breadcrumbs: [Repo](../../README.md) > [Faza 2: bootstrap repozytorium](../README.md) > **Application account pool**

## Spis tresci

- [Cel katalogu](#cel-katalogu)
- [Account pool](#account-pool)
- [Operacje](#operacje)
- [Archive aplikacji](#archive-aplikacji)
- [Uprawnienia](#uprawnienia)

## Cel katalogu

Ten katalog zawiera operacje aplikacyjne poza Terraformem.
Najwazniejsza rola to obsluga puli kont AWS w OU `Unused`.

## Account pool

OU `Unused` jest pula aktywnych kont AWS, ktore nie sa aktualnie przypisane do aplikacji.
W trybie `safe` workflow [../../.github/workflows/README.md](../../.github/workflows/README.md) najpierw probuje wykorzystac konta z tej puli, zanim Terraform utworzy nowe konta.

## Operacje

- `account-pool.py list` - pokazuje konta w OU `Unused`.
- `account-pool.py allocate` - uzywane przez workflow `bootstrap-repo.yml`; przenosi konta z `Unused` do OU aplikacji i zwraca importy Terraform.
- `account-pool.py archive` - przenosi konta aplikacji do OU `Unused`.
- `archive-application.py` - wygodny wrapper do recznego "usuniecia" aplikacji bez zamykania kont.

## Archive aplikacji

```ps1
python prerequisite-repo/app/archive-application.py `
  --app-slug todo-list `
  --aws-region eu-central-1
```

Parametr `--app-slug` jest opcjonalny w trybie interaktywnym.
Bez niego skrypt odczyta OU `APP-*` z AWS Organizations i wybierze aplikacje automatycznie albo pokaze menu, jesli jest ich kilka.

Kolejnosc:
1. Znajduje OU `APP-<APP_SLUG>`.
1. Tworzy OU `Unused`, jesli jej nie ma.
1. Zmienia nazwy kont na `UNUSED-*`.
1. Przenosi konta do `Unused`.
1. Usuwa pusta OU aplikacji.

## Uprawnienia

Zmiana nazwy kont uzywa AWS Account Management API `account put-account-name`.
Skrypt sam wlacza trusted access dla Account Management (`account.amazonaws.com`), jesli jeszcze go nie ma.

Profil musi miec uprawnienia:
- `organizations:EnableAWSServiceAccess`
- `account:PutAccountName`
