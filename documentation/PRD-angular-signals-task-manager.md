# PRD — Angular Signals Task Manager

**Version:** 2.0
**Author:** LucasM69
**Date:** 2026-06-10
**Status:** Draft

---

## 1. Overview

A showcase Angular 19+ task manager application built entirely with the Signals API. The goal is to demonstrate a concrete and publishable mastery of Angular's new reactive primitives, serving as a portfolio reference for senior engineers.

---

## 2. Objectives

- Build a fully functional task manager using Angular 19+ Standalone Components
- Demonstrate idiomatic use of `signal()`, `computed()`, and `effect()`
- Produce a clean, maintainable codebase publishable on GitHub
- Provide a CI pipeline and professional README that communicate value instantly to a technical recruiter

---

## 3. Target Audience

| Audience | Context |
|---|---|
| Developer (author) | Fullstack engineer, 7 years Angular / .NET experience, learning Signals |
| Technical recruiters | Evaluate code quality and modern Angular expertise via GitHub |

---

## 4. Technical Constraints

| Constraint | Decision | Rationale |
|---|---|---|
| Angular version | 19+ only | Required to use stable Signals API |
| Component model | Standalone Components | No NgModules — modern Angular standard |
| Reactivity | `signal()`, `computed()`, `effect()` | No `BehaviorSubject`, no RxJS except HTTP |
| State management | Custom Signals-based store | No NgRx — keep it simple and self-contained |
| Testing | Vitest | Faster than Jest, native ESM support |
| UI framework | Angular Material | Consistent design system, no custom CSS overhead |
| Persistence | SQLite via .NET Minimal API backend | Survives server restart, enables real querying |
| Backend | .NET Minimal API + Microsoft.Data.Sqlite | Matches author expertise, minimal footprint |
| Deploy | Docker Compose | Single command to run frontend + backend together |
| Dependencies | Open source only | Zero paid packages |
| CI | GitHub Actions | Build + tests on every push |

---

## 5. Architecture

### 5.1 Folder Structure

```
src/
├── app/
│   ├── core/
│   │   ├── task.store.ts          # Signal-based state store
│   │   └── task.service.ts        # HTTP calls to the backend API
│   ├── models/
│   │   └── task.model.ts          # Task interface / type
│   ├── components/
│   │   ├── task-list/             # Displays the task list
│   │   ├── task-item/             # Single task row (inline edit)
│   │   ├── task-form/             # Add task form
│   │   └── task-filter/           # Filter pill controls
│   └── app.component.ts           # Root standalone component

backend/                           # .NET Minimal API
├── Program.cs                     # App entry point + route definitions
├── TaskDb.cs                      # SQLite connection (Microsoft.Data.Sqlite)
├── Task.cs                        # Task model
├── tasks.db                       # SQLite database file (git-ignored)
└── backend.csproj

docker-compose.yml                 # Runs frontend + backend together
```

### 5.2 Data Schema

```ts
interface Task {
  id: string;         // uuid
  title: string;
  completed: boolean;
  createdAt: Date;
}
```

SQLite table: `tasks`

```sql
CREATE TABLE tasks (
  id TEXT PRIMARY KEY,
  title TEXT NOT NULL,
  completed INTEGER NOT NULL DEFAULT 0,
  created_at TEXT NOT NULL
);
```

### 5.3 API Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/tasks` | Fetch all tasks |
| POST | `/api/tasks` | Create a task |
| PATCH | `/api/tasks/:id/title` | Update task title (F03) |
| PATCH | `/api/tasks/:id/complete` | Toggle completion status (F05) |
| DELETE | `/api/tasks/:id` | Delete a task |
| GET | `/health` | Backend health check (Docker + CI) |

### 5.4 Signal Usage

| Signal type | Where | Why |
|---|---|---|
| `signal()` | Tasks array, active filter | Mutable reactive state owned by the store |
| `computed()` | Filtered list, remaining count | Derived read-only values, auto-updated |
| `effect()` | Sync store state after HTTP responses | Side effect triggered by state changes |

---

## 6. Features

---

### F01 — Task List

**As a** user,
**I want to** see all my tasks in a list,
**so that** I have an overview of what needs to be done.

