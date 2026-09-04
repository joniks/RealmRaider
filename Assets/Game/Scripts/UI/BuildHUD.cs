using System;
using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace RealmRaiders.UI
{
    public sealed class BuildHUD : MonoBehaviour
    {
        public int SlotCount => slots.Length;
        Button[] slots = Array.Empty<Button>(); Button saveButton; Text budget; Text reason; DefenseLayout layout; ResponsiveHudRoot responsive;
        public void Initialize() { layout = DefenseLayoutSave.Load(); Build(); Refresh(); }
        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); gameObject.AddComponent<GraphicRaycaster>();
            Label("SYLVAN BUILD", new Vector2(0, -120), 52); budget = Label("", new Vector2(0, -215), 30); reason = Label("", new Vector2(0, -310), 24);
            slots = new Button[5]; for (int i = 0; i < slots.Length; i++) { int index = i; slots[i] = Button("", new Vector2(0, 280 - i * 140), () => Cycle(index)); }
            saveButton = Button("SAVE & DEFEND", new Vector2(0, -650), SaveAndDefend);
            responsive = gameObject.AddComponent<ResponsiveHudRoot>(); responsive.LayoutChanged += ApplyOrientation; responsive.Initialize(false);
        }
        void Cycle(int index) { var slot = layout.Slots[index]; var next = slot.Piece; for (int i = 0; i < 4; i++) { next = (DefensePieceType)(((int)next + 1) % 4); var candidate = new DefenseSlotLayout(slot.SlotType, next); if (DefenseLayoutRules.IsAllowed(candidate)) { layout.Slots[index] = candidate; break; } } Refresh(); }
        void SaveAndDefend() { if (!DefenseLayoutRules.IsValid(layout, out _)) return; DefenseLayoutSave.Save(layout); SceneManager.LoadScene("DefenderTest"); }
        void Refresh() { var valid = DefenseLayoutRules.IsValid(layout, out var message); budget.text = $"Threat: {DefenseLayoutRules.Used(layout)}/{DefenseLayoutRules.Budget}"; reason.text = valid ? "Ready to defend" : message; if (saveButton) saveButton.interactable = valid; for (int i = 0; i < slots.Length; i++) slots[i].GetComponentInChildren<Text>().text = $"SLOT {i + 1}  {layout.Slots[i].Piece}"; }
        void ApplyOrientation(PrototypeOrientation orientation) { if (!saveButton) return; var x = orientation == PrototypeOrientation.Landscape ? 520 : 0; var y = orientation == PrototypeOrientation.Landscape ? 220 : 280; for (int i = 0; i < slots.Length; i++) slots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y - i * 115); saveButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, orientation == PrototypeOrientation.Landscape ? -450 : -650); }
        Text Label(string value, Vector2 position, int size) { var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1); rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(950, 90); var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = TextAnchor.UpperCenter; text.color = Color.white; return text; }
        Button Button(string value, Vector2 position, UnityEngine.Events.UnityAction action) { var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiPointerOwnership)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(720, 100); go.GetComponent<Image>().color = new Color(.12f, .3f, .18f, .96f); var button = go.GetComponent<Button>(); button.onClick.AddListener(action); var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text)); textGo.transform.SetParent(go.transform, false); var textRect = (RectTransform)textGo.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero; var label = textGo.GetComponent<Text>(); label.text = value; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 30; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; return button; }
    }
}
