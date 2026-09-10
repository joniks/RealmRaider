using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using RealmRaiders.Realm;
using UnityEditor;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class RealmNodeSurfaceMaterialPreviewTests
    {
        const string Folder = "Assets/Game/Resources/Art/WorldSurfaces/MWS10-SylvanClearingFloor";
        const string AlbedoAsset = Folder + "/sylvan-clearing-floor-albedo-rgb-candidate.png";
        const string NormalAsset = Folder + "/sylvan-clearing-floor-mobile-normal-rgb-candidate.png";
        const string ProvenanceAsset = Folder + "/provenance.json";
        const string AlbedoHash = "e7731ffb989ecb457b787ea19bd13314d61b684b0d4fc75b8adb507501ce6ed6";
        const string NormalHash = "aca8df9a0b0ed56bea8c839284cb4814d99ffb344307b3e3476a12262f6dcef5";
        static readonly Color Moss = new(.14f, .4f, .16f);
        static readonly Color HiddenTint = new(.08f, .17f, .11f);
        static readonly Color VisitedTint = new(.16f, .34f, .18f);

        [Test]
        public void AcceptedClearingAssetsAreByteExactAndExplicitlyImportedForMobile()
        {
            Assert.That(Sha256(AlbedoAsset), Is.EqualTo(AlbedoHash));
            Assert.That(Sha256(NormalAsset), Is.EqualTo(NormalHash));
            CollectionAssert.AreEqual(new[] { AlbedoAsset, NormalAsset }, AssetDatabase.FindAssets("", new[] { Folder })
                .Select(AssetDatabase.GUIDToAssetPath).Where(path => path.EndsWith(".png")).OrderBy(path => path).ToArray(),
                "Only the accepted 1024 RGB pair belongs in the runtime resource folder.");

            AssertImport(AlbedoAsset, TextureImporterType.Default, true);
            AssertImport(NormalAsset, TextureImporterType.NormalMap, false);
            Assert.That(PngDimensions(AlbedoAsset), Is.EqualTo(new Vector2Int(1024, 1024)));
            Assert.That(PngDimensions(NormalAsset), Is.EqualTo(new Vector2Int(1024, 1024)));

            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(ProvenanceAsset);
            Assert.That(provenance, Is.Not.Null);
            Assert.That(provenance.text, Does.Contain("realmraiders.preview.sylvan-clearing-floor.mws10.v1"));
            Assert.That(provenance.text, Does.Contain("accepted-runtime-preview-not-final-art"));
            Assert.That(provenance.text, Does.Contain("no third-party source"));
            Assert.That(provenance.text, Does.Contain("\"modulesCommit\": \"646ecde\""));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MWS10-SylvanClearingFloor/sylvan-clearing-floor-albedo-rgb-candidate.png"));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MWS10-SylvanClearingFloor/sylvan-clearing-floor-mobile-normal-rgb-candidate.png"));
            Assert.That(provenance.text, Does.Contain(AlbedoHash).And.Contain(NormalHash));
            Assert.That(provenance.text, Does.Contain("\"tileabilityVerified\": false"));
        }

        [Test]
        public void BindingLoadsAtomicPairOnceCachesOneMaterialAndAppliesPerNodeTint()
        {
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoAsset);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(NormalAsset);
            var requests = new List<string>();
            var roots = new List<GameObject>();
            try
            {
                RealmNodeSurfacePresentation.ConfigureTextureLoaderForTests(path =>
                {
                    requests.Add(path);
                    return path == RealmNodeSurfacePresentation.AlbedoResource ? albedo :
                        path == RealmNodeSurfacePresentation.NormalResource ? normal : null;
                });
                var first = CreateFloor(roots, "First Clearing");
                var second = CreateFloor(roots, "Second Clearing");
                var firstChildCount = first.transform.childCount;
                var firstColliders = first.GetComponentsInChildren<Collider>(true);

                var firstBinding = RealmNodeSurfacePresentation.Bind(first.GetComponent<Renderer>(), Moss);
                var repeatedBinding = RealmNodeSurfacePresentation.Bind(first.GetComponent<Renderer>(), Moss);
                var secondBinding = RealmNodeSurfacePresentation.Bind(second.GetComponent<Renderer>(), Moss);
                firstBinding.ApplyTint(HiddenTint);
                secondBinding.ApplyTint(VisitedTint);

                CollectionAssert.AreEqual(new[]
                {
                    RealmNodeSurfacePresentation.AlbedoResource,
                    RealmNodeSurfacePresentation.NormalResource
                }, requests);
                Assert.That(repeatedBinding, Is.SameAs(firstBinding));
                Assert.That(firstBinding.UsesSurface && secondBinding.UsesSurface, Is.True);
                Assert.That(first.GetComponent<Renderer>().sharedMaterial, Is.SameAs(second.GetComponent<Renderer>().sharedMaterial));
                var material = first.GetComponent<Renderer>().sharedMaterial;
                Assert.That(material.mainTexture, Is.SameAs(albedo));
                Assert.That(material.mainTextureScale.x, Is.EqualTo(RealmNodeSurfacePresentation.TextureTiling).Within(.001f));
                Assert.That(material.mainTextureScale.y, Is.EqualTo(RealmNodeSurfacePresentation.TextureTiling).Within(.001f));
                Assert.That(material.GetTexture("_BumpMap"), Is.SameAs(normal));
                Assert.That(material.GetFloat("_BumpScale"), Is.EqualTo(RealmNodeSurfacePresentation.NormalStrength).Within(.001f));
                Assert.That(material.IsKeywordEnabled("_NORMALMAP"), Is.True);
                AssertRendererTint(first.GetComponent<Renderer>(), HiddenTint);
                AssertRendererTint(second.GetComponent<Renderer>(), VisitedTint);
                Assert.That(first.transform.childCount, Is.EqualTo(firstChildCount));
                CollectionAssert.AreEqual(firstColliders, first.GetComponentsInChildren<Collider>(true));
                Assert.That(first.GetComponents<RealmNodeSurfacePresentation>(), Has.Length.EqualTo(1));
            }
            finally
            {
                DestroyRoots(roots);
                RealmNodeSurfacePresentation.ResetTextureLoaderForTests();
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        public void MissingOrThrowingPairKeepsExactSolidColourFogPath(int failureMode)
        {
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoAsset);
            var requests = new List<string>();
            var roots = new List<GameObject>();
            try
            {
                RealmNodeSurfacePresentation.ConfigureTextureLoaderForTests(path =>
                {
                    requests.Add(path);
                    if (path == RealmNodeSurfacePresentation.AlbedoResource)
                    {
                        if (failureMode == 2) throw new System.InvalidOperationException("Albedo unavailable");
                        return failureMode == 0 ? null : albedo;
                    }
                    if (failureMode == 1) return null;
                    throw new System.InvalidOperationException("Normal unavailable");
                });
                var first = CreateFloor(roots, "Fallback Clearing One");
                var second = CreateFloor(roots, "Fallback Clearing Two");
                var firstBinding = RealmNodeSurfacePresentation.Bind(first.GetComponent<Renderer>(), Moss);
                var secondBinding = RealmNodeSurfacePresentation.Bind(second.GetComponent<Renderer>(), Moss);
                firstBinding.ApplyTint(HiddenTint);
                secondBinding.ApplyTint(VisitedTint);

                Assert.That(requests, Has.Count.EqualTo(failureMode == 0 || failureMode == 2 ? 1 : 2));
                Assert.That(firstBinding.UsesSurface || secondBinding.UsesSurface, Is.False);
                AssertSolidFallback(first.GetComponent<Renderer>().sharedMaterial, HiddenTint);
                AssertSolidFallback(second.GetComponent<Renderer>().sharedMaterial, VisitedTint);
            }
            finally
            {
                DestroyRoots(roots);
                RealmNodeSurfacePresentation.ResetTextureLoaderForTests();
            }
        }

        [Test]
        public void RealmNodeFogTransitionsPreserveIntentionalUnvisitedAndVisitedTints()
        {
            var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoAsset);
            var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(NormalAsset);
            var roots = new List<GameObject>();
            try
            {
                RealmNodeSurfacePresentation.ConfigureTextureLoaderForTests(path =>
                    path == RealmNodeSurfacePresentation.AlbedoResource ? albedo : normal);
                var area = new GameObject("Fog Tint Clearing"); roots.Add(area);
                var floor = CreateFloor(roots, "Fog Tint Floor"); floor.transform.SetParent(area.transform, false);
                var binding = RealmNodeSurfacePresentation.Bind(floor.GetComponent<Renderer>(), Moss);
                var node = new RealmNode("Fog Tint Node");
                var view = area.AddComponent<RealmNodeView>(); view.Initialize(node, null, floor.GetComponent<Renderer>());

                Assert.That(floor.GetComponent<Renderer>().enabled, Is.False);
                Assert.That(binding.AppliedTint, Is.EqualTo(HiddenTint));
                node.Discover();
                Assert.That(floor.GetComponent<Renderer>().enabled, Is.True);
                Assert.That(binding.AppliedTint, Is.EqualTo(HiddenTint));
                AssertRendererTint(floor.GetComponent<Renderer>(), HiddenTint);
                node.Visit();
                Assert.That(binding.AppliedTint, Is.EqualTo(VisitedTint));
                AssertRendererTint(floor.GetComponent<Renderer>(), VisitedTint);
            }
            finally
            {
                DestroyRoots(roots);
                RealmNodeSurfacePresentation.ResetTextureLoaderForTests();
            }
        }

        static GameObject CreateFloor(List<GameObject> roots, string name)
        {
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            floor.name = name;
            floor.transform.localScale = new Vector3(RealmNodeSurfacePresentation.NodeWorldDiameter, .08f,
                RealmNodeSurfacePresentation.NodeWorldDiameter);
            roots.Add(floor);
            return floor;
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
            Assert.That(importer.convertToNormalmap, Is.False);
            var android = importer.GetPlatformTextureSettings("Android");
            Assert.That(android.overridden, Is.True);
            Assert.That(android.maxTextureSize, Is.EqualTo(512));
            Assert.That(android.format, Is.EqualTo(TextureImporterFormat.ASTC_6x6));
            Assert.That(android.textureCompression, Is.EqualTo(TextureImporterCompression.Compressed));
            Assert.That(AssetDatabase.LoadAssetAtPath<Texture2D>(path), Is.Not.Null);
        }

        static void AssertRendererTint(Renderer renderer, Color expected)
        {
            var properties = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(properties);
            var name = renderer.sharedMaterial.HasProperty("_BaseColor") ? "_BaseColor" : "_Color";
            Assert.That(properties.GetColor(name), Is.EqualTo(expected));
        }

        static void AssertSolidFallback(Material material, Color expected)
        {
            Assert.That(material.mainTexture, Is.Null);
            Assert.That(material.IsKeywordEnabled("_NORMALMAP"), Is.False);
            Assert.That(material.color, Is.EqualTo(expected));
        }

        static string Sha256(string path)
        {
            using (var hash = SHA256.Create())
                return string.Concat(hash.ComputeHash(File.ReadAllBytes(path)).Select(value => value.ToString("x2")));
        }

        static Vector2Int PngDimensions(string path)
        {
            var bytes = File.ReadAllBytes(path);
            if (bytes.Length < 24 || bytes[12] != 'I' || bytes[13] != 'H' || bytes[14] != 'D' || bytes[15] != 'R')
                throw new InvalidDataException($"Expected a PNG with an IHDR header at '{path}'.");
            return new Vector2Int(BigEndianInt(bytes, 16), BigEndianInt(bytes, 20));
        }

        static int BigEndianInt(byte[] bytes, int offset) =>
            bytes[offset] << 24 | bytes[offset + 1] << 16 | bytes[offset + 2] << 8 | bytes[offset + 3];

        static void DestroyRoots(List<GameObject> roots)
        {
            foreach (var root in roots.Distinct().Where(root => root)) Object.DestroyImmediate(root);
            roots.Clear();
        }
    }
}
