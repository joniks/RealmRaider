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
    public sealed class ControlStyleIconPreviewTests
    {
        const string Folder = "Assets/Game/Resources/Art/UI/ControlStyleIcons";
        const string ContextualAsset = Folder + "/contextual-rgba-candidate.png";
        const string FingertapAsset = Folder + "/fingertap-rgba-candidate.png";
        const string JoystickAsset = Folder + "/joystick-rgba-candidate.png";
        const string ProvenanceAsset = Folder + "/provenance.json";
        const string ContextualHash = "5eb4ce8861e123f6a12b5538e26210cbee664348016507f7766929963b5d6519";
        const string FingertapHash = "3cb7454bac3f4769dd29dce94f7f218524b87b85ad9b199d100407963a570a30";
        const string JoystickHash = "19a68cf9b103811291cc633c31baf7c6a8853e9f3bcbf5cc9bc505cfcedd485e";

        [Test]
        public void AcceptedControlStyleIconsAreByteExactRgbaSingleSpritesWithExplicitMobileImport()
        {
            var assets = new[] { ContextualAsset, FingertapAsset, JoystickAsset };
            CollectionAssert.AreEqual(assets.OrderBy(path => path).ToArray(), AssetDatabase.FindAssets("", new[] { Folder })
                .Select(AssetDatabase.GUIDToAssetPath).Where(path => path.EndsWith(".png")).OrderBy(path => path).ToArray(),
                "Contact and overview sheets must remain outside the runtime folder.");
            CollectionAssert.AreEqual(new[] { ContextualHash, FingertapHash, JoystickHash }, assets.Select(Sha256).ToArray());
            foreach (var asset in assets)
            {
                AssertPng(asset, 512, 512, 6);
                AssertImport(asset);
            }

            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(ProvenanceAsset);
            Assert.That(provenance, Is.Not.Null);
            Assert.That(provenance.text, Does.Contain("realmraiders.preview.control-styles.mui06.v1"));
            Assert.That(provenance.text, Does.Contain("accepted-runtime-preview-not-final-art"));
            Assert.That(provenance.text, Does.Contain("\"thirdPartySources\": false"));
            Assert.That(provenance.text, Does.Contain("\"modulesCommit\": \"04b8861\""));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI06-ControlStyleIcons/contextual-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI06-ControlStyleIcons/fingertap-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI06-ControlStyleIcons/joystick-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain(ContextualHash).And.Contain(FingertapHash).And.Contain(JoystickHash));
            Assert.That(provenance.text, Does.Contain("generationPrompts").And.Contain("automatic contextual choice without AI claim")
                .And.Contain("touch-point gesture").And.Contain("virtual stick"));
        }

        [Test]
        public void SavedStyleMappingCachesThreeSpritesAndReusesOneNoninteractiveMark()
        {
            Assert.That(HudPresentation.ControlStyleIconResourceFor(InRunControlStyleSelector.Contextual), Is.EqualTo(HudPresentation.ContextualControlStyleIconResource));
            Assert.That(HudPresentation.ControlStyleIconResourceFor(InRunControlStyleSelector.Fingertap), Is.EqualTo(HudPresentation.FingertapControlStyleIconResource));
            Assert.That(HudPresentation.ControlStyleIconResourceFor(InRunControlStyleSelector.Joystick), Is.EqualTo(HudPresentation.JoystickControlStyleIconResource));
            Assert.That(HudPresentation.ControlStyleIconResourceFor("contextual"), Is.Empty);
            Assert.That(HudPresentation.ControlStyleIconResourceFor("Unknown"), Is.Empty);

            var contextual = AssetDatabase.LoadAssetAtPath<Sprite>(ContextualAsset);
            var fingertap = AssetDatabase.LoadAssetAtPath<Sprite>(FingertapAsset);
            var joystick = AssetDatabase.LoadAssetAtPath<Sprite>(JoystickAsset);
            var root = new GameObject("Control Style Icon Mapping");
            var requests = new List<string>();
            try
            {
                var presentation = root.AddComponent<HudPresentation>();
                presentation.ConfigureControlStyleIconLoaderForTests(path =>
                {
                    requests.Add(path);
                    return path == HudPresentation.ContextualControlStyleIconResource ? contextual :
                        path == HudPresentation.FingertapControlStyleIconResource ? fingertap :
                        path == HudPresentation.JoystickControlStyleIconResource ? joystick : null;
                });
                var button = CreateButton(root.transform, InRunControlStyleSelector.AutoCopy);
                var label = button.GetComponentInChildren<Text>();
                var baselineMin = label.rectTransform.offsetMin;
                var baselineMax = label.rectTransform.offsetMax;

                var icon = presentation.DecorateControlStyleButton(button, InRunControlStyleSelector.Contextual);
                AssertIcon(icon, button, contextual);
                Assert.That(presentation.DecorateControlStyleButton(button, InRunControlStyleSelector.Contextual), Is.SameAs(icon));
                Assert.That(button.transform.Cast<Transform>().Count(child => child.name == HudPresentation.ControlStyleIconName), Is.EqualTo(1));
                Assert.That(requests, Is.EqualTo(new[] { HudPresentation.ContextualControlStyleIconResource }));
                Assert.That(label.text, Is.EqualTo(InRunControlStyleSelector.AutoCopy));
                Assert.That(label.rectTransform.offsetMin.x, Is.EqualTo(52));

                label.text = InRunControlStyleSelector.TapCopy;
                Assert.That(presentation.DecorateControlStyleButton(button, InRunControlStyleSelector.Fingertap), Is.SameAs(icon));
                AssertIcon(icon, button, fingertap);
                label.text = InRunControlStyleSelector.StickCopy;
                Assert.That(presentation.DecorateControlStyleButton(button, InRunControlStyleSelector.Joystick), Is.SameAs(icon));
                AssertIcon(icon, button, joystick);
                CollectionAssert.AreEqual(new[]
                {
                    HudPresentation.ContextualControlStyleIconResource,
                    HudPresentation.FingertapControlStyleIconResource,
                    HudPresentation.JoystickControlStyleIconResource
                }, requests);
                Assert.That(button.transform.Cast<Transform>().Count(child => child.name == HudPresentation.ControlStyleIconName), Is.EqualTo(1));

                Assert.That(presentation.DecorateControlStyleButton(button, "Unknown"), Is.Null);
                Assert.That(icon.gameObject.activeSelf, Is.False);
                Assert.That(label.text, Is.EqualTo(InRunControlStyleSelector.StickCopy));
                Assert.That(label.rectTransform.offsetMin, Is.EqualTo(baselineMin));
                Assert.That(label.rectTransform.offsetMax, Is.EqualTo(baselineMax));
                Assert.That(presentation.DecorateControlStyleButton(button, InRunControlStyleSelector.Contextual), Is.SameAs(icon));
                Assert.That(requests, Has.Count.EqualTo(3), "Returning to a resolved style must reuse its cached sprite.");
            }
            finally { Object.DestroyImmediate(root); }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingOrThrowingReplacementHidesStaleMarkAndRestoresExactTextOnlyLayout(bool loaderThrows)
        {
            var contextual = AssetDatabase.LoadAssetAtPath<Sprite>(ContextualAsset);
            var root = new GameObject("Control Style Icon Fallback");
            var cleanRoot = new GameObject("Control Style Icon Clean Fallback");
            try
            {
                var cleanPresentation = cleanRoot.AddComponent<HudPresentation>();
                var cleanButton = CreateButton(cleanRoot.transform, InRunControlStyleSelector.StickCopy);
                var cleanLabel = cleanButton.GetComponentInChildren<Text>();
                var cleanBaseline = TextLayoutSnapshot.Capture(cleanLabel);
                var cleanRequests = 0;
                cleanPresentation.ConfigureControlStyleIconLoaderForTests(_ =>
                {
                    cleanRequests++;
                    if (loaderThrows) throw new System.InvalidOperationException("Initial control style icon unavailable");
                    return null;
                });
                Assert.DoesNotThrow(() => cleanPresentation.DecorateControlStyleButton(cleanButton, InRunControlStyleSelector.Joystick));
                Assert.That(cleanPresentation.DecorateControlStyleButton(cleanButton, InRunControlStyleSelector.Joystick), Is.Null);
                Assert.That(cleanRequests, Is.EqualTo(1), "An initial failed resource lookup must be cached.");
                Assert.That(cleanButton.transform.Find(HudPresentation.ControlStyleIconName), Is.Null);
                cleanBaseline.AssertLayout(cleanLabel, InRunControlStyleSelector.StickCopy);

                var presentation = root.AddComponent<HudPresentation>();
                var button = CreateButton(root.transform, InRunControlStyleSelector.AutoCopy);
                var label = button.GetComponentInChildren<Text>();
                var baseline = TextLayoutSnapshot.Capture(label);
                var requests = 0;
                presentation.ConfigureControlStyleIconLoaderForTests(path =>
                {
                    requests++;
                    if (path == HudPresentation.ContextualControlStyleIconResource) return contextual;
                    if (loaderThrows) throw new System.InvalidOperationException("Control style icon unavailable");
                    return null;
                });

                var stale = presentation.DecorateControlStyleButton(button, InRunControlStyleSelector.Contextual);
                Assert.That(stale, Is.Not.Null);
                label.text = InRunControlStyleSelector.TapCopy;
                Assert.DoesNotThrow(() => presentation.DecorateControlStyleButton(button, InRunControlStyleSelector.Fingertap));
                Assert.That(presentation.DecorateControlStyleButton(button, InRunControlStyleSelector.Fingertap), Is.Null);
                Assert.That(requests, Is.EqualTo(2), "A failed resource lookup must be cached for that style.");
                Assert.That(stale.gameObject.activeSelf, Is.False);
                Assert.That(button.transform.Cast<Transform>().Count(child => child.name == HudPresentation.ControlStyleIconName), Is.EqualTo(1));
                baseline.AssertLayout(label, InRunControlStyleSelector.TapCopy);
                Assert.That(button.GetComponent<UiPointerOwnership>(), Is.Null);
            }
            finally { Object.DestroyImmediate(root); Object.DestroyImmediate(cleanRoot); }
        }

        static Button CreateButton(Transform parent, string copy)
        {
            var buttonObject = new GameObject("Control Style", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var labelObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var rect = (RectTransform)labelObject.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var label = labelObject.GetComponent<Text>();
            label.text = copy;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 20;
            label.alignment = TextAnchor.MiddleCenter;
            label.raycastTarget = false;
            return buttonObject.GetComponent<Button>();
        }

        static void AssertIcon(Image icon, Button button, Sprite expected)
        {
            Assert.That(icon, Is.Not.Null);
            Assert.That(icon.name, Is.EqualTo(HudPresentation.ControlStyleIconName));
            Assert.That(icon.transform.parent, Is.SameAs(button.transform));
            Assert.That(icon.sprite, Is.SameAs(expected));
            Assert.That(icon.rectTransform.sizeDelta, Is.EqualTo(new Vector2(32, 32)));
            Assert.That(icon.rectTransform.anchoredPosition, Is.EqualTo(new Vector2(12, 0)));
            Assert.That(icon.preserveAspect, Is.True);
            Assert.That(icon.raycastTarget, Is.False);
            Assert.That(icon.gameObject.activeSelf, Is.True);
            Assert.That(icon.GetComponent<Button>(), Is.Null);
            Assert.That(icon.GetComponent<EventTrigger>(), Is.Null);
            Assert.That(icon.GetComponent<UiPointerOwnership>(), Is.Null);
            Assert.That(icon.GetComponentInChildren<Canvas>(), Is.Null);
            Assert.That(icon.GetComponentInChildren<EventSystem>(), Is.Null);
            Assert.That(icon.GetComponentInChildren<AudioListener>(), Is.Null);
        }

        readonly struct TextLayoutSnapshot
        {
            readonly Vector2 anchorMin, anchorMax, offsetMin, offsetMax, pivot;
            readonly TextAnchor alignment;

            TextLayoutSnapshot(Text label)
            {
                var rect = label.rectTransform;
                anchorMin = rect.anchorMin;
                anchorMax = rect.anchorMax;
                offsetMin = rect.offsetMin;
                offsetMax = rect.offsetMax;
                pivot = rect.pivot;
                alignment = label.alignment;
            }

            public static TextLayoutSnapshot Capture(Text label) => new(label);

            public void AssertLayout(Text label, string expectedCopy)
            {
                Assert.That(label.text, Is.EqualTo(expectedCopy));
                Assert.That(label.rectTransform.anchorMin, Is.EqualTo(anchorMin));
                Assert.That(label.rectTransform.anchorMax, Is.EqualTo(anchorMax));
                Assert.That(label.rectTransform.offsetMin, Is.EqualTo(offsetMin));
                Assert.That(label.rectTransform.offsetMax, Is.EqualTo(offsetMax));
                Assert.That(label.rectTransform.pivot, Is.EqualTo(pivot));
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
