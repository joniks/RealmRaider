# Realm Raiders — Team Workflow

## Purpose

We use four Codex conversations as one small game team. Speed is useful only when every agent has a non-overlapping responsibility and a defined integration point.

The shared checkout is the **integration workspace**. It is never edited concurrently by multiple implementation agents.

## Roles and ownership

| Conversation | Role | Write scope | Must not do |
| --- | --- | --- | --- |
| Architect / Project Manager | Product owner, architecture, integration | `Docs/`, `AGENTS.md`, task definitions, acceptance records; emergency integration only | Implement a feature in parallel with Core developer, run redundant full suites, import assets without provenance |
| Core developer | One active vertical-slice implementation | Main checkout: only files explicitly reserved in the active task; authoritative gameplay, save, bootstraps, shared UI when task requires it | Work on a second feature before the first is accepted; edit Docs ownership/process files; commit/push |
| Module developer | Isolated content/module lane | Its own Git worktree and branch; new self-contained module/assets/docs only | Edit the main checkout, shared gameplay/UI files, project settings, or Unity scenes; integrate its own work |
| Reviewer / QA | Independent review and verification | Read-only by default; report findings in the conversation | Edit tracked files, run Unity while Core developer is using Unity, repeat green full suites, make a commit |

## Worktree model

```text
main checkout — Core developer only
  └─ accepted work → user commit

module worktree — Module developer only
  └─ isolated diff/branch → Architect review → explicit user approval to integrate

Reviewer / QA — read-only against the named commit or frozen diff
Architect — documentation, task ownership, review and integration decisions
```

- A worktree task starts from a named committed base revision.
- A module task must create new, namespaced files whenever possible, for example `Assets/Game/Modules/<Feature>/...`.
- If a module needs an existing shared file changed, it stops and asks Architect to schedule integration after Core developer is finished.
- Only the user performs the final commit/push unless they explicitly delegate it.

## Task lease

Every active task must state these five fields at the top of `Docs/NEXT_JOB.md` or in its assignment message:

```text
Owner: Core developer | Module developer | Reviewer / QA
Workspace: main checkout | named worktree | read-only
Base commit: <short hash>
Reserved files/folders: <exact paths>
Handoff condition: <what must be true before another role may touch them>
```

One owner at a time may write a reserved path. A task is released only after Architect accepts the handoff or explicitly reassigns it.

## Operating sequence

1. Architect defines the smallest valuable player problem and records its scope/non-goals in `Docs/NEXT_JOB.md`.
2. Core developer implements one vertical slice in the main checkout, with focused tests.
3. Core developer freezes its diff, reports changed files and one final EditMode/PlayMode total.
4. Reviewer / QA inspects the frozen diff and test evidence; it reports issues but never fixes them directly.
5. Architect accepts or returns the task, updates `Docs/DONE_JOB.md` and `Docs/PROTOTYPE_STATUS.md`, then the user commits.
6. Module work is reviewed and integrated only between committed Core tasks.

## Verification discipline

- During implementation: one focused check.
- After final code/test changes: full EditMode once and PlayMode once.
- QA reads the evidence and only reruns a suite if it found a concrete issue or the diff changed afterwards.
- Manual device checks belong to the user; reports must say exactly what was observed and never invent a smoke result.

## Unity and asset safety

- Never have two agents actively operate the same Unity project checkout.
- Reviewer does not run Unity Test Runner while Core developer has Unity open.
- Module developer uses a separate worktree and does not touch shared `Library`, `ProjectSettings`, scenes, or bootstraps.
- Third-party assets remain research-only until licence, source, import plan, and provenance record are accepted. Original generated art is a mood reference until converted into an explicitly reviewed game asset.

## Communication format

Implementation report, maximum six lines:

```text
Changed: <files>
Focused: <result>
Final EditMode / PlayMode: <totals>
Manual smoke: <observed result or not run>
Review concern: <none or one concrete item>
Commit/push: not performed
```

The Architect is the single source of truth for task priority, acceptance, shared-contract changes, and when a worktree may be integrated.
