using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using RealmRaiders.Core;
using UnityEditor;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class PrototypeArenaBoundarySurfacePreviewTests
    {
        const string Folder = "Assets/Game/Resources/Art/WorldSurfaces/MWS09-SylvanBoundary";
        const string AlbedoAsset = Folder + "/sylvan-living-root-boundary-edge-hardened-candidate.png";
        const string NormalAsset = Folder + "/sylvan-living-root-mobile-normal-rgb-candidate.png";
        const string ProvenanceAsset = Folder + "/provenance.json";
        const string AlbedoHash = "f0a79e13023c4191dd4e9405647de07db5fdc9a3cb21d340c08ba4a54c3aaba8";
        const string NormalHash = "27828c57fac349a26d73bac44497804296248131d8880a9289c7bad607312122";

        [Test]
        public void AcceptedLivingRootAssetsAreByteExactAndExplicitlyImportedForMobile()
        {
            Assert.That(Sha256(AlbedoAsset), Is.EqualTo(AlbedoHash));
            Assert.That(Sha256(NormalAsset), Is.EqualTo(NormalHash));
            CollectionAssert.AreEqual(new[] { AlbedoAsset, NormalAsset }, AssetDatabase.FindAssets("", new[] { Folder })
                .Select(AssetDatabase.GUIDToAssetPath).Where(path => path.EndsWith(".png")).OrderBy(path => path).ToArray());

            AssertImport(AlbedoAsset, TextureImporterType.Default, true);
            AssertImport(NormalAsset, TextureImporterType.NormalMap, false);

            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(ProvenanceAsset);
            Assert.That(provenance, Is.Not.Null);
            Assert.That(provenance.text, Does.Contain("realmraiders.preview.sylvan-living-root-boundary.mws09.v1"));
            Assert.That(provenance.text, Does.Contain("preview-only-not-final-art"));
            Assert.That(provenance.text, Does.Contain("original generation").And.Contain("no third-party source"));
            Assert.That(provenance.text, Does.Contain("\"modulesCommit\": \"c4295e4\"").And.Contain("\"modulesCommit\": \"f3475ac\""));
            Assert.That(provenance.text, Does.Contain("MWS08-RealmBoundarySeamHardening/sylvan-living-root-boundary-edge-hardened-candidate.png"));
            Assert.That(provenance.text, Does.Contain("MWS09-SylvanLivingRootMobileMaps/sylvan-living-root-mobile-normal-rgb-candidate.png"));
            Assert.That(provenance.text, Does.Contain(AlbedoHash).And.Contain(NormalHash));
        }

        [Test]
        public void SylvanSurfaceLoadsAtomicallyOnceAndLeavesOtherStylesTextureFree()
        {
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoAsset);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(NormalAsset);
            var requests = new List<string>();
            var owners = new List<GameObject>();
            try
            {
                PrototypeArenaBoundaryBuilder.ConfigureTextureLoaderForTests(path =>
                {
                    requests.Add(path);
                    return path == PrototypeArenaBoundaryBuilder.SylvanAlbedoResource ? albedo :
                        path == PrototypeArenaBoundaryBuilder.SylvanNormalResource ? normal : null;
                });

                var neutral = BuildRectangle(owners, PrototypeArenaBoundaryStyle.NeutralStone);
                var infernal = BuildRectangle(owners, PrototypeArenaBoundaryStyle.InfernalBasalt);
                Assert.That(requests, Is.Empty, "Non-Sylvan styles must not resolve living-root resources.");
                var firstSylvan = BuildRectangle(owners, PrototypeArenaBoundaryStyle.SylvanRoots);
                var secondSylvan = BuildRectangle(owners, PrototypeArenaBoundaryStyle.SylvanRoots);

                CollectionAssert.AreEqual(new[]
                {
                    PrototypeArenaBoundaryBuilder.SylvanAlbedoResource,
                    PrototypeArenaBoundaryBuilder.SylvanNormalResource
                }, requests);
                var firstMaterial = Renderer(firstSylvan).sharedMaterial;
                Assert.That(Renderer(secondSylvan).sharedMaterial, Is.SameAs(firstMaterial));
                Assert.That(firstMaterial.mainTexture, Is.SameAs(albedo));
                Assert.That(firstMaterial.GetTexture("_BumpMap"), Is.SameAs(normal));
                Assert.That(firstMaterial.GetFloat("_BumpScale"), Is.EqualTo(PrototypeArenaBoundaryBuilder.SylvanNormalStrength).Within(.001f));
                Assert.That(firstMaterial.IsKeywordEnabled("_NORMALMAP"), Is.True);
                AssertSolidFallback(Renderer(neutral).sharedMaterial, new Color(.17f, .22f, .21f));
                AssertSolidFallback(Renderer(infernal).sharedMaterial, new Color(.12f, .075f, .06f));
            }
            finally
            {
                DestroyOwners(owners);
                PrototypeArenaBoundaryBuilder.ResetTextureLoaderForTests();
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void MissingOrThrowingResourcePreservesWholeSolidColourFallback(int failureMode)
        {
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoAsset);
            var requests = new List<string>();
            var owners = new List<GameObject>();
            try
            {
                PrototypeArenaBoundaryBuilder.ConfigureTextureLoaderForTests(path =>
                {
                    requests.Add(path);
                    if (path == PrototypeArenaBoundaryBuilder.SylvanAlbedoResource)
                    {
                        if (failureMode == 2) throw new System.InvalidOperationException("Albedo preview unavailable");
                        return albedo;
                    }
                    if (failureMode == 1) throw new System.InvalidOperationException("Normal preview unavailable");
                    return null;
                });
                var first = BuildRectangle(owners, PrototypeArenaBoundaryStyle.SylvanRoots);
                var second = BuildRectangle(owners, PrototypeArenaBoundaryStyle.SylvanRoots);

                Assert.That(requests, Has.Count.EqualTo(failureMode == 2 ? 1 : 2),
                    "The failed atomic pair must stop safely and be cached rather than retried per owner.");
                Assert.That(Renderer(second).sharedMaterial, Is.SameAs(Renderer(first).sharedMaterial));
                AssertSolidFallback(Renderer(first).sharedMaterial, new Color(.25f, .22f, .105f));
            }
            finally
            {
                DestroyOwners(owners);
                PrototypeArenaBoundaryBuilder.ResetTextureLoaderForTests();
            }
        }

        [Test]
        public void SurfaceBindingAddsDeterministicUvTangentsWithoutChangingMeshOrColliderContract()
        {
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoAsset);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(NormalAsset);
            var owners = new List<GameObject>();
            try
            {
                PrototypeArenaBoundaryBuilder.ConfigureTextureLoaderForTests(_ => null);
                var fallback = BuildRectangle(owners, PrototypeArenaBoundaryStyle.SylvanRoots);
                var fallbackMesh = fallback.GetComponentInChildren<MeshFilter>().sharedMesh;
                var fallbackColliders = OrderedColliders(fallback);

                PrototypeArenaBoundaryBuilder.ConfigureTextureLoaderForTests(path =>
                    path == PrototypeArenaBoundaryBuilder.SylvanAlbedoResource ? albedo : normal);
                var textured = BuildRectangle(owners, PrototypeArenaBoundaryStyle.SylvanRoots);
                var texturedMesh = textured.GetComponentInChildren<MeshFilter>().sharedMesh;
                var texturedColliders = OrderedColliders(textured);

                CollectionAssert.AreEqual(fallbackMesh.vertices, texturedMesh.vertices);
                CollectionAssert.AreEqual(fallbackMesh.triangles, texturedMesh.triangles);
                CollectionAssert.AreEqual(fallbackMesh.uv, texturedMesh.uv);
                CollectionAssert.AreEqual(fallbackMesh.tangents, texturedMesh.tangents);
                Assert.That(texturedMesh.vertexCount, Is.EqualTo(112), "Surface data must not add or split the existing Sylvan rectangle vertices.");
                Assert.That(texturedMesh.triangles, Has.Length.EqualTo(564), "Surface data must not alter the existing 188 visual triangles.");
                Assert.That(texturedMesh.uv, Has.Length.EqualTo(texturedMesh.vertexCount));
                Assert.That(texturedMesh.tangents, Has.Length.EqualTo(texturedMesh.vertexCount));
                Assert.That(texturedMesh.uv.Distinct().Count(), Is.GreaterThan(8));
                foreach (var tangent in texturedMesh.tangents)
                {
                    Assert.That(IsFinite(tangent), Is.True);
                    Assert.That(new Vector3(tangent.x, tangent.y, tangent.z).sqrMagnitude, Is.GreaterThan(.5f));
                }
                Assert.That(textured.GetComponentsInChildren<MeshRenderer>(true), Has.Length.EqualTo(1));
                Assert.That(textured.GetComponentsInChildren<MeshFilter>(true), Has.Length.EqualTo(1));
                Assert.That(textured.GetComponentsInChildren<Transform>(true), Has.Length.EqualTo(6));
                Assert.That(textured.transform.childCount, Is.EqualTo(5), "The visual plus four authoritative collider children must remain the whole boundary.");
                Assert.That(textured.transform.Find(PrototypeArenaBoundaryBuilder.VisualName).GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(texturedColliders, Has.Length.EqualTo(fallbackColliders.Length));
                for (var index = 0; index < texturedColliders.Length; index++) AssertColliderEqual(fallbackColliders[index], texturedColliders[index]);
            }
            finally
            {
                DestroyOwners(owners);
                PrototypeArenaBoundaryBuilder.ResetTextureLoaderForTests();
            }
        }

        static void AssertImport(string path, TextureImporterType type, bool srgb)
        {
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            Assert.That(importer, Is.Not.Null, path);
            Assert.That(importer.textureType, Is.EqualTo(type));
            Assert.That(importer.sRGBTexture, Is.EqualTo(srgb));
            Assert.That(importer.mipmapEnabled, Is.True);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
            Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Repeat));
            Assert.That(importer.isReadable, Is.False);
            Assert.That(importer.convertToNormalmap, Is.False, "The supplied RGB normal must not be regenerated by Unity.");
            var android = importer.GetPlatformTextureSettings("Android");
            Assert.That(android.overridden, Is.True);
            Assert.That(android.maxTextureSize, Is.EqualTo(512));
            Assert.That(android.format, Is.EqualTo(TextureImporterFormat.ASTC_6x6));
            Assert.That(android.textureCompression, Is.EqualTo(TextureImporterCompression.Compressed));
            Assert.That(AssetDatabase.LoadAssetAtPath<Texture2D>(path), Is.Not.Null);
        }

        static string Sha256(string path)
        {
            using (var hash = SHA256.Create())
                return string.Concat(hash.ComputeHash(File.ReadAllBytes(path)).Select(value => value.ToString("x2")));
        }

        static GameObject BuildRectangle(List<GameObject> owners, PrototypeArenaBoundaryStyle style)
        {
            var owner = new GameObject($"{style} Surface Owner {owners.Count}");
            owners.Add(owner);
            return PrototypeArenaBoundaryBuilder.BuildRectangle(owner.transform, new Vector3(12, 0, -9),
                new Vector2(14, 68), 0, style, 1.2f, 1, 5, .4f);
        }

        static MeshRenderer Renderer(GameObject boundary) => boundary.GetComponentInChildren<MeshRenderer>();
        static BoxCollider[] OrderedColliders(GameObject boundary) => boundary.GetComponentsInChildren<BoxCollider>(true).OrderBy(item => item.name).ToArray();

        static void AssertSolidFallback(Material material, Color expected)
        {
            Assert.That(material.mainTexture, Is.Null);
            Assert.That(material.IsKeywordEnabled("_NORMALMAP"), Is.False);
            Assert.That(material.color.r, Is.EqualTo(expected.r).Within(.001f));
            Assert.That(material.color.g, Is.EqualTo(expected.g).Within(.001f));
            Assert.That(material.color.b, Is.EqualTo(expected.b).Within(.001f));
        }

        static void AssertColliderEqual(BoxCollider expected, BoxCollider actual)
        {
            Assert.That(actual.name, Is.EqualTo(expected.name));
            Assert.That(actual.transform.localPosition, Is.EqualTo(expected.transform.localPosition));
            Assert.That(actual.transform.localRotation, Is.EqualTo(expected.transform.localRotation));
            Assert.That(actual.center, Is.EqualTo(expected.center));
            Assert.That(actual.size, Is.EqualTo(expected.size));
            Assert.That(actual.isTrigger, Is.EqualTo(expected.isTrigger));
            Assert.That(actual.enabled, Is.EqualTo(expected.enabled));
            Assert.That(actual.gameObject.isStatic, Is.EqualTo(expected.gameObject.isStatic));
            Assert.That(actual.gameObject.layer, Is.EqualTo(expected.gameObject.layer));
        }

        static bool IsFinite(Vector4 value) =>
            !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
            !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
            !float.IsNaN(value.z) && !float.IsInfinity(value.z) &&
            !float.IsNaN(value.w) && !float.IsInfinity(value.w);

        static void DestroyOwners(List<GameObject> owners)
        {
            foreach (var owner in owners.Where(owner => owner)) Object.DestroyImmediate(owner);
            owners.Clear();
        }
    }
}
