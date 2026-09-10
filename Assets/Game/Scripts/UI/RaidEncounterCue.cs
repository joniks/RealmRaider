using System;
using RealmRaiders.Controllers;
using RealmRaiders.Raid;
using UnityEngine;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    [DisallowMultipleComponent]
    public sealed class RaidEncounterCue : MonoBehaviour
    {
        public const string LabelName = "Raid Encounter Cue";
        public const string IconName = "Raid Encounter Cue Icon";
        public const string DiscoveredIconResource = "Art/UI/EncounterCue/discovered-rgba-candidate";
        public const string HostilesIconResource = "Art/UI/EncounterCue/hostiles-rgba-candidate";
        public const string ClearedIconResource = "Art/UI/EncounterCue/area-clear-rgba-candidate";
        public const float DiscoveryDuration = 1.8f;
        public const float HostileCountDuration = 2.4f;
        public const float ClearDuration = 2f;

        ResponsiveHudRoot responsive;
        RectTransform labelRect;
        Text label;
        RectTransform iconRect;
        Image icon;
        float hideAt;
        RaidEncounterState current = RaidEncounterState.Hidden;

        static Sprite discoveredIcon;
        static Sprite hostilesIcon;
        static Sprite clearedIcon;
        static bool discoveredIconResolved;
        static bool hostilesIconResolved;
        static bool clearedIconResolved;
        static Func<string, Sprite> spriteLoader = LoadSprite;

        public bool Visible => label && label.gameObject.activeSelf;
        public string Text => Visible ? label.text : string.Empty;
        public bool RaycastTarget => label && label.raycastTarget;
        public RectTransform Rect => labelRect;
        public bool IconVisible => icon && icon.gameObject.activeSelf;
        public Sprite IconSprite => icon ? icon.sprite : null;
        public bool IconRaycastTarget => icon && icon.raycastTarget;
        public RectTransform IconRect => iconRect;
        public RaidEncounterState Current => current;

        public void Initialize(ResponsiveHudRoot responsiveRoot)
        {
            if (responsive) responsive.LayoutChanged -= ApplyLayout;
            responsive = responsiveRoot;
            if (!label) CreateLabel();
            if (!icon) CreateIcon();
            if (responsive)
            {
                responsive.LayoutChanged -= ApplyLayout;
                responsive.LayoutChanged += ApplyLayout;
                ApplyLayout(responsive.Orientation);
            }
            Clear();
        }

        public void Show(RaidEncounterState state)
        {
            if (!label || !state.Visible || GameplayInput.TerminalState)
            {
                Clear();
                return;
            }
            var copy = CopyFor(state);
            if (string.IsNullOrEmpty(copy))
            {
                Clear();
                return;
            }
            current = state;
            label.text = copy;
            label.gameObject.SetActive(true);
            var sprite = SpriteFor(state.Phase);
            icon.sprite = sprite;
            icon.gameObject.SetActive(sprite);
            hideAt = Time.unscaledTime + DurationFor(state.Phase);
        }

        public void Clear()
        {
            current = RaidEncounterState.Hidden;
            hideAt = 0;
            if (label)
            {
                label.text = string.Empty;
                label.gameObject.SetActive(false);
            }
            if (icon)
            {
                icon.sprite = null;
                icon.gameObject.SetActive(false);
            }
        }

        public static string CopyFor(RaidEncounterState state)
        {
            if (!state.Visible || string.IsNullOrWhiteSpace(state.NodeId)) return string.Empty;
            var node = state.NodeId.Trim().ToUpperInvariant();
            return state.Phase switch
            {
                RaidEncounterPhase.Discovered => $"{node} DISCOVERED",
                RaidEncounterPhase.Hostiles => $"{node} • {state.RemainingHostiles} {(state.RemainingHostiles == 1 ? "HOSTILE" : "HOSTILES")}",
                RaidEncounterPhase.Cleared => $"{node} • AREA CLEAR",
                _ => string.Empty
            };
        }

        public static string IconResourceFor(RaidEncounterPhase phase) => phase switch
        {
            RaidEncounterPhase.Discovered => DiscoveredIconResource,
            RaidEncounterPhase.Hostiles => HostilesIconResource,
            RaidEncounterPhase.Cleared => ClearedIconResource,
            _ => string.Empty
        };

        void Update()
        {
            if (!Visible) return;
            if (GameplayInput.TerminalState || Time.unscaledTime >= hideAt) Clear();
        }

        void CreateLabel()
        {
            var labelObject = new GameObject(LabelName, typeof(RectTransform), typeof(Text));
            labelObject.transform.SetParent(transform, false);
            labelRect = (RectTransform)labelObject.transform;
            label = labelObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontStyle = FontStyle.Bold;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(.82f, 1f, .72f);
            label.raycastTarget = false;
            label.gameObject.SetActive(false);
        }

        void CreateIcon()
        {
            var iconObject = new GameObject(IconName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.transform.SetParent(transform, false);
            iconRect = (RectTransform)iconObject.transform;
            iconRect.anchorMin = iconRect.anchorMax = new Vector2(.5f, 1);
            iconRect.pivot = new Vector2(.5f, .5f);
            icon = iconObject.GetComponent<Image>();
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.gameObject.SetActive(false);
        }

        void ApplyLayout(PrototypeOrientation orientation)
        {
            if (!labelRect) return;
            labelRect.anchorMin = labelRect.anchorMax = new Vector2(.5f, 1);
            labelRect.pivot = new Vector2(.5f, 1);
            if (orientation == PrototypeOrientation.Portrait)
            {
                labelRect.anchoredPosition = new Vector2(24, -285);
                labelRect.sizeDelta = new Vector2(680, 72);
                label.fontSize = 28;
                iconRect.anchoredPosition = new Vector2(-350, -321);
                iconRect.sizeDelta = new Vector2(44, 44);
            }
            else
            {
                labelRect.anchoredPosition = new Vector2(25, -290);
                labelRect.sizeDelta = new Vector2(540, 64);
                label.fontSize = 26;
                iconRect.anchoredPosition = new Vector2(-270, -322);
                iconRect.sizeDelta = new Vector2(40, 40);
            }
        }

        static Sprite SpriteFor(RaidEncounterPhase phase) => phase switch
        {
            RaidEncounterPhase.Discovered => ResolveSprite(ref discoveredIcon, ref discoveredIconResolved, DiscoveredIconResource),
            RaidEncounterPhase.Hostiles => ResolveSprite(ref hostilesIcon, ref hostilesIconResolved, HostilesIconResource),
            RaidEncounterPhase.Cleared => ResolveSprite(ref clearedIcon, ref clearedIconResolved, ClearedIconResource),
            _ => null
        };

        static Sprite ResolveSprite(ref Sprite sprite, ref bool resolved, string resourcePath)
        {
            if (resolved) return sprite;
            resolved = true;
            try { sprite = spriteLoader?.Invoke(resourcePath); }
            catch (Exception) { sprite = null; }
            return sprite;
        }

        static Sprite LoadSprite(string resourcePath) => Resources.Load<Sprite>(resourcePath);

        public static void ConfigureSpriteLoaderForTests(Func<string, Sprite> loader)
        {
            ResetSpriteCache();
            spriteLoader = loader ?? (_ => null);
        }

        public static void ResetSpriteLoaderForTests()
        {
            ResetSpriteCache();
            spriteLoader = LoadSprite;
        }

        static void ResetSpriteCache()
        {
            discoveredIcon = hostilesIcon = clearedIcon = null;
            discoveredIconResolved = hostilesIconResolved = clearedIconResolved = false;
        }

        static float DurationFor(RaidEncounterPhase phase) => phase switch
        {
            RaidEncounterPhase.Discovered => DiscoveryDuration,
            RaidEncounterPhase.Hostiles => HostileCountDuration,
            RaidEncounterPhase.Cleared => ClearDuration,
            _ => 0
        };

        void OnEnable()
        {
            if (responsive)
            {
                responsive.LayoutChanged -= ApplyLayout;
                responsive.LayoutChanged += ApplyLayout;
            }
        }

        void OnDisable()
        {
            if (responsive) responsive.LayoutChanged -= ApplyLayout;
            Clear();
        }

        void OnDestroy()
        {
            if (responsive) responsive.LayoutChanged -= ApplyLayout;
        }
    }
}
