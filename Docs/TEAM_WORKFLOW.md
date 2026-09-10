# Realm Raiders — Team Workflow

## Purpose

We use four active Codex conversations as one small game team. Speed is useful only when every agent has a non-overlapping responsibility and a defined integration point.

Reviewer / QA + Build is active. It is the only role that controls Unity or produces Unity test, smoke and build evidence. Architect owns static preflight and acceptance; Core and Module remain Unity-free.

The shared checkout is the **integration workspace**. It is never edited concurrently by multiple implementation agents.

## Roles and ownership

| Conversation | Role | Write scope | Must not do |
| --- | --- | --- | --- |
| Architect / Product Lead | Product owner, game design, architecture, integration, static preflight, acceptance and committer | `Docs/`, `AGENTS.md`, priorities, task definitions, acceptance records and accepted commits in the main and Modules repositories | Implement a feature in parallel with Core developer, control Unity, run tests/builds, import assets without provenance, push |
| Core developer | One active vertical-slice implementation | Main checkout: only files explicitly reserved in the active task; authoritative gameplay, save, bootstraps, shared UI when task requires it | Work on a second feature before the first is accepted; launch, close, restart, or control Unity; run Unity tests/manual smoke; edit Docs ownership/process files; commit/push |
| Module Developer / Technical Art | Isolated reusable systems and content-production lane | Its own Git worktree and branch; package code, validators, catalogues, editor-independent conversion/import tooling, visual recipes and accepted assets/provenance | Edit the main checkout or shared gameplay/UI files; control Unity; assign integration directly to Core; invent licences/sources; integrate its own work; commit/push |
| Reviewer / QA + Build | Frozen-candidate Unity compilation, focused/final suites, observed manual smoke and platform export evidence | The single healthy Unity GUI instance only after Architect assigns a named QA lease | Edit implementation/docs, restart a healthy Editor, use CLI while GUI is open, alter Test Runner layout, commit/push |

## Worktree model

```text
main checkout — Core implementation, Architect static review, then QA Unity verification
  └─ Architect-accepted work → Architect commit → user push

module/technical-art worktree — Module Developer / Technical Art only
  ├─ isolated package/tool/data diff → Architect review
  └─ frozen module candidate → Architect review when Unity integration is required

Architect / Product Lead — product/design priority, task ownership, frozen-diff static review, acceptance, documentation and commits

Reviewer / QA + Build — single Unity owner, frozen-candidate tests, observed smoke and build/export evidence
```

- A worktree task starts from a named committed base revision.
- A module task must create new, namespaced files under `Packages/com.realmraiders.<feature>/` whenever possible.
- Module work should be roughly 70–80% implementation and verification support. Documentation is a supporting deliverable, not a substitute for package code, tooling, recipes or asset preparation.
- Product/game-design briefs belong to Architect. Module Developer may write a short technical note only when a source, licence, rig, budget or package contract must be frozen before implementation.
- If a module task needs an existing main-checkout file changed, it stops and asks Architect to schedule a separate Core integration task after the current Core lease is released.
- Architect commits accepted work in both repositories. The user alone pushes those commits.

## Parallel capacity and ready queue

- Work in progress is capped at two implementation lanes: one Core candidate in the main checkout and one Module/Technical-Art candidate in an isolated worktree.
- QA is a verification lane, not an implementation lane. It touches Unity only after Core freezes a candidate and Architect completes static preflight.
- Architect keeps one bounded follow-up ready for each implementation lane before the current task finishes. A role may wait for a dependency, but it must not silently invent unrelated work.
- `Docs/CORE_READY_QUEUE.md` is the authoritative continuation queue for the main lane. A frozen Core handoff immediately moves Core to read-only preparation of the next queued gate; it never authorizes overlapping source edits.
- `Docs/MODULE_READY_QUEUE.md` is the authoritative queue for the Module/Technical-Art lane. When a Modules candidate is accepted or rejected, Architect immediately advances exactly one eligible queue item or records its concrete dependency; an idle Module role is never left to invent work.
- The recovery heartbeat checks that queue every run. It may dispatch its single `Now` item only when no other Modules lease is active, its exact package path is cleanly reserved, and the task can finish without main-checkout or Unity access.
- Core-to-Architect-to-QA fixes keep the same lease and scope. A new feature never enters the main checkout until the previous candidate is accepted and committed.
- Exception: Architect may explicitly place one small user-requested follow-up into the same verification batch before final suites when its lease is disjoint, the earlier files remain frozen, and the final suites run only after the last change. This exception is recorded in both ready queue and active lease.
- Module work reaches the main game only through a later named Core integration task. Installing a package, changing a host adapter or consuming a recipe is integration, not part of the isolated module lease.

