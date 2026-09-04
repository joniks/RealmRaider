using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    public sealed class HubHUD : MonoBehaviour
    {
        Text selected;
        RawImage guardianEntHero;
        ResponsiveHudRoot responsive;
        public void Initialize() { Build(); Refresh(); }

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); gameObject.AddComponent<GraphicRaycaster>(); guardianEntHero = HeroArt(); responsive = gameObject.AddComponent<ResponsiveHudRoot>(); responsive.LayoutChanged += ApplyHeroArtLayout; responsive.Initialize(false); ApplyHeroArtLayout(responsive.Orientation);
            Label("REALM RAIDERS", new Vector2(0, -170), 62, TextAnchor.UpperCenter); Label("Prototype Hub", new Vector2(0, -255), 30, TextAnchor.UpperCenter);
            selected = Label("", new Vector2(0, -370), 30, TextAnchor.UpperCenter); Label("ORIENTATION", new Vector2(0, -455), 24, TextAnchor.UpperCenter); Label("CONTROL STYLE", new Vector2(0, -535), 24, TextAnchor.UpperCenter);
            Button("AUTO", new Vector2(-230, 1500), () => ChooseOrientation("Auto")); Button("PORTRAIT", new Vector2(0, 1500), () => ChooseOrientation("Portrait")); Button("LANDSCAPE", new Vector2(230, 1500), () => ChooseOrientation("Landscape"));
            Button("CONTEXTUAL", new Vector2(-230, 1340), () => ChooseControl("Contextual")); Button("FINGERTAP", new Vector2(0, 1340), () => ChooseControl("Fingertap")); Button("JOYSTICK", new Vector2(230, 1340), () => ChooseControl("Joystick"));
            Button("BUILD SYLVAN", new Vector2(0, 1160), () => SelectAndLoad("Sylvan", "RealmBuild"));
            Button("DEFEND SYLVAN", new Vector2(0, 980), () => SelectAndLoad("Sylvan", "DefenderTest"));
            Button("RAID SYLVAN", new Vector2(0, 800), () => SelectAndLoad("Sylvan", "SylvanRealm"));
            Button("DEFEND INFERNAL", new Vector2(0, 620), () => SelectAndLoad("Infernal", "InfernalRealm"));
            Button("CHARACTER SANDBOX", new Vector2(0, 440), () => SceneManager.LoadScene("CharacterSandbox"));
        }

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

        void SelectAndLoad(string realm, string scene) { PrototypeSave.SelectRealm(realm); SceneManager.LoadScene(scene); }
        void ChooseOrientation(string value) { PrototypeSave.SetOrientation(value); Refresh(); }
        void ChooseControl(string value) { PrototypeSave.SetControlStyle(value); Refresh(); }
        void Refresh() => selected.text = $"Selected realm: {PrototypeSave.SelectedRealm} • {PrototypeSave.OrientationPreference} • {PrototypeSave.ControlStylePreference}";
        Text Label(string value, Vector2 position, int size, TextAnchor anchor)
        { var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1); rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(950, 90); var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = anchor; text.color = Color.white; return text; }
        Button Button(string value, Vector2 position, UnityEngine.Events.UnityAction action)
        { var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiPointerOwnership)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(value == "AUTO" || value == "PORTRAIT" || value == "LANDSCAPE" ? 180 : 620, 105); if (value == "CONTEXTUAL" || value == "FINGERTAP" || value == "JOYSTICK") rect.sizeDelta = new Vector2(180, 105); go.GetComponent<Image>().color = new Color(.12f, .3f, .18f, .96f); var button = go.GetComponent<Button>(); button.onClick.AddListener(action); var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text)); textGo.transform.SetParent(go.transform, false); var textRect = (RectTransform)textGo.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero; var label = textGo.GetComponent<Text>(); label.text = value; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 32; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; return button; }
    }
}
