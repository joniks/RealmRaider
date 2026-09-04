using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using RealmRaiders.Possession;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using RealmRaiders.Traps;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    public readonly struct DefenseHudConfig
    {
        public readonly string RealmTitle;
        public readonly string DefenderName;
        public readonly string CoreName;
        public readonly string TrapName;
        public readonly string RetryScene;
        public readonly string NextActionLabel;
        public readonly string NextActionScene;

        public DefenseHudConfig(string realmTitle, string defenderName, string coreName, string trapName, string retryScene, string nextActionLabel, string nextActionScene)
        {
            RealmTitle = realmTitle;
            DefenderName = defenderName;
            CoreName = coreName;
            TrapName = trapName;
            RetryScene = retryScene;
            NextActionLabel = nextActionLabel;
            NextActionScene = nextActionScene;
        }

        public static DefenseHudConfig Sylvan => new("SYLVAN DEFENSE", "Ent", "Heart Tree", "Root Trap", "DefenderTest", "PLAY SYLVAN RAID", "SylvanRealm");
        public static DefenseHudConfig Infernal => new("INFERNAL DEFENSE", "Brute", "Infernal Heart", "Flame Trap", "InfernalRealm", "PLAY SYLVAN RAID", "SylvanRealm");
    }

    public sealed class DefenderHUD : MonoBehaviour
    {
        Text state, invaderHealth, entHealth, energyText, selection, trapText, coreText, result;
        Button possess, release, smash, slam;
        GameObject resultPanel;
        PossessionManager possessionManager;
        PossessionEnergy energy;
        DefenseManager defense;
        CombatEntity invader, ent;
        TrapBase trap;
        DefenseHudConfig config;

        public void Initialize(DefenseManager defenseManager, PossessionManager manager, PossessionEnergy possessionEnergy, CombatEntity raidInvader, CombatEntity defender, TrapBase rootTrap, RealmCore core, DefenseHudConfig hudConfig)
        {
            defense = defenseManager; possessionManager = manager; energy = possessionEnergy; invader = raidInvader; ent = defender; trap = rootTrap; config = hudConfig;
            Build();
            manager.SelectionChanged += OnSelection; manager.PossessionChanged += OnPossession;
            defenseManager.StateChanged += OnDefenseState; possessionEnergy.Changed += (_, _) => Refresh(); core.ProgressChanged += value => coreText.text = $"{config.CoreName} danger: {value * 100:0}%";
            OnSelection(null); OnPossession(null); OnDefenseState(defenseManager.State); Refresh();
        }

        void Update() { Refresh(); }

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); gameObject.AddComponent<GraphicRaycaster>();
            state = Label(config.RealmTitle, new Vector2(0, -40), 38, TextAnchor.UpperCenter);
            invaderHealth = Label("", new Vector2(35, -105), 27, TextAnchor.UpperLeft); entHealth = Label("", new Vector2(35, -145), 27, TextAnchor.UpperLeft); energyText = Label("", new Vector2(35, -185), 27, TextAnchor.UpperLeft);
            coreText = Label($"{config.CoreName} danger: 0%", new Vector2(0, -235), 28, TextAnchor.UpperCenter); selection = Label($"Tap the {config.DefenderName} to select it", new Vector2(0, -285), 28, TextAnchor.UpperCenter); trapText = Label("", new Vector2(0, 52), 23, TextAnchor.LowerCenter, true);
            possess = Button($"POSSESS {config.DefenderName.ToUpperInvariant()}", new Vector2(0, 410), () => possessionManager.PossessSelected());
            release = Button("RELEASE", new Vector2(0, 410), possessionManager.Release);
            Button("ACTIVATE TRAP", new Vector2(0, 290), ActivateTrap);
            smash = Button("SMASH", new Vector2(-180, 165), () => Ability(0)); slam = Button("GROUND SLAM", new Vector2(180, 165), () => Ability(2));
            resultPanel = new GameObject("Defense Result", typeof(RectTransform), typeof(Image)); resultPanel.transform.SetParent(transform, false); var rect = (RectTransform)resultPanel.transform; rect.anchorMin = new Vector2(.08f, .28f); rect.anchorMax = new Vector2(.92f, .72f); rect.offsetMin = rect.offsetMax = Vector2.zero; resultPanel.GetComponent<Image>().color = new Color(.025f, .06f, .035f, .97f);
            result = Label("", Vector2.zero, 42, TextAnchor.MiddleCenter); result.transform.SetParent(resultPanel.transform, false); var resultRect = (RectTransform)result.transform; resultRect.anchorMin = new Vector2(0, .35f); resultRect.anchorMax = Vector2.one; resultRect.offsetMin = resultRect.offsetMax = Vector2.zero;
            var retry = Button("DEFEND AGAIN", new Vector2(0, 160), () => SceneManager.LoadScene(config.RetryScene)); retry.transform.SetParent(resultPanel.transform, false); var raid = Button(config.NextActionLabel, new Vector2(0, 48), () => SceneManager.LoadScene(config.NextActionScene)); raid.transform.SetParent(resultPanel.transform, false); resultPanel.SetActive(false);
            var hub = Button("MY REALM", new Vector2(0, -64), () => SceneManager.LoadScene("PrototypeHub")); hub.transform.SetParent(resultPanel.transform, false);
        }

        void ActivateTrap() { trap.TryActivate(); Refresh(); }
        void Ability(int index) => possessionManager.Possessed?.Controller<PlayerController>()?.UseAbility(index);
        void OnSelection(CombatEntity value)
        { selection.text = value ? $"Selected: {value.Definition.DisplayName}" : $"Tap the {config.DefenderName} to select it"; possess.gameObject.SetActive(value && !possessionManager.IsPossessing && !energy.IsDepleted); }
        void OnPossession(CombatEntity value)
        {
            bool active = value; release.gameObject.SetActive(active); smash.gameObject.SetActive(active); slam.gameObject.SetActive(active);
            if (!active) possess.gameObject.SetActive(false);
            selection.text = active ? $"YOU ARE THE {config.DefenderName.ToUpperInvariant()}" : $"Tap the {config.DefenderName} to select it";
        }
        void OnDefenseState(DefenseState value)
        {
            state.text = value switch { DefenseState.Possessing => "POSSESSED CREATURE", DefenseState.DefenderVictory => "DEFENSE COMPLETE", DefenseState.RealmLost => "REALM BREACHED", _ => "KEEPER OVERVIEW" };
            if (value is DefenseState.DefenderVictory or DefenseState.RealmLost)
            { resultPanel.SetActive(true); result.text = value == DefenseState.DefenderVictory ? "DEFENDER VICTORY\n\nThe invader was destroyed." : $"REALM LOST\n\nThe {config.CoreName} was captured."; }
        }
        void Refresh()
        {
            if (!invader || !ent) return;
            invaderHealth.text = $"Invader  {invader.Health.Current:0}/{invader.Health.Maximum:0} HP"; entHealth.text = $"{config.DefenderName}  {ent.Health.Current:0}/{ent.Health.Maximum:0} HP";
            energyText.text = $"Possession energy  {energy.Remaining:0.0}/{energy.Maximum:0}s"; trapText.text = trap.State == TrapState.Ready ? $"{config.TrapName} ready — activate when the invader is on it" : $"{config.TrapName}: {trap.State}";
        }

        Text Label(string value, Vector2 position, int size, TextAnchor anchor, bool bottom = false)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = bottom ? new Vector2(0, 0) : new Vector2(0, 1); rect.anchorMax = bottom ? new Vector2(1, 0) : new Vector2(1, 1); rect.pivot = bottom ? new Vector2(.5f, 0) : new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(0, 70); var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = anchor; text.color = Color.white; return text;
        }
        Button Button(string value, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(340, 96); go.GetComponent<Image>().color = new Color(.12f, .38f, .17f, .96f); var button = go.GetComponent<Button>(); button.onClick.AddListener(action); var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text)); textObject.transform.SetParent(go.transform, false); var textRect = (RectTransform)textObject.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero; var label = textObject.GetComponent<Text>(); label.text = value; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 25; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; return button;
        }
    }
}
