# GitHub governance prerequisite

Ten katalog przygotowuje rzeczy GitHub wspolne dla ownera.

## Kolejnosc

1. `bootstrap-github-governance.py` sprawdza GitHub ownera i wymagane scope'y `gh`.
1. Szuka istniejacych credentials GitHub Appki w `app/out`, lokalnym cache i AWS SSM.
1. Jesli trzeba, tworzy GitHub App przez manifest flow.
1. Zapisuje `app_id` i `private_key_pem` w AWS SSM jako centralny backup/fallback.
1. Dla organizacji zapewnia team `administrators`.

Repo secrets `GH_APP_ID` i `GH_APP_PRIVATE_KEY` nie sa tutaj ustawiane globalnie. Dla konkretnego repo robi to:
- [../../prerequisite-repo/github/03-write-bootstrap-app-secrets.py](../../prerequisite-repo/github/03-write-bootstrap-app-secrets.py)

## Szybki start

```ps1
Set-Location prerequisite-org/gh

python bootstrap-github-governance.py `
  --org KnightRadiants `
  --aws-region eu-central-1 `
  --app-description "Bootstrap app for template governance"
```

## Wymagania

- `gh auth login`
- Dla GitHub Organization: scope `admin:org`

```ps1
gh auth refresh -h github.com -s admin:org
```

Przy ownerze typu `User` skrypt pomija teamy, bo teamy istnieja tylko w organizacjach.

## Szczegoly

- [app/README.md](app/README.md)
- [team/README.md](team/README.md)
