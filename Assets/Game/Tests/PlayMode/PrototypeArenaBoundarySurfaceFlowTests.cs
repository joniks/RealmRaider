using System.Collections;
using System.Linq;
using NUnit.Framework;
using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace RealmRaiders.Tests
{
    public sealed class PrototypeArenaBoundarySurfaceFlowTests
    {
        [UnityTest]
        public IEnumerator SylvanRaidAndDefenseUseLivingRootSurfaceWhileNeutralAndInfernalStayTextureFree()
        {
            PrototypeArenaBoundaryBuilder.ResetTextureLoaderForTests();
            var albedo = Resources.Load<Texture2D>(PrototypeArenaBoundaryBuilder.SylvanAlbedoResource);
            var normal = Resources.Load<Texture2D>(PrototypeArenaBoundaryBuilder.SylvanNormalResource);
            Material sharedSylvanMaterial = null;
            try
            {
                Assert.That(albedo, Is.Not.Null);
                Assert.That(normal, Is.Not.Null);
                foreach (var scene in new[] { "SylvanRealm", "DefenderTest", "CharacterSandbox", "InfernalRealm" })
                {
                    SceneManager.LoadScene(scene);
                    yield return null;
                    yield return null;

                    var boundary = GameObject.Find(PrototypeArenaBoundaryBuilder.BoundaryName);
                    Assert.That(boundary, Is.Not.Null, scene);
                    var visual = boundary.transform.Find(PrototypeArenaBoundaryBuilder.VisualName);
                    Assert.That(visual, Is.Not.Null, scene);
                    Assert.That(visual.GetComponentsInChildren<Collider>(true), Is.Empty, $"{scene} visual presentation must remain collider-free.");
                    Assert.That(boundary.GetComponentsInChildren<MeshRenderer>(true), Has.Length.EqualTo(1));
                    Assert.That(boundary.GetComponentsInChildren<MeshFilter>(true), Has.Length.EqualTo(1));

                    var renderer = visual.GetComponent<MeshRenderer>();
                    var mesh = visual.GetComponent<MeshFilter>().sharedMesh;
                    var vertices = mesh.vertices;
                    var triangles = mesh.triangles;
                    var colliders = boundary.GetComponentsInChildren<BoxCollider>(true).OrderBy(item => item.name).ToArray();
                    var colliderSnapshots = colliders.Select(item => new ColliderSnapshot(item)).ToArray();
                    Assert.That(mesh.uv, Has.Length.EqualTo(mesh.vertexCount), $"{scene} UV completeness");
                    Assert.That(mesh.tangents, Has.Length.EqualTo(mesh.vertexCount), $"{scene} tangent completeness");
                    Assert.That(boundary.transform.childCount, Is.EqualTo(colliders.Length + 1), $"{scene} must retain one visual child plus only its collider children.");
                    foreach (var collider in colliders)
                    {
                        Assert.That(collider.isTrigger, Is.False, scene);
                        Assert.That(collider.enabled, Is.True, scene);
                        Assert.That(collider.gameObject.isStatic, Is.True, scene);
                    }
                    if (scene == "SylvanRealm") Assert.That(colliders.Length, Is.InRange(1, PrototypeArenaBoundaryBuilder.SylvanColliderBudget));
                    else Assert.That(colliders, Has.Length.EqualTo(4));

                    var sylvan = scene == "SylvanRealm" || scene == "DefenderTest";
                    if (sylvan)
                    {
                        Assert.That(renderer.sharedMaterial.mainTexture, Is.SameAs(albedo));
                        Assert.That(renderer.sharedMaterial.GetTexture("_BumpMap"), Is.SameAs(normal));
                        Assert.That(renderer.sharedMaterial.GetFloat("_BumpScale"), Is.EqualTo(PrototypeArenaBoundaryBuilder.SylvanNormalStrength).Within(.001f));
                        Assert.That(renderer.sharedMaterial.IsKeywordEnabled("_NORMALMAP"), Is.True);
                        if (sharedSylvanMaterial) Assert.That(renderer.sharedMaterial, Is.SameAs(sharedSylvanMaterial));
                        else sharedSylvanMaterial = renderer.sharedMaterial;
                    }
                    else
                    {
                        Assert.That(renderer.sharedMaterial.mainTexture, Is.Null, $"{scene} must retain its texture-free fallback.");
                        Assert.That(renderer.sharedMaterial.IsKeywordEnabled("_NORMALMAP"), Is.False);
                        var expected = scene == "CharacterSandbox" ? new Color(.17f, .22f, .21f) : new Color(.12f, .075f, .06f);
                        AssertColor(renderer.sharedMaterial.color, expected, scene);
                    }

                    yield return null;
                    CollectionAssert.AreEqual(vertices, mesh.vertices, $"{scene} visual vertex positions changed after presentation settled.");
                    CollectionAssert.AreEqual(triangles, mesh.triangles, $"{scene} visual triangles changed after presentation settled.");
                    Assert.That(colliders, Has.Length.EqualTo(colliderSnapshots.Length));
                    for (var index = 0; index < colliders.Length; index++) colliderSnapshots[index].AssertUnchanged(colliders[index], scene);
                }
            }
            finally
            {
                SceneManager.LoadScene("RealmBuild");
                PrototypeArenaBoundaryBuilder.ResetTextureLoaderForTests();
            }
            yield return null;
        }

        static void AssertColor(Color actual, Color expected, string scene)
        {
            Assert.That(actual.r, Is.EqualTo(expected.r).Within(.001f), scene);
            Assert.That(actual.g, Is.EqualTo(expected.g).Within(.001f), scene);
            Assert.That(actual.b, Is.EqualTo(expected.b).Within(.001f), scene);
        }

        readonly struct ColliderSnapshot
        {
            readonly Vector3 localPosition;
            readonly Quaternion localRotation;
            readonly Vector3 center;
            readonly Vector3 size;
            readonly bool enabled;
            readonly bool trigger;
            readonly bool isStatic;
            readonly int layer;

            public ColliderSnapshot(BoxCollider collider)
            {
                localPosition = collider.transform.localPosition;
                localRotation = collider.transform.localRotation;
                center = collider.center;
                size = collider.size;
                enabled = collider.enabled;
                trigger = collider.isTrigger;
                isStatic = collider.gameObject.isStatic;
                layer = collider.gameObject.layer;
            }

            public void AssertUnchanged(BoxCollider collider, string scene)
            {
                Assert.That(collider.transform.localPosition, Is.EqualTo(localPosition), scene);
                Assert.That(collider.transform.localRotation, Is.EqualTo(localRotation), scene);
                Assert.That(collider.center, Is.EqualTo(center), scene);
                Assert.That(collider.size, Is.EqualTo(size), scene);
                Assert.That(collider.enabled, Is.EqualTo(enabled), scene);
                Assert.That(collider.isTrigger, Is.EqualTo(trigger), scene);
                Assert.That(collider.gameObject.isStatic, Is.EqualTo(isStatic), scene);
                Assert.That(collider.gameObject.layer, Is.EqualTo(layer), scene);
            }
        }
    }
}
