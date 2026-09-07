# Realm Raiders — Team Workflow

## Purpose

We use four Codex conversations as one small game team. Speed is useful only when every agent has a non-overlapping responsibility and a defined integration point.

The shared checkout is the **integration workspace**. It is never edited concurrently by multiple implementation agents.

## Roles and ownership

| Conversation | Role | Write scope | Must not do |
| --- | --- | --- | --- |
| Architect / Project Manager | Product owner, architecture, integration, committer | `Docs/`, `AGENTS.md`, task definitions, acceptance records, and accepted commits in the main and Modules repositories | Implement a feature in parallel with Core developer, run redundant full suites, import assets without provenance, push |
| Core developer | One active vertical-slice implementation | Main checkout: only files explicitly reserved in the active task; authoritative gameplay, save, bootstraps, shared UI when task requires it | Work on a second feature before the first is accepted; launch, close, restart, or control Unity; run Unity tests/manual smoke; edit Docs ownership/process files; commit/push |
| Module developer | Isolated content/module lane | Its own Git worktree and branch; new self-contained module/assets/docs only | Edit the main checkout, shared gameplay/UI files, project settings, or Unity scenes; launch, close, restart, or control Unity; integrate its own work; commit/push |
| Reviewer / QA | Independent review and verification owner | Read-only source review; the only role permitted to launch, close, restart, or control Unity for compilation, Test Runner, and team manual smoke | Edit tracked files, repeat green full suites, commit/push |

## Worktree model

```text
main checkout — Core implementation, then QA verification (never simultaneously)
  └─ QA accepted work → Architect commit → user push

module worktree — Module developer only
  └─ isolated diff/branch → QA review → Architect commits Modules repo → user push

Reviewer / QA — read-only source review plus serial Unity verification of a frozen diff
Architect — task ownership, review decisions, documentation and commits
```

- A worktree task starts from a named committed base revision.
- A module task must create new, namespaced files whenever possible, for example `Assets/Game/Modules/<Feature>/...`.
- If a module needs an existing shared file changed, it stops and asks Architect to schedule integration after Core developer is finished.
- Architect commits accepted work in both repositories. The user alone pushes those commits.

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
2. Core developer implements one vertical slice in the main checkout without launching or controlling Unity. It freezes its diff and sends the changed-path list plus intended behaviors directly to Reviewer / QA.
3. Reviewer / QA reviews the frozen diff, then alone launches and controls Unity for compilation and focused verification. It reports concrete defects directly to Core developer.
4. Core developer fixes only the reported defects without touching Unity, freezes the checkout again, and returns it to QA. This loop continues until QA has no blocker.
5. Reviewer / QA runs the final full EditMode and PlayMode suites exactly once against the final frozen diff, then accepts or rejects it.
6. Architect records acceptance in `Docs/DONE_JOB.md` and `Docs/PROTOTYPE_STATUS.md`, commits accepted main/module work, and tells the user what to push.
7. Module work is reviewed and integrated only between committed Core tasks.

## Verification discipline

- Core developer does not launch, close, restart, or control Unity, and does not run tests. It communicates test impact and expected outcomes to QA with each frozen handoff.
- QA runs a focused check after a new frozen candidate or concrete fix.
- QA runs full EditMode once and full PlayMode once only after the final code/test change. It reruns a green suite only if the candidate changes afterwards or it found a concrete reason.
- Manual device checks belong to the user; reports must say exactly what was observed and never invent a smoke result.

## Unity and asset safety

- Only Reviewer / QA may launch, close, restart, or control Unity. Core and Module developer never use Unity UI or Unity processes.
- Reviewer / QA owns all Unity compilation, Test Runner/manual-smoke execution, and test evidence.
- Module developer uses a separate worktree and does not touch shared `Library`, `ProjectSettings`, scenes, or bootstraps.
- Third-party assets remain research-only until licence, source, import plan, and provenance record are accepted. Original generated art is a mood reference until converted into an explicitly reviewed game asset.

## Communication format

Core → QA handoff, maximum six lines:

```text
Changed: <files>
Expected: <player-visible behavior and non-goals>
Test impact: <focused tests/risks QA should cover>
Unity: not touched — QA owns Unity
Question: <none or one concrete uncertainty>
Commit/push: not performed
```

QA → Core / Architect report, maximum six lines:

```text
Review: accepted | rejected with <concrete blocker>
Focused: <result>
Final EditMode / PlayMode: <totals or not yet run>
Manual smoke: <observed result or user-owned/not run>
Changed after final suite: yes | no
Commit/push: not performed
```

The Architect is the single source of truth for task priority, acceptance, shared-contract changes, and when a worktree may be integrated.
