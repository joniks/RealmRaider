using RealmRaiders.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    public sealed class HubHUD : MonoBehaviour
    {
        Text title, subtitle, selected, realmStores, orientationTitle, controlTitle;
        Text journey, prototypeRoutes, orientationHelp, controlHelp;
        RawImage guardianEntHero;
        Button skipGuide, noticesButton;
        ResponsiveHudRoot responsive;
        HudPresentation presentation;
        ThirdPartyNoticesPanel noticesPanel;
        readonly Dictionary<string, Button> buttons = new();
        public string RealmStoresText => realmStores ? realmStores.text : string.Empty;
        public bool GuideSkipVisible => skipGuide && skipGuide.gameObject.activeSelf;
        public Button NoticesButton => noticesButton;
        public ThirdPartyNoticesPanel NoticesPanel => noticesPanel;
        public void Initialize() { PrototypeJourney.Cancel(); FirstPlayableMinute.ResetBuildHandoff(); Build(); Refresh(); }

        void Build()
        {
            presentation = gameObject.AddComponent<HudPresentation>();
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); gameObject.AddComponent<GraphicRaycaster>(); guardianEntHero = HeroArt(); responsive = gameObject.AddComponent<ResponsiveHudRoot>(); responsive.LayoutChanged += ApplyHubLayout;
            title = Label("REALM RAIDERS", new Vector2(0, -180), 62, TextAnchor.UpperCenter); subtitle = Label("Prototype Hub", new Vector2(0, -300), 30, TextAnchor.UpperCenter);
            selected = Label("", new Vector2(0, -410), 26, TextAnchor.UpperCenter, 36); realmStores = Label("", new Vector2(0, -590), 22, TextAnchor.UpperCenter, 36); realmStores.name = "Realm Stores"; realmStores.raycastTarget = false; orientationTitle = Label("ORIENTATION", new Vector2(0, -480), 24, TextAnchor.UpperCenter, 44);
            orientationHelp = Label("AUTO follows device rotation", new Vector2(0, -530), 20, TextAnchor.UpperCenter, 36); controlTitle = Label("CONTROL STYLE", new Vector2(0, -720), 24, TextAnchor.UpperCenter, 44);
            controlHelp = Label("CONTEXTUAL: fingertap in portrait • joystick in landscape", new Vector2(0, -790), 20, TextAnchor.UpperCenter, 36); journey = Label(JourneyExplanation, new Vector2(0, -970), 22, TextAnchor.UpperCenter, 50); prototypeRoutes = Label("PROTOTYPE ROUTES", new Vector2(0, -1180), 24, TextAnchor.UpperCenter, 44);
            Button("AUTO", new Vector2(-230, 1300), () => ChooseOrientation("Auto")); Button("PORTRAIT", new Vector2(0, 1300), () => ChooseOrientation("Portrait")); Button("LANDSCAPE", new Vector2(230, 1300), () => ChooseOrientation("Landscape"));
            Button("CONTEXTUAL", new Vector2(-230, 1030), () => ChooseControl("Contextual")); Button("FINGERTAP", new Vector2(0, 1030), () => ChooseControl("Fingertap")); Button("JOYSTICK", new Vector2(230, 1030), () => ChooseControl("Joystick"));
            Button("START SYLVAN JOURNEY", new Vector2(0, 820), StartJourney);
            Button("BUILD SYLVAN", new Vector2(0, 630), () => SelectAndLoad("Sylvan", "RealmBuild")); Button("DEFEND SYLVAN", new Vector2(0, 505), () => SelectAndLoad("Sylvan", "DefenderTest")); Button("RAID SYLVAN", new Vector2(0, 380), () => SelectAndLoad("Sylvan", "SylvanRealm")); Button("DEFEND INFERNAL", new Vector2(0, 255), () => SelectAndLoad("Infernal", "InfernalRealm")); Button("CHARACTER SANDBOX", new Vector2(0, 130), () => CancelAndLoad("CharacterSandbox"));
            if (FirstPlayableMinute.Load() == FirstPlayableMinuteStatus.Active) skipGuide = Button("SKIP GUIDE", Vector2.zero, SkipGuide);
            noticesButton = Button("THIRD-PARTY NOTICES", Vector2.zero, OpenNotices);
            responsive.Initialize(false); ApplyHubLayout(responsive.Orientation);
            var panelObject = new GameObject("Third-Party Notices Panel", typeof(RectTransform), typeof(ThirdPartyNoticesPanel)); panelObject.transform.SetParent(transform, false);
            noticesPanel = panelObject.GetComponent<ThirdPartyNoticesPanel>(); noticesPanel.Initialize(responsive, noticesButton, presentation, ThirdPartyNoticeCatalogue.Release);
        }

        public const string JourneyScene = "RealmBuild";
        public const string JourneyExplanation = "1. BUILD DEFENCES  →  2. RAID THE ENEMY  →  3. DEFEND YOUR REALM";
        public static string DestinationForButton(string buttonName) => buttonName switch
        {
            "START SYLVAN JOURNEY" => JourneyScene, "BUILD SYLVAN" => "RealmBuild", "DEFEND SYLVAN" => "DefenderTest",
            "RAID SYLVAN" => "SylvanRealm", "DEFEND INFERNAL" => "InfernalRealm", "CHARACTER SANDBOX" => "CharacterSandbox", _ => null
        };
        public static bool ActivatesGuideForButton(string buttonName) => buttonName == "START SYLVAN JOURNEY";

        RawImage HeroArt()
        {
            var texture = Resources.Load<Texture2D>("Art/GuardianEntHero"); if (!texture) return null;
            var go = new GameObject("Guardian Ent Hero", typeof(RectTransform), typeof(RawImage)); go.transform.SetParent(transform, false); go.transform.SetAsFirstSibling();
            var image = go.GetComponent<RawImage>(); image.texture = texture; image.raycastTarget = false; return image;
        }

        void ApplyHeroArtLayout(PrototypeOrientation orientation)
        {
            if (!guardianEntHero) return;
            var rect = guardianEntHero.rectTransform;
            if (orientation == PrototypeOrientation.Landscape)
            {
                rect.anchorMin = new Vector2(.02f, .06f); rect.anchorMax = new Vector2(.48f, .94f); rect.offsetMin = rect.offsetMax = Vector2.zero;
                guardianEntHero.color = Color.white;
            }
            else
            {
                rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
                guardianEntHero.color = new Color(1, 1, 1, .52f);
            }
        }

        void ApplyHubLayout(PrototypeOrientation orientation)
        {
            ApplyHeroArtLayout(orientation);
            var labels = new[] { title, subtitle, selected, realmStores, orientationTitle, orientationHelp, controlTitle, controlHelp, journey, prototypeRoutes };
            var ys = orientation == PrototypeOrientation.Landscape ? new[] { -55f, -135f, -185f, -230f, -280f, -330f, -390f, -440f, -505f, -590f } : new[] { -150f, -260f, -340f, -395f, -465f, -515f, -665f, -725f, -920f, -1140f };
            var anchor = .25f;
            var width = orientation == PrototypeOrientation.Landscape ? 600 : 480;
            var heights = orientation == PrototypeOrientation.Landscape ? new[] { 75f, 36f, 36f, 36f, 44f, 36f, 44f, 36f, 60f, 44f } : new[] { 90f, 50f, 36f, 36f, 44f, 36f, 44f, 50f, 70f, 44f };
            for (var i = 0; i < labels.Length; i++) { var rect = labels[i].rectTransform; rect.anchorMin = rect.anchorMax = new Vector2(anchor, 1); rect.anchoredPosition = new Vector2(0, ys[i]); rect.sizeDelta = new Vector2(width, heights[i]); }
            ApplyButtonLayout(orientation);
        }

        void ApplyButtonLayout(PrototypeOrientation orientation)
        {
            if (buttons.Count == 0) return;
            if (orientation == PrototypeOrientation.Landscape)
            {
                Place("AUTO", new Vector2(-480, 640), new Vector2(150, 80)); Place("PORTRAIT", new Vector2(-280, 640), new Vector2(150, 80)); Place("LANDSCAPE", new Vector2(-80, 640), new Vector2(150, 80));
                Place("CONTEXTUAL", new Vector2(-480, 540), new Vector2(150, 80)); Place("FINGERTAP", new Vector2(-280, 540), new Vector2(150, 80)); Place("JOYSTICK", new Vector2(-80, 540), new Vector2(150, 80));
                Place("START SYLVAN JOURNEY", new Vector2(-20, 430), new Vector2(430, 85)); Place("BUILD SYLVAN", new Vector2(-20, 330), new Vector2(430, 75)); Place("DEFEND SYLVAN", new Vector2(-20, 245), new Vector2(430, 75));
                Place("RAID SYLVAN", new Vector2(-20, 160), new Vector2(430, 75)); Place("DEFEND INFERNAL", new Vector2(-20, 75), new Vector2(430, 75)); Place("CHARACTER SANDBOX", new Vector2(-20, 0), new Vector2(430, 65));
                Place("SKIP GUIDE", new Vector2(-480, 0), new Vector2(300, 96));
                Place("THIRD-PARTY NOTICES", new Vector2(-24, -24), new Vector2(320, 72), Vector2.one, Vector2.one);
                return;
            }

            var portraitColumn = new Vector2(.58f, 0);
            Place("AUTO", new Vector2(0, 1235), new Vector2(140, 105), portraitColumn, Vector2.zero); Place("PORTRAIT", new Vector2(155, 1235), new Vector2(140, 105), portraitColumn, Vector2.zero); Place("LANDSCAPE", new Vector2(310, 1235), new Vector2(140, 105), portraitColumn, Vector2.zero);
            Place("CONTEXTUAL", new Vector2(0, 930), new Vector2(140, 105), portraitColumn, Vector2.zero); Place("FINGERTAP", new Vector2(155, 930), new Vector2(140, 105), portraitColumn, Vector2.zero); Place("JOYSTICK", new Vector2(310, 930), new Vector2(140, 105), portraitColumn, Vector2.zero);
            Place("START SYLVAN JOURNEY", new Vector2(0, 770), new Vector2(360, 120), portraitColumn, Vector2.zero); Place("BUILD SYLVAN", new Vector2(0, 580), new Vector2(360, 105), portraitColumn, Vector2.zero);
            Place("DEFEND SYLVAN", new Vector2(0, 440), new Vector2(360, 105), portraitColumn, Vector2.zero); Place("RAID SYLVAN", new Vector2(0, 300), new Vector2(360, 105), portraitColumn, Vector2.zero);
            Place("DEFEND INFERNAL", new Vector2(0, 160), new Vector2(360, 105), portraitColumn, Vector2.zero); Place("CHARACTER SANDBOX", new Vector2(0, 20), new Vector2(360, 105), portraitColumn, Vector2.zero);
            Place("SKIP GUIDE", new Vector2(-320, 20), new Vector2(280, 96), portraitColumn, Vector2.zero);
            Place("THIRD-PARTY NOTICES", new Vector2(24, -24), new Vector2(360, 88), new Vector2(0, 1), new Vector2(0, 1));
        }

        void Place(string name, Vector2 position, Vector2 size, Vector2? anchor = null, Vector2? pivot = null)
        {
            if (!buttons.TryGetValue(name, out var button)) return;
            var rect = button.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = anchor ?? new Vector2(1, 0); rect.pivot = pivot ?? new Vector2(1, 0); rect.anchoredPosition = position; rect.sizeDelta = size;
        }

        void SelectAndLoad(string realm, string scene) { PrototypeJourney.Cancel(); PrototypeSave.SelectRealm(realm); SceneManager.LoadScene(scene); }
        void CancelAndLoad(string scene) { PrototypeJourney.Cancel(); SceneManager.LoadScene(scene); }
        void StartJourney()
        {
            if (!PrototypeJourney.TryStart(out _)) return;
            FirstPlayableMinute.TryStart();
            PrototypeSave.SelectRealm("Sylvan");
            SceneManager.LoadScene(JourneyScene);
        }
        void OpenNotices() => noticesPanel?.Open();
        void SkipGuide()
        {
            if (!FirstPlayableMinute.Skip()) return;
            skipGuide.onClick.RemoveAllListeners();
            skipGuide.gameObject.SetActive(false);
        }
        void ChooseOrientation(string value) { PrototypeSave.SetOrientation(value); Refresh(); }
        void ChooseControl(string value) { PrototypeSave.SetControlStyle(value); Refresh(); }
        void Refresh()
        {
            selected.text = $"Selected realm: {PrototypeSave.SelectedRealm} • {PrototypeSave.OrientationPreference} • {PrototypeSave.ControlStylePreference}";
            presentation.DecorateRealmLabel(selected, PrototypeSave.SelectedRealm);
            realmStores.text = RealmProgress.StoreCopy();
        }
        Text Label(string value, Vector2 position, int size, TextAnchor anchor, float height = 90)
        { var go = new GameObject("Label " + value, typeof(RectTransform), typeof(Text)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1); rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(950, height); var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = anchor; text.color = Color.white; return text; }
        Button Button(string value, Vector2 position, UnityEngine.Events.UnityAction action)
        { var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiPointerOwnership)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(value == "AUTO" || value == "PORTRAIT" || value == "LANDSCAPE" ? 180 : 620, 105); if (value == "CONTEXTUAL" || value == "FINGERTAP" || value == "JOYSTICK") rect.sizeDelta = new Vector2(180, 105); if (value == "START SYLVAN JOURNEY") { rect.sizeDelta = new Vector2(760, 120); go.GetComponent<Image>().color = new Color(.22f, .55f, .3f, .98f); } else go.GetComponent<Image>().color = new Color(.12f, .3f, .18f, .96f); var button = go.GetComponent<Button>(); buttons[value] = button; presentation?.ApplyButton(go.GetComponent<Image>()); button.onClick.AddListener(action); button.onClick.AddListener(() => presentation?.PlayClick()); var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text)); textGo.transform.SetParent(go.transform, false); var textRect = (RectTransform)textGo.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero; var label = textGo.GetComponent<Text>(); label.text = value; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = value == "START SYLVAN JOURNEY" ? 36 : value == "THIRD-PARTY NOTICES" ? 24 : 32; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; return button; }
    }
}
