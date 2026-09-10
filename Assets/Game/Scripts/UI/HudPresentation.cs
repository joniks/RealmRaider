using UnityEngine;
using UnityEngine.UI;

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
        Sprite buttonSprite;
        Sprite jumpIconSprite;
        Sprite basicSlashIconSprite, bloodRushIconSprite, heavyCleaveIconSprite;
        AudioClip click, confirm, result;
        AudioSource source;
        bool resultPlayed;
        bool ownsButtonSprite;
        bool jumpIconResolved;
        bool basicSlashIconResolved, bloodRushIconResolved, heavyCleaveIconResolved;
        bool initialized;
        System.Func<string, Sprite> jumpIconLoader = LoadJumpIcon;
        System.Func<string, Sprite> abilityIconLoader = LoadAbilityIcon;

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
