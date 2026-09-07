using System.Collections;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.UI;

namespace RealmRaiders.CameraSystem
{
    public enum CombatThreatUrgency { Attacker, Attacking }

    /// <summary>Presentation-only framing and warning for one explicitly relevant nearby threat.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PrototypeCameraRig), typeof(Camera))]
    public sealed class CombatCameraAwareness : MonoBehaviour
    {
        const float NearbyDistance = 14f;
        const float RelevanceLifetime = 2.2f;
        const float ImmediateLifetime = .8f;
        const float MaxBias = 1f;
        const float EdgeLeaveInset = .06f;
        const float EdgeReturnInset = .09f;
        const float VerticalVisibilityInset = .04f;
        const float PulseDuration = .2f;
        static readonly Color Charcoal = new(.09f, .102f, .122f, .88f);
        static readonly Color Amber = new(1f, .722f, .239f, 1f);
        static readonly Color OrangeRed = new(1f, .353f, .212f, 1f);

        PrototypeCameraRig rig;
        Camera view;
        CombatEntity controlled;
        CombatEntity threat;
        CreatureBrain threatBrain;
        CombatEntity lastDamageThreat;
        float threatReportedAt = float.NegativeInfinity;
        float threatDamageAt = float.NegativeInfinity;

        ResponsiveHudRoot hud;
        RectTransform presentationRoot;
        RectTransform indicatorRoot;
        RectTransform targetPlateRoot;
        RectTransform secondaryEdgeCue;
        Image indicatorBackground;
        Image targetPlateBackground;
        Outline indicatorOutline;
        CanvasGroup indicatorGroup;
        Text indicator;
        Text targetPlate;
        CombatEntity displayedPlateTarget;
        float displayedPlateHealth = float.NaN;
        float displayedPlateMaximum = float.NaN;
        int displayedEdgeDirection;
        CombatThreatUrgency displayedEdgeUrgency;
        bool hasDisplayedEdgeState;
        bool edgeMode;
        bool edgeArrivalPulsed;
        bool immediatePulsePlayed;
        bool needsIntentReconcile;
        bool secondaryCueMoved;
        float secondaryCueOriginalY;
        Coroutine pulseRoutine;

        public bool IndicatorVisible => indicatorRoot && indicatorRoot.gameObject.activeSelf;
        public bool TargetPlateVisible => targetPlateRoot && targetPlateRoot.gameObject.activeSelf;
        public bool IndicatorRaycastTarget => indicator && indicator.raycastTarget || indicatorBackground && indicatorBackground.raycastTarget;
        public bool TargetPlateRaycastTarget => targetPlate && targetPlate.raycastTarget || targetPlateBackground && targetPlateBackground.raycastTarget;
        public string IndicatorText => IndicatorVisible ? indicator.text : string.Empty;
        public string TargetPlateText => TargetPlateVisible ? targetPlate.text : string.Empty;
        public RectTransform IndicatorRect => indicatorRoot;
        public RectTransform TargetPlateRect => targetPlateRoot;
        public Transform PresentationRoot => presentationRoot;
        public int IndicatorDirection { get; private set; }
        public int EdgePulseCount { get; private set; }
        public bool EdgePulsePlaying => pulseRoutine != null;
        public CombatThreatUrgency Urgency { get; private set; }
        public bool HasEligibleThreat => IsEligible(threat);
        public bool HasHudBinding => hud && presentationRoot;

        void Awake()
        {
            rig = GetComponent<PrototypeCameraRig>();
            view = GetComponent<Camera>();
        }

        void OnEnable()
        {
            CreatureBrain.HostileIntentChanged += ObserveHostileIntent;
            if (controlled) needsIntentReconcile = true;
        }

        public void BindHud(ResponsiveHudRoot responsiveHud, RectTransform competingEdgeCue = null)
        {
            if (hud == responsiveHud && presentationRoot)
            {
                secondaryEdgeCue = competingEdgeCue;
                return;
            }
            if (hud) hud.LayoutChanged -= ApplyLayout;
            DestroyPresentation();
            hud = responsiveHud;
            secondaryEdgeCue = competingEdgeCue;
            if (!hud) return;
            hud.LayoutChanged += ApplyLayout;
            CreatePresentation();
            ApplyLayout(hud.Orientation);
        }

