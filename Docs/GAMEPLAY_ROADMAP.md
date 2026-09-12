# Realm Raiders — Gameplay Priority Reset

Decision date: 2026-09-12. Owner: Architect / Product Lead.
Status: plan; only 18.0 is currently leased in the main checkout.

## What the code currently offers

- Sylvan has seven fixed nodes, two Wolves and one Ent. Wolf/Ent groves are
  optional branches from Crossroads; Root Path → Moonwell → Heart Tree is the
  objective route (`SylvanRealmBootstrap`).
- Moonwell is presently a named empty node. Discovery gives 5 Gold; every enemy
  gives 15 Gold, including the 340-HP Ent. Core victory gives 100 Gold and 1 Rare
  Material (`RaidManager`, `CombatStats`). The costly Ent detour lacks a distinct
  reward. Defeat currently halves Gold; Rare Materials follow existing result rules.
- `CreatureBrain.Tick` chases and repeatedly selects ability 0. Ent has Charge and
  Ground Slam definitions but its AI does not choose them. More imported models
  alone would not change this fight rhythm.
- The saved five-slot Build, cultivation, traps, possession and result loop exist.
  Much recent work improves feedback; the number of materially different encounters
  and tactical decisions is still small. Automated green does not prove enjoyment.

## Effort allocation

Plan a rolling ten-unit batch: 7 units gameplay/content, 3 units repair/foundation.
Units are relative effort, not hours or commit counts; re-estimate after each
handoff. QA/regression cost belongs to its feature's estimate. Urgent reported
playability defects can lead the batch, but must be followed by gameplay rather
than another chain of cosmetic receipts. Do not invent infrastructure to occupy a
lane. Reuse current art, combat, graph, HUD and persistence wherever practical.

| Gate | Effort | Concrete player change | Evidence of value |
| --- | --- | --- | --- |
| 18.0 repair | 3 polish | Forward-facing grounded Ent; modest landing crouch and recovery after jump or fall | Observe feet/direction/contact; no root/collider or takeoff regression |
| 18.1 route choice | 2 gameplay | One Moonwell recovery charge and an extra Rare Material for optional Ent defeat | Player chooses whether to spend healing and risk the Ent detour |
| 18.1B Infernal Ent raid | 2 gameplay | Direct Ent raids Hellhounds, a bypassable hazard and a Brute-guarded Heart | Ent area attacks feel useful; clear finale and immediate same-scene retry |
| 18.2 Ent fight | 2 gameplay | Readable heavy Ground Slam with a recovery window interleaved with basic attacks | Player can learn, evade, then punish; possession uses the same existing abilities |
| 18.3 encounter variants | 1 gameplay | Integrate the three prepared Sylvan enemy compositions | Replays change pressure and priorities without new art or unfair hidden spawns |

18.0 and 18.1 are accepted; 18.1B Infernal Ent trial is active before 18.2. Its
detailed brief is `INFERNAL_ENT_RAID_JOB.md`. Relative estimates were
rebalanced to retain the 7 gameplay / 3 polish target; re-estimate after real
implementation. Module has prepared the 18.3 compositions; Core owns integration.
The subsequent 18.4 Build/trap/possession tradeoff remains backlog for the next batch.

The desired "wow" is a playable contrast: become the heavy Ent, draw two fast
enemies into one strong area hit, survive a readable fire obstacle and defeat the
guardian. The desire to retry should come from learning and trying another tactic,
not from extra labels, random stat inflation or an unfair surprise death.

## 18.1 starting design

Moonwell offers one heal of up to 30% of maximum HP per raid, capped at maximum.
Consumption requires the living raid hero in the well's small explicit interaction
radius, below full health, and an active nonterminal raid. Reuse the existing
interaction/HUD conventions; do not steal taps or add another input mode. Prefer
an explicit reachable use action so the player decides when to spend it. Return to
the well may consume a preserved charge; entering at full health must not waste it.
The visible well changes to spent only after actual healing, with no resurrection.

The explicitly configured optional Ent pays +1 Rare Material once on its actual
death, in addition to normal kill Gold. Do not infer identity by display name.
Use the existing raid receipt/result/store pipeline; duplicate death callbacks,
revisits, result refresh and reload cannot duplicate credit. This is a provisional
balance value, not a new loot tier, inventory system or permanent healing upgrade.

## How to make the next minute interesting

Aim for a short sequence of anticipation → choice → execution → consequence:
spot a slow dangerous Ent, decide whether its material is worth the detour, avoid
its clearly signalled slam, spend a limited heal if necessary, and bring the gain
back to the existing cultivation/defense loop. Wolves should pressure movement;
Ent should reward patience and attack timing. Enemy count alone is not difficulty.

For first variants, change composition/placement within proven walkable nodes,
not graph geometry or random stat inflation. Keep openings readable and avoid
unavoidable damage at entry, offscreen mandatory attacks, choke-point body piles
or passive infinite waves. Infernal later emphasizes aggression and timed flame
pressure; Sylvan emphasizes control, detours and recovery.

## Play checks and stopping rules

- Can a new player explain the next choice without reading a design document?
- Can they identify why they took damage and change their next attempt?
- Does the optional fight offer a reward that matters in the existing Build loop?
- Are at least two tactics viable in both orientations/control styles?
- Does a repeat run change decisions, rather than only numbers or labels?

Use short observed runs and user feedback to tune these; do not claim they passed
from unit tests. Tests target actual state/credit/lifecycle risks. One final suite
pair per frozen Core batch remains the verification rule. Physical-device checks
and user push do not block independent gameplay/content work.

Defer large skill trees, multiplayer, procedural worlds, large new asset purchases,
generic content editors and wholesale physics/navigation rewrites. Introduce such
work only when a concrete playable slice requires it.
