# Realm Raiders — Core Ready Queue

This is the tracked continuation queue for the main Unity checkout. `NEXT_JOB.md` remains the full active lease; this file states what happens immediately after every intermediate result so a valid handoff cannot become silent idle time.

## Accepted — Diamond Passes 12.7–13.0

- Diamond Pass 12.7 Build-to-Defense Deployment Receipt is accepted after the user's positive manual smoke and the final automated gates.
- Diamond Pass 12.8 Camera-Relative Direct Attack Direction is accepted with deterministic legacy PlayMode gate repairs.
- Diamond Pass 12.9 Possessed-Defense Invader Awareness is accepted with factual event-driven invader intent, bounded existing awareness presentation and complete lifecycle cleanup.
- Diamond Pass 13.0 Sylvan Seam-Hardened Path Integration is accepted with byte-identical original-generated provenance, Repeat/mobile import settings and unchanged authoritative geometry/collision.
- Final Unity GUI Test Runner: EditMode `139/139`, PlayMode `79/79`, `0` failures. PlayMode XML completed 2026-09-10 06:25:24Z; scoped and complete-new-file checks are clean.
- Architect records and commits only the accepted paths; user push is asynchronous publication and does not block QA Tool 01.

## Accepted — QA Tool 01: Accessible Unity Test Gates

Owner: Core developer

Workspace: main checkout; source only, no Unity.

Base: Architect's accepted local 13.0 commit.

Reserved paths:

- `Assets/Game/Editor/RealmRaidersTestGate.cs`

Handoff: frozen scoped diff, `git diff --check`, six-line Core report; no stage, commit, push or Unity.

### Outcome

Expose two accessible Unity menu commands that start full EditMode or PlayMode runs through the installed official `TestRunnerApi`. This bypasses the Test Runner window's inaccessible internal buttons without CLI, Editor restart, window rearrangement or a second Unity process.

### Guardrails

- Add **Realm Raiders → QA → Run All EditMode Tests** and **Run All PlayMode Tests**; use public `UnityEditor.TestTools.TestRunner.Api` only.
- Each command supplies exactly one unfiltered `Filter` for its mode. It must not inspect or inherit the Test Runner search field.
- Refuse while compiling, updating, entering/inside Play Mode or while a gate launched by this tool is already active. Never start a second run or Unity process.
- Register a lightweight callback that logs factual passed/failed/skipped totals and clears the owned active state; persist only the minimum session state needed across a PlayMode domain reload.
- Do not mutate tests, Test Runner layout/filter, scenes, packages, ProjectSettings or gameplay. Do not auto-chain broad suites; Architect deliberately invokes each final gate once.
- Architect updates the QA guide after QA's direct acceptance; Core does not edit process documentation.

## Verification

Reviewer / QA + Build invoked each new menu command exactly once in the existing
healthy Editor. EditMode passed `139/139`; PlayMode passed `79/79`. PlayMode
ownership recovered across domain reload and cleared after completion. No CLI,
restart, overlapping run or Test Runner layout/filter change occurred.

## Now — Diamond Pass 13.1: Sylvan Living-Root Boundary Presentation

Core integrates the accepted MWS08 Sylvan living-root boundary albedo and accepted
MWS09 restrained normal candidate as presentation only, preserving all boundary
collider geometry, gameplay authority and mobile budgets. The matching Infernal
surface remains queued behind its documented periodicity check.
