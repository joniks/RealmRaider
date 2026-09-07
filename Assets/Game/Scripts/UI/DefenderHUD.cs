using RealmRaiders.Characters;
using RealmRaiders.Possession;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using RealmRaiders.Traps;
using RealmRaiders.Controllers;
using RealmRaiders.CameraSystem;
using RealmRaiders.AI;
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
        public readonly string RouteStart;
        public readonly string RouteGuardLine;
        public readonly string RouteInner;
        public readonly string RouteFinalGuard;

        public DefenseHudConfig(string realmTitle, string defenderName, string coreName, string trapName, string retryScene, string nextActionLabel, string nextActionScene, string routeStart, string routeGuardLine, string routeInner, string routeFinalGuard)
        {
            RealmTitle = realmTitle;
            DefenderName = defenderName;
            CoreName = coreName;
            TrapName = trapName;
            RetryScene = retryScene;
            NextActionLabel = nextActionLabel;
            NextActionScene = nextActionScene;
            RouteStart = routeStart;
            RouteGuardLine = routeGuardLine;
            RouteInner = routeInner;
            RouteFinalGuard = routeFinalGuard;
        }

        public string OpeningRouteStatus => $"INVADER HOLDING — {RouteStart.ToUpperInvariant()} AHEAD";
        public string RouteStatus(int waypointIndex) => waypointIndex switch
        {
            <= 0 => $"INVADER ADVANCING — {RouteStart.ToUpperInvariant()} AHEAD",
            1 => $"INVADER APPROACHING {RouteStart.ToUpperInvariant()}",
            2 => $"{RouteStart.ToUpperInvariant()} — {RouteGuardLine.ToUpperInvariant()} AHEAD",
            3 => $"{RouteInner.ToUpperInvariant()} — {RouteFinalGuard.ToUpperInvariant()} AHEAD",
            4 => $"FINAL APPROACH — {CoreName.ToUpperInvariant()} AHEAD",
            _ => $"INVADER AT {CoreName.ToUpperInvariant()}"
        };

        public static DefenseHudConfig Sylvan => new("SYLVAN DEFENSE", "Ent", "Heart Tree", "Root Trap", "DefenderTest", "RETURN TO BUILD", "RealmBuild", "Root Gate", "Guard Line", "Inner Root", "Heart Guard");
        public static DefenseHudConfig Infernal => new("INFERNAL DEFENSE", "Brute", "Infernal Heart", "Flame Trap", "InfernalRealm", "PLAY SYLVAN RAID", "SylvanRealm", "Flame Trap Line", "Hound Line — Lava Gate", "Lava Gate", "Brute Guard");
    }

    public sealed class DefenderHUD : MonoBehaviour
    {
        Text state, invaderHealth, entHealth, guardianEntVitality, energyText, selection, trapText, coreText, result, rootPrompt, releaseNotice, openingCue, routeStatus, dodgeLabel;
        Image energyFill;
        Button possess, release, smash, slam, activateTrap, dodge;
        GameObject resultPanel;
        PossessionManager possessionManager;
        PossessionEnergy energy;
        DefenseManager defense;
        CombatEntity invader, ent;
        GuardianEntGrowthPresentation guardianEntGrowth;
        TrapBase trap;
        DefenseHudConfig config;
        bool initialized;
        HudPresentation presentation;
        AbilityButtonReadiness[] abilityButtons;
        int displayedOpeningSeconds = -1;
        bool openingCueDismissed;
        int displayedRouteWaypoint = int.MinValue;
        CombatEntity displayedRouteTarget;
        bool displayedRouteOpening;
        bool routeStatusVisible;
        int displayedDodgeCooldownTenths = -1;

        public bool OpeningCueVisible => openingCue && openingCue.gameObject.activeSelf;
        public bool OpeningCueRaycastTarget => openingCue && openingCue.raycastTarget;
        public string RouteStatusText => routeStatus && routeStatus.gameObject.activeSelf ? routeStatus.text : string.Empty;
        public bool RouteStatusRaycastTarget => routeStatus && routeStatus.raycastTarget;
        public string GuardianEntVitalityText => guardianEntVitality && guardianEntVitality.gameObject.activeSelf ? guardianEntVitality.text : string.Empty;
        public bool GuardianEntVitalityRaycastTarget => guardianEntVitality && guardianEntVitality.raycastTarget;
        public RectTransform GuardianEntVitalityRect => guardianEntVitality ? guardianEntVitality.rectTransform : null;
        public RectTransform DefenderHealthRect => entHealth ? entHealth.rectTransform : null;
        public string AbilityButtonText(int index) => abilityButtons != null && index >= 0 && index < abilityButtons.Length ? abilityButtons[index].Text : string.Empty;
        public bool AbilityButtonInteractable(int index) => abilityButtons != null && index >= 0 && index < abilityButtons.Length && abilityButtons[index].IsInteractable;
        public string DodgeButtonText => dodge && dodge.gameObject.activeSelf && dodgeLabel ? dodgeLabel.text : string.Empty;
        public bool DodgeButtonInteractable => dodge && dodge.interactable;
        public bool DodgeButtonVisible => dodge && dodge.gameObject.activeSelf;
        public RectTransform DodgeButtonRect => dodge ? (RectTransform)dodge.transform : null;

        public void Initialize(DefenseManager defenseManager, PossessionManager manager, PossessionEnergy possessionEnergy, CombatEntity raidInvader, CombatEntity defender, TrapBase rootTrap, RealmCore core, DefenseHudConfig hudConfig)
        {
            defense = defenseManager; possessionManager = manager; energy = possessionEnergy; invader = raidInvader; ent = defender; trap = rootTrap; config = hudConfig; guardianEntGrowth = defender ? defender.GetComponent<GuardianEntGrowthPresentation>() : null;
            Build();
            manager.SelectionChanged += OnSelection; manager.PossessionChanged += OnPossession; manager.Released += OnReleased; manager.MomentFeedback += ShowMomentFeedback;
            defenseManager.StateChanged += OnDefenseState; possessionEnergy.Changed += (_, _) => Refresh(); core.ProgressChanged += value => coreText.text = $"{config.CoreName} danger: {value * 100:0}%";
            initialized = true;
            OnSelection(null); OnPossession(null); OnDefenseState(defenseManager.State); Refresh(); RefreshRouteStatus();
        }

        void Update()
        {
            if (!initialized) return;
            Refresh(); RefreshOpeningCue(); RefreshRouteStatus();
            RefreshAbilityButtons();
            var controller = possessionManager?.Possessed?.Controller<PlayerController>(); var rooted = controller && controller.IsActive && controller.RootEscapeVisible && !GameplayInput.TerminalState;
            if (rootPrompt) { rootPrompt.gameObject.SetActive(rooted); if (rooted) rootPrompt.text = controller.RootEscapeProgress >= 5 ? "BREAK FREE" : $"ROOTED — TAP TO BREAK FREE\n{controller.RootEscapeProgress}/5"; }
        }

        void Build()
        {
            presentation = gameObject.AddComponent<HudPresentation>();
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); gameObject.AddComponent<GraphicRaycaster>(); gameObject.AddComponent<ResponsiveHudRoot>().Initialize(true);
            state = Label(config.RealmTitle, new Vector2(0, -40), 38, TextAnchor.UpperCenter);
            invaderHealth = Label("", new Vector2(35, -105), 27, TextAnchor.UpperLeft); entHealth = Label("", new Vector2(35, -145), 27, TextAnchor.UpperLeft); ConstrainDefenderHealthLabel(); guardianEntVitality = GuardianEntVitalityLabel(); energyText = Label("", new Vector2(35, -185), 27, TextAnchor.UpperLeft);
            var meter = new GameObject("Possession Energy Meter", typeof(RectTransform), typeof(Image)); meter.transform.SetParent(transform, false); var meterRect = (RectTransform)meter.transform; meterRect.anchorMin = meterRect.anchorMax = new Vector2(0, 1); meterRect.pivot = new Vector2(0, 1); meterRect.anchoredPosition = new Vector2(35, -225); meterRect.sizeDelta = new Vector2(300, 18); meter.GetComponent<Image>().color = new Color(.03f, .08f, .04f, .9f); var fill = new GameObject("Fill", typeof(RectTransform), typeof(Image)); fill.transform.SetParent(meter.transform, false); var fillRect = (RectTransform)fill.transform; fillRect.anchorMin = new Vector2(0, 0); fillRect.anchorMax = new Vector2(1, 1); fillRect.pivot = new Vector2(0, .5f); fillRect.offsetMin = fillRect.offsetMax = Vector2.zero; energyFill = fill.GetComponent<Image>();
            coreText = Label($"{config.CoreName} danger: 0%", new Vector2(0, -235), 28, TextAnchor.UpperCenter); selection = Label($"Tap the {config.DefenderName} to select it", new Vector2(0, -285), 28, TextAnchor.UpperCenter); openingCue = Label("", new Vector2(0, -365), 26, TextAnchor.UpperCenter); openingCue.name = "Opening Preparation Cue"; openingCue.raycastTarget = false; openingCue.gameObject.SetActive(false); routeStatus = Label("", new Vector2(0, -445), 24, TextAnchor.UpperCenter); routeStatus.name = "Invader Route Status"; routeStatus.raycastTarget = false; routeStatus.gameObject.SetActive(false); trapText = Label("", new Vector2(0, 52), 23, TextAnchor.LowerCenter, true);
            rootPrompt = Label("", new Vector2(0, 700), 36, TextAnchor.MiddleCenter, true); rootPrompt.gameObject.SetActive(false);
            releaseNotice = Label("", new Vector2(0, 780), 30, TextAnchor.MiddleCenter, true); releaseNotice.raycastTarget = false; releaseNotice.gameObject.SetActive(false);
            possess = Button($"POSSESS {config.DefenderName.ToUpperInvariant()}", new Vector2(0, 410), PossessSelected);
            release = Button("RELEASE", new Vector2(0, 410), possessionManager.Release);
            activateTrap = Button("ACTIVATE TRAP", new Vector2(0, 290), ActivateTrap);
            smash = Button("SMASH", new Vector2(-180, 165), () => Ability(0)); slam = Button("GROUND SLAM", new Vector2(180, 165), () => Ability(2));
            abilityButtons = new[]
            {
                new AbilityButtonReadiness(smash, "SMASH", 0),
                new AbilityButtonReadiness(slam, "GROUND SLAM", 2)
            };
            dodge = Button("DODGE", new Vector2(0, 530), Dodge); dodgeLabel = dodge.GetComponentInChildren<Text>();
            resultPanel = new GameObject("Defense Result", typeof(RectTransform), typeof(Image)); resultPanel.transform.SetParent(transform, false); var rect = (RectTransform)resultPanel.transform; rect.anchorMin = new Vector2(.08f, .28f); rect.anchorMax = new Vector2(.92f, .72f); rect.offsetMin = rect.offsetMax = Vector2.zero; resultPanel.GetComponent<Image>().color = new Color(.025f, .06f, .035f, .97f);
            result = Label("", Vector2.zero, 42, TextAnchor.MiddleCenter); result.transform.SetParent(resultPanel.transform, false); var resultRect = (RectTransform)result.transform; resultRect.anchorMin = new Vector2(0, .35f); resultRect.anchorMax = Vector2.one; resultRect.offsetMin = resultRect.offsetMax = Vector2.zero;
            var retry = Button("DEFEND AGAIN", new Vector2(0, 160), () => SceneManager.LoadScene(config.RetryScene)); retry.transform.SetParent(resultPanel.transform, false); var raid = Button(config.NextActionLabel, new Vector2(0, 48), () => SceneManager.LoadScene(config.NextActionScene)); raid.transform.SetParent(resultPanel.transform, false); resultPanel.SetActive(false);
            var hub = Button("MY REALM", new Vector2(0, -64), () => SceneManager.LoadScene("PrototypeHub")); hub.transform.SetParent(resultPanel.transform, false);
        }

        void ActivateTrap()
        {
            if (!trap.TryActivate()) { Refresh(); return; }
            presentation?.PlayConfirm();
            if (!possessionManager.IsPossessing && !GameplayInput.TerminalState && !resultPanel.activeSelf)
                Camera.main?.GetComponent<PrototypeCameraRig>()?.FocusTrap(trap.transform, invader);
            Refresh();
        }
        void PossessSelected()
        {
            if (possessionManager.PossessSelected()) presentation?.PlayConfirm();
        }
        void Ability(int index) => possessionManager.Possessed?.Controller<PlayerController>()?.UseAbility(index);
        void Dodge() => possessionManager.Possessed?.Controller<PlayerController>()?.Dodge();
        void OnSelection(CombatEntity value)
        { selection.text = value ? $"Selected: {value.Definition.DisplayName}" : $"Tap the {config.DefenderName} to select it"; possess.gameObject.SetActive(value && !possessionManager.IsPossessing && !energy.IsDepleted); }
        void OnPossession(CombatEntity value)
        {
            bool active = value; release.gameObject.SetActive(active); smash.gameObject.SetActive(active); slam.gameObject.SetActive(active); dodge.gameObject.SetActive(active);
            if (active) { openingCueDismissed = true; SetOpeningCueVisible(false); }
            if (!active) possess.gameObject.SetActive(false);
            selection.text = active ? $"YOU ARE THE {config.DefenderName.ToUpperInvariant()}" : $"Tap the {config.DefenderName} to select it";
        }
        void OnReleased(bool forced) { }
        void ShowMomentFeedback(string message)
        {
            if (!releaseNotice || GameplayInput.TerminalState || (resultPanel && resultPanel.activeSelf)) return;
            releaseNotice.text = message; releaseNotice.gameObject.SetActive(true);
            CancelInvoke(nameof(HideReleaseNotice)); Invoke(nameof(HideReleaseNotice), 1.5f);
        }
        void HideReleaseNotice() { if (releaseNotice) releaseNotice.gameObject.SetActive(false); }
        void OnDefenseState(DefenseState value)
        {
            var terminal = value is DefenseState.DefenderVictory or DefenseState.RealmLost;
            GameplayInput.SetTerminalState(terminal);
            RefreshDodgeButton();
            guardianEntGrowth?.SetVisible(!terminal);
            if (guardianEntVitality) guardianEntVitality.gameObject.SetActive(guardianEntGrowth && !terminal);
            if (value is DefenseState.DefenderVictory or DefenseState.RealmLost) { SetOpeningCueVisible(false); ClearRouteStatus(); }
            state.text = value switch { DefenseState.Possessing => "POSSESSED CREATURE", DefenseState.DefenderVictory => "DEFENSE COMPLETE", DefenseState.RealmLost => "REALM BREACHED", _ => "KEEPER OVERVIEW" };
            if (value is DefenseState.DefenderVictory or DefenseState.RealmLost)
            { HideReleaseNotice(); resultPanel.SetActive(true); presentation?.PlayResult(); result.text = value == DefenseState.DefenderVictory ? "DEFENDER VICTORY\n\nThe invader was destroyed." : $"REALM LOST\n\nThe {config.CoreName} was captured."; }
        }

        void RefreshOpeningCue()
        {
            var brain = invader ? invader.Controller<RaidInvaderBrain>() : null;
            var show = brain && brain.IsOpeningHold && !openingCueDismissed && !possessionManager.IsPossessing && !defense.IsFinished && !GameplayInput.TerminalState && !(resultPanel && resultPanel.activeSelf);
            if (!show) { SetOpeningCueVisible(false); return; }
            var seconds = Mathf.Max(1, Mathf.CeilToInt(brain.OpeningSecondsRemaining));
            if (seconds != displayedOpeningSeconds)
            {
                displayedOpeningSeconds = seconds;
                openingCue.text = $"INVASION INCOMING — SELECT AND POSSESS\n{seconds}";
            }
            SetOpeningCueVisible(true);
        }

        void SetOpeningCueVisible(bool visible)
        {
            if (!openingCue) return;
            if (!visible) displayedOpeningSeconds = -1;
            if (openingCue.gameObject.activeSelf != visible) openingCue.gameObject.SetActive(visible);
        }
        void RefreshRouteStatus()
        {
            if (!routeStatus) return;
            var brain = invader ? invader.Controller<RaidInvaderBrain>() : null;
            var canShow = invader && invader.Health != null && !invader.Health.IsDead && defense != null && !defense.IsFinished && !GameplayInput.TerminalState && !(resultPanel && resultPanel.activeSelf) && brain && brain.IsActive;
            if (!canShow) { ClearRouteStatus(); return; }
            var opening = brain.IsOpeningHold;
            var target = !opening && brain.CurrentTarget && brain.CurrentTarget.Health != null && !brain.CurrentTarget.Health.IsDead ? brain.CurrentTarget : null;
            var waypoint = brain.WaypointIndex;
            if (routeStatusVisible && opening == displayedRouteOpening && target == displayedRouteTarget && waypoint == displayedRouteWaypoint) return;
            routeStatus.text = opening ? config.OpeningRouteStatus : target ? $"INVADER ENGAGING {target.Definition.DisplayName.ToUpperInvariant()}" : config.RouteStatus(waypoint);
            routeStatusVisible = true;
            displayedRouteOpening = opening;
            displayedRouteTarget = target;
            displayedRouteWaypoint = waypoint;
            routeStatus.gameObject.SetActive(true);
        }
        void ClearRouteStatus()
        {
            if (!routeStatus) { routeStatusVisible = false; displayedRouteTarget = null; displayedRouteWaypoint = int.MinValue; return; }
            if (!routeStatusVisible && !routeStatus.gameObject.activeSelf) return;
            routeStatusVisible = false;
            displayedRouteOpening = false;
            displayedRouteTarget = null;
            displayedRouteWaypoint = int.MinValue;
            if (!routeStatus) return;
            routeStatus.text = string.Empty;
            routeStatus.gameObject.SetActive(false);
        }
        void Refresh()
        {
            if (!initialized) return;
            if (!invader || !ent) return;
            invaderHealth.text = $"Invader  {invader.Health.Current:0}/{invader.Health.Maximum:0} HP"; entHealth.text = $"{config.DefenderName}  {ent.Health.Current:0}/{ent.Health.Maximum:0} HP";
            energyText.text = $"Possession energy  {energy.Remaining:0.0}/{energy.Maximum:0}s"; var ratio = energy.Maximum <= 0 ? 0 : energy.Remaining / energy.Maximum; energyFill.rectTransform.anchorMax = new Vector2(ratio, 1); energyFill.color = ratio <= .25f ? new Color(1f, .28f, .12f) : new Color(.55f, .95f, .2f);
            if (trap.State == TrapState.Ready)
            {
                var inRange = trap.TargetInRange; activateTrap.interactable = inRange;
                trapText.text = inRange ? $"ACTIVATE {config.TrapName.ToUpperInvariant()} — INVADER CAUGHT" : $"LURE INVADER INTO {config.TrapName.ToUpperInvariant()}";
                activateTrap.GetComponent<Image>().color = inRange ? new Color(.5f, .85f, .16f, .98f) : new Color(.18f, .25f, .18f, .7f);
            }
            else
            {
                activateTrap.interactable = false;
                trapText.text = trap is RootTrap root && root.RecentlyActivated ? "ROOTED!  12 DAMAGE — INVADER HELD" : $"{config.TrapName.ToUpperInvariant()} COOLDOWN — {trap.CooldownRemaining:0.0}s";
                activateTrap.GetComponent<Image>().color = new Color(.28f, .14f, .08f, .75f);
            }
        }

        void RefreshAbilityButtons()
        {
            if (abilityButtons == null) return;
            var controlled = possessionManager ? possessionManager.Possessed : null;
            var player = controlled ? controlled.Controller<PlayerController>() : null;
            var direct = player && player.IsActive && !GameplayInput.TerminalState && !(resultPanel && resultPanel.activeSelf);
            foreach (var button in abilityButtons) button.Refresh(controlled, direct);
            RefreshDodgeButton();
        }

        void RefreshDodgeButton()
        {
            if (!dodge) return;
            var controlled = possessionManager ? possessionManager.Possessed : null;
            var player = controlled ? controlled.Controller<PlayerController>() : null;
            var direct = player && player.IsActive && controlled.Health != null && !controlled.Health.IsDead && !GameplayInput.TerminalState && !(resultPanel && resultPanel.activeSelf);
            if (dodge.gameObject.activeSelf != direct) dodge.gameObject.SetActive(direct);
            if (!direct) { dodge.interactable = false; return; }
            var remaining = controlled.DodgeCooldownRemaining;
            string label;
            if (controlled.IsDodging) { displayedDodgeCooldownTenths = -1; label = "DODGING"; }
            else if (controlled.IsRooted) { displayedDodgeCooldownTenths = -1; label = "DODGE — ROOTED"; }
            else if (controlled.IsActionResolving) { displayedDodgeCooldownTenths = -1; label = "DODGE — BUSY"; }
            else if (remaining > .001f)
            {
                var tenths = Mathf.CeilToInt(remaining * 10);
                label = displayedDodgeCooldownTenths == tenths && dodgeLabel ? dodgeLabel.text : $"DODGE  {tenths / 10f:0.0}s";
                displayedDodgeCooldownTenths = tenths;
            }
            else { displayedDodgeCooldownTenths = -1; label = "DODGE"; }
            if (dodgeLabel && dodgeLabel.text != label) dodgeLabel.text = label;
            dodge.interactable = controlled.CanDodge;
        }

        public static string GuardianEntVitalityCopy(int rank)
        {
            rank = Mathf.Clamp(rank, 0, 3);
            return rank == 0 ? "GUARDIAN ENT — UNTENDED" : $"GUARDIAN ENT — RANK {rank}/3 • +{rank * 10}% MAX HEALTH";
        }

        Text GuardianEntVitalityLabel()
        {
            var go = new GameObject("Guardian Ent Vitality Status", typeof(RectTransform), typeof(Text));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(1, 1);
            rect.pivot = new Vector2(1, 1);
            rect.anchoredPosition = new Vector2(-35, -145);
            rect.sizeDelta = new Vector2(500, 42);
            var text = go.GetComponent<Text>();
            text.text = guardianEntGrowth ? GuardianEntVitalityCopy(guardianEntGrowth.Rank) : string.Empty;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 20;
            text.alignment = TextAnchor.UpperRight;
            text.color = new Color(.62f, .94f, .58f);
            text.raycastTarget = false;
            go.SetActive(guardianEntGrowth);
            return text;
        }

        void ConstrainDefenderHealthLabel()
        {
            entHealth.name = "Defender Health";
            var rect = entHealth.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.sizeDelta = new Vector2(450, 70);
        }

        Text Label(string value, Vector2 position, int size, TextAnchor anchor, bool bottom = false)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = bottom ? new Vector2(0, 0) : new Vector2(0, 1); rect.anchorMax = bottom ? new Vector2(1, 0) : new Vector2(1, 1); rect.pivot = bottom ? new Vector2(.5f, 0) : new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(0, 70); var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = anchor; text.color = Color.white; return text;
        }
        Button Button(string value, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiPointerOwnership)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(340, 96); go.GetComponent<Image>().color = new Color(.12f, .38f, .17f, .96f); var button = go.GetComponent<Button>(); presentation?.ApplyButton(go.GetComponent<Image>()); button.onClick.AddListener(action); button.onClick.AddListener(() => presentation?.PlayClick()); var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text)); textObject.transform.SetParent(go.transform, false); var textRect = (RectTransform)textObject.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero; var label = textObject.GetComponent<Text>(); label.text = value; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 25; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; return button;
        }
    }
}
