# solid-fullstack-template

Ten template pomaga wystartowac nowe repo aplikacji z gotowym podzialem na konta AWS, rolami IAM dla GitHub Actions i podstawowym governance w GitHub. Po bootstrapie repo ma przygotowane srodowiska, role do deploymentu oraz konfiguracje potrzebna do dalszych pipeline'ow.

## Wymagania startowe

Zeby skorzystac z tego template, potrzebujesz:
- AWS management account, na ktorym mozesz zarzadzac AWS Organizations i tworzyc konta czlonkowskie.
- Wlasna organizacje GitHub albo uprawnienia administratora w organizacji GitHub, w ktorej bedziesz tworzyc repo z template.
- Uprawnienia administratora do repo utworzonego z template, zeby przygotowac environment `bootstrap` i uruchomic workflow bootstrapowy.

## Trzy fazy

1. [Organization prerequisite](prerequisite-org/README.md) - jednorazowy fundament dla AWS management account i GitHub ownera.
1. [Repository prerequisite](prerequisite-repo/README.md) - przygotowanie konkretnego repo utworzonego z template.
1. [Bootstrap repo w GitHub Actions](.github/workflows/README.md) - workflow `bootstrap-repo.yml`, ktory tworzy konta aplikacji, role deployowe i governance repo.

Po tych trzech fazach codzienne deploymenty powinny uzywac juz katalogu:
- [terraform/](terraform/README.md)

## Presety

Kontrakt presetow jest w:
- [config/README.md](config/README.md)
- [config/presets.json](config/presets.json)

## Security implementation plan

Szczegolowy plan wdrozenia security i governance:
- [docs/US-1.2-SECURITY-IMPLEMENTATION-PLAN.md](docs/US-1.2-SECURITY-IMPLEMENTATION-PLAN.md)
