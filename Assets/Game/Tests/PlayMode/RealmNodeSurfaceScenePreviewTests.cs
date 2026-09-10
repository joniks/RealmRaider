using System.Collections;
using System.Linq;
using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Realm;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealmRaiders.Tests
{
    public sealed class RealmNodeSurfaceScenePreviewTests
    {
        static readonly Color Moss = new(.14f, .4f, .16f);

        [UnityTest]
        public IEnumerator MultipleSylvanNodeSurfacesPreserveAuthorityGeometryCollidersAndEncounterFlow()
        {
            RealmNodeSurfacePresentation.ResetTextureLoaderForTests();
            var albedo = Resources.Load<Texture2D>(RealmNodeSurfacePresentation.AlbedoResource);
            var normal = Resources.Load<Texture2D>(RealmNodeSurfacePresentation.NormalResource);
            var heroDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var hostileDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var heroObject = new GameObject("Node Surface Hero", typeof(CharacterController), typeof(Health),
                typeof(CombatEntity), typeof(CreatureBrain));
            var hostileObject = new GameObject("Explicit Node Hostile", typeof(CharacterController), typeof(Health),
                typeof(CombatEntity));
            var firstArea = new GameObject("Portal Surface Node");
            var secondArea = new GameObject("Wolf Grove Surface Node");
            try
            {
                Assert.That(albedo, Is.Not.Null);
                Assert.That(normal, Is.Not.Null);
                heroDefinition.DisplayName = "Surface Authority Hero";
                heroDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3 };
                hostileDefinition.DisplayName = "Surface Explicit Hostile";
                hostileDefinition.Stats = new CombatStats { MaxHealth = 40, MoveSpeed = 0 };
                var hero = heroObject.GetComponent<CombatEntity>(); hero.Initialize(heroDefinition);
                var authority = hero.Controller<CreatureBrain>(); hero.SetController(authority); hero.Motor.enabled = false;
                var authoritativeMotor = hero.Motor;
                var hostile = hostileObject.GetComponent<CombatEntity>(); hostile.Initialize(hostileDefinition);
                heroObject.transform.position = new Vector3(0, 0, -20);
                firstArea.transform.position = Vector3.zero;
                secondArea.transform.position = new Vector3(0, 0, 20);

                var firstFloor = CreateAuthoritativeFloor(firstArea.transform, "PORTAL Ground");
                var secondFloor = CreateAuthoritativeFloor(secondArea.transform, "WOLF GROVE Ground");
                var firstSnapshot = Snapshot(firstFloor);
                var secondSnapshot = Snapshot(secondFloor);
                var firstSurface = RealmNodeSurfacePresentation.Bind(firstFloor.GetComponent<Renderer>(), Moss);
                var secondSurface = RealmNodeSurfacePresentation.Bind(secondFloor.GetComponent<Renderer>(), Moss);
                var firstNode = firstArea.AddComponent<RealmNodeView>();
                var secondNode = secondArea.AddComponent<RealmNodeView>();
                firstNode.Initialize(new RealmNode("Portal"), hero, firstFloor.GetComponent<Renderer>());
                secondNode.Initialize(new RealmNode("Wolf Grove"), hero, secondFloor.GetComponent<Renderer>(), hostileObject);
                var firstEntries = 0;
                var secondEntries = 0;
                RealmNodeVisit wolfVisit = null;
                firstNode.EncounterEntered += _ => firstEntries++;
                secondNode.EncounterEntered += visit => { secondEntries++; wolfVisit = visit; };
                yield return null;

                Assert.That(firstSurface.UsesSurface && secondSurface.UsesSurface, Is.True);
                Assert.That(firstFloor.GetComponent<Renderer>().sharedMaterial, Is.SameAs(secondFloor.GetComponent<Renderer>().sharedMaterial));
                Assert.That(firstFloor.GetComponent<Renderer>().sharedMaterial.mainTexture, Is.SameAs(albedo));
                Assert.That(firstFloor.GetComponent<Renderer>().sharedMaterial.GetTexture("_BumpMap"), Is.SameAs(normal));
                AssertFloorUnchanged(firstFloor, firstSnapshot);
                AssertFloorUnchanged(secondFloor, secondSnapshot);
                Assert.That(firstFloor.GetComponentsInChildren<RealmNodeSurfacePresentation>(true), Has.Length.EqualTo(1));
                Assert.That(secondFloor.GetComponentsInChildren<RealmNodeSurfacePresentation>(true), Has.Length.EqualTo(1));
                Assert.That(firstSurface.transform.childCount, Is.EqualTo(firstSnapshot.ChildCount));
                Assert.That(secondSurface.transform.childCount, Is.EqualTo(secondSnapshot.ChildCount));
                Assert.That(hero.ActiveController, Is.SameAs(authority));
                Assert.That(hero.Motor, Is.SameAs(authoritativeMotor));
                Assert.That(hero.Motor.enabled, Is.False);

                heroObject.transform.position = firstArea.transform.position;
                Physics.SyncTransforms();
                yield return null;
                Assert.That(firstEntries, Is.EqualTo(1));
                Assert.That(firstNode.HasBeenEntered, Is.True);
                Assert.That(firstFloor.GetComponent<Renderer>().enabled, Is.True);

                heroObject.transform.position = secondArea.transform.position;
                Physics.SyncTransforms();
                yield return null;
                Assert.That(secondEntries, Is.EqualTo(1));
                Assert.That(wolfVisit, Is.Not.Null);
                Assert.That(wolfVisit.NodeId, Is.EqualTo("Wolf Grove"));
                Assert.That(wolfVisit.AliveHostiles, Has.Count.EqualTo(1));
                Assert.That(wolfVisit.AliveHostiles[0], Is.SameAs(hostile));
                Assert.That(hero.ActiveController, Is.SameAs(authority));
                Assert.That(hero.Motor, Is.SameAs(authoritativeMotor));
                AssertFloorUnchanged(firstFloor, firstSnapshot);
                AssertFloorUnchanged(secondFloor, secondSnapshot);
            }
            finally
            {
                Object.Destroy(firstArea);
                Object.Destroy(secondArea);
                Object.Destroy(hostileObject);
                Object.Destroy(heroObject);
                Object.Destroy(heroDefinition);
                Object.Destroy(hostileDefinition);
                RealmNodeSurfacePresentation.ResetTextureLoaderForTests();
            }
            yield return null;
        }

        static GameObject CreateAuthoritativeFloor(Transform parent, string name)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floor.name = name;
            floor.transform.SetParent(parent, false);
            floor.transform.localPosition = Vector3.zero;
            floor.transform.localScale = new Vector3(RealmNodeSurfacePresentation.NodeWorldDiameter, .08f,
                RealmNodeSurfacePresentation.NodeWorldDiameter);
            var primitiveCollider = floor.GetComponent<Collider>(); primitiveCollider.enabled = false;
            var groundColliderObject = new GameObject("Node Ground Collider", typeof(MeshCollider));
            groundColliderObject.transform.SetParent(floor.transform, false);
            groundColliderObject.transform.localPosition = Vector3.down;
            groundColliderObject.isStatic = true;
            var groundCollider = groundColliderObject.GetComponent<MeshCollider>();
            groundCollider.sharedMesh = floor.GetComponent<MeshFilter>().sharedMesh;
            groundCollider.convex = false;
            groundCollider.isTrigger = false;
            return floor;
        }

        static FloorSnapshot Snapshot(GameObject floor) => new(
            floor.transform.parent, floor.transform.localPosition, floor.transform.localRotation,
            floor.transform.localScale, floor.GetComponent<MeshFilter>().sharedMesh,
            floor.GetComponentsInChildren<Collider>(true), floor.transform.childCount, floor.layer);

        static void AssertFloorUnchanged(GameObject floor, FloorSnapshot snapshot)
        {
            Assert.That(floor.transform.parent, Is.SameAs(snapshot.Parent));
            Assert.That(floor.transform.localPosition, Is.EqualTo(snapshot.LocalPosition));
            Assert.That(floor.transform.localRotation, Is.EqualTo(snapshot.LocalRotation));
            Assert.That(floor.transform.localScale, Is.EqualTo(snapshot.LocalScale));
            Assert.That(floor.GetComponent<MeshFilter>().sharedMesh, Is.SameAs(snapshot.Mesh));
            CollectionAssert.AreEqual(snapshot.Colliders.Select(item => item.Collider).ToArray(),
                floor.GetComponentsInChildren<Collider>(true),
                "Node presentation must not add, remove or replace authoritative colliders.");
            foreach (var collider in snapshot.Colliders) collider.AssertUnchanged();
            Assert.That(floor.transform.childCount, Is.EqualTo(snapshot.ChildCount));
            Assert.That(floor.layer, Is.EqualTo(snapshot.Layer));
        }

        readonly struct FloorSnapshot
        {
            public readonly Transform Parent;
            public readonly Vector3 LocalPosition;
            public readonly Quaternion LocalRotation;
            public readonly Vector3 LocalScale;
            public readonly Mesh Mesh;
            public readonly ColliderSnapshot[] Colliders;
            public readonly int ChildCount;
            public readonly int Layer;

            public FloorSnapshot(Transform parent, Vector3 localPosition, Quaternion localRotation, Vector3 localScale,
                Mesh mesh, Collider[] colliders, int childCount, int layer)
            {
                Parent = parent;
                LocalPosition = localPosition;
                LocalRotation = localRotation;
                LocalScale = localScale;
                Mesh = mesh;
                Colliders = colliders.Select(ColliderSnapshot.Capture).ToArray();
                ChildCount = childCount;
                Layer = layer;
            }
        }

        readonly struct ColliderSnapshot
        {
            public readonly Collider Collider;
            readonly bool enabled;
            readonly bool isTrigger;
            readonly Vector3 localPosition;
            readonly Quaternion localRotation;
            readonly Vector3 localScale;
            readonly Mesh sharedMesh;
            readonly bool convex;
            readonly Vector3 center;
            readonly float radius;
            readonly float height;
            readonly int direction;

            ColliderSnapshot(Collider collider)
            {
                Collider = collider;
                enabled = collider.enabled;
                isTrigger = collider.isTrigger;
                localPosition = collider.transform.localPosition;
                localRotation = collider.transform.localRotation;
                localScale = collider.transform.localScale;
                var mesh = collider as MeshCollider;
                sharedMesh = mesh ? mesh.sharedMesh : null;
                convex = mesh && mesh.convex;
                var capsule = collider as CapsuleCollider;
                center = capsule ? capsule.center : Vector3.zero;
                radius = capsule ? capsule.radius : 0;
                height = capsule ? capsule.height : 0;
                direction = capsule ? capsule.direction : 0;
            }

            public static ColliderSnapshot Capture(Collider collider) => new(collider);

            public void AssertUnchanged()
            {
                Assert.That(Collider.enabled, Is.EqualTo(enabled));
                Assert.That(Collider.isTrigger, Is.EqualTo(isTrigger));
                Assert.That(Collider.transform.localPosition, Is.EqualTo(localPosition));
                Assert.That(Collider.transform.localRotation, Is.EqualTo(localRotation));
                Assert.That(Collider.transform.localScale, Is.EqualTo(localScale));
                if (Collider is MeshCollider mesh)
                {
                    Assert.That(mesh.sharedMesh, Is.SameAs(sharedMesh));
                    Assert.That(mesh.convex, Is.EqualTo(convex));
                }
                if (Collider is CapsuleCollider capsule)
                {
                    Assert.That(capsule.center, Is.EqualTo(center));
                    Assert.That(capsule.radius, Is.EqualTo(radius));
                    Assert.That(capsule.height, Is.EqualTo(height));
                    Assert.That(capsule.direction, Is.EqualTo(direction));
                }
            }
        }
    }
}
