# Preset Merge Rules

Ten dokument opisuje docelowy kontrakt merge governance dla presetow z
[../config/presets.json](../config/presets.json).

## Status kontraktu

Ten plik definiuje, co powinno byc wymagane przy merge dla branchy tworzonych
przez preset. Nie dodaje nowych pol w `config/presets.json`; implementacja
powinna wyprowadzac reguly z istniejacych pol `repo_branches`,
`aws_accounts`, `default_branch` i `enable_preview_pr`.

Stan implementacji na 2026-04-25:
- workflow `bootstrap-repo` tworzy branche i environmenty z presetu,
- automatyczne rulesety sa tworzone dla `main` oraz `dev`, jesli `dev`
  istnieje w presecie,
- aktualny bootstrap generuje jeden CODEOWNERS dla krytycznych sciezek; nie
  generuje jeszcze osobnego CODEOWNERS dla `main` i domenowego CODEOWNERS dla
  `dev`,
- aktualny bootstrap nie tworzy jeszcze per-repo reviewer teams ani nie zarzadza
  ich czlonkostwem,
- required status checks nie maja jeszcze ustalonej finalnej listy checkow,
- `stage` i `test` sa opisane w tym dokumencie jako docelowy kontrakt do
  zakodowania w rulesetach.

## Zasady globalne

Te zasady dotycza kazdego chronionego brancha w kazdym presecie:

- merge tylko przez pull request,
- direct push zablokowany,
- wymagane zielone required checks,
- minimum 1 approval od osoby innej niz autor PR,
- code owner review wymagany,
- wszystkie review threads musza byc zamkniete przed merge,
- stale approvals sa odrzucane po nowym pushu,
- promocja zmian idzie po kolei przez dostepne lane'y, bez przeskakiwania
  srodowisk.

Required checks sa tu opisane logicznie. Konkretne nazwy checkow w GitHub
Actions trzeba ustalic przed wlaczeniem ich jako wymogu w rulesetach.

## Model approvali

GitHub nie ma prostego rulesetowego warunku "wymagaj approvala od tego
konkretnego usera albo teamu" niezaleznie od plikow. Najczystszy model to:

- ruleset na branchu wlacza `require_code_owner_review`,
- `.github/CODEOWNERS` na branchu bazowym definiuje, kto musi zatwierdzic
  zmiane dla danego zestawu sciezek,
- dla PR do `main` CODEOWNERS powinien wymagac `*-main-*-reviewers`,
- dla PR do `dev` CODEOWNERS powinien wymagac odpowiedniego ownera domenowego:
  infra, backend albo frontend.

CODEOWNERS jest czytany z brancha bazowego PR. To znaczy, ze kazdy branch
moze miec inne reguly CODEOWNERS. Dzieki temu `main` moze wymagac approvala z
zespolow produkcyjnych `*-main-*-reviewers`, a `dev` moze wymagac review od
domeny, ktorej dotyka PR.

Jezeli potrzebujemy twardo wymusic approval od kazdej domeny dotknietej przez
jeden PR, np. infra i frontend w tym samym PR, nalezy dodac wymagany check CI,
ktory porowna liste zmienionych plikow z CODEOWNERS i sprawdzi approvale.
Native CODEOWNERS jest dobrym mechanizmem bazowym, ale niestandardowe reguly
multi-domenowe powinny byc weryfikowane osobnym checkiem.

## Reviewer teams

Reviewerzy powinni byc GitHub Teams w organizacji, przypiete do konkretnego
repo, brancha i domeny. Zespoly sa technicznie org-level, ale nazwa zawiera
repo, zeby byly uzywane jak repo-scoped teams i nie kolidowaly miedzy repo.

Konwencja nazw:

```text
<repo>-<branch>-frontend-reviewers
<repo>-<branch>-backend-reviewers
<repo>-<branch>-infra-reviewers
```

Przyklad dla repo `todo-list`:

