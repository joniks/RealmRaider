using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using NUnit.Framework;
using RealmRaiders.Core;
using RealmRaiders.UI;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class InfernalBruteAbilityIconPreviewTests
    {
        const string Folder = "Assets/Game/Resources/Art/UI/InfernalBruteAbilities";
        const string SmashAsset = Folder + "/smash-rgba-candidate.png";
        const string ChargeAsset = Folder + "/charge-rgba-candidate.png";
        const string GroundSlamAsset = Folder + "/ground-slam-rgba-candidate.png";
        const string ProvenanceAsset = Folder + "/provenance.json";
        const string SmashHash = "0c456b2925f393dc3d4373ca196662189389486f60e5cde44cdcb2bfffc72f59";
        const string ChargeHash = "a77df3c218e9da413f1bb78beabe1815d422d3f75185ae6dae8dd86f6710074d";
        const string GroundSlamHash = "b43d89c7678a978157156d1b31b1f4e45f28c7f1711f3edab23c8d567cf08a2c";

        [Test]
        public void AcceptedInfernalBruteIconsAreByteExactRgbaSingleSpritesWithExplicitMobileImport()
        {
            var assets = new[] { SmashAsset, ChargeAsset, GroundSlamAsset };
            CollectionAssert.AreEqual(assets.OrderBy(path => path).ToArray(), AssetDatabase.FindAssets("", new[] { Folder })
                .Select(AssetDatabase.GUIDToAssetPath).Where(path => path.EndsWith(".png")).OrderBy(path => path).ToArray(),
                "Evidence sheets and non-Infernal icons must not enter the Infernal Brute runtime folder.");
            Assert.That(Sha256(SmashAsset), Is.EqualTo(SmashHash));
            Assert.That(Sha256(ChargeAsset), Is.EqualTo(ChargeHash));
            Assert.That(Sha256(GroundSlamAsset), Is.EqualTo(GroundSlamHash));
            foreach (var asset in assets)
            {
                AssertPng(asset, 512, 512, 6);
                AssertImport(asset);
            }

            var provenance = AssetDatabase.LoadAssetAtPath<TextAsset>(ProvenanceAsset);
            Assert.That(provenance, Is.Not.Null);
            Assert.That(provenance.text, Does.Contain("realmraiders.preview.infernal-brute-abilities.mui05.v1"));
            Assert.That(provenance.text, Does.Contain("accepted-runtime-preview-not-final-art"));
            Assert.That(provenance.text, Does.Contain("\"thirdPartySources\": false"));
            Assert.That(provenance.text, Does.Contain("\"modulesCommit\": \"a4c41c1\""));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI05-InfernalBruteAbilityIcons/smash-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI05-InfernalBruteAbilityIcons/charge-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain("Modules/RealmRaider.Modules/ArtPreviews/MUI05-InfernalBruteAbilityIcons/ground-slam-rgba-candidate.png"));
            Assert.That(provenance.text, Does.Contain(SmashHash).And.Contain(ChargeHash).And.Contain(GroundSlamHash));
            Assert.That(provenance.text, Does.Contain("generationPrompts").And.Contain("forged-obsidian fist/iron impact")
                .And.Contain("forward horizontal forged-obsidian ram/wedge").And.Contain("radial circular obsidian shockwave"));
        }

        [Test]
        public void StableInfernalIdentityMappingUsesSeparateCacheAndDecoratesOnlyTruthfulAffordances()
        {
            var smashSprite = AssetDatabase.LoadAssetAtPath<Sprite>(SmashAsset);
            var chargeSprite = AssetDatabase.LoadAssetAtPath<Sprite>(ChargeAsset);
            var slamSprite = AssetDatabase.LoadAssetAtPath<Sprite>(GroundSlamAsset);
            var guardianSmashSprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Game/Resources/Art/UI/GuardianEntAbilities/smash-rgba-candidate.png");
            Assert.That(smashSprite, Is.Not.Null);
            Assert.That(chargeSprite, Is.Not.Null);
            Assert.That(slamSprite, Is.Not.Null);
            Assert.That(guardianSmashSprite, Is.Not.Null);
            Assert.That(HudPresentation.InfernalBruteAbilityIconResourceFor(PrototypeCharacterRoster.InfernalBruteId, 0, "SMASH"), Is.EqualTo(HudPresentation.InfernalBruteSmashIconResource));
            Assert.That(HudPresentation.InfernalBruteAbilityIconResourceFor(PrototypeCharacterRoster.InfernalBruteId, 1, "CHARGE"), Is.EqualTo(HudPresentation.InfernalBruteChargeIconResource));
            Assert.That(HudPresentation.InfernalBruteAbilityIconResourceFor(PrototypeCharacterRoster.InfernalBruteId, 2, "GROUND SLAM"), Is.EqualTo(HudPresentation.InfernalBruteGroundSlamIconResource));
            Assert.That(HudPresentation.InfernalBruteAbilityIconResourceFor(PrototypeCharacterRoster.GuardianEntId, 0, "SMASH"), Is.Empty);
            Assert.That(HudPresentation.InfernalBruteAbilityIconResourceFor(PrototypeCharacterRoster.InfernalBruteId, 1, "GROUND SLAM"), Is.Empty);

            var root = new GameObject("Infernal Brute Icon Mapping");
            var requests = new List<string>();
            var guardianRequests = 0;
            try
            {
                var presentation = root.AddComponent<HudPresentation>();
                presentation.ConfigureInfernalBruteAbilityIconLoaderForTests(path =>
                {
                    requests.Add(path);
                    return path == HudPresentation.InfernalBruteSmashIconResource ? smashSprite :
                        path == HudPresentation.InfernalBruteChargeIconResource ? chargeSprite :
                        path == HudPresentation.InfernalBruteGroundSlamIconResource ? slamSprite : null;
                });
                presentation.ConfigureGuardianEntAbilityIconLoaderForTests(_ => { guardianRequests++; return guardianSmashSprite; });
                var clicks = new int[2];
                var smash = CreateButton(root.transform, "SMASH", () => clicks[0]++);
                var slam = CreateButton(root.transform, "GROUND SLAM", () => clicks[1]++);
                var charge = CreateAffordance(root.transform, out var chargeLabel);
                var icons = new[]
                {
                    presentation.DecorateInfernalBruteAbilityButton(smash, PrototypeCharacterRoster.InfernalBruteId, 0, "SMASH"),
                    presentation.DecorateInfernalBruteChargeAffordance(charge, chargeLabel, PrototypeCharacterRoster.InfernalBruteId),
                    presentation.DecorateInfernalBruteAbilityButton(slam, PrototypeCharacterRoster.InfernalBruteId, 2, "GROUND SLAM")
                };
                var expected = new[] { smashSprite, chargeSprite, slamSprite };
                for (var index = 0; index < icons.Length; index++)
                {
                    Assert.That(icons[index], Is.Not.Null);
                    Assert.That(icons[index].sprite, Is.SameAs(expected[index]));
                    Assert.That(icons[index].name, Does.StartWith(HudPresentation.InfernalBruteAbilityIconNamePrefix));
                    Assert.That(icons[index].name.StartsWith(HudPresentation.GuardianEntAbilityIconNamePrefix), Is.False);
                    Assert.That(icons[index].raycastTarget, Is.False);
                    Assert.That(icons[index].preserveAspect, Is.True);
                    Assert.That(icons[index].rectTransform.sizeDelta, Is.EqualTo(new Vector2(44, 44)));
                    Assert.That(icons[index].rectTransform.anchoredPosition, Is.EqualTo(new Vector2(10, 0)));
                    Assert.That(icons[index].GetComponent<Button>(), Is.Null);
                    Assert.That(icons[index].GetComponent<EventTrigger>(), Is.Null);
                }
                Assert.That(presentation.DecorateInfernalBruteAbilityButton(smash, PrototypeCharacterRoster.InfernalBruteId, 0, "SMASH"), Is.SameAs(icons[0]));
                Assert.That(presentation.DecorateInfernalBruteChargeAffordance(charge, chargeLabel, PrototypeCharacterRoster.InfernalBruteId), Is.SameAs(icons[1]));
                Assert.That(presentation.DecorateInfernalBruteAbilityButton(slam, PrototypeCharacterRoster.InfernalBruteId, 2, "GROUND SLAM"), Is.SameAs(icons[2]));

                var guardianCacheProbe = CreateButton(root.transform, "GUARDIAN CACHE PROBE", null);
                var guardianIcon = presentation.DecorateGuardianEntAbilityButton(guardianCacheProbe, PrototypeCharacterRoster.GuardianEntId, 0, "SMASH");
                Assert.That(guardianIcon.sprite, Is.SameAs(guardianSmashSprite));
                Assert.That(guardianIcon.name, Does.StartWith(HudPresentation.GuardianEntAbilityIconNamePrefix));
                Assert.That(guardianRequests, Is.EqualTo(1));

                var secondSmash = CreateButton(root.transform, "SECOND SMASH", null);
                Assert.That(presentation.DecorateInfernalBruteAbilityButton(secondSmash, PrototypeCharacterRoster.InfernalBruteId, 0, "SMASH").sprite, Is.SameAs(smashSprite));
                CollectionAssert.AreEqual(new[]
                {
                    HudPresentation.InfernalBruteSmashIconResource,
                    HudPresentation.InfernalBruteChargeIconResource,
                    HudPresentation.InfernalBruteGroundSlamIconResource
                }, requests);
                Assert.That(smash.GetComponentInChildren<Text>().text, Is.EqualTo("SMASH"));
                Assert.That(slam.GetComponentInChildren<Text>().text, Is.EqualTo("GROUND SLAM"));
                Assert.That(chargeLabel.text, Is.EqualTo("SWIPE: CHARGE"));
                Assert.That(chargeLabel.raycastTarget, Is.False);
                Assert.That(charge.GetComponent<Button>(), Is.Null);
                smash.onClick.Invoke(); slam.onClick.Invoke();
                CollectionAssert.AreEqual(new[] { 1, 1 }, clicks);

                var unknownButton = CreateButton(root.transform, "UNKNOWN SMASH", null);
                var unknownLabel = unknownButton.GetComponentInChildren<Text>();
                var unknownMin = unknownLabel.rectTransform.offsetMin;
                Assert.That(presentation.DecorateInfernalBruteAbilityButton(unknownButton, PrototypeCharacterRoster.SylvanWolfId, 0, "SMASH"), Is.Null);
                Assert.That(unknownLabel.rectTransform.offsetMin, Is.EqualTo(unknownMin));
                Assert.That(unknownLabel.resizeTextForBestFit, Is.False);
                Assert.That(requests, Has.Count.EqualTo(3), "An unknown identity must not load Infernal resources.");
            }
            finally
            {
                Object.DestroyImmediate(root);
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void MissingOrThrowingInfernalSpritesLeaveExactTextOnlyControls(bool loaderThrows)
        {
            var root = new GameObject("Infernal Brute Icon Fallback");
            try
            {
                var presentation = root.AddComponent<HudPresentation>();
                var requests = new List<string>();
                presentation.ConfigureInfernalBruteAbilityIconLoaderForTests(path =>
                {
                    requests.Add(path);
                    if (loaderThrows) throw new System.InvalidOperationException("Infernal Brute icon unavailable");
                    return null;
                });
                var clicks = new int[2];
                var smash = CreateButton(root.transform, "SMASH", () => clicks[0]++);
                var slam = CreateButton(root.transform, "GROUND SLAM", () => clicks[1]++);
                var charge = CreateAffordance(root.transform, out var chargeLabel);
                var labels = new[] { smash.GetComponentInChildren<Text>(), chargeLabel, slam.GetComponentInChildren<Text>() };
                var originalMins = labels.Select(label => label.rectTransform.offsetMin).ToArray();
                var originalMaxes = labels.Select(label => label.rectTransform.offsetMax).ToArray();

                Assert.DoesNotThrow(() => presentation.DecorateInfernalBruteAbilityButton(smash, PrototypeCharacterRoster.InfernalBruteId, 0, "SMASH"));
                Assert.DoesNotThrow(() => presentation.DecorateInfernalBruteChargeAffordance(charge, chargeLabel, PrototypeCharacterRoster.InfernalBruteId));
                Assert.DoesNotThrow(() => presentation.DecorateInfernalBruteAbilityButton(slam, PrototypeCharacterRoster.InfernalBruteId, 2, "GROUND SLAM"));
                Assert.That(presentation.DecorateInfernalBruteAbilityButton(smash, PrototypeCharacterRoster.InfernalBruteId, 0, "SMASH"), Is.Null);
                Assert.That(presentation.DecorateInfernalBruteChargeAffordance(charge, chargeLabel, PrototypeCharacterRoster.InfernalBruteId), Is.Null);
                Assert.That(presentation.DecorateInfernalBruteAbilityButton(slam, PrototypeCharacterRoster.InfernalBruteId, 2, "GROUND SLAM"), Is.Null);
                CollectionAssert.AreEqual(new[]
                {
                    HudPresentation.InfernalBruteSmashIconResource,
                    HudPresentation.InfernalBruteChargeIconResource,
                    HudPresentation.InfernalBruteGroundSlamIconResource
                }, requests);
                CollectionAssert.AreEqual(new[] { "SMASH", "SWIPE: CHARGE", "GROUND SLAM" }, labels.Select(label => label.text).ToArray());
                for (var index = 0; index < labels.Length; index++)
                {
                    Assert.That(labels[index].rectTransform.offsetMin, Is.EqualTo(originalMins[index]));
                    Assert.That(labels[index].rectTransform.offsetMax, Is.EqualTo(originalMaxes[index]));
                    Assert.That(labels[index].resizeTextForBestFit, Is.False);
                }
                Assert.That(root.GetComponentsInChildren<Image>(true)
                    .Any(image => image.name.StartsWith(HudPresentation.InfernalBruteAbilityIconNamePrefix)), Is.False);
                smash.onClick.Invoke(); slam.onClick.Invoke();
                CollectionAssert.AreEqual(new[] { 1, 1 }, clicks);
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
            ((RectTransform)buttonObject.transform).sizeDelta = new Vector2(340, 96);
            var button = buttonObject.GetComponent<Button>();
            if (action != null) button.onClick.AddListener(action);
            CreateLabel(buttonObject.transform, copy, 25, out _);
            return button;
        }

        static RectTransform CreateAffordance(Transform parent, out Text label)
        {
            var affordance = new GameObject("Infernal Brute Charge Affordance", typeof(RectTransform));
            affordance.transform.SetParent(parent, false);
            var rect = (RectTransform)affordance.transform;
            rect.sizeDelta = new Vector2(340, 64);
            CreateLabel(affordance.transform, "SWIPE: CHARGE", 23, out label);
            label.raycastTarget = false;
            return rect;
        }

        static void CreateLabel(Transform parent, string copy, int fontSize, out Text label)
        {
            var labelObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(parent, false);
            var labelRect = (RectTransform)labelObject.transform;
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = labelRect.offsetMax = Vector2.zero;
            label = labelObject.GetComponent<Text>();
            label.text = copy;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = fontSize;
            label.alignment = TextAnchor.MiddleCenter;
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
