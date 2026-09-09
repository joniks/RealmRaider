using System.Collections;
using NUnit.Framework;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class CombatCameraReadabilityTests
    {
        [UnityTest]
        public IEnumerator JoystickWorldDrag_ChangesOnlyBoundedCameraYawAndRespectsUiTransitionAndThreatFocus()
        {
            var previousStyle = PrototypeSave.ControlStylePreference;
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig)); cameraObject.tag = "MainCamera";
            var playerObject = new GameObject("Yaw Player", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController));
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name = "Yaw Test Ground";
            var threatObject = new GameObject("Yaw Threat");
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            try
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Joystick);
                ground.transform.position = new Vector3(0, -.25f, 0); ground.transform.localScale = new Vector3(30, .5f, 30);
                playerObject.transform.position = new Vector3(0, 1, 0);
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4 };
                var entity = playerObject.GetComponent<CombatEntity>(); entity.Initialize(definition);
                var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.SnapTo(entity, CameraMode.HeroCombat);
                var player = playerObject.GetComponent<PlayerController>(); entity.SetController(player); player.SetOrientationForTests(PrototypeOrientation.Landscape);
                threatObject.transform.position = new Vector3(5, 0, 5);
                Physics.SyncTransforms();
                var settleTimeout = Time.realtimeSinceStartup + 2;
                while (!entity.IsGrounded && Time.realtimeSinceStartup < settleTimeout) yield return null;
                Assert.That(entity.IsGrounded, Is.True, "Yaw fixture must settle before camera-framing measurements.");
                rig.SnapTo(entity, CameraMode.HeroCombat);
                rig.RequestCombatFocus(threatObject.transform, .6f);
                var entityPosition = entity.transform.position;

                player.BeginWorldPointer(4101, new Vector2(200, 300), false);
                player.DragWorldPointer(4101, new Vector2(500, 300));
                player.EndWorldPointer(4101, new Vector2(500, 300), false);
                Assert.That(rig.HasControlYaw, Is.True);
                Assert.That(Mathf.Abs(rig.RequestedControlYaw), Is.InRange(.01f, PrototypeCameraRig.MaximumManualYawStep + .01f));
                Assert.That(rig.HasRequestedCombatFocus, Is.True, "Manual look cannot erase bounded threat focus.");
                Assert.That(entity.transform.position, Is.EqualTo(entityPosition), "World look drag cannot move the controlled entity.");
                Assert.That(entity.IsActionResolving, Is.False, "World look drag cannot cast an ability.");
                yield return null; yield return null;
                Assert.That(Mathf.Abs(rig.ControlYaw), Is.GreaterThan(.01f));
                Assert.That(cameraObject.transform.position.y - entity.transform.position.y, Is.EqualTo(7).Within(.15f), "Yaw must preserve vertical framing.");

                rig.ClearControlYaw();
                GameplayInput.ClaimUiPointer(4102);
                player.BeginWorldPointer(4102, new Vector2(200, 300), false);
                player.DragWorldPointer(4102, new Vector2(500, 300));
                player.EndWorldPointer(4102, new Vector2(500, 300), false);
                GameplayInput.ReleaseUiPointer(4102);
                Assert.That(rig.HasControlYaw, Is.False, "A UI-owned pointer cannot rotate the world camera.");

                rig.TransitionTo(entity, CameraMode.HeroCombat, 1);
                Assert.That(rig.RequestManualYaw(300), Is.False, "Camera transitions own framing until complete.");
            }
            finally
            {
                PrototypeSave.SetControlStyle(previousStyle);
                GameplayInput.ResetForTests();
                Object.Destroy(cameraObject); Object.Destroy(playerObject); Object.Destroy(ground); Object.Destroy(threatObject); Object.Destroy(definition);
            }
        }

        [UnityTest]
        public IEnumerator FingertapFactualMovement_RecentersFromDisplacementNotCombatFacingAndCleansUp()
        {
            var previousStyle = PrototypeSave.ControlStylePreference;
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig)); cameraObject.tag = "MainCamera";
            var playerObject = new GameObject("Locomotion Player", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController));
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var threatObject = new GameObject("Locomotion Threat");
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            try
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Fingertap);
                ground.transform.position = new Vector3(0, -.25f, 0); ground.transform.localScale = new Vector3(30, .5f, 30);
                playerObject.transform.position = new Vector3(0, 1, 0);
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4 };
                var entity = playerObject.GetComponent<CombatEntity>(); entity.Initialize(definition);
                var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.SnapTo(entity, CameraMode.HeroCombat);
                var player = playerObject.GetComponent<PlayerController>(); entity.SetController(player); player.SetOrientationForTests(PrototypeOrientation.Portrait);
                threatObject.transform.position = new Vector3(-5, 0, 5); rig.RequestCombatFocus(threatObject.transform, .5f);
                Physics.SyncTransforms();

                GameplayInput.SetMovement(Vector2.right);
                player.Tick();
                GameplayInput.ClearMovement();
                Assert.That(rig.LastLocomotionDirection.x, Is.GreaterThan(.9f));
                Assert.That(rig.RequestedControlYaw, Is.EqualTo(90).Within(.5f));
                Assert.That(rig.HasRequestedCombatFocus, Is.True);
                var requested = rig.RequestedControlYaw;

                playerObject.transform.rotation = Quaternion.Euler(0, -90, 0);
                player.Tick();
                Assert.That(rig.RequestedControlYaw, Is.EqualTo(requested).Within(.001f), "Idle/combat facing cannot steer the camera.");
                yield return null; yield return null;
                Assert.That(rig.ControlYaw, Is.GreaterThan(0));

                GameplayInput.SetTerminalState(true);
                rig.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                Assert.That(rig.HasControlYaw, Is.False);
                GameplayInput.SetTerminalState(false);
                Assert.That(rig.RequestLocomotionYaw(Vector3.right), Is.True);
                entity.SetController(null);
                Assert.That(rig.HasControlYaw, Is.False, "Controller release must clear locomotion camera intent.");
                rig.SnapTo(entity, CameraMode.HeroCombat);
                entity.SetController(player);
                player.SetOrientationForTests(PrototypeOrientation.Portrait);
                Assert.That(rig.RequestLocomotionYaw(Vector3.forward), Is.True);
                entity.Health.TakeDamage(new DamageInfo(1000, null, entity.transform.position), 0);
                Assert.That(rig.HasControlYaw, Is.False, "Controlled-entity death must clear locomotion camera intent.");
            }
            finally
            {
                PrototypeSave.SetControlStyle(previousStyle);
                GameplayInput.ResetForTests();
                Object.Destroy(cameraObject); Object.Destroy(playerObject); Object.Destroy(ground); Object.Destroy(threatObject); Object.Destroy(definition);
            }
        }

        [UnityTest]
        public IEnumerator DefenderActionsLeaveTheCombatEdgeBandClearOnBothSidesAndOrientations()
        {
            GameplayInput.ResetForTests();
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig), typeof(CombatCameraAwareness)); cameraObject.tag = "MainCamera";
            var playerObject = new GameObject("Player", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController));
            var threatObject = new GameObject("Threat", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var hudObject = new GameObject("Defender Layout Test HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(DefenderHUD), typeof(ResponsiveHudRoot));
            var playerDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var threatDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            try
            {
                playerDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                threatDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                var player = playerObject.GetComponent<CombatEntity>(); player.Initialize(playerDefinition);
                var threat = threatObject.GetComponent<CombatEntity>(); threat.Initialize(threatDefinition);
                var actions = new[]
                {
                    CreateAction(hudObject.transform, "POSSESS ENT", new Vector2(0, 410)),
                    CreateAction(hudObject.transform, "RELEASE", new Vector2(0, 410)),
                    CreateAction(hudObject.transform, "ACTIVATE TRAP", new Vector2(0, 290)),
                    CreateAction(hudObject.transform, "SMASH", new Vector2(-180, 165)),
                    CreateAction(hudObject.transform, "GROUND SLAM", new Vector2(180, 165)),
                    CreateAction(hudObject.transform, "DODGE", new Vector2(0, 530))
                };
                var canvas = hudObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var responsive = hudObject.GetComponent<ResponsiveHudRoot>(); responsive.Initialize(false);
                var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.SnapTo(player, CameraMode.HeroCombat);
                var view = cameraObject.GetComponent<Camera>(); view.transform.SetPositionAndRotation(new Vector3(0, 2, -10), Quaternion.LookRotation(Vector3.forward)); view.fieldOfView = 60; view.aspect = 1; rig.enabled = false;
                player.SetController(playerObject.GetComponent<PlayerController>());
                var awareness = rig.BindCombatHud(responsive); awareness.SetControlled(player);
                var behind = view.transform.position - view.transform.forward * 2;

                responsive.SetOrientationForTests(PrototypeOrientation.Landscape);
                ShowEdge(awareness, threat, threatObject, behind + view.transform.right * 5);
                AssertActionLane(awareness.IndicatorRect, actions, new Vector2(1920, 1080));
                ShowEdge(awareness, threat, threatObject, behind - view.transform.right * 5);
                AssertActionLane(awareness.IndicatorRect, actions, new Vector2(1920, 1080));

                responsive.SetOrientationForTests(PrototypeOrientation.Portrait);
                ShowEdge(awareness, threat, threatObject, behind + view.transform.right * 5);
                AssertActionLane(awareness.IndicatorRect, actions, new Vector2(1080, 1920));
                ShowEdge(awareness, threat, threatObject, behind - view.transform.right * 5);
                AssertActionLane(awareness.IndicatorRect, actions, new Vector2(1080, 1920));
                yield return null;
            }
            finally
            {
                GameplayInput.ResetForTests();
                Object.Destroy(hudObject); Object.Destroy(cameraObject); Object.Destroy(playerObject); Object.Destroy(threatObject);
                Object.Destroy(playerDefinition); Object.Destroy(threatDefinition);
            }
        }

        static RectTransform CreateAction(Transform parent, string name, Vector2 portraitPosition)
        {
            var action = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            action.transform.SetParent(parent, false);
            var rect = (RectTransform)action.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0);
            rect.anchoredPosition = portraitPosition;
            rect.sizeDelta = new Vector2(340, 96);
            return rect;
        }

        static void ShowEdge(CombatCameraAwareness awareness, CombatEntity threat, GameObject threatObject, Vector3 position)
        {
            threatObject.transform.position = position;
            Physics.SyncTransforms();
            awareness.ReportThreat(threat);
            awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
            Assert.That(awareness.IndicatorVisible, Is.True);
            Assert.That(awareness.TargetPlateVisible, Is.False);
        }

        static void AssertActionLane(RectTransform edge, RectTransform[] actions, Vector2 reference)
        {
            var edgeBounds = DesignRect(edge, reference);
            AssertContained(edgeBounds, reference, edge.name);
            foreach (var action in actions)
            {
                var actionBounds = DesignRect(action, reference);
                AssertContained(actionBounds, reference, action.name);
                Assert.That(edgeBounds.Overlaps(actionBounds), Is.False, $"Combat edge tab overlaps {action.name}");
            }
        }

        static Rect DesignRect(RectTransform rect, Vector2 reference)
        {
            Assert.That(rect.anchorMin, Is.EqualTo(rect.anchorMax));
            var pivotPoint = Vector2.Scale(rect.anchorMin, reference) + rect.anchoredPosition;
            return new Rect(pivotPoint - Vector2.Scale(rect.pivot, rect.sizeDelta), rect.sizeDelta);
        }

        static void AssertContained(Rect bounds, Vector2 reference, string name)
        {
            Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(0), name);
            Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(0), name);
            Assert.That(bounds.xMax, Is.LessThanOrEqualTo(reference.x), name);
            Assert.That(bounds.yMax, Is.LessThanOrEqualTo(reference.y), name);
        }
    }
}
