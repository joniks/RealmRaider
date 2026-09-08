using System;
using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    /// <summary>Scene-local BUILD emphasis and owned guide actions on the existing responsive HUD.</summary>
    [DisallowMultipleComponent]
    public sealed class FirstPlayableMinuteBuildGuide : MonoBehaviour
    {
        ResponsiveHudRoot responsive;
        Button[] slots;
        Button save;
        Button dismiss;
        Button skip;
        RectTransform emphasis;
        BuildGuideStep step;
        bool shutdown;

        public bool EmphasisVisible => emphasis && emphasis.gameObject.activeSelf;
        public bool DismissVisible => dismiss && dismiss.gameObject.activeSelf;
        public bool SkipVisible => skip && skip.gameObject.activeSelf;
        public RectTransform EmphasisRect => emphasis;
        public RectTransform DismissRect => dismiss ? (RectTransform)dismiss.transform : null;
        public RectTransform SkipRect => skip ? (RectTransform)skip.transform : null;

        public void Initialize(ResponsiveHudRoot layout, Button[] slotButtons, Button saveButton, HudPresentation skin, Action dismissAction, Action skipAction)
        {
            responsive = layout;
            slots = slotButtons;
            save = saveButton;
            emphasis = CreateEmphasis();
            dismiss = CreateAction("DISMISS", skin, dismissAction, new Color(.15f, .23f, .18f, .98f));
            skip = CreateAction("SKIP GUIDE", skin, skipAction, new Color(.18f, .18f, .2f, .98f));
            responsive.LayoutChanged += ApplyLayout;
            ApplyLayout(responsive.Orientation);
        }

        public void Show(BuildGuideStep currentStep, bool lineVisible)
        {
            if (shutdown) return;
            step = currentStep;
            skip.gameObject.SetActive(currentStep != BuildGuideStep.Hidden);
            dismiss.gameObject.SetActive(currentStep != BuildGuideStep.Hidden && lineVisible);
            emphasis.gameObject.SetActive(currentStep != BuildGuideStep.Hidden && lineVisible);
            ApplyEmphasis();
        }

        public void Shutdown()
        {
            if (shutdown) return;
            shutdown = true;
            if (responsive) responsive.LayoutChanged -= ApplyLayout;
            if (dismiss) { dismiss.onClick.RemoveAllListeners(); dismiss.gameObject.SetActive(false); }
            if (skip) { skip.onClick.RemoveAllListeners(); skip.gameObject.SetActive(false); }
            if (emphasis) emphasis.gameObject.SetActive(false);
            step = BuildGuideStep.Hidden;
        }

        void OnDisable() => Shutdown();
        void OnDestroy() => Shutdown();

        RectTransform CreateEmphasis()
        {
            var go = new GameObject("Build Guide Emphasis", typeof(RectTransform), typeof(Image), typeof(Outline));
            go.transform.SetParent(transform, false);
            var image = go.GetComponent<Image>();
            image.color = new Color(1f, .78f, .24f, .07f);
            image.raycastTarget = false;
            var outline = go.GetComponent<Outline>();
            outline.effectColor = new Color(1f, .82f, .34f, .96f);
            outline.effectDistance = new Vector2(4, -4);
            outline.useGraphicAlpha = false;
            go.SetActive(false);
            return (RectTransform)go.transform;
        }

        Button CreateAction(string name, HudPresentation skin, Action action, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiPointerOwnership));
            go.transform.SetParent(transform, false);
            var image = go.GetComponent<Image>(); image.color = color; skin?.ApplyButton(image);
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => skin?.PlayClick());
            button.onClick.AddListener(() => action?.Invoke());

            var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text));
            textObject.transform.SetParent(go.transform, false);
            var textRect = (RectTransform)textObject.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero;
            var label = textObject.GetComponent<Text>(); label.text = name; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 22; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; label.raycastTarget = false;
            return button;
        }

        void ApplyLayout(PrototypeOrientation orientation)
        {
            if (shutdown || !dismiss || !skip) return;
            if (orientation == PrototypeOrientation.Portrait)
            {
                PlaceAction(dismiss, new Vector2(-155, 530), new Vector2(290, 96));
                PlaceAction(skip, new Vector2(155, 530), new Vector2(290, 96));
            }
            else
            {
                PlaceAction(dismiss, new Vector2(-590, 245), new Vector2(300, 96));
                PlaceAction(skip, new Vector2(-270, 245), new Vector2(300, 96));
            }
            ApplyEmphasis();
        }

        static void PlaceAction(Button button, Vector2 position, Vector2 size)
        {
            var rect = (RectTransform)button.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0);
            rect.pivot = new Vector2(.5f, 0);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }

        void ApplyEmphasis()
        {
            if (!emphasis || !emphasis.gameObject.activeSelf) return;
            if (step == BuildGuideStep.Save && save)
            {
                CopyBounds((RectTransform)save.transform, 16);
                return;
            }
            if (slots == null || slots.Length == 0) { emphasis.gameObject.SetActive(false); return; }

            var first = (RectTransform)slots[0].transform;
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity);
            var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            foreach (var slot in slots)
            {
                if (!slot || !slot.gameObject.activeInHierarchy) { emphasis.gameObject.SetActive(false); return; }
                var rect = (RectTransform)slot.transform;
                var pivotPoint = rect.anchoredPosition;
                min = Vector2.Min(min, pivotPoint - Vector2.Scale(rect.pivot, rect.sizeDelta));
                max = Vector2.Max(max, pivotPoint + Vector2.Scale(Vector2.one - rect.pivot, rect.sizeDelta));
            }
            emphasis.anchorMin = emphasis.anchorMax = first.anchorMin;
            emphasis.pivot = new Vector2(.5f, .5f);
            emphasis.anchoredPosition = (min + max) * .5f;
            emphasis.sizeDelta = max - min + new Vector2(20, 20);
        }

        void CopyBounds(RectTransform target, float padding)
        {
            emphasis.anchorMin = emphasis.anchorMax = target.anchorMin;
            emphasis.pivot = target.pivot;
            emphasis.anchoredPosition = target.anchoredPosition;
            emphasis.sizeDelta = target.sizeDelta + Vector2.one * padding;
        }
    }
}