**Acceptance Criteria**
- [ ] Tasks are displayed in creation order (newest first)
- [ ] Each task shows its title and completion status
- [ ] An empty state message is shown when no tasks exist: *"No tasks yet. Add your first task above."*
- [ ] The list updates reactively when tasks are added, edited, or deleted
- [ ] Manual reordering is out of scope for v1

**Components:** `task-list`, `task-item`

**Technical Notes**
- Reads from a `computed()` signal (filtered view from the store)
- Data fetched from `GET /api/tasks` on init

---

### F02 — Add a Task

**As a** user,
**I want to** add a new task,
**so that** I can track something I need to do.

**Acceptance Criteria**
- [ ] A form/input is available to enter a task title
- [ ] Max title length is 200 characters — a character counter is displayed near the input
- [ ] Submitting an empty title is not allowed (validation)
- [ ] Pressing Enter submits the form
- [ ] After submission, the new task appears at the top of the list
- [ ] The input is cleared and focus returns to it after submission

**Components:** `task-form`

**Technical Notes**
- Calls `POST /api/tasks`, then updates the store `signal()`

---

### F03 — Edit a Task

**As a** user,
**I want to** edit the title of an existing task,
**so that** I can correct or clarify what needs to be done.

**Acceptance Criteria**
- [ ] Clicking a task title turns it into an inline input field
- [ ] The input pre-fills with the current title
- [ ] Pressing Enter saves the change
- [ ] Pressing Escape cancels and restores the original title
- [ ] Saving updates the task in place — position in the list does not change (hard requirement)

**Components:** `task-item`

**Technical Notes**
- Calls `PATCH /api/tasks/:id/title`
- Saving an empty title is blocked — same validation rule as F02 (consistency)

---

### F04 — Delete a Task

**As a** user,
**I want to** delete a task,
**so that** I can remove things that are no longer relevant.

**Acceptance Criteria**
- [ ] Each task has a delete action (icon button)
- [ ] The task is removed immediately from the list
- [ ] No confirmation dialog (considered and rejected — low stakes, no bulk delete risk)

**Components:** `task-item`

**Technical Notes**
- Calls `DELETE /api/tasks/:id`

---

### F05 — Complete a Task

**As a** user,
**I want to** mark a task as completed,
**so that** I can track my progress.

**Acceptance Criteria**
- [ ] Each task has a checkbox to toggle completion
- [ ] Completed tasks display with strikethrough text and 50% opacity
- [ ] Completed tasks stay in their current position — no auto-sort to bottom
- [ ] Toggling is instant and reactive
- [ ] Bulk complete is out of scope for v1

**Components:** `task-item`

**Technical Notes**
- Calls `PATCH /api/tasks/:id/complete`
- Completion state drives the reactive counter (F06)

---

### F06 — Reactive Counter

**As a** user,
**I want to** see how many tasks are still active,
**so that** I know at a glance how much is left to do.

**Acceptance Criteria**
- [ ] A counter displays the number of active (non-completed) tasks
- [ ] The counter updates immediately when a task is added, deleted, or toggled
- [ ] Wording: `"X task(s) remaining"`
- [ ] When count is 0: display `"All tasks completed!"`
- [ ] Counter is placed in the header, above the filter bar

**Technical Notes**
- Implemented as a `computed()` signal derived from the tasks array
- No manual subscription or change detection needed

---

### F07 — Filter Tasks

**As a** user,
**I want to** filter tasks by status and have that filter persist across reloads,
**so that** I can focus on what is relevant and resume where I left off.

**Acceptance Criteria**
- [ ] Three filter options: **All**, **Active**, **Completed**
- [ ] Default filter is **All**
- [ ] The active filter is visually highlighted (pill style)
- [ ] Switching filters updates the list instantly
- [ ] The active filter is reflected in the URL (e.g. `?filter=active`)
- [ ] Reloading the page or sharing the URL restores the same filter
- [ ] The counter always reflects **all tasks** regardless of the active filter

**Components:** `task-filter`

**Technical Notes**
- Filter state stored in a `signal()` in the store, initialized from the URL query param on app load
- Filtered list exposed as a `computed()` signal
- URL updated via Angular `Router` on filter change (no full navigation, query params only)

---

### F08 — Persistence via SQLite Backend

**As a** user,
**I want to** find my tasks after closing and reopening the app,
**so that** my data is never lost.

