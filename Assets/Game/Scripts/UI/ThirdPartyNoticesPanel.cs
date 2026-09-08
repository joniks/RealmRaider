using System.Collections.Generic;
using RealmRaiders.Controllers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    [DisallowMultipleComponent]
    public sealed class ThirdPartyNoticesPanel : MonoBehaviour
    {
        const string TitleCopy = "THIRD-PARTY NOTICES";
        readonly Dictionary<Button, bool> priorInteractability = new();
        readonly List<ThirdPartyNoticePointerOwnership> pointerOwners = new();
        ResponsiveHudRoot responsive;
        Button entryButton;
        Button closeButton;
        RectTransform surface;
        RectTransform viewport;
        RectTransform content;
        RectTransform titleRect;
        RectTransform closeRect;
        RectTransform scrollRectTransform;
        RectTransform scrollbarRect;
        Image blockerImage;
        Text bodyText;
        ScrollRect scrollRect;
        Scrollbar scrollbar;
        ThirdPartyNoticeCatalogue catalogue;
        bool subscribed;
        bool initialized;
        bool isOpen;
        bool layoutValid;
        bool contentDirty;
        PrototypeOrientation lastOrientation;
        Vector2 lastRootSize;

        public bool IsOpen => isOpen;
        public string BodyCopy => catalogue?.BodyCopy ?? string.Empty;
        public bool UsesFallback => catalogue != null && catalogue.UsedFallback;
        public float VerticalNormalizedPosition { get => !scrollRect || !scrollRect.vertical ? 1 : scrollRect.verticalNormalizedPosition; set => SetScrollPosition(value); }
        public bool CanScroll => scrollRect && scrollRect.vertical;
        public bool ScrollbarVisible => scrollbar && scrollbar.gameObject.activeSelf;
        public bool BlockerRaycastTarget => blockerImage && blockerImage.raycastTarget;
        public Button CloseButton => closeButton;
        public RectTransform SurfaceRect => surface;
        public RectTransform TitleRect => titleRect;
        public RectTransform CloseRect => closeRect;
        public RectTransform ScrollRectTransform => scrollRectTransform;
        public RectTransform ViewportRect => viewport;
        public RectTransform ContentRect => content;
        public RectTransform ScrollbarRect => scrollbarRect;
        public Text BodyText => bodyText;

        public void Initialize(ResponsiveHudRoot layout, Button entry, HudPresentation presentation, ThirdPartyNoticeCatalogue noticeCatalogue)
        {
            if (initialized) return;
            initialized = true;
            responsive = layout;
            entryButton = entry;
            catalogue = noticeCatalogue ?? ThirdPartyNoticeCatalogue.Create(null);
            Build(presentation);
            ApplyCatalogue();
            ApplyLayout(responsive.Orientation, 1);
            gameObject.SetActive(false);
        }

        public void Open()
        {
            if (!initialized || isOpen) return;
            CaptureAndBlockHubButtons();
            isOpen = true;
            gameObject.SetActive(true);
            Subscribe();
            ApplyLayout(responsive.Orientation, 1);
            EventSystem.current?.SetSelectedGameObject(closeButton.gameObject);
        }

        public void Close()
        {
            if (!isOpen) return;
            ReleaseOwnedPointers();
            Unsubscribe();
            isOpen = false;
            RestoreHubButtons();
            gameObject.SetActive(false);
            if (entryButton && entryButton.interactable) EventSystem.current?.SetSelectedGameObject(entryButton.gameObject);
        }

        public bool TryCloseFromBack()
        {
            if (!isOpen) return false;
            Close();
            return true;
        }

        public void SetCatalogueForTests(ThirdPartyNoticeCatalogue noticeCatalogue)
        {
            catalogue = noticeCatalogue ?? ThirdPartyNoticeCatalogue.Create(null);
            ApplyCatalogue();
            if (isOpen) ApplyLayout(responsive.Orientation, 1);
        }

        void Update()
        {
            if (isOpen && Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame) TryCloseFromBack();
        }

        void Build(HudPresentation presentation)
        {
            var root = (RectTransform)transform;
            root.anchorMin = Vector2.zero; root.anchorMax = Vector2.one; root.offsetMin = root.offsetMax = Vector2.zero;

            var blocker = new GameObject("Third-Party Notices Blocker", typeof(RectTransform), typeof(Image), typeof(ThirdPartyNoticePointerOwnership));
            blocker.transform.SetParent(transform, false);
            Stretch((RectTransform)blocker.transform);
            blockerImage = blocker.GetComponent<Image>(); blockerImage.color = new Color(.01f, .018f, .014f, .96f); blockerImage.raycastTarget = true;
            RegisterOwner(blocker.GetComponent<ThirdPartyNoticePointerOwnership>());

            var surfaceObject = new GameObject("Third-Party Notices Surface", typeof(RectTransform), typeof(Image), typeof(ThirdPartyNoticePointerOwnership));
            surfaceObject.transform.SetParent(transform, false);
            surface = (RectTransform)surfaceObject.transform;
            var surfaceImage = surfaceObject.GetComponent<Image>(); surfaceImage.color = new Color(.025f, .055f, .038f, .995f); surfaceImage.raycastTarget = true;
            RegisterOwner(surfaceObject.GetComponent<ThirdPartyNoticePointerOwnership>());

            var title = new GameObject("Third-Party Notices Title", typeof(RectTransform), typeof(Text));
            title.transform.SetParent(surface, false); titleRect = (RectTransform)title.transform;
            var titleText = title.GetComponent<Text>(); titleText.text = TitleCopy; titleText.font = BuiltinFont(); titleText.fontSize = 34; titleText.fontStyle = FontStyle.Bold; titleText.alignment = TextAnchor.MiddleLeft; titleText.color = Color.white; titleText.raycastTarget = false;

            closeButton = CreateCloseButton(surface, presentation);
            closeRect = (RectTransform)closeButton.transform;

            var scrollObject = new GameObject("Third-Party Notices Scroll", typeof(RectTransform), typeof(ScrollRect), typeof(ThirdPartyNoticePointerOwnership));
            scrollObject.transform.SetParent(surface, false); scrollRectTransform = (RectTransform)scrollObject.transform;
            scrollRect = scrollObject.GetComponent<ScrollRect>(); scrollRect.horizontal = false; scrollRect.vertical = true; scrollRect.movementType = ScrollRect.MovementType.Clamped; scrollRect.inertia = true; scrollRect.decelerationRate = .135f; scrollRect.scrollSensitivity = 42;
            RegisterOwner(scrollObject.GetComponent<ThirdPartyNoticePointerOwnership>());

            var viewportObject = new GameObject("Viewport", typeof(RectTransform), typeof(Image), typeof(Mask));
            viewportObject.transform.SetParent(scrollRectTransform, false); viewport = (RectTransform)viewportObject.transform; Stretch(viewport);
            var viewportImage = viewportObject.GetComponent<Image>(); viewportImage.color = new Color(0, 0, 0, .001f); viewportImage.raycastTarget = true;
            viewportObject.GetComponent<Mask>().showMaskGraphic = false;

            var contentObject = new GameObject("Notice Content", typeof(RectTransform), typeof(Text));
            contentObject.transform.SetParent(viewport, false); content = (RectTransform)contentObject.transform;
            content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one; content.pivot = new Vector2(.5f, 1); content.anchoredPosition = Vector2.zero;
            bodyText = contentObject.GetComponent<Text>(); bodyText.font = BuiltinFont(); bodyText.fontSize = 26; bodyText.lineSpacing = 1.2f; bodyText.alignment = TextAnchor.UpperLeft; bodyText.color = new Color(.93f, .97f, .94f); bodyText.horizontalOverflow = HorizontalWrapMode.Wrap; bodyText.verticalOverflow = VerticalWrapMode.Overflow; bodyText.supportRichText = true; bodyText.raycastTarget = false;

            scrollbar = CreateScrollbar(scrollRectTransform);
            scrollbarRect = (RectTransform)scrollbar.transform;
            RegisterOwner(scrollbar.GetComponent<ThirdPartyNoticePointerOwnership>());
            scrollRect.viewport = viewport; scrollRect.content = content; scrollRect.verticalScrollbar = scrollbar; scrollRect.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.Permanent; scrollRect.verticalScrollbarSpacing = 12;
        }

        Button CreateCloseButton(Transform parent, HudPresentation presentation)
        {
            var go = new GameObject("CLOSE", typeof(RectTransform), typeof(Image), typeof(Button), typeof(ThirdPartyNoticePointerOwnership));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>(); image.color = new Color(.16f, .38f, .23f, 1); presentation?.ApplyButton(image);
            var button = go.GetComponent<Button>(); button.onClick.AddListener(Close); button.onClick.AddListener(() => presentation?.PlayClick());
            RegisterOwner(go.GetComponent<ThirdPartyNoticePointerOwnership>());
            var labelObject = new GameObject("Text", typeof(RectTransform), typeof(Text)); labelObject.transform.SetParent(go.transform, false); Stretch((RectTransform)labelObject.transform);
            var label = labelObject.GetComponent<Text>(); label.text = "CLOSE"; label.font = BuiltinFont(); label.fontSize = 25; label.fontStyle = FontStyle.Bold; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; label.raycastTarget = false;
            return button;
        }

        static Scrollbar CreateScrollbar(Transform parent)
        {
            var root = new GameObject("Scrollbar", typeof(RectTransform), typeof(Image), typeof(Scrollbar), typeof(ThirdPartyNoticePointerOwnership));
            root.transform.SetParent(parent, false);
            var background = root.GetComponent<Image>(); background.color = new Color(.07f, .12f, .09f, 1); background.raycastTarget = true;
            var slideArea = new GameObject("Handle Slide Area", typeof(RectTransform)); slideArea.transform.SetParent(root.transform, false);
            var slideRect = (RectTransform)slideArea.transform; Stretch(slideRect); slideRect.offsetMin = new Vector2(5, 5); slideRect.offsetMax = new Vector2(-5, -5);
            var handleObject = new GameObject("Handle", typeof(RectTransform), typeof(Image)); handleObject.transform.SetParent(slideArea.transform, false);
            var handleRect = (RectTransform)handleObject.transform; Stretch(handleRect);
            var handle = handleObject.GetComponent<Image>(); handle.color = new Color(.46f, .72f, .52f, 1); handle.raycastTarget = false;
            var scrollbar = root.GetComponent<Scrollbar>(); scrollbar.handleRect = handleRect; scrollbar.targetGraphic = background; scrollbar.direction = Scrollbar.Direction.BottomToTop;
            return scrollbar;
        }

        void ApplyCatalogue()
        {
            if (!bodyText || catalogue == null) return;
            bodyText.text = catalogue.BodyCopy
                .Replace("REQUIRED ATTRIBUTION", "<size=30><b>REQUIRED ATTRIBUTION</b></size>")
                .Replace("VOLUNTARY PROVENANCE CREDITS", "<size=30><b>VOLUNTARY PROVENANCE CREDITS</b></size>");
            contentDirty = true;
        }

        void OnLayoutChanged(PrototypeOrientation orientation)
        {
            if (!isOpen) return;
            var normalized = scrollRect.vertical ? scrollRect.verticalNormalizedPosition : 1;
            ReleaseOwnedPointers();
            ApplyLayout(orientation, normalized);
        }

        void ApplyLayout(PrototypeOrientation orientation, float normalizedPosition)
        {
            if (!surface) return;
            Canvas.ForceUpdateCanvases();
            var rootSize = responsive ? ((RectTransform)responsive.transform).rect.size : Vector2.zero;
            var geometryChanged = !layoutValid || orientation != lastOrientation || rootSize != lastRootSize;
            if (!geometryChanged && !contentDirty)
            {
                SetScrollPosition(normalizedPosition);
                return;
            }
            layoutValid = true; lastOrientation = orientation; lastRootSize = rootSize;
            var portrait = orientation == PrototypeOrientation.Portrait;
            surface.anchorMin = portrait ? new Vector2(.06f, .06f) : new Vector2(.10f, .08f);
            surface.anchorMax = portrait ? new Vector2(.94f, .94f) : new Vector2(.90f, .92f);
            surface.offsetMin = surface.offsetMax = Vector2.zero;

            titleRect.anchorMin = new Vector2(0, 1); titleRect.anchorMax = Vector2.one; titleRect.pivot = new Vector2(.5f, 1);
            titleRect.offsetMin = new Vector2(portrait ? 48 : 44, -112); titleRect.offsetMax = new Vector2(portrait ? -180 : -170, -16);
            closeRect.anchorMin = closeRect.anchorMax = Vector2.one; closeRect.pivot = Vector2.one; closeRect.anchoredPosition = new Vector2(-16, -16); closeRect.sizeDelta = portrait ? new Vector2(144, 96) : new Vector2(136, 88);
            scrollRectTransform.anchorMin = Vector2.zero; scrollRectTransform.anchorMax = Vector2.one;
            scrollRectTransform.offsetMin = new Vector2(portrait ? 48 : 44, portrait ? 48 : 40);
            scrollRectTransform.offsetMax = new Vector2(portrait ? -48 : -44, portrait ? -128 : -120);
            viewport.offsetMin = Vector2.zero; viewport.offsetMax = new Vector2(-48, 0);
            scrollbarRect.anchorMin = new Vector2(1, 0); scrollbarRect.anchorMax = Vector2.one; scrollbarRect.pivot = new Vector2(1, .5f); scrollbarRect.anchoredPosition = Vector2.zero; scrollbarRect.sizeDelta = new Vector2(32, 0);
            bodyText.fontSize = portrait ? 26 : 24;
            Canvas.ForceUpdateCanvases();
            RefreshContentGeometry(normalizedPosition);
            contentDirty = false;
        }

        void RefreshContentGeometry(float normalizedPosition)
        {
            var width = Mathf.Max(1, viewport.rect.width - 8);
            content.sizeDelta = new Vector2(0, 0);
            Canvas.ForceUpdateCanvases();
            var settings = bodyText.GetGenerationSettings(new Vector2(width, 0));
            var preferred = bodyText.cachedTextGeneratorForLayout.GetPreferredHeight(bodyText.text, settings) / bodyText.pixelsPerUnit + 16;
            var height = Mathf.Max(viewport.rect.height, preferred);
            content.sizeDelta = new Vector2(0, height);
            var overflow = preferred > viewport.rect.height + .5f;
            scrollRect.vertical = overflow;
            scrollbar.gameObject.SetActive(overflow);
            Canvas.ForceUpdateCanvases();
            SetScrollPosition(normalizedPosition);
        }

        void SetScrollPosition(float normalizedPosition)
        {
            if (!scrollRect) return;
            var normalized = scrollRect.vertical ? Mathf.Clamp01(normalizedPosition) : 1;
            scrollRect.StopMovement();
            scrollRect.verticalNormalizedPosition = normalized;
            if (scrollbar) scrollbar.SetValueWithoutNotify(normalized);
        }

        void CaptureAndBlockHubButtons()
        {
            priorInteractability.Clear();
            var hub = GetComponentInParent<HubHUD>();
            if (!hub) return;
            foreach (var button in hub.GetComponentsInChildren<Button>(true))
            {
                if (button.transform.IsChildOf(transform)) continue;
                priorInteractability[button] = button.interactable;
                button.interactable = false;
            }
        }

        void RestoreHubButtons()
        {
            foreach (var pair in priorInteractability) if (pair.Key) pair.Key.interactable = pair.Value;
            priorInteractability.Clear();
        }

        void RegisterOwner(ThirdPartyNoticePointerOwnership owner)
        {
            owner.Initialize();
            pointerOwners.Add(owner);
        }

        void ReleaseOwnedPointers()
        {
            foreach (var owner in pointerOwners) if (owner) owner.ReleaseAll();
        }

        void Subscribe()
        {
            if (subscribed || !responsive) return;
            responsive.LayoutChanged += OnLayoutChanged;
            subscribed = true;
        }

        void Unsubscribe()
        {
            if (!subscribed || !responsive) return;
            responsive.LayoutChanged -= OnLayoutChanged;
            subscribed = false;
        }

        void OnApplicationFocus(bool focus) { if (!focus) ReleaseOwnedPointers(); }

        void OnDisable()
        {
            ReleaseOwnedPointers();
            Unsubscribe();
            if (!isOpen) return;
            isOpen = false;
            RestoreHubButtons();
            RestoreFocusIfHidden();
        }

        void OnDestroy()
        {
            ReleaseOwnedPointers();
            Unsubscribe();
            RestoreHubButtons();
            RestoreFocusIfHidden();
            if (closeButton) closeButton.onClick.RemoveAllListeners();
        }

        void RestoreFocusIfHidden()
        {
            var events = EventSystem.current;
            if (!events || !entryButton || !entryButton.interactable) return;
            var selected = events.currentSelectedGameObject;
            if (selected && selected.transform.IsChildOf(transform)) events.SetSelectedGameObject(entryButton.gameObject);
        }

        static Font BuiltinFont() => Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        static void Stretch(RectTransform rect) { rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero; }
    }

    public sealed class ThirdPartyNoticePointerOwnership : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, ICancelHandler
    {
        readonly HashSet<int> pointers = new();
        bool initialized;

        public int OwnedPointerCount => pointers.Count;
        public void Initialize() => initialized = true;
        public void OnPointerDown(PointerEventData eventData) { if (!initialized) return; pointers.Add(eventData.pointerId); GameplayInput.ClaimUiPointer(eventData.pointerId); }
        public void OnPointerUp(PointerEventData eventData) { if (!initialized) return; pointers.Remove(eventData.pointerId); GameplayInput.ReleaseUiPointer(eventData.pointerId); }
        public void OnCancel(BaseEventData eventData) { if (eventData is PointerEventData pointer) Release(pointer.pointerId); else ReleaseAll(); }
        public void ReleaseAll() { foreach (var pointer in pointers) GameplayInput.ReleaseUiPointer(pointer); pointers.Clear(); }
        void Release(int pointer) { pointers.Remove(pointer); GameplayInput.ReleaseUiPointer(pointer); }
        void OnApplicationFocus(bool focus) { if (!focus) ReleaseAll(); }
        void OnDisable() => ReleaseAll();
        void OnDestroy() => ReleaseAll();
    }
}
