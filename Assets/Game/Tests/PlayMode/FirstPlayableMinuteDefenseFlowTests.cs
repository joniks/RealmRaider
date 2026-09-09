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
                yield return CompleteOrderedControlProof(scene, true);

                scene.Invader.Health.TakeDamage(new DamageInfo(1000, null, scene.Invader.transform.position), 0);
                yield return null;
                Assert.That(scene.Defense.State, Is.EqualTo(DefenseState.DefenderVictory));
                Assert.That(scene.Guide.TerminalOutcome, Is.EqualTo(DefenseGuideTerminalOutcome.Completed));
                Assert.That(scene.Guide.GuideText, Is.EqualTo("FIRST DEFENSE COMPLETE — RETURN TO BUILD"));
                Assert.That(scene.Guide.GuideLineRaycastTarget, Is.False);
                Assert.That(scene.Guide.EmphasisTargetName, Is.EqualTo("RETURN TO BUILD"));
                Assert.That(scene.Hud.ResultText, Does.Contain("DEFENDER VICTORY").And.Contain("The invader was destroyed.").And.Contain("FIRST DEFENSE COMPLETE — RETURN TO BUILD"));
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Completed));
                Assert.That(scene.Guide.SkipVisible, Is.False);
                AssertTerminalGameplayActionsHidden(scene.Hud);
                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertTerminalGameplayActionsHidden(scene.Hud); AssertGuideLayoutClear(scene.Guide); AssertResultActionsClear(scene.Hud);
                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertTerminalGameplayActionsHidden(scene.Hud); AssertGuideLayoutClear(scene.Guide); AssertResultActionsClear(scene.Hud);
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
                Assert.That(scene.Guide.GuideText, Is.EqualTo("REALM LOST — THE CONTROL LOOP IS COMPLETE"));
                Assert.That(scene.Guide.EmphasisTargetName, Is.EqualTo("DEFEND AGAIN"));
                Assert.That(scene.Hud.ResultText, Does.Contain("REALM LOST").And.Contain("Heart Tree was captured.").And.Contain("THE CONTROL LOOP IS COMPLETE"));
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Completed));
                AssertTerminalGameplayActionsHidden(scene.Hud);
                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertTerminalGameplayActionsHidden(scene.Hud); AssertGuideLayoutClear(scene.Guide); AssertResultActionsClear(scene.Hud);
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
                first.Invader.Health.TakeDamage(new DamageInfo(1000, null, first.Invader.transform.position), 0); yield return null;
                Assert.That(first.Guide.Step, Is.EqualTo(DefenseGuideStep.Retry));
                Assert.That(first.Guide.GuideText, Is.EqualTo("TRY THE CONTROL LOOP — DEFEND AGAIN"));
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Active));
                AssertTerminalGameplayActionsHidden(first.Hud);
                yield return ExerciseTerminalCallbacksWithoutReactivation(first);
                first.Responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertTerminalGameplayActionsHidden(first.Hud); AssertGuideLayoutClear(first.Guide);
                first.Responsive.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertTerminalGameplayActionsHidden(first.Hud); AssertGuideLayoutClear(first.Guide);

                GameObject.Find("DEFEND AGAIN").GetComponent<Button>().onClick.Invoke(); yield return null; yield return null;
                var retry = GuidedScene.Capture(); retry.StopInvader();
                Assert.That(retry.Guide.Step, Is.EqualTo(DefenseGuideStep.Select));
                AssertGameplayActionsRestoredForRetry(retry.Hud, false);
                retry.Possession.Select(retry.Defender); AssertGameplayActionsRestoredForRetry(retry.Hud, true); GameObject.Find("POSSESS ENT").GetComponent<Button>().onClick.Invoke(); yield return null;
                Assert.That(retry.Possession.Possessed, Is.SameAs(retry.Defender));
                retry.Possession.Release(true); yield return null;
                Assert.That(retry.Guide.Step, Is.EqualTo(DefenseGuideStep.Select), "Forced release restarts selection and never satisfies explicit Release.");

                retry.Possession.Select(retry.Defender); GameObject.Find("POSSESS ENT").GetComponent<Button>().onClick.Invoke(); yield return null;
                retry.Defender.SetController(retry.Defender.Controller<CreatureBrain>()); yield return null;
                Assert.That(retry.Guide.Step, Is.EqualTo(DefenseGuideStep.Inactive));
                retry.Possession.Release(true); yield return null;
                Assert.That(retry.Guide.Step, Is.EqualTo(DefenseGuideStep.Select), "A later factual manager release permits a fresh attempt without proving Release.");

                retry.Possession.Select(retry.Defender); GameObject.Find("POSSESS ENT").GetComponent<Button>().onClick.Invoke(); yield return null;
                retry.Hud.DepletePossessionEnergyForTests(); yield return null;
                Assert.That(retry.Possession.Possessed, Is.Null);
                Assert.That(retry.Guide.Step, Is.EqualTo(DefenseGuideStep.Inactive), "Energy depletion cannot prove Release or advertise an impossible retry.");

                var skip = GameObject.Find("SKIP GUIDE").GetComponent<Button>(); AssertOwnedGesture(skip, 2301); skip.onClick.Invoke();
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Skipped));
                Assert.That(retry.Guide.GuideLineVisible, Is.False); Assert.That(retry.Guide.EmphasisVisible, Is.False);
                Assert.That(retry.Guide.SkipVisible, Is.False); Assert.That(retry.Guide.DismissVisible, Is.False);
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

        static IEnumerator CompleteOrderedControlProof(GuidedScene scene, bool verifyResponsivePresentation)
        {
            if (verifyResponsivePresentation)
            {
                scene.Responsive.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null;
                AssertGuideLayoutClear(scene.Guide); AssertOwnedGesture(GameObject.Find("DISMISS").GetComponent<Button>(), 2201);
            }

            SetRootPosition(scene.Defender, new Vector3(0, scene.Defender.transform.position.y, 0));
            scene.Possession.Select(scene.Defender);
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
            var actions = new[] { GameObject.Find("DEFEND AGAIN").GetComponent<RectTransform>(), GameObject.Find("RETURN TO BUILD").GetComponent<RectTransform>(), GameObject.Find("MY REALM").GetComponent<RectTransform>() };
            for (var index = 0; index < actions.Length; index++)
            {
                var bounds = WorldRect(actions[index]);
                Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(root.xMin)); Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(root.yMin));
                Assert.That(bounds.xMax, Is.LessThanOrEqualTo(root.xMax)); Assert.That(bounds.yMax, Is.LessThanOrEqualTo(root.yMax));
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
                GameplayInput.ResetForTests(); Time.timeScale = 1; Time.fixedDeltaTime = .02f;
                if (hadGuide) PlayerPrefs.SetString(FirstPlayableMinute.KeyForTests, guide); else PlayerPrefs.DeleteKey(FirstPlayableMinute.KeyForTests);
                if (hadLayout) PlayerPrefs.SetString(DefenseLayoutSave.KeyForTests, layout); else PlayerPrefs.DeleteKey(DefenseLayoutSave.KeyForTests);
                PlayerPrefs.Save(); PrototypeSave.SelectRealm(realm); PrototypeSave.SetOrientation(orientation); PrototypeSave.SetControlStyle(control); FirstPlayableMinute.ResetBuildHandoff();
            }
        }
    }
}
