using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    public sealed class HubHUD : MonoBehaviour
    {
        Text title, subtitle, selected, orientationTitle, controlTitle;
        Text journey, prototypeRoutes, orientationHelp, controlHelp;
        RawImage guardianEntHero;
        ResponsiveHudRoot responsive;
        HudPresentation presentation;
        public void Initialize() { Build(); Refresh(); }

        void Build()
        {
            presentation = gameObject.AddComponent<HudPresentation>();
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); gameObject.AddComponent<GraphicRaycaster>(); guardianEntHero = HeroArt(); responsive = gameObject.AddComponent<ResponsiveHudRoot>(); responsive.LayoutChanged += ApplyHubLayout;
            title = Label("REALM RAIDERS", new Vector2(0, -180), 62, TextAnchor.UpperCenter); subtitle = Label("Prototype Hub", new Vector2(0, -300), 30, TextAnchor.UpperCenter);
            selected = Label("", new Vector2(0, -410), 26, TextAnchor.UpperCenter, 36); orientationTitle = Label("ORIENTATION", new Vector2(0, -480), 24, TextAnchor.UpperCenter, 44);
            orientationHelp = Label("AUTO follows device rotation", new Vector2(0, -530), 20, TextAnchor.UpperCenter, 36); controlTitle = Label("CONTROL STYLE", new Vector2(0, -720), 24, TextAnchor.UpperCenter, 44);
            controlHelp = Label("CONTEXTUAL: fingertap in portrait • joystick in landscape", new Vector2(0, -790), 20, TextAnchor.UpperCenter, 36); journey = Label(JourneyExplanation, new Vector2(0, -970), 22, TextAnchor.UpperCenter, 50); prototypeRoutes = Label("PROTOTYPE ROUTES", new Vector2(0, -1180), 24, TextAnchor.UpperCenter, 44);
            Button("AUTO", new Vector2(-230, 1300), () => ChooseOrientation("Auto")); Button("PORTRAIT", new Vector2(0, 1300), () => ChooseOrientation("Portrait")); Button("LANDSCAPE", new Vector2(230, 1300), () => ChooseOrientation("Landscape"));
            Button("CONTEXTUAL", new Vector2(-230, 1030), () => ChooseControl("Contextual")); Button("FINGERTAP", new Vector2(0, 1030), () => ChooseControl("Fingertap")); Button("JOYSTICK", new Vector2(230, 1030), () => ChooseControl("Joystick"));
            Button("START SYLVAN JOURNEY", new Vector2(0, 820), () => SelectAndLoad("Sylvan", JourneyScene));
            Button("BUILD SYLVAN", new Vector2(0, 630), () => SelectAndLoad("Sylvan", "RealmBuild")); Button("DEFEND SYLVAN", new Vector2(0, 505), () => SelectAndLoad("Sylvan", "DefenderTest")); Button("RAID SYLVAN", new Vector2(0, 380), () => SelectAndLoad("Sylvan", "SylvanRealm")); Button("DEFEND INFERNAL", new Vector2(0, 255), () => SelectAndLoad("Infernal", "InfernalRealm")); Button("CHARACTER SANDBOX", new Vector2(0, 130), () => SceneManager.LoadScene("CharacterSandbox"));
            responsive.Initialize(false); ApplyHubLayout(responsive.Orientation);
        }

        public const string JourneyScene = "RealmBuild";
        public const string JourneyExplanation = "1. BUILD DEFENCES  →  2. DEFEND YOUR REALM  →  3. RAID THE ENEMY";
        public static string DestinationForButton(string buttonName) => buttonName switch
        {
            "START SYLVAN JOURNEY" => JourneyScene, "BUILD SYLVAN" => "RealmBuild", "DEFEND SYLVAN" => "DefenderTest",
            "RAID SYLVAN" => "SylvanRealm", "DEFEND INFERNAL" => "InfernalRealm", "CHARACTER SANDBOX" => "CharacterSandbox", _ => null
        };

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
            var labels = new[] { title, subtitle, selected, orientationTitle, orientationHelp, controlTitle, controlHelp, journey, prototypeRoutes };
            var ys = orientation == PrototypeOrientation.Landscape ? new[] { -70f, -190f, -300f, -410f, -460f, -540f, -590f, -670f, -760f } : new[] { -180f, -300f, -410f, -480f, -530f, -720f, -790f, -970f, -1180f };
            var anchor = orientation == PrototypeOrientation.Landscape ? .25f : .5f;
            var width = orientation == PrototypeOrientation.Landscape ? 820 : 950;
            for (var i = 0; i < labels.Length; i++) { var rect = labels[i].rectTransform; rect.anchorMin = rect.anchorMax = new Vector2(anchor, 1); rect.anchoredPosition = new Vector2(0, ys[i]); rect.sizeDelta = new Vector2(width, rect.sizeDelta.y); }
        }

        void SelectAndLoad(string realm, string scene) { PrototypeSave.SelectRealm(realm); SceneManager.LoadScene(scene); }
        void ChooseOrientation(string value) { PrototypeSave.SetOrientation(value); Refresh(); }
        void ChooseControl(string value) { PrototypeSave.SetControlStyle(value); Refresh(); }
        void Refresh() => selected.text = $"Selected realm: {PrototypeSave.SelectedRealm} • {PrototypeSave.OrientationPreference} • {PrototypeSave.ControlStylePreference}";
        Text Label(string value, Vector2 position, int size, TextAnchor anchor, float height = 90)
        { var go = new GameObject("Label " + value, typeof(RectTransform), typeof(Text)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1); rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(950, height); var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = anchor; text.color = Color.white; return text; }
        Button Button(string value, Vector2 position, UnityEngine.Events.UnityAction action)
        { var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiPointerOwnership)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(value == "AUTO" || value == "PORTRAIT" || value == "LANDSCAPE" ? 180 : 620, 105); if (value == "CONTEXTUAL" || value == "FINGERTAP" || value == "JOYSTICK") rect.sizeDelta = new Vector2(180, 105); if (value == "START SYLVAN JOURNEY") { rect.sizeDelta = new Vector2(760, 120); go.GetComponent<Image>().color = new Color(.22f, .55f, .3f, .98f); } else go.GetComponent<Image>().color = new Color(.12f, .3f, .18f, .96f); var button = go.GetComponent<Button>(); presentation?.ApplyButton(go.GetComponent<Image>()); button.onClick.AddListener(action); button.onClick.AddListener(() => presentation?.PlayClick()); var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text)); textGo.transform.SetParent(go.transform, false); var textRect = (RectTransform)textGo.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero; var label = textGo.GetComponent<Text>(); label.text = value; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = value == "START SYLVAN JOURNEY" ? 36 : 32; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; return button; }
    }
}
