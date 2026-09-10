using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using RealmRaiders.Core;
using UnityEditor;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class RealmRouteSurfaceMaterialPreviewTests
    {
        const string SylvanAsset = "Assets/Game/Resources/Art/WorldSurfaces/MWS07-SylvanPath/sylvan-stone-path-edge-hardened-candidate.png";
        const string InfernalAsset = "Assets/Game/Resources/Art/WorldSurfaces/MWS03-InfernalGroundMaterialCandidate/infernal-ground-albedo-rgb-candidate.png";
        const string SylvanResource = "Art/WorldSurfaces/MWS07-SylvanPath/sylvan-stone-path-edge-hardened-candidate";

        [Test]
        public void ImportedPreviewProviderBindsAndCachesExactAlbedos()
        {
            Assert.That(RealmRoutePresentation.SylvanAlbedoResource, Is.EqualTo(SylvanResource));
            Assert.That(RealmRoutePresentation.InfernalAlbedoResource,
                Is.EqualTo("Art/WorldSurfaces/MWS03-InfernalGroundMaterialCandidate/infernal-ground-albedo-rgb-candidate"));
            var sylvanTexture = AssertSylvanPreviewImport();
            var infernalTexture = AssertLegacyPreviewImport(InfernalAsset, "realmraiders.preview.infernal-ground-material.mws03.v1");
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

        static Texture2D AssertLegacyPreviewImport(string assetPath, string candidateId)
        {
            AssertImportSettings(assetPath, TextureWrapMode.Clamp);
            var provenancePath = assetPath.Substring(0, assetPath.LastIndexOf('/') + 1) + "provenance.json";
            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(provenancePath);
            Assert.That(provenance, Is.Not.Null, provenancePath);
            Assert.That(provenance.text, Does.Contain(candidateId));
            Assert.That(provenance.text, Does.Contain("preview-import-candidate-not-approved-for-runtime"));
            Assert.That(provenance.text, Does.Contain("\"tileabilityVerified\": false"));
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
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