        public void SetControlled(CombatEntity entity)
        {
            if (controlled == entity) return;
            if (controlled && controlled.Health != null) { controlled.Health.Damaged -= ObserveDamage; controlled.Health.Died -= Clear; }
            controlled = entity;
            if (controlled && controlled.Health != null) { controlled.Health.Damaged += ObserveDamage; controlled.Health.Died += Clear; }
            ClearThreat();
            needsIntentReconcile = controlled;
            ReconcileHostileIntent();
        }

        public void ReportThreat(CombatEntity candidate)
        {
            if (!candidate || candidate == controlled || candidate.Health == null || candidate.Health.IsDead) return;
            TrackThreat(candidate, true);
        }

        void ObserveDamage(DamageInfo hit)
        {
            var attacker = hit.Source ? hit.Source.GetComponent<CombatEntity>() : null;
            if (!attacker || attacker == controlled || attacker.Health == null || attacker.Health.IsDead) return;
            ReportThreat(attacker);
            if (threat == attacker)
            {
                lastDamageThreat = attacker;
                threatDamageAt = Time.unscaledTime;
            }
        }

        void ObserveHostileIntent(CreatureBrain brain)
        {
            if (!brain || !controlled || brain.Target != controlled || !IsHostileIntent(brain)) return;
            var candidate = brain.GetComponent<CombatEntity>();
            if (!candidate || candidate == controlled) return;
            var delta = candidate.transform.position - controlled.transform.position; delta.y = 0;
            if (delta.sqrMagnitude > NearbyDistance * NearbyDistance) return;

            // A fresh explicit report wins; otherwise keep the currently tracked hostile intent.
            if (!threat || threat == candidate || !HasFreshReport(threat) && !HasHostileIntent(threat)) TrackThreat(candidate, false);
        }

        void TrackThreat(CombatEntity candidate, bool explicitReport)
        {
            if (threat != candidate)
            {
                threat = candidate;
                threatBrain = candidate.GetComponent<CreatureBrain>();
                threatReportedAt = float.NegativeInfinity;
                lastDamageThreat = null;
                threatDamageAt = float.NegativeInfinity;
                ResetCueState();
            }
            if (explicitReport) threatReportedAt = Time.unscaledTime;
            needsIntentReconcile = false;
        }

        void FindExistingHostileIntent()
        {
            foreach (var brain in CreatureBrain.ActiveBrains) ObserveHostileIntent(brain);
        }

        void ReconcileHostileIntent()
        {
            if (!needsIntentReconcile || !CanTrackAnyThreat()) return;
            needsIntentReconcile = false;
            FindExistingHostileIntent();
        }

        void LateUpdate()
        {
            if (!CanTrackAnyThreat())
            {
                if (threat || IndicatorVisible || TargetPlateVisible || rig.HasRequestedCombatFocus) ClearThreat();
                needsIntentReconcile = controlled && controlled.Health != null && !controlled.Health.IsDead;
                return;
            }
            ReconcileHostileIntent();
            if (!threat) return;
            if (!IsEligible(threat))
            {
                ClearThreat();
                // A second pursuer may already be in Chase/Attack without producing a new state transition.
                // Reconcile once after losing a tracked threat, never as a steady-state scene scan.
                needsIntentReconcile = true;
                ReconcileHostileIntent();
                if (!IsEligible(threat)) return;
            }
            var delta = threat.transform.position - controlled.transform.position; delta.y = 0;
            // Even a close flank should be perceptible on a phone, while the rig keeps the hero primary.
            var focus = Mathf.Lerp(.5f, 1f, Mathf.Clamp01(delta.magnitude / NearbyDistance)) * MaxBias;
            rig.RequestCombatFocus(threat.transform, focus);
            UpdateCue();
        }

