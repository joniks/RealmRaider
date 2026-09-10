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
    public sealed class BloodKnightAbilityIconPreviewTests
    {
        const string Folder = "Assets/Game/Resources/Art/UI/BloodKnightAbilities";
        const string SlashAsset = Folder + "/basic-slash-rgba-candidate.png";
        const string RushAsset = Folder + "/blood-rush-rgba-candidate.png";
        const string CleaveAsset = Folder + "/heavy-cleave-rgba-candidate.png";
        const string ProvenanceAsset = Folder + "/provenance.json";
        const string SlashHash = "956033a1cad604db0bcb9ee5b463c37e4ca6a7c08758291ec843b5964136b5f2";
        const string RushHash = "775a583ae83e4617d9d145554903d2fcf3821b8e17955a3ba4f2484cd05e0d18";
        const string CleaveHash = "789fca8aa4e1ea065b0024af972ef9fd5f3a233bab07917707f4767ac49184d7";

        [Test]
        public void AcceptedAbilityIconsAreByteExactRgbaSingleSpritesWithExplicitMobileImport()
        {
            var assets = new[] { SlashAsset, RushAsset, CleaveAsset };
            CollectionAssert.AreEqual(assets.OrderBy(path => path).ToArray(), AssetDatabase.FindAssets("", new[] { Folder })
                .Select(AssetDatabase.GUIDToAssetPath).Where(path => path.EndsWith(".png")).OrderBy(path => path).ToArray(),
                "Evidence sheets must not enter the runtime resource folder.");
            Assert.That(Sha256(SlashAsset), Is.EqualTo(SlashHash));
            Assert.That(Sha256(RushAsset), Is.EqualTo(RushHash));
            Assert.That(Sha256(CleaveAsset), Is.EqualTo(CleaveHash));
            foreach (var asset in assets)
            {
                AssertPng(asset, 512, 512, 6);
                AssertImport(asset);
            }

            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(ProvenanceAsset);
            Assert.That(provenance, Is.Not.Null);
            Assert.That(provenance.text, Does.Contain("realmraiders.preview.blood-knight-abilities.mui03.v1"));
            Assert.That(provenance.text, Does.Contain("accepted-runtime-preview-not-final-art"));
            Assert.That(provenance.text, Does.Contain("\"thirdPartySources\": false"));
            Assert.That(provenance.text, Does.Contain("\"modulesCommit\": \"3fa21af\""));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI03-BloodKnightAbilityIcons/basic-slash-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI03-BloodKnightAbilityIcons/blood-rush-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI03-BloodKnightAbilityIcons/heavy-cleave-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain(SlashHash).And.Contain(RushHash).And.Contain(CleaveHash));
            Assert.That(provenance.text, Does.Contain("generationPrompts").And.Contain("thick tapered blade arc")
                .And.Contain("forward-driving armored motion wedge").And.Contain("broad, weighty iron crescent blade arc"));
        }

        [Test]
        public void StableMappingDecoratesEachButtonOnceWithoutChangingCopyOrCallbackOwnership()
        {
            var slashSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SlashAsset);
            var rushSprite = AssetDatabase.LoadAssetAtPath<Sprite>(RushAsset);
            var cleaveSprite = AssetDatabase.LoadAssetAtPath<Sprite>(CleaveAsset);
            Assert.That(HudPresentation.AbilityIconResourceFor(0, "SLASH"), Is.EqualTo(HudPresentation.BasicSlashIconResource));
            Assert.That(HudPresentation.AbilityIconResourceFor(1, "BLOOD RUSH"), Is.EqualTo(HudPresentation.BloodRushIconResource));
            Assert.That(HudPresentation.AbilityIconResourceFor(2, "CLEAVE"), Is.EqualTo(HudPresentation.HeavyCleaveIconResource));
            Assert.That(HudPresentation.AbilityIconResourceFor(0, "CLEAVE"), Is.Empty);
            Assert.That(HudPresentation.AbilityIconResourceFor(3, "SLASH"), Is.Empty);
            var root = new GameObject("Ability Icon Mapping");
            var requests = new List<string>();
            try
            {
                var presentation = root.AddComponent<HudPresentation>();
                presentation.ConfigureAbilityIconLoaderForTests(path =>
                {
                    requests.Add(path);
                    return path == HudPresentation.BasicSlashIconResource ? slashSprite :
                        path == HudPresentation.BloodRushIconResource ? rushSprite :
                        path == HudPresentation.HeavyCleaveIconResource ? cleaveSprite : null;
                });
                var clicks = new int[3];
                var buttons = new[]
                {
                    CreateButton(root.transform, "SLASH", () => clicks[0]++),
                    CreateButton(root.transform, "BLOOD RUSH", () => clicks[1]++),
                    CreateButton(root.transform, "CLEAVE", () => clicks[2]++)
                };
                var expected = new[] { slashSprite, rushSprite, cleaveSprite };
                for (var index = 0; index < buttons.Length; index++)
                {
                    var icon = presentation.DecorateAbilityButton(buttons[index], index, buttons[index].name);
                    var repeated = presentation.DecorateAbilityButton(buttons[index], index, buttons[index].name);
                    Assert.That(icon, Is.SameAs(repeated));
                    Assert.That(icon.sprite, Is.SameAs(expected[index]));
                    Assert.That(icon.raycastTarget, Is.False);
                    Assert.That(icon.preserveAspect, Is.True);
                    Assert.That(icon.rectTransform.sizeDelta, Is.EqualTo(new Vector2(44, 44)));
                    Assert.That(icon.rectTransform.anchoredPosition, Is.EqualTo(new Vector2(10, 0)));
                    Assert.That(icon.GetComponent<Button>(), Is.Null);
                    Assert.That(icon.GetComponent<EventTrigger>(), Is.Null);
                    Assert.That(icon.GetComponentInChildren<Canvas>(), Is.Null);
                    var label = buttons[index].GetComponentInChildren<Text>();
                    Assert.That(label.text, Is.EqualTo(buttons[index].name));
                    Assert.That(label.rectTransform.offsetMin.x, Is.GreaterThanOrEqualTo(58));
                    Assert.That(label.resizeTextForBestFit, Is.True);
                    buttons[index].onClick.Invoke();
                }
                CollectionAssert.AreEqual(new[] { 1, 1, 1 }, clicks);
                CollectionAssert.AreEqual(new[]
                {
                    HudPresentation.BasicSlashIconResource,
                    HudPresentation.BloodRushIconResource,
                    HudPresentation.HeavyCleaveIconResource
                }, requests);
                Assert.That(root.GetComponentsInChildren<Image>(true)
                    .Count(image => image.name.StartsWith(HudPresentation.AbilityIconNamePrefix)), Is.EqualTo(3));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingOrThrowingSpritesLeaveExactTextOnlyButtons(bool loaderThrows)
        {
            var root = new GameObject("Ability Icon Fallback");
            try
            {
                var presentation = root.AddComponent<HudPresentation>();
                var requests = 0;
                presentation.ConfigureAbilityIconLoaderForTests(_ =>
                {
                    requests++;
                    if (loaderThrows) throw new System.InvalidOperationException("Ability icon unavailable");
                    return null;
                });
                var clicks = 0;
                var button = CreateButton(root.transform, "SLASH", () => clicks++);
                var label = button.GetComponentInChildren<Text>();
                var originalMin = label.rectTransform.offsetMin;
                var originalMax = label.rectTransform.offsetMax;
                Assert.DoesNotThrow(() => presentation.DecorateAbilityButton(button, 0, "SLASH"));
                Assert.That(presentation.DecorateAbilityButton(button, 0, "SLASH"), Is.Null);
                Assert.That(requests, Is.EqualTo(1));
                Assert.That(button.transform.Find(HudPresentation.AbilityIconNamePrefix + "0"), Is.Null);
                Assert.That(label.text, Is.EqualTo("SLASH"));
                Assert.That(label.rectTransform.offsetMin, Is.EqualTo(originalMin));
                Assert.That(label.rectTransform.offsetMax, Is.EqualTo(originalMax));
                Assert.That(label.resizeTextForBestFit, Is.False);
                button.onClick.Invoke();
                Assert.That(clicks, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        static Button CreateButton(Transform parent, string copy, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject(copy, typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            ((RectTransform)buttonObject.transform).sizeDelta = new Vector2(240, 92);
            var button = buttonObject.GetComponent<Button>(); button.onClick.AddListener(action);
            var labelObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            var label = labelObject.GetComponent<Text>();
            label.text = copy;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 25;
            label.alignment = TextAnchor.MiddleCenter;
            return button;
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
