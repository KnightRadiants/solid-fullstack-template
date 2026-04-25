# GitHub Administrators Team

Breadcrumbs: [Repo](../../../README.md) > [Faza 1: bootstrap organizacji](../../README.md) > [GitHub governance](../README.md) > **Administrators team**

## Spis tresci

- [Cel katalogu](#cel-katalogu)
- [Kolejnosc wykonywania](#kolejnosc-wykonywania)
- [Uruchomienie](#uruchomienie)
- [Co robi skrypt](#co-robi-skrypt)

## Cel katalogu

Ten katalog zawiera idempotentny skrypt do przygotowania teamu administratorow.
Zwykle uruchamia go [../bootstrap-github-governance.py](../bootstrap-github-governance.py).

## Kolejnosc wykonywania

`bootstrap-gh-team.py`:
1. Wylicza slug teamu z nazwy.
1. Tworzy team, jesli nie istnieje.
1. Aktualizuje opis, nazwe i privacy, jesli team juz istnieje.
1. Dopina maintainerow.
1. Dopina memberow.
1. Opcjonalnie nadaje teamowi admin access do wskazanych repo.

## Uruchomienie

```ps1
Set-Location prerequisite-org/gh
python team/bootstrap-gh-team.py `
  --org KnightRadiants `
  --team-name "administrators" `
  --team-description "Template bootstrap administrators" `
  --maintainers "MafistoPL" `
  --admin-repos "solid-fullstack-template-manual"
```

## Co robi skrypt

- Tworzy team, jesli nie istnieje.
- Aktualizuje opis/metadata teamu, jesli juz istnieje.
- Dopina maintainerow i memberow.
- Opcjonalnie nadaje teamowi admin access do wskazanych repo.