        bool IsEligible(CombatEntity candidate)
        {
            if (!controlled || !candidate || !controlled.Health || !candidate.Health || controlled.Health.IsDead || candidate.Health.IsDead) return false;
            if (GameplayInput.TerminalState || rig.IsTransitioning || rig.Mode == CameraMode.KeeperOverview) return false;
            var player = controlled.Controller<PlayerController>();
            if (player == null || !player.IsActive) return false;
            var delta = candidate.transform.position - controlled.transform.position; delta.y = 0;
            return delta.sqrMagnitude <= NearbyDistance * NearbyDistance && (HasFreshReport(candidate) || HasHostileIntent(candidate));
        }

        bool CanTrackAnyThreat()
        {
            if (!controlled || !controlled.Health || controlled.Health.IsDead || GameplayInput.TerminalState || rig.IsTransitioning || rig.Mode == CameraMode.KeeperOverview) return false;
            var player = controlled.Controller<PlayerController>();
            return player != null && player.IsActive;
        }

        bool HasFreshReport(CombatEntity candidate) => candidate == threat && Time.unscaledTime - threatReportedAt <= RelevanceLifetime;

        bool HasHostileIntent(CombatEntity candidate)
        {
            if (!candidate) return false;
            return IsHostileIntent(candidate == threat ? threatBrain : candidate.GetComponent<CreatureBrain>());
        }

        bool HasAttackIntent(CombatEntity candidate)
        {
            var brain = candidate == threat ? threatBrain : (candidate ? candidate.GetComponent<CreatureBrain>() : null);
            return brain && brain.IsActive && brain.Target == controlled && brain.State == BrainState.Attack;
        }

        bool IsHostileIntent(CreatureBrain brain) => brain && brain.IsActive && brain.Target == controlled && (brain.State == BrainState.Chase || brain.State == BrainState.Attack);

        void UpdateCue()
        {
            if (!HasHudBinding) return;
            var viewport = view.WorldToViewportPoint(threat.transform.position + Vector3.up);
            Urgency = HasAttackIntent(threat) || threat == lastDamageThreat && Time.unscaledTime - threatDamageAt <= ImmediateLifetime
                ? CombatThreatUrgency.Attacking
                : CombatThreatUrgency.Attacker;
            if (ShouldUseEdge(viewport, edgeMode)) ShowEdge(viewport);
            else ShowTargetPlate(viewport);
        }

        void ShowEdge(Vector3 viewport)
        {
            var entering = !IndicatorVisible;
            var becameImmediate = hasDisplayedEdgeState && displayedEdgeUrgency == CombatThreatUrgency.Attacker && Urgency == CombatThreatUrgency.Attacking;
            edgeMode = true;
            if (targetPlateRoot) targetPlateRoot.gameObject.SetActive(false);
            ClearTargetPlateCache();
            IndicatorDirection = IndicatorDirectionFor(viewport, view.transform.right, threat.transform.position - view.transform.position);
            if (!hasDisplayedEdgeState || displayedEdgeDirection != IndicatorDirection || displayedEdgeUrgency != Urgency)
            {
                displayedEdgeDirection = IndicatorDirection;
                displayedEdgeUrgency = Urgency;
                hasDisplayedEdgeState = true;
                indicator.text = Urgency == CombatThreatUrgency.Attacking
                    ? IndicatorDirection < 0 ? "◀  ATTACKING" : "ATTACKING  ▶"
                    : IndicatorDirection < 0 ? "◀  ATTACKER" : "ATTACKER  ▶";
                var color = Urgency == CombatThreatUrgency.Attacking ? OrangeRed : Amber;
                indicator.color = color;
                indicatorOutline.effectColor = color;
            }
            ApplyEdgePosition();
            indicatorRoot.gameObject.SetActive(true);
            UpdateSecondaryCue();

            if (entering && !edgeArrivalPulsed)
            {
                edgeArrivalPulsed = true;
                if (Urgency == CombatThreatUrgency.Attacking) immediatePulsePlayed = true;
                PlayEdgePulse();
            }
            else if (becameImmediate && !immediatePulsePlayed)
            {
                immediatePulsePlayed = true;
                PlayEdgePulse();
            }
        }

