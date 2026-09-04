using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.Possession;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace RealmRaiders.UI
{
    public sealed class PrototypeHUD : MonoBehaviour
    {
        PossessionManager possession;
        Text title, hint, selected, heroHp, entHp;
        Button possessButton, releaseButton, attackButton, slamButton;
        Button heroButton, keeperButton;
        Button resetButton;
        CombatEntity hero, ent;

        public void Initialize(PossessionManager manager, SandboxDirector director, CombatEntity heroEntity, CombatEntity entEntity)
        {
            possession = manager; hero = heroEntity; ent = entEntity;
            Build(director);
            manager.SelectionChanged += OnSelection;
            manager.PossessionChanged += OnPossession;
            hero.Health.Changed += (_, _) => RefreshHealth();
            ent.Health.Changed += (_, _) => RefreshHealth();
            RefreshHealth(); OnSelection(null); OnPossession(null);
        }

        void Build(SandboxDirector director)
        {
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            gameObject.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            gameObject.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1080, 1920);
            gameObject.AddComponent<GraphicRaycaster>(); gameObject.AddComponent<ResponsiveHudRoot>().Initialize(true);
            title = Label("REALM RAIDERS — CHARACTER SANDBOX", new Vector2(0, -45), 35, TextAnchor.UpperCenter);
            heroHp = Label("", new Vector2(40, -115), 28, TextAnchor.UpperLeft);
            entHp = Label("", new Vector2(40, -155), 28, TextAnchor.UpperLeft);
            selected = Label("", new Vector2(0, -230), 32, TextAnchor.UpperCenter);
            hint = Label("Tap ENT to select • Swipe to dodge/charge • Tap enemy to attack", new Vector2(0, 55), 25, TextAnchor.LowerCenter, true);
            possessButton = Button("POSSESS", new Vector2(0, 250), () => possession.PossessSelected());
            releaseButton = Button("RELEASE", new Vector2(0, 250), possession.Release);
            attackButton = Button("SMASH", new Vector2(-190, 110), () => Ability(0));
            slamButton = Button("GROUND SLAM", new Vector2(190, 110), () => Ability(2));
            heroButton = Button("PLAY HERO", new Vector2(-190, 370), director.EnterHero);
            keeperButton = Button("KEEPER", new Vector2(190, 370), director.EnterKeeper);
            resetButton = Button("RESET", new Vector2(0, 490), () => SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex));
            Button("MY REALM", new Vector2(0, 610), () => SceneManager.LoadScene("PrototypeHub"));
        }

        void Ability(int index)
        { if (possession.Possessed) possession.Possessed.Controller<PlayerController>()?.UseAbility(index); }
        void OnSelection(CombatEntity value)
        { selected.text = value ? $"Selected: {value.Definition.DisplayName}" : "Keeper Overview"; possessButton.gameObject.SetActive(value && !possession.IsPossessing); }
        void OnPossession(CombatEntity value)
        {
            bool active = value;
            releaseButton.gameObject.SetActive(active); attackButton.gameObject.SetActive(active); slamButton.gameObject.SetActive(active);
            hint.text = active ? "Tap ground to move • Tap Blood Knight or SMASH • Swipe to CHARGE" : "Tap ENT to select, then POSSESS";
            if (!active) possessButton.gameObject.SetActive(false);
        }
        void RefreshHealth()
        { heroHp.text = $"Blood Knight  {hero.Health.Current:0}/{hero.Health.Maximum:0} HP"; entHp.text = $"Ent  {ent.Health.Current:0}/{ent.Health.Maximum:0} HP"; }

        Text Label(string value, Vector2 position, int size, TextAnchor anchor, bool bottom = false)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform; rect.anchorMin = bottom ? new Vector2(0, 0) : new Vector2(0, 1); rect.anchorMax = bottom ? new Vector2(1, 0) : new Vector2(1, 1);
            rect.pivot = bottom ? new Vector2(.5f, 0) : new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(0, 70);
            var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = anchor; text.color = Color.white;
            return text;
        }
        Button Button(string value, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiPointerOwnership)); go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(320, 100);
            go.GetComponent<Image>().color = new Color(.35f, .12f, .08f, .95f); var button = go.GetComponent<Button>(); button.onClick.AddListener(action);
            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text)); textGo.transform.SetParent(go.transform, false); var textRect = (RectTransform)textGo.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero;
            var label = textGo.GetComponent<Text>(); label.text = value; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 30; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white;
            return button;
        }
    }
}
