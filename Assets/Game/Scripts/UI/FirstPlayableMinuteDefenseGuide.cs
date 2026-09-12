using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.Possession;
using RealmRaiders.Raid;
using UnityEngine;
using UnityEngine.UI;

namespace RealmRaiders.UI
{
    /// <summary>Scene-local observer and presentation for the ordered Sylvan possession proof.</summary>
    [DisallowMultipleComponent]
    public sealed class FirstPlayableMinuteDefenseGuide : MonoBehaviour
    {
        const float RequiredMovement = .6f;
        public const string PossessableMarkerObjectName = "First Minute Possessable Ent Marker";
        public const string PossessableMarkerCopy = "▼  POSSESSABLE ENT";
        public const string PrematureReleaseCopy = "RELEASED EARLY — SELECT THE ENT TO TRY AGAIN";
        static readonly Vector2 PossessableMarkerSize = new(300, 52);

        ResponsiveHudRoot responsive;
        PossessionManager possession;
        PossessionEnergy energy;
        DefenseManager defense;
        CombatEntity defender;
        PrototypeCameraRig cameraRig;
        PlayerController player;
        Text normalSelection;
        Text resultLane;
        Text line;
        Button possessButton;
        Button smashButton;
        Button dodgeButton;
        Button releaseButton;
        Button retryButton;
        Button returnToBuildButton;
        Button dismissButton;
        Button skipButton;
        RectTransform emphasis;
        Text possessableMarker;
        Camera view;
        Canvas canvas;
        FirstPlayableMinuteDefenseProof proof;
        string dismissedGroup;
        string displayedCopy;
        string displayedGroup;
        string resultBaseCopy;
        RectTransform displayedTarget;
        bool selectionSuppressed;
        bool selectionWasActive;
        bool explicitReleaseRequested;
        bool explicitReleasePending;
        bool prematureReleaseExplanation;
        bool moveStartCaptured;
        bool resultGuideApplied;
        bool guideActionsActive = true;
        bool shutdown;
        int defenseSceneToken;
        Vector3 moveStart;
        Vector3 acceptedMovement;

        public DefenseGuideStep Step => proof?.Step ?? DefenseGuideStep.Inactive;
        public DefenseGuideTerminalOutcome TerminalOutcome => proof?.TerminalOutcome ?? DefenseGuideTerminalOutcome.None;
        public string GuideText => resultGuideApplied ? displayedCopy : line && line.gameObject.activeSelf ? line.text : string.Empty;
        public bool GuideLineVisible => resultGuideApplied || line && line.gameObject.activeSelf;
        public bool GuideLineRaycastTarget => resultGuideApplied ? resultLane && resultLane.raycastTarget : line && line.raycastTarget;
        public bool EmphasisVisible => emphasis && emphasis.gameObject.activeSelf;
        public string EmphasisTargetName => displayedTarget ? displayedTarget.name : string.Empty;
        public bool DismissVisible => dismissButton && dismissButton.gameObject.activeSelf;
        public bool SkipVisible => skipButton && skipButton.gameObject.activeSelf;
        public RectTransform DismissRect => dismissButton ? (RectTransform)dismissButton.transform : null;
        public RectTransform SkipRect => skipButton ? (RectTransform)skipButton.transform : null;
        public RectTransform EmphasisRect => emphasis;
        public RectTransform GuideLineRect => line ? line.rectTransform : null;
        public RectTransform PossessableMarkerRect => possessableMarker ? possessableMarker.rectTransform : null;
        public bool PossessableMarkerVisible => possessableMarker && possessableMarker.gameObject.activeSelf;

