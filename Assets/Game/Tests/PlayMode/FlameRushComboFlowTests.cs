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
    public sealed class FlameRushComboFlowTests
    {
        [UnityTearDown]
        public IEnumerator ResetState()
        {
            GameplayInput.ResetForTests();
            Time.timeScale = 1;
            yield return null;
        }

        [UnityTest]
        public IEnumerator ExactPossessedBruteCharge_DetonatesOnlyRemainingPulsesOnceAndOwnsResponsiveCopy()
        {
            GameplayInput.ResetForTests();
            var fixture = new Fixture();
            var resolved = 0;
            var claimed = 0;
            fixture.Combo.Resolved += count => { resolved++; claimed = count; };
            try
            {
                fixture.AddDuplicateInvaderCollider();
                Assert.That(fixture.Trap.TryActivate(), Is.True);
                yield return null;
                Assert.That(fixture.Combo.IsArmed, Is.True);
                Assert.That(fixture.Hud.RootTrapOpportunityText, Is.EqualTo(FlameRushCombo.OpportunityCopy));
                Assert.That(fixture.Hud.RootTrapOpportunityRaycastTarget, Is.False);

                fixture.Responsive.SetOrientationForTests(PrototypeOrientation.Portrait);
                yield return null;
                AssertContained(fixture.Hud.RootTrapOpportunityRect,
                    fixture.Responsive.GetComponent<CanvasScaler>().referenceResolution);
                fixture.Responsive.SetOrientationForTests(PrototypeOrientation.Landscape);
                yield return null;
                AssertContained(fixture.Hud.RootTrapOpportunityRect,
                    fixture.Responsive.GetComponent<CanvasScaler>().referenceResolution);

                fixture.PossessBrute();
                Assert.That(fixture.Hud.RootTrapOpportunityVisible, Is.False);
                Assert.That(fixture.Player.RequestAbility(1, Vector3.forward), Is.True);
                yield return WaitFor(() => resolved == 1, "The exact possessed Brute Charge did not resolve Flame Rush.");
                Assert.That(claimed, Is.EqualTo(2));
                Assert.That(fixture.Invader.Health.Current, Is.EqualTo(48).Within(.001f),
                    "Damage must be one ordinary 28 Charge plus the two already-scheduled 8-damage pulses.");
                Assert.That(fixture.Trap.BurnPulsesRemaining, Is.Zero);
                Assert.That(fixture.Hud.FlameRushFeedbackText, Is.EqualTo(FlameRushCombo.ConfirmationCopy));
                Assert.That(fixture.Hud.FlameRushFeedbackRaycastTarget, Is.False);

                yield return new WaitForSeconds(3.2f);
                Assert.That(fixture.Invader.Health.Current, Is.EqualTo(48).Within(.001f),
                    "Duplicate colliders and the canceled burn routine cannot replay damage.");
                Assert.That(resolved, Is.EqualTo(1));
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [UnityTest]
        public IEnumerator AiChargeAndWrongPossessedAction_DoNotResolveOrConsumeBurn()
        {
            GameplayInput.ResetForTests();
            var aiFixture = new Fixture();
            var wrongActionFixture = new Fixture(new Vector3(20, 0, 0));
            var aiResolved = 0;
            var wrongResolved = 0;
            aiFixture.Combo.Resolved += _ => aiResolved++;
            wrongActionFixture.Combo.Resolved += _ => wrongResolved++;
            try
            {
                Assert.That(aiFixture.Trap.TryActivate(), Is.True);
                Assert.That(aiFixture.Brute.TryUse(1, Vector3.forward), Is.True);
                yield return WaitForIdle(aiFixture.Brute);
                Assert.That(aiResolved, Is.Zero, "An AI-owned Charge cannot resolve Flame Rush.");

                Assert.That(wrongActionFixture.Trap.TryActivate(), Is.True);
                wrongActionFixture.PossessBrute();
                Assert.That(wrongActionFixture.Player.RequestAbility(0, Vector3.forward), Is.True);
                yield return WaitForIdle(wrongActionFixture.Brute);
                Assert.That(wrongResolved, Is.Zero, "Possessed Smash is not the exact slot-1 Charge.");
                Assert.That(wrongActionFixture.Combo.IsArmed, Is.True,
                    "A wrong action before the first scheduled pulse must not consume the burn opportunity.");
                Assert.That(wrongActionFixture.Trap.BurnPulsesRemaining, Is.EqualTo(2));
            }
            finally
            {
                aiFixture.Destroy();
                wrongActionFixture.Destroy();
            }
        }

        [UnityTest]
        public IEnumerator ReleaseClearsComboButPreservesOrdinaryScheduledBurn()
        {
            GameplayInput.ResetForTests();
            var fixture = new Fixture();
            var resolved = 0;
            fixture.Combo.Resolved += _ => resolved++;
            try
            {
                Assert.That(fixture.Trap.TryActivate(), Is.True);
                fixture.PossessBrute();
                fixture.Possession.Release();
                Assert.That(fixture.Combo.IsArmed, Is.False);
                Assert.That(fixture.Trap.BurnPulsesRemaining, Is.EqualTo(2),
                    "Possession cleanup must not cancel the Flame Trap's ordinary scheduled burn.");
                Assert.That(fixture.Hud.FlameRushFeedbackText, Is.Empty);
                var notice = FindReleaseNotice(fixture.Hud);
                Assert.That(notice.gameObject.activeSelf, Is.True);
                Assert.That(notice.text, Does.StartWith("RELEASED —"));
                var releaseCopy = notice.text;
                fixture.Hud.BindFlameRush(null);
                Assert.That(notice.gameObject.activeSelf, Is.True,
                    "A late combo clear cannot hide newer possession/release feedback.");
                Assert.That(notice.text, Is.EqualTo(releaseCopy));

                yield return new WaitForSeconds(3.2f);
                Assert.That(fixture.Invader.Health.Current, Is.EqualTo(76).Within(.001f));
                Assert.That(fixture.Trap.BurnPulsesRemaining, Is.Zero);
                Assert.That(resolved, Is.Zero);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [UnityTest]
        public IEnumerator FatalOrdinaryCharge_ClaimsRemainingScheduleWithoutDamagingDeadInvader()
        {
            GameplayInput.ResetForTests();
            var fixture = new Fixture(default, 36);
            var resolved = 0;
            var claimed = 0;
            fixture.Combo.Resolved += count => { resolved++; claimed = count; };
            try
            {
                Assert.That(fixture.Trap.TryActivate(), Is.True);
                Assert.That(fixture.Invader.Health.Current, Is.EqualTo(28));
                fixture.PossessBrute();
                Assert.That(fixture.Player.RequestAbility(1, Vector3.forward), Is.True);
                yield return WaitFor(() => resolved == 1, "Fatal ordinary Charge did not consume the exact burn run.");
                Assert.That(claimed, Is.EqualTo(2));
                Assert.That(fixture.Invader.Health.IsDead, Is.True);
                Assert.That(fixture.Invader.Health.Current, Is.Zero);
                Assert.That(fixture.Trap.BurnPulsesRemaining, Is.Zero);
                yield return new WaitForSeconds(3.2f);
                Assert.That(fixture.Invader.Health.Current, Is.Zero);
                Assert.That(resolved, Is.EqualTo(1));
            }
            finally
            {
                fixture.Destroy();
            }
        }

        [UnityTest]
        public IEnumerator ExpiredBurnTerminalAndHelperDisable_FailClosedWithoutReplay()
        {
            GameplayInput.ResetForTests();
            var expired = new Fixture();
            var terminal = new Fixture(new Vector3(20, 0, 0));
            var disabled = new Fixture(new Vector3(40, 0, 0));
            var resolved = 0;
            expired.Combo.Resolved += _ => resolved++;
            terminal.Combo.Resolved += _ => resolved++;
            disabled.Combo.Resolved += _ => resolved++;
            try
            {
                Assert.That(expired.Trap.TryActivate(), Is.True);
                yield return new WaitForSeconds(3.2f);
                Assert.That(expired.Combo.IsArmed, Is.False);
                expired.PossessBrute();
                Assert.That(expired.Player.RequestAbility(1, Vector3.forward), Is.True);
                yield return WaitForIdle(expired.Brute);
                Assert.That(resolved, Is.Zero);

                Assert.That(terminal.Trap.TryActivate(), Is.True);
                GameplayInput.SetTerminalState(true);
                yield return null;
                Assert.That(terminal.Combo.IsArmed, Is.False);
                Assert.That(terminal.Hud.RootTrapOpportunityVisible, Is.False);
                GameplayInput.SetTerminalState(false);

                Assert.That(disabled.Trap.TryActivate(), Is.True);
                disabled.Combo.enabled = false;
                Assert.That(disabled.Combo.IsOperational, Is.False);
                Assert.That(disabled.Combo.IsArmed, Is.False);
                Assert.That(disabled.Trap.BurnPulsesRemaining, Is.EqualTo(2),
                    "Disabling the helper cannot stop the Flame Trap burn routine.");
                yield return new WaitForSeconds(3.2f);
                Assert.That(disabled.Invader.Health.Current, Is.EqualTo(76).Within(.001f));
                Assert.That(resolved, Is.Zero);
            }
            finally
            {
                GameplayInput.SetTerminalState(false);
                expired.Destroy();
                terminal.Destroy();
                disabled.Destroy();
            }
        }

        [UnityTest]
        public IEnumerator ControllerLossReinitializeAndTeardown_ClearOnlyComboStateAndNeverReplay()
        {
            GameplayInput.ResetForTests();
            var fixture = new Fixture();
            var resolved = 0;
            fixture.Combo.Resolved += _ => resolved++;
            try
            {
                Assert.That(fixture.Trap.TryActivate(), Is.True);
                fixture.PossessBrute();
                fixture.Brute.SetController(fixture.Brute.Controller<CreatureBrain>());
                Assert.That(fixture.Possession.IsPossessing, Is.True,
                    "The fixture must remove controller authority without calling Possession.Release.");
                Assert.That(fixture.Combo.IsArmed, Is.False);
                Assert.That(fixture.Trap.BurnPulsesRemaining, Is.EqualTo(2));

                fixture.Combo.Initialize(fixture.Trap, fixture.Invader, fixture.Brute,
                    fixture.Possession, fixture.Defense);
                Assert.That(fixture.Combo.IsOperational, Is.True);
                Assert.That(fixture.Combo.IsArmed, Is.False,
                    "Reinitialization cannot replay an already-published activation revision.");
                Object.Destroy(fixture.Combo);
                yield return null;
                Assert.That(fixture.Trap.BurnPulsesRemaining, Is.EqualTo(2),
                    "Helper teardown cannot cancel ordinary scheduled burn.");
                yield return new WaitForSeconds(3.2f);
                Assert.That(fixture.Invader.Health.Current, Is.EqualTo(76).Within(.001f));
                Assert.That(resolved, Is.Zero);
            }
            finally
            {
                fixture.Destroy();
            }
        }

        static IEnumerator WaitForIdle(CombatEntity entity)
        {
            yield return WaitFor(() => entity.ActionPhase == CombatActionPhase.Idle,
                "Combat action did not return to Idle.");
        }

        static IEnumerator WaitFor(System.Func<bool> condition, string failure)
        {
            var deadline = Time.realtimeSinceStartup + 2f;
            do { yield return null; } while (!condition() && Time.realtimeSinceStartup < deadline);
            Assert.That(condition(), Is.True, failure);
        }

        static void AssertContained(RectTransform rect, Vector2 parentSize)
        {
            Assert.That(rect, Is.Not.Null);
            var anchorReference = new Vector2(
                Mathf.Lerp(rect.anchorMin.x, rect.anchorMax.x, rect.pivot.x),
                Mathf.Lerp(rect.anchorMin.y, rect.anchorMax.y, rect.pivot.y));
            var pivotPoint = Vector2.Scale(anchorReference, parentSize) + rect.anchoredPosition;
            var size = rect.sizeDelta;
            if (rect.anchorMin != rect.anchorMax)
                size += Vector2.Scale(rect.anchorMax - rect.anchorMin, parentSize);
            var bounds = new Rect(pivotPoint - Vector2.Scale(rect.pivot, size), size);
            Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(bounds.xMax, Is.LessThanOrEqualTo(parentSize.x));
            Assert.That(bounds.yMax, Is.LessThanOrEqualTo(parentSize.y));
        }

        static Text FindReleaseNotice(DefenderHUD hud)
        {
            foreach (var label in hud.GetComponentsInChildren<Text>(true))
                if (label.name == "Possession Release Notice") return label;
            Assert.Fail("Possession Release Notice was not built.");
            return null;
        }

        sealed class Fixture
        {
            readonly GameObject root, cameraObject, invaderObject, bruteObject, coreObject, trapObject, hudObject;
            readonly CharacterDefinition invaderDefinition, bruteDefinition;
            readonly AbilityDefinition smash, charge, slam;
            bool destroyed;

            public readonly CombatEntity Invader;
            public readonly CombatEntity Brute;
            public readonly PlayerController Player;
            public readonly PossessionManager Possession;
            public readonly DefenseManager Defense;
            public readonly FlameTrap Trap;
            public readonly FlameRushCombo Combo;
            public readonly DefenderHUD Hud;
            public readonly ResponsiveHudRoot Responsive;

            public Fixture(Vector3 offset = default, float invaderHealth = 100)
            {
                root = new GameObject("Flame Rush Root");
                cameraObject = new GameObject("Flame Rush Camera", typeof(Camera), typeof(PrototypeCameraRig));
                cameraObject.tag = "MainCamera";
                invaderObject = new GameObject("Flame Rush Invader", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
                bruteObject = new GameObject("Flame Rush Brute", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
                coreObject = new GameObject("Flame Rush Core", typeof(RealmCore));
                trapObject = new GameObject("Flame Rush Trap", typeof(FlameTrap));
                hudObject = new GameObject("Flame Rush HUD", typeof(DefenderHUD));

                smash = Ability("Smash", AbilityKind.Melee, 10);
                charge = Ability("Charge", AbilityKind.Dash, 28);
                slam = Ability("Ground Slam", AbilityKind.Area, 20);
                invaderDefinition = Definition("Raid Invader", false, invaderHealth, System.Array.Empty<AbilityDefinition>());
                bruteDefinition = Definition("Infernal Brute", true, 200, new[] { smash, charge, slam });
                bruteDefinition.ArchetypeId = PrototypeCharacterRoster.InfernalBruteId;
                Invader = invaderObject.GetComponent<CombatEntity>();
                Invader.Initialize(invaderDefinition);
                Brute = bruteObject.GetComponent<CombatEntity>();
                Brute.Initialize(bruteDefinition);
                Brute.SetController(Brute.Controller<CreatureBrain>());
                bruteObject.transform.position = offset;
                invaderObject.transform.position = offset + new Vector3(0, 0, 1);
                trapObject.transform.position = invaderObject.transform.position;
                coreObject.transform.position = offset + new Vector3(100, 0, 100);
                Physics.SyncTransforms();

                var rig = cameraObject.GetComponent<PrototypeCameraRig>();
                rig.ConfigureOverview(offset + new Vector3(0, 12, -12), Quaternion.Euler(45, 0, 0));
                rig.SnapToOverview();
                Possession = root.AddComponent<PossessionManager>();
                var energy = new PossessionEnergy(30);
                Possession.Initialize(rig);
                Possession.ConfigureEnergy(energy);
                Possession.Register(Brute);
                var core = coreObject.GetComponent<RealmCore>();
                core.Initialize(Invader);
                Defense = root.AddComponent<DefenseManager>();
                Defense.Initialize(Invader, core, Possession);
                Trap = trapObject.GetComponent<FlameTrap>();
                Trap.Automatic = false;
                Trap.Initialize(Invader);
                Combo = root.AddComponent<FlameRushCombo>();
                Combo.Initialize(Trap, Invader, Brute, Possession, Defense);
                Hud = hudObject.GetComponent<DefenderHUD>();
                Hud.Initialize(Defense, Possession, energy, Invader, Brute, Trap, core, DefenseHudConfig.Infernal);
                Hud.BindFlameRush(Combo);
                Responsive = hudObject.GetComponent<ResponsiveHudRoot>();
                Player = Brute.Controller<PlayerController>();
            }

            public void PossessBrute()
            {
                Possession.Select(Brute);
                Assert.That(Possession.PossessSelected(), Is.True);
                Assert.That(Possession.Possessed, Is.SameAs(Brute));
                Assert.That(Brute.ActiveController, Is.SameAs(Player));
                Assert.That(Player.IsActive, Is.True);
            }

            public void AddDuplicateInvaderCollider()
            {
                var child = new GameObject("Duplicate Invader Collider", typeof(BoxCollider));
                child.transform.SetParent(invaderObject.transform, false);
                child.GetComponent<BoxCollider>().size = Vector3.one;
                Physics.SyncTransforms();
            }

            public void Destroy()
            {
                if (destroyed) return;
                destroyed = true;
                if (Possession && Possession.IsPossessing) Possession.Release();
                Object.Destroy(hudObject);
                Object.Destroy(trapObject);
                Object.Destroy(coreObject);
                Object.Destroy(bruteObject);
                Object.Destroy(invaderObject);
                Object.Destroy(cameraObject);
                Object.Destroy(root);
                Object.Destroy(invaderDefinition);
                Object.Destroy(bruteDefinition);
                Object.Destroy(smash);
                Object.Destroy(charge);
                Object.Destroy(slam);
            }

            static CharacterDefinition Definition(string name, bool possessable, float health,
                AbilityDefinition[] abilities)
            {
                var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
                definition.DisplayName = name;
                definition.Possessable = possessable;
                definition.Stats = new CombatStats { MaxHealth = health, MoveSpeed = 3, AttackSpeed = 1 };
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
                ability.DashDistance = kind == AbilityKind.Dash ? .1f : 0;
                return ability;
            }
        }
    }
}
