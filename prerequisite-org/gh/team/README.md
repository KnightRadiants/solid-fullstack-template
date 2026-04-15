# GH prerequisite: team

Ten katalog zawiera idempotentny skrypt do przygotowania teamu administratorow.

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

Co robi skrypt:
- tworzy team, jesli nie istnieje,
- aktualizuje opis/metadata teamu, jesli juz istnieje,
- dopina maintainerow i memberow,
- opcjonalnie nadaje teamowi admin access do wskazanych repo.
