using System.Linq;
using NUnit.Framework;
using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RealmRaiders.Tests
{
    public sealed class RealmRoutePresentationTests
    {
        [TestCase(RealmRouteStyle.SylvanOrganic, "Organic Lane Mass 1")]
        [TestCase(RealmRouteStyle.InfernalFractured, "Basalt Causeway Plate 1")]
        public void DefenseLaneIsIdempotentBoundedAndKeepsAuthoritativeSurface(RealmRouteStyle style, string expectedPart)
        {
            var root = CreateAuthoritativeRoute();
            var snapshot = Snapshot(root);
            try
            {
                var presentation = RealmRoutePresentation.BuildDefenseLane(root.transform, style);
                var repeated = RealmRoutePresentation.BuildDefenseLane(root.transform, style);

                Assert.That(repeated, Is.SameAs(presentation));
                Assert.That(root.transform.Cast<Transform>().Count(child => child.name == RealmRoutePresentation.RootName), Is.EqualTo(1));
                Assert.That(presentation.GetComponentsInChildren<Renderer>(true), Has.Length.EqualTo(RealmRoutePresentation.DefenseRendererCeiling));
                Assert.That(presentation.Find(expectedPart), Is.Not.Null);
                AssertPresentationOnly(presentation);
                AssertAuthoritativeState(root, snapshot, true);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void CoveredRaidSegmentDisablesOnlyRendererAndRetainsCenterYawWidthLengthAndCollider()
        {
            var root = CreateAuthoritativeRoute();
            root.transform.localScale = new Vector3(7, .12f, 20);
            var snapshot = Snapshot(root);
            try
            {
                var presentation = RealmRoutePresentation.BuildSegment(root.transform, RealmRouteStyle.SylvanOrganic);
                var band = presentation.Find("Organic Route Band");

                Assert.That(presentation.GetComponentsInChildren<Renderer>(true), Has.Length.EqualTo(RealmRoutePresentation.SegmentRendererCeiling));
                Assert.That(band, Is.Not.Null);
                Assert.That(band.localPosition.x, Is.EqualTo(0).Within(.0001f));
                Assert.That(band.localPosition.z, Is.EqualTo(0).Within(.0001f));
                Assert.That(band.localScale.x, Is.EqualTo(7).Within(.0001f));
                Assert.That(band.localScale.y * 2, Is.EqualTo(20).Within(.0001f));
                AssertPresentationOnly(presentation);
                AssertAuthoritativeState(root, snapshot, false);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [Test]
        public void StylesBuildDeterministicallyWithDistinctPrimitivePlacementAndSharedMaterials()
        {
            var firstSylvan = CreateAuthoritativeRoute();
            var secondSylvan = CreateAuthoritativeRoute();
            var infernal = CreateAuthoritativeRoute();
            try
            {
                var first = RealmRoutePresentation.BuildDefenseLane(firstSylvan.transform, RealmRouteStyle.SylvanOrganic);
                var second = RealmRoutePresentation.BuildDefenseLane(secondSylvan.transform, RealmRouteStyle.SylvanOrganic);
                var fractured = RealmRoutePresentation.BuildDefenseLane(infernal.transform, RealmRouteStyle.InfernalFractured);

                Assert.That(Signature(first), Is.EqualTo(Signature(second)));
                Assert.That(Signature(first), Is.Not.EqualTo(Signature(fractured)));
                var firstRenderers = first.GetComponentsInChildren<Renderer>(true);
                var secondRenderers = second.GetComponentsInChildren<Renderer>(true);
                for (var index = 0; index < firstRenderers.Length; index++) Assert.That(firstRenderers[index].sharedMaterial, Is.SameAs(secondRenderers[index].sharedMaterial));
                Assert.That(first.GetComponentInChildren<CapsuleCollider>(true), Is.Null);
                Assert.That(fractured.GetComponentInChildren<BoxCollider>(true), Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(firstSylvan);
                Object.DestroyImmediate(secondSylvan);
                Object.DestroyImmediate(infernal);
            }
        }

        static GameObject CreateAuthoritativeRoute()
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = "Authoritative Route";
            root.transform.SetPositionAndRotation(new Vector3(3, -.25f, 7), Quaternion.Euler(0, 27, 0));
            root.transform.localScale = new Vector3(14, .5f, 68);
            root.layer = 7;
            root.GetComponent<Collider>().isTrigger = true;
            return root;
        }

        static (Vector3 position, Quaternion rotation, Vector3 scale, int layer, string tag, bool colliderEnabled, bool trigger) Snapshot(GameObject root)
        {
            var collider = root.GetComponent<Collider>();
            return (root.transform.position, root.transform.rotation, root.transform.localScale, root.layer, root.tag, collider.enabled, collider.isTrigger);
        }

        static void AssertAuthoritativeState(GameObject root, (Vector3 position, Quaternion rotation, Vector3 scale, int layer, string tag, bool colliderEnabled, bool trigger) expected, bool rendererEnabled)
        {
            var collider = root.GetComponent<Collider>();
            Assert.That(root.transform.position, Is.EqualTo(expected.position));
            Assert.That(root.transform.rotation, Is.EqualTo(expected.rotation));
            Assert.That(root.transform.localScale, Is.EqualTo(expected.scale));
            Assert.That(root.layer, Is.EqualTo(expected.layer));
            Assert.That(root.tag, Is.EqualTo(expected.tag));
            Assert.That(collider.enabled, Is.EqualTo(expected.colliderEnabled));
            Assert.That(collider.isTrigger, Is.EqualTo(expected.trigger));
            Assert.That(root.GetComponent<Renderer>().enabled, Is.EqualTo(rendererEnabled));
        }

        static string Signature(Transform root)
        {
            return string.Join("|", root.Cast<Transform>().Select(child => $"{child.name}:{child.GetComponent<MeshFilter>().sharedMesh.name}:{child.localPosition}:{child.localRotation.eulerAngles}:{child.localScale}"));
        }

        static void AssertPresentationOnly(Transform root)
        {
            Assert.That(root.GetComponentsInChildren<Collider>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Rigidbody>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<CharacterController>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Camera>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Light>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<AudioListener>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<Canvas>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<EventSystem>(true), Is.Empty);
            Assert.That(root.GetComponentsInChildren<MonoBehaviour>(true), Is.Empty);
        }
    }
}
