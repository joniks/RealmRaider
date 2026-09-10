using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using RealmRaiders.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class RealmIdentityIconPreviewTests
    {
        const string Folder = "Assets/Game/Resources/Art/UI/RealmIdentityIcons";
        const string SylvanAsset = Folder + "/sylvan-realm-rgba-candidate.png";
        const string InfernalAsset = Folder + "/infernal-realm-rgba-candidate.png";
        const string ProvenanceAsset = Folder + "/provenance.json";
        const string SylvanHash = "abc195dc86c4502d8a88feb9cf92d87eb37efdac5550d9ed1a5854afddeffd77";
        const string InfernalHash = "6350b291e050d9e7d6de20f2ec19071343d67084de99c955ccd6045503f6eb39";

        [Test]
        public void AcceptedRealmIdentityIconsAreByteExactRgbaSingleSpritesWithExplicitMobileImport()
        {
            var assets = new[] { SylvanAsset, InfernalAsset };
            CollectionAssert.AreEqual(assets.OrderBy(path => path).ToArray(), AssetDatabase.FindAssets("", new[] { Folder })
                .Select(AssetDatabase.GUIDToAssetPath).Where(path => path.EndsWith(".png")).OrderBy(path => path).ToArray(),
                "Contact and overview sheets must remain outside the runtime folder.");
            Assert.That(Sha256(SylvanAsset), Is.EqualTo(SylvanHash));
            Assert.That(Sha256(InfernalAsset), Is.EqualTo(InfernalHash));
            foreach (var asset in assets)
            {
                AssertPng(asset, 512, 512, 6);
                AssertImport(asset);
            }

            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(ProvenanceAsset);
            Assert.That(provenance, Is.Not.Null);
            Assert.That(provenance.text, Does.Contain("realmraiders.preview.realm-identities.mui07.v1"));
            Assert.That(provenance.text, Does.Contain("accepted-runtime-preview-not-final-art"));
            Assert.That(provenance.text, Does.Contain("\"thirdPartySources\": false"));
            Assert.That(provenance.text, Does.Contain("\"modulesCommit\": \"04b8861\""));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI07-RealmIdentityIcons/sylvan-realm-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI07-RealmIdentityIcons/infernal-realm-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain(SylvanHash).And.Contain(InfernalHash));
            Assert.That(provenance.text, Does.Contain("generationPrompts").And.Contain("living canopy/root/seed continuity mark")
                .And.Contain("forged obsidian gate continuity mark"));
            Assert.That(provenance.text, Does.Contain("Sylvan 32px fine root-and-leaf detail"));
        }

        [Test]
        public void StableRealmIdentityMappingCachesFamiliesAndReusesOneNoninteractiveMark()
        {
            var sylvanSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SylvanAsset);
            var infernalSprite = AssetDatabase.LoadAssetAtPath<Sprite>(InfernalAsset);
            Assert.That(sylvanSprite, Is.Not.Null);
            Assert.That(infernalSprite, Is.Not.Null);
            Assert.That(HudPresentation.RealmIdentityIconResourceFor(HudPresentation.SylvanRealmIdentity), Is.EqualTo(HudPresentation.SylvanRealmIdentityIconResource));
            Assert.That(HudPresentation.RealmIdentityIconResourceFor(HudPresentation.InfernalRealmIdentity), Is.EqualTo(HudPresentation.InfernalRealmIdentityIconResource));
            Assert.That(HudPresentation.RealmIdentityIconResourceFor("sylvan"), Is.Empty);
            Assert.That(HudPresentation.RealmIdentityIconResourceFor("Unknown"), Is.Empty);

            var root = new GameObject("Realm Identity Mapping");
            var requests = new List<string>();
            try
            {
                var presentation = root.AddComponent<HudPresentation>();
                presentation.ConfigureRealmIdentityIconLoaderForTests(path =>
                {
                    requests.Add(path);
                    return path == HudPresentation.SylvanRealmIdentityIconResource ? sylvanSprite :
                        path == HudPresentation.InfernalRealmIdentityIconResource ? infernalSprite : null;
                });
                var label = CreateLabel(root.transform, "Selected realm: Sylvan");
                var snapshot = TextSnapshot.Capture(label);

                var mark = presentation.DecorateRealmLabel(label, HudPresentation.SylvanRealmIdentity);
                AssertMark(mark, label, sylvanSprite);
                Assert.That(presentation.DecorateRealmLabel(label, HudPresentation.SylvanRealmIdentity), Is.SameAs(mark));
                Assert.That(label.GetComponentsInChildren<Image>(true), Has.Length.EqualTo(1));
                snapshot.AssertUnchanged(label);

                Assert.That(presentation.DecorateRealmLabel(label, HudPresentation.InfernalRealmIdentity), Is.SameAs(mark));
                AssertMark(mark, label, infernalSprite);
                CollectionAssert.AreEqual(new[]
                {
                    HudPresentation.SylvanRealmIdentityIconResource,
                    HudPresentation.InfernalRealmIdentityIconResource
                }, requests);

                var secondLabel = CreateLabel(root.transform, "SYLVAN BUILD");
                var secondMark = presentation.DecorateRealmLabel(secondLabel, HudPresentation.SylvanRealmIdentity);
                AssertMark(secondMark, secondLabel, sylvanSprite);
                Assert.That(requests, Has.Count.EqualTo(2), "Each explicit realm sprite must be cached once per presentation.");

                var abilityRequests = 0;
                presentation.ConfigureAbilityIconLoaderForTests(_ => { abilityRequests++; return sylvanSprite; });
                var abilityButton = CreateButton(root.transform, "SLASH");
                var abilityIcon = presentation.DecorateAbilityButton(abilityButton, 0, "SLASH");
                Assert.That(abilityIcon, Is.Not.Null);
                Assert.That(abilityIcon.name, Does.StartWith(HudPresentation.AbilityIconNamePrefix));
                Assert.That(abilityIcon.name, Is.Not.EqualTo(HudPresentation.RealmIdentityIconName));
                Assert.That(abilityRequests, Is.EqualTo(1));
                Assert.That(requests, Has.Count.EqualTo(2), "Ability decoration must not touch the realm identity cache.");

                Assert.That(presentation.DecorateRealmLabel(label, "Unknown"), Is.Null);
                Assert.That(mark.gameObject.activeSelf, Is.False, "Unknown identity must hide a stale known-realm mark.");
                snapshot.AssertUnchanged(label);
                Assert.That(presentation.DecorateRealmLabel(label, HudPresentation.SylvanRealmIdentity), Is.SameAs(mark));
                Assert.That(mark.gameObject.activeSelf, Is.True);
                Assert.That(mark.sprite, Is.SameAs(sylvanSprite));
                Assert.That(requests, Has.Count.EqualTo(2));

                var unknownLabel = CreateLabel(root.transform, "Unknown realm");
                var unknownSnapshot = TextSnapshot.Capture(unknownLabel);
                Assert.That(presentation.DecorateRealmLabel(unknownLabel, "Unknown"), Is.Null);
                Assert.That(unknownLabel.GetComponentsInChildren<Image>(true), Is.Empty);
                unknownSnapshot.AssertUnchanged(unknownLabel);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingOrThrowingRealmIdentitySpriteLeavesExactTextOnlyLayout(bool loaderThrows)
        {
            var root = new GameObject("Realm Identity Fallback");
            try
            {
                var presentation = root.AddComponent<HudPresentation>();
                var requests = 0;
                presentation.ConfigureRealmIdentityIconLoaderForTests(_ =>
                {
                    requests++;
                    if (loaderThrows) throw new System.InvalidOperationException("Realm identity icon unavailable");
                    return null;
                });
                var label = CreateLabel(root.transform, "SYLVAN RAID");
                var snapshot = TextSnapshot.Capture(label);
                Assert.DoesNotThrow(() => presentation.DecorateRealmLabel(label, HudPresentation.SylvanRealmIdentity));
                Assert.That(presentation.DecorateRealmLabel(label, HudPresentation.SylvanRealmIdentity), Is.Null);
                Assert.That(requests, Is.EqualTo(1), "A failed realm resource lookup must also be cached.");
                Assert.That(label.GetComponentsInChildren<Image>(true), Is.Empty);
                snapshot.AssertUnchanged(label);

                var sylvanSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SylvanAsset);
                Assert.That(sylvanSprite, Is.Not.Null);
                presentation.ConfigureRealmIdentityIconLoaderForTests(path =>
                {
                    if (path == HudPresentation.SylvanRealmIdentityIconResource) return sylvanSprite;
                    if (loaderThrows) throw new System.InvalidOperationException("Replacement realm identity icon unavailable");
                    return null;
                });
                var switchingLabel = CreateLabel(root.transform, "Selected realm: Sylvan");
                var switchingSnapshot = TextSnapshot.Capture(switchingLabel);
                var staleMark = presentation.DecorateRealmLabel(switchingLabel, HudPresentation.SylvanRealmIdentity);
                Assert.That(staleMark, Is.Not.Null);
                Assert.DoesNotThrow(() => presentation.DecorateRealmLabel(switchingLabel, HudPresentation.InfernalRealmIdentity));
                Assert.That(presentation.DecorateRealmLabel(switchingLabel, HudPresentation.InfernalRealmIdentity), Is.Null);
                Assert.That(staleMark.gameObject.activeSelf, Is.False, "A failed replacement must hide the previous realm's mark.");
                Assert.That(switchingLabel.GetComponentsInChildren<Image>(true), Has.Length.EqualTo(1));
                switchingSnapshot.AssertUnchanged(switchingLabel);
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static Text CreateLabel(Transform parent, string copy)
        {
            var labelObject = new GameObject("Realm Label", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            var rect = (RectTransform)labelObject.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1);
            rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = new Vector2(17, -43);
            rect.sizeDelta = new Vector2(950, 90);
            var label = labelObject.GetComponent<Text>();
            label.text = copy;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 38;
            label.alignment = TextAnchor.UpperCenter;
            return label;
        }

        static Button CreateButton(Transform parent, string copy)
        {
            var buttonObject = new GameObject(copy, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var button = buttonObject.GetComponent<Button>();
            var label = CreateLabel(buttonObject.transform, copy);
            label.alignment = TextAnchor.MiddleCenter;
            return button;
        }

        static void AssertMark(Image mark, Text label, Sprite expected)
        {
            Assert.That(mark, Is.Not.Null);
            Assert.That(mark.name, Is.EqualTo(HudPresentation.RealmIdentityIconName));
            Assert.That(mark.transform.parent, Is.SameAs(label.transform));
            Assert.That(mark.sprite, Is.SameAs(expected));
            Assert.That(mark.raycastTarget, Is.False);
            Assert.That(mark.preserveAspect, Is.True);
            Assert.That(mark.rectTransform.anchorMin, Is.EqualTo(new Vector2(.5f, .5f)));
            Assert.That(mark.rectTransform.anchorMax, Is.EqualTo(new Vector2(.5f, .5f)));
            Assert.That(mark.rectTransform.pivot, Is.EqualTo(new Vector2(0, .5f)));
            Assert.That(mark.rectTransform.anchoredPosition, Is.EqualTo(new Vector2(label.preferredWidth * .5f + 10, 0)));
            Assert.That(mark.rectTransform.sizeDelta, Is.EqualTo(new Vector2(48, 48)));
            Assert.That(mark.GetComponent<Button>(), Is.Null);
            Assert.That(mark.GetComponent<EventTrigger>(), Is.Null);
            Assert.That(mark.GetComponent<UiPointerOwnership>(), Is.Null);
        }

        readonly struct TextSnapshot
        {
            readonly string copy;
            readonly Vector2 anchorMin, anchorMax, pivot, position, size;
            readonly TextAnchor alignment;

            TextSnapshot(Text label)
            {
                var rect = label.rectTransform;
                copy = label.text;
                anchorMin = rect.anchorMin;
                anchorMax = rect.anchorMax;
                pivot = rect.pivot;
                position = rect.anchoredPosition;
                size = rect.sizeDelta;
                alignment = label.alignment;
            }

            public static TextSnapshot Capture(Text label) => new(label);

            public void AssertUnchanged(Text label)
            {
                Assert.That(label.text, Is.EqualTo(copy));
                Assert.That(label.rectTransform.anchorMin, Is.EqualTo(anchorMin));
                Assert.That(label.rectTransform.anchorMax, Is.EqualTo(anchorMax));
                Assert.That(label.rectTransform.pivot, Is.EqualTo(pivot));
                Assert.That(label.rectTransform.anchoredPosition, Is.EqualTo(position));
                Assert.That(label.rectTransform.sizeDelta, Is.EqualTo(size));
                Assert.That(label.alignment, Is.EqualTo(alignment));
            }
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