**Acceptance Criteria**
- [ ] Tasks are stored in a SQLite database on the backend
- [ ] All CRUD operations go through the REST API
- [ ] Data survives server restarts
- [ ] If the API is unreachable, a clear error message is shown (no silent failure)
- [ ] The database schema includes a `version` field for future migrations

**Technical Notes**
- Backend: .NET Minimal API + `Microsoft.Data.Sqlite`
- SQLite file: `backend/tasks.db`
- Schema versioning handled via a `meta` table: `{ key: 'schema_version', value: '1' }`

---

## 7. Non-Functional Requirements

| Requirement | Target |
|---|---|
| Browser support | Latest Chrome, Firefox, Edge |
| Accessibility | WCAG 2.1 AA — keyboard navigation, ARIA labels |
| Performance | Initial load < 200 KB (gzipped Angular bundle) |
| Test coverage | ≥ 80% on frontend store and computed signals — no backend tests (CRUD-only API) |

---

## 8. CI / CD

GitHub Actions workflow triggered on push to any branch:
1. `npm ci` → `ng build` → `ng test --watch=false` (frontend)
2. `dotnet restore` → `dotnet build` (backend — no unit tests, CRUD-only API)

Docker Compose spins up both services locally with a single `docker compose up`.

---

## 9. Success Criteria

- [ ] `npm install` + `ng serve` starts the frontend
- [ ] `dotnet run` starts the backend
- [ ] `ng test` passes all unit tests with ≥ 80% coverage on signals and store
- [ ] All 8 features are implemented and working
- [ ] Filter state persists across page refresh via URL
- [ ] Tasks persist across server restarts via SQLite
- [ ] GitHub Actions pipeline is green
- [ ] README is self-explanatory to a technical recruiter visiting the repo cold

---

## 10. Out of Scope

- Authentication
- Multi-user support
- Mobile-specific UI
- Due dates or task priorities
- Manual task reordering
- Bulk complete / bulk delete

---

## 11. UAT Scenarios

### UAT-E — End-to-End User Journey

| # | Scenario | Steps | Expected Result |
|---|---|---|---|
| E01 | Full task lifecycle | Start from empty list → add 3 tasks → complete 1 → delete 1 → edit the last one | List reflects all changes in real time, counter stays accurate |
| E02 | State survives reload | Add 2 tasks, complete 1, set filter to Active, reload page | Filter is restored from URL, tasks are loaded from backend, counter is correct |
| E03 | Fresh Docker startup | Clone repo, run `docker compose up`, open app | Both services start cleanly, app loads with an empty task list |

---

### UAT-C — Cross-Feature Scenarios

| # | Scenario | Steps | Expected Result |
|---|---|---|---|
| C01 | Add then filter | Add a task → switch filter to Active | New task appears in Active filter |
| C02 | Complete then filter | Complete a task → switch filter to Completed | Task appears in Completed, disappears from Active, counter decrements |
| C03 | Counter accuracy across filters | Add 3 tasks, complete 1, switch between All / Active / Completed | Counter always shows 2 remaining regardless of active filter |
| C04 | Edit then reload | Edit a task title → reload page | Updated title persists (stored in SQLite) |
| C05 | Delete then filter | Add 2 tasks → delete 1 → switch to All | Only 1 task remains across all filters |
| C06 | Filter URL sharing | Set filter to Completed → copy URL → open in new tab | New tab opens with Completed filter active |

---

### UAT-N — Negative / Error Scenarios

| # | Scenario | Steps | Expected Result |
|---|---|---|---|
| N01 | Empty title on Add | Click submit with empty input | Form blocked, validation error shown, no task created |
| N02 | Empty title on Edit | Click task title, clear the input, press Enter | Save blocked, validation error shown, original title restored |
| N03 | Title at max length | Type exactly 200 characters, submit | Task created successfully |
| N04 | Title over max length | Type 201+ characters | Input blocked at 200, character counter turns red |
| N05 | API unreachable | Stop the backend, try to add a task | Clear error message shown, app does not crash silently |
| N06 | API recovers | Restart backend after N05 | App resumes normal operation without requiring a full reload |

---

### UAT-NFR — Non-Functional Requirements

