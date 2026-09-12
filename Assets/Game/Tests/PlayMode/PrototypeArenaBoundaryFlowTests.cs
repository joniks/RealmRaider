using System.Collections;
using System.Linq;
using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
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
            var identityKey = SylvanStarterRealmIdentity.KeyForTests;
            var hadIdentity = PlayerPrefs.HasKey(identityKey);
            var previousIdentity = PlayerPrefs.GetString(identityKey, string.Empty);
            var owner = new GameObject("Sylvan Motion Owner");
            GameObject probe = null;
            try
            {
                SylvanStarterRealmIdentity.ResetForTests();
                SylvanStarterRealmIdentity.SetFactoriesForTests(() => "234567890abcdef1234567890abcdef1", () => 0);
                var layout = SylvanRealmLayoutMaterializer.Create(SylvanStarterRealmIdentity.LoadOrCreate());
                var nodes = Offset(layout.CreateNodeFootprints(), offset);
                var paths = Offset(layout.CreatePathFootprints(), offset);
                var junctionMoon = layout.Edges.Single(edge => Connects(edge, layout.LandmarkJunction, layout.MoonwellRecovery));
                var moonEnt = layout.Edges.Single(edge => Connects(edge, layout.MoonwellRecovery, layout.EntGroveEncounter));
                var portalEdge = layout.Edges.Single(edge => edge.From == layout.PortalStart || edge.To == layout.PortalStart);
                probe = CreateProbe("Sylvan Motion Probe", World(layout.LandmarkJunction.Center, offset));
                PrototypeArenaBoundaryBuilder.BuildSylvan(owner.transform, nodes, paths);
                var controller = probe.GetComponent<CharacterController>();
                var health = probe.GetComponent<Health>();
                Physics.SyncTransforms();

                var moonwell = World(layout.MoonwellRecovery.Center, offset);
                controller.Move(moonwell - controller.transform.position);
                Assert.That(HorizontalDistance(controller.transform.position, moonwell), Is.LessThan(.1f), "A declared junction-to-Moonwell edge must remain clear.");
                Assert.That(junctionMoon, Is.Not.Null);
                var ent = World(layout.EntGroveEncounter.Center, offset);
                controller.Move(ent - controller.transform.position);
                Assert.That(HorizontalDistance(controller.transform.position, ent), Is.LessThan(.1f), "A declared Moonwell-to-Ent edge must remain clear.");
                Assert.That(moonEnt, Is.Not.Null);

                var pathCenter = World(junctionMoon.Footprint.Center, offset);
                controller.transform.position = pathCenter; Physics.SyncTransforms();
                var pathRight = Quaternion.Euler(0, junctionMoon.Footprint.Yaw, 0) * Vector3.right;
                controller.Move(pathRight * 30);
                var lateral = Mathf.Abs(Vector3.Dot(controller.transform.position - pathCenter, pathRight));
                Assert.That(lateral, Is.LessThan(junctionMoon.FloorPathWidth * .5f - .24f), "The factual path must block its outer shoulder.");

                var portal = World(layout.PortalStart.Center, offset);
                controller.transform.position = portal; Physics.SyncTransforms();
                var neighbor = portalEdge.From == layout.PortalStart ? portalEdge.To : portalEdge.From;
                var away = (layout.PortalStart.Center - neighbor.Center).normalized;
                controller.Move(new Vector3(away.x, 0, away.y) * 20);
                Assert.That(HorizontalDistance(controller.transform.position, portal), Is.LessThan(3.3f), "An exposed node corner must be physically closed.");
                Assert.That(controller.transform.position.y, Is.EqualTo(1).Within(.01f));
                Assert.That(health.Current, Is.EqualTo(100));
            }
            finally
            {
                if (probe) Object.Destroy(probe);
                Object.Destroy(owner);
                SylvanStarterRealmIdentity.ResetForTests();
                if (hadIdentity) PlayerPrefs.SetString(identityKey, previousIdentity); else PlayerPrefs.DeleteKey(identityKey);
                PlayerPrefs.Save();
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator SylvanPortalToCrossroadsSupportsNormalMovementAndDashWithoutStateDamage()
        {
            var previousTimeScale = Time.timeScale;
            var identityKey = SylvanStarterRealmIdentity.KeyForTests;
            var hadIdentity = PlayerPrefs.HasKey(identityKey);
            var previousIdentity = PlayerPrefs.GetString(identityKey, string.Empty);
            try
            {
                SylvanStarterRealmIdentity.ResetForTests();
                SylvanStarterRealmIdentity.SetFactoriesForTests(() => "1234567890abcdef1234567890abcdef", () => 1);
                Time.timeScale = 1;
                GameplayInput.SetTerminalState(false);
                GameplayInput.ClearMovement();
                SceneManager.LoadScene("SylvanRealm");
                yield return null;
                yield return null;

                var heroObject = GameObject.Find("Blood Knight");
                Assert.That(heroObject, Is.Not.Null);
                var hero = heroObject.GetComponent<CombatEntity>();
                var player = hero.Controller<PlayerController>();
                var layout = SylvanRealmLayoutMaterializer.Create(SylvanStarterRealmIdentity.LoadOrCreate());
                var firstEdge = layout.Edges.Single(edge => edge.From == layout.PortalStart || edge.To == layout.PortalStart);
                var destinationNode = firstEdge.From == layout.PortalStart ? firstEdge.To : firstEdge.From;
                var corridorDirection = (destinationNode.Center - layout.PortalStart.Center).normalized;
                var portal = GameObject.Find("PORTAL");
                var crossroads = GameObject.Find("CROSSROADS");
                var crossroadsGround = GameObject.Find("CROSSROADS Ground");
                Assert.That(player, Is.Not.Null);
                Assert.That(player.IsActive, Is.True);
                Assert.That(portal, Is.Not.Null);
                Assert.That(crossroads, Is.Not.Null);
                Assert.That(crossroadsGround, Is.Not.Null);
                Assert.That(crossroadsGround.GetComponent<CapsuleCollider>(), Is.Null,
                    "A flattened Cylinder CapsuleCollider must not wall off the node rim.");
                var crossroadsSurface = crossroadsGround.GetComponentInChildren<MeshCollider>();
                Assert.That(crossroadsSurface, Is.Not.Null);
                Assert.That(crossroadsSurface.sharedMesh, Is.SameAs(crossroadsGround.GetComponent<MeshFilter>().sharedMesh));
                Assert.That(crossroadsSurface.bounds.max.y, Is.EqualTo(0).Within(.01f),
                    "The physical node surface must meet the path surface without a raised seam.");
                foreach (var area in new[] { portal, crossroads })
                    foreach (Transform child in area.transform)
                        if (child.name == "Tree")
                        {
                            Assert.That(child.GetComponent<Renderer>(), Is.Not.Null);
                            Assert.That(child.GetComponent<Collider>(), Is.Null, "Route decoration must not own movement collision.");
                        }

                var health = hero.Health.Current;
                var movementTimeout = Time.realtimeSinceStartup + 6;
                while (PlanarProgress(hero.transform.position, layout.PortalStart.Center, corridorDirection) <
                       Vector2.Distance(layout.PortalStart.Center, destinationNode.Center) - .25f && Time.realtimeSinceStartup < movementTimeout)
                {
                    GameplayInput.SetMovement(corridorDirection);
                    player.Tick();
                    GameplayInput.ClearMovement();
                    yield return null;
                    Assert.That(IsInsideFirstCorridorFootprint(hero.transform.position, firstEdge), Is.True,
                        $"Normal movement left the Portal/Crossroads footprint at {hero.transform.position}.");
                }
                GameplayInput.ClearMovement();
                Assert.That(PlanarProgress(hero.transform.position, layout.PortalStart.Center, corridorDirection),
                    Is.GreaterThanOrEqualTo(Vector2.Distance(layout.PortalStart.Center, destinationNode.Center) - .25f),
                    "Normal player movement must cross the factual Portal to Crossroads route.");
                Assert.That(IsInsideFirstCorridorFootprint(hero.transform.position, firstEdge), Is.True);
                Assert.That(hero.Health.Current, Is.EqualTo(health));
                Assert.That(hero.IsActionResolving, Is.False);

                hero.transform.SetPositionAndRotation(new Vector3(layout.PortalStart.Center.x, 1, layout.PortalStart.Center.y), Quaternion.identity);
                Physics.SyncTransforms();
                var dashDirection = new Vector3(corridorDirection.x, 0, corridorDirection.y);
                Assert.That(hero.TryUse(1, dashDirection), Is.True, "The existing Blood Rush must start from the Portal.");
                var actionTimeout = Time.realtimeSinceStartup + 2;
                while (hero.IsActionResolving && Time.realtimeSinceStartup < actionTimeout) yield return null;
                Assert.That(hero.IsActionResolving, Is.False);
                Assert.That(PlanarProgress(hero.transform.position, layout.PortalStart.Center, corridorDirection), Is.GreaterThan(5.75f),
                    "The existing dash must cross the Portal node edge into the first corridor.");
                Assert.That(hero.Health.Current, Is.EqualTo(health));
            }
            finally
            {
                GameplayInput.ClearMovement();
                GameplayInput.SetTerminalState(false);
                Time.timeScale = previousTimeScale;
                SylvanStarterRealmIdentity.ResetForTests();
                if (hadIdentity) PlayerPrefs.SetString(identityKey, previousIdentity); else PlayerPrefs.DeleteKey(identityKey);
                PlayerPrefs.Save();
            }

            SceneManager.LoadScene("RealmBuild");
            yield return null;
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
            var identityKey = SylvanStarterRealmIdentity.KeyForTests;
            var hadIdentity = PlayerPrefs.HasKey(identityKey);
            var previousIdentity = PlayerPrefs.GetString(identityKey, string.Empty);
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
                SylvanStarterRealmIdentity.ResetForTests();
                SylvanStarterRealmIdentity.SetFactoriesForTests(() => "34567890abcdef1234567890abcdef12", () => 0);
                var layout = SylvanRealmLayoutMaterializer.Create(SylvanStarterRealmIdentity.LoadOrCreate());
                var testedPath = layout.Edges.Single(edge => Connects(edge, layout.LandmarkJunction, layout.MoonwellRecovery)).Footprint;
                var sylvanPathCenter = new Vector3(testedPath.Center.x + offset, 0, testedPath.Center.y);
                var pathRight = Quaternion.Euler(0, testedPath.Yaw, 0) * Vector3.right;
                PrototypeArenaBoundaryBuilder.BuildSylvan(sylvanOwner.transform,
                    Offset(layout.CreateNodeFootprints(), offset), Offset(layout.CreatePathFootprints(), offset));
                sylvanGround = CreateGround("Sylvan AI Ground", sylvanPathCenter, new Vector3(testedPath.Size.x, .5f, testedPath.Size.y), testedPath.Yaw);
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
                SylvanStarterRealmIdentity.ResetForTests();
                if (hadIdentity) PlayerPrefs.SetString(identityKey, previousIdentity); else PlayerPrefs.DeleteKey(identityKey);
                PlayerPrefs.Save();
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

        static float PlanarProgress(Vector3 position, Vector2 origin, Vector2 direction) =>
            Vector2.Dot(new Vector2(position.x, position.z) - origin, direction);

        static bool IsInsideFirstCorridorFootprint(Vector3 position, SylvanRealmMaterializationEdge edge)
        {
            var point = new Vector2(position.x, position.z);
            const float tolerance = .05f;
            if (Vector2.Distance(point, edge.From.Center) <= SylvanRealmLayoutMaterializer.NodeRadius + tolerance ||
                Vector2.Distance(point, edge.To.Center) <= SylvanRealmLayoutMaterializer.NodeRadius + tolerance) return true;
            var rotation = Quaternion.Euler(0, -edge.Footprint.Yaw, 0);
            var local3 = rotation * new Vector3(point.x - edge.Footprint.Center.x, 0, point.y - edge.Footprint.Center.y);
            return Mathf.Abs(local3.x) <= edge.Footprint.Size.x * .5f + tolerance &&
                   Mathf.Abs(local3.z) <= edge.Footprint.Size.y * .5f + tolerance;
        }

        static bool Connects(SylvanRealmMaterializationEdge edge, SylvanRealmMaterializationNode first,
            SylvanRealmMaterializationNode second) => edge.From == first && edge.To == second || edge.From == second && edge.To == first;

        static Vector3 World(Vector2 center, float offset) => new(center.x + offset, 1, center.y);

        static ArenaCircleFootprint[] Offset(ArenaCircleFootprint[] source, float offset)
        {
            return source.Select(item => new ArenaCircleFootprint(item.Center + Vector2.right * offset, item.Radius)).ToArray();
        }

        static ArenaPathFootprint[] Offset(ArenaPathFootprint[] source, float offset)
        {
            return source.Select(item => new ArenaPathFootprint(item.Center + Vector2.right * offset, item.Size, item.Yaw)).ToArray();
        }
    }
}
