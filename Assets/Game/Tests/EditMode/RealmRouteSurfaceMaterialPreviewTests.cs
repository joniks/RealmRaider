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
        const string SylvanAsset = "Assets/Game/Resources/Art/WorldSurfaces/MWS02-SylvanGroundMaterialCandidate/sylvan-ground-albedo-rgb-candidate.png";
        const string InfernalAsset = "Assets/Game/Resources/Art/WorldSurfaces/MWS03-InfernalGroundMaterialCandidate/infernal-ground-albedo-rgb-candidate.png";

        [Test]
        public void ImportedPreviewProviderCachesExactAlbedosAndPreservesColorFallback()
        {
            var sylvanTexture = AssertPreviewImport(SylvanAsset, "realmraiders.preview.sylvan-ground-material.mws02.v1");
            var infernalTexture = AssertPreviewImport(InfernalAsset, "realmraiders.preview.infernal-ground-material.mws03.v1");
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
                CollectionAssert.AreEquivalent(new[] { RealmRoutePresentation.SylvanAlbedoResource, RealmRoutePresentation.InfernalAlbedoResource }, requests);

                DestroyRoots(roots);
                RealmRoutePresentation.ConfigureTextureLoaderForTests(_ => throw new System.InvalidOperationException("Failed preview resource"));
                var fallbackSylvanA = CreateRoute(roots);
                var fallbackSylvanB = CreateRoute(roots);
                var fallbackInfernal = CreateRoute(roots);
                var fallbackSylvanPresentation = RealmRoutePresentation.BuildDefenseLane(fallbackSylvanA.transform, RealmRouteStyle.SylvanOrganic);
                RealmRoutePresentation.BuildDefenseLane(fallbackSylvanB.transform, RealmRouteStyle.SylvanOrganic);
                var fallbackInfernalPresentation = RealmRoutePresentation.BuildDefenseLane(fallbackInfernal.transform, RealmRouteStyle.InfernalFractured);

                AssertFallback(fallbackSylvanA.GetComponent<Renderer>().sharedMaterial, new Color(.08f, .24f, .1f));
                AssertFallback(fallbackSylvanPresentation.GetComponentInChildren<Renderer>().sharedMaterial, new Color(.25f, .36f, .22f));
                AssertFallback(fallbackInfernal.GetComponent<Renderer>().sharedMaterial, new Color(.12f, .045f, .035f));
                AssertFallback(fallbackInfernalPresentation.GetComponentInChildren<Renderer>().sharedMaterial, new Color(.27f, .23f, .2f));
                Assert.That(fallbackSylvanB.GetComponent<Renderer>().sharedMaterial, Is.SameAs(fallbackSylvanA.GetComponent<Renderer>().sharedMaterial));
            }
            finally
            {
                DestroyRoots(roots);
                RealmRoutePresentation.ResetTextureLoaderForTests();
            }
        }

        static Texture2D AssertPreviewImport(string assetPath, string candidateId)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            Assert.That(importer, Is.Not.Null, assetPath);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Default));
            Assert.That(importer.sRGBTexture, Is.True);
            Assert.That(importer.mipmapEnabled, Is.True);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
            Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));
            Assert.That(importer.isReadable, Is.False);
            var android = importer.GetPlatformTextureSettings("Android");
            Assert.That(android.overridden, Is.True);
            Assert.That(android.maxTextureSize, Is.EqualTo(512));
            Assert.That(android.format, Is.EqualTo(TextureImporterFormat.ASTC_6x6));

            var provenancePath = assetPath.Substring(0, assetPath.LastIndexOf('/') + 1) + "provenance.json";
            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(provenancePath);
            Assert.That(provenance, Is.Not.Null, provenancePath);
            Assert.That(provenance.text, Does.Contain(candidateId));
            Assert.That(provenance.text, Does.Contain("preview-import-candidate-not-approved-for-runtime"));
            Assert.That(provenance.text, Does.Contain("\"tileabilityVerified\": false"));
            Assert.That(AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath.Replace("albedo", "normal")), Is.Null, "Deferred normal candidates must not enter the main project.");
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            Assert.That(texture, Is.Not.Null, assetPath);
            return texture;
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