| # | Scenario | Steps | Expected Result |
|---|---|---|---|
| NFR01 | Cross-browser — Chrome | Run full E01 journey on Chrome | All features work correctly |
| NFR02 | Cross-browser — Firefox | Run full E01 journey on Firefox | All features work correctly |
| NFR03 | Cross-browser — Edge | Run full E01 journey on Edge | All features work correctly |
| NFR04 | Keyboard navigation | Tab through the entire app without using a mouse | All interactive elements reachable and operable via keyboard |
| NFR05 | ARIA / screen reader | Run a screen reader (e.g. NVDA) on the task list | Tasks, buttons, and state changes are announced correctly |
| NFR06 | Bundle size | Run `ng build --configuration production`, check output | Gzipped main bundle ≤ 200 KB |
| NFR07 | CI pipeline | Push to any branch | GitHub Actions runs frontend build + tests, backend build — all green |

---

## 12. Open Questions

All open questions resolved — see Decision Log.

---

## 13. Technical Specifications

### 12.1 Prerequisites

| Tool | Minimum version |
|---|---|
| Node.js | 20 LTS |
| Angular CLI | 19+ |
| .NET SDK | 8.0 |
| Docker | 24+ |
| Docker Compose | 2.0+ |

---

### 12.2 Environment Configuration

**Frontend** — `src/environments/environment.ts`
```ts
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5000'
};
```

**Frontend (Docker)** — `src/environments/environment.production.ts`
```ts
export const environment = {
  production: true,
  apiUrl: 'http://backend:5000'
};
```

**Backend** — `.env` (git-ignored)
```
ASPNETCORE_URLS=http://+:5000
ASPNETCORE_ENVIRONMENT=Development
```

---

### 12.3 CORS

The backend allows all origins (`*`) for simplicity. Configured in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()));

app.UseCors();
```

---

### 12.4 Database Initialisation

EF Core manages the SQLite schema. Migrations run automatically on startup:

```csharp
using var scope = app.Services.CreateScope();
var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
db.Database.Migrate();
```

Migration files are committed to source control. `tasks.db` is git-ignored.

---

### 12.5 UUID Generation

UUIDs are generated by the backend on `POST /api/tasks`:

```csharp
var task = new Task { Id = Guid.NewGuid().ToString(), ... };
```

The frontend never generates IDs.

---

### 12.6 Validation

Validation is enforced at **both layers** — frontend for UX speed, backend as source of truth.

| Rule | Frontend | Backend |
|---|---|---|
| Title required | Reactive form validator | Model annotation `[Required]` |
| Title max 200 chars | `maxlength` validator + counter | `[MaxLength(200)]` |

---

### 12.7 Error Format

All API errors follow **RFC 7807 Problem Details** (native .NET support via `Results.Problem()`):

```json
{
  "status": 400,
  "title": "Validation failed",
  "detail": "Title cannot be empty.",
  "instance": "/api/tasks"
}
```

Angular reads the `detail` field to display the user-facing error message. No custom parsing needed.

---

### 12.8 Docker Compose

```yaml
services:
  backend:
    build: ./backend
    ports:
      - "5000:5000"
    volumes:
      - db-data:/app/data
    env_file:
      - .env
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:5000/health"]
      interval: 10s
      timeout: 5s
      retries: 3
    restart: unless-stopped

  frontend:
    build: ./src
    ports:
      - "4200:80"
    depends_on:
      backend:
        condition: service_healthy
    restart: unless-stopped

volumes:
  db-data:
```

The backend exposes a `GET /health` endpoint returning `200 OK` — used by Docker health check and CI smoke tests.

---

### 12.9 Dev Setup (without Docker)

```bash
# Backend
cd backend
dotnet restore
dotnet ef database update
dotnet run

