using System.Collections;
using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.Possession;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class JumpFlowTests
    {
        [UnityTearDown]
        public IEnumerator ResetTransientState()
        {
            GameplayInput.ResetForTests();
            Time.timeScale = 1;
            var active = SceneManager.GetActiveScene();
            if (active.IsValid() && (active.name == "SylvanRealm" || active.name == "DefenderTest"))
            {
                var cleanup = SceneManager.CreateScene("JumpFlowTests Cleanup");
                SceneManager.SetActiveScene(cleanup);
                yield return SceneManager.UnloadSceneAsync(active);
            }
            else yield return null;
        }

        [UnityTest]
        public IEnumerator DirectPlayerJump_RisesAcceptsHorizontalInputAndLandsOnce()
        {
            GameplayInput.ResetForTests();
            var cameraObject = MainCamera();
            var ground = CreateGround("Jump Ground", Vector3.zero, new Vector3(20, .5f, 20));
            var fixture = new EntityFixture(new Vector3(0, 1, 0), false);
            try
            {
                fixture.Entity.SetController(fixture.Player);
                yield return Settle(fixture);
                var start = fixture.Entity.transform.position;
                var health = fixture.Entity.Health.Current;

                Assert.That(fixture.Player.Jump(), Is.True);
                Assert.That(fixture.Entity.IsJumping, Is.True);
                Assert.That(fixture.Player.Jump(), Is.False, "A held or repeated press cannot double jump.");
                GameplayInput.SetMovement(Vector2.right);
                var peak = start.y;
                var timeout = Time.realtimeSinceStartup + 4;
                while ((fixture.Entity.IsJumping || !fixture.Entity.IsGrounded) && Time.realtimeSinceStartup < timeout)
                {
                    yield return null;
                    peak = Mathf.Max(peak, fixture.Entity.transform.position.y);
                }
                GameplayInput.ClearMovement();
                yield return Settle(fixture);

                Assert.That(fixture.Entity.IsJumping, Is.False);
                Assert.That(fixture.Entity.IsGrounded, Is.True);
                Assert.That(peak, Is.GreaterThan(start.y + .5f));
                Assert.That(fixture.Entity.transform.position.x, Is.GreaterThan(start.x + .5f), "Existing horizontal input must remain effective in air.");
                Assert.That(fixture.Entity.transform.position.y, Is.EqualTo(start.y).Within(.080001f));
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(health));
                Assert.That(fixture.Entity.IsActionResolving, Is.False);
                Assert.That(fixture.Entity.Health.IsDamageImmune, Is.False);
            }
            finally
            {
                GameplayInput.ResetForTests();
                fixture.Dispose();
                Object.Destroy(ground);
                Object.Destroy(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator CoyoteJump_UsesOneStrictGraceWindowAndClearsAcrossLifecycle()
        {
            GameplayInput.ResetForTests();
            var cameraObject = MainCamera();
            var ground = CreateGround("Coyote Jump Ground", Vector3.zero, new Vector3(20, .5f, 20));
            var fixture = new EntityFixture(new Vector3(0, 1, 0), false);
            try
            {
                fixture.Entity.SetController(fixture.Player);

                yield return RecordGroundAndLeave(fixture);
                Assert.That(fixture.Player.Jump(), Is.True, "The first press immediately after factual grounding may use coyote time.");
                Assert.That(fixture.Player.Jump(), Is.False, "Coyote time cannot create a second airborne jump.");
                fixture.Entity.ApplyRoot(.01f);
                fixture.Entity.BreakRoot();

                yield return RecordGroundAndLeave(fixture);
                yield return new WaitForSeconds(CombatEntity.JumpCoyoteSeconds);
                yield return null;
                Assert.That(fixture.Entity.IsGrounded, Is.False);
                Assert.That(fixture.Player.Jump(), Is.False, "Coyote time rejects at 0.10 seconds plus one frame.");

                yield return RecordGroundAndLeave(fixture);
                fixture.Entity.ApplyRoot(1);
                fixture.Entity.BreakRoot();
                Assert.That(fixture.Player.Jump(), Is.False, "Root cleanup cannot leave a delayed jump grace.");

                yield return RecordGroundAndLeave(fixture);
                GameplayInput.SetTerminalState(true);
                yield return null;
                GameplayInput.SetTerminalState(false);
                Assert.That(fixture.Player.Jump(), Is.False, "Terminal cleanup cannot leave a delayed jump grace.");

                yield return RecordGroundAndLeave(fixture);
                fixture.Entity.SetController(fixture.Ai);
                fixture.Entity.SetController(fixture.Player);
                Assert.That(fixture.Player.Jump(), Is.False, "Controller release cannot carry grace into the next direct-control state.");

                yield return RecordGroundAndLeave(fixture);
                fixture.Entity.Motor.enabled = false;
                yield return null;
                fixture.Entity.Motor.enabled = true;
                Assert.That(fixture.Player.Jump(), Is.False, "Disabled Motor cleanup cannot leave a delayed jump grace.");
            }
            finally
            {
                GameplayInput.ResetForTests();
                fixture.Dispose();
                Object.Destroy(ground);
                Object.Destroy(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator JumpVisualMotion_AccentsOnlyThePivotAndCancelsWithoutDelayedLanding()
        {
            GameplayInput.ResetForTests();
            var cameraObject = MainCamera();
            var ground = CreateGround("Jump Motion Ground", Vector3.zero, new Vector3(20, .5f, 20));
            var fixture = new EntityFixture(new Vector3(0, 1, 0), false);
            try
            {
                fixture.Entity.SetController(fixture.Player);
                yield return Settle(fixture);
                var motion = fixture.Root.GetComponent<CharacterVisualMotion>(); var pivot = motion.PresentationPivot;
                var rootScale = fixture.Root.transform.localScale; var motorHeight = fixture.Entity.Motor.height; var motorRadius = fixture.Entity.Motor.radius; var motorCenter = fixture.Entity.Motor.center;

                Assert.That(fixture.Player.Jump(), Is.True);
                yield return null;
                Assert.That(pivot.localScale.y, Is.GreaterThan(motion.BaseScale.y), "Real takeoff stretches only the assembled presentation pivot.");
                Assert.That(fixture.Root.transform.localScale, Is.EqualTo(rootScale));
                Assert.That(fixture.Entity.Motor.height, Is.EqualTo(motorHeight)); Assert.That(fixture.Entity.Motor.radius, Is.EqualTo(motorRadius)); Assert.That(fixture.Entity.Motor.center, Is.EqualTo(motorCenter));
                AssertPivotBounds(motion);

                yield return WaitForGrounded(fixture.Entity);
                yield return null;
                Assert.That(fixture.Entity.IsJumping, Is.False);
                Assert.That(pivot.localScale.y, Is.LessThan(motion.BaseScale.y), "Only the factual grounded end of a real jump settles the pivot.");
                Assert.That(fixture.Root.transform.localScale, Is.EqualTo(rootScale));
                AssertPivotBounds(motion);

                yield return Settle(fixture);
                Assert.That(fixture.Player.Jump(), Is.True);
                GameplayInput.SetTerminalState(true);
                yield return null;
                GameplayInput.SetTerminalState(false);
                yield return null;
                AssertNoLandingSettle(motion);

                yield return Settle(fixture);
                Assert.That(fixture.Player.Jump(), Is.True);
                fixture.Entity.SetController(fixture.Ai);
                yield return null;
                AssertNoLandingSettle(motion);

                fixture.Entity.SetController(fixture.Player);
                yield return Settle(fixture);
                Assert.That(fixture.Player.Jump(), Is.True);
                fixture.Entity.Motor.enabled = false;
                yield return null;
                fixture.Entity.Motor.enabled = true;
                yield return null;
                AssertNoLandingSettle(motion);
            }
            finally
            {
                GameplayInput.ResetForTests();
                fixture.Dispose();
                Object.Destroy(ground);
                Object.Destroy(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator JumpGate_RejectsRootActionDodgeTerminalReleaseDisableAirborneAndDeath()
        {
            GameplayInput.ResetForTests();
            var cameraObject = MainCamera();
            var ground = CreateGround("Jump Gate Ground", Vector3.zero, new Vector3(80, .5f, 40));
            var fixture = new EntityFixture(new Vector3(-20, 1, 0), true);
            var airborne = new EntityFixture(new Vector3(0, 5, 0), false);
            var death = new EntityFixture(new Vector3(20, 1, 0), false);
            try
            {
                fixture.Entity.SetController(fixture.Player);
                death.Entity.SetController(death.Player);
                airborne.Entity.SetController(airborne.Player);
                yield return Settle(fixture);
                yield return Settle(death);
                yield return WaitForAirborne(airborne);
                Assert.That(airborne.Player.Jump(), Is.False, "An airborne character cannot start a jump.");
                Assert.That(airborne.Entity.IsJumping, Is.False);

                fixture.Entity.ApplyRoot(1);
                Assert.That(fixture.Player.Jump(), Is.False);
                Assert.That(fixture.Entity.IsJumping, Is.False);
                fixture.Entity.BreakRoot();

                Assert.That(fixture.Entity.TryUse(0, Vector3.forward), Is.True);
                Assert.That(fixture.Player.Jump(), Is.False, "Ability authority excludes jump.");
                Assert.That(fixture.Entity.IsJumping, Is.False);
                yield return new WaitForSeconds(.35f);
                Assert.That(fixture.Player.Dodge(), Is.True);
                Assert.That(fixture.Player.Jump(), Is.False, "Dodge authority excludes jump.");
                Assert.That(fixture.Entity.IsJumping, Is.False);
                fixture.Entity.ApplyRoot(.1f);
                fixture.Entity.BreakRoot();
                yield return null;

                GameplayInput.SetTerminalState(true);
                Assert.That(fixture.Player.Jump(), Is.False);
                Assert.That(fixture.Entity.IsJumping, Is.False);
                GameplayInput.SetTerminalState(false);
                fixture.Entity.Motor.enabled = false;
                Assert.That(fixture.Player.Jump(), Is.False);
                Assert.That(fixture.Entity.IsJumping, Is.False);
                fixture.Entity.transform.position += Vector3.up * .25f;
                fixture.Entity.Motor.enabled = true;
                Physics.SyncTransforms();
                yield return Settle(fixture);

                Assert.That(fixture.Player.Jump(), Is.True);
                Assert.That(fixture.Entity.TryUse(0, Vector3.forward), Is.False, "A jump cannot overlap an ability.");
                Assert.That(fixture.Player.Dodge(), Is.False, "A jump cannot overlap dodge.");
                fixture.Entity.ApplyRoot(.1f);
                Assert.That(fixture.Entity.IsJumping, Is.False, "Root must clear active vertical impulse.");
                fixture.Entity.BreakRoot();
                yield return Settle(fixture);
                Assert.That(fixture.Player.Jump(), Is.True);
                fixture.Entity.SetController(fixture.Ai);
                Assert.That(fixture.Entity.IsJumping, Is.False, "Possession release/controller swap must clear vertical impulse.");
                Assert.That(fixture.Player.Jump(), Is.False);

                Assert.That(death.Player.Jump(), Is.True);
                death.Entity.Health.TakeDamage(new DamageInfo(1000, null, death.Entity.transform.position), 0);
                Assert.That(death.Entity.Health.IsDead, Is.True);
                Assert.That(death.Entity.IsJumping, Is.False);
                Assert.That(death.Player.Jump(), Is.False);

                fixture.Entity.SetController(fixture.Player);
                yield return Settle(fixture);
                Assert.That(fixture.Player.Jump(), Is.True);
                fixture.Player.enabled = false;
                Assert.That(fixture.Entity.IsJumping, Is.False, "Controller disable must clear jump state immediately.");
                Assert.That(fixture.Entity.TryJump(), Is.False, "Gameplay authority must reject a disabled player controller.");
            }
            finally
            {
                GameplayInput.ResetForTests();
                fixture.Dispose(); airborne.Dispose(); death.Dispose();
                Object.Destroy(ground);
                Object.Destroy(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator AirborneMovement_RemainsInsideStaticArenaBoundaryAndLands()
        {
            GameplayInput.ResetForTests();
            var cameraObject = MainCamera();
            var owner = new GameObject("Jump Boundary Owner");
            var center = new Vector3(2000, 0, 2000);
            var ground = CreateGround("Jump Boundary Ground", center, new Vector3(14, .5f, 14));
            var fixture = new EntityFixture(center + new Vector3(5.4f, 1, 0), false);
            try
            {
                PrototypeArenaBoundaryBuilder.BuildRectangle(owner.transform, center, new Vector2(14, 14), 0,
                    PrototypeArenaBoundaryStyle.NeutralStone, 1.1f, .9f, 6, .4f);
                fixture.Entity.SetController(fixture.Player);
                Physics.SyncTransforms();
                yield return Settle(fixture);
                var health = fixture.Entity.Health.Current;
                var innerFace = center.x + 7 - .4f;

                Assert.That(fixture.Player.Jump(), Is.True);
                GameplayInput.SetMovement(Vector2.right);
                var timeout = Time.realtimeSinceStartup + 4;
                while ((fixture.Entity.IsJumping || !fixture.Entity.IsGrounded) && Time.realtimeSinceStartup < timeout) yield return null;
                GameplayInput.ClearMovement();
                yield return Settle(fixture);

                Assert.That(fixture.Entity.IsJumping, Is.False);
                Assert.That(fixture.Entity.IsGrounded, Is.True);
                Assert.That(fixture.Entity.transform.position.x, Is.LessThan(innerFace + .01f), "Jump cannot pass the existing static boundary.");
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(health));
                Assert.That(fixture.Entity.IsActionResolving, Is.False);
            }
            finally
            {
                GameplayInput.ResetForTests();
                fixture.Dispose();
                Object.Destroy(ground);
                Object.Destroy(owner);
                Object.Destroy(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator RaidAndPossessedDefenderJumpButtons_OwnInputGateAndFitBothOrientations()
        {
            var previousStyle = PrototypeSave.ControlStylePreference;
            try
            {
            GameplayInput.ResetForTests();
            PrototypeSave.SetControlStyle(InRunControlStyleSelector.Joystick);
            SceneManager.LoadScene("SylvanRealm");
            yield return null;
            yield return null;

            var raidHud = Object.FindFirstObjectByType<RaidHUD>();
            var raidRoot = raidHud.GetComponent<ResponsiveHudRoot>();
            var hero = GameObject.Find("Blood Knight").GetComponent<CombatEntity>();
            yield return WaitForGrounded(hero);
            Assert.That(raidHud.JumpButtonVisible, Is.True);
            Assert.That(raidHud.JumpButtonInteractable, Is.True);
            Assert.That(raidHud.JumpButtonText, Is.EqualTo("JUMP"));
            AssertUiOwnership(raidHud.JumpButtonRect);
            yield return AssertActionLayout(raidRoot, raidHud.JumpButtonRect, PrototypeOrientation.Portrait);
            yield return AssertActionLayout(raidRoot, raidHud.JumpButtonRect, PrototypeOrientation.Landscape);
            hero.ApplyRoot(.1f);
            yield return null;
            Assert.That(raidHud.JumpButtonVisible, Is.True);
            Assert.That(raidHud.JumpButtonInteractable, Is.False);
            Assert.That(raidHud.JumpButtonText, Does.Contain("ROOTED"));
            hero.BreakRoot();
            yield return null;
            Assert.That(raidHud.JumpButtonInteractable, Is.True);
            raidHud.JumpButtonRect.GetComponent<Button>().onClick.Invoke();
            Assert.That(hero.IsJumping, Is.True);
            yield return null;
            Assert.That(raidHud.JumpButtonInteractable, Is.False);
            Assert.That(raidHud.JumpButtonText, Does.Contain("AIRBORNE"));
            GameplayInput.SetTerminalState(true);
            yield return null;
            Assert.That(hero.IsJumping, Is.False);
            Assert.That(raidHud.JumpButtonVisible, Is.False);

            GameplayInput.SetTerminalState(false);
            SceneManager.LoadScene("DefenderTest");
            yield return null;
            yield return null;
            var defenderHud = Object.FindFirstObjectByType<DefenderHUD>();
            var defenderRoot = defenderHud.GetComponent<ResponsiveHudRoot>();
            var possession = Object.FindFirstObjectByType<PossessionManager>();
            var ent = GameObject.Find("Guardian Ent").GetComponent<CombatEntity>();
            Assert.That(defenderHud.JumpButtonVisible, Is.False, "Keeper view must not expose direct movement actions.");
            possession.Select(ent);
            Assert.That(possession.PossessSelected(), Is.True);
            yield return WaitForGrounded(ent);
            Assert.That(defenderHud.JumpButtonVisible, Is.True);
            Assert.That(defenderHud.JumpButtonInteractable, Is.True);
            AssertUiOwnership(defenderHud.JumpButtonRect);
            yield return AssertActionLayout(defenderRoot, defenderHud.JumpButtonRect, PrototypeOrientation.Portrait);
            yield return AssertActionLayout(defenderRoot, defenderHud.JumpButtonRect, PrototypeOrientation.Landscape);
            defenderHud.JumpButtonRect.GetComponent<Button>().onClick.Invoke();
            Assert.That(ent.IsJumping, Is.True);
            possession.Release();
            yield return null;
            Assert.That(ent.IsJumping, Is.False);
            Assert.That(defenderHud.JumpButtonVisible, Is.False);
            }
            finally { PrototypeSave.SetControlStyle(previousStyle); }
        }

        [UnityTest]
        public IEnumerator FingertapDoubleTap_UsesExistingJumpAndRetainsGroundDestination()
        {
            var previousStyle = PrototypeSave.ControlStylePreference;
            var cameraObject = MainCamera();
            var ground = CreateGround("Fingertap Jump Ground", Vector3.zero, new Vector3(30, .5f, 30));
            var fixture = new EntityFixture(new Vector3(0, 1, 0), false);
            try
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Fingertap);
                ConfigureInputCamera(cameraObject.GetComponent<Camera>(), new Vector3(0, 9, -8), new Vector3(0, 0, 5));
                fixture.Entity.SetController(fixture.Player);
                fixture.Player.SetOrientationForTests(PrototypeOrientation.Portrait);
                yield return Settle(fixture);
                var screenPoint = cameraObject.GetComponent<Camera>().WorldToScreenPoint(new Vector3(0, 0, 5));

                Tap(fixture.Player, 3101, screenPoint);
                Assert.That(fixture.Player.HasDestination, Is.True);
                Assert.That(fixture.Entity.IsJumping, Is.False);
                var firstDestination = fixture.Player.Destination;
                Tap(fixture.Player, 3102, screenPoint + Vector3.right * 2);

                Assert.That(fixture.Entity.IsJumping, Is.True, "The second matching empty-ground tap must use the existing jump authority.");
                Assert.That(fixture.Player.HasDestination, Is.True, "Jump must retain the first tap's forward movement intent.");
                Assert.That(HorizontalDistance(fixture.Player.Destination, firstDestination), Is.LessThan(.01f), "The second release must not replace the first tap's destination.");
                Assert.That(fixture.Player.Destination.z, Is.GreaterThan(fixture.Entity.transform.position.z + 2));
            }
            finally
            {
                PrototypeSave.SetControlStyle(previousStyle);
                fixture.Dispose();
                Object.Destroy(ground);
                Object.Destroy(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator FingertapDoubleTap_RejectsUiSwipeEntityRootAndControlStyleChange()
        {
            var previousStyle = PrototypeSave.ControlStylePreference;
            var cameraObject = MainCamera();
            var ground = CreateGround("Rejected Fingertap Ground", Vector3.zero, new Vector3(40, .5f, 40));
            var fixture = new EntityFixture(new Vector3(0, 1, 0), false);
            var enemy = new EntityFixture(new Vector3(0, 1, 5), false);
            var interactive = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Fingertap);
                var view = cameraObject.GetComponent<Camera>();
                ConfigureInputCamera(view, new Vector3(0, 9, -8), new Vector3(0, 0, 5));
                fixture.Entity.SetController(fixture.Player);
                enemy.Entity.SetController(enemy.Ai);
                fixture.Player.SetOrientationForTests(PrototypeOrientation.Portrait);
                interactive.name = "Interactive Realm Core";
                interactive.transform.position = new Vector3(-4, .5f, 5);
                interactive.AddComponent<RealmRaiders.Realm.RealmCore>();
                yield return Settle(fixture);
                var groundPoint = view.WorldToScreenPoint(new Vector3(4, 0, 5));
                var enemyPoint = view.WorldToScreenPoint(enemy.Entity.transform.position);
                var interactivePoint = view.WorldToScreenPoint(interactive.transform.position);

                fixture.Player.BeginWorldPointer(3201, groundPoint, true);
                fixture.Player.EndWorldPointer(3201, groundPoint, true);
                fixture.Player.BeginWorldPointer(3202, groundPoint, true);
                fixture.Player.EndWorldPointer(3202, groundPoint, true);
                Assert.That(fixture.Entity.IsJumping, Is.False, "UI releases cannot participate in a world double tap.");

                fixture.Player.BeginWorldPointer(3203, groundPoint, false);
                fixture.Player.EndWorldPointer(3203, groundPoint + Vector3.right * (PlayerController.SwipePixels + 10), false);
                Assert.That(fixture.Entity.IsJumping, Is.False, "A swipe is never a jump tap.");

                Tap(fixture.Player, 3204, enemyPoint);
                Tap(fixture.Player, 3205, enemyPoint + Vector3.right);
                Assert.That(fixture.Entity.IsJumping, Is.False, "Entity taps cannot become a ground double tap.");

                Tap(fixture.Player, 3210, interactivePoint);
                Tap(fixture.Player, 3211, interactivePoint + Vector3.right);
                Assert.That(fixture.Entity.IsJumping, Is.False, "Interactive targets cannot become a ground double tap.");

                Tap(fixture.Player, 3206, groundPoint);
                fixture.Entity.ApplyRoot(1);
                Tap(fixture.Player, 3207, groundPoint);
                Assert.That(fixture.Entity.IsJumping, Is.False, "Rooted combat state must reject a double-tap jump.");
                fixture.Entity.BreakRoot();

                Tap(fixture.Player, 3208, groundPoint);
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Joystick);
                fixture.Player.Tick();
                Tap(fixture.Player, 3209, groundPoint);
                Assert.That(fixture.Entity.IsJumping, Is.False, "A control-style change must clear the pending ground tap.");
                Assert.That(fixture.Player.HasDestination, Is.False, "The style change must clear tap-navigation state.");

                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Contextual);
                fixture.Player.SetOrientationForTests(PrototypeOrientation.Portrait);
                Tap(fixture.Player, 3212, groundPoint);
                fixture.Player.SetOrientationForTests(PrototypeOrientation.Landscape);
                Tap(fixture.Player, 3213, groundPoint);
                Assert.That(fixture.Entity.IsJumping, Is.False, "An effective-style change caused by orientation must clear the pending ground tap.");
                Assert.That(fixture.Player.HasDestination, Is.False, "Contextual landscape must not retain portrait tap navigation.");
            }
            finally
            {
                PrototypeSave.SetControlStyle(previousStyle);
                fixture.Dispose(); enemy.Dispose();
                Object.Destroy(interactive);
                Object.Destroy(ground);
                Object.Destroy(cameraObject);
            }
        }

        static void Tap(PlayerController player, int pointerId, Vector2 screenPosition)
        {
            player.BeginWorldPointer(pointerId, screenPosition, false);
            player.EndWorldPointer(pointerId, screenPosition, false);
        }

        static void ConfigureInputCamera(Camera view, Vector3 position, Vector3 lookAt)
        {
            view.transform.SetPositionAndRotation(position, Quaternion.LookRotation(lookAt - position));
            view.fieldOfView = 60;
            view.aspect = 1;
        }

        static float HorizontalDistance(Vector3 first, Vector3 second) =>
            Vector2.Distance(new Vector2(first.x, first.z), new Vector2(second.x, second.z));

        static IEnumerator Settle(EntityFixture fixture)
        {
            var timeout = Time.realtimeSinceStartup + 2;
            var stableFrames = 0;
            var previousY = fixture.Entity.transform.position.y;
            while (stableFrames < 2 && Time.realtimeSinceStartup < timeout)
            {
                yield return null;
                var currentY = fixture.Entity.transform.position.y;
                stableFrames = fixture.Entity.IsGrounded && Mathf.Abs(currentY - previousY) < .005f ? stableFrames + 1 : 0;
                previousY = currentY;
            }
            Assert.That(stableFrames, Is.EqualTo(2), $"{fixture.Root.name} did not reach a stable grounded contact.");
            Assert.That(fixture.Entity.IsGrounded, Is.True, fixture.Root.name);
        }

        static IEnumerator RecordGroundAndLeave(EntityFixture fixture)
        {
            var position = fixture.Entity.transform.position;
            position.y = 1;
            fixture.Entity.transform.position = position;
            Physics.SyncTransforms();
            yield return Settle(fixture);

            fixture.Entity.transform.position += Vector3.up * 3;
            Physics.SyncTransforms();
            fixture.Entity.Move(Vector3.zero);
            Assert.That(fixture.Entity.IsGrounded, Is.False, "The fixture must leave ground after recording a factual motor contact.");
        }

        static IEnumerator WaitForGrounded(CombatEntity entity)
        {
            var timeout = Time.realtimeSinceStartup + 1;
            while (!entity.IsGrounded && Time.realtimeSinceStartup < timeout) yield return null;
            Assert.That(entity.IsGrounded, Is.True, entity.name);
            yield return null;
        }

        static void AssertPivotBounds(CharacterVisualMotion motion)
        {
            var offset = motion.PresentationPivot.localPosition - motion.BasePosition;
            Assert.That(offset.magnitude, Is.LessThanOrEqualTo(.080001f));
            Assert.That(motion.PresentationPivot.localScale.x / motion.BaseScale.x, Is.InRange(.90f, 1.10f));
            Assert.That(motion.PresentationPivot.localScale.y / motion.BaseScale.y, Is.InRange(.90f, 1.10f));
            Assert.That(motion.PresentationPivot.localScale.z / motion.BaseScale.z, Is.InRange(.90f, 1.10f));
        }

        static void AssertNoLandingSettle(CharacterVisualMotion motion)
        {
            Assert.That(motion.PresentationPivot.localScale.y, Is.GreaterThan(motion.BaseScale.y * .97f), "Cancelled jumps cannot schedule a delayed landing squash.");
            AssertPivotBounds(motion);
        }

        static IEnumerator WaitForAirborne(EntityFixture fixture)
        {
            var timeout = Time.realtimeSinceStartup + .25f;
            while (fixture.Entity.IsGrounded && Time.realtimeSinceStartup < timeout)
            {
                fixture.Player.Tick();
                yield return null;
            }
            Assert.That(fixture.Entity.IsGrounded, Is.False, $"{fixture.Root.name} retained stale grounded contact after leaving the ground.");
        }

        static IEnumerator AssertActionLayout(ResponsiveHudRoot root, RectTransform jump, PrototypeOrientation orientation)
        {
            root.SetOrientationForTests(orientation);
            yield return null;
            var reference = root.GetComponent<CanvasScaler>().referenceResolution;
            var jumpRect = DesignRect(jump, reference);
            Assert.That(jumpRect.xMin, Is.GreaterThanOrEqualTo(0), orientation.ToString());
            Assert.That(jumpRect.yMin, Is.GreaterThanOrEqualTo(0), orientation.ToString());
            Assert.That(jumpRect.xMax, Is.LessThanOrEqualTo(reference.x), orientation.ToString());
            Assert.That(jumpRect.yMax, Is.LessThanOrEqualTo(reference.y), orientation.ToString());
            foreach (var button in root.GetComponentsInChildren<Button>(false))
            {
                var other = (RectTransform)button.transform;
                if (other == jump || other.parent != root.transform) continue;
                Assert.That(jumpRect.Overlaps(DesignRect(other, reference)), Is.False,
                    $"Jump overlaps {button.name} in {orientation}.");
            }
        }

        static void AssertUiOwnership(RectTransform button)
        {
            var ownership = button.GetComponent<UiPointerOwnership>();
            Assert.That(ownership, Is.Not.Null);
            var pointer = new PointerEventData(EventSystem.current) { pointerId = 1200 };
            ownership.OnPointerDown(pointer);
            Assert.That(GameplayInput.IsUiOwned(pointer.pointerId), Is.True);
            ownership.OnPointerUp(pointer);
            Assert.That(GameplayInput.IsUiOwned(pointer.pointerId), Is.False);
        }

        static Rect DesignRect(RectTransform rect, Vector2 parentSize)
        {
            Assert.That(rect.anchorMin, Is.EqualTo(rect.anchorMax), rect.name);
            var pivotPoint = Vector2.Scale(rect.anchorMin, parentSize) + rect.anchoredPosition;
            return new Rect(pivotPoint - Vector2.Scale(rect.pivot, rect.sizeDelta), rect.sizeDelta);
        }

        static GameObject MainCamera()
        {
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            camera.tag = "MainCamera";
            return camera;
        }

        static GameObject CreateGround(string name, Vector3 center, Vector3 scale)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = name;
            ground.transform.position = center + Vector3.down * .25f;
            ground.transform.localScale = scale;
            return ground;
        }

        sealed class EntityFixture
        {
            public readonly GameObject Root;
            public readonly CombatEntity Entity;
            public readonly PlayerController Player;
            public readonly CreatureBrain Ai;
            readonly CharacterDefinition definition;
            readonly AbilityDefinition ability;
            readonly CharacterVisualRecipe visualRecipe;

            public EntityFixture(Vector3 position, bool withAbility)
            {
                Root = new GameObject("Jump Test Entity");
                Root.transform.position = position;
                Root.AddComponent<CharacterController>();
                Root.AddComponent<Health>();
                Entity = Root.AddComponent<CombatEntity>();
                Player = Root.AddComponent<PlayerController>();
                Ai = Root.AddComponent<CreatureBrain>();
                definition = ScriptableObject.CreateInstance<CharacterDefinition>();
                definition.DisplayName = "Jump Test Entity";
                definition.Possessable = true;
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4, AttackSpeed = 1 };
                visualRecipe = ScriptableObject.CreateInstance<CharacterVisualRecipe>();
                visualRecipe.Family = CharacterVisualFamily.Humanoid;
                visualRecipe.Primary = Color.red;
                visualRecipe.Secondary = Color.black;
                visualRecipe.AccentColor = Color.yellow;
                definition.VisualRecipe = visualRecipe;
                if (withAbility)
                {
                    ability = ScriptableObject.CreateInstance<AbilityDefinition>();
                    ability.DisplayName = "Jump Gate Action";
                    ability.Kind = AbilityKind.Melee;
                    ability.Windup = .1f;
                    ability.Cooldown = 0;
                    ability.Range = 1;
                    ability.Radius = .1f;
                    definition.Abilities = new[] { ability };
                }
                Entity.Initialize(definition);
            }

            public void Dispose()
            {
                Object.Destroy(Root);
                Object.Destroy(definition);
                Object.Destroy(visualRecipe);
                if (ability) Object.Destroy(ability);
            }
        }
    }
}
