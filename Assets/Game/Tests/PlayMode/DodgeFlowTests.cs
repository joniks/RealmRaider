using System.Collections;
using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Possession;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class DodgeFlowTests
    {
        [UnityTearDown]
        public IEnumerator ResetTransientState()
        {
            GameplayInput.ResetForTests();
            Time.timeScale = 1;
            var active = SceneManager.GetActiveScene();
            if (active.IsValid() && (active.name == "SylvanRealm" || active.name == "DefenderTest"))
            {
                var cleanup = SceneManager.CreateScene("DodgeFlowTests Cleanup");
                SceneManager.SetActiveScene(cleanup);
                yield return SceneManager.UnloadSceneAsync(active);
            }
            else yield return null;
        }

        [UnityTest]
        public IEnumerator DirectPlayerDodge_UsesMovementIntentAndHasANarrowDamageWindow()
        {
            GameplayInput.ResetForTests();
            Time.timeScale = 1;
            var cameraObject = MainCamera();
            var fixture = new EntityFixture(false);
            try
            {
                fixture.Entity.SetController(fixture.Player);
                GameplayInput.SetMovement(Vector2.right);
                fixture.Player.Tick();
                GameplayInput.ClearMovement();
                var start = fixture.Entity.transform.position;

                Assert.That(fixture.Player.Dodge(), Is.True);
                Assert.That(fixture.Entity.IsDodging, Is.True);
                Assert.That(fixture.Entity.Health.IsDamageImmune, Is.True);
                Assert.That(fixture.Entity.Health.DamageImmunityRemaining, Is.LessThanOrEqualTo(CombatEntity.DodgeImmunityDuration));
                fixture.Entity.Health.TakeDamage(new DamageInfo(25, null, fixture.Entity.transform.position), 0);
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(100));

                yield return new WaitForSeconds(CombatEntity.DodgeImmunityDuration + .08f);
                var displacement = fixture.Entity.transform.position - start;
                Assert.That(fixture.Entity.IsDodging, Is.False);
                Assert.That(fixture.Entity.Health.IsDamageImmune, Is.False);
                Assert.That(displacement.x, Is.GreaterThan(2.4f).And.LessThanOrEqualTo(CombatEntity.DodgeDistance + .05f));
                Assert.That(Mathf.Abs(displacement.z), Is.LessThan(.05f));
                fixture.Entity.Health.TakeDamage(new DamageInfo(25, null, fixture.Entity.transform.position), 0);
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(75));
                Assert.That(fixture.Player.Dodge(), Is.False, "Cooldown must reject an immediate second dodge.");

                yield return new WaitForSeconds(CombatEntity.DodgeCooldown);
                Assert.That(fixture.Player.Dodge(), Is.True);
                GameplayInput.SetTerminalState(true);
                yield return null;
                Assert.That(fixture.Entity.IsDodging, Is.False);
                Assert.That(fixture.Entity.Health.IsDamageImmune, Is.False);
            }
            finally
            {
                GameplayInput.ResetForTests();
                fixture.Dispose();
                Object.Destroy(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator Dodge_RejectsAiRootAndAbilityOverlapAndCleansLifecycleState()
        {
            GameplayInput.ResetForTests();
            Time.timeScale = 1;
            var cameraObject = MainCamera();
            var fixture = new EntityFixture(true);
            var deathFixture = new EntityFixture(false);
            var disableFixture = new EntityFixture(false);
            var trapFixture = new EntityFixture(false);
            try
            {
                fixture.Entity.SetController(fixture.Ai);
                Assert.That(fixture.Entity.TryDodge(Vector3.right), Is.False, "AI control must never gain dodge.");
                fixture.Entity.SetController(fixture.Player);
                fixture.Entity.ApplyRoot(1);
                Assert.That(fixture.Player.Dodge(), Is.False);
                Assert.That(fixture.Player.RootEscapeVisible, Is.True); Assert.That(fixture.Player.RootEscapeProgress, Is.Zero);
                fixture.Entity.BreakRoot();
                GameplayInput.SetTerminalState(true);
                Assert.That(fixture.Player.Dodge(), Is.False);
                GameplayInput.SetTerminalState(false);

                Assert.That(fixture.Entity.TryUse(0, Vector3.forward), Is.True);
                Assert.That(fixture.Player.Dodge(), Is.False, "An active ability must own the action window.");
                yield return new WaitForSeconds(.35f);

                fixture.Entity.transform.rotation = Quaternion.LookRotation(Vector3.left);
                Assert.That(fixture.Player.Dodge(), Is.True);
                Assert.That(Vector3.Dot(fixture.Entity.transform.forward, Vector3.left), Is.GreaterThan(.99f), "No movement history must fall back to facing.");
                Assert.That(fixture.Entity.TryUse(0, Vector3.forward), Is.False, "Abilities must not begin during dodge.");
                fixture.Entity.SetController(fixture.Ai);
                Assert.That(fixture.Entity.IsDodging, Is.False);
                Assert.That(fixture.Entity.Health.IsDamageImmune, Is.False);

                deathFixture.Entity.SetController(deathFixture.Player);
                Assert.That(deathFixture.Player.Dodge(), Is.True);
                deathFixture.Entity.SendMessage("OnDeath", SendMessageOptions.RequireReceiver);
                Assert.That(deathFixture.Entity.IsDodging, Is.False);
                Assert.That(deathFixture.Entity.Health.IsDamageImmune, Is.False);
                deathFixture.Entity.Health.TakeDamage(new DamageInfo(1000, null, Vector3.zero), 0);
                Assert.That(deathFixture.Entity.Health.IsDead, Is.True);
                Assert.That(deathFixture.Player.Dodge(), Is.False);

                disableFixture.Entity.SetController(disableFixture.Player);
                Assert.That(disableFixture.Player.Dodge(), Is.True);
                disableFixture.Entity.enabled = false;
                Assert.That(disableFixture.Entity.IsDodging, Is.False);
                Assert.That(disableFixture.Entity.Health.IsDamageImmune, Is.False);

                trapFixture.Entity.SetController(trapFixture.Player);
                Assert.That(trapFixture.Player.Dodge(), Is.True);
                trapFixture.Entity.ApplyRoot(1);
                Assert.That(trapFixture.Entity.IsDodging, Is.False, "A root must stop dodge travel.");
                Assert.That(trapFixture.Entity.Health.IsDamageImmune, Is.True, "Root-before-damage traps must preserve the exact immunity window.");
                trapFixture.Entity.Health.TakeDamage(new DamageInfo(20, null, Vector3.zero), 0);
                Assert.That(trapFixture.Entity.Health.Current, Is.EqualTo(100), "Trap damage inside the dodge window must be ignored.");
            }
            finally
            {
                GameplayInput.ResetForTests();
                fixture.Dispose(); deathFixture.Dispose(); disableFixture.Dispose(); trapFixture.Dispose();
                Object.Destroy(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator RaidAndPossessedDefenderHud_OwnAndLayOutDodgeInBothOrientations()
        {
            GameplayInput.ResetForTests();
            SceneManager.LoadScene("SylvanRealm");
            yield return null;
            yield return null;

            var raidHud = Object.FindFirstObjectByType<RaidHUD>();
            var raidRoot = raidHud.GetComponent<ResponsiveHudRoot>();
            Assert.That(raidHud.DodgeButtonVisible, Is.True);
            Assert.That(raidHud.DodgeButtonInteractable, Is.True);
            AssertUiOwnership(raidHud.DodgeButtonRect);
            raidRoot.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertActionLayout(raidRoot, raidHud.DodgeButtonRect);
            raidRoot.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertActionLayout(raidRoot, raidHud.DodgeButtonRect);
            var hero = GameObject.Find("Blood Knight").GetComponent<CombatEntity>();
            GameObject.Find("DODGE").GetComponent<Button>().onClick.Invoke(); yield return null;
            Assert.That(hero.IsDodging, Is.True); Assert.That(raidHud.DodgeButtonInteractable, Is.False); Assert.That(raidHud.DodgeButtonText, Does.Contain("DODG"));
            hero.ApplyRoot(1); yield return null;
            Assert.That(raidHud.DodgeButtonVisible, Is.True); Assert.That(raidHud.DodgeButtonInteractable, Is.False); Assert.That(raidHud.DodgeButtonText, Does.Contain("ROOTED"));
            hero.BreakRoot(); GameplayInput.SetTerminalState(true); yield return null; Assert.That(raidHud.DodgeButtonVisible, Is.False);

            GameplayInput.SetTerminalState(false);
            SceneManager.LoadScene("DefenderTest");
            yield return null;
            yield return null;
            var defenderHud = Object.FindFirstObjectByType<DefenderHUD>();
            var defenderRoot = defenderHud.GetComponent<ResponsiveHudRoot>();
            Assert.That(defenderHud.DodgeButtonVisible, Is.False);
            var possession = Object.FindFirstObjectByType<PossessionManager>();
            var ent = GameObject.Find("Guardian Ent").GetComponent<CombatEntity>();
            possession.Select(ent); Assert.That(possession.PossessSelected(), Is.True); yield return null;
            Assert.That(defenderHud.DodgeButtonVisible, Is.True); Assert.That(defenderHud.DodgeButtonInteractable, Is.True);
            AssertUiOwnership(defenderHud.DodgeButtonRect);
            defenderRoot.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertActionLayout(defenderRoot, defenderHud.DodgeButtonRect);
            defenderRoot.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertActionLayout(defenderRoot, defenderHud.DodgeButtonRect);
            GameObject.Find("DODGE").GetComponent<Button>().onClick.Invoke(); yield return null; Assert.That(ent.IsDodging, Is.True);
            possession.Release(); yield return null; Assert.That(defenderHud.DodgeButtonVisible, Is.False); Assert.That(ent.IsDodging, Is.False); Assert.That(ent.Health.IsDamageImmune, Is.False);
            possession.Select(ent); Assert.That(possession.PossessSelected(), Is.True); GameplayInput.SetTerminalState(true); yield return null; Assert.That(defenderHud.DodgeButtonVisible, Is.False);
            GameplayInput.SetTerminalState(false); possession.Release(); Time.timeScale = 1; GameplayInput.ResetForTests();
        }

        static GameObject MainCamera()
        {
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camera.tag = "MainCamera";
            return camera;
        }

        static void AssertUiOwnership(RectTransform button)
        {
            var ownership = button.GetComponent<UiPointerOwnership>();
            Assert.That(ownership, Is.Not.Null);
            var pointer = new PointerEventData(EventSystem.current) { pointerId = 906 };
            ownership.OnPointerDown(pointer); Assert.That(GameplayInput.IsUiOwned(pointer.pointerId), Is.True);
            ownership.OnPointerUp(pointer); Assert.That(GameplayInput.IsUiOwned(pointer.pointerId), Is.False);
        }

        static void AssertActionLayout(ResponsiveHudRoot root, RectTransform dodge)
        {
            var reference = root.GetComponent<CanvasScaler>().referenceResolution;
            var dodgeRect = DesignRect(dodge, reference);
            Assert.That(dodgeRect.xMin, Is.GreaterThanOrEqualTo(0)); Assert.That(dodgeRect.yMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(dodgeRect.xMax, Is.LessThanOrEqualTo(reference.x)); Assert.That(dodgeRect.yMax, Is.LessThanOrEqualTo(reference.y));
            foreach (var button in root.GetComponentsInChildren<Button>(false))
            {
                var other = (RectTransform)button.transform;
                if (other == dodge || other.parent != root.transform) continue;
                Assert.That(dodgeRect.Overlaps(DesignRect(other, reference)), Is.False, $"Dodge overlaps {button.name}");
            }
        }

        static Rect DesignRect(RectTransform rect, Vector2 parentSize)
        {
            Assert.That(rect.anchorMin, Is.EqualTo(rect.anchorMax));
            var pivotPoint = Vector2.Scale(rect.anchorMin, parentSize) + rect.anchoredPosition;
            return new Rect(pivotPoint - Vector2.Scale(rect.pivot, rect.sizeDelta), rect.sizeDelta);
        }

        sealed class EntityFixture
        {
            public readonly GameObject Root;
            public readonly CombatEntity Entity;
            public readonly PlayerController Player;
            public readonly CreatureBrain Ai;
            readonly CharacterDefinition definition;
            readonly AbilityDefinition ability;

            public EntityFixture(bool withAbility)
            {
                Root = new GameObject("Dodge Test Entity", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
                definition = ScriptableObject.CreateInstance<CharacterDefinition>();
                definition.DisplayName = "Dodge Test Entity";
                definition.Possessable = true;
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4, AttackSpeed = 1 };
                if (withAbility)
                {
                    ability = ScriptableObject.CreateInstance<AbilityDefinition>();
                    ability.DisplayName = "Test Action"; ability.Kind = AbilityKind.Melee; ability.Windup = .1f; ability.Cooldown = 0; ability.Range = 1; ability.Radius = .1f;
                    definition.Abilities = new[] { ability };
                }
                Entity = Root.GetComponent<CombatEntity>(); Player = Root.GetComponent<PlayerController>(); Ai = Root.GetComponent<CreatureBrain>();
                Entity.Initialize(definition);
            }

            public void Dispose()
            {
                Object.Destroy(Root);
                Object.Destroy(definition);
                if (ability) Object.Destroy(ability);
            }
        }
    }
}
