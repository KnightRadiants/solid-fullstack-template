# GitHub App Manifest Flow

Breadcrumbs: [Repo](../../../README.md) > [Faza 1: bootstrap organizacji](../../README.md) > [GitHub governance](../README.md) > **GitHub App**

## Spis tresci

- [Cel katalogu](#cel-katalogu)
- [Kolejnosc wykonywania](#kolejnosc-wykonywania)
- [Uruchomienie](#uruchomienie)
- [Wynik](#wynik)
- [Wymagane permissiony appki](#wymagane-permissiony-appki)

## Cel katalogu

Ten katalog zawiera manifest-flow do utworzenia GitHub App.
Zwykle nie uruchamiasz go bezposrednio, bo robi to [../bootstrap-github-governance.py](../bootstrap-github-governance.py).

## Kolejnosc wykonywania

`bootstrap-gh-app-manifest.py`:
1. Buduje manifest GitHub App.
1. Otwiera lokalna strone auto-submit w przegladarce.
1. Przekierowuje do GitHub App manifest flow.
1. Czeka na callback z GitHub.
1. Wymienia manifest code na credentials Appki.
1. Zapisuje private key i payload credentials do katalogu output.

## Uruchomienie

```ps1
Set-Location prerequisite-org/gh
python app/bootstrap-gh-app-manifest.py `
  --org KnightRadiants `
  --app-name "gha-template-bootstrap" `
  --description "Bootstrap app for template governance" `
  --output-dir "./app/out"
```

Przegladarka otwiera sie automatycznie.
Jesli chcesz to wylaczyc, uzyj `--no-open-browser`.

## Wynik

Po zatwierdzeniu w przegladarce skrypt zapisze:
- `app/out/github-app-<APP_ID>.private-key.pem`,
- `app/out/github-app-<APP_ID>.credentials.json`.

## Wymagane permissiony appki

Repository permissions:
- `Administration: Read and write`
- `Contents: Read and write`
- `Actions: Read and write`
- `Deployments: Read and write`
- `Environments: Read and write`
- `Metadata: Read-only`