# Frontend (separate terminal)
cd src
npm install
ng serve
```

---

## 14. Signal Learning Exercises

This project is intentionally designed as a hands-on exercise for Angular Signals and the newest Angular 19+ APIs. Each concept below maps to a concrete implementation point in the project.

### Core Signals API

| Concept | Exercise | Where |
|---|---|---|
| `signal()` | Declare the tasks array and active filter as writable signals | `task.store.ts` |
| `computed()` | Derive the filtered task list and remaining count — never compute inline | `task.store.ts` |
| `effect()` | Trigger seed check and sync store after HTTP responses | `task.store.ts` |
| Mutating signals | Use `.set()` for full replacement and `.update()` for derived updates — understand when each applies | `task.store.ts` |

### Angular 19+ Features to explore

| Feature | Exercise | Where |
|---|---|---|
| `input()` signal-based inputs | Replace `@Input()` with `input()` on all child components | `task-item`, `task-form` |
| `output()` signal-based outputs | Replace `@Output()` / `EventEmitter` with `output()` | `task-item`, `task-form` |
| `viewChild()` / `viewChildren()` | Use `viewChild()` to grab the title input ref in inline edit | `task-item` |
| `linkedSignal()` | Use `linkedSignal()` to derive the edit input value from the task title | `task-item` |
| `resource()` / `httpResource()` | Load tasks on init using the new `httpResource()` API instead of a manual HTTP call | `task.store.ts` |
| `@if` / `@for` / `@switch` | Use new built-in control flow syntax — no `*ngIf` or `*ngFor` | All templates |
| `@defer` | Defer the task list rendering until data is ready | `app.component.ts` |
| Signal-based forms | Explore `FormField` signal integration for the add/edit form | `task-form` |

### Learning Rules

- Use the new APIs directly — no `@Input()`, no `*ngFor`, no `BehaviorSubject`. You already know the old way
- For each `computed()`, be able to articulate why it is derived and not a `signal()` — if you can't explain it, reconsider the design
- For each `effect()`, be able to articulate what side effect it manages and why it cannot be a `computed()`
- If a new API feels awkward, read the Angular RFC or changelog for that feature before reaching for the familiar pattern

---

## 15. Implementation Order

### Phase 1 — Foundation

| Order | Feature | Rationale |
|---|---|---|
| 1 | F08 — SQLite backend | Everything depends on the API. Build and verify all endpoints with a REST client before touching Angular |

### Phase 2 — Core Angular shell

| Order | Feature | Rationale |
|---|---|---|
| 2 | F01 — Task list | Sets up the Angular project, store, service, and base component tree. All other features plug into this |
| 3 | F02 — Add a task | First write operation — validates the full frontend → API → store → UI loop |

### Phase 3 — Task operations

| Order | Feature | Rationale |
|---|---|---|
| 4 | F05 — Complete a task | Simplest mutation (checkbox toggle), good warm-up before the more complex edit |
| 5 | F03 — Inline edit | More complex UX (keyboard handling, position preservation) |
| 6 | F04 — Delete a task | Straightforward, but depends on the task item component being stable from F03/F05 |

### Phase 4 — Reactive layer

| Order | Feature | Rationale |
|---|---|---|
| 7 | F06 — Reactive counter | Pure `computed()` signal, zero HTTP — fast to implement once the store is solid |
| 8 | F07 — Filter + URL sync | Builds on the store and counter — last because it touches routing |

---

## 16. Branching Strategy


### Rules

- One branch per feature — no mixing of features in a single branch
- Branch naming: `feat/F0X-short-description` (kebab-case, lowercase)
- Branch off `main` for every feature
- Merge via Pull Request on GitHub — no direct push to `main`

### Examples

```
feat/F01-task-list
feat/F02-add-task
feat/F03-inline-edit
feat/F04-delete-task
feat/F05-complete-task
feat/F06-reactive-counter
feat/F07-filter-url-sync
feat/F08-sqlite-backend
```

### Branch lifecycle

1. Create branch from `main`: `git checkout -b feat/F0X-description`
2. Develop and commit atomically (see Commit Rules)
3. Open a Pull Request on GitHub targeting `main`
4. Merge once CI is green

---

## 17. Commit Rules

### Convention

This project follows [Conventional Commits](https://www.conventionalcommits.org/).

```
<type>: <subject> [F0X]

