using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Traps;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class FlameTrapFlowTests
    {
        [UnityTearDown]
        public IEnumerator ResetTransientState()
        {
            GameplayInput.ResetForTests();
            Time.timeScale = 1;
            var active = SceneManager.GetActiveScene();
            if (active.IsValid() && active.name == "InfernalRealm")
            {
                var cleanup = SceneManager.CreateScene("FlameTrapFlowTests Cleanup");
                SceneManager.SetActiveScene(cleanup);
                yield return SceneManager.UnloadSceneAsync(active);
            }
            else yield return null;
        }

        [UnityTest]
        public IEnumerator ManualActivation_AppliesThreeSeparatePulsesWithoutControlEffectsOrOverlap()
        {
            Time.timeScale = 1;
            var fixture = new TrapFixture();
            var pulseTimes = new List<float>();
            var pulseAmounts = new List<float>();
            try
            {
                var startedAt = Time.time;
                fixture.Entity.Health.Damaged += hit => { pulseTimes.Add(Time.time - startedAt); pulseAmounts.Add(hit.Amount); };
                Assert.That(fixture.Trap.TryActivate(), Is.True);
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(92));
                Assert.That(fixture.Trap.BurnPulsesRemaining, Is.EqualTo(2));
                Assert.That(fixture.Entity.IsRooted, Is.False);
                Assert.That(fixture.Trap.State, Is.EqualTo(TrapState.Cooldown));
                Assert.That(fixture.Trap.CooldownDuration, Is.EqualTo(6));
                Assert.That(fixture.Trap.TryActivate(), Is.False, "Cooldown must prevent overlapping burn sequences.");

                yield return new WaitForSeconds(.38f);
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(84));
                Assert.That(fixture.Trap.BurnPulsesRemaining, Is.EqualTo(1));
                Assert.That(fixture.Entity.IsRooted, Is.False);

                yield return new WaitForSeconds(.38f);
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(76));
                Assert.That(fixture.Trap.BurnPulsesRemaining, Is.Zero);
                Assert.That(fixture.Entity.IsRooted, Is.False);
                Assert.That(fixture.Trap.State, Is.EqualTo(TrapState.Cooldown));
                Assert.That(pulseAmounts, Is.EqualTo(new[] { 8f, 8f, 8f }));
                Assert.That(pulseTimes, Has.Count.EqualTo(3));
                Assert.That(pulseTimes[0], Is.LessThan(.03f));
                Assert.That(pulseTimes[1], Is.InRange(.33f, .48f));
                Assert.That(pulseTimes[2], Is.InRange(.68f, .86f));

                Time.timeScale = 20;
                yield return new WaitForSeconds(6.05f);
                yield return null;
                Assert.That(fixture.Trap.State, Is.EqualTo(TrapState.Ready));
            }
            finally
            {
                Time.timeScale = 1;
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator BurnSequence_StopsWhenTargetDiesOrTrapIsDisabled()
        {
            Time.timeScale = 1;
            var deathFixture = new TrapFixture();
            var disableFixture = new TrapFixture();
            try
            {
                Assert.That(deathFixture.Trap.TryActivate(), Is.True);
                Assert.That(deathFixture.Entity.Health.Current, Is.EqualTo(92));
                deathFixture.Entity.Health.TakeDamage(new DamageInfo(1000, null, deathFixture.Entity.transform.position), 0);
                Assert.That(deathFixture.Entity.Health.IsDead, Is.True);
                Assert.That(deathFixture.Trap.BurnPulsesRemaining, Is.Zero);

                Assert.That(disableFixture.Trap.TryActivate(), Is.True);
                Assert.That(disableFixture.Entity.Health.Current, Is.EqualTo(92));
                disableFixture.Trap.enabled = false;
                Assert.That(disableFixture.Trap.BurnPulsesRemaining, Is.Zero);
                yield return new WaitForSeconds(.76f);
                Assert.That(disableFixture.Entity.Health.Current, Is.EqualTo(92));
                Assert.That(deathFixture.Entity.Health.Current, Is.Zero);
            }
            finally
            {
                deathFixture.Dispose();
                disableFixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator DodgeImmunity_IgnoresOnlyThePulseInsideItsWindow()
        {
            GameplayInput.ResetForTests();
            Time.timeScale = 1;
            var fixture = new TrapFixture(100, true);
            try
            {
                fixture.Entity.SetController(fixture.Player);
                Assert.That(fixture.Entity.TryDodge(Vector3.right), Is.True);
                Assert.That(fixture.Trap.TryActivate(), Is.True);
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(100), "The immediate pulse is inside dodge immunity.");
                Assert.That(fixture.Entity.IsRooted, Is.False);

                yield return new WaitForSeconds(.38f);
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(92), "The next pulse lands after dodge immunity expires.");
                Assert.That(fixture.Entity.IsRooted, Is.False);

                yield return new WaitForSeconds(.38f);
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(84));
                Assert.That(fixture.Trap.BurnPulsesRemaining, Is.Zero);
            }
            finally
            {
                GameplayInput.ResetForTests();
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator SylvanRootTrap_BehaviorRemainsUnchanged()
        {
            Time.timeScale = 1;
            var fixture = new TrapFixture();
            var rootObject = new GameObject("Sylvan Root Trap Test", typeof(RootTrap));
            var root = rootObject.GetComponent<RootTrap>();
            root.Automatic = false;
            root.Initialize(fixture.Entity);
            try
            {
                Assert.That(root.TryActivate(), Is.True);
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(88));
                Assert.That(fixture.Entity.IsRooted, Is.True);
                Assert.That(root.RecentlyActivated, Is.True);
                Assert.That(root.CooldownDuration, Is.EqualTo(8));
                yield return new WaitForSeconds(1.15f);
            }
            finally
            {
                Object.Destroy(rootObject);
                fixture.Dispose();
            }
        }

        [UnityTest]
        public IEnumerator InfernalHud_ShowsTruthfulIgnitedStateAndKeepsTrapControlContained()
        {
            GameplayInput.ResetForTests();
            SceneManager.LoadScene("InfernalRealm");
            yield return null;
            yield return null;

            var trap = Object.FindFirstObjectByType<FlameTrap>();
            var hud = Object.FindFirstObjectByType<DefenderHUD>();
            var invader = GameObject.Find("Invading Blood Knight").GetComponent<CombatEntity>();
            var responsive = hud.GetComponent<ResponsiveHudRoot>();
            Assert.That(trap.Automatic, Is.False, "The scene Flame Trap must wait for the Keeper's manual activation.");
            invader.SetController(null);
            invader.transform.position = trap.transform.position;
            Physics.SyncTransforms();
            yield return null;

            Assert.That(hud.TrapButtonInteractable, Is.True);
            Assert.That(hud.TrapStatusRaycastTarget, Is.False);
            var button = hud.TrapButtonRect.GetComponent<Button>();
            Assert.That(button.GetComponent<UiPointerOwnership>(), Is.Not.Null);
            button.onClick.Invoke();
            yield return null;
            Assert.That(hud.TrapStatusText, Is.EqualTo("IGNITED — 2 BURN PULSES REMAIN"));
            Assert.That(hud.TrapButtonInteractable, Is.False);

            responsive.SetOrientationForTests(PrototypeOrientation.Portrait);
            yield return null;
            AssertContained(hud.TrapButtonRect, responsive.GetComponent<CanvasScaler>().referenceResolution);
            responsive.SetOrientationForTests(PrototypeOrientation.Landscape);
            yield return null;
            AssertContained(hud.TrapButtonRect, responsive.GetComponent<CanvasScaler>().referenceResolution);

            var pointer = new PointerEventData(EventSystem.current) { pointerId = 907 };
            var ownership = button.GetComponent<UiPointerOwnership>();
            ownership.OnPointerDown(pointer);
            Assert.That(GameplayInput.IsUiOwned(pointer.pointerId), Is.True);
            ownership.OnPointerUp(pointer);
            Assert.That(GameplayInput.IsUiOwned(pointer.pointerId), Is.False);

            yield return new WaitForSeconds(.76f);
            Assert.That(hud.TrapStatusText, Does.StartWith("FLAME TRAP COOLDOWN —"));
        }

        static void AssertContained(RectTransform rect, Vector2 parentSize)
        {
            Assert.That(rect.anchorMin, Is.EqualTo(rect.anchorMax));
            var pivotPoint = Vector2.Scale(rect.anchorMin, parentSize) + rect.anchoredPosition;
            var bounds = new Rect(pivotPoint - Vector2.Scale(rect.pivot, rect.sizeDelta), rect.sizeDelta);
            Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(bounds.xMax, Is.LessThanOrEqualTo(parentSize.x));
            Assert.That(bounds.yMax, Is.LessThanOrEqualTo(parentSize.y));
        }

        sealed class TrapFixture
        {
            public readonly GameObject Root;
            public readonly CombatEntity Entity;
            public readonly PlayerController Player;
            public readonly FlameTrap Trap;
            readonly CharacterDefinition definition;
            readonly GameObject trapObject;

            public TrapFixture(float maximumHealth = 100, bool withPlayer = false)
            {
                Root = withPlayer
                    ? new GameObject("Flame Trap Target", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController))
                    : new GameObject("Flame Trap Target", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
                definition = ScriptableObject.CreateInstance<CharacterDefinition>();
                definition.DisplayName = "Flame Trap Target";
                definition.Stats = new CombatStats { MaxHealth = maximumHealth, MoveSpeed = 4, AttackSpeed = 1 };
                definition.Abilities = System.Array.Empty<AbilityDefinition>();
                Entity = Root.GetComponent<CombatEntity>();
                Entity.Initialize(definition);
                Player = Root.GetComponent<PlayerController>();

                trapObject = new GameObject("Flame Trap Test", typeof(FlameTrap));
                Trap = trapObject.GetComponent<FlameTrap>();
                Trap.Automatic = false;
                Trap.Initialize(Entity);
            }

            public void Dispose()
            {
                Object.Destroy(trapObject);
                Object.Destroy(Root);
                Object.Destroy(definition);
            }
        }
    }
}
