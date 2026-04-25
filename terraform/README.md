# Application Terraform

## Mapa README

- [Repo](../README.md)
  - [Faza 1: Organization prerequisite](../prerequisite-org/README.md)
    - [AWS foundation](../prerequisite-org/terraform/README.md)
    - [GitHub governance](../prerequisite-org/gh/README.md)
      - [GitHub App](../prerequisite-org/gh/app/README.md)
      - [Administrators team](../prerequisite-org/gh/team/README.md)
  - [Faza 2: Repository prerequisite](../prerequisite-repo/README.md)
    - [GitHub Actions workflow](../.github/workflows/README.md)
    - [Account pool](../prerequisite-repo/app/README.md)
    - [Terraform prerequisite](../prerequisite-repo/terraform/README.md)
      - [aws-accounts](../prerequisite-repo/terraform/aws-accounts/README.md)
      - [aws-iam](../prerequisite-repo/terraform/aws-iam/README.md)
  - **Faza 3: Application Terraform**
  - [Wspolne: Config](../config/README.md)
  - [Wspolne: Scripts](../scripts/README.md)

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
