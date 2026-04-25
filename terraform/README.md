# Application Terraform

Breadcrumbs: [Repo](../README.md) > [Faza 3: korzystanie z repozytorium](../README.md#faza-3-korzystanie-z-repozytorium) > **Application Terraform**

## Spis tresci

- [Cel katalogu](#cel-katalogu)
- [Co powinno tu trafic](#co-powinno-tu-trafic)
- [Czego tutaj nie trzymamy](#czego-tutaj-nie-trzymamy)
- [Powiazane katalogi](#powiazane-katalogi)

## Cel katalogu

Ten katalog jest zarezerwowany na docelowa infrastrukture runtime aplikacji po zakonczeniu bootstrapu.

## Co powinno tu trafic

Tutaj powinny trafiac zasoby aplikacyjne, np.:
- VPC,
- ECS/Lambda,
- RDS,
- S3 aplikacyjne,
- CloudFront,
- monitoring.

## Czego tutaj nie trzymamy

Prerequisite'y zostaly rozdzielone:
- [../prerequisite-org/README.md](../prerequisite-org/README.md) - fundament wspolny dla AWS management account i GitHub ownera,
- [../prerequisite-repo/README.md](../prerequisite-repo/README.md) - przygotowanie konkretnego repo i kont aplikacyjnych.

## Powiazane katalogi

- [../config/README.md](../config/README.md) - presety uzywane podczas bootstrapu.
- [../prerequisite-repo/app/README.md](../prerequisite-repo/app/README.md) - account pool i archiwizacja aplikacji.
