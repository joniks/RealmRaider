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
        public const float DiscoveryDuration = 1.8f;
        public const float HostileCountDuration = 2.4f;
        public const float ClearDuration = 2f;

        ResponsiveHudRoot responsive;
        RectTransform labelRect;
        Text label;
        float hideAt;
        RaidEncounterState current = RaidEncounterState.Hidden;

        public bool Visible => label && label.gameObject.activeSelf;
        public string Text => Visible ? label.text : string.Empty;
        public bool RaycastTarget => label && label.raycastTarget;
        public RectTransform Rect => labelRect;
        public RaidEncounterState Current => current;

        public void Initialize(ResponsiveHudRoot responsiveRoot)
        {
            if (responsive) responsive.LayoutChanged -= ApplyLayout;
            responsive = responsiveRoot;
            if (!label) CreateLabel();
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

        void ApplyLayout(PrototypeOrientation orientation)
        {
            if (!labelRect) return;
            labelRect.anchorMin = labelRect.anchorMax = new Vector2(.5f, 1);
            labelRect.pivot = new Vector2(.5f, 1);
            if (orientation == PrototypeOrientation.Portrait)
            {
                labelRect.anchoredPosition = new Vector2(0, -285);
                labelRect.sizeDelta = new Vector2(760, 72);
                label.fontSize = 28;
            }
            else
            {
                labelRect.anchoredPosition = new Vector2(0, -290);
                labelRect.sizeDelta = new Vector2(620, 64);
                label.fontSize = 26;
            }
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
