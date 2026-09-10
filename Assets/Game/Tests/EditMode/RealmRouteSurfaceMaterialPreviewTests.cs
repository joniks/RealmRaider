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
    public sealed class RealmRouteSurfaceMaterialPreviewTests
    {
        const string SylvanAsset = "Assets/Game/Resources/Art/WorldSurfaces/MWS07-SylvanPath/sylvan-stone-path-edge-hardened-candidate.png";
        const string InfernalAsset = "Assets/Game/Resources/Art/WorldSurfaces/MWS07-InfernalPath/infernal-basalt-path-edge-hardened-candidate.png";
        const string LegacyInfernalAsset = "Assets/Game/Resources/Art/WorldSurfaces/MWS03-InfernalGroundMaterialCandidate/infernal-ground-albedo-rgb-candidate.png";
        const string SylvanResource = "Art/WorldSurfaces/MWS07-SylvanPath/sylvan-stone-path-edge-hardened-candidate";
        const string InfernalResource = "Art/WorldSurfaces/MWS07-InfernalPath/infernal-basalt-path-edge-hardened-candidate";
        const string InfernalHash = "c8df59807a4fe21c9a5cc27ce3f776f43f1be1a689aab0366c27fd245606da88";

        [Test]
        public void ImportedPreviewProviderBindsAndCachesExactAlbedos()
        {
            Assert.That(RealmRoutePresentation.SylvanAlbedoResource, Is.EqualTo(SylvanResource));
            Assert.That(RealmRoutePresentation.InfernalAlbedoResource, Is.EqualTo(InfernalResource));
            var sylvanTexture = AssertSylvanPreviewImport();
            var infernalTexture = AssertInfernalPreviewImport();
            Assert.That(AssetDatabase.LoadAssetAtPath<Texture2D>(LegacyInfernalAsset), Is.Not.Null, "MWS03 remains available as the unbound legacy preview.");
            var roots = new List<GameObject>();
            try
            {
                var requests = new List<string>();
                RealmRoutePresentation.ConfigureTextureLoaderForTests(path =>
                {
                    requests.Add(path);
                    return path == RealmRoutePresentation.SylvanAlbedoResource ? sylvanTexture :
                        path == RealmRoutePresentation.InfernalAlbedoResource ? infernalTexture : null;
                });

                var firstSylvan = CreateRoute(roots);
                var secondSylvan = CreateRoute(roots);
                var infernal = CreateRoute(roots);
                var firstPresentation = RealmRoutePresentation.BuildDefenseLane(firstSylvan.transform, RealmRouteStyle.SylvanOrganic);
                var secondPresentation = RealmRoutePresentation.BuildDefenseLane(secondSylvan.transform, RealmRouteStyle.SylvanOrganic);
                var infernalPresentation = RealmRoutePresentation.BuildDefenseLane(infernal.transform, RealmRouteStyle.InfernalFractured);

                Assert.That(firstSylvan.GetComponent<Renderer>().sharedMaterial.mainTexture, Is.SameAs(sylvanTexture));
                Assert.That(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial.mainTexture, Is.SameAs(sylvanTexture));
                Assert.That(infernal.GetComponent<Renderer>().sharedMaterial.mainTexture, Is.SameAs(infernalTexture));
                Assert.That(infernalPresentation.GetComponentInChildren<Renderer>().sharedMaterial.mainTexture, Is.SameAs(infernalTexture));
                Assert.That(secondSylvan.GetComponent<Renderer>().sharedMaterial, Is.SameAs(firstSylvan.GetComponent<Renderer>().sharedMaterial));
                Assert.That(secondPresentation.GetComponentInChildren<Renderer>().sharedMaterial, Is.SameAs(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial));
                Assert.That(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial,
                    Is.Not.SameAs(firstSylvan.GetComponent<Renderer>().sharedMaterial));
                CollectionAssert.AreEqual(new[] { SylvanResource, RealmRoutePresentation.InfernalAlbedoResource }, requests);
            }
            finally
            {
                DestroyRoots(roots);
                RealmRoutePresentation.ResetTextureLoaderForTests();
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void UnavailableSylvanPreviewPreservesExactCachedColorFallback(bool loaderThrows)
        {
            var roots = new List<GameObject>();
            var loadCount = 0;
            try
            {
                RealmRoutePresentation.ConfigureTextureLoaderForTests(path =>
                {
                    Assert.That(path, Is.EqualTo(SylvanResource));
                    loadCount++;
                    if (loaderThrows) throw new System.InvalidOperationException("Failed preview resource");
                    return null;
                });

                var first = CreateRoute(roots);
                var second = CreateRoute(roots);
                var firstPresentation = RealmRoutePresentation.BuildDefenseLane(first.transform, RealmRouteStyle.SylvanOrganic);
                var secondPresentation = RealmRoutePresentation.BuildDefenseLane(second.transform, RealmRouteStyle.SylvanOrganic);

                Assert.That(loadCount, Is.EqualTo(1));
                AssertFallback(first.GetComponent<Renderer>().sharedMaterial, new Color(.08f, .24f, .1f));
                AssertFallback(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial, new Color(.25f, .36f, .22f));
                Assert.That(second.GetComponent<Renderer>().sharedMaterial, Is.SameAs(first.GetComponent<Renderer>().sharedMaterial));
                Assert.That(secondPresentation.GetComponentInChildren<Renderer>().sharedMaterial,
                    Is.SameAs(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial));
                Assert.That(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial,
                    Is.Not.SameAs(first.GetComponent<Renderer>().sharedMaterial));
            }
            finally
            {
                DestroyRoots(roots);
                RealmRoutePresentation.ResetTextureLoaderForTests();
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void UnavailableInfernalPreviewPreservesExactCachedColorFallback(bool loaderThrows)
        {
            var roots = new List<GameObject>();
            var loadCount = 0;
            try
            {
                RealmRoutePresentation.ConfigureTextureLoaderForTests(path =>
                {
                    Assert.That(path, Is.EqualTo(InfernalResource));
                    loadCount++;
                    if (loaderThrows) throw new System.InvalidOperationException("Failed preview resource");
                    return null;
                });

                var first = CreateRoute(roots);
                var second = CreateRoute(roots);
                var firstPresentation = RealmRoutePresentation.BuildDefenseLane(first.transform, RealmRouteStyle.InfernalFractured);
                var secondPresentation = RealmRoutePresentation.BuildDefenseLane(second.transform, RealmRouteStyle.InfernalFractured);

                Assert.That(loadCount, Is.EqualTo(1));
                AssertFallback(first.GetComponent<Renderer>().sharedMaterial, new Color(.12f, .045f, .035f));
                AssertFallback(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial, new Color(.27f, .23f, .2f));
                Assert.That(second.GetComponent<Renderer>().sharedMaterial, Is.SameAs(first.GetComponent<Renderer>().sharedMaterial));
                Assert.That(secondPresentation.GetComponentInChildren<Renderer>().sharedMaterial,
                    Is.SameAs(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial));
                Assert.That(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial,
                    Is.Not.SameAs(first.GetComponent<Renderer>().sharedMaterial));
            }
            finally
            {
                DestroyRoots(roots);
                RealmRoutePresentation.ResetTextureLoaderForTests();
            }
        }

        static Texture2D AssertSylvanPreviewImport()
        {
            AssertImportSettings(SylvanAsset, TextureWrapMode.Repeat);
            var provenancePath = "Assets/Game/Resources/Art/WorldSurfaces/MWS07-SylvanPath/provenance.json";
            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(provenancePath);
            Assert.That(provenance, Is.Not.Null, provenancePath);
            Assert.That(provenance.text, Does.Contain("realmraiders.preview.sylvan-walkable-surface.mws07.v1"));
            Assert.That(provenance.text, Does.Contain("original generation followed by MWS07 seam-hardening"));
            Assert.That(provenance.text, Does.Contain("preview-only-not-production-approved"));
            Assert.That(provenance.text, Does.Contain("\"modulesCommit\": \"6138f71\""));
            Assert.That(provenance.text, Does.Contain(
                "Modules/RealmRaider.Modules/ArtPreviews/MWS07-WalkableSurfaceSeamHardening/sylvan-stone-path-edge-hardened-candidate.png"));
            Assert.That(provenance.text, Does.Contain("c5043bfb793fa4c85ff7e6b7284ef3a1d845bd56d8c4480628cfd91d6ccc101c"));

            var folder = SylvanAsset.Substring(0, SylvanAsset.LastIndexOf('/'));
            var images = AssetDatabase.FindAssets("", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".png"))
                .ToArray();
            CollectionAssert.AreEqual(new[] { SylvanAsset }, images, "Only the accepted RGB albedo belongs in this resource folder.");
            return AssetDatabase.LoadAssetAtPath<Texture2D>(SylvanAsset);
        }

        static Texture2D AssertInfernalPreviewImport()
        {
            AssertImportSettings(InfernalAsset, TextureWrapMode.Repeat);
            Assert.That(Sha256(InfernalAsset), Is.EqualTo(InfernalHash));
            const string provenancePath = "Assets/Game/Resources/Art/WorldSurfaces/MWS07-InfernalPath/provenance.json";
            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(provenancePath);
            Assert.That(provenance, Is.Not.Null, provenancePath);
            Assert.That(provenance.text, Does.Contain("realmraiders.preview.infernal-walkable-surface.mws07.v1"));
            Assert.That(provenance.text, Does.Contain("original generation followed by MWS07 seam-hardening"));
            Assert.That(provenance.text, Does.Contain("preview-only-not-production-approved"));
            Assert.That(provenance.text, Does.Contain("\"modulesCommit\": \"6138f71\""));
            Assert.That(provenance.text, Does.Contain(
                "Modules/RealmRaider.Modules/ArtPreviews/MWS07-WalkableSurfaceSeamHardening/infernal-basalt-path-edge-hardened-candidate.png"));
            Assert.That(provenance.text, Does.Contain(InfernalHash));
            Assert.That(provenance.text, Does.Contain("\"thirdPartySources\": false"));
            Assert.That(provenance.text, Does.Contain("Bright ember junctions make the 1024-pixel period easier to recognize on device"));

            var folder = InfernalAsset.Substring(0, InfernalAsset.LastIndexOf('/'));
            var images = AssetDatabase.FindAssets("", new[] { folder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => path.EndsWith(".png"))
                .ToArray();
            CollectionAssert.AreEqual(new[] { InfernalAsset }, images, "Only the accepted RGB albedo belongs in this resource folder.");
            return AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalAsset);
        }

        static string Sha256(string path)
        {
            using var hash = SHA256.Create();
            return string.Concat(hash.ComputeHash(File.ReadAllBytes(path)).Select(value => value.ToString("x2")));
        }

        static void AssertImportSettings(string assetPath, TextureWrapMode wrapMode)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            Assert.That(importer, Is.Not.Null, assetPath);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Default));
            Assert.That(importer.sRGBTexture, Is.True);
            Assert.That(importer.mipmapEnabled, Is.True);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
            Assert.That(importer.wrapMode, Is.EqualTo(wrapMode));
            Assert.That(importer.isReadable, Is.False);
            var android = importer.GetPlatformTextureSettings("Android");
            Assert.That(android.overridden, Is.True);
            Assert.That(android.maxTextureSize, Is.EqualTo(512));
            Assert.That(android.format, Is.EqualTo(TextureImporterFormat.ASTC_6x6));
            Assert.That(android.textureCompression, Is.EqualTo(TextureImporterCompression.Compressed));

            Assert.That(AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath), Is.Not.Null, assetPath);
        }

        static GameObject CreateRoute(List<GameObject> roots)
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roots.Add(root);
            root.transform.localScale = new Vector3(14, .5f, 68);
            return root;
        }

        static void AssertFallback(Material material, Color expected)
        {
            Assert.That(material.mainTexture, Is.Null);
            Assert.That(material.color.r, Is.EqualTo(expected.r).Within(.001f));
            Assert.That(material.color.g, Is.EqualTo(expected.g).Within(.001f));
            Assert.That(material.color.b, Is.EqualTo(expected.b).Within(.001f));
        }

        static void DestroyRoots(List<GameObject> roots)
        {
            foreach (var root in roots.Where(root => root)) Object.DestroyImmediate(root);
            roots.Clear();
        }
    }
}