        public void Initialize(
            ResponsiveHudRoot layout,
            PossessionManager manager,
            PossessionEnergy possessionEnergy,
            DefenseManager defenseManager,
            CombatEntity sylvanDefender,
            Text selectionLane,
            Text defenseResultLane,
            Button possessAction,
            Button smashAction,
            Button dodgeAction,
            Button releaseAction,
            Button retryAction,
            Button returnToBuildAction,
            HudPresentation skin,
            int sceneToken)
        {
            responsive = layout;
            possession = manager;
            energy = possessionEnergy;
            defense = defenseManager;
            defender = sylvanDefender;
            cameraRig = manager.CameraRig;
            view = cameraRig ? cameraRig.GetComponent<Camera>() : null;
            canvas = GetComponentInParent<Canvas>();
            normalSelection = selectionLane;
            resultLane = defenseResultLane;
            possessButton = possessAction;
            smashButton = smashAction;
            dodgeButton = dodgeAction;
            releaseButton = releaseAction;
            retryButton = retryAction;
            returnToBuildButton = returnToBuildAction;
            defenseSceneToken = sceneToken;
            proof = new FirstPlayableMinuteDefenseProof(true);

            var rect = (RectTransform)transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            line = CreateLine();
            emphasis = CreateEmphasis();
            possessableMarker = CreatePossessableMarker();
            dismissButton = CreateAction("DISMISS", skin, DismissCurrent, new Color(.15f, .23f, .18f, .98f));
            skipButton = CreateAction("SKIP GUIDE", skin, SkipGuide, new Color(.18f, .18f, .2f, .98f));

            responsive.LayoutChanged += OnLayoutChanged;
            possession.SelectionChanged += OnSelectionChanged;
            possession.PossessionChanged += OnPossessionChanged;
            possession.Released += OnReleased;
            defense.StateChanged += OnDefenseState;
            if (defender && defender.Health != null) defender.Health.Died += OnDefenderDied;
            ApplyLayout(responsive.Orientation);
            RefreshPresentation(true);
        }

        public void ObserveSmash(CombatEntity actor, PlayerController controller, bool accepted)
        {
            if (shutdown || !accepted || actor != defender || controller != player || !controller.IsActive || actor.ActionPhase == CombatActionPhase.Idle) return;
            if (proof.TryAttack()) OnStepChanged();
        }

        public void ObserveDodge(CombatEntity actor, PlayerController controller, bool accepted)
        {
            if (shutdown || !accepted || actor != defender || controller != player || !controller.IsActive || !actor.IsDodging) return;
            if (proof.TryDodge()) OnStepChanged();
        }

        public void BeginExplicitRelease(CombatEntity actor)
        {
            explicitReleaseRequested = false;
            explicitReleasePending = false;
            if (shutdown || proof == null || proof.TerminalOutcome != DefenseGuideTerminalOutcome.None || actor != defender
                || possession.Possessed != defender || defender.Health == null || defender.Health.IsDead) return;
            explicitReleaseRequested = proof.Step is DefenseGuideStep.Move or DefenseGuideStep.Attack or DefenseGuideStep.Dodge or DefenseGuideStep.Release;
            explicitReleasePending = explicitReleaseRequested && proof.Step == DefenseGuideStep.Release;
        }

        public void PrepareRetry()
        {
            if (!shutdown && proof.TerminalOutcome == DefenseGuideTerminalOutcome.Retry)
                FirstPlayableMinute.PrepareDefenseRetry(defenseSceneToken);
        }

        public void RefreshForTests() => RefreshPresentation(true);

        void Update()
        {
            if (shutdown || proof == null) return;
            if (proof.TerminalOutcome != DefenseGuideTerminalOutcome.None) { HidePossessableMarker(); return; }
            if (proof.Step == DefenseGuideStep.Select && !CanAttemptAgain()) { if (proof.Interrupt(false)) OnStepChanged(); }
            else if (proof.Step == DefenseGuideStep.Possess && (possession.Selected != defender || !CanAttemptAgain())) { if (proof.Interrupt(CanAttemptAgain())) OnStepChanged(); }
            else if (proof.Step is DefenseGuideStep.Move or DefenseGuideStep.Attack or DefenseGuideStep.Dodge or DefenseGuideStep.Release)
            {
                if (possession.Possessed != defender || !player || !player.IsActive || defender.Health.IsDead)
                {
                    UnbindPlayer();
                    if (proof.Interrupt(CanAttemptAgain())) OnStepChanged();
                }
            }

            if (proof.Step == DefenseGuideStep.Move && !moveStartCaptured && PossessionCameraSettled() && !defender.IsActionResolving && !defender.IsDodging)
            {
                moveStart = defender.transform.position;
                acceptedMovement = Vector3.zero;
                moveStartCaptured = true;
            }
            if (proof.Step == DefenseGuideStep.KeeperReturn && KeeperReturnReady() && proof.TryKeeperReturn()) OnStepChanged();
            RefreshPresentation(false);
        }

