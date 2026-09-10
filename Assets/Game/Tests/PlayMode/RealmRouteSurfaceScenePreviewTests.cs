using System.Collections;
using NUnit.Framework;
using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealmRaiders.Tests
{
    public sealed class RealmRouteSurfaceScenePreviewTests
    {
        [UnityTest]
        public IEnumerator SylvanAndInfernalPreviewBindingsLeaveAuthoritativeGeometryAndCollisionUntouched()
        {
            RealmRoutePresentation.ResetTextureLoaderForTests();
            var sylvanTexture = Resources.Load<Texture2D>(RealmRoutePresentation.SylvanAlbedoResource);
            var infernalTexture = Resources.Load<Texture2D>(RealmRoutePresentation.InfernalAlbedoResource);
            var authority = new GameObject("Authoritative Route Container");
            authority.transform.SetPositionAndRotation(new Vector3(6, 1, -8), Quaternion.Euler(0, 9, 0));
            authority.transform.localScale = new Vector3(1.1f, 1, .9f);
            var sylvan = CreateAuthoritativeRoute(authority.transform, "Sylvan Preview Route", new Vector3(3, -.25f, 7), Quaternion.Euler(0, 27, 0));
            var sylvanDefense = CreateAuthoritativeRoute(authority.transform, "Sylvan Preview Floor", new Vector3(-4, -.25f, -3), Quaternion.Euler(0, 5, 0));
            var infernal = CreateAuthoritativeRoute(authority.transform, "Infernal Preview Floor", new Vector3(-2, -.25f, 4), Quaternion.Euler(0, -13, 0));
            var sylvanSnapshot = Snapshot(sylvan);
            var sylvanDefenseSnapshot = Snapshot(sylvanDefense);
            var infernalSnapshot = Snapshot(infernal);
            try
            {
                Assert.That(sylvanTexture, Is.Not.Null);
                Assert.That(infernalTexture, Is.Not.Null);
                var sylvanPresentation = RealmRoutePresentation.BuildSegment(sylvan.transform, RealmRouteStyle.SylvanOrganic);
                var sylvanDefensePresentation = RealmRoutePresentation.BuildDefenseLane(sylvanDefense.transform, RealmRouteStyle.SylvanOrganic);
                var infernalPresentation = RealmRoutePresentation.BuildDefenseLane(infernal.transform, RealmRouteStyle.InfernalFractured);
                yield return null;

                AssertBinding(sylvan, sylvanPresentation, sylvanTexture, sylvanSnapshot, false);
                AssertBinding(sylvanDefense, sylvanDefensePresentation, sylvanTexture, sylvanDefenseSnapshot, true);
                AssertBinding(infernal, infernalPresentation, infernalTexture, infernalSnapshot, true);
            }
            finally
            {
                Object.Destroy(authority);
                RealmRoutePresentation.ResetTextureLoaderForTests();
            }
        }

        static GameObject CreateAuthoritativeRoute(Transform parent, string name, Vector3 position, Quaternion rotation)
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = name;
            root.transform.SetParent(parent, false);
            root.transform.SetLocalPositionAndRotation(position, rotation);
            root.transform.localScale = new Vector3(14, .5f, 68);
            root.layer = 7;
            root.GetComponent<Collider>().isTrigger = true;
            return root;
        }

        static (Transform parent, Vector3 position, Quaternion rotation, Vector3 localPosition, Quaternion localRotation,
            Vector3 localScale, Mesh mesh, Collider collider, Vector3 colliderCenter, Vector3 colliderSize, int layer) Snapshot(GameObject root) =>
            (root.transform.parent, root.transform.position, root.transform.rotation, root.transform.localPosition,
                root.transform.localRotation, root.transform.localScale, root.GetComponent<MeshFilter>().sharedMesh,
                root.GetComponent<Collider>(), root.GetComponent<BoxCollider>().center, root.GetComponent<BoxCollider>().size,
                root.layer);

        static void AssertBinding(GameObject root, Transform presentation, Texture2D expectedTexture,
            (Transform parent, Vector3 position, Quaternion rotation, Vector3 localPosition, Quaternion localRotation,
                Vector3 localScale, Mesh mesh, Collider collider, Vector3 colliderCenter, Vector3 colliderSize,
                int layer) snapshot, bool rootRendererEnabled)
        {
            Assert.That(root.transform.parent, Is.SameAs(snapshot.parent));
            Assert.That(root.transform.position, Is.EqualTo(snapshot.position));
            Assert.That(root.transform.rotation, Is.EqualTo(snapshot.rotation));
            Assert.That(root.transform.localPosition, Is.EqualTo(snapshot.localPosition));
            Assert.That(root.transform.localRotation, Is.EqualTo(snapshot.localRotation));
            Assert.That(root.transform.localScale, Is.EqualTo(snapshot.localScale));
            Assert.That(root.GetComponent<MeshFilter>().sharedMesh, Is.SameAs(snapshot.mesh));
            Assert.That(root.GetComponent<Collider>(), Is.SameAs(snapshot.collider));
            Assert.That(root.layer, Is.EqualTo(snapshot.layer));
            Assert.That(snapshot.collider.enabled, Is.True);
            Assert.That(snapshot.collider.isTrigger, Is.True);
            Assert.That(((BoxCollider)snapshot.collider).center, Is.EqualTo(snapshot.colliderCenter));
            Assert.That(((BoxCollider)snapshot.collider).size, Is.EqualTo(snapshot.colliderSize));
            Assert.That(root.GetComponent<Renderer>().enabled, Is.EqualTo(rootRendererEnabled));
            Assert.That(root.GetComponent<Renderer>().sharedMaterial.mainTexture, Is.SameAs(expectedTexture));
            Assert.That(presentation.GetComponentsInChildren<Collider>(true), Is.Empty);
            foreach (var renderer in presentation.GetComponentsInChildren<Renderer>(true))
                Assert.That(renderer.sharedMaterial.mainTexture, Is.SameAs(expectedTexture));
        }
    }
}
