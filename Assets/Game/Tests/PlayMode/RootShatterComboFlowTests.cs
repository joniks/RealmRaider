using System.Collections;
using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.Possession;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using RealmRaiders.Traps;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class RootShatterComboFlowTests
    {
        [UnityTest]
        public IEnumerator ManualTrapPossessExactEntGroundSlam_ResolvesOnceAndBreaksRoot()
        {
            GameplayInput.SetTerminalState(false);
            var fixture = new Fixture(200, 20);
            var resolved = 0;
            fixture.Combo.Resolved += () => resolved++;
            try
            {
                fixture.Trap.transform.position = Vector3.right * 50;
                Assert.That(fixture.Trap.TryActivate(), Is.False);
                Assert.That(fixture.Trap.ActivationRevision, Is.Zero, "Failed activation cannot publish a revision.");
                fixture.Trap.transform.position = fixture.Invader.transform.position;
                fixture.AddInvaderCollider();
                Assert.That(fixture.Invader.GetComponentsInChildren<Collider>().Length, Is.GreaterThanOrEqualTo(2));
                fixture.ActivateTrap();
                Assert.That(fixture.Trap.ActivationRevision, Is.EqualTo(1));
                Assert.That(fixture.Combo.ArmedRevision, Is.EqualTo(1));
                Assert.That(fixture.Invader.Health.Current, Is.EqualTo(188).Within(.001f));

                fixture.Invader.Health.TakeDamage(new DamageInfo(1, fixture.OtherSource, fixture.Invader.transform.position), 0);
                Assert.That(fixture.Combo.IsArmed, Is.True, "Unrelated damage cannot consume the trap revision.");
                fixture.PossessEnt();

                Assert.That(fixture.Player.RequestAbility(0, Vector3.forward), Is.True);
                yield return WaitForIdle(fixture.Ent);
                Assert.That(resolved, Is.Zero, "A different ability cannot resolve Root Shatter.");
                Assert.That(fixture.Combo.IsArmed, Is.True);
                Assert.That(fixture.Invader.IsRooted, Is.True);

                Assert.That(fixture.Player.RequestAbility(2, Vector3.forward), Is.True);
                yield return WaitFor(() => resolved == 1, "Root Shatter did not resolve from the exact Ground Slam hit.");
                Assert.That(fixture.Invader.Health.Current, Is.EqualTo(139).Within(.001f),
                    "Multi-collider contact must still apply Trap 12 + unrelated 1 + Smash 10 + Ground Slam 20 + Root Shatter 18 exactly once.");
                Assert.That(fixture.Invader.IsRooted, Is.False);
                Assert.That(fixture.Combo.IsArmed, Is.False);
                Assert.That(fixture.Hud.RootShatterFeedbackText, Is.EqualTo(RootShatterCombo.ConfirmationCopy));
                Assert.That(fixture.Hud.RootShatterFeedbackRaycastTarget, Is.False);

                fixture.Invader.Health.TakeDamage(new DamageInfo(1, fixture.Ent.gameObject, fixture.Invader.transform.position), 0);
                Assert.That(fixture.Invader.Health.Current, Is.EqualTo(138).Within(.001f));
                Assert.That(resolved, Is.EqualTo(1), "A consumed revision blocks duplicate and re-entrant callbacks.");
                fixture.Possession.Release();
                Assert.That(fixture.Hud.RootShatterFeedbackText, Is.Empty, "Release clears combo-owned transient copy.");
            }
            finally
            {
                fixture.Destroy();
                GameplayInput.SetTerminalState(false);
            }
        }

        [UnityTest]
        public IEnumerator AiSlamExpiredRootReleaseTerminalAndRetry_DoNotResolveOrLeak()
        {
            GameplayInput.SetTerminalState(false);
            Fixture immune = null, ai = null, expired = null, released = null, terminal = null, retry = null;
            try
            {
                immune = new Fixture(200, 20);
                var invaderPlayer = immune.Invader.Controller<PlayerController>();
                immune.Invader.SetController(invaderPlayer);
                Assert.That(immune.Invader.TryDodge(Vector3.right), Is.True);
                immune.ActivateTrap();
                Assert.That(immune.Invader.Health.Current, Is.EqualTo(200).Within(.001f),
                    "A successful activation arms its revision even when ordinary trap damage is immune.");
                Assert.That(immune.Trap.ActivationRevision, Is.EqualTo(1));
                Assert.That(immune.Combo.IsArmed, Is.True);
                immune.Destroy();
                yield return null;

                ai = new Fixture(200, 20);
                var aiResolved = 0;
                ai.Combo.Resolved += () => aiResolved++;
                ai.ActivateTrap();
                Assert.That(ai.Ent.TryUse(2, Vector3.forward), Is.True);
                yield return WaitForIdle(ai.Ent);
                Assert.That(aiResolved, Is.Zero);
                Assert.That(ai.Invader.Health.Current, Is.EqualTo(168).Within(.001f), "AI Slam keeps only ordinary damage.");
                Assert.That(ai.Combo.IsArmed, Is.True, "An AI action cannot consume the player opportunity.");
                ai.Destroy();
                yield return null;

                expired = new Fixture(200, 20);
                var expiredResolved = 0;
                expired.Combo.Resolved += () => expiredResolved++;
                expired.ActivateTrap();
                expired.Invader.BreakRoot();
                yield return null;
                Assert.That(expired.Combo.IsArmed, Is.False);
                expired.PossessEnt();
                Assert.That(expired.Player.RequestAbility(2, Vector3.forward), Is.True);
                yield return WaitForIdle(expired.Ent);
                Assert.That(expiredResolved, Is.Zero);
                Assert.That(expired.Invader.Health.Current, Is.EqualTo(168).Within(.001f));
                expired.Destroy();
                yield return null;

                released = new Fixture(200, 20);
                var releasedResolved = 0;
                released.Combo.Resolved += () => releasedResolved++;
                released.ActivateTrap();
                released.PossessEnt();
                released.Possession.Release();
                Assert.That(released.Combo.IsArmed, Is.False, "Explicit release consumes no damage but clears the opportunity.");
                Assert.That(released.Ent.TryUse(2, Vector3.forward), Is.True);
                yield return WaitForIdle(released.Ent);
                Assert.That(releasedResolved, Is.Zero);
                Assert.That(released.Invader.Health.Current, Is.EqualTo(168).Within(.001f));
                released.Destroy();
                yield return null;

                terminal = new Fixture(200, 20);
                terminal.ActivateTrap();
                GameplayInput.SetTerminalState(true);
                yield return null;
                Assert.That(terminal.Combo.IsArmed, Is.False, "Terminal input state clears the run-local revision.");
                terminal.Destroy();
                yield return null;
                GameplayInput.SetTerminalState(false);

                retry = new Fixture(200, 20);
                Assert.That(Object.FindObjectsByType<RootShatterCombo>(FindObjectsSortMode.None), Has.Length.EqualTo(1),
                    "Teardown and same-scene reconstruction leave one fresh combo owner.");
                Assert.That(retry.Trap.ActivationRevision, Is.Zero);
                Assert.That(retry.Combo.IsArmed, Is.False);
                Assert.That(retry.Hud.RootShatterFeedbackText, Is.Empty);
            }
            finally
            {
                immune?.Destroy(); ai?.Destroy(); expired?.Destroy(); released?.Destroy(); terminal?.Destroy(); retry?.Destroy();
                GameplayInput.SetTerminalState(false);
            }
        }

        [UnityTest]
        public IEnumerator FatalOrdinaryGroundSlam_PublishesSuccessWithoutBonusAgainstDeadInvader()
        {
            GameplayInput.SetTerminalState(false);
            var fixture = new Fixture(50, 38);
            var resolved = 0;
            fixture.Combo.Resolved += () => resolved++;
            try
            {
                fixture.ActivateTrap();
                Assert.That(fixture.Invader.Health.Current, Is.EqualTo(38).Within(.001f));
                fixture.PossessEnt();
                Assert.That(fixture.Player.RequestAbility(2, Vector3.forward), Is.True);
                yield return WaitFor(() => resolved == 1, "Fatal ordinary Ground Slam did not publish Root Shatter success.");
                Assert.That(fixture.Invader.Health.Current, Is.Zero);
                Assert.That(fixture.Invader.Health.IsDead, Is.True);
                Assert.That(fixture.Invader.IsRooted, Is.False);
                Assert.That(fixture.Defense.State, Is.EqualTo(DefenseState.DefenderVictory));
                Assert.That(fixture.Combo.IsArmed, Is.False);
                Assert.That(fixture.Hud.RootShatterFeedbackText, Is.Empty, "Terminal result owns presentation immediately.");
            }
            finally
            {
                fixture.Destroy();
                GameplayInput.SetTerminalState(false);
            }
        }

        [UnityTest]
        public IEnumerator SameTrapSecondActivation_UsesNextRevisionAndRearmsSameCombo()
        {
            GameplayInput.SetTerminalState(false);
            var fixture = new Fixture(200, 20);
            try
            {
                fixture.ActivateTrap();
                Assert.That(fixture.Trap.ActivationRevision, Is.EqualTo(1));
                yield return AdvanceCooldown(fixture.Trap);
                Assert.That(fixture.Combo.IsArmed, Is.False, "The first root expires before the trap becomes ready.");

                Assert.That(fixture.Trap.TryActivate(), Is.True);
                Assert.That(fixture.Trap.ActivationRevision, Is.EqualTo(2));
                Assert.That(fixture.Combo.ArmedRevision, Is.EqualTo(2));
            }
            finally
            {
                fixture.Destroy();
                GameplayInput.SetTerminalState(false);
            }
        }

        [UnityTest]
        public IEnumerator EntDeathControllerLossAndComboDisable_ClearAndUnsubscribeExactRun()
        {
            GameplayInput.SetTerminalState(false);
            Fixture entDeath = null, controllerLoss = null, disabled = null;
            try
            {
                entDeath = new Fixture(200, 20);
                entDeath.ActivateTrap();
                entDeath.Ent.Health.TakeDamage(new DamageInfo(1000, entDeath.OtherSource, entDeath.Ent.transform.position), 0);
                Assert.That(entDeath.Combo.IsArmed, Is.False, "Exact Ent death clears the armed revision.");
                entDeath.Destroy();
                yield return null;

                controllerLoss = new Fixture(200, 20);
                controllerLoss.ActivateTrap();
                controllerLoss.PossessEnt();
                controllerLoss.Ent.SetController(controllerLoss.Ent.Controller<CreatureBrain>());
                Assert.That(controllerLoss.Possession.IsPossessing, Is.True,
                    "The test must remove controller authority without using Possession.Release.");
                Assert.That(controllerLoss.Combo.IsArmed, Is.False, "Direct controller loss clears the opportunity immediately.");
                controllerLoss.Destroy();
                yield return null;

                disabled = new Fixture(200, 20);
                disabled.ActivateTrap();
                disabled.Combo.enabled = false;
                Assert.That(disabled.Combo.IsOperational, Is.False);
                Assert.That(disabled.Combo.IsArmed, Is.False);
                yield return AdvanceCooldown(disabled.Trap);
                Assert.That(disabled.Trap.TryActivate(), Is.True);
                Assert.That(disabled.Combo.IsArmed, Is.False, "A disabled helper must be unsubscribed from its old trap.");
            }
            finally
            {
                entDeath?.Destroy(); controllerLoss?.Destroy(); disabled?.Destroy();
                GameplayInput.SetTerminalState(false);
            }
        }

        [UnityTest]
        public IEnumerator DisabledHudRejectsRootShatterAcquisitionAndReenableStaysEmpty()
        {
            GameplayInput.SetTerminalState(false);
            var fixture = new Fixture(200, 20);
            var resolved = 0;
            fixture.Combo.Resolved += () => resolved++;
            try
            {
                fixture.ActivateTrap();
                fixture.PossessEnt();
                fixture.Hud.enabled = false;
                Assert.That(fixture.Player.RequestAbility(2, Vector3.forward), Is.True);
                yield return WaitFor(() => resolved == 1, "Disabled-HUD flow did not resolve the factual combo.");
                Assert.That(fixture.Hud.RootShatterFeedbackText, Is.Empty);
                fixture.Hud.enabled = true;
                yield return null;
                Assert.That(fixture.Hud.RootShatterFeedbackText, Is.Empty,
                    "Re-enabling the HUD cannot replay feedback acquired while disabled.");
                Assert.That(fixture.Hud.RootShatterFeedbackRaycastTarget, Is.False);
                Text notice = null;
                foreach (var label in fixture.Hud.GetComponentsInChildren<Text>(true))
                    if (label.name == "Possession Release Notice") notice = label;
                Assert.That(notice, Is.Not.Null);
                Assert.That(notice.gameObject.activeSelf, Is.False, "No stale transient notice may reactivate on enable.");
            }
            finally
            {
                fixture.Destroy();
                GameplayInput.SetTerminalState(false);
            }
        }

        [UnityTest]
        public IEnumerator BonusDamageUsesExactInvaderArmorAuthority()
        {
            GameplayInput.SetTerminalState(false);
            var fixture = new Fixture(200, 20, 100);
            var resolved = 0;
            fixture.Combo.Resolved += () => resolved++;
            try
            {
                fixture.ActivateTrap();
                Assert.That(fixture.Invader.Health.Current, Is.EqualTo(194).Within(.001f));
                fixture.PossessEnt();
                Assert.That(fixture.Player.RequestAbility(2, Vector3.forward), Is.True);
                yield return WaitFor(() => resolved == 1, "Armored Root Shatter did not resolve.");
                Assert.That(fixture.Invader.Health.Current, Is.EqualTo(175).Within(.001f),
                    "Armor 100 reduces trap 12→6, Slam 20→10 and Root Shatter 18→9 through normal Health authority.");
            }
            finally
            {
                fixture.Destroy();
                GameplayInput.SetTerminalState(false);
            }
        }

        [UnityTest]
        public IEnumerator ReinitializeSameCombo_ClearsOldRunAndRebindsOnlyFreshExactReferences()
        {
            GameplayInput.SetTerminalState(false);
            Fixture oldRun = null, freshRun = null;
            try
            {
                oldRun = new Fixture(200, 20);
                oldRun.ActivateTrap();
                Assert.That(oldRun.Combo.IsArmed, Is.True);

                freshRun = new Fixture(200, 20);
                freshRun.Combo.enabled = false;
                oldRun.Combo.Initialize(freshRun.Trap, freshRun.Invader, freshRun.Ent,
                    freshRun.Possession, freshRun.Defense);
                Assert.That(oldRun.Combo.IsOperational, Is.True);
                Assert.That(oldRun.Combo.IsArmed, Is.False, "Reinitialize must not retain the old activation revision.");

                yield return AdvanceCooldown(oldRun.Trap);
                Assert.That(oldRun.Trap.TryActivate(), Is.True);
                Assert.That(oldRun.Combo.IsArmed, Is.False, "The previous trap must be unsubscribed after reinitialize.");

                Assert.That(freshRun.Trap.TryActivate(), Is.True);
                Assert.That(oldRun.Combo.ArmedRevision, Is.EqualTo(freshRun.Trap.ActivationRevision),
                    "Only the fresh exact trap may arm the reinitialized combo.");
            }
            finally
            {
                oldRun?.Destroy();
                freshRun?.Destroy();
                GameplayInput.SetTerminalState(false);
            }
        }

        static IEnumerator WaitForIdle(CombatEntity entity)
        {
            yield return WaitFor(() => entity.ActionPhase == CombatActionPhase.Idle, "Action did not return to Idle.");
        }

        static IEnumerator WaitFor(System.Func<bool> condition, string failure)
        {
            var deadline = Time.realtimeSinceStartup + 2f;
            do { yield return null; } while (!condition() && Time.realtimeSinceStartup < deadline);
            Assert.That(condition(), Is.True, failure);
        }

        static IEnumerator AdvanceCooldown(RootTrap trap)
        {
            var previousScale = Time.timeScale;
            try
            {
                Time.timeScale = 100;
                yield return new WaitForSeconds(trap.CooldownDuration + .1f);
                yield return null;
            }
            finally
            {
                Time.timeScale = previousScale;
            }
            Assert.That(trap.State, Is.EqualTo(TrapState.Ready));
        }

        sealed class Fixture
        {
            readonly GameObject cameraObject, invaderObject, entObject, coreObject, possessionObject,
                defenseObject, trapObject, comboObject, hudObject;
            readonly CharacterDefinition invaderDefinition, entDefinition;
            readonly AbilityDefinition smash, charge, slam;
            bool destroyed;
            public readonly GameObject OtherSource;
            public readonly CombatEntity Invader, Ent;
            public readonly PossessionManager Possession;
            public readonly DefenseManager Defense;
            public readonly RootTrap Trap;
            public readonly RootShatterCombo Combo;
            public readonly DefenderHUD Hud;
            public readonly PlayerController Player;

            public Fixture(float invaderHealth, float slamDamage, float invaderArmor = 0)
            {
                cameraObject = new GameObject("Root Shatter Camera", typeof(Camera), typeof(PrototypeCameraRig));
                cameraObject.tag = "MainCamera";
                invaderObject = new GameObject("Root Shatter Invader", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController));
                entObject = new GameObject("Root Shatter Ent", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
                coreObject = new GameObject("Root Shatter Core", typeof(RealmCore));
                possessionObject = new GameObject("Root Shatter Possession", typeof(PossessionManager));
                defenseObject = new GameObject("Root Shatter Defense", typeof(DefenseManager));
                trapObject = new GameObject("Root Shatter Trap", typeof(RootTrap));
                comboObject = new GameObject("Root Shatter Combo", typeof(RootShatterCombo));
                hudObject = new GameObject("Root Shatter HUD", typeof(DefenderHUD));
                OtherSource = new GameObject("Unrelated Source");

                smash = Ability("Smash", AbilityKind.Melee, 10);
                charge = Ability("Charge", AbilityKind.Dash, 10);
                slam = Ability("Ground Slam", AbilityKind.Area, slamDamage);
                invaderDefinition = Definition("Raid Invader", false, invaderHealth, System.Array.Empty<AbilityDefinition>(), invaderArmor);
                entDefinition = Definition("Guardian Ent", true, 150, new[] { smash, charge, slam });
                entDefinition.ArchetypeId = PrototypeCharacterRoster.GuardianEntId;
                Invader = invaderObject.GetComponent<CombatEntity>();
                Invader.Initialize(invaderDefinition);
                Ent = entObject.GetComponent<CombatEntity>();
                Ent.Initialize(entDefinition);
                Ent.SetController(Ent.Controller<CreatureBrain>());
                entObject.transform.position = Vector3.zero;
                invaderObject.transform.position = new Vector3(0, 0, 1);
                Physics.SyncTransforms();

                var rig = cameraObject.GetComponent<PrototypeCameraRig>();
                rig.ConfigureOverview(new Vector3(0, 12, -12), Quaternion.Euler(45, 0, 0));
                rig.SnapToOverview();
                Possession = possessionObject.GetComponent<PossessionManager>();
                var energy = new PossessionEnergy(30);
                Possession.Initialize(rig);
                Possession.ConfigureEnergy(energy);
                Possession.Register(Ent);
                coreObject.transform.position = new Vector3(100, 0, 100);
                var core = coreObject.GetComponent<RealmCore>();
                core.Initialize(Invader);
                Defense = defenseObject.GetComponent<DefenseManager>();
                Defense.Initialize(Invader, core, Possession);
                trapObject.transform.position = invaderObject.transform.position;
                Trap = trapObject.GetComponent<RootTrap>();
                Trap.Automatic = false;
                Trap.Initialize(Invader);
                Combo = comboObject.GetComponent<RootShatterCombo>();
                Combo.Initialize(Trap, Invader, Ent, Possession, Defense);
                Hud = hudObject.GetComponent<DefenderHUD>();
                Hud.Initialize(Defense, Possession, energy, Invader, Ent, Trap, core, DefenseHudConfig.Sylvan);
                Hud.BindRootShatter(Combo);
                Player = Ent.Controller<PlayerController>();
            }

            public void ActivateTrap()
            {
                Assert.That(Trap.TryActivate(), Is.True);
                Assert.That(Invader.IsRooted, Is.True);
                Assert.That(Combo.IsArmed, Is.True);
            }

            public void AddInvaderCollider()
            {
                var child = new GameObject("Second Invader Collider", typeof(BoxCollider));
                child.transform.SetParent(invaderObject.transform, false);
                child.GetComponent<BoxCollider>().size = Vector3.one;
                Physics.SyncTransforms();
            }

            public void PossessEnt()
            {
                Possession.Select(Ent);
                Assert.That(Possession.PossessSelected(), Is.True);
                Assert.That(Possession.Possessed, Is.SameAs(Ent));
                Assert.That(Ent.ActiveController, Is.SameAs(Player));
                Assert.That(Player.IsActive, Is.True);
            }

            public void Destroy()
            {
                if (destroyed) return;
                destroyed = true;
                if (Possession && Possession.IsPossessing) Possession.Release();
                Object.Destroy(hudObject);
                Object.Destroy(comboObject);
                Object.Destroy(trapObject);
                Object.Destroy(defenseObject);
                Object.Destroy(possessionObject);
                Object.Destroy(coreObject);
                Object.Destroy(entObject);
                Object.Destroy(invaderObject);
                Object.Destroy(cameraObject);
                Object.Destroy(OtherSource);
                Object.Destroy(invaderDefinition);
                Object.Destroy(entDefinition);
                Object.Destroy(smash);
                Object.Destroy(charge);
                Object.Destroy(slam);
            }

            static CharacterDefinition Definition(string name, bool possessable, float health, AbilityDefinition[] abilities, float armor = 0)
            {
                var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
                definition.DisplayName = name;
                definition.Possessable = possessable;
                definition.Stats = new CombatStats { MaxHealth = health, MoveSpeed = 3, AttackSpeed = 1, Armor = armor };
                definition.Abilities = abilities;
                return definition;
            }

            static AbilityDefinition Ability(string name, AbilityKind kind, float damage)
            {
                var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
                ability.DisplayName = name;
                ability.Kind = kind;
                ability.Damage = damage;
                ability.Range = 1;
                ability.Radius = 2.5f;
                ability.Windup = .01f;
                ability.Recovery = .01f;
                ability.Cooldown = .1f;
                ability.DashDistance = 1;
                return ability;
            }
        }
    }
}
