# GH prerequisite: app

Ten katalog zawiera manifest-flow do utworzenia GitHub App.

## Uruchomienie

```ps1
Set-Location terraform/prerequisite/gh
python app/bootstrap-gh-app-manifest.py `
  --org KnightRadiants `
  --app-name "gha-template-bootstrap" `
  --description "Bootstrap app for template governance" `
  --output-dir "./app/out" `
  --open-browser
```

Po zatwierdzeniu w przegladarce skrypt zapisze:
- `app/out/github-app-<APP_ID>.private-key.pem`
- `app/out/github-app-<APP_ID>.credentials.json`

## Wymagane permissiony appki

Repository permissions:
- `Administration: Read and write`
- `Contents: Read and write`
- `Actions: Read and write`
- `Deployments: Read and write`
- `Environments: Read and write`
- `Metadata: Read-only`
