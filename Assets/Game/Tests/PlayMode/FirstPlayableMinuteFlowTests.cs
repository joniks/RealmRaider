using System.Collections;
using NUnit.Framework;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class FirstPlayableMinuteFlowTests
    {
        [UnityTest]
        public IEnumerator JourneyAloneActivatesGuideAndLegacyBuildRouteDoesNot()
        {
            var saved = new SavedPreferences();
            try
            {
                FirstPlayableMinute.ResetForTests();
                DefenseLayoutSave.Save(DefenseLayout.Default());
                SceneManager.LoadScene("PrototypeHub"); yield return null; yield return null;
                Assert.That(Object.FindFirstObjectByType<HubHUD>().GuideSkipVisible, Is.False);

                GameObject.Find("BUILD SYLVAN").GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("RealmBuild"));
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.NotStarted));
                Assert.That(Object.FindFirstObjectByType<FirstPlayableMinuteBuildGuide>(), Is.Null);

                SceneManager.LoadScene("PrototypeHub"); yield return null; yield return null;
                GameObject.Find("START SYLVAN JOURNEY").GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                var build = Object.FindFirstObjectByType<BuildHUD>();
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("RealmBuild"));
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Active));
                Assert.That(build.GuideStep, Is.EqualTo(BuildGuideStep.Choose));
                Assert.That(build.GuideText, Is.EqualTo("CHANGE ONE DEFENSE — TAP A SLOT"));
                Assert.That(build.GuideLineVisible, Is.True);
                AssertSceneSingletons();
            }
            finally
            {
                saved.Restore();
            }
        }

        [UnityTest]
        public IEnumerator ActiveHubSkipOwnsPointerAndPreservesJourneyNavigationWithoutGuide()
        {
            var saved = new SavedPreferences();
            try
            {
                FirstPlayableMinute.ResetForTests(); FirstPlayableMinute.TryStart();
                SceneManager.LoadScene("PrototypeHub"); yield return null; yield return null;
                var hub = Object.FindFirstObjectByType<HubHUD>(); var skip = GameObject.Find("SKIP GUIDE").GetComponent<Button>();
                Assert.That(hub.GuideSkipVisible, Is.True);
                AssertOwnedGesture(skip, 1001);
                skip.onClick.Invoke();
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Skipped));
                Assert.That(hub.GuideSkipVisible, Is.False);

                GameObject.Find("START SYLVAN JOURNEY").GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("RealmBuild"));
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Skipped));
                Assert.That(Object.FindFirstObjectByType<FirstPlayableMinuteBuildGuide>(), Is.Null);
                AssertSceneSingletons();
            }
            finally
            {
                saved.Restore();
            }
        }

        [UnityTest]
        public IEnumerator BuildGuideShowsExactStatesDismissesAcrossRotationAndSkipsCleanly()
        {
            var saved = new SavedPreferences();
            try
            {
                FirstPlayableMinute.ResetForTests(); FirstPlayableMinute.TryStart(); DefenseLayoutSave.Save(DefenseLayout.Default());
                SceneManager.LoadScene("RealmBuild"); yield return null; yield return null;
                var hud = Object.FindFirstObjectByType<BuildHUD>(); var responsive = Object.FindFirstObjectByType<ResponsiveHudRoot>();
                var saveImage = GameObject.Find("SAVE & DEFEND").GetComponent<Image>();
                var originalColor = saveImage.color; var originalSprite = saveImage.sprite; var originalType = saveImage.type;
                Assert.That(hud.GuideStep, Is.EqualTo(BuildGuideStep.Choose));
                Assert.That(hud.GuideText, Is.EqualTo("CHANGE ONE DEFENSE — TAP A SLOT"));
                Assert.That(hud.GuideLineRaycastTarget, Is.False);
                Assert.That(hud.GuideEmphasisVisible, Is.True);

                responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertGuideLayoutClear(hud);
                var dismiss = GameObject.Find("DISMISS").GetComponent<Button>();
                AssertOwnedGesture(dismiss, 1002); dismiss.onClick.Invoke();
                Assert.That(hud.GuideLineVisible, Is.False); Assert.That(hud.GuideEmphasisVisible, Is.False); Assert.That(hud.GuideSkipVisible, Is.True);
                responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null;
                Assert.That(hud.GuideLineVisible, Is.False, "Dismissal must survive rotation for the same factual step.");

                hud.CycleSlotForTests(2);
                Assert.That(hud.GuideStep, Is.EqualTo(BuildGuideStep.Fix));
                Assert.That(hud.GuideText, Is.EqualTo("KEEP CHOOSING — Place exactly one Ent."));
                Assert.That(hud.GuideLineVisible, Is.True); Assert.That(hud.SaveInteractable, Is.False);
                hud.CycleSlotForTests(1);
                Assert.That(hud.GuideStep, Is.EqualTo(BuildGuideStep.Save));
                Assert.That(hud.GuideText, Is.EqualTo("PLAN READY — SAVE & DEFEND"));
                Assert.That(hud.SaveInteractable, Is.True); Assert.That(hud.GuideEmphasisVisible, Is.True);
                AssertGuideLayoutClear(hud);
                Assert.That(saveImage.color, Is.EqualTo(originalColor)); Assert.That(saveImage.sprite, Is.SameAs(originalSprite)); Assert.That(saveImage.type, Is.EqualTo(originalType));

                var skip = GameObject.Find("SKIP GUIDE").GetComponent<Button>();
                AssertOwnedGesture(skip, 1003); skip.onClick.Invoke();
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Skipped));
                Assert.That(hud.GuideStep, Is.EqualTo(BuildGuideStep.Hidden));
                Assert.That(hud.GuideLineVisible, Is.False); Assert.That(hud.GuideSkipVisible, Is.False); Assert.That(hud.GuideEmphasisVisible, Is.False);
                Assert.That(hud.SaveInteractable, Is.True, "Guide removal must not change authoritative BUILD validity.");
                Assert.That(saveImage.color, Is.EqualTo(originalColor)); Assert.That(saveImage.sprite, Is.SameAs(originalSprite)); Assert.That(saveImage.type, Is.EqualTo(originalType));
                AssertSceneSingletons();
            }
            finally
            {
                saved.Restore();
            }
        }

        [UnityTest]
        public IEnumerator SaveAuthoritySetsHandoffOnlyForActiveChangedValidPlan()
        {
            var saved = new SavedPreferences();
            try
            {
                FirstPlayableMinute.ResetForTests(); FirstPlayableMinute.TryStart(); DefenseLayoutSave.Save(DefenseLayout.Default());
                SceneManager.LoadScene("RealmBuild"); yield return null; yield return null;
                GameObject.Find("SAVE & DEFEND").GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("DefenderTest"));
                Assert.That(FirstPlayableMinute.ChangedBuildAcceptedForSession, Is.False, "An unchanged valid save is not the BUILD proof.");

                SceneManager.LoadScene("RealmBuild"); yield return null; yield return null;
                var invalidHud = Object.FindFirstObjectByType<BuildHUD>(); invalidHud.CycleSlotForTests(2);
                GameObject.Find("SAVE & DEFEND").GetComponent<Button>().onClick.Invoke();
                yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("RealmBuild"));
                Assert.That(FirstPlayableMinute.ChangedBuildAcceptedForSession, Is.False);

                SceneManager.LoadScene("RealmBuild"); yield return null; yield return null;
                var changedHud = Object.FindFirstObjectByType<BuildHUD>(); changedHud.CycleSlotForTests(2); changedHud.CycleSlotForTests(1);
                GameObject.Find("SAVE & DEFEND").GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("DefenderTest"));
                Assert.That(FirstPlayableMinute.ChangedBuildAcceptedForSession, Is.False, "Defender initialization consumes the one-shot BUILD handoff.");
                Assert.That(FirstPlayableMinute.DefenseSessionEligibleForTests, Is.True);
                Assert.That(FirstPlayableMinute.DefenseSceneActiveForTests, Is.True);
                Assert.That(Object.FindFirstObjectByType<FirstPlayableMinuteDefenseGuide>(), Is.Not.Null);
                var savedLayout = DefenseLayoutSave.Load();
                Assert.That(savedLayout.Slots[1].Piece, Is.EqualTo(DefensePieceType.Ent));
                Assert.That(savedLayout.Slots[2].Piece, Is.EqualTo(DefensePieceType.Empty));

                SceneManager.LoadScene("RealmBuild"); yield return null; yield return null;
                Assert.That(FirstPlayableMinute.ChangedBuildAcceptedForSession, Is.False, "A BUILD reload starts a fresh scene-local comparison and handoff.");
            }
            finally
            {
                saved.Restore();
            }
        }

        static void AssertOwnedGesture(Button button, int pointerId)
        {
            var ownership = button.GetComponent<UiPointerOwnership>();
            Assert.That(ownership, Is.Not.Null);
            var pointer = new PointerEventData(EventSystem.current) { pointerId = pointerId };
            ownership.OnPointerDown(pointer); Assert.That(GameplayInput.IsUiOwned(pointerId), Is.True);
            ownership.OnPointerUp(pointer); Assert.That(GameplayInput.IsUiOwned(pointerId), Is.False);
        }

        static void AssertGuideLayoutClear(BuildHUD hud)
        {
            var reference = Object.FindFirstObjectByType<CanvasScaler>().referenceResolution;
            var controls = new[] { hud.GuideDismissRect, hud.GuideSkipRect };
            foreach (var control in controls)
            {
                if (!control || !control.gameObject.activeSelf) continue;
                var bounds = DesignRect(control, reference);
                AssertContained(bounds, reference, control.name);
                foreach (var button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
                {
                    var other = (RectTransform)button.transform;
                    if (other == control) continue;
                    Assert.That(bounds.Overlaps(DesignRect(other, reference)), Is.False, $"{control.name} overlaps {other.name}");
                }
                foreach (var label in Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
                {
                    if (label.GetComponentInParent<Button>()) continue;
                    Assert.That(bounds.Overlaps(DesignRect(label.rectTransform, reference)), Is.False, $"{control.name} overlaps {label.name}");
                }
            }
            if (hud.GuideEmphasisVisible) AssertContained(DesignRect(hud.GuideEmphasisRect, reference), reference, hud.GuideEmphasisRect.name);
        }

        static Rect DesignRect(RectTransform rect, Vector2 reference)
        {
            Assert.That(rect.anchorMin, Is.EqualTo(rect.anchorMax));
            var pivotPoint = Vector2.Scale(rect.anchorMin, reference) + rect.anchoredPosition;
            return new Rect(pivotPoint - Vector2.Scale(rect.pivot, rect.sizeDelta), rect.sizeDelta);
        }

        static void AssertContained(Rect rect, Vector2 reference, string name)
        {
            Assert.That(rect.xMin, Is.GreaterThanOrEqualTo(0), name); Assert.That(rect.yMin, Is.GreaterThanOrEqualTo(0), name);
            Assert.That(rect.xMax, Is.LessThanOrEqualTo(reference.x), name); Assert.That(rect.yMax, Is.LessThanOrEqualTo(reference.y), name);
        }

        static void AssertSceneSingletons()
        {
            Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
        }

        sealed class SavedPreferences
        {
            readonly bool hadGuide = PlayerPrefs.HasKey(FirstPlayableMinute.KeyForTests);
            readonly string guide = PlayerPrefs.GetString(FirstPlayableMinute.KeyForTests, string.Empty);
            readonly bool hadLayout = PlayerPrefs.HasKey(DefenseLayoutSave.KeyForTests);
            readonly string layout = PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests, string.Empty);
            readonly string realm = PrototypeSave.SelectedRealm;

            public void Restore()
            {
                if (hadGuide) PlayerPrefs.SetString(FirstPlayableMinute.KeyForTests, guide); else PlayerPrefs.DeleteKey(FirstPlayableMinute.KeyForTests);
                if (hadLayout) PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, layout); else PlayerPrefs.DeleteKey(DefenseLayoutSave.KeyForTests);
                PlayerPrefs.Save(); PrototypeSave.SelectRealm(realm); FirstPlayableMinute.ResetBuildHandoff(); GameplayInput.ResetForTests();
            }
        }
    }
}
