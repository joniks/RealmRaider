using NUnit.Framework;
using RealmRaiders.Core;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class PrototypeArenaBoundaryBuilderTests
    {
        [Test]
        public void RectangleBuildHasFourOverlappingStaticRunsAndOneCombinedVisual()
        {
            var owner = new GameObject("Rectangle Boundary Owner");
            try
            {
                var boundary = PrototypeArenaBoundaryBuilder.BuildRectangle(owner.transform, Vector3.zero,
                    new Vector2(22, 28), 0, PrototypeArenaBoundaryStyle.NeutralStone, 1.1f, .9f, 6, .5f);
                var repeat = PrototypeArenaBoundaryBuilder.BuildRectangle(owner.transform, Vector3.zero,
                    new Vector2(22, 28), 0, PrototypeArenaBoundaryStyle.NeutralStone, 1.1f, .9f, 6, .5f);

                Assert.That(repeat, Is.SameAs(boundary));
                Assert.That(boundary.name, Is.EqualTo(PrototypeArenaBoundaryBuilder.BoundaryName));
                Assert.That(boundary.GetComponentsInChildren<Transform>(true), Has.Length.EqualTo(6));
                Assert.That(boundary.GetComponentsInChildren<MeshRenderer>(true), Has.Length.EqualTo(1));
                Assert.That(boundary.GetComponentsInChildren<MeshFilter>(true), Has.Length.EqualTo(1));
                var mesh = boundary.GetComponentInChildren<MeshFilter>().sharedMesh;
                Assert.That(mesh.triangles.Length / 3, Is.LessThanOrEqualTo(512));

                var colliders = boundary.GetComponentsInChildren<BoxCollider>(true);
                Assert.That(colliders, Has.Length.EqualTo(4));
                foreach (var collider in colliders)
                {
                    Assert.That(collider.isTrigger, Is.False);
                    Assert.That(collider.gameObject.isStatic, Is.True);
                    Assert.That(collider.size.y, Is.EqualTo(6).Within(.001f));
                }
                Assert.That(colliders[0].transform.localPosition.z, Is.EqualTo(-13.95f).Within(.001f));
                Assert.That(colliders[1].transform.localPosition.x, Is.EqualTo(10.95f).Within(.001f));
                Assert.That(colliders[2].transform.localPosition.z, Is.EqualTo(13.95f).Within(.001f));
                Assert.That(colliders[3].transform.localPosition.x, Is.EqualTo(-10.95f).Within(.001f));
                Assert.That(colliders[0].size.z, Is.EqualTo(21 + PrototypeArenaBoundaryBuilder.CornerOverlap * 2).Within(.001f));
                Assert.That(colliders[1].size.z, Is.EqualTo(27 + PrototypeArenaBoundaryBuilder.CornerOverlap * 2).Within(.001f));
                Physics.SyncTransforms();
                var cornerOverlapX = Mathf.Min(colliders[0].bounds.max.x, colliders[1].bounds.max.x)
                    - Mathf.Max(colliders[0].bounds.min.x, colliders[1].bounds.min.x);
                var cornerOverlapZ = Mathf.Min(colliders[0].bounds.max.z, colliders[1].bounds.max.z)
                    - Mathf.Max(colliders[0].bounds.min.z, colliders[1].bounds.min.z);
                Assert.That(cornerOverlapX, Is.GreaterThanOrEqualTo(.1f));
                Assert.That(cornerOverlapZ, Is.GreaterThanOrEqualTo(.1f));
                AssertSafeGeneratedComponents(boundary);
            }
            finally { Object.DestroyImmediate(owner); }
        }

        [Test]
        public void SylvanFactualCorrectionChangesThreeComponentsToOneAndMeetsBudget()
        {
            var owner = new GameObject("Sylvan Boundary Owner");
            try
            {
                Assert.That(PrototypeArenaBoundaryBuilder.CountFootprintComponents(Nodes(), Paths(false)), Is.EqualTo(3));
                Assert.That(PrototypeArenaBoundaryBuilder.CountFootprintComponents(Nodes(), Paths(true)), Is.EqualTo(1));
                Assert.Throws<System.InvalidOperationException>(() =>
                    PrototypeArenaBoundaryBuilder.BuildSylvan(owner.transform, Nodes(), Paths(false)));
                Assert.That(owner.transform.childCount, Is.Zero, "The builder must not bridge or partially build a disconnected factual footprint.");

                var boundary = PrototypeArenaBoundaryBuilder.BuildSylvan(owner.transform, Nodes(), Paths(true));
                var colliders = boundary.GetComponentsInChildren<BoxCollider>(true);
                Assert.That(colliders.Length, Is.GreaterThan(20).And.LessThanOrEqualTo(PrototypeArenaBoundaryBuilder.SylvanColliderBudget));
                Assert.That(boundary.GetComponentsInChildren<Transform>(true).Length, Is.LessThanOrEqualTo(58));
                Assert.That(boundary.GetComponentsInChildren<MeshRenderer>(true), Has.Length.EqualTo(1));
                Assert.That(boundary.GetComponentsInChildren<MeshFilter>(true), Has.Length.EqualTo(1));
                Assert.That(boundary.GetComponentInChildren<MeshFilter>().sharedMesh.triangles.Length / 3, Is.LessThanOrEqualTo(4000));
                foreach (var collider in colliders)
                {
                    Assert.That(collider.isTrigger, Is.False);
                    Assert.That(collider.gameObject.isStatic, Is.True);
                    Assert.That(collider.size.y, Is.EqualTo(5).Within(.001f));
                }
                var portalCenter = new Vector3(0, 0, -50);
                BoxCollider southPortalRun = null;
                var southZ = float.MaxValue;
                foreach (var collider in colliders)
                {
                    var innerCenter = collider.transform.position - collider.transform.right * (collider.size.x * .5f);
                    if (innerCenter.z >= southZ) continue;
                    southZ = innerCenter.z;
                    southPortalRun = collider;
                }
                Assert.That(southPortalRun, Is.Not.Null);
                var innerMidpoint = southPortalRun.transform.position - southPortalRun.transform.right * (southPortalRun.size.x * .5f);
                var halfRun = (southPortalRun.size.z - PrototypeArenaBoundaryBuilder.CornerOverlap * 2) * .5f;
                var firstEndpoint = innerMidpoint - southPortalRun.transform.forward * halfRun;
                var secondEndpoint = innerMidpoint + southPortalRun.transform.forward * halfRun;
                Assert.That(Vector2.Distance(new Vector2(firstEndpoint.x, firstEndpoint.z), new Vector2(portalCenter.x, portalCenter.z)), Is.LessThanOrEqualTo(3.01f));
                Assert.That(Vector2.Distance(new Vector2(secondEndpoint.x, secondEndpoint.z), new Vector2(portalCenter.x, portalCenter.z)), Is.LessThanOrEqualTo(3.01f),
                    "The exposed node collider inner face must apply the 0.25 inset to the factual 3.25 radius.");
                Assert.That(PrototypeArenaBoundaryBuilder.BuildSylvan(owner.transform, Nodes(), Paths(true)), Is.SameAs(boundary));
                AssertSafeGeneratedComponents(boundary);
            }
            finally { Object.DestroyImmediate(owner); }
        }

        [Test]
        public void SylvanStyleMaterialIsLazilySharedAcrossSceneOwners()
        {
            var firstOwner = new GameObject("First Sylvan Owner");
            var secondOwner = new GameObject("Second Sylvan Owner");
            try
            {
                var first = PrototypeArenaBoundaryBuilder.BuildRectangle(firstOwner.transform, Vector3.zero,
                    new Vector2(14, 68), 0, PrototypeArenaBoundaryStyle.SylvanRoots, 1.2f, 1, 5, .4f);
                var second = PrototypeArenaBoundaryBuilder.BuildRectangle(secondOwner.transform, Vector3.zero,
                    new Vector2(14, 68), 0, PrototypeArenaBoundaryStyle.SylvanRoots, 1.2f, 1, 5, .4f);
                Assert.That(first.GetComponentInChildren<MeshRenderer>().sharedMaterial,
                    Is.SameAs(second.GetComponentInChildren<MeshRenderer>().sharedMaterial));
            }
            finally { Object.DestroyImmediate(firstOwner); Object.DestroyImmediate(secondOwner); }
        }

        static void AssertSafeGeneratedComponents(GameObject boundary)
        {
            Assert.That(boundary.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
            Assert.That(boundary.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
            Assert.That(boundary.GetComponentsInChildren<MeshCollider>(true), Is.Empty);
            Assert.That(boundary.GetComponentsInChildren<Camera>(true), Is.Empty);
            Assert.That(boundary.GetComponentsInChildren<Light>(true), Is.Empty);
            Assert.That(boundary.GetComponentsInChildren<AudioSource>(true), Is.Empty);
            Assert.That(boundary.GetComponentsInChildren<AudioListener>(true), Is.Empty);
        }

        internal static ArenaCircleFootprint[] Nodes(float offset = 0)
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

        internal static ArenaPathFootprint[] Paths(bool corrected, float offset = 0)
        {
            return new[]
            {
                new ArenaPathFootprint(new Vector2(offset, -40), new Vector2(7, 20)),
                new ArenaPathFootprint(new Vector2(offset - 7, -20), new Vector2(6, 28), -35),
                new ArenaPathFootprint(new Vector2(offset + 7, -13), new Vector2(6, 38), 25),
                new ArenaPathFootprint(new Vector2(offset, -12), new Vector2(7, 36)),
                new ArenaPathFootprint(new Vector2(offset + 5, 16), new Vector2(7, 26), corrected ? 22 : -22),
                new ArenaPathFootprint(new Vector2(offset + 5, 39), new Vector2(7, 25), corrected ? -24 : 24)
            };
        }
    }
}
