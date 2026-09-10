using RealmRaiders.Controllers;
using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    [DisallowMultipleComponent]
    public sealed class InRunControlStyleSelector : MonoBehaviour
    {
        public const string Contextual = "Contextual";
        public const string Fingertap = "Fingertap";
        public const string Joystick = "Joystick";
        public const string AutoCopy = "CONTROL: AUTO";
        public const string TapCopy = "CONTROL: TAP";
        public const string StickCopy = "CONTROL: STICK";

        ResponsiveHudRoot responsive;
        HudPresentation presentation;
        Button button;
        Text label;
        string displayedStyle;
        bool hasLiveState;
        bool displayedLive;

        public RectTransform SelectorRect => button ? (RectTransform)button.transform : null;
        public Button SelectorButton => button;
        public Text SelectorLabel => label;
        public bool Visible => button && button.gameObject.activeSelf;
        public string SavedStyle => PrototypeSave.ControlStylePreference;
        public string EffectiveStyle => EffectiveStyleFor(PrototypeSave.ControlStylePreference, responsive && responsive.Orientation == PrototypeOrientation.Landscape);

        public static InRunControlStyleSelector Attach(ResponsiveHudRoot root, HudPresentation presentation)
        {
            if (!root) throw new System.ArgumentNullException(nameof(root));
            var existing = root.GetComponentInChildren<InRunControlStyleSelector>(true);
            if (existing) return existing;

            var host = new GameObject("In-Run Control Style Selector", typeof(RectTransform), typeof(InRunControlStyleSelector));
            host.transform.SetParent(root.transform, false);
            var hostRect = (RectTransform)host.transform;
            hostRect.anchorMin = Vector2.zero;
            hostRect.anchorMax = Vector2.one;
            hostRect.offsetMin = hostRect.offsetMax = Vector2.zero;
            var selector = host.GetComponent<InRunControlStyleSelector>();
            selector.Initialize(root, presentation);
            return selector;
        }

        public static string NormalizePreference(string value) => value switch
        {
            Contextual => Contextual,
            Fingertap => Fingertap,
            Joystick => Joystick,
            _ => Contextual
        };

        public static string NextPreference(string value) => value switch
        {
            Contextual => Fingertap,
            Fingertap => Joystick,
            Joystick => Contextual,
            _ => Contextual
        };

        public static string CopyFor(string value) => NormalizePreference(value) switch
        {
            Fingertap => TapCopy,
            Joystick => StickCopy,
            _ => AutoCopy
        };

        public static string EffectiveStyleFor(string value, bool landscape)
        {
            var saved = NormalizePreference(value);
            return saved == Contextual ? (landscape ? Joystick : Fingertap) : saved;
        }

        public void Cycle()
        {
            if (GameplayInput.TerminalState) return;
            PrototypeSave.SetControlStyle(NextPreference(PrototypeSave.ControlStylePreference));
            responsive.RefreshControlPresentation();
            RefreshNow();
        }

        public void RefreshNow()
        {
            if (!button) return;
            var live = !GameplayInput.TerminalState;
            if (hasLiveState && displayedLive && !live) GameplayInput.ResetTransientInput();
            hasLiveState = true;
            displayedLive = live;
            if (button.gameObject.activeSelf != live) button.gameObject.SetActive(live);
            button.interactable = live;
            if (live)
            {
                var savedStyle = NormalizePreference(PrototypeSave.ControlStylePreference);
                var copy = CopyFor(savedStyle);
                if (label.text != copy) label.text = copy;
                if (displayedStyle != savedStyle)
                {
                    displayedStyle = savedStyle;
                    presentation?.DecorateControlStyleButton(button, savedStyle);
                }
            }
            responsive?.RefreshControlPresentation();
        }

        void Initialize(ResponsiveHudRoot root, HudPresentation presentation)
        {
            responsive = root;
            this.presentation = presentation;
            var buttonObject = new GameObject("Control Style", typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiPointerOwnership));
            buttonObject.transform.SetParent(transform, false);
            buttonObject.GetComponent<Image>().color = new Color(.08f, .22f, .13f, .94f);
            button = buttonObject.GetComponent<Button>();
            presentation?.ApplyButton(buttonObject.GetComponent<Image>());
            button.onClick.AddListener(Cycle);
            button.onClick.AddListener(() => presentation?.PlayClick());

            var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(buttonObject.transform, false);
            var textRect = (RectTransform)textObject.transform;
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = textRect.offsetMax = Vector2.zero;
            label = textObject.GetComponent<Text>();
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 20;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.raycastTarget = false;

            responsive.LayoutChanged += OnLayoutChanged;
            ApplyLayout(responsive.Orientation);
            RefreshNow();
        }

        void Update() => RefreshNow();

        void OnLayoutChanged(PrototypeOrientation orientation)
        {
            ApplyLayout(orientation);
            RefreshNow();
        }

        void ApplyLayout(PrototypeOrientation orientation)
        {
            if (!button) return;
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = Vector2.zero;
            rect.pivot = Vector2.zero;
            rect.anchoredPosition = orientation == PrototypeOrientation.Portrait ? new Vector2(40, 480) : new Vector2(40, 430);
            rect.sizeDelta = new Vector2(280, 64);
        }

        void OnDisable()
        {
            if (hasLiveState) GameplayInput.ResetTransientInput();
        }

        void OnDestroy()
        {
            if (responsive) responsive.LayoutChanged -= OnLayoutChanged;
        }
    }
}
