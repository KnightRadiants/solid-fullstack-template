# US-1.2 Security and Governance Implementation Plan

## 1. Cel dokumentu

Ten dokument opisuje dokladnie, krok po kroku, co wdrazamy teraz, aby:
- ograniczyc bootstrap AWS tylko do kontrolowanego kontekstu,
- odciac collaboratorow od sekretow i zmiennych krytycznych dla AWS/GitHub governance,
- wdrozyc jasne zasady merge i approval flow dla `dev`, `main` i dodatkowych
  branchy release/QA z presetow.

Dokument dotyczy repo tworzonych z template. Testy wykonujemy na repo testowych (fork/new repo), nie na source template.

## 2. Stan obecny (baseline)

Status zweryfikowany na podstawie aktualnego repo: `2026-04-25`.

Mamy:
- `bootstrap-repo` orchestration,
- `bootstrap-repo` z jobami `create-app-accounts`, `resolve-targets`, `create-deploy-roles`, `configure-github-repo`, `bind-deploy-roles`,
- environment gate: wszystkie joby zmieniajace bootstrap/governance dzialaja na `environment: bootstrap`,
- fail-fast guard blokujacy `workflow_dispatch` spoza `refs/heads/main` przed approvalem `bootstrap`,
- admin gate: wszystkie joby zmieniajace bootstrap/governance wymagaja `require-admin-access`,
- least-privilege `permissions:` per job (`id-token`/`contents`/`deployments` tylko tam, gdzie potrzebne),
- GitHub App token flow dla operacji governance,
- sekrety `GH_APP_ID` i `GH_APP_PRIVATE_KEY` trzymane na environment `bootstrap`,
- bootstrapowe zmienne AWS (`AWS_REGION`, `AWS_ACCOUNT_ID`, `BOOTSTRAP_ROLE_NAME`, `TF_STATE_BUCKET`) trzymane na environment `bootstrap`,
- role OIDC w AWS (`gha-bootstrap-org`, `gha-environment-deploy`) z trustem opartym o `repo + environment`,
- automatyczne utrzymanie `.github/CODEOWNERS` dla krytycznych sciezek,
- automatyczne rulesety ochronne dla `main` oraz `dev` (jesli `dev` istnieje w presecie): PR required + code owner review + min approvals.

Brakuje / otwarte:
- required status checks w rulesetach (po ustaleniu docelowej listy checkow CI),
- automatyczne tworzenie per-repo reviewer teams w organizacji:
  `<repo>-<branch>-frontend-reviewers`,
  `<repo>-<branch>-backend-reviewers`,
  `<repo>-<branch>-infra-reviewers`,
- automatyczne dodawanie poczatkowego repo ownera (`github.actor` z
  `bootstrap-repo`) i uprawnionych administratorow jako maintainerow reviewer
  teams,
- branch-specific CODEOWNERS: `main`, `dev`, `test` i `stage` uzywaja
  reviewer teams per branch/domain, np.
  `<repo>-dev-frontend-reviewers`,
- rulesety dla `test` i `stage` wedlug kontraktu
  [preset-merge-rules.md](./preset-merge-rules.md),
- decyzja o emergency bypass bez omijania required checks (Etap 5),
- centralnego modelu "kto ma dostep do czego" zakodowanego w rulesetach.
- rozdzielenia rulesetow na status-check rulesets bez bypassu oraz review
  rulesets z opcjonalnym bypass dla repo ownera/administratorow.
- personal account fallback: CODEOWNERS wskazuje bezposrednio repo ownera,
  PR od innej osoby wymaga approvala repo ownera, a self-merge repo ownera
  wymaga emergency bypass tylko dla review rule.

## 3. Target security model (docelowy)

### 3.1 Kto moze robic bootstrap

- Bootstrapy (`bootstrap-repo` i workflow bootstrapowe) dzialaja tylko z `main`.
- Workflow bootstrapowe wymagaja wejscia przez GitHub Environment `bootstrap`.
- Environment `bootstrap` ma `Required reviewers` = team administratorow.

### 3.2 Gdzie trzymamy sekrety i zmienne

- Wrazliwe dane do governance (`GH_APP_PRIVATE_KEY`, `GH_APP_ID`) tylko jako `Environment secrets` (`bootstrap`).
- Dane techniczne bootstrapowe do AWS:
  - preferencyjnie jako `Environment variables` (`bootstrap`) lub org-level restricted to selected repos.
