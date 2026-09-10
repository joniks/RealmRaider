using UnityEngine;
using UnityEngine.UI;
using RealmRaiders.Core;

namespace RealmRaiders.UI
{
    /// <summary>Scene-local skin and restrained feedback for runtime-created HUDs.</summary>
    [DisallowMultipleComponent]
    public sealed class HudPresentation : MonoBehaviour
    {
        const string ResourceRoot = "ThirdParty/Kenney/InterfacePolish/";
        public const string JumpIconResource = "Art/UI/JumpAbility/jump-ability-icon-rgba-candidate";
        public const string JumpIconName = "Jump Action Icon Preview";
        public const string AbilityIconNamePrefix = "Blood Knight Ability Icon ";
        public const string BasicSlashIconResource = "Art/UI/BloodKnightAbilities/basic-slash-rgba-candidate";
        public const string BloodRushIconResource = "Art/UI/BloodKnightAbilities/blood-rush-rgba-candidate";
        public const string HeavyCleaveIconResource = "Art/UI/BloodKnightAbilities/heavy-cleave-rgba-candidate";
        public const string GuardianEntAbilityIconNamePrefix = "Guardian Ent Ability Icon ";
        public const string GuardianEntSmashIconResource = "Art/UI/GuardianEntAbilities/smash-rgba-candidate";
        public const string GuardianEntChargeIconResource = "Art/UI/GuardianEntAbilities/charge-rgba-candidate";
        public const string GuardianEntGroundSlamIconResource = "Art/UI/GuardianEntAbilities/ground-slam-rgba-candidate";
        public const string InfernalBruteAbilityIconNamePrefix = "Infernal Brute Ability Icon ";
        public const string InfernalBruteSmashIconResource = "Art/UI/InfernalBruteAbilities/smash-rgba-candidate";
        public const string InfernalBruteChargeIconResource = "Art/UI/InfernalBruteAbilities/charge-rgba-candidate";
        public const string InfernalBruteGroundSlamIconResource = "Art/UI/InfernalBruteAbilities/ground-slam-rgba-candidate";
        public const string SylvanRealmIdentity = "Sylvan";
        public const string InfernalRealmIdentity = "Infernal";
        public const string RealmIdentityIconName = "Realm Identity Mark";
        public const string SylvanRealmIdentityIconResource = "Art/UI/RealmIdentityIcons/sylvan-realm-rgba-candidate";
        public const string InfernalRealmIdentityIconResource = "Art/UI/RealmIdentityIcons/infernal-realm-rgba-candidate";
        public const string ControlStyleIconName = "Control Style Icon";
        public const string ContextualControlStyleIconResource = "Art/UI/ControlStyleIcons/contextual-rgba-candidate";
        public const string FingertapControlStyleIconResource = "Art/UI/ControlStyleIcons/fingertap-rgba-candidate";
        public const string JoystickControlStyleIconResource = "Art/UI/ControlStyleIcons/joystick-rgba-candidate";
        Sprite buttonSprite;
        Sprite jumpIconSprite;
        Sprite basicSlashIconSprite, bloodRushIconSprite, heavyCleaveIconSprite;
        Sprite guardianEntSmashIconSprite, guardianEntChargeIconSprite, guardianEntGroundSlamIconSprite;
        Sprite infernalBruteSmashIconSprite, infernalBruteChargeIconSprite, infernalBruteGroundSlamIconSprite;
        Sprite sylvanRealmIdentityIconSprite, infernalRealmIdentityIconSprite;
        Sprite contextualControlStyleIconSprite, fingertapControlStyleIconSprite, joystickControlStyleIconSprite;
        AudioClip click, confirm, result;
        AudioSource source;
        bool resultPlayed;
        bool ownsButtonSprite;
        bool jumpIconResolved;
        bool basicSlashIconResolved, bloodRushIconResolved, heavyCleaveIconResolved;
        bool guardianEntSmashIconResolved, guardianEntChargeIconResolved, guardianEntGroundSlamIconResolved;
        bool infernalBruteSmashIconResolved, infernalBruteChargeIconResolved, infernalBruteGroundSlamIconResolved;
        bool sylvanRealmIdentityIconResolved, infernalRealmIdentityIconResolved;
        bool contextualControlStyleIconResolved, fingertapControlStyleIconResolved, joystickControlStyleIconResolved;
        Text controlStyleLabel;
        Vector2 controlStyleLabelOffsetMin, controlStyleLabelOffsetMax;
        bool controlStyleLabelLayoutCaptured;
        bool initialized;
        System.Func<string, Sprite> jumpIconLoader = LoadJumpIcon;
        System.Func<string, Sprite> abilityIconLoader = LoadAbilityIcon;
        System.Func<string, Sprite> guardianEntAbilityIconLoader = LoadGuardianEntAbilityIcon;
        System.Func<string, Sprite> infernalBruteAbilityIconLoader = LoadInfernalBruteAbilityIcon;
        System.Func<string, Sprite> realmIdentityIconLoader = LoadRealmIdentityIcon;
        System.Func<string, Sprite> controlStyleIconLoader = LoadControlStyleIcon;