        void OnSelectionChanged(CombatEntity selected)
        {
            if (shutdown || proof.TerminalOutcome != DefenseGuideTerminalOutcome.None) return;
            if (selected == defender)
            {
                HidePossessableMarker();
                if (defender.IsPossessable && proof.TrySelect()) OnStepChanged();
            }
            else if (!selected && proof.Step == DefenseGuideStep.Possess && proof.Interrupt(CanAttemptAgain())) OnStepChanged();
        }

        void OnPossessionChanged(CombatEntity controlled)
        {
            if (shutdown || proof.TerminalOutcome != DefenseGuideTerminalOutcome.None) return;
            if (controlled) HidePossessableMarker();
            if (controlled == defender)
            {
                BindPlayer(defender.Controller<PlayerController>());
                if (proof.TryPossess()) OnStepChanged();
                return;
            }
            if (explicitReleaseRequested) return;
            UnbindPlayer();
            if (proof.Interrupt(CanAttemptAgain())) OnStepChanged();
        }

        void OnReleased(bool forced)
        {
            if (shutdown || proof.TerminalOutcome != DefenseGuideTerminalOutcome.None) return;
            var requested = explicitReleaseRequested;
            var accepted = requested && explicitReleasePending && !forced && proof.Step == DefenseGuideStep.Release;
            var prematureStep = proof.Step is DefenseGuideStep.Move or DefenseGuideStep.Attack or DefenseGuideStep.Dodge;
            var explainPrematureRelease = requested && !explicitReleasePending && !forced && prematureStep;
            explicitReleaseRequested = false;
            explicitReleasePending = false;
            UnbindPlayer();
            if (accepted)
            {
                if (proof.TryRelease()) OnStepChanged();
            }
            else
            {
                var canRetry = CanAttemptAgain();
                if (proof.Interrupt(canRetry)) OnStepChanged(explainPrematureRelease && canRetry);
                else prematureReleaseExplanation = false;
            }
        }

        void OnLocomotionAccepted(PlayerController source, Vector3 displacement)
        {
            if (shutdown || proof.Step != DefenseGuideStep.Move || source != player || !moveStartCaptured || defender.IsActionResolving || defender.IsDodging || defender.IsRooted) return;
            displacement.y = 0; acceptedMovement += displacement;
            var fromStart = defender.transform.position - moveStart; fromStart.y = 0;
            if (acceptedMovement.magnitude >= RequiredMovement && fromStart.magnitude >= RequiredMovement && proof.TryMove()) OnStepChanged();
        }

        void OnDefenderDied()
        {
            HidePossessableMarker();
            explicitReleaseRequested = false;
            explicitReleasePending = false;
            prematureReleaseExplanation = false;
            UnbindPlayer();
            if (!shutdown && proof.TerminalOutcome == DefenseGuideTerminalOutcome.None && proof.Interrupt(false)) OnStepChanged();
        }

        void OnDefenseState(DefenseState state)
        {
            if (shutdown || state is not (DefenseState.DefenderVictory or DefenseState.RealmLost)) return;
            HidePossessableMarker();
            explicitReleaseRequested = false;
            explicitReleasePending = false;
            prematureReleaseExplanation = false;
            UnbindPlayer();
            if (proof.Step == DefenseGuideStep.KeeperReturn && KeeperReturnReady()) proof.TryKeeperReturn();
            var outcome = proof.ReachTerminal();
            if (outcome == DefenseGuideTerminalOutcome.Completed) { FirstPlayableMinute.TryComplete(); guideActionsActive = false; }
            if (outcome != DefenseGuideTerminalOutcome.None) OnStepChanged();
        }

        void BindPlayer(PlayerController value)
        {
            if (player == value) return;
            UnbindPlayer();
            player = value;
            if (player) player.LocomotionAccepted += OnLocomotionAccepted;
            moveStartCaptured = false;
        }

        void UnbindPlayer()
        {
            if (player) player.LocomotionAccepted -= OnLocomotionAccepted;
            player = null;
            moveStartCaptured = false;
            acceptedMovement = Vector3.zero;
        }

        bool CanAttemptAgain() => defender && defender.IsPossessable && energy != null && !energy.IsDepleted && possession && !possession.IsPossessing && defense && !defense.IsFinished;
        bool PossessionCameraSettled() => cameraRig && !cameraRig.IsTransitioning && cameraRig.Mode == CameraMode.PossessedCreature && possession.Possessed == defender;
        bool KeeperReturnReady() => cameraRig && !cameraRig.IsTransitioning && cameraRig.Mode == CameraMode.KeeperOverview && !possession.IsPossessing && defender && defender.Health != null && !defender.Health.IsDead && defender.gameObject.scene.IsValid() && defender.gameObject.scene.isLoaded;

