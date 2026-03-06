# US-1.2 Security and Governance Implementation Plan

## 1. Cel dokumentu

Ten dokument opisuje dokladnie, krok po kroku, co wdrazamy teraz, aby:
- ograniczyc bootstrap AWS tylko do kontrolowanego kontekstu,
- odciac collaboratorow od sekretow i zmiennych krytycznych dla AWS/GitHub governance,
- wdrozyc jasne zasady merge i approval flow dla `dev` i `main`.

Dokument dotyczy repo tworzonych z template. Testy wykonujemy na repo testowych (fork/new repo), nie na source template.

## 2. Stan obecny (baseline)

Mamy:
- `bootstrap-all` orchestration,
- `bootstrap-org`, `bootstrap-iam-matrix`, `bootstrap-gh-core`, `bootstrap-gh-bind`,
- GitHub App token flow dla operacji governance,
- role OIDC w AWS (`gha-bootstrap-org`, `gha-environment-deploy`).

Brakuje:
- twardych branch/ruleset protections,
- twardego ograniczenia bootstrap workflow do `main` + admin gate,
- centralnego modelu "kto ma dostep do czego".

## 3. Target security model (docelowy)

### 3.1 Kto moze robic bootstrap

- Bootstrapy (`bootstrap-all` i workflow bootstrapowe) dzialaja tylko z `main`.
- Workflow bootstrapowe wymagaja wejscia przez GitHub Environment `bootstrap`.
- Environment `bootstrap` ma `Required reviewers` = team administratorow.

### 3.2 Gdzie trzymamy sekrety i zmienne

- Wrazliwe dane do governance (GitHub App private key) tylko jako `Environment secrets` (`bootstrap`).
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

- `dev`:
  - merge tylko przez PR,
  - required checks zielone,
  - code owner review required dla wszystkich poza repo owner,
  - minimum 1 approval (nie od autora), chyba ze owner korzysta z bypass.
- `main`:
  - merge tylko przez PR,
  - required checks zielone,
  - code owner review required,
  - repo owner jako bypass actor dla chronionych branchy (`main`, `dev`, i pozostale objete rulesetem), self-merge bez self-approval, ale z checkami.

## 4. Plan wdrozenia (kolejnosc)

## Etap 0: Prerequisite GH automation (`gh/team` + orchestrator)

### Zmiany

1. Rozdzielic prerequisite GH na:
   - `terraform/prerequisite/gh/app/` (manifest flow GitHub App),
   - `terraform/prerequisite/gh/team/` (ensure team `administrators`, membership, opcjonalnie repo permissions),
   - `terraform/prerequisite/gh/bootstrap-gh.py` (lokalny orchestrator).
1. Dodac krok automatycznego ustawiania secrets/variables po `gh/app`:
   - `GH_APP_ID`
   - `GH_APP_PRIVATE_KEY`
   - pozostale bootstrapowe zmienne potrzebne workflow.
1. Zapewnic idempotencje:
   - "create if missing, update if exists".

### Test akceptacyjny

- Uruchomienie orchestratora na czystej organizacji tworzy team, app i wpisuje wymagane secrets/variables.
- Ponowne uruchomienie nie psuje stanu i nie duplikuje zasobow.

## Etap 1: Zamrozenie powierzchni ataku bootstrap

### Zmiany

1. Dodac guard branch dla bootstrap workflow:
   - fail jesli `github.ref != refs/heads/main` dla `workflow_dispatch`.
1. Wymusic environment `bootstrap` na jobach bootstrapowych.
1. Dodac minimalne uprawnienia `permissions:` per workflow (least privilege).

### Pliki

- `.github/workflows/bootstrap-all.yml`
- `.github/workflows/bootstrap-org.yml`
- `.github/workflows/bootstrap-iam-matrix.yml`
- `.github/workflows/bootstrap-gh-core.yml`
- `.github/workflows/bootstrap-gh-bind.yml`

### Test akceptacyjny

- Uruchomienie bootstrap z `dev` ma fail-fast.
- Uruchomienie bootstrap z `main` przechodzi przez `bootstrap` environment gate.

## Etap 2: Sekrety i zmienne tylko przez environment gate

### Zmiany

1. Przeniesc `GH_APP_PRIVATE_KEY` do `bootstrap` environment secret.
1. Przeniesc `GH_APP_ID` do `bootstrap` environment variable/secret.
1. Przeniesc bootstrapowe AWS vars do `bootstrap` environment variables:
   - `AWS_ROLE_TO_ASSUME`
   - `TF_STATE_BUCKET`
   - `TF_LOCK_TABLE`
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
   - required status checks
   - code owner review required (dla wszystkich poza owner, ktory moze uzyc bypass)
   - min approvals = 1
1. Dodac ruleset dla `main`:
   - PR required
   - required status checks
   - code owner review required
   - brak direct push
1. Dodac CODEOWNERS dla:
   - `.github/workflows/**`
   - `terraform/**`
   - `config/presets.json`

### Test akceptacyjny

- Collaborator nie moze pushowac bezposrednio do `dev/main`.
- PR do `main` od collaboratora wymaga code owner approval.

## Etap 5: Owner bypass i policy dla self-merge

### Zmiany

1. Ustawic repo owner jako bypass actor dla wszystkich chronionych branchy (`main`, `dev`, pozostale objete rulesetem).
1. Nie zdejmowac required checks.
1. Utrzymac audit trail (merge przez PR, nie direct push).

### Test akceptacyjny

- Owner moze zmergowac wlasny PR do `main` bez dodatkowego approvala.
- Owner moze zmergowac wlasny PR do `dev` bez dodatkowego approvala.
- Ten sam PR nadal wymaga zielonych checkow.

## Etap 6: E2E security regression na repo testowym

### Scenariusze

1. Positive path:
   - `bootstrap-all` na `main` z approvalem `bootstrap` env.
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
1. PR do `main` wymaga code owner approval (chyba ze owner korzysta z bypass dla self-PR).
1. Merge do `main` odpala deploy `prod`.

## 5.3 Kto akceptuje co

- PR do `dev`: code owner approval wymagany dla wszystkich poza repo owner; dodatkowo min 1 approval.
- PR do `main`: code owner/repo owner.
- Owner:
  - moze self-merge do `main` i `dev` przez bypass actor,
  - nadal nie omija required checks.

## 6. Definition of Done

Wdrozenie uznajemy za zakonczone, gdy:

1. Bootstrap jest uruchamialny tylko z `main` i przez `bootstrap` environment gate.
1. Sekrety governance nie sa dostepne poza `bootstrap` environment.
1. OIDC trust jest zawezony do repo + environment.
1. Rulesety `dev` i `main` sa aktywne i przetestowane.
1. Negatywne testy dostepu (`dev` -> `prod role`) daja `AccessDenied`.
1. Proces PR->dev oraz dev->main jest udokumentowany i powtarzalny.

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