- Runtime role mapping (`AWS_ROLE_TO_ASSUME`) trzymamy per environment (`dev`, `prod`, `preview`, itd.).

### 3.3 OIDC trust boundaries

- Rola bootstrapowa (`gha-bootstrap-org`) przyjmuje token tylko dla:
  - konkretnego repo,
  - environment `bootstrap`.
- Role runtime (`gha-environment-deploy`) tylko dla:
  - konkretnego repo,
  - konkretnego environment (`dev`, `prod`, ...).

### 3.4 Merge i approvals

Szczegolowy kontrakt per preset jest w
[preset-merge-rules.md](./preset-merge-rules.md). Ponizsze zasady sa baza
dla minimalnego przeplywu `dev`/`main`.

- `dev`:
  - merge tylko przez PR,
  - required checks zielone,
  - code owner review wymagany wedlug sciezek i reviewer teams:
    `<repo>-dev-infra-reviewers`, `<repo>-dev-backend-reviewers`,
    `<repo>-dev-frontend-reviewers`,
  - minimum 1 approval od odpowiedniego ownera domenowego, nie od autora.
- `main`:
  - merge tylko przez PR,
  - required checks zielone,
  - CODEOWNERS oparty o reviewer teams `*-main-*-reviewers`, ktore domyslnie
    zawieraja repo ownera i administratorow; dodanie innych osob jest jawna
    delegacja produkcyjnego approvala,
  - minimum 1 approval spelniajacy code owner review, nie od autora.

## 4. Plan wdrozenia (kolejnosc)

### Status etapow (na 2026-04-25)

- Etap 0: zrealizowany.
- Etap 1: zrealizowany.
- Etap 2: zrealizowany.
- Etap 3: zrealizowany funkcjonalnie (`repo + environment` w trust policy); dalsze zaciesnianie policy mozliwe iteracyjnie.
- Etap 4: zrealizowany czesciowo (rulesety + CODEOWNERS dla `main`/`dev`; required status checks oraz rulesety `test`/`stage` nadal otwarte).
- Etap 5: nie zrealizowany.
- Etap 6: nie zrealizowany.

## Etap 0: Prerequisite GH automation (`gh/team` + orchestrator)

### Zmiany

1. Rozdzielic prerequisite GH na:
   - `prerequisite-org/gh/app/` (manifest flow GitHub App),
   - `prerequisite-org/gh/team/` (ensure team `administrators`, membership, opcjonalnie repo permissions),
   - `prerequisite-org/gh/bootstrap-github-governance.py` (lokalny orchestrator).
1. Zapewnic sciezke automatycznego ustawiania kontraktu bootstrap repo:
   - `GH_APP_ID` i `GH_APP_PRIVATE_KEY` jako environment secrets (`bootstrap`) przez `--bootstrap-repo` / `03-write-bootstrap-app-secrets.py`,
   - bootstrapowe variables AWS przez `02-write-bootstrap-variables.py`.
1. Zapewnic idempotencje:
   - "create if missing, update if exists".

### Test akceptacyjny

- Uruchomienie orchestratora na czystej organizacji tworzy team i app, a dla wskazanego bootstrap repo wpisuje wymagane environment secrets.
- Ponowne uruchomienie nie psuje stanu i nie duplikuje zasobow.

## Etap 1: Zamrozenie powierzchni ataku bootstrap

### Zmiany

1. Dodac guard branch dla bootstrap workflow:
   - fail jesli `github.ref != refs/heads/main` dla `workflow_dispatch`.
1. Wymusic environment `bootstrap` na jobach bootstrapowych.
1. Dodac minimalne uprawnienia `permissions:` per workflow (least privilege).

### Pliki

- `.github/workflows/bootstrap-repo.yml`

### Test akceptacyjny

- Uruchomienie bootstrap z `dev` ma fail-fast.
- Uruchomienie bootstrap z `main` przechodzi przez `bootstrap` environment gate.

## Etap 2: Sekrety i zmienne tylko przez environment gate

### Zmiany

