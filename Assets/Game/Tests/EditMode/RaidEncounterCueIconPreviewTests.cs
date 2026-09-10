using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using RealmRaiders.Raid;
using RealmRaiders.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class RaidEncounterCueIconPreviewTests
    {
        const string Folder = "Assets/Game/Resources/Art/UI/EncounterCue";
        const string DiscoveredAsset = Folder + "/discovered-rgba-candidate.png";
        const string HostilesAsset = Folder + "/hostiles-rgba-candidate.png";
        const string ClearedAsset = Folder + "/area-clear-rgba-candidate.png";
        const string ProvenanceAsset = Folder + "/provenance.json";
        const string DiscoveredHash = "5b42f7d3220c81eeabf2dd7d1524bf4c946fecc8a47dc57822974c01bee334f1";
        const string HostilesHash = "02229bca31794d8aa556cc1f52ebc27604d02dd9c200b3fcfba891b932942613";
        const string ClearedHash = "3756f8b0aea1b12bc7dd844432a2adb6b1303214baff44038f56eba4cf702b1d";

        [Test]
        public void AcceptedEncounterIconsAreByteExactRgbaSingleSpritesWithExplicitMobileImport()
        {
            var assets = new[] { ClearedAsset, DiscoveredAsset, HostilesAsset };
            CollectionAssert.AreEqual(assets, AssetDatabase.FindAssets("", new[] { Folder })
                .Select(AssetDatabase.GUIDToAssetPath).Where(path => path.EndsWith(".png")).OrderBy(path => path).ToArray(),
                "Evidence sheets must not enter the runtime resource folder.");
            Assert.That(Sha256(DiscoveredAsset), Is.EqualTo(DiscoveredHash));
            Assert.That(Sha256(HostilesAsset), Is.EqualTo(HostilesHash));
            Assert.That(Sha256(ClearedAsset), Is.EqualTo(ClearedHash));
            foreach (var asset in assets)
            {
                AssertPng(asset, 512, 512, 6);
                AssertImport(asset);
            }

            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(ProvenanceAsset);
            Assert.That(provenance, Is.Not.Null);
            Assert.That(provenance.text, Does.Contain("realmraiders.preview.encounter-cues.mui02.v1"));
            Assert.That(provenance.text, Does.Contain("accepted-runtime-preview-not-final-art"));
            Assert.That(provenance.text, Does.Contain("\"thirdPartySources\": false"));
            Assert.That(provenance.text, Does.Contain("\"modulesCommit\": \"3bc738c\""));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI02-EncounterCueIconSet/discovered-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI02-EncounterCueIconSet/hostiles-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI02-EncounterCueIconSet/area-clear-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain(DiscoveredHash).And.Contain(HostilesHash).And.Contain(ClearedHash));
            Assert.That(provenance.text, Does.Contain("generationPrompts").And.Contain("Sylvan waystone seed")
                .And.Contain("three upward dark talon tips").And.Contain("open circular pair of Sylvan leaves"));
        }

        [Test]
        public void PhaseMappingLoadsEachSpriteOnceAndKeepsTruthfulTextAuthoritative()
        {
            var discoveredSprite = AssetDatabase.LoadAssetAtPath<Sprite>(DiscoveredAsset);
            var hostilesSprite = AssetDatabase.LoadAssetAtPath<Sprite>(HostilesAsset);
            var clearedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ClearedAsset);
            Assert.That(RaidEncounterCue.IconResourceFor(RaidEncounterPhase.Discovered), Is.EqualTo(RaidEncounterCue.DiscoveredIconResource));
            Assert.That(RaidEncounterCue.IconResourceFor(RaidEncounterPhase.Hostiles), Is.EqualTo(RaidEncounterCue.HostilesIconResource));
            Assert.That(RaidEncounterCue.IconResourceFor(RaidEncounterPhase.Cleared), Is.EqualTo(RaidEncounterCue.ClearedIconResource));
            Assert.That(RaidEncounterCue.IconResourceFor(RaidEncounterPhase.Hidden), Is.Empty);
            var requests = new List<string>();
            var root = new GameObject("Encounter Icon Mapping");
            try
            {
                RaidEncounterCue.ConfigureSpriteLoaderForTests(path =>
                {
                    requests.Add(path);
                    return path == RaidEncounterCue.DiscoveredIconResource ? discoveredSprite :
                        path == RaidEncounterCue.HostilesIconResource ? hostilesSprite :
                        path == RaidEncounterCue.ClearedIconResource ? clearedSprite : null;
                });
                EncounterStates(out var discovered, out var hostiles, out var cleared);
                var cue = root.AddComponent<RaidEncounterCue>(); cue.Initialize(null);
                Assert.That(root.GetComponentsInChildren<Image>(true), Has.Length.EqualTo(1));
                Assert.That(cue.IconRaycastTarget, Is.False);
                Assert.That(cue.GetComponentInChildren<EventTrigger>(true), Is.Null);
                Assert.That(cue.GetComponentInChildren<Canvas>(true), Is.Null);
                Assert.That(cue.GetComponentInChildren<EventSystem>(true), Is.Null);

                cue.Show(discovered);
                AssertPresentation(cue, discoveredSprite, "CROSSROADS DISCOVERED");
                cue.Show(discovered);
                AssertPresentation(cue, discoveredSprite, "CROSSROADS DISCOVERED");
                cue.Show(hostiles);
                AssertPresentation(cue, hostilesSprite, "WOLF GROVE • 2 HOSTILES");
                cue.Show(cleared);
                AssertPresentation(cue, clearedSprite, "WOLF GROVE • AREA CLEAR");
                CollectionAssert.AreEqual(new[]
                {
                    RaidEncounterCue.DiscoveredIconResource,
                    RaidEncounterCue.HostilesIconResource,
                    RaidEncounterCue.ClearedIconResource
                }, requests);
                cue.Clear();
                Assert.That(cue.Visible || cue.IconVisible, Is.False);
                Assert.That(cue.IconSprite, Is.Null);
            }
            finally
            {
                Object.DestroyImmediate(root);
                RaidEncounterCue.ResetSpriteLoaderForTests();
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingOrThrowingSpritesKeepExactTextOnlyCue(bool loaderThrows)
        {
            var root = new GameObject("Encounter Icon Fallback");
            var requests = 0;
            try
            {
                RaidEncounterCue.ConfigureSpriteLoaderForTests(_ =>
                {
                    requests++;
                    if (loaderThrows) throw new System.InvalidOperationException("Sprite unavailable");
                    return null;
                });
                EncounterStates(out var discovered, out var hostiles, out var cleared);
                var cue = root.AddComponent<RaidEncounterCue>(); cue.Initialize(null);
                foreach (var state in new[] { discovered, hostiles, cleared })
                {
                    cue.Show(state);
                    Assert.That(cue.Text, Is.EqualTo(RaidEncounterCue.CopyFor(state)));
                    Assert.That(cue.Visible, Is.True);
                    Assert.That(cue.IconVisible, Is.False);
                    Assert.That(cue.IconSprite, Is.Null);
                }
                Assert.That(requests, Is.EqualTo(3));
                Assert.That(root.GetComponentsInChildren<Image>(true), Has.Length.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(root);
                RaidEncounterCue.ResetSpriteLoaderForTests();
            }
        }

        static void EncounterStates(out RaidEncounterState discovered, out RaidEncounterState hostiles, out RaidEncounterState cleared)
        {
            var lifecycle = new RaidEncounterLifecycle();
            Assert.That(lifecycle.TryEnter("Crossroads", 0, out discovered), Is.True);
            Assert.That(lifecycle.TryEnter("Wolf Grove", 2, out hostiles), Is.True);
            Assert.That(lifecycle.TrySetRemaining("Wolf Grove", 0, out cleared), Is.True);
        }

        static void AssertPresentation(RaidEncounterCue cue, Sprite expected, string copy)
        {
            Assert.That(cue.Visible, Is.True);
            Assert.That(cue.Text, Is.EqualTo(copy));
            Assert.That(cue.IconVisible, Is.True);
            Assert.That(cue.IconSprite, Is.SameAs(expected));
        }

        static void AssertImport(string asset)
        {
            var importer = AssetImporter.GetAtPath(asset) as TextureImporter;
            Assert.That(importer, Is.Not.Null, asset);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
            Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single));
            Assert.That(importer.alphaIsTransparency, Is.True);
            Assert.That(importer.mipmapEnabled, Is.False);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
            Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));
            Assert.That(importer.isReadable, Is.False);
            var android = importer.GetPlatformTextureSettings("Android");
            Assert.That(android.overridden, Is.True);
            Assert.That(android.maxTextureSize, Is.EqualTo(256));
            Assert.That(android.format, Is.EqualTo(TextureImporterFormat.ASTC_6x6));
            Assert.That(android.textureCompression, Is.EqualTo(TextureImporterCompression.Compressed));
            Assert.That(AssetDatabase.LoadAssetAtPath<Sprite>(asset), Is.Not.Null);
        }

        static void AssertPng(string path, int width, int height, byte colorType)
        {
            var bytes = File.ReadAllBytes(path);
            Assert.That(bytes.Length, Is.GreaterThanOrEqualTo(26));
            Assert.That(bytes[12], Is.EqualTo((byte)'I'));
            Assert.That(bytes[13], Is.EqualTo((byte)'H'));
            Assert.That(bytes[14], Is.EqualTo((byte)'D'));
            Assert.That(bytes[15], Is.EqualTo((byte)'R'));
            Assert.That(BigEndianInt(bytes, 16), Is.EqualTo(width));
            Assert.That(BigEndianInt(bytes, 20), Is.EqualTo(height));
            Assert.That(bytes[25], Is.EqualTo(colorType), "PNG IHDR color type 6 is RGBA.");
        }

        static int BigEndianInt(byte[] bytes, int offset) =>
            bytes[offset] << 24 | bytes[offset + 1] << 16 | bytes[offset + 2] << 8 | bytes[offset + 3];

        static string Sha256(string path)
        {
            using (var hash = SHA256.Create())
                return string.Concat(hash.ComputeHash(File.ReadAllBytes(path)).Select(value => value.ToString("x2")));
        }
    }
}
