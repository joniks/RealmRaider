# Realm Raiders — Team Workflow

## Purpose

We use four Codex conversations as one small game team. Speed is useful only when every agent has a non-overlapping responsibility and a defined integration point.

The shared checkout is the **integration workspace**. It is never edited concurrently by multiple implementation agents.

## Roles and ownership

| Conversation | Role | Write scope | Must not do |
| --- | --- | --- | --- |
| Architect / Product Lead | Product owner, game design, architecture, integration, committer | `Docs/`, `AGENTS.md`, priorities, task definitions, acceptance records, and accepted commits in the main and Modules repositories | Implement a feature in parallel with Core developer, run redundant full suites, import assets without provenance, push |
| Core developer | One active vertical-slice implementation | Main checkout: only files explicitly reserved in the active task; authoritative gameplay, save, bootstraps, shared UI when task requires it | Work on a second feature before the first is accepted; launch, close, restart, or control Unity; run Unity tests/manual smoke; edit Docs ownership/process files; commit/push |
| Module Developer / Technical Art | Isolated reusable systems and content-production lane | Its own Git worktree and branch; package code, validators, catalogues, editor-independent conversion/import tooling, visual recipes and accepted assets/provenance | Edit the main checkout or shared gameplay/UI files; control Unity; assign integration directly to Core; invent licences/sources; integrate its own work; commit/push |
| Reviewer / QA + Build | Independent review, verification and release-evidence owner | Read-only source review; acceptance checklists; the only role permitted to launch, close, restart, or control Unity for compilation, Test Runner, team manual smoke and export verification | Edit tracked files, inspect a moving implementation as if frozen, repeat green full suites, commit/push |

## Worktree model

```text
main checkout — Core implementation, then QA verification (never simultaneously)
  └─ QA accepted work → Architect commit → user push

module/technical-art worktree — Module Developer / Technical Art only
  ├─ isolated package/tool/data diff → Architect review
  └─ frozen module candidate → QA review when Unity integration is required

Reviewer / QA + Build — read-only source review plus serial Unity verification of a frozen diff
Architect / Product Lead — product/design priority, task ownership, review decisions, documentation and commits
```

- A worktree task starts from a named committed base revision.
- A module task must create new, namespaced files under `Packages/com.realmraiders.<feature>/` whenever possible.
- Module work should be roughly 70–80% implementation and verification support. Documentation is a supporting deliverable, not a substitute for package code, tooling, recipes or asset preparation.
- Product/game-design briefs belong to Architect. Module Developer may write a short technical note only when a source, licence, rig, budget or package contract must be frozen before implementation.
- If a module task needs an existing main-checkout file changed, it stops and asks Architect to schedule a separate Core integration task after the current Core lease is released.
- Architect commits accepted work in both repositories. The user alone pushes those commits.

## Parallel capacity and ready queue

- Work in progress is capped at two implementation lanes: one Core candidate in the main checkout and one Module/Technical-Art candidate in an isolated worktree.
- QA is a shared gate, not a third implementation lane. While candidates are moving, QA may prepare acceptance checks from the frozen task contract, but it touches Unity only after a candidate is explicitly frozen and Architect clears its static preflight.
- Architect keeps one bounded follow-up ready for each implementation lane before the current task finishes. A role may wait for a dependency, but it must not silently invent unrelated work.
- Core-to-QA fixes keep the same lease and scope. A new feature never enters the main checkout until the previous candidate is accepted and committed.
- Module work reaches the main game only through a later named Core integration task. Installing a package, changing a host adapter or consuming a recipe is integration, not part of the isolated module lease.

## Task lease

Every active task must state these five fields at the top of `Docs/NEXT_JOB.md` or in its assignment message:

```text
Owner: Core developer | Module Developer / Technical Art | Reviewer / QA + Build
Workspace: main checkout | named worktree | read-only
Base commit: <short hash>
Reserved files/folders: <exact paths>
Handoff condition: <what must be true before another role may touch them>
```

One owner at a time may write a reserved path. A task is released only after Architect accepts the handoff or explicitly reassigns it.

## Operating sequence

