using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    public sealed class RaidHUD : MonoBehaviour
    {
        public const string PlanNextDefenseAction = "PLAN NEXT DEFENSE";
        public const string PlanNextDefenseScene = "RealmBuild";
        public const string DefendYourRealmAction = "DEFEND YOUR REALM";
        public const string JourneyDefenseScene = "DefenderTest";
        Text state, health, stats, objective, result, rootPrompt, objectiveCompass, dodgeLabel, jumpLabel, controlHint;
        GameObject resultPanel;
        RectTransform resultRect;
        Button planNextDefense, raidAgain, realmHub, dodge, jump, moonwellAction;
        ResponsiveHudRoot responsive;
        CombatEntity hero;
        RaidManager raid;
        RealmCore core;
        Camera view;
        HudPresentation presentation;
        RaidEncounterCue encounterCue;
        RaidRewardCue rewardCue;
        MoonwellRecovery moonwell;
        RaidHudConfig config;
        string stateTitle;
        CombatEntity objectiveGuardian;
        System.Func<bool> objectiveLocked;
        System.Func<string> objectiveWardCopy;
        string lockedObjectiveCopy;
        InRunControlStyleSelector controlStyleSelector;
        AbilityButtonReadiness[] abilityButtons;
        float objectiveProgress;
        int compassDirection;
        string compassTargetCopy = string.Empty;
        int displayedDodgeCooldownTenths = -1;
        bool resultRewardCredited;
        int journeyToken;
        int journeyResultToken;
        bool journeyHandoff;
        bool journeyResultReached;

        public bool ObjectiveCompassVisible => objectiveCompass && objectiveCompass.gameObject.activeSelf;
        public int ObjectiveCompassDirection => compassDirection;
        public bool ObjectiveCompassRaycastTarget => objectiveCompass && objectiveCompass.raycastTarget;
        public bool ResultPanelVisible => resultPanel && resultPanel.activeSelf;
        public string ResultText => result ? result.text : string.Empty;
        public string AbilityButtonText(int index) => abilityButtons != null && index >= 0 && index < abilityButtons.Length ? abilityButtons[index].Text : string.Empty;
        public bool AbilityButtonInteractable(int index) => abilityButtons != null && index >= 0 && index < abilityButtons.Length && abilityButtons[index].IsInteractable;
        public string DodgeButtonText => dodgeLabel ? dodgeLabel.text : string.Empty;
        public bool DodgeButtonInteractable => dodge && dodge.interactable;
        public bool DodgeButtonVisible => dodge && dodge.gameObject.activeSelf;
        public RectTransform DodgeButtonRect => dodge ? (RectTransform)dodge.transform : null;
        public string JumpButtonText => jump && jump.gameObject.activeSelf && jumpLabel ? jumpLabel.text : string.Empty;
        public bool JumpButtonInteractable => jump && jump.interactable;
        public bool JumpButtonVisible => jump && jump.gameObject.activeSelf;
        public RectTransform JumpButtonRect => jump ? (RectTransform)jump.transform : null;
        public RectTransform ObjectiveCompassRect => objectiveCompass ? objectiveCompass.rectTransform : null;
        public string ResultPrimaryActionText => planNextDefense ? planNextDefense.GetComponentInChildren<Text>().text : string.Empty;
        public bool ResultPrimaryActionVisible => planNextDefense && planNextDefense.gameObject.activeSelf;
        public InRunControlStyleSelector ControlStyleSelector => controlStyleSelector;
        public string ControlHintText => controlHint ? controlHint.text : string.Empty;
        public RaidEncounterCue EncounterCue => encounterCue;
        public RaidRewardCue RewardCue => rewardCue;
        public string HealthText => health ? health.text : string.Empty;
        public Color HealthTint => health ? health.color : DirectControlHealthReadability.NeutralTint;
        public bool LowHealthVisible { get; private set; }
        public RectTransform HealthRect => health ? health.rectTransform : null;
        public bool MoonwellActionVisible => moonwellAction && moonwellAction.gameObject.activeSelf;
        public bool MoonwellActionInteractable => moonwellAction && moonwellAction.interactable;
        public string MoonwellActionText => moonwellAction ? moonwellAction.GetComponentInChildren<Text>().text : string.Empty;
        public string ObjectiveText => objective ? objective.text : string.Empty;
        public string StateText => state ? state.text : string.Empty;
        public string RetryScene => config?.RetryScene ?? RaidHudConfig.Sylvan.RetryScene;

        public void Initialize(RaidManager manager, CombatEntity raidHero, RealmCore objectiveTarget, Camera raidCamera,
            MoonwellRecovery recovery = null, RaidHudConfig raidConfig = null, CombatEntity exactObjectiveGuardian = null,
            string variantDisplayName = null, System.Func<bool> objectiveLocked = null, string lockedObjectiveCopy = null,
            System.Func<string> objectiveWardCopy = null)
        {
            config = raidConfig ?? RaidHudConfig.Sylvan;
            stateTitle = string.IsNullOrWhiteSpace(variantDisplayName) ? config.StateTitle : $"{config.StateTitle} • {variantDisplayName.ToUpperInvariant()}";
            if (config.SupportsJourney && PrototypeJourney.Stage == PrototypeJourneyStage.Raid) journeyToken = PrototypeJourney.ActiveToken;
            else if (PrototypeJourney.IsActive) { PrototypeJourney.Cancel(); FirstPlayableMinute.ResetBuildHandoff(); }
            raid = manager; hero = raidHero; core = objectiveTarget; view = raidCamera; moonwell = recovery; objectiveGuardian = exactObjectiveGuardian;
            this.objectiveLocked = objectiveLocked; this.lockedObjectiveCopy = lockedObjectiveCopy; this.objectiveWardCopy = objectiveWardCopy; Build();
            manager.StateChanged += OnState; manager.Finished += ShowResult; manager.EncounterChanged += OnEncounter; manager.Rewarded += OnReward;
            hero.Health.Changed += OnHeroHealthChanged;
            Refresh(); OnState(manager.State); OnEncounter(manager.Encounter);
        }

        public static string ResultActionDestination(string action) => ResultActionDestination(action, RaidHudConfig.Sylvan);
        public static string ResultActionDestination(string action, RaidHudConfig raidConfig) => action switch
        {
            PlanNextDefenseAction => PlanNextDefenseScene,
            DefendYourRealmAction => JourneyDefenseScene,
            "RAID AGAIN" => (raidConfig ?? RaidHudConfig.Sylvan).RetryScene,
            "MY REALM" => "PrototypeHub",
            _ => string.Empty
        };

        public static string ResultCopy(RaidResult value)
            => ResultCopy(value, RaidHudConfig.Sylvan);

        public static string ResultCopy(RaidResult value, RaidHudConfig raidConfig)
        {
            var selected = raidConfig ?? RaidHudConfig.Sylvan;
            var outcome = value.Victory
                ? $"VICTORY\n\n{selected.VictoryCopy}"
                : $"DEFEAT\n\n{selected.DefeatCopy}";
            return FormatResultCopy(value, outcome);
        }

        static string JourneyResultCopy(RaidResult value)
        {
            var outcome = value.Victory
                ? "VICTORY\n\nThe Heart Tree fell. Your built Realm is under attack — defend it now."
                : "DEFEAT\n\nYour Realm still needs defense — defend it now or retry this raid.";
            return FormatResultCopy(value, outcome);
        }

        static string FormatResultCopy(RaidResult value, string outcome)
        {
            return $"{outcome}\n\nGold collected: {value.Gold}\nRare materials: {value.RareMaterials}\nEnemies defeated: {value.EnemiesDefeated}\nRooms discovered: {value.RoomsDiscovered}\nRaid duration: {value.Duration:0}s\nCore reached: {(value.CoreReached ? "yes" : "no")}\n\nSecured for your Realm: {value.Gold} GOLD • {value.RareMaterials} RARE MATERIALS";
        }

        void Update()
        {
            if (raid) Refresh();
            RefreshAbilityButtons();
            RefreshDodgeButton();
            RefreshJumpButton();
            RefreshMoonwellAction();
            RefreshControlHint();
            var controller = hero ? hero.Controller<PlayerController>() : null;
            if (rootPrompt)
            {
                var rooted = controller && controller.IsActive && controller.RootEscapeVisible && !GameplayInput.TerminalState;
                rootPrompt.gameObject.SetActive(rooted);
                if (rooted) rootPrompt.text = controller.RootEscapeProgress >= 5 ? "BREAK FREE" : $"ROOTED — TAP TO BREAK FREE\n{controller.RootEscapeProgress}/5";
            }
            UpdateObjectiveCompass();
        }

        void Build()
        {
            presentation = gameObject.AddComponent<HudPresentation>();
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920);
            gameObject.AddComponent<GraphicRaycaster>(); responsive = gameObject.AddComponent<ResponsiveHudRoot>(); responsive.LayoutChanged += ApplyResultLayout; responsive.Initialize(true);
            state = Label(stateTitle, new Vector2(0, -40), 38, TextAnchor.UpperCenter);
            presentation.DecorateRealmLabel(state, config.RealmIdentity);
            health = Label("", new Vector2(35, -105), 28, TextAnchor.UpperLeft);
            stats = Label("", new Vector2(35, -150), 25, TextAnchor.UpperLeft);
            objective = Label(ObjectiveCopy(), new Vector2(0, -205), 28, TextAnchor.UpperCenter); objective.raycastTarget = false;
            objectiveCompass = Label("", Vector2.zero, 24, TextAnchor.MiddleCenter); objectiveCompass.name = "Heart Tree Compass"; objectiveCompass.raycastTarget = false; objectiveCompass.gameObject.SetActive(false);
            rootPrompt = Label("", new Vector2(0, 350), 36, TextAnchor.MiddleCenter, true); rootPrompt.gameObject.SetActive(false);
            controlHint = Label("", new Vector2(0, 45), 23, TextAnchor.LowerCenter, true); controlHint.raycastTarget = false;
            encounterCue = gameObject.AddComponent<RaidEncounterCue>(); encounterCue.Initialize(responsive);
            rewardCue = gameObject.AddComponent<RaidRewardCue>(); rewardCue.Initialize(responsive);
            abilityButtons = new[]
            {
                AbilityButton(config.AbilityLabel(0), new Vector2(-260, 110), 0),
                AbilityButton(config.AbilityLabel(1), new Vector2(0, 110), 1),
                AbilityButton(config.AbilityLabel(2), new Vector2(260, 110), 2)
            };
            dodge = Button("DODGE", new Vector2(0, 220), Dodge); dodgeLabel = dodge.GetComponentInChildren<Text>();
            jump = Button("JUMP", new Vector2(260, 220), Jump); jumpLabel = jump.GetComponentInChildren<Text>(); presentation.DecorateJumpButton(jump);
            if (moonwell) moonwellAction = Button("MOONWELL — READY", new Vector2(-260, 220), UseMoonwell);
            resultPanel = new GameObject("Raid Result", typeof(RectTransform), typeof(Image)); resultPanel.transform.SetParent(transform, false);
            var rect = (RectTransform)resultPanel.transform; rect.anchorMin = new Vector2(.08f, .24f); rect.anchorMax = new Vector2(.92f, .76f); rect.offsetMin = rect.offsetMax = Vector2.zero;
            resultPanel.GetComponent<Image>().color = new Color(.025f, .06f, .035f, .97f);
            result = Label("", new Vector2(0, -560), 32, TextAnchor.UpperCenter); result.transform.SetParent(resultPanel.transform, false); resultRect = (RectTransform)result.transform;
            planNextDefense = Button(PlanNextDefenseAction, Vector2.zero, ContinueAfterRaid); planNextDefense.transform.SetParent(resultPanel.transform, false); planNextDefense.GetComponent<Image>().color = new Color(.24f, .58f, .25f, .98f); ((RectTransform)planNextDefense.transform).sizeDelta = new Vector2(280, 100);
            raidAgain = Button("RAID AGAIN", Vector2.zero, RetryRaid); raidAgain.transform.SetParent(resultPanel.transform, false);
            realmHub = Button("MY REALM", Vector2.zero, ReturnToHub); realmHub.transform.SetParent(resultPanel.transform, false);
            planNextDefense.gameObject.SetActive(config.ShowPlanNextDefense);
            ApplyResultLayout(responsive.Orientation);
            resultPanel.SetActive(false);
            controlStyleSelector = InRunControlStyleSelector.Attach(responsive, presentation);
        }

        void ApplyResultLayout(PrototypeOrientation orientation)
        {
            if (!resultPanel || !resultRect || !planNextDefense || !raidAgain || !realmHub) return;
            var panelRect = (RectTransform)resultPanel.transform;
            panelRect.anchorMin = orientation == PrototypeOrientation.Landscape ? new Vector2(.08f, .1f) : new Vector2(.08f, .18f);
            panelRect.anchorMax = orientation == PrototypeOrientation.Landscape ? new Vector2(.92f, .9f) : new Vector2(.92f, .82f);
            if (orientation == PrototypeOrientation.Landscape)
            {
                resultRect.anchorMin = new Vector2(0, 1); resultRect.anchorMax = new Vector2(.62f, 1); resultRect.pivot = new Vector2(.5f, 1); resultRect.anchoredPosition = new Vector2(0, -48); resultRect.sizeDelta = new Vector2(0, 500); result.alignment = TextAnchor.UpperLeft;
                ((RectTransform)planNextDefense.transform).sizeDelta = new Vector2(280, 100);
                ((RectTransform)raidAgain.transform).sizeDelta = new Vector2(240, 92);
                ((RectTransform)realmHub.transform).sizeDelta = new Vector2(240, 92);
                if (config.ShowPlanNextDefense)
                {
                    PlaceResultAction(planNextDefense, new Vector2(.82f, .5f), new Vector2(0, 135));
                    PlaceResultAction(raidAgain, new Vector2(.82f, .5f), Vector2.zero);
                    PlaceResultAction(realmHub, new Vector2(.82f, .5f), new Vector2(0, -135));
                }
                else
                {
                    PlaceResultAction(raidAgain, new Vector2(.82f, .5f), new Vector2(0, 65));
                    PlaceResultAction(realmHub, new Vector2(.82f, .5f), new Vector2(0, -65));
                }
            }
            else
            {
                resultRect.anchorMin = new Vector2(0, 1); resultRect.anchorMax = new Vector2(1, 1); resultRect.pivot = new Vector2(.5f, 1); resultRect.anchoredPosition = new Vector2(0, -55); resultRect.sizeDelta = new Vector2(0, 470); result.alignment = TextAnchor.UpperCenter;
                if (config.ShowPlanNextDefense)
                {
                    StretchResultAction(planNextDefense, new Vector2(.1f, .54f), new Vector2(.9f, .78f));
                    StretchResultAction(raidAgain, new Vector2(.1f, .3f), new Vector2(.9f, .5f));
                    StretchResultAction(realmHub, new Vector2(.1f, .06f), new Vector2(.9f, .26f));
                }
                else
                {
                    StretchResultAction(raidAgain, new Vector2(.1f, .4f), new Vector2(.9f, .64f));
                    StretchResultAction(realmHub, new Vector2(.1f, .1f), new Vector2(.9f, .34f));
                }
            }
        }

        static void PlaceResultAction(Button button, Vector2 anchor, Vector2 position)
        {
            var rect = (RectTransform)button.transform; rect.anchorMin = rect.anchorMax = anchor; rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = position;
        }

        static void StretchResultAction(Button button, Vector2 anchorMin, Vector2 anchorMax)
        {
            var rect = (RectTransform)button.transform; rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = new Vector2(.5f, .5f); rect.offsetMin = rect.offsetMax = Vector2.zero;
        }

        void ContinueAfterRaid()
        {
            if (journeyToken == 0) { SceneManager.LoadScene(PlanNextDefenseScene); return; }
            if (!journeyResultReached || !PrototypeJourney.TryBeginDefense(journeyResultToken)) return;
            journeyHandoff = true;
            SceneManager.LoadScene(JourneyDefenseScene);
        }

        void RetryRaid()
        {
            if (journeyToken != 0)
            {
                if (!journeyResultReached || !PrototypeJourney.TryRetryRaid(journeyResultToken)) return;
                journeyHandoff = true;
            }
            else if (PrototypeJourney.IsActive) return;
            SceneManager.LoadScene(config.RetryScene);
        }

        void ReturnToHub()
        {
            PrototypeJourney.Cancel();
            FirstPlayableMinute.ResetBuildHandoff();
            SceneManager.LoadScene("PrototypeHub");
        }

        void Ability(int index) => hero.Controller<PlayerController>()?.UseAbility(index);
        void Dodge() => hero.Controller<PlayerController>()?.Dodge();
        void Jump() => hero.Controller<PlayerController>()?.Jump();
        void UseMoonwell()
        {
            if (moonwell && moonwell.TryUse() > 0) presentation?.PlayConfirm();
            RefreshMoonwellAction();
        }
        public void SetObjectiveProgress(float progress)
        {
            objectiveProgress = progress;
            objective.text = progress > 0 ? $"Capturing {config.ObjectiveName}  {progress * 100:0}%" : ObjectiveCopy();
        }
        public void RefreshObjectiveCopy()
        {
            if (objective && objectiveProgress <= .001f) objective.text = ObjectiveCopy();
        }
        string ObjectiveCopy()
        {
            if (!GameplayInput.TerminalState && !(resultPanel && resultPanel.activeSelf) && objectiveWardCopy != null)
            {
                var wardCopy = objectiveWardCopy();
                if (!string.IsNullOrWhiteSpace(wardCopy)) return wardCopy;
            }
            var locked = objectiveLocked != null
                ? objectiveLocked()
                : objectiveGuardian && objectiveGuardian.Health != null && !objectiveGuardian.Health.IsDead;
            if (!locked) return $"Reach the {config.ObjectiveName}";
            return string.IsNullOrWhiteSpace(lockedObjectiveCopy) ? config.LockedObjectiveCopy : lockedObjectiveCopy;
        }
        void OnState(RaidState value)
        {
            state.text = $"{stateTitle} — {value}";
            presentation.DecorateRealmLabel(state, config.RealmIdentity);
            if (value == RaidState.RaidStarting) rewardCue?.Clear();
            if (value is RaidState.Victory or RaidState.Defeat or RaidState.Escape or RaidState.RaidResult) encounterCue?.Clear();
            if (value is RaidState.Defeat or RaidState.Escape or RaidState.RaidResult) rewardCue?.Clear();
            RefreshHealth();
            RefreshMoonwellAction();
        }
        void OnReward(RaidRewardFact fact) => rewardCue?.Enqueue(fact);
        void OnEncounter(RaidEncounterState value)
        {
            if (GameplayInput.TerminalState || resultPanel && resultPanel.activeSelf) encounterCue?.Clear();
            else encounterCue?.Show(value);
        }
        void Refresh()
        {
            RefreshHealth();
            if (objectiveProgress <= .001f && objective) objective.text = ObjectiveCopy();
            stats.text = $"Gold {raid.Gold}   Enemies {raid.EnemiesDefeated}   Rooms {raid.RoomsDiscovered}   {raid.Duration:0}s";
        }
        void OnHeroHealthChanged(float current, float maximum) => RefreshHealth();
        void RefreshHealth()
        {
            if (!health || !hero || hero.Health == null) return;
            var player = hero.Controller<PlayerController>();
            var direct = player && player.IsActive;
            var terminal = GameplayInput.TerminalState || resultPanel && resultPanel.activeSelf || IsTerminalRaidState();
            ApplyHealthPresentation(DirectControlHealthReadability.Map(config.HeroName, hero.Health.Current, hero.Health.Maximum, direct, terminal));
        }
        bool IsTerminalRaidState() => raid && (raid.State == RaidState.Victory || raid.State == RaidState.Defeat || raid.State == RaidState.Escape || raid.State == RaidState.RaidResult);
        void RefreshMoonwellAction()
        {
            if (!moonwellAction || !moonwell) return;
            var terminal = GameplayInput.TerminalState || resultPanel && resultPanel.activeSelf || IsTerminalRaidState();
            var visible = !terminal;
            if (moonwellAction.gameObject.activeSelf != visible) moonwellAction.gameObject.SetActive(visible);
            if (!visible) { moonwellAction.interactable = false; return; }
            moonwellAction.interactable = moonwell.CanUse;
            var copy = moonwell.State switch
            {
                MoonwellRecoveryState.Ready => "USE MOONWELL",
                MoonwellRecoveryState.FullHealth => "MOONWELL — FULL HP",
                MoonwellRecoveryState.OutOfRange => "MOONWELL — DISTANT",
                MoonwellRecoveryState.Spent => "MOONWELL — SPENT",
                _ => "MOONWELL — UNAVAILABLE"
            };
            var label = moonwellAction.GetComponentInChildren<Text>();
            if (label && label.text != copy) label.text = copy;
        }
        void ApplyHealthPresentation(DirectControlHealthReadabilityState next)
        {
            if (health.text != next.Copy) health.text = next.Copy;
            if (health.color != next.Tint) health.color = next.Tint;
            LowHealthVisible = next.IsLow;
        }
        void RestoreHealthPresentation()
        {
            if (!health || !hero || hero.Health == null) return;
            ApplyHealthPresentation(DirectControlHealthReadability.Map(config.HeroName, hero.Health.Current, hero.Health.Maximum, false, true));
        }
        void RefreshAbilityButtons()
        {
            if (abilityButtons == null) return;
            var player = hero ? hero.Controller<PlayerController>() : null;
            var direct = player && player.IsActive && !GameplayInput.TerminalState && !(resultPanel && resultPanel.activeSelf);
            foreach (var button in abilityButtons) button.Refresh(hero, direct && !hero.IsJumping);
        }
        void RefreshDodgeButton()
        {
            if (!dodge) return;
            var player = hero ? hero.Controller<PlayerController>() : null;
            var direct = player && player.IsActive && hero.Health != null && !hero.Health.IsDead && !GameplayInput.TerminalState && !(resultPanel && resultPanel.activeSelf);
            if (dodge.gameObject.activeSelf != direct) dodge.gameObject.SetActive(direct);
            if (!direct) { dodge.interactable = false; return; }
            var remaining = hero.DodgeCooldownRemaining;
            string label;
            if (hero.IsDodging) { displayedDodgeCooldownTenths = -1; label = "DODGING"; }
            else if (hero.IsRooted) { displayedDodgeCooldownTenths = -1; label = "DODGE — ROOTED"; }
            else if (hero.IsJumping) { displayedDodgeCooldownTenths = -1; label = "DODGE — AIRBORNE"; }
            else if (hero.IsActionResolving) { displayedDodgeCooldownTenths = -1; label = "DODGE — BUSY"; }
            else if (remaining > .001f)
            {
                var tenths = Mathf.CeilToInt(remaining * 10);
                label = displayedDodgeCooldownTenths == tenths && dodgeLabel ? dodgeLabel.text : $"DODGE  {tenths / 10f:0.0}s";
                displayedDodgeCooldownTenths = tenths;
            }
            else { displayedDodgeCooldownTenths = -1; label = "DODGE"; }
            if (dodgeLabel && dodgeLabel.text != label) dodgeLabel.text = label;
            dodge.interactable = hero.CanDodge;
        }
        void RefreshJumpButton()
        {
            if (!jump) return;
            var player = hero ? hero.Controller<PlayerController>() : null;
            var direct = player && player.IsActive && UsesJoystickControls() && hero.Health != null && !hero.Health.IsDead && !GameplayInput.TerminalState && !(resultPanel && resultPanel.activeSelf);
            if (jump.gameObject.activeSelf != direct) jump.gameObject.SetActive(direct);
            if (!direct) { jump.interactable = false; return; }
            var label = hero.IsRooted ? "JUMP — ROOTED"
                : hero.IsJumping || !hero.IsGrounded ? "JUMP — AIRBORNE"
                : hero.IsDodging || hero.IsActionResolving ? "JUMP — BUSY"
                : "JUMP";
            if (jumpLabel && jumpLabel.text != label) jumpLabel.text = label;
            jump.interactable = hero.CanJump;
        }
        void RefreshControlHint()
        {
            if (!controlHint) return;
            var copy = UsesJoystickControls()
                ? "STICK: MOVE • DRAG WORLD: LOOK • JUMP: LEAP"
                : $"TAP: MOVE/ATTACK • DOUBLE-TAP GROUND: JUMP • SWIPE: {config.AbilityLabel(1)}";
            if (controlHint.text != copy) controlHint.text = copy;
        }
        bool UsesJoystickControls() => responsive && PrototypeSave.EffectiveControlStyle(responsive.Orientation == PrototypeOrientation.Landscape) == "Joystick";
        void ShowResult(RaidResult value)
        {
            CreditResultOnce(value);
            if (journeyToken != 0 && !journeyResultReached && PrototypeJourney.TryReachRaidResult(journeyToken))
            {
                journeyResultReached = true;
                journeyResultToken = PrototypeJourney.ActiveToken;
            }
            var journeyResult = config.SupportsJourney && journeyResultReached && PrototypeJourney.ActiveToken == journeyResultToken && PrototypeJourney.Stage == PrototypeJourneyStage.RaidResult;
            SetPrimaryActionCopy(journeyResult ? DefendYourRealmAction : PlanNextDefenseAction);
            GameplayInput.SetTerminalState(true);
            controlStyleSelector?.RefreshNow();
            RefreshDodgeButton();
            RefreshJumpButton();
            SetCompassVisible(false);
            encounterCue?.Clear();
            rewardCue?.Clear();
            resultPanel.SetActive(true);
            presentation?.PlayResult();
            result.text = journeyResult ? JourneyResultCopy(value) : ResultCopy(value, config);
        }

        void SetPrimaryActionCopy(string copy)
        {
            if (!planNextDefense) return;
            planNextDefense.name = copy;
            planNextDefense.GetComponentInChildren<Text>().text = copy;
        }

        void CreditResultOnce(RaidResult value)
        {
            if (resultRewardCredited) return;
            if (raid && !raid.TryClaimRealmRewards()) return;
            RealmProgress.Credit(value);
            resultRewardCredited = true;
        }

        void UpdateObjectiveCompass()
        {
            if (!objectiveCompass) return;
            if (!view || !core || !hero || hero.Health.IsDead || objectiveProgress > .001f || GameplayInput.TerminalState || resultPanel && resultPanel.activeSelf || !IsActionableRaidState()) { SetCompassVisible(false); return; }
            var target = ObjectiveTarget();
            var viewport = view.WorldToViewportPoint(target.position + Vector3.up * 2f);
            if (viewport.z > 0 && viewport.x >= 0 && viewport.x <= 1 && viewport.y >= 0 && viewport.y <= 1) { SetCompassVisible(false); return; }
            var direction = ObjectiveDirectionFor(view, viewport, target.position - view.transform.position);
            var targetCopy = objectiveGuardian && objectiveGuardian.Health != null && !objectiveGuardian.Health.IsDead ? "INFERNAL BRUTE" : config.ObjectiveName.ToUpperInvariant();
            if (direction != compassDirection || compassTargetCopy != targetCopy)
            {
                compassDirection = direction;
                compassTargetCopy = targetCopy;
                objectiveCompass.text = direction < 0 ? $"◀  {targetCopy}" : $"{targetCopy}  ▶";
                var rect = objectiveCompass.rectTransform; var safe = Screen.safeArea;
                var edge = direction < 0 ? safe.xMin / Mathf.Max(1, Screen.width) : safe.xMax / Mathf.Max(1, Screen.width);
                rect.anchorMin = rect.anchorMax = new Vector2(edge, .5f); rect.pivot = new Vector2(direction < 0 ? 0 : 1, .5f); rect.anchoredPosition = new Vector2(direction < 0 ? 28 : -28, 0); rect.sizeDelta = new Vector2(220, 64);
            }
            SetCompassVisible(true);
        }

        Transform ObjectiveTarget() => objectiveGuardian && objectiveGuardian.Health != null && !objectiveGuardian.Health.IsDead ? objectiveGuardian.transform : core.transform;

        bool IsActionableRaidState() => raid && raid.State is RaidState.RaidStarting or RaidState.Exploring or RaidState.Combat;
        void SetCompassVisible(bool visible) { if (objectiveCompass && objectiveCompass.gameObject.activeSelf != visible) objectiveCompass.gameObject.SetActive(visible); if (!visible) { compassDirection = 0; compassTargetCopy = string.Empty; } }
        public static int ObjectiveDirectionFor(Camera camera, Vector3 viewport, Vector3 worldOffset)
        {
            if (viewport.x < 0) return -1;
            if (viewport.x > 1) return 1;
            return Vector3.Dot(camera.transform.right, worldOffset) < 0 ? -1 : 1;
        }

        void OnDestroy()
        {
            if (raid) { raid.StateChanged -= OnState; raid.Finished -= ShowResult; raid.EncounterChanged -= OnEncounter; raid.Rewarded -= OnReward; }
            if (hero && hero.Health != null) hero.Health.Changed -= OnHeroHealthChanged;
            objectiveWardCopy = null;
            if (responsive) responsive.LayoutChanged -= ApplyResultLayout;
            encounterCue?.Clear();
            rewardCue?.Clear();
            if (journeyToken != 0 && !journeyHandoff && PrototypeJourney.Cancel(journeyResultReached ? journeyResultToken : journeyToken))
                FirstPlayableMinute.ResetBuildHandoff();
        }

        void OnDisable() => RestoreHealthPresentation();

        Text Label(string value, Vector2 position, int size, TextAnchor anchor, bool bottom = false)
        {
            var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform; rect.anchorMin = bottom ? new Vector2(0, 0) : new Vector2(0, 1); rect.anchorMax = bottom ? new Vector2(1, 0) : new Vector2(1, 1); rect.pivot = bottom ? new Vector2(.5f, 0) : new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(0, bottom ? 60 : 70);
            var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = anchor; text.color = Color.white; return text;
        }
        Button Button(string value, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiPointerOwnership)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 0); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(240, 92);
            go.GetComponent<Image>().color = new Color(.12f, .38f, .17f, .96f); var button = go.GetComponent<Button>(); presentation?.ApplyButton(go.GetComponent<Image>()); button.onClick.AddListener(action); button.onClick.AddListener(() => presentation?.PlayClick());
            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text)); textGo.transform.SetParent(go.transform, false); var textRect = (RectTransform)textGo.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero; var label = textGo.GetComponent<Text>(); label.text = value; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 25; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; return button;
        }

        AbilityButtonReadiness AbilityButton(string label, Vector2 position, int index)
        {
            var button = Button(label, position, () => Ability(index));
            if (config.UsesGuardianEntAbilityIcons)
                presentation?.DecorateGuardianEntAbilityButton(button, hero && hero.Definition ? hero.Definition.ArchetypeId : string.Empty, index, label);
            else presentation?.DecorateAbilityButton(button, index, label);
            return new AbilityButtonReadiness(button, label, index);
        }
    }
}
