# Scripts

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

  - [Wspolne: Backend API](../backend/README.md)
  - [Wspolne: Config](../config/README.md)
  - **Wspolne: Scripts**

## Spis tresci

- [Cel katalogu](#cel-katalogu)
- [close-accounts-in-ou.ps1](#close-accounts-in-oups1)
- [Wymagania](#wymagania)
- [Uzycie](#uzycie)
- [Uwagi](#uwagi)

## Cel katalogu

Ten katalog zawiera skrypty pomocnicze spoza glownego bootstrap flow.

## close-accounts-in-ou.ps1

Skrypt do masowego wysylania `CloseAccount` dla wszystkich aktywnych kont w podanym OU AWS Organizations.

Co robi:
1. Pobiera konta z OU (`list-accounts-for-parent`).
1. Filtruje tylko konta w stanie `ACTIVE`.
1. Dla kazdego konta wywoluje `aws organizations close-account`.
1. Nie usuwa OU i nie przenosi kont.

## Wymagania

- AWS CLI z profilem, ktory ma uprawnienia Organizations na management account.
- Aktywna sesja SSO (`aws sso login`).

## Uzycie

```powershell
$env:AWS_PROFILE = "mafi-general-sso"
aws sso login --profile $env:AWS_PROFILE
```

Podglad bez zmian:

```powershell
.\scripts\close-accounts-in-ou.ps1 -OuId "ou-xxxxxxxxxxxx" -WhatIf
```

Wykonanie z potwierdzeniem:

```powershell
.\scripts\close-accounts-in-ou.ps1 -OuId "ou-xxxxxxxxxxxx"
```

Wykonanie bez dodatkowego promptu `Type CLOSE to continue`:

```powershell
.\scripts\close-accounts-in-ou.ps1 -OuId "ou-xxxxxxxxxxxx" -Force
```

Wykonanie bez interaktywnego promptu `Confirm` PowerShell:

```powershell
.\scripts\close-accounts-in-ou.ps1 -OuId "ou-xxxxxxxxxxxx" -Force -Confirm:$false
```

## Uwagi

- `CloseAccount` jest asynchroniczne; status kont zmienia sie po czasie.
- Skrypt nie zamyka kont, ktore nie sa w stanie `ACTIVE`.
- Skrypt nie modyfikuje Terraform state.
