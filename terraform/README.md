# Application Terraform

Ten katalog jest zarezerwowany na docelowa infrastrukture aplikacji.

Prerequisite'y zostaly rozdzielone:
- `prerequisite-org/` - fundament wspolny dla AWS management account i GitHub ownera
- `prerequisite-repo/` - przygotowanie konkretnego repo i kont aplikacyjnych

Tutaj powinny trafiac dopiero zasoby runtime aplikacji, np. VPC, ECS/Lambda, RDS, S3 aplikacyjne, CloudFront i monitoring.