## Task lease

Every active task must state these five fields at the top of `Docs/NEXT_JOB.md` or in its assignment message:

```text
Owner: Core developer | Module Developer / Technical Art | Reviewer / QA + Build | Architect / Product Lead
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
4. Core developer implements one vertical slice in the main checkout without launching or controlling Unity. It freezes its diff and sends the changed-path list plus intended behaviors directly to Architect.
   After sending the handoff, Core immediately reads the next queued gate and may prepare a read-only scope/risk note; it does not modify new source until Architect activates that lease.
5. Architect runs static preflight: reserved paths, scope/non-goals, acceptance coverage and `git diff --check`. Only then does Architect assign one named frozen-candidate QA lease.
6. QA performs focused Unity verification and reports evidence or a concrete defect to Architect; Architect returns that defect directly to Core developer.
7. Core developer fixes only the reported defect without touching Unity, freezes the checkout again, and returns it through Architect preflight. This loop continues until the candidate has no blocker.
8. QA runs the final full EditMode and PlayMode suites exactly once against the final frozen diff and reports exact totals; Architect then accepts or rejects it.
9. Architect records acceptance in `Docs/DONE_JOB.md` and `Docs/PROTOTYPE_STATUS.md`, commits accepted main/module work, and tells the user what to push.
10. Accepted module work is integrated only through a later named Core task between committed Core candidates.
11. A local user push is asynchronous publication, not a team gate. Work continues from Architect's accepted local commit without waiting for the remote to change.

## Verification discipline

- Core developer does not launch, close, restart, or control Unity, and does not run tests. It communicates test impact and expected outcomes to Architect with each frozen handoff.
- QA uses the Unity Editor Test Runner GUI for tests and the existing **Realm Raiders → Build** menu for platform export while a healthy Editor is open. It does not close or restart that Editor merely to export. The repository CLI wrapper is a fallback only when the user-approved boundary already has Unity fully closed, Unity Hub remains open and signed in, and no competing Unity process exists.
- QA runs a focused check after Architect assigns a new frozen candidate or concrete fix.
- QA runs full EditMode once and full PlayMode once only after the final code/test change. A green suite is rerun only if the candidate changes afterwards or a concrete reason exists.
- Manual device checks belong to the user; reports must say exactly what was observed and never invent a smoke result.

### Test Runner recovery ladder

When QA uses the Unity Test Runner and it appears stale, filtered, or its result pane is incomplete, use this order:

1. Clear the search/filter, select the intended mode, and confirm the visible test count before starting anything.
2. After every Test Runner click, wait for the run to settle, then take a fresh UI state/result snapshot. Do not issue another click while compilation, a suite, scene loading, layout refresh, or result saving may still be in progress.
3. Before the next gate, confirm the preceding result, visible test count and Console state. A saved result or a quiet UI is not itself evidence that the next action is safe.
4. If the UI evidence is incomplete, wait once more and refresh the visible state; then report the exact limitation. A user-run green suite may be recorded as user evidence, but its total must remain unspecified unless QA actually saw it.
5. Run only the gate required by the current candidate; do not restart a broad suite merely to recreate evidence.

An ordinary Unity restart is **not** a Test Runner or export step. QA may launch, close or restart Unity only when the user explicitly asks, or after an undeniable editor crash/hang and confirmation. CLI is not a Test Runner shortcut: the 2026-09-10 licensed `-runTests` pilot loaded the project but never started its focused test, so tests remain GUI-owned. Headless CLI remains an optional closed-Editor export fallback. Never use blind coordinate clicks, `Reimport All`, Library deletion, or a broad cache reset. In this Unity version, a Project-window action labelled **Assets → Reimport** may trigger the broad reimport warning; treat it as `Reimport All` and cancel it.

## Continuous, compact operating rhythm

- An authorized next step is taken without waiting for another chat message. An agent may stop only at a completed handoff or a concrete external blocker; it reports that boundary immediately.
- A completed handoff still requires a transition: continue the same lease, enter read-only next-gate preparation, or receive Architect's next lease. A generic final message must never leave an eligible `Now` task undispatched.
- Within one coordination turn of a valid handoff, Architect inspects it and chooses exactly one outcome: accept and advance, return one concrete defect, or record a precise external dependency and move the role to a non-overlapping ready task.
- Every ten-minute recovery run checks the Core and Module ready queues, resumes an idle owner with an eligible lease, and verifies that no role is waiting for a user push.
- Every role keeps a compact, thread-local checkpoint of no more than eight bullets: active lease/base, owned paths, current evidence, next safe step, exclusions and blocker. The repository documents remain the authoritative detail.
- Before any GUI `Run All`, QA opens Test Runner, clears a stale filter and verifies the visible test count. It never uses headless batchmode or blind clicks to force a test result.
- When Test Runner evidence is unreliable, QA follows the recovery ladder above and reports the first concrete boundary; it does not repeat green suites or turn a UI problem into a source-code investigation.
- If a Mac lock, UI automation limitation or missing approval blocks progress, preserve the frozen candidate, do not invent a workaround or broaden scope, and resume the already-authorized sequence as soon as the boundary clears.

## Unity and asset safety

- Only Reviewer / QA + Build may launch or control Unity for a frozen Core candidate. Architect, Core and Module Developer / Technical Art do not use Unity UI or Unity processes.
- QA owns Unity compilation, Test Runner/manual-smoke execution, export verification and test evidence.
- After every accepted Core commit, QA uses the existing Unity GUI build menu to refresh `Builds/AndroidStudio` while keeping a healthy Editor open. It records the commit SHA and confirms the export completed. Use `Tools/realmraider-unity-cli.sh export-android` only when Unity is already closed under an approved boundary.
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

Core → Architect handoff, maximum six lines:

```text
Changed: <files>
Expected: <player-visible behavior and non-goals>
Test impact: <focused tests/risks Architect should cover>
Unity: not touched — QA owns verification
Question: <none or one concrete uncertainty>
Commit/push: not performed
```

QA → Architect evidence, maximum six lines:

```text
Candidate: <lease/base/frozen diff>
Focused: <result>
Final EditMode / PlayMode: <totals or not yet run>
Manual smoke: <observed result or user-owned/not run>
Changed after final suite: yes | no
Build/export: <result or not requested>; commit/push not performed
```

## Completion delivery

- Ending a task or leaving files in a worktree is not a handoff. The owner sends the relevant six-line report immediately after its last check.
- Core sends every frozen candidate to Architect. Architect sends every acceptance or rejection directly to Core. Module Developer / Technical Art sends every isolated-module handoff to Architect.
- If direct cross-task delivery is unavailable, the owner puts the complete report in its own final response; Architect retrieves it from task status. A generic “done” message is not sufficient.
- A task is not accepted from a silent, empty, queued or interrupted completion. Architect checks its status, reads the owned artifact when necessary, and asks the owner to resend the complete report before integration.
- Architect acknowledges each valid handoff by accepting it, returning one concrete correction, or naming the next owner. This acknowledgement releases the task lease.
- Reports contain evidence already produced; agents do not rerun green full suites or repeat work merely to generate another message.

## Thread health

- Rename roles when their responsibility changes; a stale title is an operational defect because it invites the wrong work.
- After roughly five accepted passes, or after two missed/empty handoffs, Architect recommends replacing that role conversation with a fresh one at the next clean commit boundary.
- A replacement conversation receives the shared files and one current lease, not a transcript-sized custom prompt. Repository context remains the source of truth.

The Architect is the single source of truth for task priority, acceptance, shared-contract changes, and when a worktree may be integrated.
