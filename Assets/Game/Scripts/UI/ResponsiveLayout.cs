using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using RealmRaiders.Controllers;
using RealmRaiders.Core;

namespace RealmRaiders.UI
{
    public enum PrototypeOrientation { Portrait, Landscape }

    public static class ResponsiveLayout
    {
        public static PrototypeOrientation Classify(Vector2 usableSize) => usableSize.x >= usableSize.y ? PrototypeOrientation.Landscape : PrototypeOrientation.Portrait;
        public static Rect SafeAreaPixels => Screen.safeArea;
        public static Rect NormalizeSafeArea(Rect safeArea, Vector2 screenSize) => new(safeArea.x / Mathf.Max(1, screenSize.x), safeArea.y / Mathf.Max(1, screenSize.y), safeArea.width / Mathf.Max(1, screenSize.x), safeArea.height / Mathf.Max(1, screenSize.y));
        public static Rect NormalizedSafeArea => NormalizeSafeArea(Screen.safeArea, new Vector2(Screen.width, Screen.height));
        public static Vector2 NormalizeJoystick(Vector2 raw, float deadZone) { var magnitude = Mathf.Clamp01(raw.magnitude); if (magnitude <= Mathf.Clamp01(deadZone)) return Vector2.zero; return raw.normalized * ((magnitude - deadZone) / Mathf.Max(.001f, 1 - deadZone)); }
    }

    public sealed class ResponsiveHudRoot : MonoBehaviour
    {
        public event Action<PrototypeOrientation> LayoutChanged;
        public PrototypeOrientation Orientation { get; private set; }
        public Rect SafeAreaPixels => ResponsiveLayout.SafeAreaPixels;
        CanvasScaler scaler; RectTransform rect; VirtualJoystick joystick; Vector2 lastSize; Rect lastSafe; PrototypeOrientation? testOverride; readonly Dictionary<RectTransform, (Vector2 min, Vector2 max, Vector2 pivot, Vector2 position)> original = new();
        public void SetOrientationForTests(PrototypeOrientation orientation) { testOverride = orientation; Apply(true); }
        public void ClearOrientationOverrideForTests() { testOverride = null; Apply(true); }
        public void Initialize(bool gameplay)
        {
            GameplayInput.SetTerminalState(false);
            rect = GetComponent<RectTransform>(); scaler = GetComponent<CanvasScaler>();
            if (!scaler) scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.matchWidthOrHeight = .5f;
            if (gameplay) { var go = new GameObject("Landscape Joystick", typeof(RectTransform), typeof(Image), typeof(VirtualJoystick)); go.transform.SetParent(transform, false); joystick = go.GetComponent<VirtualJoystick>(); }
            CaptureButtons();
            Apply(true);
        }
        void Update() { CaptureButtons(); Apply(false); ReflowActions(Orientation); if (joystick) joystick.SetVisible(PrototypeSave.EffectiveControlStyle(Orientation == PrototypeOrientation.Landscape) == "Joystick"); }
        void CaptureButtons() { foreach (var button in GetComponentsInChildren<Button>(true)) { var buttonRect = (RectTransform)button.transform; if (!original.ContainsKey(buttonRect)) original[buttonRect] = (buttonRect.anchorMin, buttonRect.anchorMax, buttonRect.pivot, buttonRect.anchoredPosition); } }
        void Apply(bool force)
        {
            var size = new Vector2(Screen.width, Screen.height); var safe = Screen.safeArea; var orientation = testOverride ?? ResponsiveLayout.Classify(safe.size);
            if (!force && orientation == Orientation && size == lastSize && safe == lastSafe) return;
            lastSize = size; lastSafe = safe; Orientation = orientation; GameplayInput.ResetTransientInput(); scaler.referenceResolution = orientation == PrototypeOrientation.Portrait ? new Vector2(1080, 1920) : new Vector2(1920, 1080);
            var normalized = ResponsiveLayout.NormalizedSafeArea; rect.anchorMin = normalized.min; rect.anchorMax = normalized.max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            if (joystick) joystick.SetVisible(PrototypeSave.EffectiveControlStyle(orientation == PrototypeOrientation.Landscape) == "Joystick");
            ReflowActions(orientation);
            LayoutChanged?.Invoke(Orientation);
        }
        void ReflowActions(PrototypeOrientation orientation)
        {
            if (GetComponent<BuildHUD>()) return;
            var buttons = GetComponentsInChildren<Button>(true); var index = 0;
            foreach (var button in buttons)
            {
                var buttonRect = (RectTransform)button.transform; if (!original.ContainsKey(buttonRect)) continue;
                if (orientation == PrototypeOrientation.Landscape) { buttonRect.anchorMin = buttonRect.anchorMax = new Vector2(1, 0); buttonRect.pivot = new Vector2(1, 0); buttonRect.anchoredPosition = new Vector2(LandscapeButtonX(button.name), LandscapeButtonY(button.name, index++)); }
                else { var value = original[buttonRect]; buttonRect.anchorMin = value.min; buttonRect.anchorMax = value.max; buttonRect.pivot = value.pivot; buttonRect.anchoredPosition = value.position; }
            }
        }
        float LandscapeButtonY(string name, int index)
        {
            if (GetComponent<HubHUD>()) return name == "AUTO" || name == "PORTRAIT" || name == "LANDSCAPE" ? 900 : name == "CONTEXTUAL" || name == "FINGERTAP" || name == "JOYSTICK" ? 760 : name == "BUILD SYLVAN" ? 620 : name == "DEFEND SYLVAN" ? 500 : name == "RAID SYLVAN" ? 380 : name == "DEFEND INFERNAL" ? 260 : 140;
            return 110 + index * 108;
        }
        float LandscapeButtonX(string name) => GetComponent<HubHUD>() && (name == "AUTO" || name == "PORTRAIT" || name == "LANDSCAPE" || name == "CONTEXTUAL" || name == "FINGERTAP" || name == "JOYSTICK") ? name == "AUTO" || name == "CONTEXTUAL" ? -620 : name == "PORTRAIT" || name == "FINGERTAP" ? -310 : -70 : -70;
    }