1. Architect defines product/game-design priority and keeps one ready, bounded task for each implementation lane.
2. Module Developer / Technical Art implements one reusable package/tool/data slice in an isolated worktree and freezes it for Architect review.
3. Architect records the current Core scope/non-goals in `Docs/NEXT_JOB.md`.
4. Core developer implements one vertical slice in the main checkout without launching or controlling Unity. It freezes its diff and sends the changed-path list plus intended behaviors directly to Reviewer / QA + Build and Architect.
5. Architect runs a read-only static preflight: reserved paths, scope/non-goals, acceptance coverage and `git diff --check`. QA does not start Unity until Architect explicitly clears the candidate.
6. Reviewer / QA + Build reviews the cleared frozen diff, then alone launches and controls Unity for compilation and focused verification. It reports concrete defects directly to Core developer.
7. Core developer fixes only the reported defects without touching Unity, freezes the checkout again, and returns it through Architect preflight to QA. This loop continues until QA has no blocker.
8. Reviewer / QA + Build runs the final full EditMode and PlayMode suites exactly once against the final frozen diff, then accepts or rejects it.
9. Architect records acceptance in `Docs/DONE_JOB.md` and `Docs/PROTOTYPE_STATUS.md`, commits accepted main/module work, and tells the user what to push.
10. Accepted module work is integrated only through a later named Core task between committed Core candidates.

## Verification discipline

- Core developer does not launch, close, restart, or control Unity, and does not run tests. It communicates test impact and expected outcomes to QA with each frozen handoff.
- QA runs a focused check after a new frozen candidate or concrete fix.
- QA runs full EditMode once and full PlayMode once only after the final code/test change. It reruns a green suite only if the candidate changes afterwards or it found a concrete reason.
- Manual device checks belong to the user; reports must say exactly what was observed and never invent a smoke result.

## Unity and asset safety

- Only Reviewer / QA + Build may launch, close, restart, or control Unity. Core and Module Developer / Technical Art never use Unity UI or Unity processes.
- Reviewer / QA + Build owns all Unity compilation, Test Runner/manual-smoke execution, export verification and test evidence.
- Module Developer / Technical Art uses a separate worktree and does not touch shared `Library`, `ProjectSettings`, scenes or bootstraps.
- Third-party assets remain research-only until licence, source, import plan, and provenance record are accepted. Original generated art is a mood reference until converted into an explicitly reviewed game asset.

## Communication format

Module Developer / Technical Art → Architect handoff, maximum six lines:

```text
Changed: <isolated package/tool/data files>
Contract: <public API/data behavior and explicit non-goals>
Static checks: <results; no invented Unity evidence>
Integration need: <none or exact future main-checkout seam>
Risks: <performance, licence, compatibility, or none>
Unity/commit/push: not performed
```

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

## Completion delivery

- Ending a task or leaving files in a worktree is not a handoff. The owner sends the relevant six-line report immediately after its last check.
- Core sends every frozen candidate to both QA and Architect. QA sends every acceptance or rejection to both Core and Architect. Module Developer / Technical Art sends every isolated-module handoff to Architect.
- If direct cross-task delivery is unavailable, the owner puts the complete report in its own final response; Architect retrieves it from task status. A generic “done” message is not sufficient.
- A task is not accepted from a silent, empty, queued or interrupted completion. Architect checks its status, reads the owned artifact when necessary, and asks the owner to resend the complete report before integration.
- Architect acknowledges each valid handoff by accepting it, returning one concrete correction, or naming the next owner. This acknowledgement releases the task lease.
- Reports contain evidence already produced; agents do not rerun green full suites or repeat work merely to generate another message.

## Thread health

- Rename roles when their responsibility changes; a stale title is an operational defect because it invites the wrong work.
- After roughly five accepted passes, or after two missed/empty handoffs, Architect recommends replacing that role conversation with a fresh one at the next clean commit boundary.
- A replacement conversation receives the shared files and one current lease, not a transcript-sized custom prompt. Repository context remains the source of truth.

The Architect is the single source of truth for task priority, acceptance, shared-contract changes, and when a worktree may be integrated.
