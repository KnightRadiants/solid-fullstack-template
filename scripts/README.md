# Scripts

## close-accounts-in-ou.ps1

Skrypt do masowego wysylania `CloseAccount` dla wszystkich aktywnych kont w podanym OU AWS Organizations.

### Co robi

1. Pobiera konta z OU (`list-accounts-for-parent`).
1. Filtruje tylko konta w stanie `ACTIVE`.
1. Dla kazdego konta wywoluje `aws organizations close-account`.
1. Nie usuwa OU i nie przenosi kont.

### Wymagania

- AWS CLI z profilem, ktory ma uprawnienia Organizations na management account.
- Aktywna sesja SSO (`aws sso login`).

### Uzycie

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

### Uwagi

- `CloseAccount` jest asynchroniczne; status kont zmienia sie po czasie.
- Skrypt nie zamyka kont, ktore nie sa w stanie `ACTIVE`.
- Skrypt nie modyfikuje Terraform state.
