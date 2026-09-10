using System;
using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace RealmRaiders.UI
{
    public sealed class BuildHUD : MonoBehaviour
    {
        public const string SaveAndDefendAction = "SAVE & DEFEND";
        public const string SaveAndRaidAction = "SAVE & RAID";
        public int SlotCount => slots.Length;
        public string DefensePlanText => plan ? plan.text : string.Empty;
        public string RealmStoresText => realmStores ? realmStores.text : string.Empty;
        public string GuardianEntUpgradeText => cultivateEnt ? cultivateEnt.GetComponentInChildren<Text>().text : string.Empty;
        public string SlotCopy(int index) => index >= 0 && index < slots.Length ? slots[index].GetComponentInChildren<Text>().text : string.Empty;
        public bool SaveInteractable => saveButton && saveButton.interactable;
        public string SaveActionText => saveButton ? saveButton.GetComponentInChildren<Text>().text : string.Empty;
        public bool GuardianEntUpgradeInteractable => cultivateEnt && cultivateEnt.interactable;
        public BuildGuideStep GuideStep => guideStep;
        public bool GuideLineVisible => guide && guide.DismissVisible;
        public string GuideText => reason ? reason.text : string.Empty;
        public bool GuideLineRaycastTarget => reason && reason.raycastTarget;
        public bool GuideSkipVisible => guide && guide.SkipVisible;
        public bool GuideEmphasisVisible => guide && guide.EmphasisVisible;
        public RectTransform GuideEmphasisRect => guide ? guide.EmphasisRect : null;
        public RectTransform GuideDismissRect => guide ? guide.DismissRect : null;
        public RectTransform GuideSkipRect => guide ? guide.SkipRect : null;
        Button[] slots = Array.Empty<Button>(); Button saveButton, cultivateEnt; Text title; Text budget; Text reason; Text realmStores; Text plan; DefenseLayout layout; ResponsiveHudRoot responsive; HudPresentation presentation; BuildPlanPreview planPreview;
        BuildLayoutSnapshot entryLayout; BuildGuideStep guideStep, dismissedStep; FirstPlayableMinuteBuildGuide guide;
        int journeyToken;
        bool journeyHandoff;
        public void Initialize()
        {
            FirstPlayableMinute.ResetBuildHandoff();
            if (PrototypeJourney.Stage == PrototypeJourneyStage.Build) journeyToken = PrototypeJourney.ActiveToken;
            else if (PrototypeJourney.IsActive) PrototypeJourney.Cancel();
            layout = DefenseLayoutSave.Load(); entryLayout = FirstPlayableMinute.CaptureBuildEntry(layout); Build(); Refresh();
        }
        void Build()
        {
            presentation = gameObject.AddComponent<HudPresentation>();
            var canvas = gameObject.AddComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay; var scaler = gameObject.AddComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920); gameObject.AddComponent<GraphicRaycaster>();
            title = Label("SYLVAN BUILD", new Vector2(0, -120), 52); presentation.DecorateRealmLabel(title, HudPresentation.SylvanRealmIdentity); budget = Label("", new Vector2(0, -215), 30); reason = Label("", new Vector2(0, -310), 24); reason.name = "Build Guidance"; reason.raycastTarget = false; realmStores = Label("", new Vector2(0, -385), 21); realmStores.name = "Realm Stores"; realmStores.rectTransform.sizeDelta = new Vector2(950, 48); realmStores.raycastTarget = false;
            plan = Label("", new Vector2(0, -400), 18); plan.name = "Defense Plan Summary"; plan.rectTransform.sizeDelta = new Vector2(950, 90); plan.raycastTarget = false;
            var previewObject = new GameObject("Build Plan Preview", typeof(RectTransform), typeof(BuildPlanPreview)); previewObject.transform.SetParent(transform, false); planPreview = previewObject.GetComponent<BuildPlanPreview>(); planPreview.Initialize();
            slots = new Button[5]; for (int i = 0; i < slots.Length; i++) { int index = i; slots[i] = Button("", new Vector2(0, 280 - i * 140), () => Cycle(index)); }
            cultivateEnt = Button("CULTIVATE GUARDIAN ENT", Vector2.zero, OnPurchaseGuardianEntVitality); cultivateEnt.name = "CULTIVATE GUARDIAN ENT"; cultivateEnt.GetComponentInChildren<Text>().fontSize = 17;
            saveButton = Button(journeyToken != 0 ? SaveAndRaidAction : SaveAndDefendAction, new Vector2(0, -650), SaveAndContinue);
            responsive = gameObject.AddComponent<ResponsiveHudRoot>(); responsive.LayoutChanged += ApplyOrientation; responsive.Initialize(false);
            if (FirstPlayableMinute.Load() == FirstPlayableMinuteStatus.Active) { guide = gameObject.AddComponent<FirstPlayableMinuteBuildGuide>(); guide.Initialize(responsive, slots, saveButton, presentation, DismissGuide, SkipGuide); }
        }
        public void CycleSlotForTests(int index) => Cycle(index);
        public bool PurchaseGuardianEntVitalityForTests() => PurchaseGuardianEntVitality();
        void Cycle(int index) { var slot = layout.Slots[index]; var next = slot.Piece; for (int i = 0; i < 4; i++) { next = (DefensePieceType)(((int)next + 1) % 4); var candidate = new DefenseSlotLayout(slot.SlotType, next); if (DefenseLayoutRules.IsAllowed(candidate)) { layout.Slots[index] = candidate; break; } } Refresh(); }
        void SaveAndContinue()
        {
            if (!DefenseLayoutRules.IsValid(layout, out _)) return;
            if (journeyToken != 0 && (PrototypeJourney.ActiveToken != journeyToken || PrototypeJourney.Stage != PrototypeJourneyStage.Build)) return;
            if (journeyToken == 0 && PrototypeJourney.IsActive) return;
            DefenseLayoutSave.Save(layout);
            planPreview?.Refresh(layout);
            FirstPlayableMinute.TryAcceptChangedBuild(entryLayout, layout);
            if (journeyToken == 0) { SceneManager.LoadScene("DefenderTest"); return; }
            if (!PrototypeJourney.TryBeginRaid(journeyToken)) return;
            journeyHandoff = true;
            SceneManager.LoadScene("SylvanRealm");
        }
        bool PurchaseGuardianEntVitality()
        {
            if (!RealmProgress.TryPurchaseGuardianEntVitality(out _)) return false;
            presentation?.PlayConfirm();
            Refresh();
            return true;
        }
        void OnPurchaseGuardianEntVitality() => PurchaseGuardianEntVitality();
        void Refresh()
        {
            var valid = DefenseLayoutRules.IsValid(layout, out var message); budget.text = $"Threat: {DefenseLayoutRules.Used(layout)}/{DefenseLayoutRules.Budget}"; realmStores.text = RealmProgress.StoreCopy(); plan.text = FormatDefensePlan(layout); if (saveButton) saveButton.interactable = valid;
            var guideActive = FirstPlayableMinute.Load() == FirstPlayableMinuteStatus.Active && guide != null;
            if (guideActive)
            {
                var nextStep = FirstPlayableMinute.EvaluateBuild(entryLayout, layout, out var guideReason);
                if (nextStep != guideStep) { guideStep = nextStep; dismissedStep = BuildGuideStep.Hidden; }
                var lineVisible = dismissedStep != guideStep;
                reason.text = lineVisible ? BuildGuideCopy(guideStep, guideReason) : valid ? ReadyCopy() : message;
                guide.Show(guideStep, lineVisible);
            }
            else
            {
                guideStep = BuildGuideStep.Hidden;
                reason.text = valid ? ReadyCopy() : message;
                guide?.Shutdown();
            }
            if (cultivateEnt)
            {
                var progress = RealmProgress.Load();
                cultivateEnt.interactable = RealmProgress.CanPurchaseGuardianEntVitality();
                cultivateEnt.GetComponentInChildren<Text>().text = GuardianEntUpgradeCopy(progress);
            }
            for (int i = 0; i < slots.Length; i++) slots[i].GetComponentInChildren<Text>().text = FormatSlotCopy(i, layout.Slots[i]);
            planPreview?.Refresh(layout);
        }
        void DismissGuide() { if (guideStep == BuildGuideStep.Hidden) return; dismissedStep = guideStep; Refresh(); }
        void SkipGuide() { if (!FirstPlayableMinute.Skip()) return; dismissedStep = guideStep = BuildGuideStep.Hidden; guide?.Shutdown(); Refresh(); }
        string BuildGuideCopy(BuildGuideStep step, string validationReason) => journeyToken != 0 && step == BuildGuideStep.Save ? "PLAN READY — SAVE & RAID" : FirstPlayableMinute.BuildCopy(step, validationReason);
        string ReadyCopy() => journeyToken != 0 ? "Ready to raid" : "Ready to defend";
        void OnDestroy()
        {
            if (responsive) responsive.LayoutChanged -= ApplyOrientation;
            guide?.Shutdown();
            if (journeyToken != 0 && !journeyHandoff) PrototypeJourney.Cancel(journeyToken);
        }
        void ApplyOrientation(PrototypeOrientation orientation)
        {
            if (!saveButton) return;
            var landscape = orientation == PrototypeOrientation.Landscape;
            var x = landscape ? 520 : 0; var y = landscape ? 220 : 280;
            for (int i = 0; i < slots.Length; i++) slots[i].GetComponent<RectTransform>().anchoredPosition = new Vector2(x, y - i * 115);
            saveButton.GetComponent<RectTransform>().anchoredPosition = new Vector2(x, landscape ? -450 : -770);
            var copyX = landscape ? -430 : 0;
            title.rectTransform.anchoredPosition = new Vector2(copyX, landscape ? -55 : -120);
            budget.rectTransform.anchoredPosition = new Vector2(copyX, landscape ? -145 : -215);
            reason.rectTransform.anchoredPosition = new Vector2(copyX, landscape ? -235 : -310);
            if (landscape)
            {
                realmStores.rectTransform.anchorMin = realmStores.rectTransform.anchorMax = new Vector2(.5f, 1);
                realmStores.rectTransform.pivot = new Vector2(.5f, 1);
                realmStores.rectTransform.anchoredPosition = new Vector2(copyX, -470);
                PlaceUpgrade(new Vector2(.5f, 1), new Vector2(copyX, -570), new Vector2(.5f, 1));
                plan.rectTransform.anchorMin = plan.rectTransform.anchorMax = new Vector2(.5f, 1);
                plan.rectTransform.pivot = new Vector2(.5f, 1);
                plan.rectTransform.anchoredPosition = new Vector2(copyX, -340);
            }
            else
            {
                // This is the clear gap between the fixed five-slot stack and the primary save action.
                realmStores.rectTransform.anchorMin = realmStores.rectTransform.anchorMax = new Vector2(.5f, 0);
                realmStores.rectTransform.pivot = new Vector2(.5f, 0);
                realmStores.rectTransform.anchoredPosition = new Vector2(0, 260);
                // Keep the cultivation card in the clear portrait lane between the status copy
                // and the five fixed build slots. This also stays separate at scaled test sizes.
                PlaceUpgrade(new Vector2(.5f, 0), new Vector2(0, 1350), new Vector2(.5f, 0));
                plan.rectTransform.anchorMin = plan.rectTransform.anchorMax = new Vector2(.5f, 0);
                plan.rectTransform.pivot = new Vector2(.5f, 0);
                plan.rectTransform.anchoredPosition = new Vector2(0, 320);
            }
            planPreview?.ApplyOrientation(orientation);
        }
        void PlaceUpgrade(Vector2 anchor, Vector2 position, Vector2 pivot)
        {
            if (!cultivateEnt) return;
            var rect = cultivateEnt.GetComponent<RectTransform>(); rect.anchorMin = rect.anchorMax = anchor; rect.pivot = pivot; rect.anchoredPosition = position; rect.sizeDelta = new Vector2(720, 100);
        }
        public static string GuardianEntUpgradeCopy(RealmProgressData progress)
        {
            if (progress.GuardianEntVitalityRank >= RealmProgress.GuardianEntVitalityRankCap) return "GUARDIAN ENT FULLY CULTIVATED\nRANK 3/3 • +30% MAX HEALTH IN NEXT DEFENSE";
            var rank = progress.GuardianEntVitalityRank;
            var nextRank = rank + 1;
            var canAfford = progress.Gold >= RealmProgress.GuardianEntVitalityGoldCost && progress.RareMaterials >= RealmProgress.GuardianEntVitalityRareMaterialCost;
            var costLine = canAfford
                ? $"COST: {RealmProgress.GuardianEntVitalityGoldCost} GOLD • {RealmProgress.GuardianEntVitalityRareMaterialCost} RARE MATERIAL"
                : $"NEEDS: {MissingGuardianEntCost(progress)}";
            return $"CULTIVATE GUARDIAN ENT — RANK {rank}/3\nCURRENT: +{rank * 10}% MAX HEALTH IN NEXT DEFENSE\nNEXT CULTIVATION: +{nextRank * 10}% TOTAL\n{costLine}";
        }
        static string MissingGuardianEntCost(RealmProgressData progress)
        {
            var gold = Mathf.Max(0, RealmProgress.GuardianEntVitalityGoldCost - progress.Gold);
            var rare = Mathf.Max(0, RealmProgress.GuardianEntVitalityRareMaterialCost - progress.RareMaterials);
            return gold > 0 && rare > 0 ? $"{gold} GOLD • {rare} RARE MATERIAL" : gold > 0 ? $"{gold} GOLD" : $"{rare} RARE MATERIAL";
        }
        public static string FormatSlotCopy(int index, DefenseSlotLayout slot)
        {
            return $"{BuildPlanPreview.SlotName(index)}\n{BuildPlanPreview.PieceName(slot.Piece)} • {BuildPlanPreview.Role(slot.Piece)} • {DefenseLayoutRules.Cost(slot.Piece)} THREAT";
        }
        public static string FormatDefensePlan(DefenseLayout defenseLayout)
        {
            return $"DEFENSE PLAN — INVADER → HEART TREE\n{PlanStage(defenseLayout, 3)}  →  {PlanStage(defenseLayout, 0)}  →  {PlanStage(defenseLayout, 1)}\n{PlanStage(defenseLayout, 4)}  →  {PlanStage(defenseLayout, 2)}  →  HEART TREE";
        }
        static string PlanStage(DefenseLayout defenseLayout, int index)
        {
            var piece = PieceAt(defenseLayout, index);
            return $"{BuildPlanPreview.SlotName(index)}: {BuildPlanPreview.PieceName(piece)}{(piece == DefensePieceType.Ent ? " [POSSESSABLE]" : string.Empty)}";
        }
        static DefensePieceType PieceAt(DefenseLayout defenseLayout, int index) => defenseLayout?.Slots != null && index >= 0 && index < defenseLayout.Slots.Length ? defenseLayout.Slots[index].Piece : DefensePieceType.Empty;
        Text Label(string value, Vector2 position, int size) { var go = new GameObject("Label", typeof(RectTransform), typeof(Text)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, 1); rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(950, 90); var text = go.GetComponent<Text>(); text.text = value; text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = size; text.alignment = TextAnchor.UpperCenter; text.color = Color.white; return text; }
        Button Button(string value, Vector2 position, UnityEngine.Events.UnityAction action) { var go = new GameObject(value, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiPointerOwnership)); go.transform.SetParent(transform, false); var rect = (RectTransform)go.transform; rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(720, 100); go.GetComponent<Image>().color = new Color(.12f, .3f, .18f, .96f); var button = go.GetComponent<Button>(); presentation?.ApplyButton(go.GetComponent<Image>()); button.onClick.AddListener(action); button.onClick.AddListener(() => presentation?.PlayClick()); var textGo = new GameObject("Text", typeof(RectTransform), typeof(Text)); textGo.transform.SetParent(go.transform, false); var textRect = (RectTransform)textGo.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero; var label = textGo.GetComponent<Text>(); label.text = value; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 30; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; return button; }
    }
}
