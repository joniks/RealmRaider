using System.Collections;
using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Possession;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class KeeperSelectionFlowTests
    {
        static int cleanupSceneIndex;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            GameplayInput.SetTerminalState(false);
            var previous = SceneManager.GetActiveScene();
            if (!previous.IsValid() || !previous.isLoaded || previous.buildIndex < 0)
            {
                yield return null;
                yield break;
            }

            var cleanup = SceneManager.CreateScene($"KeeperSelectionFlowTests {++cleanupSceneIndex}");
            SceneManager.SetActiveScene(cleanup);
            yield return SceneManager.UnloadSceneAsync(previous);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GameplayInput.ResetForTests();
            yield return null;
        }

        [UnityTest]
        public IEnumerator ShortNearMissSelectsRegisteredCreatureOnceInPortraitAndLandscape()
        {
            var cases = new[]
            {
                ("Guardian Ent", new Vector2(1080, 1920)),
                ("Infernal Brute", new Vector2(1920, 1080))
            };

            foreach (var item in cases)
            {
                var fixture = new SelectionFixture(item.Item1);
                try
                {
                    fixture.Camera.aspect = item.Item2.x / item.Item2.y;
                    var selectionEvents = 0;
                    fixture.Manager.SelectionChanged += selected => { if (selected) selectionEvents++; };
                    var position = fixture.NearMissPosition(item.Item2);
                    var entityPosition = fixture.Entity.transform.position;
                    var entityId = fixture.Entity.GetEntityId();
                    var selectionIdentity = fixture.Entity.SelectionIdentity;
                    var health = fixture.Entity.Health.Current;
                    var abilityReadyAt = fixture.Entity.Abilities[0].ReadyAt;
                    var controller = fixture.Entity.ActiveController;
                    var energy = fixture.Energy.Remaining;
                    var motor = fixture.Entity.Motor;
                    var motorCenter = motor.center;
                    var motorHeight = motor.height;
                    var motorRadius = motor.radius;
                    var canvasCount = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
                    var eventSystemCount = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
                    var listenerCount = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;

                    var rayHitsEntity = Physics.Raycast(fixture.Camera.ScreenPointToRay(position), out var exactHit, 500f, ~0, QueryTriggerInteraction.Ignore) &&
                                        exactHit.collider.GetComponentInParent<CombatEntity>() == fixture.Entity;
                    Assert.That(rayHitsEntity, Is.False, "The synthetic point must exercise near-miss tolerance, not an exact collider hit.");
                    fixture.Manager.BeginKeeperPress(101, position, 10f);
                    Assert.That(fixture.Manager.EndKeeperPress(101, position, 10.1f, item.Item2), Is.True);

                    Assert.That(fixture.Manager.Selected, Is.SameAs(fixture.Entity));
                    Assert.That(selectionEvents, Is.EqualTo(1));
                    var marker = fixture.Entity.transform.Find("Possession Selection Presentation");
                    Assert.That(marker, Is.Not.Null);
                    Assert.That(fixture.Entity.GetComponentsInChildren<PossessionSelectionPresentation>(true), Has.Length.EqualTo(1));
                    var markerCopy = marker.GetComponent<PossessionSelectionPresentation>().LabelTransform.GetComponent<TextMesh>().text;
                    Assert.That(markerCopy, Does.Contain("SELECTED"));
                    Assert.That(markerCopy, Does.Contain("PRESS POSSESS"));
                    Assert.That(fixture.Entity.transform.position, Is.EqualTo(entityPosition));
                    Assert.That(fixture.Entity.GetEntityId(), Is.EqualTo(entityId));
                    Assert.That(fixture.Entity.SelectionIdentity, Is.EqualTo(selectionIdentity));
                    Assert.That(fixture.Entity.Health.Current, Is.EqualTo(health));
                    Assert.That(fixture.Entity.Abilities[0].ReadyAt, Is.EqualTo(abilityReadyAt));
                    Assert.That(fixture.Entity.ActiveController, Is.SameAs(controller));
                    Assert.That(fixture.Energy.Remaining, Is.EqualTo(energy));
                    Assert.That(fixture.Entity.Motor, Is.SameAs(motor));
                    Assert.That(motor.center, Is.EqualTo(motorCenter));
                    Assert.That(motor.height, Is.EqualTo(motorHeight));
                    Assert.That(motor.radius, Is.EqualTo(motorRadius));
                    foreach (var markerCollider in marker.GetComponentsInChildren<Collider>(true))
                        Assert.That(markerCollider.enabled, Is.False, "The existing visual marker must not affect selection physics.");
                    Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(canvasCount));
                    Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(eventSystemCount));
                    Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(listenerCount));
                }
                finally { fixture.Destroy(); }
                yield return null;
            }
        }

        [UnityTest]
        public IEnumerator KeeperGestureRejectsUiSwipeCancelOcclusionUnregisteredTerminalAndPossession()
        {
            var fixture = new SelectionFixture("Guardian Ent");
            var screenSize = new Vector2(1080, 1920);
            var nearMiss = fixture.NearMissPosition(screenSize);
            GameObject occluder = null;
            GameObject invaderObject = null;
            CharacterDefinition invaderDefinition = null;
            try
            {
                fixture.Camera.aspect = screenSize.x / screenSize.y;
                var acceptedSelections = 0;
                fixture.Manager.SelectionChanged += selected => { if (selected) acceptedSelections++; };

                GameplayInput.ClaimUiPointer(201);
                fixture.Manager.BeginKeeperPress(201, nearMiss, 20f);
                GameplayInput.ReleaseUiPointer(201);
                Assert.That(fixture.Manager.EndKeeperPress(201, nearMiss, 20.1f, screenSize), Is.False,
                    "A UI-owned press must remain rejected after the UI releases ownership.");

                fixture.Manager.BeginKeeperPress(202, nearMiss, 21f);
                var swipe = nearMiss + Vector2.right * (KeeperSelectionResolver.ShortEdge(screenSize) * PossessionManager.KeeperTapSlopShortEdge + 1);
                Assert.That(fixture.Manager.EndKeeperPress(202, swipe, 21.1f, screenSize), Is.False);

                fixture.Manager.BeginKeeperPress(203, nearMiss, 22f);
                fixture.Manager.CancelKeeperPress(203);
                Assert.That(fixture.Manager.EndKeeperPress(203, nearMiss, 22.1f, screenSize), Is.False);

                occluder = GameObject.CreatePrimitive(PrimitiveType.Cube);
                occluder.name = "Selection Occluder";
                occluder.transform.position = Vector3.Lerp(fixture.Camera.transform.position, fixture.SelectionPoint, .5f);
                occluder.transform.localScale = new Vector3(5, 5, 1);
                occluder.transform.rotation = fixture.Camera.transform.rotation;
                fixture.Manager.BeginKeeperPress(204, nearMiss, 23f);
                Assert.That(fixture.Manager.EndKeeperPress(204, nearMiss, 23.1f, screenSize), Is.False);
                occluder.SetActive(false);

                invaderObject = new GameObject("Unregistered Invader", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
                invaderObject.transform.position = new Vector3(3, 0, 0);
                invaderDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
                invaderDefinition.DisplayName = "Unregistered Invader";
                invaderDefinition.Possessable = true;
                invaderDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                invaderDefinition.Abilities = System.Array.Empty<AbilityDefinition>();
                var invader = invaderObject.GetComponent<CombatEntity>();
                invader.Initialize(invaderDefinition);
                Physics.SyncTransforms();
                var invaderPoint = fixture.Camera.WorldToScreenPoint(invader.Motor.bounds.center);
                fixture.Manager.BeginKeeperPress(205, invaderPoint, 24f);
                Assert.That(fixture.Manager.EndKeeperPress(205, invaderPoint, 24.1f, screenSize), Is.False);

                GameplayInput.SetTerminalState(true);
                fixture.Manager.BeginKeeperPress(206, nearMiss, 25f);
                Assert.That(fixture.Manager.EndKeeperPress(206, nearMiss, 25.1f, screenSize), Is.False);
                GameplayInput.SetTerminalState(false);

                fixture.Rig.TransitionTo(null, CameraMode.KeeperOverview, .2f);
                fixture.Manager.BeginKeeperPress(207, nearMiss, 26f);
                Assert.That(fixture.Manager.EndKeeperPress(207, nearMiss, 26.1f, screenSize), Is.False);
                fixture.Rig.TransitionTo(null, CameraMode.KeeperOverview, .001f);
                yield return null;
                yield return null;

                nearMiss = fixture.NearMissPosition(screenSize);
                Assert.That(fixture.Tap(208, nearMiss, screenSize, 27f), Is.True);
                Assert.That(acceptedSelections, Is.EqualTo(1));
                Assert.That(fixture.Manager.PossessSelected(), Is.True);
                Assert.That(fixture.Tap(209, nearMiss, screenSize, 28f), Is.False);
                Assert.That(acceptedSelections, Is.EqualTo(1));
                Assert.That(fixture.Manager.Possessed, Is.SameAs(fixture.Entity));

                fixture.Manager.Release();
                fixture.Rig.TransitionTo(null, CameraMode.KeeperOverview, .001f);
                yield return null;
                yield return null;
                nearMiss = fixture.NearMissPosition(screenSize);
                Assert.That(fixture.Tap(210, nearMiss, screenSize, 29f), Is.True);
                Assert.That(acceptedSelections, Is.EqualTo(2));
                Assert.That(fixture.Manager.Selected, Is.SameAs(fixture.Entity));
                Assert.That(fixture.Entity.ActiveController, Is.SameAs(fixture.Brain));
            }
            finally
            {
                GameplayInput.SetTerminalState(false);
                GameplayInput.ReleaseUiPointer(201);
                Object.Destroy(occluder);
                Object.Destroy(invaderObject);
                Object.Destroy(invaderDefinition);
                fixture.Destroy();
            }
        }

        sealed class SelectionFixture
        {
            public readonly GameObject CameraObject;
            public readonly GameObject EntityObject;
            public readonly GameObject ManagerObject;
            public readonly CharacterDefinition Definition;
            public readonly AbilityDefinition Ability;
            public readonly Camera Camera;
            public readonly PrototypeCameraRig Rig;
            public readonly CombatEntity Entity;
            public readonly CreatureBrain Brain;
            public readonly PossessionManager Manager;
            public readonly PossessionEnergy Energy;

            public Vector3 SelectionPoint => Entity.Motor.bounds.center;

            public SelectionFixture(string displayName)
            {
                CameraObject = new GameObject("Keeper Selection Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig));
                CameraObject.tag = "MainCamera";
                EntityObject = new GameObject(displayName, typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
                ManagerObject = new GameObject("Keeper Selection Manager", typeof(PossessionManager));
                Definition = ScriptableObject.CreateInstance<CharacterDefinition>();
                Definition.DisplayName = displayName;
                Definition.Possessable = true;
                Definition.Stats = new CombatStats { MaxHealth = 180, MoveSpeed = 3, AttackSpeed = 1 };
                Ability = ScriptableObject.CreateInstance<AbilityDefinition>();
                Ability.DisplayName = "Selection Test Ability";
                Ability.Cooldown = 3;
                Definition.Abilities = new[] { Ability };

                Entity = EntityObject.GetComponent<CombatEntity>();
                Entity.Initialize(Definition);
                Brain = EntityObject.GetComponent<CreatureBrain>();
                Entity.SetController(Brain);

                Camera = CameraObject.GetComponent<Camera>();
                Camera.fieldOfView = 45;
                var cameraPosition = new Vector3(0, 20, -40);
                var cameraRotation = Quaternion.LookRotation(SelectionPoint - cameraPosition);
                Rig = CameraObject.GetComponent<PrototypeCameraRig>();
                Rig.ConfigureOverview(cameraPosition, cameraRotation);
                Rig.SnapToOverview();

                Manager = ManagerObject.GetComponent<PossessionManager>();
                Manager.Initialize(Rig);
                Energy = new PossessionEnergy(30);
                Manager.ConfigureEnergy(Energy);
                Manager.Register(Entity);
                Physics.SyncTransforms();
            }

            public Vector2 NearMissPosition(Vector2 screenSize)
            {
                var center = Camera.WorldToScreenPoint(SelectionPoint);
                return new Vector2(center.x, center.y) + Vector2.right *
                    (KeeperSelectionResolver.ShortEdge(screenSize) * KeeperSelectionResolver.NearMissRadiusShortEdge * .82f);
            }

            public bool Tap(int pointerId, Vector2 position, Vector2 screenSize, float startedAt)
            {
                Manager.BeginKeeperPress(pointerId, position, startedAt);
                return Manager.EndKeeperPress(pointerId, position, startedAt + .1f, screenSize);
            }

            public void Destroy()
            {
                if (Manager && Manager.IsPossessing) Manager.Release();
                Object.Destroy(ManagerObject);
                Object.Destroy(EntityObject);
                Object.Destroy(CameraObject);
                Object.Destroy(Definition);
                Object.Destroy(Ability);
            }
        }
    }
}
