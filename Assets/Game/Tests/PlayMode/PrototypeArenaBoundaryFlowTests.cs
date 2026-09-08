using System.Collections;
using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RealmRaiders.Tests
{
    public sealed class PrototypeArenaBoundaryFlowTests
    {
        [UnityTest]
        public IEnumerator OnlyTheFourGameplayZonesBuildOneBoundary()
        {
            foreach (var scene in new[] { "PrototypeHub", "RealmBuild" })
            {
                SceneManager.LoadScene(scene); yield return null; yield return null;
                Assert.That(GameObject.Find(PrototypeArenaBoundaryBuilder.BoundaryName), Is.Null,
                    $"{scene} is a UI screen and must not build a world boundary.");
            }

            var gameplayScenes = new[] { "CharacterSandbox", "DefenderTest", "InfernalRealm", "SylvanRealm" };
            foreach (var scene in gameplayScenes)
            {
                SceneManager.LoadScene(scene); yield return null; yield return null;
                var boundary = GameObject.Find(PrototypeArenaBoundaryBuilder.BoundaryName);
                Assert.That(boundary, Is.Not.Null, $"{scene} must build its explicit arena boundary.");
                Assert.That(boundary.GetComponentsInChildren<MeshRenderer>(true), Has.Length.EqualTo(1));
                var colliderCount = boundary.GetComponentsInChildren<BoxCollider>(true).Length;
                if (scene == "SylvanRealm") Assert.That(colliderCount, Is.InRange(1, PrototypeArenaBoundaryBuilder.SylvanColliderBudget));
                else Assert.That(colliderCount, Is.EqualTo(4));
                Assert.That(boundary.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
                foreach (var collider in boundary.GetComponentsInChildren<BoxCollider>(true)) Assert.That(collider.isTrigger, Is.False);
                if (scene == "CharacterSandbox") Assert.That(GameObject.Find("Boundary Stone"), Is.Null);
            }

            SceneManager.LoadScene("RealmBuild"); yield return null; yield return null;
        }

        [UnityTest]
        public IEnumerator RectangleRunsStopOrdinaryFastAndDiagonalControllerMotionWithoutDamage()
        {
            var owners = new GameObject[3];
            var probes = new GameObject[3];
            var sizes = new[] { new Vector2(22, 28), new Vector2(14, 68), new Vector2(14, 68) };
            var insets = new[] { .5f, .4f, .4f };
            var styles = new[]
            {
                PrototypeArenaBoundaryStyle.NeutralStone,
                PrototypeArenaBoundaryStyle.SylvanRoots,
                PrototypeArenaBoundaryStyle.InfernalBasalt
            };
            try
            {
                for (var zone = 0; zone < owners.Length; zone++)
                {
                    var center = new Vector3(1000 + zone * 100, 0, 1000);
                    owners[zone] = new GameObject($"Rectangle Motion Owner {zone}");
                    PrototypeArenaBoundaryBuilder.BuildRectangle(owners[zone].transform, center, sizes[zone], 0,
                        styles[zone], 1.1f, 1, zone == 0 ? 6 : 5, insets[zone]);
                    probes[zone] = CreateProbe($"Rectangle Motion Probe {zone}", center + Vector3.up);
                    var controller = probes[zone].GetComponent<CharacterController>();
                    var health = probes[zone].GetComponent<Health>();
                    Physics.SyncTransforms();

                    for (var step = 0; step < 40; step++) controller.Move(Vector3.right * .5f);
                    var clearRight = center.x + sizes[zone].x * .5f - insets[zone];
                    Assert.That(controller.transform.position.x, Is.LessThan(clearRight + .01f));
                    Assert.That(controller.transform.position.y, Is.EqualTo(1).Within(.01f));

                    controller.transform.position = new Vector3(clearRight - 1, 1, center.z); Physics.SyncTransforms();
                    controller.Move(Vector3.right * 6);
                    Assert.That(controller.transform.position.x, Is.LessThan(clearRight + .01f), "A factual fast motor move cannot tunnel through a straight run.");

                    var clearFar = center.z + sizes[zone].y * .5f - insets[zone];
                    controller.transform.position = new Vector3(clearRight - 1, 1, clearFar - 1); Physics.SyncTransforms();
                    controller.Move(new Vector3(6, 0, 6));
                    Assert.That(controller.transform.position.x, Is.LessThan(clearRight + .01f));
                    Assert.That(controller.transform.position.z, Is.LessThan(clearFar + .01f), "Overlapping runs must close the diagonal corner seam.");
                    Assert.That(health.Current, Is.EqualTo(100));
                }
            }
            finally
            {
                foreach (var probe in probes) if (probe) Object.Destroy(probe);
                foreach (var owner in owners) if (owner) Object.Destroy(owner);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator SylvanCorrectedJunctionsStayOpenWhileOuterPathAndNodeShouldersBlockMotion()
        {
            const float offset = 2000;
            var owner = new GameObject("Sylvan Motion Owner");
            var probe = CreateProbe("Sylvan Motion Probe", new Vector3(offset, 1, 5));
            try
            {
                PrototypeArenaBoundaryBuilder.BuildSylvan(owner.transform, Nodes(offset), Paths(offset));
                var controller = probe.GetComponent<CharacterController>();
                var health = probe.GetComponent<Health>();
                Physics.SyncTransforms();

                var moonwell = new Vector3(offset + 10, 1, 27);
                controller.Move(moonwell - controller.transform.position);
                Assert.That(HorizontalDistance(controller.transform.position, moonwell), Is.LessThan(.1f), "Root Path to Moonwell opening must remain clear.");
                var heart = new Vector3(offset, 1, 50);
                controller.Move(heart - controller.transform.position);
                Assert.That(HorizontalDistance(controller.transform.position, heart), Is.LessThan(.1f), "Moonwell to Heart Tree opening must remain clear.");

                var pathCenter = new Vector3(offset + 5, 1, 16);
                controller.transform.position = pathCenter; Physics.SyncTransforms();
                var pathRight = Quaternion.Euler(0, 22, 0) * Vector3.right;
                controller.Move(pathRight * 30);
                var lateral = Mathf.Abs(Vector3.Dot(controller.transform.position - pathCenter, pathRight));
                Assert.That(lateral, Is.LessThan(3.26f), "The 7-wide path must retain its 6.5-wide clear corridor and block its outer shoulder.");

                var portal = new Vector3(offset, 1, -50);
                controller.transform.position = portal; Physics.SyncTransforms();
                controller.Move(new Vector3(-20, 0, -20));
                Assert.That(HorizontalDistance(controller.transform.position, portal), Is.LessThan(3.3f), "An exposed node corner must be physically closed.");
                Assert.That(controller.transform.position.y, Is.EqualTo(1).Within(.01f));
                Assert.That(health.Current, Is.EqualTo(100));
            }
            finally { Object.Destroy(probe); Object.Destroy(owner); }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ExistingDashResolutionStopsAtBoundaryWithoutDamageOrActionOverride()
        {
            var owner = new GameObject("Dash Boundary Owner");
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            GameObject entityObject = null;
            try
            {
                var center = new Vector3(3000, 0, 1000);
                PrototypeArenaBoundaryBuilder.BuildRectangle(owner.transform, center, new Vector2(14, 20), 0,
                    PrototypeArenaBoundaryStyle.SylvanRoots, 1.2f, 1, 5, .4f);
                ability.DisplayName = "Boundary Dash"; ability.Kind = AbilityKind.Dash; ability.DashDistance = 6;
                ability.Windup = 0; ability.Cooldown = 0; ability.Range = 1; ability.Radius = .5f;
                definition.DisplayName = "Boundary Dash Entity";
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4, AttackSpeed = 1 };
                definition.Abilities = new[] { ability };
                var innerFace = center.x + 7 - .4f;
                entityObject = new GameObject("Boundary Dash Entity", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
                entityObject.transform.position = new Vector3(innerFace - 1, 1, center.z);
                var entity = entityObject.GetComponent<CombatEntity>(); entity.Initialize(definition);
                Physics.SyncTransforms();

                Assert.That(entity.TryUse(0, Vector3.right), Is.True);
                var timeout = Time.realtimeSinceStartup + 1;
                while (entity.IsActionResolving && Time.realtimeSinceStartup < timeout) yield return null;
                Assert.That(entity.IsActionResolving, Is.False, "The existing dash action must finish normally when collision shortens its displacement.");
                Assert.That(entity.transform.position.x, Is.LessThan(innerFace + .01f));
                Assert.That(entity.transform.position.x, Is.GreaterThan(innerFace - 1));
                Assert.That(entity.Health.Current, Is.EqualTo(100));
            }
            finally
            {
                if (entityObject) Object.Destroy(entityObject);
                Object.Destroy(definition); Object.Destroy(ability); Object.Destroy(owner);
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator ExistingCreatureBrainCannotDriveAcrossRectangleOrSylvanBoundary()
        {
            var previousTimeScale = Time.timeScale;
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var rectangleOwner = new GameObject("Rectangle AI Boundary Owner");
            var sylvanOwner = new GameObject("Sylvan AI Boundary Owner");
            GameObject rectangleGround = null, sylvanGround = null;
            GameObject rectangleAi = null, rectangleTarget = null, sylvanAi = null, sylvanTarget = null;
            try
            {
                Time.timeScale = 1;
                definition.DisplayName = "Boundary AI Fixture";
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4, AttackSpeed = 1 };
                definition.Abilities = System.Array.Empty<AbilityDefinition>();

                var rectangleCenter = new Vector3(4000, 0, 1000);
                var rectangleInnerFace = rectangleCenter.x + 7 - .4f;
                PrototypeArenaBoundaryBuilder.BuildRectangle(rectangleOwner.transform, rectangleCenter, new Vector2(14, 20), 0,
                    PrototypeArenaBoundaryStyle.SylvanRoots, 1.2f, 1, 5, .4f);
                rectangleGround = CreateGround("Rectangle AI Ground", rectangleCenter, new Vector3(14, .5f, 20), 0);
                rectangleAi = CreateEntity("Rectangle Boundary AI", new Vector3(rectangleInnerFace - 1.5f, 1, rectangleCenter.z), definition, true, out var rectangleBrain);
                rectangleTarget = CreateEntity("Rectangle Outside Target", new Vector3(rectangleInnerFace + 8, 1, rectangleCenter.z), definition, false, out _);
                rectangleBrain.Target = rectangleTarget.GetComponent<CombatEntity>();

                const float offset = 5000;
                var sylvanPathCenter = new Vector3(offset + 5, 0, 16);
                var pathRight = Quaternion.Euler(0, 22, 0) * Vector3.right;
                PrototypeArenaBoundaryBuilder.BuildSylvan(sylvanOwner.transform, Nodes(offset), Paths(offset));
                sylvanGround = CreateGround("Sylvan AI Ground", sylvanPathCenter, new Vector3(7, .5f, 26), 22);
                sylvanAi = CreateEntity("Sylvan Boundary AI", sylvanPathCenter + Vector3.up + pathRight * 1.75f, definition, true, out var sylvanBrain);
                sylvanTarget = CreateEntity("Sylvan Outside Target", sylvanPathCenter + Vector3.up + pathRight * 10, definition, false, out _);
                sylvanBrain.Target = sylvanTarget.GetComponent<CombatEntity>();
                Physics.SyncTransforms();

                var timeout = Time.time + 1.2f;
                while (Time.time < timeout) yield return null;

                Assert.That(rectangleBrain.State, Is.EqualTo(BrainState.Chase));
                Assert.That(rectangleAi.transform.position.x, Is.LessThan(rectangleInnerFace + .01f));
                Assert.That(rectangleAi.GetComponent<Health>().Current, Is.EqualTo(100));
                Assert.That(sylvanBrain.State, Is.EqualTo(BrainState.Chase));
                var sylvanLateral = Mathf.Abs(Vector3.Dot(sylvanAi.transform.position - (sylvanPathCenter + Vector3.up), pathRight));
                Assert.That(sylvanLateral, Is.LessThan(3.26f));
                Assert.That(sylvanAi.GetComponent<Health>().Current, Is.EqualTo(100));
            }
            finally
            {
                Time.timeScale = previousTimeScale;
                foreach (var item in new[] { rectangleAi, rectangleTarget, sylvanAi, sylvanTarget, rectangleGround, sylvanGround, rectangleOwner, sylvanOwner })
                    if (item) Object.Destroy(item);
                Object.Destroy(definition);
            }
            yield return null;
        }

        static GameObject CreateProbe(string name, Vector3 position)
        {
            var probe = new GameObject(name, typeof(CharacterController), typeof(Health));
            probe.transform.position = position;
            probe.GetComponent<Health>().Initialize(100);
            return probe;
        }

        static GameObject CreateEntity(string name, Vector3 position, CharacterDefinition definition,
            bool withBrain, out CreatureBrain brain)
        {
            var root = new GameObject(name); root.transform.position = position;
            root.AddComponent<CharacterController>(); root.AddComponent<Health>();
            var entity = root.AddComponent<CombatEntity>();
            brain = withBrain ? root.AddComponent<CreatureBrain>() : null;
            entity.Initialize(definition);
            if (brain) entity.SetController(brain);
            return root;
        }

        static GameObject CreateGround(string name, Vector3 center, Vector3 scale, float yaw)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name = name;
            ground.transform.position = center + Vector3.down * .25f;
            ground.transform.rotation = Quaternion.Euler(0, yaw, 0);
            ground.transform.localScale = scale;
            return ground;
        }

        static float HorizontalDistance(Vector3 first, Vector3 second)
        {
            return Vector2.Distance(new Vector2(first.x, first.z), new Vector2(second.x, second.z));
        }

        static ArenaCircleFootprint[] Nodes(float offset)
        {
            return new[]
            {
                new ArenaCircleFootprint(new Vector2(offset, -50), 3.25f),
                new ArenaCircleFootprint(new Vector2(offset, -30), 3.25f),
                new ArenaCircleFootprint(new Vector2(offset - 14, -10), 3.25f),
                new ArenaCircleFootprint(new Vector2(offset + 14, 4), 3.25f),
                new ArenaCircleFootprint(new Vector2(offset, 5), 3.25f),
                new ArenaCircleFootprint(new Vector2(offset + 10, 27), 3.25f),
                new ArenaCircleFootprint(new Vector2(offset, 50), 3.25f)
            };
        }

        static ArenaPathFootprint[] Paths(float offset)
        {
            return new[]
            {
                new ArenaPathFootprint(new Vector2(offset, -40), new Vector2(7, 20)),
                new ArenaPathFootprint(new Vector2(offset - 7, -20), new Vector2(6, 28), -35),
                new ArenaPathFootprint(new Vector2(offset + 7, -13), new Vector2(6, 38), 25),
                new ArenaPathFootprint(new Vector2(offset, -12), new Vector2(7, 36)),
                new ArenaPathFootprint(new Vector2(offset + 5, 16), new Vector2(7, 26), 22),
                new ArenaPathFootprint(new Vector2(offset + 5, 39), new Vector2(7, 25), -24)
            };
        }
    }
}