```text
todo-list-dev-frontend-reviewers
todo-list-dev-backend-reviewers
todo-list-dev-infra-reviewers
todo-list-main-frontend-reviewers
todo-list-main-backend-reviewers
todo-list-main-infra-reviewers
todo-list-stage-frontend-reviewers
todo-list-stage-backend-reviewers
todo-list-stage-infra-reviewers
```

Zasady czlonkostwa:

- bootstrap powinien automatycznie tworzyc reviewer teams dla branchy
  istniejacych w `repo_branches`,
- repo owner wybrany dla bootstrapowanego repo powinien byc automatycznie
  dodany jako `maintainer` do wszystkich reviewer teams tego repo,
- minimalny kontrakt bez nowego workflow inputu: poczatkowym repo ownerem jest
  `github.actor`, ktory uruchamia `bootstrap-repo`,
- czlonkowie teamu `administrators`, ktorzy maja moc zatwierdzac i zarzadzac
  reviewer teams, powinni byc dodani jako `maintainer` do reviewer teams albo
  jawnie wpisani w CODEOWNERS,
- zwykli reviewerzy domenowi sa dodawani jako `member`,
- czlonkostwem reviewer teamu moze zarzadzac tylko org owner albo team
  maintainer; w naszym modelu repo owner i uprawnieni administratorzy sa
  dodawani jako maintainers tych reviewer teams,
- PR author nie moze zatwierdzic wlasnego PR jako wymagany reviewer,
- czlonek `administrators` albo repo owner moze zatwierdzac PR niezaleznie od
  domeny, jezeli jest czlonkiem odpowiedniego reviewer teamu dla target
  brancha.

`main` jest bardziej restrykcyjny niz `dev`: main reviewer teams powinny
domyslnie zawierac tylko repo ownera i administratorow. Dodanie innych osob
do `*-main-*-reviewers` jest jawna delegacja produkcyjnego approvala.

GitHub Teams istnieja tylko w organizacjach. Dla personal account fallbackiem
sa bezposrednie user handles w CODEOWNERS, ale taki fallback nie spelnia
kontraktu zarzadzanych reviewer teams. Docelowo automatyczny model grup
reviewerskich wymaga GitHub Organization.

Automatyczne tworzenie teamow i zarzadzanie czlonkostwem wymaga uprawnien
organizacyjnych. Implementacja musi uzyc GitHub App/tokena z organization
permission `Members: Read and write` albo wykonac ten krok w prerequisite
narzedziem uruchamianym przez GitHub org ownera.

## Personal account fallback

Jesli repo nie jest w GitHub Organization, tylko pod personal account, nie ma
GitHub Teams. Wtedy wybieramy tryb ograniczony:

- nie tworzymy reviewer teams,
- nie probujemy automatycznie zarzadzac grupami reviewerow,
- CODEOWNERS uzywa bezposredniego user handle wlasciciela personal account
  (`github.repository_owner`),
- PR od osoby innej niz repo owner wymaga approvala repo ownera,
- PR autora bedacego repo ownerem nie moze byc zatwierdzony przez jego wlasny
  approval, wiec self-merge wymaga emergency bypass dla review rule,
- emergency bypass nie moze omijac required status checks.

Minimalny CODEOWNERS dla personal account:

```text
* @repo-owner
```

Dla personal repo mozna opcjonalnie wpisac osobnych userow per domena, ale to
nie jest zarzadzany model grupowy i kazda zmiana reviewerow wymaga zmiany
CODEOWNERS:

```text
.github/** @repo-owner
config/** @repo-owner
docs/** @repo-owner
prerequisite-org/** @repo-owner
prerequisite-repo/** @repo-owner
scripts/** @repo-owner
terraform/** @repo-owner

backend/** @backend-reviewer
api/** @backend-reviewer
services/** @backend-reviewer

frontend/** @frontend-reviewer
web/** @frontend-reviewer
ui/** @frontend-reviewer
```

