using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using RealmRaiders.Raid;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    public sealed class RaidHUD : MonoBehaviour
    {
        Text state, health, stats, objective, result, rootPrompt;
        GameObject resultPanel;
        CombatEntity hero;
        RaidManager raid;

        public void Initialize(RaidManager manager, CombatEntity raidHero)
        {
            raid = manager; hero = raidHero; Build();
            manager.StateChanged += OnState; manager.Finished += ShowResult;
            hero.Health.Changed += (_, _) => Refresh();
            Refresh(); OnState(manager.State);
        }

        void Update() { if (raid) Refresh(); var controller = hero ? hero.Controller<PlayerController>() : null; if (rootPrompt) { var rooted = controller && controller.IsActive && controller.RootEscapeVisible && !GameplayInput.TerminalState; rootPrompt.gameObject.SetActive(rooted); if (rooted) rootPrompt.text = controller.RootEscapeProgress >= 5 ? "BREAK FREE" : $"ROOTED — TAP TO BREAK FREE\n{controller.RootEscapeProgress}/5"; } }

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920);
            gameObject.AddComponent<GraphicRaycaster>(); gameObject.AddComponent<ResponsiveHudRoot>().Initialize(true);
            state = Label("SYLVAN RAID", new Vector2(0, -40), 38, TextAnchor.UpperCenter);
            health = Label("", new Vector2(35, -105), 28, TextAnchor.UpperLeft);
            stats = Label("", new Vector2(35, -150), 25, TextAnchor.UpperLeft);
            objective = Label("Reach the Heart Tree", new Vector2(0, -205), 28, TextAnchor.UpperCenter);
            rootPrompt = Label("", new Vector2(0, 350), 36, TextAnchor.MiddleCenter, true); rootPrompt.gameObject.SetActive(false);
            Label("Tap ground: move • Tap enemy: attack • Swipe: Blood Rush", new Vector2(0, 45), 23, TextAnchor.LowerCenter, true);
            Button("SLASH", new Vector2(-260, 110), () => Ability(0));
            Button("BLOOD RUSH", new Vector2(0, 110), () => Ability(1));
            Button("CLEAVE", new Vector2(260, 110), () => Ability(2));
            resultPanel = new GameObject("Raid Result", typeof(RectTransform), typeof(Image)); resultPanel.transform.SetParent(transform, false);
            var rect = (RectTransform)resultPanel.transform; rect.anchorMin = new Vector2(.08f, .24f); rect.anchorMax = new Vector2(.92f, .76f); rect.offsetMin = rect.offsetMax = Vector2.zero;
            resultPanel.GetComponent<Image>().color = new Color(.025f, .06f, .035f, .97f);
            result = Label("", new Vector2(0, -560), 32, TextAnchor.UpperCenter); result.transform.SetParent(resultPanel.transform, false); var resultRect = (RectTransform)result.transform; resultRect.anchorMin = new Vector2(0, 1); resultRect.anchorMax = new Vector2(1, 1); resultRect.anchoredPosition = new Vector2(0, -55); resultRect.sizeDelta = new Vector2(0, 430);
            var again = Button("RAID AGAIN", new Vector2(0, 170), () => SceneManager.LoadScene("SylvanRealm")); again.transform.SetParent(resultPanel.transform, false);
            var back = Button("CHARACTER SANDBOX", new Vector2(0, 55), () => SceneManager.LoadScene("CharacterSandbox")); back.transform.SetParent(resultPanel.transform, false);
            var hub = Button("MY REALM", new Vector2(0, -60), () => SceneManager.LoadScene("PrototypeHub")); hub.transform.SetParent(resultPanel.transform, false);
            resultPanel.SetActive(false);
        }

        void Ability(int index) => hero.Controller<PlayerController>()?.UseAbility(index);
        public void SetObjectiveProgress(float progress) => objective.text = progress > 0 ? $"Capturing Heart Tree  {progress * 100:0}%" : "Reach the Heart Tree";
        void OnState(RaidState value) => state.text = $"SYLVAN RAID — {value}";
        void Refresh()
        {
            health.text = $"Blood Knight  {hero.Health.Current:0}/{hero.Health.Maximum:0} HP";
            stats.text = $"Gold {raid.Gold}   Enemies {raid.EnemiesDefeated}   Rooms {raid.RoomsDiscovered}   {raid.Duration:0}s";
        }
        void ShowResult(RaidResult value)
        {
            GameplayInput.SetTerminalState(true);
            resultPanel.SetActive(true);
            result.text = $"{(value.Victory ? "VICTORY" : "DEFEAT")}\n\nGold collected: {value.Gold}\nRare materials: {value.RareMaterials}\nEnemies defeated: {value.EnemiesDefeated}\nRooms discovered: {value.RoomsDiscovered}\nRaid duration: {value.Duration:0}s\nCore reached: {(value.CoreReached ? "yes" : "no")}";
        }

        Text Label(string value, Vector2 position, int size, TextAnchor anchor, bool bottom = false)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform; rect.anchorMin = bottom ? new Vector2(0, 0) : new Vector2(0, 1); rect.anchorMax = bottom ? new Vector2(1, 0) : new Vector2(1, 1); rect.pivot = bottom ? new Vector2(.5f, 0) : new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(0, bottom ? 60 : 70);
            var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = anchor; text.color = Color.white; return text;
        }
        Button Button(string value, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiPointerOwnership)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(240, 92);
            go.GetComponent<Image>().color = new Color(.12f, .38f, .17f, .96f); var button = go.GetComponent<Button>(); button.onClick.AddListener(action);
            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text)); textGo.transform.SetParent(go.transform, false); var textRect = (RectTransform)textGo.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero; var label = textGo.GetComponent<Text>(); label.text = value; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 25; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; return button;
        }
    }
}
