# 18.1B — Infernal Raid with Ent Hero

User-requested 2026-09-12. Status: READY after 18.1 acceptance; not implemented.
Owner: Core developer, main checkout; QA alone controls Unity.
Base: the future accepted 18.1 commit (Architect supplies it at activation).
Handoff: one frozen playable vertical slice, focused regression coverage and
static report; no commit/push/Android export.

## Player outcome

Hub has a clearly labelled `RAID INFERNAL — ENT` test entry. It opens a separate
`InfernalRaid` scene with the existing animated Guardian Ent directly controlled
from the start, using Smash, Charge and Ground Slam. Existing Infernal defense,
Sylvan raid and the canonical Sylvan journey remain available and unchanged.

This is a short authored combat trial, targeting roughly 60–90 seconds for an
engaged first run (a design target, not verified duration). Use current licensed
characters and accepted Infernal surfaces. No new art download or model purchase.

## Intended playable beats

1. Safe arrival on the existing 14×68 lane footprint: Ent at the south end,
   no active enemy/hazard in immediate strike range. Ground fit from 18.0 applies.
2. Two Hellhounds create movement pressure and a chance to land a satisfying
   multi-target Ground Slam. Keep room to dodge/charge around them.
3. One readable limited-width fire/lava hazard rewards choosing when/where to
   cross. It must be bypassable with the scaled Ent capsule; no full-width raised
   collider, unavoidable root lock, invisible trigger or permanent damage loop.
4. A Brute guards the final approach. The explicitly supplied Brute must be dead
   before the Infernal Heart capture begins; HUD explains this prerequisite.
   Use existing health/death/ability/telegraph authority, no invented boss stages.
5. A factual result offers retry of THIS scene and Hub return. One more attempt
   should be immediate; do not route retry to Sylvan or start the Sylvan journey.

Module's read-only audit has supplied the geometry/AI facts. First-trial layout:

| Item | X/Z | Configuration |
| --- | --- | --- |
| Direct Ent | 0 / -30 | Scale 1.45; capsule radius 1.16m; grounded support fit |
| Hellhound A | -3.6 / -13 | Existing Hellhound archetype/Leap |
| Hellhound B | 3.6 / -7 | Existing Hellhound archetype/Leap; can join pressure as hero advances |
| One Flame Trap | 0 / 2 | Automatic explicitly enabled AFTER Initialize; trigger radius 2m |
| Brute guardian | 0 / 16 | Existing Brute stats/basic Smash; exact objective prerequisite |
| Infernal Heart | 0 / 30 | Existing 2.5s capture after guardian defeat |

Keep the Ent centre within |X| <= 5.64m; around fire, X = +/-3.5m leaves a safe
bypass. The fire's visible danger footprint must match its 2m trigger radius
(4m diameter), not a guessed primitive scale; disable its decorative collider.
Use only this one hazard in the initial trial. Do not copy the old full-width
LavaGate: it raises a collider for 10s and roots, risking another blocked route.
Do not add the audit's second hazard or require all three enemies dead: only the
explicit final Brute gates Heart, so retreating past hounds remains a player choice.
Detection is currently 11m and home leash 18m; positions should be verified against
the actual scaled colliders during integration. The first hound starts pressure
after a short safe opening. No special Brute AI attack is claimed in this gate.

## Proposed reserved paths at activation

- New `Assets/Game/Scripts/Core/InfernalRaidBootstrap.cs` and `.meta`.
- New `Assets/Game/Scenes/InfernalRaid.unity` and `.meta`, using the existing minimal
  runtime-bootstrap scene convention (no copied embedded camera/actor duplicates).
- `Assets/Game/Scripts/UI/HubHUD.cs` for entry and intentional responsive placement.
- `Assets/Game/Scripts/UI/RaidHUD.cs` plus optional new `RaidHudConfig.cs`/`.meta`
  in that folder for a small explicit per-raid context.
- `Assets/Game/Editor/PlatformBuild.cs` scene allowlist and
  `ProjectSettings/EditorBuildSettings.asset` exact new scene registration only.
- New focused `Assets/Game/Tests/PlayMode/InfernalRaidFlowTests.cs`/`.meta`, and
  existing relevant Hub/Raid EditMode tests named by Core before activation.

Do not edit InfernalRealmBootstrap, CreatureBrain, general combat/physics, Modules,
assets/import settings or unrelated ProjectSettings without a concrete amendment.

## HUD and state requirements

RaidHUD currently hardcodes Sylvan, Blood Knight, Heart Tree, three Knight labels,
the Sylvan icon and Sylvan retry. Supply hero identity, realm/objective copy,
ability labels/icons, retry destination and standalone result routes explicitly.
Preserve the current Sylvan defaults/tests. Use Ent icons already owned by the
project. Do not duplicate the full RaidHUD implementation or introduce a general
UI/plugin framework.

Keep joystick/fingertap, jump, dodge, manual camera and orientation semantics.
No possession setup is required to play this Ent trial; the same entity remains
authoritative. No Moonwell or Sylvan bonus-Ent reference leaks into Infernal.
Standard raid loot may use the existing truthful result/store pipeline once; do
not claim Infernal Build/progression systems that do not exist.

## Verification

- Hub destination and scene build registration; exactly one camera/listener,
  EventSystem and raid controller; living direct Ent with correct three abilities.
- Ent feet/forward and both control styles/orientations; no blocker at the hazard.
- Explicit Brute gates the Heart; dead hero/terminal/reload cannot complete or
  duplicate rewards, callbacks or scene handoff.
- Result retry goes to InfernalRaid, Hub return is truthful, Sylvan/Infernal defense
  baselines remain intact. One final Edit/Play pair after the frozen candidate.
- Record actual manual playability/feel separately; automation counts do not prove
  the targeted duration, visual quality or replay desire.
