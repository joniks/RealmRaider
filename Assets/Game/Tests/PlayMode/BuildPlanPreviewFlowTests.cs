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
    public sealed class BuildPlanPreviewFlowTests
    {
        [UnityTearDown]
        public IEnumerator RemoveRealmBuildScene()
        {
            GameplayInput.ResetForTests();
            var buildScene = SceneManager.GetSceneByName("RealmBuild");
            if (!buildScene.IsValid() || !buildScene.isLoaded) yield break;

            var cleanupScene = SceneManager.CreateScene("Build Plan Preview Cleanup");
            SceneManager.SetActiveScene(cleanupScene);
            yield return SceneManager.UnloadSceneAsync(buildScene);
            GameplayInput.ResetForTests();
        }

        [UnityTest]
        public IEnumerator RealmBuildPreviewTracksRealCycleAndStaysClearInBothLayouts()
        {
            var saved = new SavedPreferences();
            try
            {
                FirstPlayableMinute.ResetForTests();
                FirstPlayableMinute.TryStart();
                DefenseLayoutSave.Save(DefenseLayout.Default());
                SceneManager.LoadScene("RealmBuild");
                yield return null;
                yield return null;

                var hud = Object.FindFirstObjectByType<BuildHUD>();
                var preview = Object.FindFirstObjectByType<BuildPlanPreview>();
                var responsive = Object.FindFirstObjectByType<ResponsiveHudRoot>();
                Assert.That(hud, Is.Not.Null);
                Assert.That(preview, Is.Not.Null);
                Assert.That(responsive, Is.Not.Null);
                Assert.That(Object.FindObjectsByType<BuildPlanPreview>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));

                Assert.That(new[] { preview.PieceAt(0), preview.PieceAt(1), preview.PieceAt(2), preview.PieceAt(3), preview.PieceAt(4) }, Is.EqualTo(new[]
                {
                    DefensePieceType.RootTrap,
                    DefensePieceType.Wolf,
                    DefensePieceType.Wolf,
                    DefensePieceType.Empty,
                    DefensePieceType.Ent
                }));
                AssertPreviewIsPresentationOnly(preview);

                var rootBeforeCycle = preview.RootRect;
                var firstNodeBeforeCycle = preview.NodeRectAt(0);
                hud.CycleSlotForTests(3);
                Assert.That(preview.RootRect, Is.SameAs(rootBeforeCycle));
                Assert.That(preview.NodeRectAt(0), Is.SameAs(firstNodeBeforeCycle));
                Assert.That(preview.PieceAt(0), Is.EqualTo(DefensePieceType.Empty));
                Assert.That(preview.NodeTextAt(0), Does.Contain("ROOT GATE\nOPEN\nUNASSIGNED"));
                Assert.That(hud.DefensePlanText, Does.Contain("ROOT GATE: OPEN"));

                hud.CycleSlotForTests(4);
                Assert.That(preview.PieceAt(3), Is.EqualTo(DefensePieceType.RootTrap));
                Assert.That(preview.NodeTextAt(3), Does.Contain("INNER ROOT\nROOT TRAP\nMANUAL HOLD TRAP"));
                Assert.That(hud.DefensePlanText, Does.Contain("INNER ROOT: ROOT TRAP"));
                Assert.That(hud.SaveInteractable, Is.True);

                responsive.SetOrientationForTests(PrototypeOrientation.Portrait);
                yield return null;
                AssertPreviewLayoutClear(preview);
                responsive.SetOrientationForTests(PrototypeOrientation.Landscape);
                yield return null;
                AssertPreviewLayoutClear(preview);
            }
            finally { saved.Restore(); }
        }

        static void AssertPreviewIsPresentationOnly(BuildPlanPreview preview)
        {
            Assert.That(preview.GetComponentsInChildren<Image>(true), Has.Length.EqualTo(7));
            Assert.That(preview.GetComponentsInChildren<Text>(true), Has.Length.EqualTo(6));
            Assert.That(preview.GetComponentsInChildren<Button>(true), Is.Empty);
            Assert.That(preview.GetComponentsInChildren<UiPointerOwnership>(true), Is.Empty);
            Assert.That(preview.GetComponentsInChildren<Canvas>(true), Is.Empty);
            Assert.That(preview.GetComponentsInChildren<EventSystem>(true), Is.Empty);
            Assert.That(preview.GetComponentsInChildren<AudioListener>(true), Is.Empty);
            foreach (var graphic in preview.GetComponentsInChildren<Graphic>(true)) Assert.That(graphic.raycastTarget, Is.False, graphic.name);
        }

        static void AssertPreviewLayoutClear(BuildPlanPreview preview)
        {
            Canvas.ForceUpdateCanvases();
            var reference = Object.FindFirstObjectByType<CanvasScaler>().referenceResolution;
            var previewBounds = DesignRect(preview.RootRect, reference);
            AssertContained(previewBounds, reference, preview.RootRect.name);

            foreach (var button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
                Assert.That(previewBounds.Overlaps(DesignRect((RectTransform)button.transform, reference)), Is.False, $"Preview overlaps {button.name}");
            foreach (var text in Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
            {
                if (text.GetComponentInParent<BuildPlanPreview>() || text.GetComponentInParent<Button>()) continue;
                Assert.That(previewBounds.Overlaps(DesignRect(text.rectTransform, reference)), Is.False, $"Preview overlaps {text.name}");
            }

            var itemRects = new Rect[preview.NodeCount + 1];
            for (var index = 0; index < preview.NodeCount; index++) itemRects[index] = LocalRect(preview.NodeRectAt(index));
            itemRects[itemRects.Length - 1] = LocalRect(preview.EndpointRect);
            for (var index = 0; index < itemRects.Length; index++)
            {
                AssertContained(itemRects[index], preview.RootRect.sizeDelta, $"Preview item {index}");
                for (var other = index + 1; other < itemRects.Length; other++)
                    Assert.That(itemRects[index].Overlaps(itemRects[other]), Is.False, $"Preview items overlap: {index}/{other}");
            }
        }

        static Rect DesignRect(RectTransform rect, Vector2 reference)
        {
            Assert.That(rect.anchorMin, Is.EqualTo(rect.anchorMax), $"Expected fixed anchor for {rect.name}");
            var pivotPoint = Vector2.Scale(rect.anchorMin, reference) + rect.anchoredPosition;
            return new Rect(pivotPoint - Vector2.Scale(rect.pivot, rect.sizeDelta), rect.sizeDelta);
        }

        static Rect LocalRect(RectTransform rect) => new(rect.anchoredPosition - Vector2.Scale(rect.pivot, rect.sizeDelta), rect.sizeDelta);

        static void AssertContained(Rect rect, Vector2 bounds, string name)
        {
            Assert.That(rect.xMin, Is.GreaterThanOrEqualTo(0), name);
            Assert.That(rect.yMin, Is.GreaterThanOrEqualTo(0), name);
            Assert.That(rect.xMax, Is.LessThanOrEqualTo(bounds.x), name);
            Assert.That(rect.yMax, Is.LessThanOrEqualTo(bounds.y), name);
        }

        sealed class SavedPreferences
        {
            readonly bool hadGuide = PlayerPrefs.HasKey(FirstPlayableMinute.KeyForTests);
            readonly string guide = PlayerPrefs.GetString(FirstPlayableMinute.KeyForTests, string.Empty);
            readonly bool hadLayout = PlayerPrefs.HasKey(DefenseLayoutSave.KeyForTests);
            readonly string layout = PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests, string.Empty);

            public void Restore()
            {
                if (hadGuide) PlayerPrefs.SetString(FirstPlayableMinute.KeyForTests, guide); else PlayerPrefs.DeleteKey(FirstPlayableMinute.KeyForTests);
                if (hadLayout) PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, layout); else PlayerPrefs.DeleteKey(DefenseLayoutSave.KeyForTests);
                PlayerPrefs.Save();
                FirstPlayableMinute.ResetBuildHandoff();
            }
        }
    }
}