        void ShowTargetPlate(Vector3 viewport)
        {
            edgeMode = false;
            if (indicatorRoot) indicatorRoot.gameObject.SetActive(false);
            StopEdgePulse();
            RestoreSecondaryCue();
            var health = threat.Health.Current;
            var maximum = threat.Health.Maximum;
            if (displayedPlateTarget != threat || !Mathf.Approximately(displayedPlateHealth, health) || !Mathf.Approximately(displayedPlateMaximum, maximum))
            {
                var displayName = threat.Definition && !string.IsNullOrWhiteSpace(threat.Definition.DisplayName) ? threat.Definition.DisplayName : threat.name;
                targetPlate.text = $"ATTACKER  {displayName.ToUpperInvariant()}  {health:0}/{maximum:0} HP";
                displayedPlateTarget = threat;
                displayedPlateHealth = health;
                displayedPlateMaximum = maximum;
            }
            var orientation = hud.Orientation;
            var reference = orientation == PrototypeOrientation.Portrait ? new Vector2(1080, 1920) : new Vector2(1920, 1080);
            targetPlateRoot.anchoredPosition = TargetPlatePositionFor(viewport, Screen.safeArea, new Vector2(Screen.width, Screen.height), reference, TargetPlateSizeFor(orientation));
            targetPlateRoot.gameObject.SetActive(true);
        }

        void PlayEdgePulse()
        {
            StopEdgePulse();
            EdgePulseCount++;
            pulseRoutine = StartCoroutine(EdgePulse());
        }

        IEnumerator EdgePulse()
        {
            for (var elapsed = 0f; elapsed < PulseDuration; elapsed += Time.unscaledDeltaTime)
            {
                var amount = Mathf.SmoothStep(0, 1, elapsed / PulseDuration);
                indicatorGroup.alpha = Mathf.Lerp(.55f, 1, amount);
                indicatorRoot.localScale = Vector3.one * Mathf.Lerp(.92f, 1, amount);
                yield return null;
            }
            pulseRoutine = null;
            ResetPulseVisual();
        }

        void StopEdgePulse()
        {
            if (pulseRoutine != null) StopCoroutine(pulseRoutine);
            pulseRoutine = null;
            ResetPulseVisual();
        }

        void ResetPulseVisual()
        {
            if (indicatorGroup) indicatorGroup.alpha = 1;
            if (indicatorRoot) indicatorRoot.localScale = Vector3.one;
        }

        void ApplyLayout(PrototypeOrientation orientation)
        {
            if (!presentationRoot) return;
            indicatorRoot.sizeDelta = EdgeSizeFor(orientation);
            indicator.fontSize = orientation == PrototypeOrientation.Portrait ? 34 : 30;
            targetPlateRoot.sizeDelta = TargetPlateSizeFor(orientation);
            targetPlate.fontSize = orientation == PrototypeOrientation.Portrait ? 26 : 24;
            ApplyEdgePosition();
        }

        void ApplyEdgePosition()
        {
            if (!indicatorRoot || !hud || IndicatorDirection == 0) return;
            var portrait = hud.Orientation == PrototypeOrientation.Portrait;
            indicatorRoot.anchorMin = indicatorRoot.anchorMax = new Vector2(IndicatorDirection < 0 ? 0 : 1, portrait ? .54f : .5f);
            indicatorRoot.pivot = new Vector2(IndicatorDirection < 0 ? 0 : 1, .5f);
            var inset = portrait ? 24 : 28;
            indicatorRoot.anchoredPosition = new Vector2(IndicatorDirection < 0 ? inset : -inset, 0);
        }

        void UpdateSecondaryCue()
        {
            if (!secondaryEdgeCue || !secondaryEdgeCue.gameObject.activeSelf)
            {
                RestoreSecondaryCue();
                return;
            }
            var secondaryDirection = secondaryEdgeCue.anchorMin.x < .5f ? -1 : 1;
            if (secondaryDirection != IndicatorDirection)
            {
                RestoreSecondaryCue();
                return;
            }
            if (!secondaryCueMoved) secondaryCueOriginalY = secondaryEdgeCue.anchoredPosition.y;
            secondaryCueMoved = true;
            secondaryEdgeCue.anchoredPosition = new Vector2(secondaryEdgeCue.anchoredPosition.x, secondaryCueOriginalY - 76);
        }