Ten fallback jest dobry dla forkow, repo prywatnych i testow bootstrapu poza
organizacja. Pelny governance z reviewer teams wymaga GitHub Organization.

## Self-merge i bypass

GitHub nie liczy approvala autora PR jako wymaganego approvala. To jest dobre
dla zwyklych contributorow, bo wymusza review od drugiej osoby. Wyjatek dla
repo ownera albo administratorow nie moze byc zrobiony jako "self-approval";
technicznie musi byc zrobiony jako bypass rulesetu wymagajacego PR review.

Zeby taki bypass nie omijal testow, rulesety powinny byc rozdzielone:

- ruleset `bootstrap-checks-<branch>`:
  - required status checks,
  - brak bypass actors,
- ruleset `bootstrap-review-<branch>`:
  - PR required,
  - code owner review required,
  - min approvals = 1,
  - bypass actors tylko dla repo ownera/administratorow, najlepiej w trybie
    `pull_request`, jesli API/plan GitHuba na to pozwala.

W standardowym flow kazdy PR wymaga approvala z odpowiedniego reviewer teamu.
W personal account fallbacku PR od innej osoby wymaga approvala repo ownera
z CODEOWNERS. Repo owner albo administrator moze uzyc bypass tylko jako jawnej
sciezki awaryjnej/audytowalnej; required checks nadal musza przejsc.

Proponowany CODEOWNERS dla `main`:

```text
* @org/<repo>-main-infra-reviewers

.github/** @org/<repo>-main-infra-reviewers
config/** @org/<repo>-main-infra-reviewers
docs/** @org/<repo>-main-infra-reviewers
prerequisite-org/** @org/<repo>-main-infra-reviewers
prerequisite-repo/** @org/<repo>-main-infra-reviewers
scripts/** @org/<repo>-main-infra-reviewers
terraform/** @org/<repo>-main-infra-reviewers

backend/** @org/<repo>-main-backend-reviewers
api/** @org/<repo>-main-backend-reviewers
services/** @org/<repo>-main-backend-reviewers

frontend/** @org/<repo>-main-frontend-reviewers
web/** @org/<repo>-main-frontend-reviewers
ui/** @org/<repo>-main-frontend-reviewers
```

Proponowany CODEOWNERS dla `dev`, `test` i `stage`:

```text
.github/** @org/<repo>-<branch>-infra-reviewers
config/** @org/<repo>-<branch>-infra-reviewers
docs/** @org/<repo>-<branch>-infra-reviewers
prerequisite-org/** @org/<repo>-<branch>-infra-reviewers
prerequisite-repo/** @org/<repo>-<branch>-infra-reviewers
scripts/** @org/<repo>-<branch>-infra-reviewers
terraform/** @org/<repo>-<branch>-infra-reviewers

backend/** @org/<repo>-<branch>-backend-reviewers
api/** @org/<repo>-<branch>-backend-reviewers
services/** @org/<repo>-<branch>-backend-reviewers

frontend/** @org/<repo>-<branch>-frontend-reviewers
web/** @org/<repo>-<branch>-frontend-reviewers
ui/** @org/<repo>-<branch>-frontend-reviewers
```

Autor PR nie moze zatwierdzic wlasnego PR jako wymagany reviewer, wiec dla
repo jednoosobowych trzeba miec drugiego uprawnionego ownera/reviewera albo
swiadomie dopuscic osobny emergency bypass poza standardowym flow.

## Znaczenie branchy

| Branch | Rola | Typowy merge do brancha | Minimalny gate |
| --- | --- | --- | --- |
| `main` | produkcja / prod release | z ostatniego stabilnego brancha przed produkcja | release checks, approval z `*-main-*-reviewers`, 1 approval, resolved threads |
| `dev` | integracja developerska | z `feature/*`, `fix/*`, `chore/*` | CI checks, approval z `*-dev-*-reviewers`, 1 approval |
| `test` | dedykowany QA/regresja | z `dev` | CI + regression/integration checks, approval z `*-test-*-reviewers`, resolved threads |
| `stage` | release candidate / staging | z `test`, a gdy nie ma `test`, z `dev` | release smoke checks, approval z `*-stage-*-reviewers`, resolved threads |

