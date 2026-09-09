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
            var attack = ScriptableObject.CreateInstance<AbilityDefinition>();
            try
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Joystick);
                ground.transform.position = new Vector3(0, -.25f, 0); ground.transform.localScale = new Vector3(30, .5f, 30);
                playerObject.transform.position = new Vector3(0, 1, 0);
                attack.DisplayName = "Yaw Slash"; attack.Windup = .02f; attack.Cooldown = .1f;
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4 }; definition.Abilities = new[] { attack };
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
                var visibleForward = PlayerController.ResolveCameraPlaneDirection(cameraObject.transform, Vector2.up, entity.transform.forward);
                Assert.That(player.UseAbility(0), Is.True, "The HUD attack remains available after a camera-only drag.");
                AssertDirection(entity.transform.forward, visibleForward, "The following HUD attack must use the visible camera direction.");
                yield return WaitForActionPhase(entity, CombatActionPhase.Idle);

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
                Object.Destroy(cameraObject); Object.Destroy(playerObject); Object.Destroy(ground); Object.Destroy(threatObject); Object.Destroy(definition); Object.Destroy(attack);
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
        public IEnumerator CameraRelativeAttackDirection_MapsButtonsAndSwipesAcrossModesOrientationsAndStyles()
        {
            var previousStyle = PrototypeSave.ControlStylePreference;
            var cameraObject = new GameObject("Direction Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig)); cameraObject.tag = "MainCamera";
            var playerObject = new GameObject("Direction Player", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController));
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var buttonAttack = DirectionalAbility("Button Attack");
            var swipeAttack = DirectionalAbility("Swipe Attack");
            try
            {
                GameplayInput.ResetForTests();
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4 };
                definition.Abilities = new[] { buttonAttack, swipeAttack };
                var entity = playerObject.GetComponent<CombatEntity>(); entity.Initialize(definition);
                var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.SnapTo(entity, CameraMode.HeroCombat); rig.enabled = false;
                var player = playerObject.GetComponent<PlayerController>(); entity.SetController(player);

                yield return AssertButtonDirection(rig, cameraObject.transform, entity, player, CameraMode.HeroCombat, PrototypeOrientation.Portrait, InRunControlStyleSelector.Fingertap, 72);
                yield return AssertButtonDirection(rig, cameraObject.transform, entity, player, CameraMode.HeroCombat, PrototypeOrientation.Landscape, InRunControlStyleSelector.Joystick, -38);
                yield return AssertButtonDirection(rig, cameraObject.transform, entity, player, CameraMode.PossessedCreature, PrototypeOrientation.Portrait, InRunControlStyleSelector.Joystick, 138);
                yield return AssertButtonDirection(rig, cameraObject.transform, entity, player, CameraMode.PossessedCreature, PrototypeOrientation.Landscape, InRunControlStyleSelector.Fingertap, -112);

                yield return AssertSwipeDirection(rig, cameraObject.transform, entity, player, CameraMode.HeroCombat, PrototypeOrientation.Portrait, Vector2.up * 100, 35, 4201);
                yield return AssertSwipeDirection(rig, cameraObject.transform, entity, player, CameraMode.PossessedCreature, PrototypeOrientation.Landscape, Vector2.right * 100, -55, 4202);

                entity.transform.rotation = Quaternion.LookRotation(Vector3.left);
                cameraObject.transform.rotation = Quaternion.LookRotation(Vector3.down, Vector3.forward);
                AssertDirection(PlayerController.ResolveCameraPlaneDirection(cameraObject.transform, Vector2.up, entity.transform.forward), Vector3.left,
                    "A vertical camera axis must fall back to the controlled entity's valid planar facing.");
            }
            finally
            {
                PrototypeSave.SetControlStyle(previousStyle);
                GameplayInput.ResetForTests();
                Object.Destroy(cameraObject); Object.Destroy(playerObject); Object.Destroy(definition); Object.Destroy(buttonAttack); Object.Destroy(swipeAttack);
            }
        }

        [UnityTest]
        public IEnumerator CameraRelativeAttackDirection_PreservesEnemyTapAndBufferedInputSnapshot()
        {
            var previousStyle = PrototypeSave.ControlStylePreference;
            var cameraObject = new GameObject("Target Camera", typeof(Camera), typeof(AudioListener)); cameraObject.tag = "MainCamera";
            var playerObject = new GameObject("Target Player", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController));
            var enemyObject = new GameObject("Tapped Enemy", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var playerDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var enemyDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var first = DirectionalAbility("Target Slash");
            var buffered = DirectionalAbility("Buffered Slash");
            try
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Fingertap);
                playerDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4 }; playerDefinition.Abilities = new[] { first, buffered };
                enemyDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                var entity = playerObject.GetComponent<CombatEntity>(); playerObject.transform.position = new Vector3(0, 1, 0); entity.Initialize(playerDefinition);
                enemyObject.transform.position = new Vector3(2, 1, 0); enemyObject.GetComponent<CombatEntity>().Initialize(enemyDefinition);
                var camera = cameraObject.GetComponent<Camera>(); cameraObject.transform.position = new Vector3(0, 6, -8); cameraObject.transform.LookAt(playerObject.transform.position + Vector3.up);
                var player = playerObject.GetComponent<PlayerController>(); entity.SetController(player); player.SetOrientationForTests(PrototypeOrientation.Portrait);
                Physics.SyncTransforms();

                var enemyScreen = camera.WorldToScreenPoint(enemyObject.transform.position);
                Assert.That(enemyScreen.z, Is.GreaterThan(0));
                player.BeginWorldPointer(4301, enemyScreen, false);
                player.EndWorldPointer(4301, enemyScreen, false);
                Assert.That(entity.ActionPhase, Is.EqualTo(CombatActionPhase.Windup), "A nearby factual enemy tap must start the target attack.");
                AssertDirection(entity.transform.forward, Vector3.right, "Explicit target direction must remain stronger than camera-forward intent.");
                yield return WaitForActionPhase(entity, CombatActionPhase.Idle);

                cameraObject.transform.rotation = Quaternion.Euler(55, 0, 0);
                Assert.That(player.UseAbility(0), Is.True);
                yield return WaitForActionPhase(entity, CombatActionPhase.Recovery);
                cameraObject.transform.rotation = Quaternion.Euler(55, 90, 0);
                var inputTimeDirection = PlayerController.ResolveCameraPlaneDirection(cameraObject.transform, Vector2.up, entity.transform.forward);
                var previousReadyAt = entity.Abilities[1].ReadyAt;
                Assert.That(player.UseAbility(1), Is.True, "A ready HUD ability should enter the existing Recovery buffer.");
                Assert.That(player.IsAbilityBuffered(1), Is.True);
                cameraObject.transform.rotation = Quaternion.Euler(55, -90, 0);
                yield return WaitForAbilityConsumption(entity.Abilities[1], previousReadyAt);
                AssertDirection(entity.transform.forward, inputTimeDirection, "Buffered execution must retain its input-time world direction after later camera yaw.");

                yield return WaitForActionPhase(entity, CombatActionPhase.Recovery);
                Assert.That(player.RequestAbility(0, Vector3.back), Is.True);
                Assert.That(player.HasBufferedAbility, Is.True);
                entity.SetController(null);
                Assert.That(player.HasBufferedAbility, Is.False, "Controller loss remains authoritative over buffered direction cleanup.");
            }
            finally
            {
                PrototypeSave.SetControlStyle(previousStyle);
                GameplayInput.ResetForTests();
                Object.Destroy(cameraObject); Object.Destroy(playerObject); Object.Destroy(enemyObject);
                Object.Destroy(playerDefinition); Object.Destroy(enemyDefinition); Object.Destroy(first); Object.Destroy(buffered);
            }
        }

        [UnityTest]
        public IEnumerator CameraRelativeAttackDirection_HeldMovementPreservesAcceptedWindupAndImpactDirection()
        {
            var previousStyle = PrototypeSave.ControlStylePreference;
            var cameraObject = new GameObject("Held Direction Camera", typeof(Camera), typeof(AudioListener)); cameraObject.tag = "MainCamera";
            var playerObject = new GameObject("Held Direction Player", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController));
            var acceptedTargetObject = new GameObject("Accepted Direction Target", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var movementTargetObject = new GameObject("Movement Direction Target", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name = "Held Direction Ground";
            var playerDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var acceptedTargetDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var movementTargetDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var attack = DirectionalAbility("Held Direction Slash", .18f); attack.Range = 2; attack.Radius = .3f; attack.Damage = 20;
            try
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Joystick);
                playerDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4 }; playerDefinition.Abilities = new[] { attack };
                acceptedTargetDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                movementTargetDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                var entity = playerObject.GetComponent<CombatEntity>(); playerObject.transform.position = new Vector3(0, 1, 0); entity.Initialize(playerDefinition);
                var acceptedTarget = acceptedTargetObject.GetComponent<CombatEntity>(); acceptedTarget.Initialize(acceptedTargetDefinition);
                var movementTarget = movementTargetObject.GetComponent<CombatEntity>(); movementTarget.Initialize(movementTargetDefinition);
                ground.transform.position = new Vector3(0, -.25f, 0); ground.transform.localScale = new Vector3(20, .5f, 20);
                cameraObject.transform.rotation = Quaternion.Euler(55, 90, 0);
                var player = playerObject.GetComponent<PlayerController>(); entity.SetController(player); player.SetOrientationForTests(PrototypeOrientation.Landscape);
                var acceptedDirection = PlayerController.ResolveCameraPlaneDirection(cameraObject.transform, Vector2.up, entity.transform.forward);
                Physics.SyncTransforms();

                Assert.That(player.UseAbility(0), Is.True);
                Assert.That(entity.ActionPhase, Is.EqualTo(CombatActionPhase.Windup));
                AssertDirection(entity.transform.forward, acceptedDirection, "The action must accept the visible direction before movement.");
                var telegraph = GameObject.Find("Melee Range");
                Assert.That(telegraph, Is.Not.Null);
                AssertDirection(telegraph.transform.forward, acceptedDirection, "The telegraph must use the accepted direction.");

                var start = entity.transform.position;
                GameplayInput.SetMovement(Vector2.up);
                player.Tick();
                GameplayInput.ClearMovement();
                Assert.That(entity.transform.position.z, Is.GreaterThan(start.z), "Existing held-input translation must continue during Windup.");
                AssertDirection(entity.transform.forward, acceptedDirection, "Held movement must not rotate an already accepted action.");
                AssertDirection(telegraph.transform.forward, acceptedDirection, "Held movement must not retarget the existing telegraph.");

                acceptedTargetObject.transform.position = entity.transform.position + acceptedDirection * 1.5f;
                movementTargetObject.transform.position = entity.transform.position + Vector3.forward * 1.5f;
                Physics.SyncTransforms();
                yield return WaitForActionPhase(entity, CombatActionPhase.Recovery);
                Assert.That(acceptedTarget.Health.Current, Is.LessThan(acceptedTarget.Health.Maximum), "Impact geometry must follow the accepted camera direction.");
                Assert.That(movementTarget.Health.Current, Is.EqualTo(movementTarget.Health.Maximum), "Held translation input cannot retarget impact geometry.");
                AssertDirection(entity.transform.forward, acceptedDirection, "Recovery keeps the accepted action facing until completion.");
            }
            finally
            {
                PrototypeSave.SetControlStyle(previousStyle);
                GameplayInput.ResetForTests();
                Object.Destroy(cameraObject); Object.Destroy(playerObject); Object.Destroy(acceptedTargetObject); Object.Destroy(movementTargetObject); Object.Destroy(ground);
                Object.Destroy(playerDefinition); Object.Destroy(acceptedTargetDefinition); Object.Destroy(movementTargetDefinition); Object.Destroy(attack);
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

        static AbilityDefinition DirectionalAbility(string name, float windup = .02f)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            ability.DisplayName = name;
            ability.Kind = AbilityKind.Melee;
            ability.Damage = 10;
            ability.Range = 2;
            ability.Radius = .5f;
            ability.Cooldown = .01f;
            ability.Windup = windup;
            return ability;
        }

        static IEnumerator AssertButtonDirection(PrototypeCameraRig rig, Transform cameraTransform, CombatEntity entity, PlayerController player,
            CameraMode mode, PrototypeOrientation orientation, string style, float yaw)
        {
            PrototypeSave.SetControlStyle(style);
            player.SetOrientationForTests(orientation);
            player.Tick();
            rig.SnapTo(entity, mode);
            cameraTransform.rotation = Quaternion.Euler(58, yaw, 0);
            var expected = PlayerController.ResolveCameraPlaneDirection(cameraTransform, Vector2.up, entity.transform.forward);
            Assert.That(rig.Mode, Is.EqualTo(mode));
            Assert.That(player.EffectiveControlStyle, Is.EqualTo(style));
            Assert.That(player.UseAbility(0), Is.True);
            AssertDirection(entity.transform.forward, expected, $"HUD direction mismatch for {mode}/{orientation}/{style}.");
            yield return WaitForActionPhase(entity, CombatActionPhase.Idle);
        }

        static IEnumerator AssertSwipeDirection(PrototypeCameraRig rig, Transform cameraTransform, CombatEntity entity, PlayerController player,
            CameraMode mode, PrototypeOrientation orientation, Vector2 screenDelta, float yaw, int pointerId)
        {
            PrototypeSave.SetControlStyle(InRunControlStyleSelector.Fingertap);
            player.SetOrientationForTests(orientation);
            player.Tick();
            rig.SnapTo(entity, mode);
            cameraTransform.rotation = Quaternion.Euler(58, yaw, 0);
            var expected = PlayerController.ResolveCameraPlaneDirection(cameraTransform, screenDelta, entity.transform.forward);
            player.BeginWorldPointer(pointerId, new Vector2(200, 300), false);
            player.EndWorldPointer(pointerId, new Vector2(200, 300) + screenDelta, false);
            Assert.That(entity.ActionPhase, Is.EqualTo(CombatActionPhase.Windup), $"Swipe must cast in {mode}/{orientation}.");
            AssertDirection(entity.transform.forward, expected, $"Swipe direction mismatch for {mode}/{orientation}.");
            yield return WaitForActionPhase(entity, CombatActionPhase.Idle);
        }

        static IEnumerator WaitForActionPhase(CombatEntity entity, CombatActionPhase phase)
        {
            var deadline = Time.realtimeSinceStartup + 2;
            while (entity && entity.ActionPhase != phase && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(entity.ActionPhase, Is.EqualTo(phase), $"Timed out waiting for {phase}.");
        }

        static IEnumerator WaitForAbilityConsumption(AbilityRuntime ability, float previousReadyAt)
        {
            var deadline = Time.realtimeSinceStartup + 2;
            while (ability.ReadyAt <= previousReadyAt && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(ability.ReadyAt, Is.GreaterThan(previousReadyAt), "Timed out waiting for buffered direction execution.");
        }

        static void AssertDirection(Vector3 actual, Vector3 expected, string message)
        {
            actual.y = 0; expected.y = 0;
            Assert.That(actual.sqrMagnitude, Is.GreaterThan(.001f), message);
            Assert.That(expected.sqrMagnitude, Is.GreaterThan(.001f), message);
            Assert.That(Vector3.Dot(actual.normalized, expected.normalized), Is.GreaterThan(.995f), message);
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