        void RestoreSecondaryCue()
        {
            if (secondaryCueMoved && secondaryEdgeCue) secondaryEdgeCue.anchoredPosition = new Vector2(secondaryEdgeCue.anchoredPosition.x, secondaryCueOriginalY);
            secondaryCueMoved = false;
        }

        public static bool ShouldUseEdge(Vector3 viewport, bool currentlyEdge)
        {
            if (viewport.z <= 0 || viewport.y < VerticalVisibilityInset || viewport.y > 1 - VerticalVisibilityInset) return true;
            var inset = currentlyEdge ? EdgeReturnInset : EdgeLeaveInset;
            return viewport.x < inset || viewport.x > 1 - inset;
        }

        public static Vector2 EdgeSizeFor(PrototypeOrientation orientation) => orientation == PrototypeOrientation.Portrait ? new Vector2(300, 80) : new Vector2(260, 68);
        public static Vector2 TargetPlateSizeFor(PrototypeOrientation orientation) => orientation == PrototypeOrientation.Portrait ? new Vector2(380, 64) : new Vector2(340, 58);

        public static Vector2 TargetPlatePositionFor(Vector3 viewport, Rect safeAreaPixels, Vector2 screenSize, Vector2 referenceSize, Vector2 plateSize)
        {
            var safeWidth = Mathf.Max(1, safeAreaPixels.width);
            var safeHeight = Mathf.Max(1, safeAreaPixels.height);
            var screenPoint = new Vector2(viewport.x * Mathf.Max(1, screenSize.x), viewport.y * Mathf.Max(1, screenSize.y));
            var normalized = new Vector2((screenPoint.x - safeAreaPixels.xMin) / safeWidth, (screenPoint.y - safeAreaPixels.yMin) / safeHeight);
            var position = Vector2.Scale(normalized, referenceSize) + Vector2.up * 28;
            position.x = Mathf.Clamp(position.x, 20 + plateSize.x * .5f, referenceSize.x - 20 - plateSize.x * .5f);
            position.y = Mathf.Clamp(position.y, 20, referenceSize.y - 20 - plateSize.y);
            return position;
        }

        /// <summary>Legacy normalized clamp retained for callers that only need a safe viewport anchor.</summary>
        public static Vector2 TargetPlateAnchorFor(Vector3 viewport, Rect safeAreaPixels, Vector2 screenSize)
        {
            var width = Mathf.Max(1, screenSize.x);
            var height = Mathf.Max(1, screenSize.y);
            var safeMin = new Vector2(safeAreaPixels.xMin / width, safeAreaPixels.yMin / height);
            var safeMax = new Vector2(safeAreaPixels.xMax / width, safeAreaPixels.yMax / height);
            const float horizontalMargin = .17f;
            const float verticalMargin = .06f;
            return new Vector2(Mathf.Clamp(viewport.x, safeMin.x + horizontalMargin, safeMax.x - horizontalMargin), Mathf.Clamp(viewport.y, safeMin.y + verticalMargin, safeMax.y - verticalMargin));
        }

        public static int IndicatorDirectionFor(Vector3 viewport, Vector3 cameraRight, Vector3 worldOffset)
        {
            return viewport.z > 0 ? (viewport.x < .5f ? -1 : 1) : (Vector3.Dot(cameraRight, worldOffset) < 0 ? -1 : 1);
        }

        public void Clear()
        {
            SetControlled(null);
            ClearThreat();
        }

        public void ClearThreat()
        {
            threat = null;
            threatBrain = null;
            lastDamageThreat = null;
            threatReportedAt = float.NegativeInfinity;
            threatDamageAt = float.NegativeInfinity;
            needsIntentReconcile = false;
            IndicatorDirection = 0;
            Urgency = CombatThreatUrgency.Attacker;
            ResetCueState();
            if (rig) rig.ClearCombatFocus();
        }

        void ResetCueState()
        {
            edgeMode = false;
            edgeArrivalPulsed = false;
            immediatePulsePlayed = false;
            displayedEdgeDirection = 0;
            hasDisplayedEdgeState = false;
            if (indicatorRoot) indicatorRoot.gameObject.SetActive(false);
            StopEdgePulse();
            RestoreSecondaryCue();
            ClearTargetPlate();
        }