Konta `preview`, `shared` i `logging` nie tworza samodzielnych long-lived
branchy. `preview` jest lane'em dla PR, a `shared` i `logging` sa kontami
pomocniczymi.

## Preset: minimal

Preset:
- branche: `main`,
- konta: `prod`,
- default branch: `main`,
- preview PR: wylaczone.

Przeplyw:

```text
feature/* -> main -> prod
```

Wymagania przy merge do `main`:
- PR required,
- required checks zielone,
- approval z odpowiedniego `*-main-*-reviewers`,
- minimum 1 approval spelniajacy code owner review,
- wszystkie review threads resolved,
- direct push zablokowany.

Ten preset jest najmniejszy, wiec `main` jest jednoczesnie branchem
integracyjnym i release branchem. Nadaje sie do prostych aplikacji albo
repo startowych, gdzie koszt utrzymania osobnego `dev` jest wiekszy niz
ryzyko procesu.

## Preset: dev-lite

Preset:
- branche: `main`, `dev`,
- konta: `prod`, `dev`, `shared`,
- default branch: `dev`,
- preview PR: wylaczone.

Przeplyw:

```text
feature/* -> dev -> main -> prod
```

Wymagania przy merge do `dev`:
- PR required,
- required checks zielone,
- minimum 1 approval z odpowiedniego `*-dev-*-reviewers`,
- wszystkie review threads resolved,
- direct push zablokowany.

Wymagania przy merge do `main`:
- PR required, zwykle `dev -> main`,
- release checks zielone,
- approval z odpowiedniego `*-main-*-reviewers`,
- minimum 1 approval spelniajacy code owner review,
- wszystkie review threads resolved,
- direct push zablokowany.

Ten preset rozdziela prace developerska od produkcji, ale nie tworzy preview
per PR.

## Preset: dev-standard

Preset:
- branche: `main`, `dev`,
- konta: `prod`, `dev`, `preview`, `shared`,
- default branch: `dev`,
- preview PR: wlaczone.

Przeplyw:

```text
feature/* -> preview PR -> dev -> main -> prod
```

Wymagania przy merge do `dev`:
- PR required,
- required checks zielone,
- preview PR utworzony i sprawdzony przez pipeline, gdy workflow preview
  istnieje,
- minimum 1 approval z odpowiedniego `*-dev-*-reviewers`,
- wszystkie review threads resolved,
- direct push zablokowany.

Wymagania przy merge do `main`:
- PR required, zwykle `dev -> main`,
- release checks zielone,
- approval z odpowiedniego `*-main-*-reviewers`,
- minimum 1 approval spelniajacy code owner review,
- wszystkie review threads resolved,
- direct push zablokowany.

Ten preset jest domyslny dla zespolow aplikacyjnych: daje stale `dev`,
produkcje oraz krotko zyjace preview dla PR.

## Preset: release

Preset:
- branche: `main`, `dev`, `stage`,
- konta: `prod`, `dev`, `stage`, `preview`, `shared`, `logging`,
- default branch: `dev`,
- preview PR: wlaczone.

Przeplyw:

```text
feature/* -> preview PR -> dev -> stage -> main -> prod
```

Wymagania przy merge do `dev`:
- PR required,
- required checks zielone,
- preview PR utworzony i sprawdzony przez pipeline, gdy workflow preview
  istnieje,
- minimum 1 approval z odpowiedniego `*-dev-*-reviewers`,
- wszystkie review threads resolved,
- direct push zablokowany.

Wymagania przy merge do `stage`:
- PR required, zwykle `dev -> stage`,
- required checks zielone,
- release smoke lub E2E checks zielone, gdy istnieja,
- minimum 1 approval z odpowiedniego `*-stage-*-reviewers`,
- wszystkie review threads resolved,
- direct push zablokowany.

