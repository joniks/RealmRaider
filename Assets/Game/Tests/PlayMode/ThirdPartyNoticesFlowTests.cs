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
    public sealed class ThirdPartyNoticesFlowTests
    {
        [UnityTest]
        public IEnumerator NoticesEntryExistsForEveryGuideStatusWithoutMutatingIt()
        {
            var saved = new SavedState();
            try
            {
                foreach (var expected in new[] { FirstPlayableMinuteStatus.NotStarted, FirstPlayableMinuteStatus.Active, FirstPlayableMinuteStatus.Skipped, FirstPlayableMinuteStatus.Completed })
                {
                    SetGuideStatus(expected);
                    SceneManager.LoadScene("PrototypeHub"); yield return null; yield return null;
                    var hub = Object.FindFirstObjectByType<HubHUD>();
                    Assert.That(hub.NoticesButton, Is.Not.Null);
                    Assert.That(hub.NoticesButton.gameObject.activeInHierarchy, Is.True);
                    Assert.That(Object.FindObjectsByType<ThirdPartyNoticesPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(1));
                    Assert.That(hub.NoticesPanel.IsOpen, Is.False);
                    Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(expected));
                }
            }
            finally { saved.Restore(); }
        }

        [UnityTest]
        public IEnumerator HubNoticesAreOfflineModalReflowableAndStateNeutral()
        {
            var saved = new SavedState();
            try
            {
                FirstPlayableMinute.ResetForTests(); FirstPlayableMinute.TryStart();
                PrototypeSave.SelectRealm("Infernal"); PrototypeSave.SetOrientation("Portrait"); PrototypeSave.SetControlStyle("Joystick");
                var status = FirstPlayableMinute.Load(); var realm = PrototypeSave.SelectedRealm; var orientation = PrototypeSave.OrientationPreference; var control = PrototypeSave.ControlStylePreference; var stores = RealmProgress.StoreCopy();
                SceneManager.LoadScene("PrototypeHub"); yield return null; yield return null;

                var hub = Object.FindFirstObjectByType<HubHUD>(); var panel = hub.NoticesPanel; var responsive = hub.GetComponent<ResponsiveHudRoot>();
                Assert.That(panel, Is.Not.Null); Assert.That(panel.IsOpen, Is.False);
                Assert.That(CountButtonsNamed("THIRD-PARTY NOTICES"), Is.EqualTo(1));
                var canvasCount = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
                var eventSystemCount = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
                var listenerCount = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
                AssertOwnership(hub.NoticesButton.GetComponent<UiPointerOwnership>(), 3100);

                hub.NoticesButton.onClick.Invoke(); yield return null;
                Assert.That(panel.IsOpen, Is.True); Assert.That(panel.transform.parent, Is.EqualTo(hub.transform));
                Assert.That(Object.FindObjectsByType<ThirdPartyNoticesPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(panel.VerticalNormalizedPosition, Is.EqualTo(1).Within(.01f));
                Assert.That(panel.BodyCopy, Does.StartWith("REQUIRED ATTRIBUTION").And.Contain(ThirdPartyNoticeCatalogue.RequiredCredit).And.Contain(ThirdPartyNoticeCatalogue.RequiredSourceUrl).And.Contain(ThirdPartyNoticeCatalogue.CcByUrl));
                Assert.That(panel.BodyCopy, Does.Contain("UI Pack by Kenney — CC0 1.0 Universal.").And.Contain("Interface Sounds by Kenney — CC0 1.0 Universal.").And.Contain(ThirdPartyNoticeCatalogue.Cc0Url));
                Assert.That(panel.BodyCopy.IndexOf("REQUIRED ATTRIBUTION", System.StringComparison.Ordinal), Is.LessThan(panel.BodyCopy.IndexOf("VOLUNTARY PROVENANCE CREDITS", System.StringComparison.Ordinal)));
                Assert.That(panel.BodyCopy.ToLowerInvariant(), Does.Not.Contain("quaternius"));
                Assert.That(panel.BodyText.horizontalOverflow, Is.EqualTo(HorizontalWrapMode.Wrap));
                Assert.That(panel.BodyText.raycastTarget, Is.False);
                Assert.That(panel.ViewportRect.GetComponent<Mask>(), Is.Not.Null);
                Assert.That(panel.BlockerRaycastTarget, Is.True);
                Assert.That(panel.ScrollbarVisible, Is.EqualTo(panel.CanScroll));
                Assert.That(GameObject.Find("START SYLVAN JOURNEY").GetComponent<Button>().interactable, Is.False);
                Assert.That(GameObject.Find("SKIP GUIDE").GetComponent<Button>().interactable, Is.False);
                Assert.That(panel.CloseButton.interactable, Is.True);
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(panel.CloseButton.gameObject));
                AssertOwnership(GameObject.Find("Third-Party Notices Blocker").GetComponent<ThirdPartyNoticePointerOwnership>(), 3101);
                AssertOwnership(panel.ScrollRectTransform.GetComponent<ThirdPartyNoticePointerOwnership>(), 3102);
                AssertOwnership(panel.ScrollbarRect.GetComponent<ThirdPartyNoticePointerOwnership>(), 3103);
                AssertOwnership(panel.CloseButton.GetComponent<ThirdPartyNoticePointerOwnership>(), 3104);

                responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; Canvas.ForceUpdateCanvases(); AssertLayoutInsideSafeRoot(hub, panel);
                var requestedScroll = panel.CanScroll ? .37f : 1;
                var titleBefore = WorldRect(panel.TitleRect); var closeBefore = WorldRect(panel.CloseRect);
                panel.VerticalNormalizedPosition = requestedScroll; yield return null;
                AssertRectEqual(WorldRect(panel.TitleRect), titleBefore, "Scrolling moved the fixed title.");
                AssertRectEqual(WorldRect(panel.CloseRect), closeBefore, "Scrolling moved the fixed close button.");
                responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; Canvas.ForceUpdateCanvases();
                Assert.That(panel.IsOpen, Is.True); AssertLayoutInsideSafeRoot(hub, panel);
                Assert.That(panel.VerticalNormalizedPosition, Is.EqualTo(panel.CanScroll ? requestedScroll : 1).Within(.06f));

                panel.CloseButton.onClick.Invoke(); yield return null;
                Assert.That(panel.IsOpen, Is.False); Assert.That(hub.NoticesButton.interactable, Is.True);
                Assert.That(GameObject.Find("START SYLVAN JOURNEY").GetComponent<Button>().interactable, Is.True);
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(hub.NoticesButton.gameObject));
                AssertHubState(status, realm, orientation, control, stores);
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("PrototypeHub"));

                hub.NoticesButton.onClick.Invoke(); yield return null;
                Assert.That(panel.VerticalNormalizedPosition, Is.EqualTo(1).Within(.01f), "Reopening starts at the mandatory attribution.");
                Assert.That(panel.TryCloseFromBack(), Is.True); yield return null;
                Assert.That(panel.TryCloseFromBack(), Is.False);
                AssertHubState(status, realm, orientation, control, stores);
                hub.NoticesButton.onClick.Invoke(); yield return null; panel.Close(); yield return null;
                Assert.That(Object.FindObjectsByType<ThirdPartyNoticesPanel>(FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(canvasCount));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(eventSystemCount));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(listenerCount));

                GameObject.Find("START SYLVAN JOURNEY").GetComponent<Button>().onClick.Invoke(); yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(HubHUD.JourneyScene));
            }
            finally { saved.Restore(); }
        }

        [UnityTest]
        public IEnumerator FallbackRemainsClosableAndDisableReleasesOwnedGesture()
        {
            var saved = new SavedState();
            try
            {
                FirstPlayableMinute.ResetForTests();
                SceneManager.LoadScene("PrototypeHub"); yield return null; yield return null;
                var hub = Object.FindFirstObjectByType<HubHUD>(); var panel = hub.NoticesPanel;
                var warnings = 0; panel.SetCatalogueForTests(ThirdPartyNoticeCatalogue.Create(null, _ => warnings++));
                hub.NoticesButton.onClick.Invoke(); yield return null;
                Assert.That(warnings, Is.EqualTo(1)); Assert.That(panel.UsesFallback, Is.True);
                Assert.That(panel.BodyCopy, Is.EqualTo(ThirdPartyNoticeCatalogue.FallbackCopy));
                Assert.That(panel.CloseButton.gameObject.activeInHierarchy, Is.True);

                var ownership = panel.ScrollRectTransform.GetComponent<ThirdPartyNoticePointerOwnership>();
                var pointer = new PointerEventData(EventSystem.current) { pointerId = 3201 };
                ownership.OnPointerDown(pointer); Assert.That(GameplayInput.IsUiOwned(pointer.pointerId), Is.True);
                panel.gameObject.SetActive(false); yield return null;
                Assert.That(GameplayInput.IsUiOwned(pointer.pointerId), Is.False);
                Assert.That(panel.IsOpen, Is.False);
                Assert.That(hub.NoticesButton.interactable, Is.True);
                Assert.That(GameObject.Find("START SYLVAN JOURNEY").GetComponent<Button>().interactable, Is.True);
                Assert.That(EventSystem.current.currentSelectedGameObject, Is.EqualTo(hub.NoticesButton.gameObject));
            }
            finally { saved.Restore(); }
        }

        static void AssertOwnership(ThirdPartyNoticePointerOwnership ownership, int pointerId)
        {
            Assert.That(ownership, Is.Not.Null);
            var pointer = new PointerEventData(EventSystem.current) { pointerId = pointerId };
            ownership.OnPointerDown(pointer); Assert.That(GameplayInput.IsUiOwned(pointerId), Is.True);
            ownership.OnPointerUp(pointer); Assert.That(GameplayInput.IsUiOwned(pointerId), Is.False);
        }

        static void AssertOwnership(UiPointerOwnership ownership, int pointerId)
        {
            Assert.That(ownership, Is.Not.Null);
            var pointer = new PointerEventData(EventSystem.current) { pointerId = pointerId };
            ownership.OnPointerDown(pointer); Assert.That(GameplayInput.IsUiOwned(pointerId), Is.True);
            ownership.OnPointerUp(pointer); Assert.That(GameplayInput.IsUiOwned(pointerId), Is.False);
        }

        static int CountButtonsNamed(string name)
        {
            var count = 0;
            foreach (var button in Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None)) if (button.name == name) count++;
            return count;
        }

        static void AssertLayoutInsideSafeRoot(HubHUD hub, ThirdPartyNoticesPanel panel)
        {
            var root = WorldRect((RectTransform)hub.transform); var surface = WorldRect(panel.SurfaceRect);
            AssertContained(surface, root, "notice surface");
            AssertContained(WorldRect(panel.TitleRect), surface, "title");
            AssertContained(WorldRect(panel.CloseRect), surface, "close");
            AssertContained(WorldRect(panel.ScrollRectTransform), surface, "scroll body");
            AssertNoOverlap(panel.TitleRect, panel.CloseRect, "title/close");
            AssertNoOverlap(panel.TitleRect, panel.ScrollRectTransform, "title/body");
            AssertNoOverlap(panel.CloseRect, panel.ScrollRectTransform, "close/body");
            AssertNoOverlap(panel.ViewportRect, panel.ScrollbarRect, "body/scrollbar");
        }

        static void AssertHubState(FirstPlayableMinuteStatus status, string realm, string orientation, string control, string stores)
        {
            Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(status));
            Assert.That(PrototypeSave.SelectedRealm, Is.EqualTo(realm)); Assert.That(PrototypeSave.OrientationPreference, Is.EqualTo(orientation)); Assert.That(PrototypeSave.ControlStylePreference, Is.EqualTo(control));
            Assert.That(RealmProgress.StoreCopy(), Is.EqualTo(stores));
        }

        static void SetGuideStatus(FirstPlayableMinuteStatus status)
        {
            FirstPlayableMinute.ResetForTests();
            if (status == FirstPlayableMinuteStatus.Active) FirstPlayableMinute.TryStart();
            else if (status == FirstPlayableMinuteStatus.Skipped) FirstPlayableMinute.Skip();
            else if (status == FirstPlayableMinuteStatus.Completed) { FirstPlayableMinute.TryStart(); FirstPlayableMinute.TryComplete(); }
        }

        static Rect WorldRect(RectTransform rect) { var corners = new Vector3[4]; rect.GetWorldCorners(corners); return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y); }
        static void AssertRectEqual(Rect actual, Rect expected, string message) { Assert.That(actual.xMin, Is.EqualTo(expected.xMin).Within(.01f), message); Assert.That(actual.yMin, Is.EqualTo(expected.yMin).Within(.01f), message); Assert.That(actual.xMax, Is.EqualTo(expected.xMax).Within(.01f), message); Assert.That(actual.yMax, Is.EqualTo(expected.yMax).Within(.01f), message); }
        static void AssertContained(Rect inner, Rect outer, string name) { Assert.That(inner.xMin, Is.GreaterThanOrEqualTo(outer.xMin), name); Assert.That(inner.yMin, Is.GreaterThanOrEqualTo(outer.yMin), name); Assert.That(inner.xMax, Is.LessThanOrEqualTo(outer.xMax), name); Assert.That(inner.yMax, Is.LessThanOrEqualTo(outer.yMax), name); }
        static void AssertNoOverlap(RectTransform a, RectTransform b, string name) => Assert.That(WorldRect(a).Overlaps(WorldRect(b)), Is.False, name);

        sealed class SavedState
        {
            readonly bool hadGuide = PlayerPrefs.HasKey(FirstPlayableMinute.KeyForTests);
            readonly string guide = PlayerPrefs.GetString(FirstPlayableMinute.KeyForTests, string.Empty);
            readonly string realm = PrototypeSave.SelectedRealm;
            readonly string orientation = PrototypeSave.OrientationPreference;
            readonly string control = PrototypeSave.ControlStylePreference;

            public void Restore()
            {
                GameplayInput.ResetForTests();
                if (hadGuide) PlayerPrefs.SetString(FirstPlayableMinute.KeyForTests, guide); else PlayerPrefs.DeleteKey(FirstPlayableMinute.KeyForTests);
                PlayerPrefs.Save(); FirstPlayableMinute.ResetBuildHandoff();
                PrototypeSave.SelectRealm(realm); PrototypeSave.SetOrientation(orientation); PrototypeSave.SetControlStyle(control);
            }
        }
    }
}
