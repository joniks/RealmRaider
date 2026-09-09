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
            var sylvan = CreateAuthoritativeRoute("Sylvan Preview Route", new Vector3(3, -.25f, 7), Quaternion.Euler(0, 27, 0));
            var infernal = CreateAuthoritativeRoute("Infernal Preview Floor", new Vector3(-2, -.25f, 4), Quaternion.Euler(0, -13, 0));
            var sylvanSnapshot = Snapshot(sylvan);
            var infernalSnapshot = Snapshot(infernal);
            try
            {
                Assert.That(sylvanTexture, Is.Not.Null);
                Assert.That(infernalTexture, Is.Not.Null);
                var sylvanPresentation = RealmRoutePresentation.BuildSegment(sylvan.transform, RealmRouteStyle.SylvanOrganic);
                var infernalPresentation = RealmRoutePresentation.BuildDefenseLane(infernal.transform, RealmRouteStyle.InfernalFractured);
                yield return null;

                AssertBinding(sylvan, sylvanPresentation, sylvanTexture, sylvanSnapshot, false);
                AssertBinding(infernal, infernalPresentation, infernalTexture, infernalSnapshot, true);
            }
            finally
            {
                Object.Destroy(sylvan);
                Object.Destroy(infernal);
                RealmRoutePresentation.ResetTextureLoaderForTests();
            }
        }

        static GameObject CreateAuthoritativeRoute(string name, Vector3 position, Quaternion rotation)
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            root.name = name;
            root.transform.SetPositionAndRotation(position, rotation);
            root.transform.localScale = new Vector3(14, .5f, 68);
            root.layer = 7;
            root.GetComponent<Collider>().isTrigger = true;
            return root;
        }

        static (Vector3 position, Quaternion rotation, Vector3 scale, Mesh mesh, Collider collider) Snapshot(GameObject root) =>
            (root.transform.position, root.transform.rotation, root.transform.localScale, root.GetComponent<MeshFilter>().sharedMesh, root.GetComponent<Collider>());

        static void AssertBinding(GameObject root, Transform presentation, Texture2D expectedTexture,
            (Vector3 position, Quaternion rotation, Vector3 scale, Mesh mesh, Collider collider) snapshot, bool rootRendererEnabled)
        {
            Assert.That(root.transform.position, Is.EqualTo(snapshot.position));
            Assert.That(root.transform.rotation, Is.EqualTo(snapshot.rotation));
            Assert.That(root.transform.localScale, Is.EqualTo(snapshot.scale));
            Assert.That(root.GetComponent<MeshFilter>().sharedMesh, Is.SameAs(snapshot.mesh));
            Assert.That(root.GetComponent<Collider>(), Is.SameAs(snapshot.collider));
            Assert.That(snapshot.collider.enabled, Is.True);
            Assert.That(snapshot.collider.isTrigger, Is.True);
            Assert.That(root.GetComponent<Renderer>().enabled, Is.EqualTo(rootRendererEnabled));
            Assert.That(root.GetComponent<Renderer>().sharedMaterial.mainTexture, Is.SameAs(expectedTexture));
            Assert.That(presentation.GetComponentsInChildren<Collider>(true), Is.Empty);
            foreach (var renderer in presentation.GetComponentsInChildren<Renderer>(true))
                Assert.That(renderer.sharedMaterial.mainTexture, Is.SameAs(expectedTexture));
        }
    }
}