        public bool ResultCuePlayed => resultPlayed;

        void Awake() => EnsureInitialized();

        void EnsureInitialized()
        {
            if (initialized) return;
            initialized = true;
            buttonSprite = LoadButtonSprite();
            click = Resources.Load<AudioClip>(ResourceRoot + "Audio/click_001");
            confirm = Resources.Load<AudioClip>(ResourceRoot + "Audio/confirmation_003");
            result = Resources.Load<AudioClip>(ResourceRoot + "Audio/bong_001");
            source = GetComponent<AudioSource>();
            if (!source) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0;
            source.volume = .28f;
        }

        void OnDestroy()
        {
            if (ownsButtonSprite && buttonSprite) Destroy(buttonSprite);
        }

        Sprite LoadButtonSprite()
        {
            const string path = ResourceRoot + "Sprites/button_rectangle_depth_gradient";
            var sprite = Resources.Load<Sprite>(path);
            if (sprite) return sprite;

            var texture = Resources.Load<Texture2D>(path);
            if (!texture) return null;
            ownsButtonSprite = true;
            return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(.5f, .5f), 100, 0, SpriteMeshType.FullRect, new Vector4(6, 6, 6, 6));
        }

        public void ApplyButton(Image image)
        {
            EnsureInitialized();
            if (!image || !buttonSprite) return;
            image.sprite = buttonSprite;
            image.type = Image.Type.Sliced;
        }

