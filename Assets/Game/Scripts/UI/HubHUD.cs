using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    public sealed class HubHUD : MonoBehaviour
    {
        Text selected;
        public void Initialize() { Build(); Refresh(); }

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); gameObject.AddComponent<GraphicRaycaster>();
            Label("REALM RAIDERS", new Vector2(0, -170), 62, TextAnchor.UpperCenter); Label("Prototype Hub", new Vector2(0, -255), 30, TextAnchor.UpperCenter);
            selected = Label("", new Vector2(0, -370), 30, TextAnchor.UpperCenter);
            Button("DEFEND SYLVAN", new Vector2(0, 560), () => SelectAndLoad("Sylvan", "DefenderTest"));
            Button("RAID SYLVAN", new Vector2(0, 430), () => SelectAndLoad("Sylvan", "SylvanRealm"));
            Button("DEFEND INFERNAL", new Vector2(0, 300), () => SelectAndLoad("Infernal", "InfernalRealm"));
            Button("CHARACTER SANDBOX", new Vector2(0, 170), () => SceneManager.LoadScene("CharacterSandbox"));
        }

        void SelectAndLoad(string realm, string scene) { PrototypeSave.SelectRealm(realm); SceneManager.LoadScene(scene); }
        void Refresh() => selected.text = $"Selected realm: {PrototypeSave.SelectedRealm}";
        Text Label(string value, Vector2 position, int size, TextAnchor anchor)
        { var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1); rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(950, 90); var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = anchor; text.color = Color.white; return text; }
        Button Button(string value, Vector2 position, UnityEngine.Events.UnityAction action)
        { var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(620, 105); go.GetComponent<Image>().color = new Color(.12f, .3f, .18f, .96f); var button = go.GetComponent<Button>(); button.onClick.AddListener(action); var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text)); textGo.transform.SetParent(go.transform, false); var textRect = (RectTransform)textGo.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero; var label = textGo.GetComponent<Text>(); label.text = value; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 32; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; return button; }
    }
}
