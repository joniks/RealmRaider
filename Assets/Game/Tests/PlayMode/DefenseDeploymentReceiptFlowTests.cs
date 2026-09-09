using System.Collections;
using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.Possession;
using RealmRaiders.Raid;
using RealmRaiders.Traps;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class DefenseDeploymentReceiptFlowTests
    {
        static int cleanupIndex;

        [UnityTearDown]
        public IEnumerator RemoveLoadedFixtureScene()
        {
            GameplayInput.ResetForTests();
            var active = SceneManager.GetActiveScene();
            if (active.IsValid() && active.isLoaded && active.name == "DefenderTest")
            {
                var cleanup = SceneManager.CreateScene($"Defense Deployment Receipt Cleanup {++cleanupIndex}");
                SceneManager.SetActiveScene(cleanup);
                yield return SceneManager.UnloadSceneAsync(active);
            }
            GameplayInput.ResetForTests();
        }

        [UnityTest]
        public IEnumerator SylvanDeploymentReceipt_UsesActualDeploymentAndCleansEveryOpeningExit()
        {
            var saved = new SavedPreferences();
            try
            {
                var layout = SparseValidLayout();
                FirstPlayableMinute.ResetForTests();
                DefenseLayoutSave.Save(layout);
                SceneManager.LoadScene("DefenderTest");
                yield return null;
                yield return null;

                var hud = Object.FindFirstObjectByType<DefenderHUD>();
                var responsive = hud.GetComponent<ResponsiveHudRoot>();
                var receipt = hud.DeploymentReceipt;
                var possession = Object.FindFirstObjectByType<PossessionManager>();
                var defender = GameObject.Find("Guardian Ent").GetComponent<CombatEntity>();
                var invader = GameObject.Find("Invading Blood Knight").GetComponent<CombatEntity>();
                var defense = Object.FindFirstObjectByType<DefenseManager>();

                Assert.That(receipt, Is.Not.Null);
                Assert.That(receipt.Visible, Is.True);
                Assert.That(receipt.Copy, Is.EqualTo(
                    "DEPLOYED DEFENSE — INVADER → HEART TREE\n" +
                    "ROOT GATE: ROOT TRAP  →  OUTER GUARD: WOLF  →  MID GUARD: GUARDIAN ENT\n" +
                    "INNER ROOT: OPEN  →  HEART GUARD: OPEN  →  HEART TREE"));
                Assert.That(receipt.PieceAtSlot(0), Is.EqualTo(DefensePieceType.Wolf));
                Assert.That(receipt.PieceAtSlot(1), Is.EqualTo(DefensePieceType.Ent));
                Assert.That(receipt.PieceAtSlot(2), Is.EqualTo(DefensePieceType.Empty));
                Assert.That(receipt.PieceAtSlot(3), Is.EqualTo(DefensePieceType.RootTrap));
                Assert.That(receipt.PieceAtSlot(4), Is.EqualTo(DefensePieceType.Empty));
                Assert.That(GameObject.Find("Realm Wolf A"), Is.Not.Null);
                Assert.That(GameObject.Find("Realm Wolf B"), Is.Null, "An OPEN creature slot must not be described by a creature that was never created.");
                Assert.That(Object.FindObjectsByType<RootTrap>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(GameObject.Find("Manual Root Trap").transform.position.z, Is.EqualTo(-7).Within(.001f));
                AssertPresentationOnly(receipt);

                responsive.SetOrientationForTests(PrototypeOrientation.Portrait);
                yield return null;
                AssertLayoutClear(hud);
                responsive.SetOrientationForTests(PrototypeOrientation.Landscape);
                yield return null;
                AssertLayoutClear(hud);

                hud.enabled = false;
                Assert.That(receipt.Visible, Is.False, "Disabling the HUD must clear its opening receipt immediately.");
                hud.enabled = true;
                yield return null;
                Assert.That(receipt.Visible, Is.True, "Re-enabled HUD may resume the still-active normal opening hold.");

                possession.Select(defender);
                Assert.That(possession.PossessSelected(), Is.True);
                Assert.That(receipt.Visible, Is.False, "Possession must clear the receipt synchronously.");
                possession.Release();
                hud.SendMessage("RefreshDeploymentReceipt", SendMessageOptions.RequireReceiver);
                Assert.That(receipt.Visible, Is.False, "Release must not resurrect a dismissed opening receipt.");

                invader.Health.TakeDamage(new DamageInfo(1000, null, invader.transform.position), 0);
                Assert.That(defense.State, Is.EqualTo(DefenseState.DefenderVictory));
                Assert.That(receipt.Visible, Is.False, "A terminal result must keep the receipt hidden.");

                FirstPlayableMinute.ResetForTests();
                DefenseLayoutSave.Save(layout);
                SceneManager.LoadScene("DefenderTest");
                yield return null;
                yield return null;
                hud = Object.FindFirstObjectByType<DefenderHUD>();
                receipt = hud.DeploymentReceipt;
                invader = GameObject.Find("Invading Blood Knight").GetComponent<CombatEntity>();
                var brain = invader.Controller<RaidInvaderBrain>();
                var start = invader.transform.position;
                brain.Configure(new[] { start + Vector3.forward * 5 }, new[] { GameObject.Find("Guardian Ent").GetComponent<CombatEntity>() }, 0);
                invader.SetController(brain);
                brain.Tick();
                hud.SendMessage("RefreshDeploymentReceipt", SendMessageOptions.RequireReceiver);
                Assert.That(brain.IsOpeningHold, Is.False);
                Assert.That(invader.transform.position.z, Is.GreaterThan(start.z), "The fixture must prove the invader has entered its movement phase.");
                Assert.That(receipt.Visible, Is.False, "Invader movement must end the deployment receipt.");

                FirstPlayableMinute.ResetForTests();
                Assert.That(FirstPlayableMinute.TryStart(), Is.True);
                Assert.That(FirstPlayableMinute.TryAcceptChangedBuild(FirstPlayableMinute.CaptureBuildEntry(DefenseLayout.Default()), layout), Is.True);
                DefenseLayoutSave.Save(layout);
                SceneManager.LoadScene("DefenderTest");
                yield return null;
                yield return null;
                hud = Object.FindFirstObjectByType<DefenderHUD>();
                Assert.That(hud.FirstMinuteGuide, Is.Not.Null);
                Assert.That(hud.FirstMinuteGuide.GuideLineVisible, Is.True);
                Assert.That(hud.DeploymentReceipt, Is.Not.Null);
                Assert.That(hud.DeploymentReceipt.Visible, Is.False, "The first-minute guide is the sole opening instructional copy.");
                Assert.That(hud.OpeningCueVisible, Is.False);
            }
            finally { saved.Restore(); }
        }

        static DefenseLayout SparseValidLayout() => new(new[]
        {
            new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf),
            new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Ent),
            new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Empty),
            new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.RootTrap),
            new DefenseSlotLayout(DefenseSlotType.Trap, DefensePieceType.Empty)
        });

        static void AssertPresentationOnly(DefenseDeploymentReceipt receipt)
        {
            Assert.That(receipt.RaycastTarget, Is.False);
            Assert.That(receipt.GetComponentsInChildren<Button>(true), Is.Empty);
            Assert.That(receipt.GetComponentsInChildren<Selectable>(true), Is.Empty);
            Assert.That(receipt.GetComponentsInChildren<UiPointerOwnership>(true), Is.Empty);
            Assert.That(receipt.GetComponentsInChildren<Canvas>(true), Is.Empty);
            Assert.That(receipt.GetComponentsInChildren<EventSystem>(true), Is.Empty);
            Assert.That(receipt.GetComponentsInChildren<AudioListener>(true), Is.Empty);
            foreach (var graphic in receipt.GetComponentsInChildren<Graphic>(true)) Assert.That(graphic.raycastTarget, Is.False, graphic.name);
        }

        static void AssertLayoutClear(DefenderHUD hud)
        {
            Canvas.ForceUpdateCanvases();
            var receipt = hud.DeploymentReceipt;
            var reference = hud.GetComponent<CanvasScaler>().referenceResolution;
            var receiptBounds = DesignRect(receipt.Rect, reference);
            AssertContained(receiptBounds, reference, receipt.Rect.name);
            foreach (var button in hud.GetComponentsInChildren<Button>(false))
                Assert.That(receiptBounds.Overlaps(DesignRect((RectTransform)button.transform, reference)), Is.False, $"Receipt overlaps action {button.name}");
            foreach (var text in hud.GetComponentsInChildren<Text>(false))
            {
                if (text.transform == receipt.transform || text.GetComponentInParent<Button>()) continue;
                Assert.That(receiptBounds.Overlaps(DesignRect(text.rectTransform, reference)), Is.False, $"Receipt overlaps {text.name} text='{text.text}'");
            }
        }

        static void AssertContained(Rect bounds, Vector2 reference, string message)
        {
            Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(0), message);
            Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(0), message);
            Assert.That(bounds.xMax, Is.LessThanOrEqualTo(reference.x), message);
            Assert.That(bounds.yMax, Is.LessThanOrEqualTo(reference.y), message);
        }

        static Rect DesignRect(RectTransform rect, Vector2 parentSize)
        {
            var anchorMin = Vector2.Scale(rect.anchorMin, parentSize);
            var anchorSize = Vector2.Scale(rect.anchorMax - rect.anchorMin, parentSize);
            var size = anchorSize + rect.sizeDelta;
            var pivotPoint = anchorMin + Vector2.Scale(anchorSize, rect.pivot) + rect.anchoredPosition;
            return new Rect(pivotPoint - Vector2.Scale(size, rect.pivot), size);
        }

        sealed class SavedPreferences
        {
            readonly bool hadGuide = PlayerPrefs.HasKey(FirstPlayableMinute.KeyForTests);
            readonly string guide = PlayerPrefs.GetString(FirstPlayableMinute.KeyForTests, string.Empty);
            readonly bool hadLayout = PlayerPrefs.HasKey(DefenseLayoutSave.KeyForTests);
            readonly string layout = PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests, string.Empty);

            public void Restore()
            {
                GameplayInput.ResetForTests();
                if (hadGuide) PlayerPrefs.SetString(FirstPlayableMinute.KeyForTests, guide); else PlayerPrefs.DeleteKey(FirstPlayableMinute.KeyForTests);
                if (hadLayout) PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, layout); else PlayerPrefs.DeleteKey(DefenseLayoutSave.KeyForTests);
                PlayerPrefs.Save();
                FirstPlayableMinute.ResetBuildHandoff();
            }
        }
    }
}