        void ClearTargetPlate()
        {
            ClearTargetPlateCache();
            if (targetPlateRoot) targetPlateRoot.gameObject.SetActive(false);
        }

        void ClearTargetPlateCache()
        {
            displayedPlateTarget = null;
            displayedPlateHealth = float.NaN;
            displayedPlateMaximum = float.NaN;
        }

        void OnDisable()
        {
            CreatureBrain.HostileIntentChanged -= ObserveHostileIntent;
            ClearThreat();
        }

        void OnDestroy()
        {
            CreatureBrain.HostileIntentChanged -= ObserveHostileIntent;
            if (controlled && controlled.Health != null) { controlled.Health.Damaged -= ObserveDamage; controlled.Health.Died -= Clear; }
            if (hud) hud.LayoutChanged -= ApplyLayout;
            if (rig) rig.ClearCombatFocus();
            DestroyPresentation();
        }

        void CreatePresentation()
        {
            var root = new GameObject("Combat Threat Presentation", typeof(RectTransform));
            root.transform.SetParent(hud.transform, false);
            presentationRoot = (RectTransform)root.transform;
            presentationRoot.anchorMin = Vector2.zero;
            presentationRoot.anchorMax = Vector2.one;
            presentationRoot.offsetMin = presentationRoot.offsetMax = Vector2.zero;
            presentationRoot.SetAsLastSibling();

            var edge = new GameObject("Threat Edge Tab", typeof(RectTransform), typeof(CanvasGroup), typeof(Image), typeof(Outline));
            edge.transform.SetParent(presentationRoot, false);
            indicatorRoot = (RectTransform)edge.transform;
            indicatorGroup = edge.GetComponent<CanvasGroup>();
            indicatorGroup.blocksRaycasts = false;
            indicatorGroup.interactable = false;
            indicatorBackground = edge.GetComponent<Image>();
            indicatorBackground.color = Charcoal;
            indicatorBackground.raycastTarget = false;
            indicatorOutline = edge.GetComponent<Outline>();
            indicatorOutline.effectDistance = new Vector2(2, -2);
            indicator = CreateLabel("Threat Edge Text", indicatorRoot);
            indicator.fontStyle = FontStyle.Bold;

            var plate = new GameObject("Threat Target Plate", typeof(RectTransform), typeof(Image), typeof(Outline));
            plate.transform.SetParent(presentationRoot, false);
            targetPlateRoot = (RectTransform)plate.transform;
            targetPlateRoot.anchorMin = targetPlateRoot.anchorMax = Vector2.zero;
            targetPlateRoot.pivot = new Vector2(.5f, 0);
            targetPlateBackground = plate.GetComponent<Image>();
            targetPlateBackground.color = Charcoal;
            targetPlateBackground.raycastTarget = false;
            var plateOutline = plate.GetComponent<Outline>();
            plateOutline.effectColor = Amber;
            plateOutline.effectDistance = new Vector2(2, -2);
            targetPlate = CreateLabel("Threat Target Text", targetPlateRoot);
            targetPlate.fontStyle = FontStyle.Bold;
            targetPlate.color = Amber;

            indicatorRoot.gameObject.SetActive(false);
            targetPlateRoot.gameObject.SetActive(false);
        }

        static Text CreateLabel(string name, Transform parent)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(Text));
            label.transform.SetParent(parent, false);
            var rect = (RectTransform)label.transform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var text = label.GetComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.alignment = TextAnchor.MiddleCenter;
            text.raycastTarget = false;
            return text;
        }

        void DestroyPresentation()
        {
            StopEdgePulse();
            RestoreSecondaryCue();
            if (presentationRoot)
            {
                presentationRoot.gameObject.SetActive(false);
                Destroy(presentationRoot.gameObject);
            }
            presentationRoot = null;
            indicatorRoot = null;
            targetPlateRoot = null;
            indicatorBackground = null;
            targetPlateBackground = null;
            indicatorOutline = null;
            indicatorGroup = null;
            indicator = null;
            targetPlate = null;
        }
    }
}
