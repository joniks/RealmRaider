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
        Text state, health, stats, objective, result, rootPrompt, objectiveCompass, dodgeLabel;
        GameObject resultPanel;
        RectTransform resultRect;
        Button planNextDefense, raidAgain, realmHub, dodge;
        ResponsiveHudRoot responsive;
        CombatEntity hero;
        RaidManager raid;
        RealmCore core;
        Camera view;
        HudPresentation presentation;
        AbilityButtonReadiness[] abilityButtons;
        float objectiveProgress;
        int compassDirection;
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
        public RectTransform ObjectiveCompassRect => objectiveCompass ? objectiveCompass.rectTransform : null;
        public string ResultPrimaryActionText => planNextDefense ? planNextDefense.GetComponentInChildren<Text>().text : string.Empty;

        public void Initialize(RaidManager manager, CombatEntity raidHero, RealmCore objectiveTarget, Camera raidCamera)
        {
            if (PrototypeJourney.Stage == PrototypeJourneyStage.Raid) journeyToken = PrototypeJourney.ActiveToken;
            else if (PrototypeJourney.IsActive) { PrototypeJourney.Cancel(); FirstPlayableMinute.ResetBuildHandoff(); }
            raid = manager; hero = raidHero; core = objectiveTarget; view = raidCamera; Build();
            manager.StateChanged += OnState; manager.Finished += ShowResult;
            hero.Health.Changed += (_, _) => Refresh();
            Refresh(); OnState(manager.State);
        }

        public static string ResultActionDestination(string action) => action switch
        {
            PlanNextDefenseAction => PlanNextDefenseScene,
            DefendYourRealmAction => JourneyDefenseScene,
            "RAID AGAIN" => "SylvanRealm",
            "MY REALM" => "PrototypeHub",
            _ => string.Empty
        };

        public static string ResultCopy(RaidResult value)
        {
            var outcome = value.Victory
                ? "VICTORY\n\nThe Heart Tree fell. Return to your Realm and plan the next defense."
                : "DEFEAT\n\nRevise the next defense, or try this raid again.";
            return $"{outcome}\n\nGold collected: {value.Gold}\nRare materials: {value.RareMaterials}\nEnemies defeated: {value.EnemiesDefeated}\nRooms discovered: {value.RoomsDiscovered}\nRaid duration: {value.Duration:0}s\nCore reached: {(value.CoreReached ? "yes" : "no")}\n\nSecured for your Realm: {value.Gold} GOLD • {value.RareMaterials} RARE MATERIALS";
        }

        void Update()
        {
            if (raid) Refresh();
            RefreshAbilityButtons();
            RefreshDodgeButton();
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
            state = Label("SYLVAN RAID", new Vector2(0, -40), 38, TextAnchor.UpperCenter);
            health = Label("", new Vector2(35, -105), 28, TextAnchor.UpperLeft);
            stats = Label("", new Vector2(35, -150), 25, TextAnchor.UpperLeft);
            objective = Label("Reach the Heart Tree", new Vector2(0, -205), 28, TextAnchor.UpperCenter);
            objectiveCompass = Label("", Vector2.zero, 24, TextAnchor.MiddleCenter); objectiveCompass.name = "Heart Tree Compass"; objectiveCompass.raycastTarget = false; objectiveCompass.gameObject.SetActive(false);
            rootPrompt = Label("", new Vector2(0, 350), 36, TextAnchor.MiddleCenter, true); rootPrompt.gameObject.SetActive(false);
            Label("Tap ground: move • Tap enemy: attack • Swipe: Blood Rush", new Vector2(0, 45), 23, TextAnchor.LowerCenter, true);
            abilityButtons = new[]
            {
                new AbilityButtonReadiness(Button("SLASH", new Vector2(-260, 110), () => Ability(0)), "SLASH", 0),
                new AbilityButtonReadiness(Button("BLOOD RUSH", new Vector2(0, 110), () => Ability(1)), "BLOOD RUSH", 1),
                new AbilityButtonReadiness(Button("CLEAVE", new Vector2(260, 110), () => Ability(2)), "CLEAVE", 2)
            };
            dodge = Button("DODGE", new Vector2(0, 220), Dodge); dodgeLabel = dodge.GetComponentInChildren<Text>();
            resultPanel = new GameObject("Raid Result", typeof(RectTransform), typeof(Image)); resultPanel.transform.SetParent(transform, false);
            var rect = (RectTransform)resultPanel.transform; rect.anchorMin = new Vector2(.08f, .24f); rect.anchorMax = new Vector2(.92f, .76f); rect.offsetMin = rect.offsetMax = Vector2.zero;
            resultPanel.GetComponent<Image>().color = new Color(.025f, .06f, .035f, .97f);
            result = Label("", new Vector2(0, -560), 32, TextAnchor.UpperCenter); result.transform.SetParent(resultPanel.transform, false); resultRect = (RectTransform)result.transform;
            planNextDefense = Button(PlanNextDefenseAction, Vector2.zero, ContinueAfterRaid); planNextDefense.transform.SetParent(resultPanel.transform, false); planNextDefense.GetComponent<Image>().color = new Color(.24f, .58f, .25f, .98f); ((RectTransform)planNextDefense.transform).sizeDelta = new Vector2(280, 100);
            raidAgain = Button("RAID AGAIN", Vector2.zero, RetryRaid); raidAgain.transform.SetParent(resultPanel.transform, false);
            realmHub = Button("MY REALM", Vector2.zero, ReturnToHub); realmHub.transform.SetParent(resultPanel.transform, false);
            ApplyResultLayout(responsive.Orientation);
            resultPanel.SetActive(false);
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
                PlaceResultAction(planNextDefense, new Vector2(.82f, .5f), new Vector2(0, 135));
                PlaceResultAction(raidAgain, new Vector2(.82f, .5f), Vector2.zero);
                PlaceResultAction(realmHub, new Vector2(.82f, .5f), new Vector2(0, -135));
            }
            else
            {
                resultRect.anchorMin = new Vector2(0, 1); resultRect.anchorMax = new Vector2(1, 1); resultRect.pivot = new Vector2(.5f, 1); resultRect.anchoredPosition = new Vector2(0, -55); resultRect.sizeDelta = new Vector2(0, 470); result.alignment = TextAnchor.UpperCenter;
                StretchResultAction(planNextDefense, new Vector2(.1f, .54f), new Vector2(.9f, .78f));
                StretchResultAction(raidAgain, new Vector2(.1f, .3f), new Vector2(.9f, .5f));
                StretchResultAction(realmHub, new Vector2(.1f, .06f), new Vector2(.9f, .26f));
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
            SceneManager.LoadScene("SylvanRealm");
        }

        void ReturnToHub()
        {
            PrototypeJourney.Cancel();
            FirstPlayableMinute.ResetBuildHandoff();
            SceneManager.LoadScene("PrototypeHub");
        }

        void Ability(int index) => hero.Controller<PlayerController>()?.UseAbility(index);
        void Dodge() => hero.Controller<PlayerController>()?.Dodge();
        public void SetObjectiveProgress(float progress) { objectiveProgress = progress; objective.text = progress > 0 ? $"Capturing Heart Tree  {progress * 100:0}%" : "Reach the Heart Tree"; }
        void OnState(RaidState value) => state.text = $"SYLVAN RAID — {value}";
        void Refresh()
        {
            health.text = $"Blood Knight  {hero.Health.Current:0}/{hero.Health.Maximum:0} HP";
            stats.text = $"Gold {raid.Gold}   Enemies {raid.EnemiesDefeated}   Rooms {raid.RoomsDiscovered}   {raid.Duration:0}s";
        }
        void RefreshAbilityButtons()
        {
            if (abilityButtons == null) return;
            var player = hero ? hero.Controller<PlayerController>() : null;
            var direct = player && player.IsActive && !GameplayInput.TerminalState && !(resultPanel && resultPanel.activeSelf);
            foreach (var button in abilityButtons) button.Refresh(hero, direct);
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
        void ShowResult(RaidResult value)
        {
            CreditResultOnce(value);
            if (journeyToken != 0 && !journeyResultReached && PrototypeJourney.TryReachRaidResult(journeyToken))
            {
                journeyResultReached = true;
                journeyResultToken = PrototypeJourney.ActiveToken;
            }
            var journeyResult = journeyResultReached && PrototypeJourney.ActiveToken == journeyResultToken && PrototypeJourney.Stage == PrototypeJourneyStage.RaidResult;
            SetPrimaryActionCopy(journeyResult ? DefendYourRealmAction : PlanNextDefenseAction);
            GameplayInput.SetTerminalState(true);
            RefreshDodgeButton();
            SetCompassVisible(false);
            resultPanel.SetActive(true);
            presentation?.PlayResult();
            result.text = ResultCopy(value);
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
            var viewport = view.WorldToViewportPoint(core.transform.position + Vector3.up * 2f);
            if (viewport.z > 0 && viewport.x >= 0 && viewport.x <= 1 && viewport.y >= 0 && viewport.y <= 1) { SetCompassVisible(false); return; }
            var direction = ObjectiveDirectionFor(view, viewport, core.transform.position - view.transform.position);
            if (direction != compassDirection)
            {
                compassDirection = direction;
                objectiveCompass.text = direction < 0 ? "◀  HEART TREE" : "HEART TREE  ▶";
                var rect = objectiveCompass.rectTransform; var safe = Screen.safeArea;
                var edge = direction < 0 ? safe.xMin / Mathf.Max(1, Screen.width) : safe.xMax / Mathf.Max(1, Screen.width);
                rect.anchorMin = rect.anchorMax = new Vector2(edge, .5f); rect.pivot = new Vector2(direction < 0 ? 0 : 1, .5f); rect.anchoredPosition = new Vector2(direction < 0 ? 28 : -28, 0); rect.sizeDelta = new Vector2(220, 64);
            }
            SetCompassVisible(true);
        }

        bool IsActionableRaidState() => raid && raid.State is RaidState.RaidStarting or RaidState.Exploring or RaidState.Combat;
        void SetCompassVisible(bool visible) { if (objectiveCompass && objectiveCompass.gameObject.activeSelf != visible) objectiveCompass.gameObject.SetActive(visible); if (!visible) compassDirection = 0; }
        public static int ObjectiveDirectionFor(Camera camera, Vector3 viewport, Vector3 worldOffset)
        {
            if (viewport.x < 0) return -1;
            if (viewport.x > 1) return 1;
            return Vector3.Dot(camera.transform.right, worldOffset) < 0 ? -1 : 1;
        }

        void OnDestroy()
        {
            if (raid) { raid.StateChanged -= OnState; raid.Finished -= ShowResult; }
            if (responsive) responsive.LayoutChanged -= ApplyResultLayout;
            if (journeyToken != 0 && !journeyHandoff && PrototypeJourney.Cancel(journeyResultReached ? journeyResultToken : journeyToken))
                FirstPlayableMinute.ResetBuildHandoff();
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
            go.GetComponent<Image>().color = new Color(.12f, .38f, .17f, .96f); var button = go.GetComponent<Button>(); presentation?.ApplyButton(go.GetComponent<Image>()); button.onClick.AddListener(action); button.onClick.AddListener(() => presentation?.PlayClick());
            var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text)); textGo.transform.SetParent(go.transform, false); var textRect = (RectTransform)textGo.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero; var label = textGo.GetComponent<Text>(); label.text = value; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 25; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; return button;
        }
    }
}