        public Image DecorateRealmLabel(Text label, string realmIdentity)
        {
            if (!label) return null;
            var existing = label.transform.Find(RealmIdentityIconName);
            var resourcePath = RealmIdentityIconResourceFor(realmIdentity);
            if (string.IsNullOrEmpty(resourcePath))
            {
                if (existing) existing.gameObject.SetActive(false);
                return null;
            }

            var sprite = ResolveRealmIdentityIcon(realmIdentity, resourcePath);
            if (!sprite)
            {
                if (existing) existing.gameObject.SetActive(false);
                return null;
            }

            Image icon;
            if (existing)
            {
                icon = existing.GetComponent<Image>();
                if (!icon) return null;
            }
            else
            {
                var iconObject = new GameObject(RealmIdentityIconName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconObject.transform.SetParent(label.transform, false);
                icon = iconObject.GetComponent<Image>();
            }

            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(.5f, .5f);
            iconRect.pivot = new Vector2(0, .5f);
            iconRect.anchoredPosition = new Vector2(label.preferredWidth * .5f + 10, 0);
            iconRect.sizeDelta = new Vector2(48, 48);
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.gameObject.SetActive(true);
            return icon;
        }

        public static string RealmIdentityIconResourceFor(string realmIdentity) => realmIdentity switch
        {
            SylvanRealmIdentity => SylvanRealmIdentityIconResource,
            InfernalRealmIdentity => InfernalRealmIdentityIconResource,
            _ => string.Empty
        };

        public Image DecorateControlStyleButton(Button button, string savedStyle)
        {
            if (!button) return null;
            var label = button.GetComponentInChildren<Text>(true);
            CaptureControlStyleLabelLayout(label);
            var existing = button.transform.Find(ControlStyleIconName);
            var resourcePath = ControlStyleIconResourceFor(savedStyle);
            var sprite = string.IsNullOrEmpty(resourcePath) ? null : ResolveControlStyleIcon(savedStyle, resourcePath);
            if (!sprite)
            {
                if (existing) existing.gameObject.SetActive(false);
                RestoreControlStyleLabelLayout(label);
                return null;
            }

            Image icon;
            if (existing)
            {
                icon = existing.GetComponent<Image>();
                if (!icon) { existing.gameObject.SetActive(false); RestoreControlStyleLabelLayout(label); return null; }
            }
            else
            {
                var iconObject = new GameObject(ControlStyleIconName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
                iconObject.transform.SetParent(button.transform, false);
                icon = iconObject.GetComponent<Image>();
            }

            var iconRect = icon.rectTransform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0, .5f);
            iconRect.pivot = new Vector2(0, .5f);
            iconRect.anchoredPosition = new Vector2(12, 0);
            iconRect.sizeDelta = new Vector2(32, 32);
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.gameObject.SetActive(true);
            if (label)
            {
                var rect = label.rectTransform;
                rect.offsetMin = new Vector2(Mathf.Max(controlStyleLabelOffsetMin.x, 52), controlStyleLabelOffsetMin.y);
                rect.offsetMax = controlStyleLabelOffsetMax;
            }
            return icon;
        }

        public static string ControlStyleIconResourceFor(string savedStyle) => savedStyle switch
        {
            InRunControlStyleSelector.Contextual => ContextualControlStyleIconResource,
            InRunControlStyleSelector.Fingertap => FingertapControlStyleIconResource,
            InRunControlStyleSelector.Joystick => JoystickControlStyleIconResource,
            _ => string.Empty
        };

        void CaptureControlStyleLabelLayout(Text label)
        {
            if (!label || controlStyleLabelLayoutCaptured && controlStyleLabel == label) return;
            controlStyleLabel = label;
            controlStyleLabelOffsetMin = label.rectTransform.offsetMin;
            controlStyleLabelOffsetMax = label.rectTransform.offsetMax;
            controlStyleLabelLayoutCaptured = true;
        }

        void RestoreControlStyleLabelLayout(Text label)
        {
            if (!label || !controlStyleLabelLayoutCaptured || controlStyleLabel != label) return;
            label.rectTransform.offsetMin = controlStyleLabelOffsetMin;
            label.rectTransform.offsetMax = controlStyleLabelOffsetMax;
        }

        public Image DecorateJumpButton(Button button)
        {
            if (!button) return null;
            var existing = button.transform.Find(JumpIconName);
            if (existing) return existing.GetComponent<Image>();
            var sprite = ResolveJumpIcon();
            if (!sprite) return null;

            var iconObject = new GameObject(JumpIconName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(button.transform, false);
            var iconRect = (RectTransform)iconObject.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0, .5f);
            iconRect.pivot = new Vector2(0, .5f);
            iconRect.anchoredPosition = new Vector2(10, 0);
            iconRect.sizeDelta = new Vector2(44, 44);
            var icon = iconObject.GetComponent<Image>();
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var label = button.GetComponentInChildren<Text>(true);
            if (label)
            {
                var labelRect = label.rectTransform;
                labelRect.offsetMin = new Vector2(Mathf.Max(58, labelRect.offsetMin.x), labelRect.offsetMin.y);
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = Mathf.Min(18, label.fontSize);
                label.resizeTextMaxSize = Mathf.Max(label.fontSize, label.resizeTextMinSize);
            }
            return icon;
        }

        public Image DecorateAbilityButton(Button button, int abilityIndex, string semanticLabel)
        {
            if (!button) return null;
            var resourcePath = AbilityIconResourceFor(abilityIndex, semanticLabel);
            if (string.IsNullOrEmpty(resourcePath)) return null;
            var iconName = AbilityIconNamePrefix + abilityIndex;
            var existing = button.transform.Find(iconName);
            if (existing) return existing.GetComponent<Image>();
            var sprite = ResolveAbilityIcon(abilityIndex, resourcePath);
            if (!sprite) return null;

            var iconObject = new GameObject(iconName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(button.transform, false);
            var iconRect = (RectTransform)iconObject.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0, .5f);
            iconRect.pivot = new Vector2(0, .5f);
            iconRect.anchoredPosition = new Vector2(10, 0);
            iconRect.sizeDelta = new Vector2(44, 44);
            var icon = iconObject.GetComponent<Image>();
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            var label = button.GetComponentInChildren<Text>(true);
            if (label)
            {
                var labelRect = label.rectTransform;
                labelRect.offsetMin = new Vector2(Mathf.Max(58, labelRect.offsetMin.x), labelRect.offsetMin.y);
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = Mathf.Min(15, label.fontSize);
                label.resizeTextMaxSize = Mathf.Max(label.fontSize, label.resizeTextMinSize);
            }
            return icon;
        }

        public static string AbilityIconResourceFor(int abilityIndex, string semanticLabel) =>
            (abilityIndex, semanticLabel) switch
            {
                (0, "SLASH") => BasicSlashIconResource,
                (1, "BLOOD RUSH") => BloodRushIconResource,
                (2, "CLEAVE") => HeavyCleaveIconResource,
                _ => string.Empty
            };

        public Image DecorateGuardianEntAbilityButton(Button button, string archetypeId, int abilityIndex, string semanticName)
        {
            if (!button) return null;
            return DecorateGuardianEntAbilityIcon(button.transform, button.GetComponentInChildren<Text>(true), archetypeId, abilityIndex, semanticName);
        }

        public Image DecorateGuardianEntChargeAffordance(RectTransform affordance, Text label, string archetypeId)
        {
            if (!affordance || !label) return null;
            return DecorateGuardianEntAbilityIcon(affordance, label, archetypeId, 1, "CHARGE");
        }

        public static string GuardianEntAbilityIconResourceFor(string archetypeId, int abilityIndex, string semanticName)
        {
            if (archetypeId != PrototypeCharacterRoster.GuardianEntId) return string.Empty;
            return (abilityIndex, semanticName) switch
            {
                (0, "SMASH") => GuardianEntSmashIconResource,
                (1, "CHARGE") => GuardianEntChargeIconResource,
                (2, "GROUND SLAM") => GuardianEntGroundSlamIconResource,
                _ => string.Empty
            };
        }

        Image DecorateGuardianEntAbilityIcon(Transform host, Text label, string archetypeId, int abilityIndex, string semanticName)
        {
            if (!host) return null;
            var resourcePath = GuardianEntAbilityIconResourceFor(archetypeId, abilityIndex, semanticName);
            if (string.IsNullOrEmpty(resourcePath)) return null;
            var iconName = GuardianEntAbilityIconNamePrefix + abilityIndex;
            var existing = host.Find(iconName);
            if (existing) return existing.GetComponent<Image>();
            var sprite = ResolveGuardianEntAbilityIcon(abilityIndex, resourcePath);
            if (!sprite) return null;

            var iconObject = new GameObject(iconName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(host, false);
            var iconRect = (RectTransform)iconObject.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0, .5f);
            iconRect.pivot = new Vector2(0, .5f);
            iconRect.anchoredPosition = new Vector2(10, 0);
            iconRect.sizeDelta = new Vector2(44, 44);
            var icon = iconObject.GetComponent<Image>();
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            if (label)
            {
                var labelRect = label.rectTransform;
                labelRect.offsetMin = new Vector2(Mathf.Max(58, labelRect.offsetMin.x), labelRect.offsetMin.y);
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = Mathf.Min(15, label.fontSize);
                label.resizeTextMaxSize = Mathf.Max(label.fontSize, label.resizeTextMinSize);
            }
            return icon;
        }

        public Image DecorateInfernalBruteAbilityButton(Button button, string archetypeId, int abilityIndex, string semanticName)
        {
            if (!button) return null;
            return DecorateInfernalBruteAbilityIcon(button.transform, button.GetComponentInChildren<Text>(true), archetypeId, abilityIndex, semanticName);
        }

        public Image DecorateInfernalBruteChargeAffordance(RectTransform affordance, Text label, string archetypeId)
        {
            if (!affordance || !label) return null;
            return DecorateInfernalBruteAbilityIcon(affordance, label, archetypeId, 1, "CHARGE");
        }

        public static string InfernalBruteAbilityIconResourceFor(string archetypeId, int abilityIndex, string semanticName)
        {
            if (archetypeId != PrototypeCharacterRoster.InfernalBruteId) return string.Empty;
            return (abilityIndex, semanticName) switch
            {
                (0, "SMASH") => InfernalBruteSmashIconResource,
                (1, "CHARGE") => InfernalBruteChargeIconResource,
                (2, "GROUND SLAM") => InfernalBruteGroundSlamIconResource,
                _ => string.Empty
            };
        }

        Image DecorateInfernalBruteAbilityIcon(Transform host, Text label, string archetypeId, int abilityIndex, string semanticName)
        {
            if (!host) return null;
            var resourcePath = InfernalBruteAbilityIconResourceFor(archetypeId, abilityIndex, semanticName);
            if (string.IsNullOrEmpty(resourcePath)) return null;
            var iconName = InfernalBruteAbilityIconNamePrefix + abilityIndex;
            var existing = host.Find(iconName);
            if (existing) return existing.GetComponent<Image>();
            var sprite = ResolveInfernalBruteAbilityIcon(abilityIndex, resourcePath);
            if (!sprite) return null;

            var iconObject = new GameObject(iconName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(host, false);
            var iconRect = (RectTransform)iconObject.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(0, .5f);
            iconRect.pivot = new Vector2(0, .5f);
            iconRect.anchoredPosition = new Vector2(10, 0);
            iconRect.sizeDelta = new Vector2(44, 44);
            var icon = iconObject.GetComponent<Image>();
            icon.sprite = sprite;
            icon.preserveAspect = true;
            icon.raycastTarget = false;

            if (label)
            {
                var labelRect = label.rectTransform;
                labelRect.offsetMin = new Vector2(Mathf.Max(58, labelRect.offsetMin.x), labelRect.offsetMin.y);
                label.resizeTextForBestFit = true;
                label.resizeTextMinSize = Mathf.Min(15, label.fontSize);
                label.resizeTextMaxSize = Mathf.Max(label.fontSize, label.resizeTextMinSize);
            }
            return icon;
        }

        Sprite ResolveJumpIcon()
        {
            if (jumpIconResolved) return jumpIconSprite;
            jumpIconResolved = true;
            try { jumpIconSprite = jumpIconLoader?.Invoke(JumpIconResource); }
            catch (System.Exception) { jumpIconSprite = null; }
            return jumpIconSprite;
        }

        static Sprite LoadJumpIcon(string resourcePath) => Resources.Load<Sprite>(resourcePath);

        public void ConfigureJumpIconLoaderForTests(System.Func<string, Sprite> loader)
        {
            jumpIconLoader = loader ?? (_ => null);
            jumpIconSprite = null;
            jumpIconResolved = false;
        }

        Sprite ResolveAbilityIcon(int abilityIndex, string resourcePath) => abilityIndex switch
        {
            0 => ResolveSprite(ref basicSlashIconSprite, ref basicSlashIconResolved, resourcePath),
            1 => ResolveSprite(ref bloodRushIconSprite, ref bloodRushIconResolved, resourcePath),
            2 => ResolveSprite(ref heavyCleaveIconSprite, ref heavyCleaveIconResolved, resourcePath),
            _ => null
        };

        Sprite ResolveSprite(ref Sprite sprite, ref bool resolved, string resourcePath)
        {
            if (resolved) return sprite;
            resolved = true;
            try { sprite = abilityIconLoader?.Invoke(resourcePath); }
            catch (System.Exception) { sprite = null; }
            return sprite;
        }

        static Sprite LoadAbilityIcon(string resourcePath) => Resources.Load<Sprite>(resourcePath);

        public void ConfigureAbilityIconLoaderForTests(System.Func<string, Sprite> loader)
        {
            abilityIconLoader = loader ?? (_ => null);
            basicSlashIconSprite = bloodRushIconSprite = heavyCleaveIconSprite = null;
            basicSlashIconResolved = bloodRushIconResolved = heavyCleaveIconResolved = false;
        }

        Sprite ResolveGuardianEntAbilityIcon(int abilityIndex, string resourcePath) => abilityIndex switch
        {
            0 => ResolveGuardianEntSprite(ref guardianEntSmashIconSprite, ref guardianEntSmashIconResolved, resourcePath),
            1 => ResolveGuardianEntSprite(ref guardianEntChargeIconSprite, ref guardianEntChargeIconResolved, resourcePath),
            2 => ResolveGuardianEntSprite(ref guardianEntGroundSlamIconSprite, ref guardianEntGroundSlamIconResolved, resourcePath),
            _ => null
        };

        Sprite ResolveGuardianEntSprite(ref Sprite sprite, ref bool resolved, string resourcePath)
        {
            if (resolved) return sprite;
            resolved = true;
            try { sprite = guardianEntAbilityIconLoader?.Invoke(resourcePath); }
            catch (System.Exception) { sprite = null; }
            return sprite;
        }

        static Sprite LoadGuardianEntAbilityIcon(string resourcePath) => Resources.Load<Sprite>(resourcePath);

        public void ConfigureGuardianEntAbilityIconLoaderForTests(System.Func<string, Sprite> loader)
        {
            guardianEntAbilityIconLoader = loader ?? (_ => null);
            guardianEntSmashIconSprite = guardianEntChargeIconSprite = guardianEntGroundSlamIconSprite = null;
            guardianEntSmashIconResolved = guardianEntChargeIconResolved = guardianEntGroundSlamIconResolved = false;
        }

        Sprite ResolveInfernalBruteAbilityIcon(int abilityIndex, string resourcePath) => abilityIndex switch
        {
            0 => ResolveInfernalBruteSprite(ref infernalBruteSmashIconSprite, ref infernalBruteSmashIconResolved, resourcePath),
            1 => ResolveInfernalBruteSprite(ref infernalBruteChargeIconSprite, ref infernalBruteChargeIconResolved, resourcePath),
            2 => ResolveInfernalBruteSprite(ref infernalBruteGroundSlamIconSprite, ref infernalBruteGroundSlamIconResolved, resourcePath),
            _ => null
        };

        Sprite ResolveInfernalBruteSprite(ref Sprite sprite, ref bool resolved, string resourcePath)
        {
            if (resolved) return sprite;
            resolved = true;
            try { sprite = infernalBruteAbilityIconLoader?.Invoke(resourcePath); }
            catch (System.Exception) { sprite = null; }
            return sprite;
        }

        static Sprite LoadInfernalBruteAbilityIcon(string resourcePath) => Resources.Load<Sprite>(resourcePath);

        public void ConfigureInfernalBruteAbilityIconLoaderForTests(System.Func<string, Sprite> loader)
        {
            infernalBruteAbilityIconLoader = loader ?? (_ => null);
            infernalBruteSmashIconSprite = infernalBruteChargeIconSprite = infernalBruteGroundSlamIconSprite = null;
            infernalBruteSmashIconResolved = infernalBruteChargeIconResolved = infernalBruteGroundSlamIconResolved = false;
        }

        Sprite ResolveRealmIdentityIcon(string realmIdentity, string resourcePath) => realmIdentity switch
        {
            SylvanRealmIdentity => ResolveRealmIdentitySprite(ref sylvanRealmIdentityIconSprite, ref sylvanRealmIdentityIconResolved, resourcePath),
            InfernalRealmIdentity => ResolveRealmIdentitySprite(ref infernalRealmIdentityIconSprite, ref infernalRealmIdentityIconResolved, resourcePath),
            _ => null
        };

        Sprite ResolveRealmIdentitySprite(ref Sprite sprite, ref bool resolved, string resourcePath)
        {
            if (resolved) return sprite;
            resolved = true;
            try { sprite = realmIdentityIconLoader?.Invoke(resourcePath); }
            catch (System.Exception) { sprite = null; }
            return sprite;
        }

        static Sprite LoadRealmIdentityIcon(string resourcePath) => Resources.Load<Sprite>(resourcePath);

        public void ConfigureRealmIdentityIconLoaderForTests(System.Func<string, Sprite> loader)
        {
            realmIdentityIconLoader = loader ?? (_ => null);
            sylvanRealmIdentityIconSprite = infernalRealmIdentityIconSprite = null;
            sylvanRealmIdentityIconResolved = infernalRealmIdentityIconResolved = false;
        }

        Sprite ResolveControlStyleIcon(string savedStyle, string resourcePath) => savedStyle switch
        {
            InRunControlStyleSelector.Contextual => ResolveControlStyleSprite(ref contextualControlStyleIconSprite, ref contextualControlStyleIconResolved, resourcePath),
            InRunControlStyleSelector.Fingertap => ResolveControlStyleSprite(ref fingertapControlStyleIconSprite, ref fingertapControlStyleIconResolved, resourcePath),
            InRunControlStyleSelector.Joystick => ResolveControlStyleSprite(ref joystickControlStyleIconSprite, ref joystickControlStyleIconResolved, resourcePath),
            _ => null
        };

        Sprite ResolveControlStyleSprite(ref Sprite sprite, ref bool resolved, string resourcePath)
        {
            if (resolved) return sprite;
            resolved = true;
            try { sprite = controlStyleIconLoader?.Invoke(resourcePath); }
            catch (System.Exception) { sprite = null; }
            return sprite;
        }

        static Sprite LoadControlStyleIcon(string resourcePath) => Resources.Load<Sprite>(resourcePath);

        public void ConfigureControlStyleIconLoaderForTests(System.Func<string, Sprite> loader)
        {
            controlStyleIconLoader = loader ?? (_ => null);
            contextualControlStyleIconSprite = fingertapControlStyleIconSprite = joystickControlStyleIconSprite = null;
            contextualControlStyleIconResolved = fingertapControlStyleIconResolved = joystickControlStyleIconResolved = false;
        }

        public void PlayClick() { EnsureInitialized(); Play(click); }
        public void PlayConfirm() { EnsureInitialized(); Play(confirm); }
        public void PlayResult()
        {
            EnsureInitialized();
            if (resultPlayed) return;
            resultPlayed = true;
            Play(result);
        }

        void Play(AudioClip clip)
        {
            if (clip && source) source.PlayOneShot(clip);
        }
    }
}