        void OnStepChanged(bool explainPrematureRelease = false)
        {
            prematureReleaseExplanation = explainPrematureRelease;
            dismissedGroup = null;
            moveStartCaptured = false;
            acceptedMovement = Vector3.zero;
            explicitReleaseRequested = false;
            explicitReleasePending = false;
            ApplyLayout(responsive.Orientation);
            RefreshPresentation(true);
        }

        void RefreshPresentation(bool force)
        {
            if (shutdown || proof == null) return;
            Presentation(out var group, out var copy, out var target);
            var show = !string.IsNullOrEmpty(copy) && dismissedGroup != group;
            if (!force && displayedGroup == group && displayedCopy == copy && displayedTarget == target && GuideLineVisible == show)
            {
                RefreshPossessableMarker();
                return;
            }
            displayedGroup = group;
            displayedCopy = copy;
            displayedTarget = target;
            ApplyLineLayout(group, responsive.Orientation);
            var terminalPresentation = proof.TerminalOutcome != DefenseGuideTerminalOutcome.None;
            SetGuideLine(!terminalPresentation && show ? copy : string.Empty);
            SetResultGuide(terminalPresentation && show ? copy : string.Empty);
            ApplyEmphasis(show ? target : null);
            dismissButton.gameObject.SetActive(guideActionsActive && show);
            skipButton.gameObject.SetActive(guideActionsActive);
            RefreshPossessableMarker();
        }

        void Presentation(out string group, out string copy, out RectTransform target)
        {
            group = proof.Step switch
            {
                DefenseGuideStep.Select => "Select",
                DefenseGuideStep.Possess => "Possess",
                DefenseGuideStep.Move => "Move",
                DefenseGuideStep.Attack => "Attack",
                DefenseGuideStep.Dodge => "Dodge",
                DefenseGuideStep.Release => "Release",
                DefenseGuideStep.KeeperReturn => "KeeperReturn",
                DefenseGuideStep.Result => "Result",
                DefenseGuideStep.Retry => "Retry",
                _ => "Inactive"
            };
            copy = string.Empty; target = null;
            switch (proof.Step)
            {
                case DefenseGuideStep.Select:
                    copy = !CanAttemptAgain() ? string.Empty
                        : prematureReleaseExplanation ? KeeperReturnReady() ? PrematureReleaseCopy : string.Empty
                        : "SELECT — TAP THE ENT";
                    break;
                case DefenseGuideStep.Possess:
                    copy = possession.Selected == defender && CanAttemptAgain() ? "TAKE CONTROL — TAP POSSESS ENT" : string.Empty;
                    target = copy.Length > 0 && possessButton && possessButton.gameObject.activeInHierarchy ? (RectTransform)possessButton.transform : null;
                    break;
                case DefenseGuideStep.Move:
                    if (!PossessionCameraSettled()) break;
                    var joystick = PrototypeSave.EffectiveControlStyle(responsive.Orientation == PrototypeOrientation.Landscape) == "Joystick";
                    copy = joystick ? "MOVE — DRAG THE JOYSTICK" : "MOVE — TAP OPEN GROUND";
                    target = joystick && responsive.JoystickRect && responsive.JoystickRect.gameObject.activeInHierarchy ? responsive.JoystickRect : null;
                    break;
                case DefenseGuideStep.Attack:
                    if (defender.IsActionResolving)
                    {
                        group = "AttackWait";
                        copy = "WAIT — ACTION IN PROGRESS";
                    }
                    else if (defender.Abilities.Count > 0 && !defender.Abilities[0].IsReady)
                    {
                        group = "AttackRecovering";
                        copy = "SMASH RECOVERING — KEEP MOVING";
                    }
                    else if (defender.Abilities.Count > 0)
                    {
                        copy = "ATTACK — TAP SMASH";
                        target = smashButton && smashButton.gameObject.activeInHierarchy ? (RectTransform)smashButton.transform : null;
                    }
                    break;
                case DefenseGuideStep.Dodge:
                    if (defender.IsRooted) { group = "DodgeRoot"; copy = "ROOTED — TAP THE WORLD TO BREAK FREE"; }
                    else if (defender.IsActionResolving) { group = "DodgeWait"; copy = "WAIT — ACTION IN PROGRESS"; }
                    else if (defender.CanDodge) { group = "DodgeReady"; copy = "ESCAPE — TAP DODGE"; target = dodgeButton && dodgeButton.gameObject.activeInHierarchy ? (RectTransform)dodgeButton.transform : null; }
                    break;
                case DefenseGuideStep.Release:
                    copy = "RETURN — TAP RELEASE";
                    target = releaseButton && releaseButton.gameObject.activeInHierarchy ? (RectTransform)releaseButton.transform : null;
                    break;
                case DefenseGuideStep.KeeperReturn:
                case DefenseGuideStep.Result:
                    copy = "KEEPER VIEW — WATCH THE RESULT";
                    break;
                case DefenseGuideStep.Retry:
                    group = "Retry"; copy = "TRY THE CONTROL LOOP — DEFEND AGAIN";
                    target = retryButton && retryButton.gameObject.activeInHierarchy ? (RectTransform)retryButton.transform : null;
                    break;
            }

            if (proof.TerminalOutcome == DefenseGuideTerminalOutcome.Completed)
            {
                if (defense.State == DefenseState.DefenderVictory)
                {
                    group = "CompleteWin"; copy = "FIRST DEFENSE COMPLETE — RETURN TO BUILD";
                    target = returnToBuildButton && returnToBuildButton.gameObject.activeInHierarchy ? (RectTransform)returnToBuildButton.transform : null;
                }
                else
                {
                    group = "CompleteLoss"; copy = "REALM LOST — RETURN TO BUILD AND ADJUST DEFENCES";
                    target = returnToBuildButton && returnToBuildButton.gameObject.activeInHierarchy ? (RectTransform)returnToBuildButton.transform : null;
                }
            }
        }

