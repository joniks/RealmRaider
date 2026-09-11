using System.Collections;
using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.Possession;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class FirstPlayableMinuteDefenseFlowTests
    {
        [UnityTest]
        public IEnumerator OrderedSylvanProofCompletesOnFactualVictory()
        {
            var saved = new SavedPreferences();
            try
            {
                PrepareChangedBuildHandoff();
                SceneManager.LoadScene("DefenderTest"); yield return null; yield return null;
                var scene = GuidedScene.Capture(); scene.StopInvader();
                Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Select));
                Assert.That(scene.Guide.GuideText, Is.EqualTo("SELECT — TAP THE ENT"));
                Assert.That(scene.Guide.GuideLineRaycastTarget, Is.False);
                AssertSceneSingletons();
                yield return VerifyPossessableMarkerPresentation(scene);
                yield return CompleteOrderedControlProof(scene, true);

                scene.Invader.Health.TakeDamage(new DamageInfo(1000, null, scene.Invader.transform.position), 0);
                yield return null;
                Assert.That(scene.Defense.State, Is.EqualTo(DefenseState.DefenderVictory));
                Assert.That(scene.Guide.TerminalOutcome, Is.EqualTo(DefenseGuideTerminalOutcome.Completed));
                Assert.That(scene.Guide.GuideText, Is.EqualTo("FIRST DEFENSE COMPLETE — RETURN TO BUILD"));
                Assert.That(scene.Guide.GuideLineRaycastTarget, Is.False);
                Assert.That(scene.Guide.EmphasisTargetName, Is.EqualTo("RETURN TO BUILD"));
                Assert.That(scene.Hud.ResultText, Does.Contain("DEFENDER VICTORY").And.Contain("The invader was destroyed.")
                    .And.Contain("DEFENSE FACTS — INVADER DEFEATED • CORE DANGER").And.Contain("FIRST DEFENSE COMPLETE — RETURN TO BUILD"));
                var frozenResultCopy = scene.Hud.ResultText;
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Completed));
                Assert.That(scene.Guide.SkipVisible, Is.False);
                AssertPossessableMarkerHidden(scene.Guide);
                AssertTerminalGameplayActionsHidden(scene.Hud);
                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; Assert.That(scene.Hud.ResultText, Is.EqualTo(frozenResultCopy)); AssertTerminalGameplayActionsHidden(scene.Hud); AssertGuideLayoutClear(scene.Guide); AssertResultActionsClear(scene.Hud);
                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; Assert.That(scene.Hud.ResultText, Is.EqualTo(frozenResultCopy)); AssertTerminalGameplayActionsHidden(scene.Hud); AssertGuideLayoutClear(scene.Guide); AssertResultActionsClear(scene.Hud);
                AssertSceneSingletons();
            }
            finally { saved.Restore(); }
        }

        [UnityTest]
        public IEnumerator OrderedSylvanProofCompletesOnFactualLoss()
        {
            var saved = new SavedPreferences();
            try
            {
                PrepareChangedBuildHandoff();
                SceneManager.LoadScene("DefenderTest"); yield return null; yield return null;
                var scene = GuidedScene.Capture(); scene.StopInvader();
                yield return CompleteOrderedControlProof(scene, false);

                var core = Object.FindFirstObjectByType<RealmCore>(); core.InteractionDuration = .001f;
                scene.Invader.transform.position = core.transform.position;
                yield return null; yield return null;
                Assert.That(scene.Defense.State, Is.EqualTo(DefenseState.RealmLost));
                Assert.That(scene.Guide.TerminalOutcome, Is.EqualTo(DefenseGuideTerminalOutcome.Completed));
                Assert.That(scene.Guide.GuideText, Is.EqualTo("REALM LOST — RETURN TO BUILD AND ADJUST DEFENCES"));
                Assert.That(scene.Guide.EmphasisTargetName, Is.EqualTo("RETURN TO BUILD"));
                Assert.That(scene.Hud.ResultText, Does.Contain("REALM LOST").And.Contain("Heart Tree was captured.")
                    .And.Contain("DEFENSE FACTS — CORE CAPTURED • INVADER").And.Contain("RETURN TO BUILD AND ADJUST DEFENCES"));
                var frozenResultCopy = scene.Hud.ResultText;
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Completed));
                AssertTerminalGameplayActionsHidden(scene.Hud);
                var completionWrites = FirstPlayableMinute.SuccessfulWritesForTests;
                var progress = RealmProgress.Load();
                var retryAction = GameObject.Find("DEFEND AGAIN").GetComponent<Button>();
                Assert.That(retryAction.gameObject.activeSelf, Is.True, "DEFEND AGAIN remains available as a secondary result action.");
                Assert.That(retryAction.interactable, Is.True);

                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null;
                Assert.That(scene.Hud.ResultText, Is.EqualTo(frozenResultCopy));
                Assert.That(scene.Guide.GuideText, Is.EqualTo("REALM LOST — RETURN TO BUILD AND ADJUST DEFENCES"));
                Assert.That(scene.Guide.EmphasisTargetName, Is.EqualTo("RETURN TO BUILD"));
                AssertTerminalGameplayActionsHidden(scene.Hud); AssertGuideLayoutClear(scene.Guide); AssertResultActionsClear(scene.Hud);
                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null;
                Assert.That(scene.Hud.ResultText, Is.EqualTo(frozenResultCopy));
                Assert.That(scene.Guide.GuideText, Is.EqualTo("REALM LOST — RETURN TO BUILD AND ADJUST DEFENCES"));
                Assert.That(scene.Guide.EmphasisTargetName, Is.EqualTo("RETURN TO BUILD"));
                AssertTerminalGameplayActionsHidden(scene.Hud); AssertGuideLayoutClear(scene.Guide); AssertResultActionsClear(scene.Hud);

                GameObject.Find("RETURN TO BUILD").GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("RealmBuild"));
                Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Build));
                var buildHuds = Object.FindObjectsByType<BuildHUD>(FindObjectsSortMode.None);
                Assert.That(buildHuds, Has.Length.EqualTo(1));
                Assert.That(buildHuds[0].SaveActionText, Is.EqualTo(BuildHUD.SaveAndRaidAction));
                Assert.That(GameObject.Find(BuildHUD.SaveAndRaidAction), Is.Not.Null);
                Assert.That(Object.FindObjectsByType<FirstPlayableMinuteDefenseGuide>(FindObjectsSortMode.None), Is.Empty);
                Assert.That(CountSceneObjectsNamed("First Playable Minute Defense Guide"), Is.Zero);
                Assert.That(CountSceneObjectsNamed("Defense Result"), Is.Zero);
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Completed));
                Assert.That(FirstPlayableMinute.SuccessfulWritesForTests, Is.EqualTo(completionWrites), "Result navigation cannot persist guide completion twice.");
                var afterBuild = RealmProgress.Load();
                Assert.That(afterBuild.Gold, Is.EqualTo(progress.Gold));
                Assert.That(afterBuild.RareMaterials, Is.EqualTo(progress.RareMaterials));
                Assert.That(afterBuild.CompletedRaids, Is.EqualTo(progress.CompletedRaids));
                Assert.That(afterBuild.Victories, Is.EqualTo(progress.Victories));
                Assert.That(afterBuild.GuardianEntVitalityRank, Is.EqualTo(progress.GuardianEntVitalityRank));

                GameObject.Find(BuildHUD.SaveAndRaidAction).GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("SylvanRealm"));
                Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Raid));
                Assert.That(Object.FindObjectsByType<BuildHUD>(FindObjectsSortMode.None), Is.Empty);
                Assert.That(Object.FindObjectsByType<FirstPlayableMinuteDefenseGuide>(FindObjectsSortMode.None), Is.Empty);
                Assert.That(CountSceneObjectsNamed("Defense Result"), Is.Zero);
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Completed));
                Assert.That(FirstPlayableMinute.SuccessfulWritesForTests, Is.EqualTo(completionWrites), "Starting the next raid cannot persist guide completion twice.");
                var inRaid = RealmProgress.Load();
                Assert.That(inRaid.Gold, Is.EqualTo(progress.Gold));
                Assert.That(inRaid.RareMaterials, Is.EqualTo(progress.RareMaterials));
                Assert.That(inRaid.CompletedRaids, Is.EqualTo(progress.CompletedRaids));
                Assert.That(inRaid.Victories, Is.EqualTo(progress.Victories));
                Assert.That(inRaid.GuardianEntVitalityRank, Is.EqualTo(progress.GuardianEntVitalityRank));
            }
            finally { saved.Restore(); }
        }

        [UnityTest]
        public IEnumerator EarlyTerminalRetriesWhileForcedAndControllerLossNeverProveRelease()
        {
            var saved = new SavedPreferences();
            try
            {
                PrepareChangedBuildHandoff();
                SceneManager.LoadScene("DefenderTest"); yield return null; yield return null;
                var first = GuidedScene.Capture(); first.StopInvader();
                var firstMarker = AssertPossessableMarkerVisible(first.Guide);
                GameObject.Find("DISMISS").GetComponent<Button>().onClick.Invoke();
                AssertPossessableMarkerHidden(first.Guide);
                first.Invader.Health.TakeDamage(new DamageInfo(1000, null, first.Invader.transform.position), 0); yield return null;
                Assert.That(first.Guide.Step, Is.EqualTo(DefenseGuideStep.Retry));
                Assert.That(first.Guide.GuideText, Is.EqualTo("TRY THE CONTROL LOOP — DEFEND AGAIN"));
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Active));
                Assert.That(first.Guide.GuideText, Does.Not.Contain("RELEASED EARLY"), "A terminal manager callback cannot blame the player.");
                AssertPossessableMarkerHidden(first.Guide);
                AssertTerminalGameplayActionsHidden(first.Hud);
                yield return ExerciseTerminalCallbacksWithoutReactivation(first);
                first.Responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertTerminalGameplayActionsHidden(first.Hud); AssertGuideLayoutClear(first.Guide);
                first.Responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertTerminalGameplayActionsHidden(first.Hud); AssertGuideLayoutClear(first.Guide);

                GameObject.Find("DEFEND AGAIN").GetComponent<Button>().onClick.Invoke(); yield return null; yield return null;
                Assert.That(!firstMarker, Is.True, "The terminal scene teardown must destroy its guide marker.");
                var retry = GuidedScene.Capture(); retry.StopInvader();
                Assert.That(retry.Guide.Step, Is.EqualTo(DefenseGuideStep.Select));
                AssertGameplayActionsRestoredForRetry(retry.Hud, false);
                var retryMarker = AssertPossessableMarkerVisible(retry.Guide);
                retry.Possession.Select(retry.Defender); AssertGameplayActionsRestoredForRetry(retry.Hud, true); GameObject.Find("POSSESS ENT").GetComponent<Button>().onClick.Invoke(); yield return null;
                AssertPossessableMarkerHidden(retry.Guide);
                Assert.That(retry.Possession.Possessed, Is.SameAs(retry.Defender));
                retry.Possession.Release(true); yield return null;
                Assert.That(retry.Guide.Step, Is.EqualTo(DefenseGuideStep.Select), "Forced release restarts selection and never satisfies explicit Release.");
                yield return new WaitForSecondsRealtime(.8f); yield return null; retry.Guide.RefreshForTests();
                Assert.That(retry.Guide.GuideText, Is.EqualTo("SELECT — TAP THE ENT"), "A forced release must retain neutral Select guidance.");
                Assert.That(AssertPossessableMarkerVisible(retry.Guide), Is.SameAs(retryMarker), "Forced Keeper return must reuse the existing guide marker.");

                retry.Possession.Select(retry.Defender); GameObject.Find("POSSESS ENT").GetComponent<Button>().onClick.Invoke(); yield return null;
                AssertPossessableMarkerHidden(retry.Guide);
                Assert.That(retry.Guide.Step, Is.EqualTo(DefenseGuideStep.Move));
                GameObject.Find("RELEASE").GetComponent<Button>().onClick.Invoke(); yield return null;
                Assert.That(retry.Guide.Step, Is.EqualTo(DefenseGuideStep.Select), "A premature explicit release preserves the factual proof restart.");
                Assert.That(retry.Guide.GuideText, Is.Not.EqualTo(FirstPlayableMinuteDefenseGuide.PrematureReleaseCopy), "The explanation waits for factual Keeper return.");
                AssertPossessableMarkerHidden(retry.Guide);
                yield return new WaitForSecondsRealtime(.8f); yield return null; retry.Guide.RefreshForTests();
                retry.Responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; retry.Guide.RefreshForTests();
                Assert.That(retry.Guide.GuideText, Is.EqualTo(FirstPlayableMinuteDefenseGuide.PrematureReleaseCopy));
                Assert.That(AssertPossessableMarkerVisible(retry.Guide), Is.SameAs(retryMarker));
                AssertGuideLayoutClear(retry.Guide);
                retry.Responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; retry.Guide.RefreshForTests();
                Assert.That(retry.Guide.GuideText, Is.EqualTo(FirstPlayableMinuteDefenseGuide.PrematureReleaseCopy), "Orientation cannot discard the factual explanation.");
                Assert.That(AssertPossessableMarkerVisible(retry.Guide), Is.SameAs(retryMarker));
                AssertGuideLayoutClear(retry.Guide);

                retry.Possession.Select(retry.Defender);
                Assert.That(retry.Guide.Step, Is.EqualTo(DefenseGuideStep.Possess));
                Assert.That(retry.Guide.GuideText, Is.EqualTo("TAKE CONTROL — TAP POSSESS ENT"), "Reselect immediately clears the early-release reason.");
                GameObject.Find("POSSESS ENT").GetComponent<Button>().onClick.Invoke(); yield return null;
                retry.Defender.SetController(retry.Defender.Controller<CreatureBrain>()); yield return null;
                Assert.That(retry.Guide.Step, Is.EqualTo(DefenseGuideStep.Inactive));
                retry.Possession.Release(true); yield return null;
                Assert.That(retry.Guide.Step, Is.EqualTo(DefenseGuideStep.Select), "A later factual manager release permits a fresh attempt without proving Release.");
                Assert.That(retry.Guide.GuideText, Is.Not.EqualTo(FirstPlayableMinuteDefenseGuide.PrematureReleaseCopy), "Controller loss and manager release cannot blame the player.");
                yield return new WaitForSecondsRealtime(.8f); yield return null; retry.Guide.RefreshForTests();
                Assert.That(retry.Guide.GuideText, Is.EqualTo("SELECT — TAP THE ENT"));

                retry.Possession.Select(retry.Defender); GameObject.Find("POSSESS ENT").GetComponent<Button>().onClick.Invoke(); yield return null;
                retry.Hud.DepletePossessionEnergyForTests(); yield return null;
                Assert.That(retry.Possession.Possessed, Is.Null);
                Assert.That(retry.Guide.Step, Is.EqualTo(DefenseGuideStep.Inactive), "Energy depletion cannot prove Release or advertise an impossible retry.");
                Assert.That(retry.Guide.GuideText, Does.Not.Contain("RELEASED EARLY"), "Energy depletion cannot blame the player.");

                var skip = GameObject.Find("SKIP GUIDE").GetComponent<Button>(); AssertOwnedGesture(skip, 2301); skip.onClick.Invoke();
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Skipped));
                Assert.That(retry.Guide.GuideLineVisible, Is.False); Assert.That(retry.Guide.EmphasisVisible, Is.False);
                Assert.That(retry.Guide.SkipVisible, Is.False); Assert.That(retry.Guide.DismissVisible, Is.False);
                AssertPossessableMarkerHidden(retry.Guide);

                var skippedMarker = retry.Guide.PossessableMarkerRect;
                PrepareChangedBuildHandoff();
                SceneManager.LoadScene("DefenderTest"); yield return null; yield return null;
                Assert.That(!skippedMarker, Is.True, "Skipped guide teardown must not orphan its marker.");
                var death = GuidedScene.Capture(); death.StopInvader();
                AssertPossessableMarkerVisible(death.Guide);
                death.Defender.Health.TakeDamage(new DamageInfo(1000, null, death.Defender.transform.position), 0); yield return null;
                AssertPossessableMarkerHidden(death.Guide);
                Assert.That(death.Guide.GuideText, Does.Not.Contain("RELEASED EARLY"), "Defender death cannot blame the player.");
                SceneManager.LoadScene("PrototypeHub"); yield return null;
                Assert.That(CountSceneObjectsNamed(FirstPlayableMinuteDefenseGuide.PossessableMarkerObjectName), Is.Zero, "Scene teardown must leave no guide marker orphan.");
            }
            finally { saved.Restore(); }
        }

        [UnityTest]
        public IEnumerator DirectAndInfernalDefenseRoutesNeverConsumeFreshActiveGuide()
        {
            var saved = new SavedPreferences();
            try
            {
                FirstPlayableMinute.ResetForTests(); FirstPlayableMinute.TryStart(); DefenseLayoutSave.Save(DefenseLayout.Default());
                SceneManager.LoadScene("DefenderTest"); yield return null; yield return null;
                Assert.That(Object.FindFirstObjectByType<FirstPlayableMinuteDefenseGuide>(), Is.Null);

                SceneManager.LoadScene("PrototypeHub"); yield return null;
                PrepareChangedBuildHandoff();
                SceneManager.LoadScene("InfernalRealm"); yield return null; yield return null;
                Assert.That(Object.FindFirstObjectByType<FirstPlayableMinuteDefenseGuide>(), Is.Null);
                Assert.That(FirstPlayableMinute.ChangedBuildAcceptedForSession, Is.True, "Infernal UI must not consume the Sylvan handoff.");
            }
            finally { saved.Restore(); }
        }

        static IEnumerator VerifyPossessableMarkerPresentation(GuidedScene scene)
        {
            var controller = scene.Defender.ActiveController;
            var selected = scene.Possession.Selected;
            var health = scene.Defender.Health.Current;
            var cameraRig = scene.Possession.CameraRig;
            var cameraMode = cameraRig.Mode;
            var cameraTransitioning = cameraRig.IsTransitioning;
            var cameraPosition = cameraRig.transform.position;
            var cameraRotation = cameraRig.transform.rotation;
            var abilities = new AbilityRuntime[scene.Defender.Abilities.Count];
            for (var index = 0; index < abilities.Length; index++) abilities[index] = scene.Defender.Abilities[index];

            scene.Responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null;
            scene.Guide.RefreshForTests();
            var marker = AssertPossessableMarkerVisible(scene.Guide);
            scene.Responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null;
            scene.Guide.RefreshForTests();
            Assert.That(AssertPossessableMarkerVisible(scene.Guide), Is.SameAs(marker), "Orientation changes must reuse one guide marker.");

            Assert.That(scene.Possession.Selected, Is.SameAs(selected), "Marker presentation cannot select a creature.");
            Assert.That(scene.Defender.ActiveController, Is.SameAs(controller), "Marker presentation cannot change controller authority.");
            Assert.That(scene.Defender.Health.Current, Is.EqualTo(health), "Marker presentation cannot mutate health.");
            Assert.That(scene.Defender.Abilities.Count, Is.EqualTo(abilities.Length), "Marker presentation cannot change the ability set.");
            for (var index = 0; index < abilities.Length; index++)
                Assert.That(scene.Defender.Abilities[index], Is.SameAs(abilities[index]), $"Marker presentation replaced ability {index}.");
            Assert.That(cameraRig.Mode, Is.EqualTo(cameraMode), "Marker presentation cannot change camera mode.");
            Assert.That(cameraRig.IsTransitioning, Is.EqualTo(cameraTransitioning), "Marker presentation cannot request a camera transition.");
            Assert.That(cameraRig.transform.position, Is.EqualTo(cameraPosition), "Marker presentation cannot move the camera.");
            Assert.That(cameraRig.transform.rotation, Is.EqualTo(cameraRotation), "Marker presentation cannot rotate the camera.");
        }

        static RectTransform AssertPossessableMarkerVisible(FirstPlayableMinuteDefenseGuide guide)
        {
            var marker = guide.PossessableMarkerRect;
            Assert.That(marker, Is.Not.Null);
            Assert.That(marker.name, Is.EqualTo(FirstPlayableMinuteDefenseGuide.PossessableMarkerObjectName));
            Assert.That(marker.parent, Is.SameAs(guide.transform));
            Assert.That(guide.PossessableMarkerVisible, Is.True);
            Assert.That(marker.gameObject.activeSelf, Is.True);
            var label = marker.GetComponent<Text>();
            Assert.That(label, Is.Not.Null);
            Assert.That(label.text, Is.EqualTo(FirstPlayableMinuteDefenseGuide.PossessableMarkerCopy));
            Assert.That(label.raycastTarget, Is.False);
            Assert.That(marker.GetComponent<Button>(), Is.Null);
            Assert.That(marker.GetComponent<EventTrigger>(), Is.Null);
            Assert.That(marker.GetComponent<UiPointerOwnership>(), Is.Null);
            Assert.That(CountGuideMarkers(guide), Is.EqualTo(1), "The guide must own exactly one reusable marker.");

            var safeBounds = WorldRect((RectTransform)guide.transform);
            var markerBounds = WorldRect(marker);
            Assert.That(markerBounds.xMin, Is.GreaterThanOrEqualTo(safeBounds.xMin - .1f));
            Assert.That(markerBounds.yMin, Is.GreaterThanOrEqualTo(safeBounds.yMin - .1f));
            Assert.That(markerBounds.xMax, Is.LessThanOrEqualTo(safeBounds.xMax + .1f));
            Assert.That(markerBounds.yMax, Is.LessThanOrEqualTo(safeBounds.yMax + .1f));
            return marker;
        }

        static void AssertPossessableMarkerHidden(FirstPlayableMinuteDefenseGuide guide)
        {
            Assert.That(guide.PossessableMarkerRect, Is.Not.Null);
            Assert.That(guide.PossessableMarkerVisible, Is.False);
            Assert.That(guide.PossessableMarkerRect.gameObject.activeSelf, Is.False);
            Assert.That(CountGuideMarkers(guide), Is.EqualTo(1), "A hidden marker must remain owned for factual retry reuse.");
        }

        static int CountGuideMarkers(FirstPlayableMinuteDefenseGuide guide)
        {
            var count = 0;
            foreach (var child in guide.GetComponentsInChildren<RectTransform>(true))
                if (child.name == FirstPlayableMinuteDefenseGuide.PossessableMarkerObjectName) count++;
            return count;
        }

        static int CountSceneObjectsNamed(string objectName)
        {
            var count = 0;
            foreach (var root in SceneManager.GetActiveScene().GetRootGameObjects())
                foreach (var child in root.GetComponentsInChildren<Transform>(true))
                    if (child.name == objectName) count++;
            return count;
        }

        static IEnumerator CompleteOrderedControlProof(GuidedScene scene, bool verifyResponsivePresentation)
        {
            if (verifyResponsivePresentation)
            {
                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null;
                AssertGuideLayoutClear(scene.Guide); AssertOwnedGesture(GameObject.Find("DISMISS").GetComponent<Button>(), 2201);
            }

            SetRootPosition(scene.Defender, new Vector3(0, scene.Defender.transform.position.y, 0));
            var selectMarker = scene.Guide.PossessableMarkerRect;
            scene.Possession.Select(scene.Defender);
            Assert.That(scene.Guide.PossessableMarkerRect, Is.SameAs(selectMarker));
            AssertPossessableMarkerHidden(scene.Guide);
            Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Possess));
            Assert.That(scene.Guide.GuideText, Is.EqualTo("TAKE CONTROL — TAP POSSESS ENT"));
            Assert.That(scene.Guide.EmphasisTargetName, Is.EqualTo("POSSESS ENT"));
            var originalEntity = scene.Defender;
            GameObject.Find("POSSESS ENT").GetComponent<Button>().onClick.Invoke();
            Assert.That(scene.Possession.Possessed, Is.SameAs(originalEntity));
            Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Move));
            Assert.That(scene.Guide.GuideText, Is.Empty, "Movement guidance waits for the real possession camera transition.");
            yield return new WaitForSecondsRealtime(1f); yield return null;

            scene.Responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null;
            scene.Guide.RefreshForTests();
            Assert.That(scene.Guide.GuideText, Is.EqualTo("MOVE — TAP OPEN GROUND"));
            if (verifyResponsivePresentation)
            {
                PrototypeSave.SetControlStyle("Fingertap");
                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; scene.Guide.RefreshForTests();
                Assert.That(scene.Guide.GuideText, Is.EqualTo("MOVE — TAP OPEN GROUND"));
                PrototypeSave.SetControlStyle("Joystick");
                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; scene.Guide.RefreshForTests();
                Assert.That(scene.Guide.GuideText, Is.EqualTo("MOVE — DRAG THE JOYSTICK"));
                Assert.That(scene.Guide.EmphasisTargetName, Is.EqualTo("Landscape Joystick"));
                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; scene.Guide.RefreshForTests();
                Assert.That(scene.Guide.GuideText, Is.EqualTo("MOVE — DRAG THE JOYSTICK"));
                PrototypeSave.SetControlStyle("Contextual");
                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; scene.Guide.RefreshForTests();
                Assert.That(scene.Guide.GuideText, Is.EqualTo("MOVE — TAP OPEN GROUND"));
                GameObject.Find("DISMISS").GetComponent<Button>().onClick.Invoke();
                Assert.That(scene.Guide.GuideLineVisible, Is.False);
                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null;
                Assert.That(scene.Guide.GuideLineVisible, Is.False, "Move dismissal survives orientation and effective-style change.");
                AssertIndependentJoystickAndActionOwnership(scene.Responsive.JoystickRect, GameObject.Find("SMASH").GetComponent<RectTransform>());
                AssertGuideLayoutClear(scene.Guide);
            }

            var movementOrigin = scene.Defender.transform.position;
            GameplayInput.SetMovement(new Vector2(.01f, 0)); scene.Player.Tick(); GameplayInput.ClearMovement();
            Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Move), "Joystick noise is not locomotion.");
            SetRootPosition(scene.Defender, movementOrigin + Vector3.right);
            scene.Player.Tick();
            Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Move), "External root displacement without accepted player locomotion is not proof.");
            SetRootPosition(scene.Defender, movementOrigin);
            GameObject.Find("DODGE").GetComponent<Button>().onClick.Invoke();
            Assert.That(scene.Defender.IsDodging, Is.True); Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Move), "An out-of-order Dodge remains valid gameplay but is not movement proof.");
            var dodgeDeadline = Time.realtimeSinceStartup + 1f;
            while (scene.Defender.IsDodging && Time.realtimeSinceStartup < dodgeDeadline) yield return null;
            Assert.That(scene.Defender.IsDodging, Is.False, "The factual locomotion check starts only after Dodge has ended.");
            Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Move), "Dodge displacement is not accepted locomotion.");
            SetRootPosition(scene.Defender, movementOrigin);

            var deadline = Time.realtimeSinceStartup + 2f;
            while (scene.Guide.Step == DefenseGuideStep.Move && Time.realtimeSinceStartup < deadline)
            {
                GameplayInput.SetMovement(Vector2.right);
                scene.Player.Tick();
                yield return null;
            }
            GameplayInput.ClearMovement();
            Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Attack));
            var factualMovement = scene.Defender.transform.position - movementOrigin; factualMovement.y = 0;
            Assert.That(factualMovement.magnitude, Is.GreaterThanOrEqualTo(.6f), "Movement proof requires authoritative root displacement from accepted locomotion.");
            Assert.That(scene.Guide.GuideText, Is.EqualTo("ATTACK — TAP SMASH"));

            Assert.That(scene.Defender.TryUse(2, Vector3.forward), Is.True);
            GameObject.Find("SMASH").GetComponent<Button>().onClick.Invoke();
            Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Attack), "A blocked Smash cannot advance the proof.");
            yield return new WaitForSecondsRealtime(1f); yield return null; scene.Guide.RefreshForTests();
            Assert.That(scene.Guide.GuideText, Is.EqualTo("ATTACK — TAP SMASH"));
            GameObject.Find("SMASH").GetComponent<Button>().onClick.Invoke();
            Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Dodge));
            scene.Guide.RefreshForTests();
            Assert.That(scene.Guide.GuideText, Is.EqualTo("WAIT — ACTION IN PROGRESS"));
            GameObject.Find("DODGE").GetComponent<Button>().onClick.Invoke();
            Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Dodge), "Busy Dodge is rejected and cannot advance.");
            yield return new WaitForSecondsRealtime(.8f); yield return null;

            scene.Defender.ApplyRoot(.5f); yield return null; scene.Guide.RefreshForTests();
            Assert.That(scene.Guide.GuideText, Is.EqualTo("ROOTED — TAP THE WORLD TO BREAK FREE"));
            AssertGuideLayoutClear(scene.Guide);
            GameObject.Find("DODGE").GetComponent<Button>().onClick.Invoke();
            Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Dodge), "Rooted Dodge is rejected.");
            scene.Defender.BreakRoot(); scene.Guide.RefreshForTests();
            Assert.That(scene.Guide.GuideText, Is.EqualTo("ESCAPE — TAP DODGE"));
            GameObject.Find("DODGE").GetComponent<Button>().onClick.Invoke();
            Assert.That(scene.Defender.IsDodging, Is.True); Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Release));
            Assert.That(scene.Guide.GuideText, Is.EqualTo("RETURN — TAP RELEASE"));

            GameObject.Find("RELEASE").GetComponent<Button>().onClick.Invoke();
            Assert.That(scene.Possession.Possessed, Is.Null); Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.KeeperReturn));
            Assert.That(scene.Guide.GuideText, Does.Not.Contain("RELEASED EARLY"), "The correctly ordered explicit release cannot show a warning.");
            yield return new WaitForSecondsRealtime(.8f); yield return null;
            Assert.That(scene.Guide.Step, Is.EqualTo(DefenseGuideStep.Result));
            Assert.That(scene.Guide.GuideText, Is.EqualTo("KEEPER VIEW — WATCH THE RESULT"));
            Assert.That(scene.Defender, Is.SameAs(originalEntity));
            Assert.That(scene.Defender.Controller<CreatureBrain>().IsActive, Is.True);
        }

        static void PrepareChangedBuildHandoff()
        {
            FirstPlayableMinute.ResetForTests(); FirstPlayableMinute.TryStart(); DefenseLayoutSave.Save(DefenseLayout.Default()); PrototypeSave.SetControlStyle("Contextual");
            var original = DefenseLayout.Default(); var changed = DefenseLayout.Default();
            changed.Slots[0] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Ent);
            changed.Slots[2] = new DefenseSlotLayout(DefenseSlotType.Creature, DefensePieceType.Wolf);
            Assert.That(FirstPlayableMinute.TryAcceptChangedBuild(FirstPlayableMinute.CaptureBuildEntry(original), changed), Is.True);
        }

        static void SetRootPosition(CombatEntity entity, Vector3 position)
        {
            var motor = entity.Motor;
            var wasEnabled = motor.enabled;
            motor.enabled = false;
            entity.transform.position = position;
            motor.enabled = wasEnabled;
        }

        static void AssertOwnedGesture(Button button, int pointerId)
        {
            var ownership = button.GetComponent<UiPointerOwnership>(); Assert.That(ownership, Is.Not.Null);
            var pointer = new PointerEventData(EventSystem.current) { pointerId = pointerId };
            ownership.OnPointerDown(pointer); Assert.That(GameplayInput.IsUiOwned(pointerId), Is.True);
            ownership.OnPointerUp(pointer); Assert.That(GameplayInput.IsUiOwned(pointerId), Is.False);
        }

        static void AssertIndependentJoystickAndActionOwnership(RectTransform joystickRect, RectTransform actionRect)
        {
            Assert.That(joystickRect, Is.Not.Null); Assert.That(joystickRect.gameObject.activeInHierarchy, Is.True);
            var joystick = joystickRect.GetComponent<VirtualJoystick>(); var action = actionRect.GetComponent<UiPointerOwnership>();
            var center = RectTransformUtility.WorldToScreenPoint(null, joystickRect.TransformPoint(Vector3.zero));
            var stickPointer = new PointerEventData(EventSystem.current) { pointerId = 2202, position = center };
            var actionPointer = new PointerEventData(EventSystem.current) { pointerId = 2203 };
            joystick.OnPointerDown(stickPointer); action.OnPointerDown(actionPointer);
            Assert.That(GameplayInput.IsUiOwned(2202), Is.True); Assert.That(GameplayInput.IsUiOwned(2203), Is.True);
            action.OnPointerUp(actionPointer); Assert.That(GameplayInput.IsUiOwned(2202), Is.True, "Second-finger action release cannot release the joystick pointer.");
            joystick.OnPointerUp(stickPointer); Assert.That(GameplayInput.HasUiOwnership, Is.False);
        }

        static void AssertGuideLayoutClear(FirstPlayableMinuteDefenseGuide guide)
        {
            var reference = Object.FindFirstObjectByType<CanvasScaler>().referenceResolution;
            if (guide.GuideLineRect && guide.GuideLineRect.gameObject.activeSelf)
            {
                var line = DesignRect(guide.GuideLineRect, reference); var worldLine = WorldRect(guide.GuideLineRect);
                Assert.That(line.xMin, Is.GreaterThanOrEqualTo(0)); Assert.That(line.yMin, Is.GreaterThanOrEqualTo(0));
                Assert.That(line.xMax, Is.LessThanOrEqualTo(reference.x)); Assert.That(line.yMax, Is.LessThanOrEqualTo(reference.y));
                foreach (var label in Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
                    if (label.rectTransform != guide.GuideLineRect && !label.GetComponentInParent<Button>())
                    {
                        var labelRect = WorldRect(label.rectTransform);
                        Assert.That(worldLine.Overlaps(labelRect), Is.False,
                            $"Guide line overlaps {label.name} text='{label.text}' line={worldLine} label={labelRect}");
                    }
            }
            foreach (var control in new[] { guide.DismissRect, guide.SkipRect })
            {
                if (!control || !control.gameObject.activeSelf) continue;
                var bounds = DesignRect(control, reference);
                var worldBounds = WorldRect(control);
                Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(0), control.name); Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(0), control.name);
                Assert.That(bounds.xMax, Is.LessThanOrEqualTo(reference.x), control.name); Assert.That(bounds.yMax, Is.LessThanOrEqualTo(reference.y), control.name);
                foreach (var button in Object.FindObjectsByType<Button>(FindObjectsSortMode.None))
                {
                    var other = (RectTransform)button.transform; if (other == control) continue;
                    Assert.That(worldBounds.Overlaps(WorldRect(other)), Is.False, $"{control.name} overlaps {other.name}");
                }
                foreach (var label in Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
                {
                    if (label.GetComponentInParent<Button>() || label.rectTransform == guide.GuideLineRect) continue;
                    Assert.That(worldBounds.Overlaps(WorldRect(label.rectTransform)), Is.False, $"{control.name} overlaps {label.name}");
                }
            }
            if (guide.EmphasisVisible)
            {
                var bounds = DesignRect(guide.EmphasisRect, reference);
                Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(0), "Guide emphasis"); Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(0), "Guide emphasis");
                Assert.That(bounds.xMax, Is.LessThanOrEqualTo(reference.x), "Guide emphasis"); Assert.That(bounds.yMax, Is.LessThanOrEqualTo(reference.y), "Guide emphasis");
            }
        }

        static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4]; rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }

        static void AssertResultActionsClear(DefenderHUD hud)
        {
            var root = WorldRect((RectTransform)hud.transform);
            var resultBounds = WorldRect(hud.ResultRect);
            Assert.That(resultBounds.xMin, Is.GreaterThanOrEqualTo(root.xMin)); Assert.That(resultBounds.yMin, Is.GreaterThanOrEqualTo(root.yMin));
            Assert.That(resultBounds.xMax, Is.LessThanOrEqualTo(root.xMax)); Assert.That(resultBounds.yMax, Is.LessThanOrEqualTo(root.yMax));
            var actions = new[] { GameObject.Find("DEFEND AGAIN").GetComponent<RectTransform>(), GameObject.Find("RETURN TO BUILD").GetComponent<RectTransform>(), GameObject.Find("MY REALM").GetComponent<RectTransform>() };
            for (var index = 0; index < actions.Length; index++)
            {
                var bounds = WorldRect(actions[index]);
                Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(root.xMin)); Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(root.yMin));
                Assert.That(bounds.xMax, Is.LessThanOrEqualTo(root.xMax)); Assert.That(bounds.yMax, Is.LessThanOrEqualTo(root.yMax));
                Assert.That(resultBounds.Overlaps(bounds), Is.False, $"Result facts overlap {actions[index].name}");
                for (var other = index + 1; other < actions.Length; other++)
                    Assert.That(bounds.Overlaps(WorldRect(actions[other])), Is.False, $"Result actions overlap: {actions[index].name}/{actions[other].name}");
            }
        }

        static void AssertTerminalGameplayActionsHidden(DefenderHUD hud)
        {
            foreach (var name in new[] { "POSSESS ENT", "RELEASE", "ACTIVATE TRAP", "SMASH", "GROUND SLAM", "DODGE", "JUMP" })
            {
                var action = FindHudButton(hud, name);
                Assert.That(action.gameObject.activeSelf, Is.False, $"{name} remains visible after the terminal result.");
                Assert.That(action.interactable, Is.False, $"{name} remains enabled after the terminal result.");
            }
        }

        static void AssertGameplayActionsRestoredForRetry(DefenderHUD hud, bool selectionMade)
        {
            var trapAction = FindHudButton(hud, "ACTIVATE TRAP");
            Assert.That(trapAction.gameObject.activeSelf, Is.True, "A fresh retry must restore the trap action.");
            var possessAction = FindHudButton(hud, "POSSESS ENT");
            Assert.That(possessAction.gameObject.activeSelf, Is.EqualTo(selectionMade), "A fresh retry must restore selection-driven possession visibility.");
            if (selectionMade) Assert.That(possessAction.interactable, Is.True, "The restored possession action must accept the selected defender.");
        }

        static IEnumerator ExerciseTerminalCallbacksWithoutReactivation(GuidedScene scene)
        {
            GameplayInput.SetTerminalState(false);
            try
            {
                scene.Possession.Select(scene.Defender);
                Assert.That(scene.Possession.PossessSelected(), Is.True, "The fixture must raise a late possession callback.");
                yield return null;
                AssertTerminalGameplayActionsHidden(scene.Hud);
                scene.Possession.Release(true);
                yield return null;
                AssertTerminalGameplayActionsHidden(scene.Hud);
            }
            finally { GameplayInput.SetTerminalState(true); }
        }

        static Button FindHudButton(DefenderHUD hud, string name)
        {
            foreach (var button in hud.GetComponentsInChildren<Button>(true))
                if (button.name == name) return button;
            Assert.Fail($"Missing Defender HUD action: {name}");
            return null;
        }

        static Rect DesignRect(RectTransform rect, Vector2 parentSize)
        {
            var anchorMin = Vector2.Scale(rect.anchorMin, parentSize); var anchorSize = Vector2.Scale(rect.anchorMax - rect.anchorMin, parentSize);
            var size = anchorSize + rect.sizeDelta; var pivotPoint = anchorMin + Vector2.Scale(anchorSize, rect.pivot) + rect.anchoredPosition;
            return new Rect(pivotPoint - Vector2.Scale(size, rect.pivot), size);
        }

        static void AssertSceneSingletons()
        {
            Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
        }

        sealed class GuidedScene
        {
            public readonly DefenderHUD Hud;
            public readonly FirstPlayableMinuteDefenseGuide Guide;
            public readonly PossessionManager Possession;
            public readonly DefenseManager Defense;
            public readonly CombatEntity Defender;
            public readonly CombatEntity Invader;
            public readonly PlayerController Player;
            public readonly ResponsiveHudRoot Responsive;

            GuidedScene(DefenderHUD hud, PossessionManager possession, DefenseManager defense, CombatEntity defender, CombatEntity invader)
            {
                Hud = hud; Guide = hud.FirstMinuteGuide; Possession = possession; Defense = defense; Defender = defender; Invader = invader;
                Player = defender.Controller<PlayerController>(); Responsive = hud.GetComponent<ResponsiveHudRoot>();
            }

            public static GuidedScene Capture()
            {
                var hud = Object.FindFirstObjectByType<DefenderHUD>(); var possession = Object.FindFirstObjectByType<PossessionManager>(); var defense = Object.FindFirstObjectByType<DefenseManager>();
                var defender = GameObject.Find("Guardian Ent").GetComponent<CombatEntity>(); var invader = GameObject.Find("Invading Blood Knight").GetComponent<CombatEntity>();
                Assert.That(Object.FindObjectsByType<FirstPlayableMinuteDefenseGuide>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(hud.FirstMinuteGuide, Is.Not.Null); return new GuidedScene(hud, possession, defense, defender, invader);
            }

            public void StopInvader() => Invader.SetController(null);
        }

        sealed class SavedPreferences
        {
            readonly bool hadGuide = PlayerPrefs.HasKey(FirstPlayableMinute.KeyForTests);
            readonly string guide = PlayerPrefs.GetString(FirstPlayableMinute.KeyForTests, string.Empty);
            readonly bool hadLayout = PlayerPrefs.HasKey(DefenseLayoutSave.KeyForTests);
            readonly string layout = PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests, string.Empty);
            readonly string realm = PrototypeSave.SelectedRealm;
            readonly string orientation = PrototypeSave.OrientationPreference;
            readonly string control = PrototypeSave.ControlStylePreference;

            public void Restore()
            {
                GameplayInput.ResetForTests(); PrototypeJourney.ResetForTests(); Time.timeScale = 1; Time.fixedDeltaTime = .02f;
                if (hadGuide) PlayerPrefs.SetString(FirstPlayableMinute.KeyForTests, guide); else PlayerPrefs.DeleteKey(FirstPlayableMinute.KeyForTests);
                if (hadLayout) PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, layout); else PlayerPrefs.DeleteKey(DefenseLayoutSave.KeyForTests);
                PlayerPrefs.Save(); PrototypeSave.SelectRealm(realm); PrototypeSave.SetOrientation(orientation); PrototypeSave.SetControlStyle(control); FirstPlayableMinute.ResetBuildHandoff();
            }
        }
    }
}
