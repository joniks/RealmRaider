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
        const string RouteNormalFolder = "Assets/Game/Resources/Art/WorldSurfaces/MWS12-WalkableRouteNormalPair";
        const string SylvanRouteNormalAsset = RouteNormalFolder + "/sylvan-route-mobile-normal-rgb-candidate.png";
        const string InfernalRouteNormalAsset = RouteNormalFolder + "/infernal-route-mobile-normal-rgb-candidate.png";
        const string RouteNormalProvenanceAsset = RouteNormalFolder + "/provenance.json";
        const string InfernalCourtyardFolder = "Assets/Game/Resources/Art/WorldSurfaces/MWS11-InfernalCourtyardFloor";
        const string InfernalCourtyardAlbedoAsset = InfernalCourtyardFolder + "/infernal-courtyard-floor-albedo-rgb-candidate.png";
        const string InfernalCourtyardNormalAsset = InfernalCourtyardFolder + "/infernal-courtyard-floor-mobile-normal-rgb-candidate.png";
        const string InfernalCourtyardProvenanceAsset = InfernalCourtyardFolder + "/provenance.json";
        const string LegacyInfernalAsset = "Assets/Game/Resources/Art/WorldSurfaces/MWS03-InfernalGroundMaterialCandidate/infernal-ground-albedo-rgb-candidate.png";
        const string SylvanResource = "Art/WorldSurfaces/MWS07-SylvanPath/sylvan-stone-path-edge-hardened-candidate";
        const string InfernalResource = "Art/WorldSurfaces/MWS07-InfernalPath/infernal-basalt-path-edge-hardened-candidate";
        const string InfernalHash = "c8df59807a4fe21c9a5cc27ce3f776f43f1be1a689aab0366c27fd245606da88";
        const string SylvanRouteNormalHash = "aec5888a187057ba19b5a41c98c79100719f01a50aec8c0a7eeb25d8ea514dae";
        const string InfernalRouteNormalHash = "34a99a389b7ffa28b37b81e7b02460a5af7646358726b27d347280bcc23aa816";
        const string InfernalCourtyardAlbedoHash = "666ce002ccf7dc577264eef1062e0d100fab2cb5195058398186427e2269a52c";
        const string InfernalCourtyardNormalHash = "731339751ae4c004379cbba1c424e87efc33f390e9ef5708071b5e1076bc46f7";

        [Test]
        public void ImportedPreviewProviderBindsStyleLocalFloorAndRouteSurfaces()
        {
            Assert.That(RealmRoutePresentation.SylvanAlbedoResource, Is.EqualTo(SylvanResource));
            Assert.That(RealmRoutePresentation.InfernalAlbedoResource, Is.EqualTo(InfernalResource));
            Assert.That(RealmRoutePresentation.RouteNormalStrength, Is.EqualTo(.20f));
            Assert.That(RealmRoutePresentation.SylvanRouteNormalResource, Is.EqualTo("Art/WorldSurfaces/MWS12-WalkableRouteNormalPair/sylvan-route-mobile-normal-rgb-candidate"));
            Assert.That(RealmRoutePresentation.InfernalRouteNormalResource, Is.EqualTo("Art/WorldSurfaces/MWS12-WalkableRouteNormalPair/infernal-route-mobile-normal-rgb-candidate"));
            var sylvanTexture = AssertSylvanPreviewImport();
            var infernalTexture = AssertInfernalPreviewImport();
            var sylvanRouteNormal = AssertRouteNormalPreviewImport(SylvanRouteNormalAsset, SylvanRouteNormalHash);
            var infernalRouteNormal = AssertRouteNormalPreviewImport(InfernalRouteNormalAsset, InfernalRouteNormalHash);
            var courtyardAlbedo = AssertInfernalCourtyardPreviewImport();
            var courtyardNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalCourtyardNormalAsset);
            Assert.That(AssetDatabase.LoadAssetAtPath<Texture2D>(LegacyInfernalAsset), Is.Not.Null, "MWS03 remains available as the unbound legacy preview.");
            var roots = new List<GameObject>();
            try
            {
                var requests = new List<string>();
                RealmRoutePresentation.ConfigureTextureLoaderForTests(path =>
                {
                    requests.Add(path);
                    return path == RealmRoutePresentation.SylvanAlbedoResource ? sylvanTexture :
                        path == RealmRoutePresentation.InfernalAlbedoResource ? infernalTexture :
                        path == RealmRoutePresentation.SylvanRouteNormalResource ? sylvanRouteNormal :
                        path == RealmRoutePresentation.InfernalRouteNormalResource ? infernalRouteNormal :
                        path == RealmRoutePresentation.InfernalCourtyardAlbedoResource ? courtyardAlbedo :
                        path == RealmRoutePresentation.InfernalCourtyardNormalResource ? courtyardNormal : null;
                });

                var firstSylvan = CreateRoute(roots);
                var secondSylvan = CreateRoute(roots);
                var infernal = CreateRoute(roots);
                var secondInfernal = CreateRoute(roots, new Vector3(20, .5f, 32));
                var firstPresentation = RealmRoutePresentation.BuildDefenseLane(firstSylvan.transform, RealmRouteStyle.SylvanOrganic);
                var secondPresentation = RealmRoutePresentation.BuildDefenseLane(secondSylvan.transform, RealmRouteStyle.SylvanOrganic);
                var infernalPresentation = RealmRoutePresentation.BuildDefenseLane(infernal.transform, RealmRouteStyle.InfernalFractured);
                RealmRoutePresentation.BuildDefenseLane(secondInfernal.transform, RealmRouteStyle.InfernalFractured);

                Assert.That(firstSylvan.GetComponent<Renderer>().sharedMaterial.mainTexture, Is.SameAs(sylvanTexture));
                Assert.That(firstSylvan.GetComponent<Renderer>().sharedMaterial.GetTexture("_BumpMap"), Is.Null, "Route normals never attach to the Sylvan floor.");
                Assert.That(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial.mainTexture, Is.SameAs(sylvanTexture));
                AssertRouteNormal(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial, sylvanRouteNormal);
                var infernalFloor = infernal.GetComponent<Renderer>().sharedMaterial;
                Assert.That(infernalFloor.mainTexture, Is.SameAs(courtyardAlbedo));
                Assert.That(infernalFloor.GetTexture("_BumpMap"), Is.SameAs(courtyardNormal));
                Assert.That(infernalFloor.GetFloat("_BumpScale"), Is.EqualTo(RealmRoutePresentation.InfernalCourtyardNormalStrength).Within(.001f));
                Assert.That(infernalFloor.IsKeywordEnabled("_NORMALMAP"), Is.True);
                Assert.That(infernalPresentation.GetComponentInChildren<Renderer>().sharedMaterial.mainTexture, Is.SameAs(infernalTexture));
                AssertRouteNormal(infernalPresentation.GetComponentInChildren<Renderer>().sharedMaterial, infernalRouteNormal);
                Assert.That(secondInfernal.GetComponent<Renderer>().sharedMaterial, Is.SameAs(infernalFloor));
                AssertRendererCourtyardTiling(infernal.GetComponent<Renderer>(), new Vector2(14f / RealmRoutePresentation.InfernalCourtyardWorldUnitsPerTile, 68f / RealmRoutePresentation.InfernalCourtyardWorldUnitsPerTile));
                AssertRendererCourtyardTiling(secondInfernal.GetComponent<Renderer>(), new Vector2(20f / RealmRoutePresentation.InfernalCourtyardWorldUnitsPerTile, 32f / RealmRoutePresentation.InfernalCourtyardWorldUnitsPerTile));
                Assert.That(secondSylvan.GetComponent<Renderer>().sharedMaterial, Is.SameAs(firstSylvan.GetComponent<Renderer>().sharedMaterial));
                Assert.That(secondPresentation.GetComponentInChildren<Renderer>().sharedMaterial, Is.SameAs(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial));
                Assert.That(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial,
                    Is.Not.SameAs(firstSylvan.GetComponent<Renderer>().sharedMaterial));
                CollectionAssert.AreEqual(new[]
                {
                    SylvanResource,
                    RealmRoutePresentation.SylvanRouteNormalResource,
                    RealmRoutePresentation.InfernalCourtyardAlbedoResource,
                    RealmRoutePresentation.InfernalCourtyardNormalResource,
                    RealmRoutePresentation.InfernalAlbedoResource,
                    RealmRoutePresentation.InfernalRouteNormalResource
                }, requests);
            }
            finally
            {
                DestroyRoots(roots);
                RealmRoutePresentation.ResetTextureLoaderForTests();
            }
        }

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void MissingOrThrowingSylvanRoutePairPreservesExactCachedRouteFallback(int failureMode)
        {
            var roots = new List<GameObject>();
            var loadCount = 0;
            var sylvanAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>(SylvanAsset);
            try
            {
                RealmRoutePresentation.ConfigureTextureLoaderForTests(path =>
                {
                    loadCount++;
                    if (path == RealmRoutePresentation.SylvanAlbedoResource)
                    {
                        if (failureMode == 2) throw new System.InvalidOperationException("Sylvan route albedo unavailable");
                        return failureMode == 0 ? null : sylvanAlbedo;
                    }
                    Assert.That(path, Is.EqualTo(RealmRoutePresentation.SylvanRouteNormalResource));
                    if (failureMode == 3) throw new System.InvalidOperationException("Sylvan route normal unavailable");
                    return failureMode == 1 ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(SylvanRouteNormalAsset);
                });

                var first = CreateRoute(roots);
                var second = CreateRoute(roots);
                var firstPresentation = RealmRoutePresentation.BuildDefenseLane(first.transform, RealmRouteStyle.SylvanOrganic);
                var secondPresentation = RealmRoutePresentation.BuildDefenseLane(second.transform, RealmRouteStyle.SylvanOrganic);

                Assert.That(loadCount, Is.EqualTo(failureMode == 0 || failureMode == 2 ? 1 : 2));
                if (failureMode == 0 || failureMode == 2) AssertFallback(first.GetComponent<Renderer>().sharedMaterial, new Color(.08f, .24f, .1f));
                else Assert.That(first.GetComponent<Renderer>().sharedMaterial.mainTexture, Is.SameAs(sylvanAlbedo));
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

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void MissingOrThrowingInfernalRoutePairPreservesCausewayFallbackWithoutDiscardingCourtyardFloor(int failureMode)
        {
            var roots = new List<GameObject>();
            var loadCount = 0;
            var courtyardAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalCourtyardAlbedoAsset);
            var courtyardNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalCourtyardNormalAsset);
            try
            {
                RealmRoutePresentation.ConfigureTextureLoaderForTests(path =>
                {
                    loadCount++;
                    if (path == RealmRoutePresentation.InfernalCourtyardAlbedoResource) return courtyardAlbedo;
                    if (path == RealmRoutePresentation.InfernalCourtyardNormalResource) return courtyardNormal;
                    if (path == RealmRoutePresentation.InfernalAlbedoResource)
                    {
                        if (failureMode == 2) throw new System.InvalidOperationException("Infernal route albedo unavailable");
                        return failureMode == 0 ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalAsset);
                    }
                    Assert.That(path, Is.EqualTo(RealmRoutePresentation.InfernalRouteNormalResource));
                    if (failureMode == 3) throw new System.InvalidOperationException("Infernal route normal unavailable");
                    return failureMode == 1 ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalRouteNormalAsset);
                });

                var first = CreateRoute(roots);
                var second = CreateRoute(roots);
                var firstPresentation = RealmRoutePresentation.BuildDefenseLane(first.transform, RealmRouteStyle.InfernalFractured);
                var secondPresentation = RealmRoutePresentation.BuildDefenseLane(second.transform, RealmRouteStyle.InfernalFractured);

                Assert.That(loadCount, Is.EqualTo(failureMode == 0 || failureMode == 2 ? 3 : 4));
                Assert.That(first.GetComponent<Renderer>().sharedMaterial.mainTexture, Is.SameAs(courtyardAlbedo));
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

        [TestCase(0)]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void MissingOrThrowingInfernalCourtyardPairPreservesExactCachedFloorFallback(int failureMode)
        {
            var routeAlbedo = AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalAsset);
            var routeNormal = AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalRouteNormalAsset);
            var requests = new List<string>();
            var roots = new List<GameObject>();
            try
            {
                RealmRoutePresentation.ConfigureTextureLoaderForTests(path =>
                {
                    requests.Add(path);
                    if (path == RealmRoutePresentation.InfernalCourtyardAlbedoResource)
                    {
                        if (failureMode == 2) throw new System.InvalidOperationException("Courtyard albedo unavailable");
                        return failureMode == 0 ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalCourtyardAlbedoAsset);
                    }
                    if (path == RealmRoutePresentation.InfernalCourtyardNormalResource)
                    {
                        if (failureMode == 3) throw new System.InvalidOperationException("Courtyard normal unavailable");
                        return failureMode == 1 ? null : AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalCourtyardNormalAsset);
                    }
                    if (path == RealmRoutePresentation.InfernalAlbedoResource) return routeAlbedo;
                    return path == RealmRoutePresentation.InfernalRouteNormalResource ? routeNormal : null;
                });
                var first = CreateRoute(roots);
                var second = CreateRoute(roots);
                var firstPresentation = RealmRoutePresentation.BuildDefenseLane(first.transform, RealmRouteStyle.InfernalFractured);
                var secondPresentation = RealmRoutePresentation.BuildDefenseLane(second.transform, RealmRouteStyle.InfernalFractured);

                var failedPairRequests = failureMode == 0 || failureMode == 2 ? 1 : 2;
                Assert.That(requests.Count(path => path == RealmRoutePresentation.InfernalCourtyardAlbedoResource || path == RealmRoutePresentation.InfernalCourtyardNormalResource), Is.EqualTo(failedPairRequests));
                Assert.That(requests.Count(path => path == RealmRoutePresentation.InfernalAlbedoResource), Is.EqualTo(1));
                Assert.That(requests.Count(path => path == RealmRoutePresentation.InfernalRouteNormalResource), Is.EqualTo(1));
                AssertFallback(first.GetComponent<Renderer>().sharedMaterial, new Color(.12f, .045f, .035f));
                Assert.That(second.GetComponent<Renderer>().sharedMaterial, Is.SameAs(first.GetComponent<Renderer>().sharedMaterial));
                Assert.That(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial.mainTexture, Is.SameAs(routeAlbedo));
                AssertRouteNormal(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial, routeNormal);
                Assert.That(secondPresentation.GetComponentInChildren<Renderer>().sharedMaterial, Is.SameAs(firstPresentation.GetComponentInChildren<Renderer>().sharedMaterial));
            }
            finally
            {
                DestroyRoots(roots);
                RealmRoutePresentation.ResetTextureLoaderForTests();
            }
        }

        [Test]
        public void RoutePairFailuresCacheIndependentlyWithoutCrossRealmFallback()
        {
            var requests = new List<string>();
            var roots = new List<GameObject>();
            try
            {
                RealmRoutePresentation.ConfigureTextureLoaderForTests(path =>
                {
                    requests.Add(path);
                    if (path == RealmRoutePresentation.SylvanAlbedoResource) return AssetDatabase.LoadAssetAtPath<Texture2D>(SylvanAsset);
                    if (path == RealmRoutePresentation.SylvanRouteNormalResource) return null;
                    if (path == RealmRoutePresentation.InfernalCourtyardAlbedoResource) return AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalCourtyardAlbedoAsset);
                    if (path == RealmRoutePresentation.InfernalCourtyardNormalResource) return AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalCourtyardNormalAsset);
                    if (path == RealmRoutePresentation.InfernalAlbedoResource) return AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalAsset);
                    return path == RealmRoutePresentation.InfernalRouteNormalResource
                        ? AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalRouteNormalAsset) : null;
                });
                var sylvan = CreateRoute(roots);
                var secondSylvan = CreateRoute(roots);
                var infernal = CreateRoute(roots);
                var secondInfernal = CreateRoute(roots);
                var sylvanPresentation = RealmRoutePresentation.BuildDefenseLane(sylvan.transform, RealmRouteStyle.SylvanOrganic);
                RealmRoutePresentation.BuildDefenseLane(secondSylvan.transform, RealmRouteStyle.SylvanOrganic);
                var infernalPresentation = RealmRoutePresentation.BuildDefenseLane(infernal.transform, RealmRouteStyle.InfernalFractured);
                RealmRoutePresentation.BuildDefenseLane(secondInfernal.transform, RealmRouteStyle.InfernalFractured);

                Assert.That(requests.Count(path => path == RealmRoutePresentation.SylvanRouteNormalResource), Is.EqualTo(1));
                Assert.That(requests.Count(path => path == RealmRoutePresentation.InfernalRouteNormalResource), Is.EqualTo(1));
                AssertFallback(sylvanPresentation.GetComponentInChildren<Renderer>().sharedMaterial, new Color(.25f, .36f, .22f));
                AssertRouteNormal(infernalPresentation.GetComponentInChildren<Renderer>().sharedMaterial,
                    AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalRouteNormalAsset));
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

        static Texture2D AssertInfernalCourtyardPreviewImport()
        {
            AssertImportSettings(InfernalCourtyardAlbedoAsset, TextureWrapMode.Repeat);
            AssertNormalImportSettings(InfernalCourtyardNormalAsset, TextureWrapMode.Repeat);
            Assert.That(Sha256(InfernalCourtyardAlbedoAsset), Is.EqualTo(InfernalCourtyardAlbedoHash));
            Assert.That(Sha256(InfernalCourtyardNormalAsset), Is.EqualTo(InfernalCourtyardNormalHash));
            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(InfernalCourtyardProvenanceAsset);
            Assert.That(provenance, Is.Not.Null, InfernalCourtyardProvenanceAsset);
            Assert.That(provenance.text, Does.Contain("realmraiders.preview.infernal-courtyard-floor.mws11.v1"));
            Assert.That(provenance.text, Does.Contain("preview-import-candidate-not-final-art").And.Contain("no third-party source"));
            Assert.That(provenance.text, Does.Contain("\"modulesCommit\": \"2f5457f\""));
            Assert.That(provenance.text, Does.Contain(InfernalCourtyardAlbedoHash).And.Contain(InfernalCourtyardNormalHash));
            Assert.That(provenance.text, Does.Contain("--strength 0.30").And.Contain("4.0 world units").And.Contain("repeat periodicity"));
            CollectionAssert.AreEqual(new[] { InfernalCourtyardAlbedoAsset, InfernalCourtyardNormalAsset },
                AssetDatabase.FindAssets("", new[] { InfernalCourtyardFolder }).Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => path.EndsWith(".png")).OrderBy(path => path).ToArray());
            return AssetDatabase.LoadAssetAtPath<Texture2D>(InfernalCourtyardAlbedoAsset);
        }

        static Texture2D AssertRouteNormalPreviewImport(string assetPath, string expectedHash)
        {
            AssertNormalImportSettings(assetPath, TextureWrapMode.Repeat);
            Assert.That(Sha256(assetPath), Is.EqualTo(expectedHash));
            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(RouteNormalProvenanceAsset);
            Assert.That(provenance, Is.Not.Null, RouteNormalProvenanceAsset);
            Assert.That(provenance.text, Does.Contain("realmraiders.preview.walkable-route-normal-pair.mws12.v1"));
            Assert.That(provenance.text, Does.Contain("preview-import-candidate-not-final-art").And.Contain("no third-party source"));
            Assert.That(provenance.text, Does.Contain("\"modulesCommit\": \"e4d1358\""));
            Assert.That(provenance.text, Does.Contain(expectedHash).And.Contain("0.20").And.Contain("periodicity"));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MWS12-WalkableRouteNormalPair/" + Path.GetFileName(assetPath)));
            CollectionAssert.AreEqual(new[] { InfernalRouteNormalAsset, SylvanRouteNormalAsset },
                AssetDatabase.FindAssets("", new[] { RouteNormalFolder }).Select(AssetDatabase.GUIDToAssetPath)
                    .Where(path => path.EndsWith(".png")).OrderBy(path => path).ToArray());
            return AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
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

        static void AssertNormalImportSettings(string assetPath, TextureWrapMode wrapMode)
        {
            var importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            Assert.That(importer, Is.Not.Null, assetPath);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.NormalMap));
            Assert.That(importer.sRGBTexture, Is.False);
            Assert.That(importer.mipmapEnabled, Is.True);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
            Assert.That(importer.wrapMode, Is.EqualTo(wrapMode));
            Assert.That(importer.isReadable, Is.False);
            Assert.That(importer.convertToNormalmap, Is.False);
            var android = importer.GetPlatformTextureSettings("Android");
            Assert.That(android.overridden, Is.True);
            Assert.That(android.maxTextureSize, Is.EqualTo(512));
            Assert.That(android.format, Is.EqualTo(TextureImporterFormat.ASTC_6x6));
            Assert.That(android.textureCompression, Is.EqualTo(TextureImporterCompression.Compressed));
            Assert.That(AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath), Is.Not.Null, assetPath);
        }

        static GameObject CreateRoute(List<GameObject> roots, Vector3? scale = null)
        {
            var root = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roots.Add(root);
            root.transform.localScale = scale ?? new Vector3(14, .5f, 68);
            return root;
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

        static void AssertRouteNormal(Material material, Texture2D expectedNormal)
        {
            Assert.That(material.GetTexture("_BumpMap"), Is.SameAs(expectedNormal));
            Assert.That(material.GetFloat("_BumpScale"), Is.EqualTo(RealmRoutePresentation.RouteNormalStrength).Within(.001f));
            Assert.That(material.IsKeywordEnabled("_NORMALMAP"), Is.True);
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