1. Przeniesc `GH_APP_PRIVATE_KEY` do `bootstrap` environment secret.
1. Przeniesc `GH_APP_ID` do `bootstrap` environment secret.
1. Przeniesc bootstrapowe AWS vars do `bootstrap` environment variables:
   - `AWS_ROLE_TO_ASSUME` (opcjonalnie, zamiast pary ponizej)
   - `AWS_ACCOUNT_ID`
   - `BOOTSTRAP_ROLE_NAME`
   - `TF_STATE_BUCKET`
   - `AWS_REGION` (opcjonalnie globalna)
1. Usunac duplikaty tych danych z repo/global context, jesli niepotrzebne.

### Test akceptacyjny

- Collaborator bez approvala admina nie moze odpalic bootstrap z dostepem do sekretow.
- Bootstrap z approvalem admina dziala end-to-end.

## Etap 3: OIDC hardening (AWS)

### Zmiany

1. Dopicac trust policy roli bootstrapowej:
   - `sub = repo:<ORG>/<REPO>:environment:bootstrap`
1. Zweryfikowac role runtime:
   - `sub = repo:<ORG>/<REPO>:environment:<ENV>`
1. Ograniczyc policy do minimalnych akcji potrzebnych dla konkretnego etapu.

### Test akceptacyjny

- Job poza environment `bootstrap` nie moze assume bootstrap role.
- Job `dev` nie moze assume `prod` runtime role.

## Etap 4: GitHub rulesets i branch protection

### Zmiany

1. Dodac ruleset dla `dev`:
   - PR required
   - required status checks (po ustaleniu listy checkow CI)
   - code owner review required wedlug domeny sciezek i reviewer teams
   - min approvals = 1
   - jesli wymagamy approvala od kazdej dotknietej domeny w PR multi-domenowym,
     dodac osobny required check CI walidujacy approvale wzgledem CODEOWNERS
1. Dodac ruleset dla `main`:
   - PR required
   - required status checks (po ustaleniu listy checkow CI)
   - code owner review required od `*-main-*-reviewers`
   - brak direct push
1. Rozdzielic rulesety tak, zeby required checks byly w rulesecie bez bypassu,
   a ewentualny bypass repo ownera/administratorow dotyczy tylko review rule.
1. Rozszerzyc rulesety na `test` i `stage`, gdy preset tworzy te branche,
   zgodnie z [preset-merge-rules.md](./preset-merge-rules.md).
1. Dodac branch-specific CODEOWNERS dla reviewer teams:
   - `main`: `*-main-*-reviewers`,
   - `dev`: `*-dev-*-reviewers`,
   - `test`: `*-test-*-reviewers`, gdy branch istnieje,
   - `stage`: `*-stage-*-reviewers`, gdy branch istnieje.
1. Dodac automatyczne tworzenie reviewer teams i przypiecie ich do repo z
   minimalnym uprawnieniem potrzebnym do code review.
1. Zapewnic uprawnienia do automatyzacji teamow:
   - GitHub Organization jest wymagane, bo personal account nie ma Teams,
   - GitHub App/token musi miec organization permission `Members: Read and write`
     albo krok musi byc wykonany w prerequisite przez GitHub org ownera.
1. Dodac personal account fallback:
   - nie tworzyc reviewer teams,
   - generowac CODEOWNERS z bezposrednim user handle wlasciciela personal
     account (`github.repository_owner`),
   - wymagac approvala repo ownera dla PR od innych osob,
   - self-merge repo ownera dopuscic tylko przez emergency review bypass,
   - required checks pozostaja w rulesecie bez bypassu.

### Test akceptacyjny

- Collaborator nie moze pushowac bezposrednio do `dev/main`.
- PR do `main` od collaboratora wymaga approvala z odpowiedniego
  `*-main-*-reviewers`.
- Reviewer teams sa utworzone tylko dla branchy z presetu i maja w skladzie
  poczatkowego repo ownera oraz administratorow jako maintainerow.
- Dla personal account reviewer teams nie sa tworzone, CODEOWNERS wskazuje
  repo ownera, a PR od innej osoby wymaga jego approvala.

## Etap 5: Emergency bypass i policy dla self-merge

### Zmiany

1. Ustalic, czy repo dopuszcza emergency bypass.
1. Jezeli bypass istnieje, nie moze zdejmowac required checks: required checks
   musza byc w osobnym rulesecie bez bypass actors.
1. Standardowy flow do `main` wymaga approvala z odpowiedniego
   `*-main-*-reviewers`; te zespoly domyslnie zawieraja repo ownera i
   administratorow.
