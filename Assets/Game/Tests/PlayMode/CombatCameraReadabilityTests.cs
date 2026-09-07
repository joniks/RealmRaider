using System.Collections;
using NUnit.Framework;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class CombatCameraReadabilityTests
    {
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
