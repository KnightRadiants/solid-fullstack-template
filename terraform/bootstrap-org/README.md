# Bootstrap

Po sklonowaniu repozytorium, musisz utworzyć odpowiedną strukturę na swoim koncie AWS.
Wymagane jest połącznie z kontem AWS, które jest kontem Organization Managment Account.

## Konfiguracja AWS-CLI

### Jeśli korzystasz z Identity Federation (zalecana metoda)

Todo: Tu można bylo by dodać cały opis zakładania federacji.

1. Ustaw profil: `$env:AWS_PROFILE = "mafi-general-sso"`
1. Skonfiguruj sso login: `aws sts get-caller-identity --profile$env:AWS_PROFILE`
1. Zaloguj się: `aws sso login --profile $env:AWS_PROFILE`
1. Zobacz jako kto jesteś zalogowany: `aws sts get-caller-identity --profile $env:AWS_PROFILE`

### Jeśli generujesz klucze CLI

Todo: uzupełnić tą sekcję.