1. Standardowy flow do `dev` wymaga ownera domenowego zalezne od sciezek:
   infra/backend/frontend, przez `*-dev-*-reviewers`.
1. Utrzymac audit trail (merge przez PR, nie direct push), takze dla
   przypadkow awaryjnych.

### Test akceptacyjny

- PR do `main` bez approvala z odpowiedniego `*-main-*-reviewers` nie moze
  byc zmergowany.
- PR do `dev` dotykajacy infra wymaga approvala z
  `<repo>-dev-infra-reviewers`.
- PR do `dev` dotykajacy backendu wymaga approvala z
  `<repo>-dev-backend-reviewers`.
- PR do `dev` dotykajacy frontendu wymaga approvala z
  `<repo>-dev-frontend-reviewers`.
- Kazdy PR nadal wymaga zielonych checkow.
- Repo owner/admin self-merge dziala tylko przez jawny review bypass; ten bypass
  nie omija required checks.

## Etap 6: E2E security regression na repo testowym

### Scenariusze

1. Positive path:
   - `bootstrap-repo` na `main` z approvalem `bootstrap` env.
1. Negative path A:
   - bootstrap uruchomiony z `dev` -> fail.
1. Negative path B:
   - collaborator probuje uruchomic bootstrap bez approvala -> fail/gate.
1. Negative path C:
   - `dev` workflow probuje assume role `prod` -> AccessDenied.

### Artefakty

- screenshoty/links do runow,
- zapis finalnych rulesetow,
- zapis finalnych environment protections.

## 5. Proces developerski po wdrozeniu

Szczegolowe warianty dla presetow `minimal`, `dev-lite`, `dev-standard`,
`release` i `full-qa` sa opisane w
[preset-merge-rules.md](./preset-merge-rules.md).

## 5.1 Tworzenie feature

1. Developer tworzy `feature/<nazwa>` od `dev`.
1. Otwiera PR do `dev`.
1. Pipeline PR uruchamia CI + preview + E2E (etap gate).
1. Po zielonych checkach i approvalu PR trafia do `dev`.
1. Merge do `dev` odpala deploy na stale `dev`.
1. Zamkniecie PR (w tym merge) odpala destroy preview environment.

## 5.2 Release do produkcji

1. Tworzony PR `dev -> main`.
1. Dzialaja checki release gate.
1. PR do `main` wymaga approvala z odpowiedniego `*-main-*-reviewers`.
1. Merge do `main` odpala deploy `prod`.

## 5.3 Kto akceptuje co

- PR do `dev`: code owner approval wymagany wedlug sciezek i reviewer teams
  `*-dev-infra-reviewers`, `*-dev-backend-reviewers`,
  `*-dev-frontend-reviewers`; dodatkowo min 1 approval.
- PR do `main`: approval wymagany przez `*-main-*-reviewers`; domyslnie sa tam
  repo owner i administratorzy, a kazda dodatkowa osoba jest jawna delegacja.
- Owner nie omija required checks; ewentualny emergency bypass jest osobna
  decyzja governance, nie standardowym flow.

## 6. Definition of Done

Wdrozenie uznajemy za zakonczone, gdy:

1. Bootstrap jest uruchamialny tylko z `main` i przez `bootstrap` environment gate.
1. Sekrety governance nie sa dostepne poza `bootstrap` environment.
1. OIDC trust jest zawezony do repo + environment.
1. Rulesety wymagane przez wybrany preset sa aktywne i przetestowane:
   `main` zawsze, a `dev`, `test` i `stage` wtedy, gdy istnieja w
   `repo_branches`.
1. Negatywne testy dostepu (`dev` -> `prod role`) daja `AccessDenied`.
1. Proces promocji zmian opisany w [preset-merge-rules.md](./preset-merge-rules.md)
   jest udokumentowany i powtarzalny.

## 7. Kolejnosc wykonania (operacyjna checklista)

1. Etap 0
1. Etap 1
1. Etap 2
1. Etap 3
1. Etap 4
1. Etap 5
1. Etap 6

Nie zmieniamy kolejnosci, bo kazdy kolejny etap opiera sie na poprzednim.

## 8. Kolejnosc wzgledem minimalnej aplikacji FE + BE

To wdrozenie security/governance wykonujemy najpierw.
Minimalna aplikacje FE + BE dodajemy dopiero po zamknieciu etapow z tego dokumentu.
