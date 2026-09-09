using System.Linq;
using NUnit.Framework;
using RealmRaiders.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class JumpActionIconPreviewTests
    {
        const string AssetPath = "Assets/Game/Resources/Art/UI/JumpAbility/jump-ability-icon-rgba-candidate.png";
        const string ProvenancePath = "Assets/Game/Resources/Art/UI/JumpAbility/provenance.json";

        [Test]
        public void ImportedPreviewDecoratesOnceWithoutOwningActionAndFailsBackToText()
        {
            var importer = AssetImporter.GetAtPath(AssetPath) as TextureImporter;
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.textureType, Is.EqualTo(TextureImporterType.Sprite));
            Assert.That(importer.spriteImportMode, Is.EqualTo(SpriteImportMode.Single));
            Assert.That(importer.alphaIsTransparency, Is.True);
            Assert.That(importer.mipmapEnabled, Is.False);
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
            Assert.That(importer.wrapMode, Is.EqualTo(TextureWrapMode.Clamp));
            Assert.That(importer.isReadable, Is.False);
            var android = importer.GetPlatformTextureSettings("Android");
            Assert.That(android.overridden, Is.True);
            Assert.That(android.maxTextureSize, Is.EqualTo(512));
            Assert.That(android.format, Is.EqualTo(TextureImporterFormat.ASTC_6x6));

            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(ProvenancePath);
            Assert.That(provenance, Is.Not.Null);
            Assert.That(provenance.text, Does.Contain("realmraiders.preview.jump-ability-icon.mui01.v1"));
            Assert.That(provenance.text, Does.Contain("preview-import-candidate-not-approved-for-runtime"));
            Assert.That(provenance.text, Does.Contain("genuine-transparent-background"));
            var expectedSprite = AssetDatabase.LoadAssetAtPath<Sprite>(AssetPath);
            Assert.That(expectedSprite, Is.Not.Null);
            Assert.That(Resources.Load<Sprite>(HudPresentation.JumpIconResource), Is.SameAs(expectedSprite));

            var hud = new GameObject("Jump Icon HUD");
            var fallbackHud = new GameObject("Fallback Jump Icon HUD");
            try
            {
                var presentation = hud.AddComponent<HudPresentation>();
                var clicks = 0;
                var button = CreateButton(hud.transform, () => clicks++);
                var label = button.GetComponentInChildren<Text>();
                var requests = 0;
                presentation.ConfigureJumpIconLoaderForTests(path =>
                {
                    requests++;
                    Assert.That(path, Is.EqualTo(HudPresentation.JumpIconResource));
                    return expectedSprite;
                });

                var icon = presentation.DecorateJumpButton(button);
                var repeated = presentation.DecorateJumpButton(button);
                Assert.That(icon, Is.Not.Null);
                Assert.That(repeated, Is.SameAs(icon));
                Assert.That(requests, Is.EqualTo(1));
                Assert.That(button.transform.Cast<Transform>().Count(child => child.name == HudPresentation.JumpIconName), Is.EqualTo(1));
                Assert.That(icon.transform.parent, Is.SameAs(button.transform));
                Assert.That(icon.sprite, Is.SameAs(expectedSprite));
                Assert.That(icon.raycastTarget, Is.False);
                Assert.That(icon.preserveAspect, Is.True);
                Assert.That(icon.GetComponent<Button>(), Is.Null);
                Assert.That(icon.GetComponent<EventTrigger>(), Is.Null);
                Assert.That(icon.GetComponentInChildren<Canvas>(), Is.Null);
                Assert.That(icon.GetComponentInChildren<EventSystem>(), Is.Null);
                Assert.That(icon.GetComponentInChildren<AudioListener>(), Is.Null);
                Assert.That(icon.material, Is.SameAs(icon.defaultMaterial));
                Assert.That(label.rectTransform.offsetMin.x, Is.GreaterThanOrEqualTo(58));
                Assert.That(label.resizeTextForBestFit, Is.True);
                foreach (var copy in new[] { "JUMP", "JUMP — ROOTED", "JUMP — AIRBORNE", "JUMP — BUSY" })
                {
                    label.text = copy;
                    Assert.That(label.text, Is.EqualTo(copy));
                }
                button.onClick.Invoke();
                Assert.That(clicks, Is.EqualTo(1), "The supplied Button retains action ownership.");

                var fallbackPresentation = fallbackHud.AddComponent<HudPresentation>();
                var fallbackClicks = 0;
                var fallbackButton = CreateButton(fallbackHud.transform, () => fallbackClicks++);
                var fallbackLabel = fallbackButton.GetComponentInChildren<Text>();
                fallbackPresentation.ConfigureJumpIconLoaderForTests(_ => throw new System.InvalidOperationException("Missing preview"));
                Assert.DoesNotThrow(() => fallbackPresentation.DecorateJumpButton(fallbackButton));
                Assert.That(fallbackButton.transform.Find(HudPresentation.JumpIconName), Is.Null);
                Assert.That(fallbackLabel.text, Is.EqualTo("JUMP"));
                Assert.That(fallbackLabel.rectTransform.offsetMin, Is.EqualTo(Vector2.zero));
                fallbackButton.onClick.Invoke();
                Assert.That(fallbackClicks, Is.EqualTo(1));
            }
            finally
            {
                Object.DestroyImmediate(hud);
                Object.DestroyImmediate(fallbackHud);
            }
        }

        static Button CreateButton(Transform parent, UnityEngine.Events.UnityAction action)
        {
            var buttonObject = new GameObject("JUMP", typeof(RectTransform), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);
            var button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(action);
            var labelObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            var label = labelObject.GetComponent<Text>();
            label.text = "JUMP";
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 25;
            label.alignment = TextAnchor.MiddleCenter;
            return button;
        }
    }
}
