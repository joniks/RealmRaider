using System.Collections;
using NUnit.Framework;
using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
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
            var sylvanRouteNormal = Resources.Load<Texture2D>(RealmRoutePresentation.SylvanRouteNormalResource);
            var infernalRouteTexture = Resources.Load<Texture2D>(RealmRoutePresentation.InfernalAlbedoResource);
            var infernalRouteNormal = Resources.Load<Texture2D>(RealmRoutePresentation.InfernalRouteNormalResource);
            var infernalCourtyardAlbedo = Resources.Load<Texture2D>(RealmRoutePresentation.InfernalCourtyardAlbedoResource);
            var infernalCourtyardNormal = Resources.Load<Texture2D>(RealmRoutePresentation.InfernalCourtyardNormalResource);
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
                Assert.That(sylvanRouteNormal, Is.Not.Null);
                Assert.That(infernalRouteTexture, Is.Not.Null);
                Assert.That(infernalRouteNormal, Is.Not.Null);
                Assert.That(infernalCourtyardAlbedo, Is.Not.Null);
                Assert.That(infernalCourtyardNormal, Is.Not.Null);
                var sylvanPresentation = RealmRoutePresentation.BuildSegment(sylvan.transform, RealmRouteStyle.SylvanOrganic);
                var sylvanDefensePresentation = RealmRoutePresentation.BuildDefenseLane(sylvanDefense.transform, RealmRouteStyle.SylvanOrganic);
                var infernalPresentation = RealmRoutePresentation.BuildDefenseLane(infernal.transform, RealmRouteStyle.InfernalFractured);
                yield return null;

                AssertBinding(sylvan, sylvanPresentation, sylvanTexture, sylvanTexture, sylvanRouteNormal, sylvanSnapshot, false);
                AssertBinding(sylvanDefense, sylvanDefensePresentation, sylvanTexture, sylvanTexture, sylvanRouteNormal, sylvanDefenseSnapshot, true);
                AssertBinding(infernal, infernalPresentation, infernalCourtyardAlbedo, infernalRouteTexture, infernalRouteNormal, infernalSnapshot, true);
                Assert.That(sylvan.GetComponent<Renderer>().sharedMaterial.GetTexture("_BumpMap"), Is.Null);
                Assert.That(sylvanDefense.GetComponent<Renderer>().sharedMaterial.GetTexture("_BumpMap"), Is.Null);
                var infernalFloorMaterial = infernal.GetComponent<Renderer>().sharedMaterial;
                Assert.That(infernalFloorMaterial.GetTexture("_BumpMap"), Is.SameAs(infernalCourtyardNormal));
                Assert.That(infernalFloorMaterial.GetFloat("_BumpScale"), Is.EqualTo(RealmRoutePresentation.InfernalCourtyardNormalStrength).Within(.001f));
                AssertRendererCourtyardTiling(infernal.GetComponent<Renderer>(), new Vector2(
                    Mathf.Abs(infernal.transform.lossyScale.x) / RealmRoutePresentation.InfernalCourtyardWorldUnitsPerTile,
                    Mathf.Abs(infernal.transform.lossyScale.z) / RealmRoutePresentation.InfernalCourtyardWorldUnitsPerTile));
            }
            finally
            {
                Object.Destroy(authority);
                RealmRoutePresentation.ResetTextureLoaderForTests();
            }
        }

        [UnityTest]
        public IEnumerator InfernalRealmVolcanicFloorUsesCourtyardPairWhileCausewayStaysMws07()
        {
            RealmRoutePresentation.ResetTextureLoaderForTests();
            var courtyardAlbedo = Resources.Load<Texture2D>(RealmRoutePresentation.InfernalCourtyardAlbedoResource);
            var courtyardNormal = Resources.Load<Texture2D>(RealmRoutePresentation.InfernalCourtyardNormalResource);
            var routeAlbedo = Resources.Load<Texture2D>(RealmRoutePresentation.InfernalAlbedoResource);
            var routeNormal = Resources.Load<Texture2D>(RealmRoutePresentation.InfernalRouteNormalResource);
            try
            {
                Assert.That(courtyardAlbedo, Is.Not.Null);
                Assert.That(courtyardNormal, Is.Not.Null);
                Assert.That(routeAlbedo, Is.Not.Null);
                Assert.That(routeNormal, Is.Not.Null);
                SceneManager.LoadScene("InfernalRealm");
                yield return null;
                yield return null;

                var floor = GameObject.Find("Volcanic Floor");
                Assert.That(floor, Is.Not.Null);
                var presentation = floor.transform.Find(RealmRoutePresentation.RootName);
                Assert.That(presentation, Is.Not.Null);
                var snapshot = Snapshot(floor);
                AssertBinding(floor, presentation, courtyardAlbedo, routeAlbedo, routeNormal, snapshot, true);
                var floorMaterial = floor.GetComponent<Renderer>().sharedMaterial;
                Assert.That(floorMaterial.GetTexture("_BumpMap"), Is.SameAs(courtyardNormal));
                Assert.That(floorMaterial.GetFloat("_BumpScale"), Is.EqualTo(RealmRoutePresentation.InfernalCourtyardNormalStrength).Within(.001f));
                AssertRendererCourtyardTiling(floor.GetComponent<Renderer>(), new Vector2(14f / RealmRoutePresentation.InfernalCourtyardWorldUnitsPerTile, 68f / RealmRoutePresentation.InfernalCourtyardWorldUnitsPerTile));
                Assert.That(presentation.GetComponentsInChildren<Renderer>(true), Has.Length.EqualTo(RealmRoutePresentation.DefenseRendererCeiling));
                foreach (var renderer in presentation.GetComponentsInChildren<Renderer>(true))
                {
                    Assert.That(renderer.sharedMaterial.mainTexture, Is.SameAs(routeAlbedo));
                    Assert.That(renderer.sharedMaterial.GetTexture("_BumpMap"), Is.SameAs(routeNormal));
                    Assert.That(renderer.sharedMaterial.GetFloat("_BumpScale"), Is.EqualTo(RealmRoutePresentation.RouteNormalStrength).Within(.001f));
                }

                yield return null;
                AssertBinding(floor, presentation, courtyardAlbedo, routeAlbedo, routeNormal, snapshot, true);
            }
            finally
            {
                SceneManager.LoadScene("RealmBuild");
                RealmRoutePresentation.ResetTextureLoaderForTests();
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator SylvanRouteNormalsStayOutOfNodeFloorsAndArenaBoundary()
        {
            RealmRoutePresentation.ResetTextureLoaderForTests();
            var routeNormal = Resources.Load<Texture2D>(RealmRoutePresentation.SylvanRouteNormalResource);
            var nodeNormal = Resources.Load<Texture2D>("Art/WorldSurfaces/MWS10-SylvanClearingFloor/sylvan-clearing-floor-mobile-normal-rgb-candidate");
            var boundaryNormal = Resources.Load<Texture2D>(PrototypeArenaBoundaryBuilder.SylvanNormalResource);
            try
            {
                Assert.That(routeNormal, Is.Not.Null);
                Assert.That(nodeNormal, Is.Not.Null);
                Assert.That(boundaryNormal, Is.Not.Null);
                SceneManager.LoadScene("SylvanRealm");
                yield return null;
                yield return null;

                var path = GameObject.Find("Living Path");
                var nodeFloor = GameObject.Find("PORTAL Ground");
                var boundary = GameObject.Find(PrototypeArenaBoundaryBuilder.BoundaryName);
                Assert.That(path, Is.Not.Null);
                Assert.That(nodeFloor, Is.Not.Null);
                Assert.That(boundary, Is.Not.Null);
                var pathPresentation = path.transform.Find(RealmRoutePresentation.RootName);
                var boundaryVisual = boundary.transform.Find(PrototypeArenaBoundaryBuilder.VisualName);
                Assert.That(pathPresentation, Is.Not.Null);
                Assert.That(boundaryVisual, Is.Not.Null);
                var pathRenderer = pathPresentation.GetComponentInChildren<Renderer>();
                var nodeRenderer = nodeFloor.GetComponent<Renderer>();
                var boundaryRenderer = boundaryVisual.GetComponent<Renderer>();
                Assert.That(pathRenderer.sharedMaterial.GetTexture("_BumpMap"), Is.SameAs(routeNormal));
                Assert.That(nodeRenderer.sharedMaterial.GetTexture("_BumpMap"), Is.SameAs(nodeNormal));
                Assert.That(boundaryRenderer.sharedMaterial.GetTexture("_BumpMap"), Is.SameAs(boundaryNormal));
                Assert.That(nodeRenderer.sharedMaterial.GetTexture("_BumpMap"), Is.Not.SameAs(routeNormal));
                Assert.That(boundaryRenderer.sharedMaterial.GetTexture("_BumpMap"), Is.Not.SameAs(routeNormal));
            }
            finally
            {
                SceneManager.LoadScene("RealmBuild");
                RealmRoutePresentation.ResetTextureLoaderForTests();
            }
            yield return null;
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
            Vector3 localScale, Mesh mesh, Collider collider, bool colliderIsTrigger, Vector3 colliderCenter, Vector3 colliderSize, int layer) Snapshot(GameObject root) =>
            (root.transform.parent, root.transform.position, root.transform.rotation, root.transform.localPosition,
                root.transform.localRotation, root.transform.localScale, root.GetComponent<MeshFilter>().sharedMesh,
                root.GetComponent<Collider>(), root.GetComponent<Collider>().isTrigger, root.GetComponent<BoxCollider>().center, root.GetComponent<BoxCollider>().size,
                root.layer);

        static void AssertBinding(GameObject root, Transform presentation, Texture2D expectedRootTexture, Texture2D expectedPresentationTexture,
            Texture2D expectedPresentationNormal,
            (Transform parent, Vector3 position, Quaternion rotation, Vector3 localPosition, Quaternion localRotation,
                Vector3 localScale, Mesh mesh, Collider collider, bool colliderIsTrigger, Vector3 colliderCenter, Vector3 colliderSize,
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
            Assert.That(snapshot.collider.isTrigger, Is.EqualTo(snapshot.colliderIsTrigger));
            Assert.That(((BoxCollider)snapshot.collider).center, Is.EqualTo(snapshot.colliderCenter));
            Assert.That(((BoxCollider)snapshot.collider).size, Is.EqualTo(snapshot.colliderSize));
            Assert.That(root.GetComponent<Renderer>().enabled, Is.EqualTo(rootRendererEnabled));
            Assert.That(root.GetComponent<Renderer>().sharedMaterial.mainTexture, Is.SameAs(expectedRootTexture));
            Assert.That(presentation.GetComponentsInChildren<Collider>(true), Is.Empty);
            foreach (var renderer in presentation.GetComponentsInChildren<Renderer>(true))
            {
                Assert.That(renderer.sharedMaterial.mainTexture, Is.SameAs(expectedPresentationTexture));
                Assert.That(renderer.sharedMaterial.GetTexture("_BumpMap"), Is.SameAs(expectedPresentationNormal));
                Assert.That(renderer.sharedMaterial.GetFloat("_BumpScale"), Is.EqualTo(RealmRoutePresentation.RouteNormalStrength).Within(.001f));
            }
        }

        static void AssertRendererCourtyardTiling(Renderer renderer, Vector2 expected)
        {
            var properties = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties);
            var expectedScaleOffset = new Vector4(expected.x, expected.y, 0, 0);
            var albedoProperty = renderer.sharedMaterial.HasProperty("_BaseMap") ? "_BaseMap_ST" : "_MainTex_ST";
            Assert.That(properties.GetVector(albedoProperty), Is.EqualTo(expectedScaleOffset));
            Assert.That(properties.GetVector("_BumpMap_ST"), Is.EqualTo(expectedScaleOffset));
        }
    }
}
