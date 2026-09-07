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
        public string DefensePlanText => plan ? plan.text : string.Empty;
        public string SlotCopy(int index) => index >= 0 && index < slots.Length ? slots[index].GetComponentInChildren<Text>().text : string.Empty;
        public bool SaveInteractable => saveButton && saveButton.interactable;
        Button[] slots = Array.Empty<Button>(); Button saveButton; Text title; Text budget; Text reason; Text plan; DefenseLayout layout; ResponsiveHudRoot responsive; HudPresentation presentation;
        public void Initialize() { layout = DefenseLayoutSave.Load(); Build(); Refresh(); }
        void Build()
        {
            presentation = gameObject.AddComponent<HudPresentation>();
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); gameObject.AddComponent<GraphicRaycaster>();
            title = Label("SYLVAN BUILD", new Vector2(0, -120), 52); budget = Label("", new Vector2(0, -215), 30); reason = Label("", new Vector2(0, -310), 24);
            plan = Label("", new Vector2(0, -400), 18); plan.name = "Defense Plan Summary"; plan.rectTransform.sizeDelta = new Vector2(950, 90); plan.raycastTarget = false;
            slots = new Button[5]; for (int i = 0; i < slots.Length; i++) { int index = i; slots[i] = Button("", new Vector2(0, 280 - i * 140), () => Cycle(index)); }
            saveButton = Button("SAVE & DEFEND", new Vector2(0, -650), SaveAndDefend);
            responsive = gameObject.AddComponent<ResponsiveHudRoot>(); responsive.LayoutChanged += ApplyOrientation; responsive.Initialize(false);
        }
        public void CycleSlotForTests(int index) => Cycle(index);
        void Cycle(int index) { var slot = layout.Slots[index]; var next = slot.Piece; for (int i = 0; i < 4; i++) { next = (DefensePieceType)(((int)next + 1) % 4); var candidate = new DefenseSlotLayout(slot.SlotType, next); if (DefenseLayoutRules.IsAllowed(candidate)) { layout.Slots[index] = candidate; break; } } Refresh(); }
        void SaveAndDefend() { if (!DefenseLayoutRules.IsValid(layout, out _)) return; DefenseLayoutSave.Save(layout); SceneManager.LoadScene("DefenderTest"); }
        void Refresh() { var valid = DefenseLayoutRules.IsValid(layout, out var message); budget.text = $"Threat: {DefenseLayoutRules.Used(layout)}/{DefenseLayoutRules.Budget}"; reason.text = valid ? "Ready to defend" : message; plan.text = FormatDefensePlan(layout); if (saveButton) saveButton.interactable = valid; for (int i = 0; i < slots.Length; i++) slots[i].GetComponentInChildren<Text>().text = FormatSlotCopy(i, layout.Slots[i]); }
        void ApplyOrientation(PrototypeOrientation orientation)
        {
            if (!saveButton) return;
            var landscape = orientation == PrototypeOrientation.Landscape;
            var x = landscape ? 520 : 0; var y = landscape ? 220 : 280;
            for (int i = 0; i < slots.Length; i++) slots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y - i * 115);
            saveButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, landscape ? -450 : -650);
            var copyX = landscape ? -430 : 0;
            title.rectTransform.anchoredPosition = new Vector2(copyX, landscape ? -55 : -120);
            budget.rectTransform.anchoredPosition = new Vector2(copyX, landscape ? -145 : -215);
            reason.rectTransform.anchoredPosition = new Vector2(copyX, landscape ? -235 : -310);
            if (landscape)
            {
                plan.rectTransform.anchorMin = plan.rectTransform.anchorMax = new Vector2(.5f, 1);
                plan.rectTransform.pivot = new Vector2(.5f, 1);
                plan.rectTransform.anchoredPosition = new Vector2(copyX, -340);
            }
            else
            {
                // This is the clear gap between the fixed five-slot stack and SAVE & DEFEND.
                plan.rectTransform.anchorMin = plan.rectTransform.anchorMax = new Vector2(.5f, 0);
                plan.rectTransform.pivot = new Vector2(.5f, 0);
                plan.rectTransform.anchoredPosition = new Vector2(0, 430);
            }
        }
        public static string FormatSlotCopy(int index, DefenseSlotLayout slot)
        {
            return $"{SlotName(index)}\n{PieceName(slot.Piece)} • {Role(slot.Piece)} • {DefenseLayoutRules.Cost(slot.Piece)} THREAT";
        }
        public static string FormatDefensePlan(DefenseLayout defenseLayout)
        {
            return $"DEFENSE PLAN — INVADER → HEART TREE\n{PlanStage(defenseLayout, 3)}  →  {PlanStage(defenseLayout, 0)}  →  {PlanStage(defenseLayout, 1)}\n{PlanStage(defenseLayout, 4)}  →  {PlanStage(defenseLayout, 2)}  →  HEART TREE";
        }
        static string PlanStage(DefenseLayout defenseLayout, int index)
        {
            var piece = PieceAt(defenseLayout, index);
            return $"{SlotName(index)}: {PieceName(piece)}{(piece == DefensePieceType.Ent ? " [POSSESSABLE]" : string.Empty)}";
        }
        static DefensePieceType PieceAt(DefenseLayout defenseLayout, int index) => defenseLayout?.Slots != null && index >= 0 && index < defenseLayout.Slots.Length ? defenseLayout.Slots[index].Piece : DefensePieceType.Empty;
        static string SlotName(int index) => index switch { 0 => "OUTER GUARD", 1 => "MID GUARD", 2 => "HEART GUARD", 3 => "ROOT GATE", 4 => "INNER ROOT", _ => "DEFENSE POINT" };
        static string PieceName(DefensePieceType piece) => piece switch { DefensePieceType.Wolf => "WOLF", DefensePieceType.Ent => "ENT", DefensePieceType.RootTrap => "ROOT TRAP", _ => "OPEN" };
        static string Role(DefensePieceType piece) => piece switch { DefensePieceType.Wolf => "FAST INTERCEPT", DefensePieceType.Ent => "POSSESSABLE GUARDIAN", DefensePieceType.RootTrap => "MANUAL HOLD TRAP", _ => "UNASSIGNED" };
        Text Label(string value, Vector2 position, int size) { var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1); rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(950, 90); var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = TextAnchor.UpperCenter; text.color = Color.white; return text; }
        Button Button(string value, Vector2 position, UnityEngine.Events.UnityAction action) { var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiPointerOwnership)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(720, 100); go.GetComponent<Image>().color = new Color(.12f, .3f, .18f, .96f); var button = go.GetComponent<Button>(); presentation?.ApplyButton(go.GetComponent<Image>()); button.onClick.AddListener(action); button.onClick.AddListener(() => presentation?.PlayClick()); var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text)); textGo.transform.SetParent(go.transform, false); var textRect = (RectTransform)textGo.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero; var label = textGo.GetComponent<Text>(); label.text = value; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 30; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; return button; }
    }
}