    public sealed class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, ICancelHandler
    {
        public float DeadZone = .12f; public float Radius = 110;
        Image baseImage, knob; int pointerId = int.MinValue; RectTransform rect; Canvas canvas;
        void Awake() { rect = (RectTransform)transform; canvas = GetComponentInParent<Canvas>(); baseImage = GetComponent<Image>(); baseImage.color = new Color(.05f, .12f, .08f, .8f); rect.anchorMin = new Vector2(0, 0); rect.anchorMax = new Vector2(0, 0); rect.pivot = new Vector2(.5f, .5f); rect.sizeDelta = new Vector2(260, 260); rect.anchoredPosition = new Vector2(170, 170); var child = new GameObject("Knob", typeof(RectTransform), typeof(Image)); child.transform.SetParent(transform, false); knob = child.GetComponent<Image>(); knob.color = new Color(.4f, .8f, .5f, .95f); var knobRect = (RectTransform)child.transform; knobRect.sizeDelta = new Vector2(110, 110); }
        public void SetVisible(bool visible) { visible &= GameplayInput.DirectControlActive && !GameplayInput.TerminalState; if (gameObject.activeSelf != visible) gameObject.SetActive(visible); if (!visible) ResetJoystick(); }
        public void OnPointerDown(PointerEventData eventData) { pointerId = eventData.pointerId; GameplayInput.ClaimUiPointer(pointerId); UpdateValue(eventData); }
        public void OnDrag(PointerEventData eventData) { if (eventData.pointerId == pointerId) UpdateValue(eventData); }
        public void OnPointerUp(PointerEventData eventData) { if (eventData.pointerId == pointerId) ResetJoystick(); }
        public void OnCancel(BaseEventData eventData) => ResetJoystick();
        void UpdateValue(PointerEventData data) { if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rect, data.position, canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null, out var local)) return; local /= Radius; GameplayInput.SetMovement(ResponsiveLayout.NormalizeJoystick(local, DeadZone)); if (knob) ((RectTransform)knob.transform).anchoredPosition = Vector2.ClampMagnitude(local * Radius, Radius); }
        void ResetJoystick() { if (pointerId != int.MinValue) GameplayInput.ReleaseUiPointer(pointerId); pointerId = int.MinValue; GameplayInput.ClearMovement(); if (knob) ((RectTransform)knob.transform).anchoredPosition = Vector2.zero; }
        void OnDisable() => ResetJoystick();
        void OnApplicationFocus(bool focus) { if (!focus) ResetJoystick(); }
    }

    public sealed class UiPointerOwnership : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ICancelHandler
    {
        public void OnPointerDown(PointerEventData eventData) => GameplayInput.ClaimUiPointer(eventData.pointerId);
        public void OnPointerUp(PointerEventData eventData) => GameplayInput.ReleaseUiPointer(eventData.pointerId);
        public void OnCancel(BaseEventData eventData) { if (eventData is PointerEventData pointer) GameplayInput.ReleaseUiPointer(pointer.pointerId); }
    }
}