[optional body]
```

### Types

| Type | When to use |
|---|---|
| `feat` | A new feature or acceptance criterion implemented |
| `fix` | A bug fix |
| `docs` | Documentation only (PRD, README, comments) |
| `test` | Adding or updating tests |
| `refactor` | Code change that neither fixes a bug nor adds a feature |
| `chore` | Build, CI, config, tooling |
| `style` | Formatting, whitespace — no logic change |

### Rules

- **Subject line**: max 72 characters, sentence case, no trailing period
- **Feature reference**: every commit related to a feature must end with its ID in brackets — e.g. `[F01]`, `[F03]`
- **Atomic commits**: one commit per development unit — a single component, a single endpoint, a single test suite. Never bundle unrelated changes in one commit
- **Body**: optional, use it to explain *why*, not *what*. The subject line covers the what
- **No scope prefix** — `feat(frontend):` style scopes are not used; the feature ID `[FXX]` provides sufficient context

### Examples

```
feat: add task list component with empty state [F01]
feat: add inline edit with Enter/Escape handling [F03]
fix: block empty title on save in edit mode [F03]
feat: split PATCH into title and complete endpoints [F03][F05]
test: add computed signal tests for filtered task list [F07]
chore: add docker-compose.yml for local dev setup
docs: update PRD with UAT scenarios
```

### What not to do

```
feat: WIP
fix: stuff
feat: add everything
feat(frontend): add component and fix bug and update tests
```

---

## 18. README Specification

The README is a portfolio deliverable. A technical recruiter visiting the repo should understand the project value within 30 seconds.

### Required sections (in order)

| Section | Content |
|---|---|
| Title + badges | Project name, CI status badge, Angular version badge, .NET version badge |
| One-liner | One sentence describing what the project is and why it exists |
| Demo screenshot | A single screenshot of the running app (task list with some tasks) |
| Tech stack | Angular 19+, Signals, .NET Minimal API, SQLite, Docker — with brief rationale |
| Key concepts | 3–4 bullet points on what the project demonstrates (Signals, computed(), inline edit, URL sync) |
| Getting started | Two paths: Docker (`docker compose up`) and manual (backend then frontend) |
| Running tests | `ng test` command and what is tested |
| Project structure | Condensed folder tree with one-line descriptions |
| Architecture decisions | Link to the Decision Log in the PRD |

### Badges

```markdown
![CI](https://github.com/<user>/task-manager/actions/workflows/ci.yml/badge.svg)
![Angular](https://img.shields.io/badge/Angular-19+-DD0031?logo=angular)
![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)
![SQLite](https://img.shields.io/badge/SQLite-003B57?logo=sqlite)
```

### Rules

- No wall of text — short paragraphs, bullet points, code blocks
- Getting started must work on a fresh machine with only Docker installed
- README is written in English

---

## 19. Seed Data

The app ships with sample tasks on first run to demonstrate all features immediately — relevant for portfolio demo purposes.

### Seed content

| Title | Status |
|---|---|
| Setup Angular project with Signals | Completed |
| Build the task store with signal() | Completed |
| Add inline edit with keyboard support | Active |
| Implement filter with URL sync | Active |
| Write unit tests for computed signals | Active |

### Rules

- Seed runs only if the `tasks` table is empty on backend startup
- Seed is idempotent — running it twice does not create duplicates
- `created_at` values are staggered (1 minute apart) to produce a meaningful creation-order sort

### Implementation

```csharp
if (!db.Tasks.Any())
{
    db.Tasks.AddRange(seedTasks);
    db.SaveChanges();
}
```

---

## 20. Decision Log

| Decision | Chosen | Rejected | Reason |
|---|---|---|---|
| UI framework | Angular Material | Tailwind | Faster to set up, built-in a11y support |
| Testing | Vitest | Jest | Native ESM, faster cold start |
| Persistence | SQLite + REST backend | localStorage / in-memory | Real persistence, enables SQL querying, better portfolio value |
| Backend | .NET Minimal API | Node.js/Express | Matches author's C#/.NET expertise, minimal footprint |
| Deploy | Docker Compose | Vercel+Railway / Azure | Single command, self-contained, no cloud account needed |
| State | Custom Signals store | NgRx | Simpler, showcases Signals without abstraction overhead |
| Filter UI | Pill buttons | Tabs | Tabs merge F05 and F07 — pill buttons keep concerns separate |
| Edit UX | Inline editing | Modal / separate form | Less disruptive, stays in context of the list |
| Delete confirmation | None | Dialog | Low stakes single-item action, no bulk delete risk |
| Completed task sorting | Stay in place (v1), sort to bottom deferred to v2 | Always sort | Avoids layout jump in v1, leaves room for v2 improvement |