        void DismissCurrent()
        {
            if (shutdown || string.IsNullOrEmpty(displayedGroup)) return;
            dismissedGroup = displayedGroup;
            RefreshPresentation(true);
        }

        void SkipGuide()
        {
            if (!FirstPlayableMinute.Skip()) return;
            Shutdown();
        }

        Text CreateLine()
        {
            var go = new GameObject("First Minute Guide Line", typeof(RectTransform), typeof(Text)); go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform; rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1); rect.pivot = new Vector2(.5f, 1); rect.anchoredPosition = new Vector2(0, -365); rect.sizeDelta = new Vector2(0, 50);
            var text = go.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); text.fontSize = 28; text.alignment = TextAnchor.UpperCenter; text.color = new Color(1f, .9f, .52f); text.raycastTarget = false; go.SetActive(false); return text;
        }

        RectTransform CreateEmphasis()
        {
            var go = new GameObject("First Minute Guide Emphasis", typeof(RectTransform), typeof(Image), typeof(Outline)); go.transform.SetParent(transform, false);
            var image = go.GetComponent<Image>(); image.color = new Color(1f, .78f, .24f, .07f); image.raycastTarget = false;
            var outline = go.GetComponent<Outline>(); outline.effectColor = new Color(1f, .82f, .34f, .96f); outline.effectDistance = new Vector2(4, -4); outline.useGraphicAlpha = false;
            go.SetActive(false); return (RectTransform)go.transform;
        }

        Text CreatePossessableMarker()
        {
            var go = new GameObject(PossessableMarkerObjectName, typeof(RectTransform), typeof(Text), typeof(Outline));
            go.transform.SetParent(transform, false);
            var rect = (RectTransform)go.transform;
            rect.anchorMin = rect.anchorMax = new Vector2(.5f, .5f);
            rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = PossessableMarkerSize;
            var text = go.GetComponent<Text>();
            text.text = PossessableMarkerCopy;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = 22;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(1f, .9f, .52f);
            text.raycastTarget = false;
            var outline = go.GetComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, .9f);
            outline.effectDistance = new Vector2(2, -2);
            go.SetActive(false);
            return text;
        }

        void RefreshPossessableMarker()
        {
            if (!possessableMarker) return;
            var visible = !shutdown && proof != null && proof.Step == DefenseGuideStep.Select
                && proof.TerminalOutcome == DefenseGuideTerminalOutcome.None && dismissedGroup != "Select"
                && !GameplayInput.TerminalState && CanAttemptAgain() && defender.Health != null && !defender.Health.IsDead
                && cameraRig && cameraRig.Mode == CameraMode.KeeperOverview && !cameraRig.IsTransitioning;
            if (!visible || !view)
            {
                HidePossessableMarker();
                return;
            }

            var root = (RectTransform)transform;
            var screenPoint = view.WorldToScreenPoint(defender.transform.TransformPoint(Vector3.up * 2.8f));
            var eventCamera = canvas && canvas.renderMode != RenderMode.ScreenSpaceOverlay ? canvas.worldCamera : null;
            if (screenPoint.z <= 0 || !RectTransformUtility.ScreenPointToLocalPointInRectangle(root, screenPoint, eventCamera, out var localPoint))
            {
                HidePossessableMarker();
                return;
            }

            var bounds = root.rect;
            var halfSize = PossessableMarkerSize * .5f;
            const float margin = 12;
            localPoint.x = Mathf.Clamp(localPoint.x, bounds.xMin + halfSize.x + margin, bounds.xMax - halfSize.x - margin);
            localPoint.y = Mathf.Clamp(localPoint.y, bounds.yMin + halfSize.y + margin, bounds.yMax - halfSize.y - margin);
            possessableMarker.rectTransform.anchoredPosition = localPoint - bounds.center;
            possessableMarker.gameObject.SetActive(true);
        }

        void HidePossessableMarker()
        {
            if (possessableMarker) possessableMarker.gameObject.SetActive(false);
        }

        Button CreateAction(string name, HudPresentation skin, UnityEngine.Events.UnityAction action, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(UiPointerOwnership)); go.transform.SetParent(transform, false);
            var image = go.GetComponent<Image>(); image.color = color; skin?.ApplyButton(image);
            var button = go.GetComponent<Button>(); button.onClick.AddListener(() => skin?.PlayClick()); button.onClick.AddListener(action);
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text)); textObject.transform.SetParent(go.transform, false);
            var textRect = (RectTransform)textObject.transform; textRect.anchorMin = Vector2.zero; textRect.anchorMax = Vector2.one; textRect.offsetMin = textRect.offsetMax = Vector2.zero;
            var label = textObject.GetComponent<Text>(); label.text = name; label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize = 21; label.alignment = TextAnchor.MiddleCenter; label.color = Color.white; label.raycastTarget = false;
            return button;
        }

        void OnLayoutChanged(PrototypeOrientation orientation)
        {
            if (shutdown) return;
            ApplyLayout(orientation);
            RefreshPresentation(true);
        }

        void ApplyLayout(PrototypeOrientation orientation)
        {
            if (proof != null && proof.TerminalOutcome != DefenseGuideTerminalOutcome.None)
            {
                if (orientation == PrototypeOrientation.Portrait)
                {
                    PlaceAction(dismissButton, new Vector2(40, 360), new Vector2(280, 96), Vector2.zero, Vector2.zero);
                    PlaceAction(skipButton, new Vector2(-40, 360), new Vector2(280, 96), new Vector2(1, 0), new Vector2(1, 0));
                }
                else
                {
                    PlaceAction(dismissButton, new Vector2(100, 190), new Vector2(300, 96), Vector2.zero, Vector2.zero);
                    PlaceAction(skipButton, new Vector2(420, 190), new Vector2(300, 96), Vector2.zero, Vector2.zero);
                }
                return;
            }
            if (orientation == PrototypeOrientation.Portrait)
            {
                PlaceAction(dismissButton, new Vector2(-20, 590), new Vector2(300, 96), new Vector2(1, 0), new Vector2(1, 0));
                PlaceAction(skipButton, new Vector2(-20, 480), new Vector2(300, 96), new Vector2(1, 0), new Vector2(1, 0));
            }
            else
            {
                PlaceAction(dismissButton, new Vector2(340, 360), new Vector2(300, 96), Vector2.zero, Vector2.zero);
                PlaceAction(skipButton, new Vector2(340, 250), new Vector2(300, 96), Vector2.zero, Vector2.zero);
            }
        }

        void ApplyLineLayout(string group, PrototypeOrientation orientation)
        {
            if (!line) return;
            var rect = line.rectTransform;
            if (group == "DodgeRoot")
            {
                rect.anchorMin = rect.anchorMax = Vector2.zero; rect.pivot = Vector2.zero;
                rect.anchoredPosition = orientation == PrototypeOrientation.Portrait ? new Vector2(55, 610) : new Vector2(460, 470);
                rect.sizeDelta = orientation == PrototypeOrientation.Portrait ? new Vector2(650, 50) : new Vector2(1000, 50);
                return;
            }
            rect.anchorMin = new Vector2(0, 1); rect.anchorMax = new Vector2(1, 1); rect.pivot = new Vector2(.5f, 1);
            rect.anchoredPosition = new Vector2(0, -365); rect.sizeDelta = new Vector2(0, 50);
        }

        static void PlaceAction(Button button, Vector2 position, Vector2 size, Vector2 anchor, Vector2 pivot)
        {
            var rect = (RectTransform)button.transform; rect.anchorMin = rect.anchorMax = anchor; rect.pivot = pivot; rect.anchoredPosition = position; rect.sizeDelta = size;
        }

        void SetGuideLine(string copy)
        {
            var visible = !string.IsNullOrEmpty(copy);
            if (visible && !selectionSuppressed)
            {
                selectionWasActive = normalSelection && normalSelection.gameObject.activeSelf;
                if (normalSelection) normalSelection.gameObject.SetActive(false);
                selectionSuppressed = true;
            }
            else if (!visible) RestoreSelection();
            if (line) { line.text = copy; line.gameObject.SetActive(visible); }
        }

        void RestoreSelection()
        {
            if (!selectionSuppressed) return;
            if (normalSelection) normalSelection.gameObject.SetActive(selectionWasActive);
            selectionSuppressed = false;
        }

        void SetResultGuide(string copy)
        {
            if (!resultLane) return;
            if (!string.IsNullOrEmpty(copy))
            {
                if (!resultGuideApplied) resultBaseCopy = resultLane.text;
                resultLane.text = resultBaseCopy + "\n\n" + copy;
                resultGuideApplied = true;
            }
            else RestoreResultLane();
        }

        void RestoreResultLane()
        {
            if (!resultGuideApplied) return;
            if (resultLane) resultLane.text = resultBaseCopy;
            resultGuideApplied = false;
            resultBaseCopy = string.Empty;
        }

        void ApplyEmphasis(RectTransform target)
        {
            if (!emphasis) return;
            if (!target) { emphasis.gameObject.SetActive(false); return; }
            var corners = new Vector3[4]; target.GetWorldCorners(corners);
            var min = new Vector2(float.PositiveInfinity, float.PositiveInfinity); var max = new Vector2(float.NegativeInfinity, float.NegativeInfinity);
            foreach (var corner in corners)
            {
                var local = (Vector2)transform.InverseTransformPoint(corner); min = Vector2.Min(min, local); max = Vector2.Max(max, local);
            }
            emphasis.anchorMin = emphasis.anchorMax = new Vector2(.5f, .5f); emphasis.pivot = new Vector2(.5f, .5f);
            emphasis.anchoredPosition = (min + max) * .5f; emphasis.sizeDelta = max - min + new Vector2(16, 16); emphasis.gameObject.SetActive(true);
        }

        public void Shutdown()
        {
            if (shutdown) return;
            shutdown = true;
            if (responsive) responsive.LayoutChanged -= OnLayoutChanged;
            if (possession)
            {
                possession.SelectionChanged -= OnSelectionChanged;
                possession.PossessionChanged -= OnPossessionChanged;
                possession.Released -= OnReleased;
            }
            if (defense) defense.StateChanged -= OnDefenseState;
            if (defender && defender.Health != null) defender.Health.Died -= OnDefenderDied;
            UnbindPlayer();
            if (dismissButton) { dismissButton.onClick.RemoveAllListeners(); dismissButton.gameObject.SetActive(false); }
            if (skipButton) { skipButton.onClick.RemoveAllListeners(); skipButton.gameObject.SetActive(false); }
            if (line) line.gameObject.SetActive(false);
            if (emphasis) emphasis.gameObject.SetActive(false);
            HidePossessableMarker();
            explicitReleaseRequested = false;
            explicitReleasePending = false;
            prematureReleaseExplanation = false;
            RestoreSelection();
            RestoreResultLane();
            displayedTarget = null;
            FirstPlayableMinute.EndDefenseScene(defenseSceneToken);
            defenseSceneToken = 0;
        }

        void OnDisable() => Shutdown();
        void OnDestroy() => Shutdown();
    }
}