Wymagania przy merge do `main`:
- PR required, zwykle `stage -> main`,
- required checks zielone,
- approval z odpowiedniego `*-main-*-reviewers`,
- release approval wymagany; moze byc tym samym approvalem, jesli reviewer team
  pelni role release approvera,
- wszystkie review threads resolved,
- direct push zablokowany.

Ten preset dodaje `stage` jako ostatnia probe generalna przed produkcja.
`stage` powinien byc stabilniejszy niz `dev`; poprawki do release ida przez
PR i wracaja do nizszych lane'ow, zeby historia nie rozjechala sie miedzy
branchami.

## Preset: full-qa

Preset:
- branche: `main`, `dev`, `stage`, `test`,
- konta: `prod`, `dev`, `stage`, `test`, `preview`, `shared`, `logging`,
- default branch: `dev`,
- preview PR: wlaczone.

Przeplyw:

```text
feature/* -> preview PR -> dev -> test -> stage -> main -> prod
```

Wymagania przy merge do `dev`:
- PR required,
- required checks zielone,
- preview PR utworzony i sprawdzony przez pipeline, gdy workflow preview
  istnieje,
- minimum 1 approval z odpowiedniego `*-dev-*-reviewers`,
- wszystkie review threads resolved,
- direct push zablokowany.

Wymagania przy merge do `test`:
- PR required, zwykle `dev -> test`,
- CI checks zielone,
- regression/integration checks zielone, gdy istnieja,
- minimum 1 approval z odpowiedniego `*-test-*-reviewers`,
- wszystkie review threads resolved,
- direct push zablokowany.

Wymagania przy merge do `stage`:
- PR required, zwykle `test -> stage`,
- release smoke lub E2E checks zielone, gdy istnieja,
- minimum 1 approval z odpowiedniego `*-stage-*-reviewers`,
- wszystkie review threads resolved,
- direct push zablokowany.

Wymagania przy merge do `main`:
- PR required, zwykle `stage -> main`,
- required checks zielone,
- approval z odpowiedniego `*-main-*-reviewers`,
- release approval wymagany; moze byc tym samym approvalem, jesli reviewer team
  pelni role release approvera,
- wszystkie review threads resolved,
- direct push zablokowany.

Ten preset jest dla zespolow, ktore potrzebuja osobnego toru QA przed
stagingiem. `test` sluzy do szerszej regresji i integracji, a `stage` zostaje
zamrozonym kandydatem release.

## Kontrakt dla implementacji rulesetow

Implementacja rulesetow powinna wynikac z presetu:

- `main` jest zawsze chroniony,
- `dev` jest chroniony, gdy istnieje w `repo_branches`,
- `test` jest chroniony, gdy istnieje w `repo_branches`,
- `stage` jest chroniony, gdy istnieje w `repo_branches`,
- kazdy chroniony branch powinien miec CODEOWNERS oparty o reviewer teams
  `<repo>-<branch>-frontend-reviewers`,
  `<repo>-<branch>-backend-reviewers` i
  `<repo>-<branch>-infra-reviewers`,
- ruleset nie powinien byc tworzony dla kont bez odpowiadajacego brancha
  (`preview`, `shared`, `logging`),
- required checks mozna wlaczyc dopiero po ustaleniu konkretnych nazw jobow CI,
- automatyczne tworzenie i aktualizacja reviewer teams wymagaja GitHub
  Organization oraz uprawnien do zarzadzania teamami/czlonkostwem,
- personal account dziala tylko w trybie ograniczonym: CODEOWNERS wskazuje
  user handle repo ownera, a self-merge repo ownera wymaga emergency bypass
  dla review rule,
- self-merge wyjatku dla repo ownera/administratorow nie robimy przez
  self-approval; jesli jest potrzebny, robimy go przez bypass tylko na rulesecie
  review, przy osobnym rulesecie status checks bez bypassu.
