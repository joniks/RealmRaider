using System;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using UnityEngine;
using UnityEngine.UI;

namespace RealmRaiders.CameraSystem
{
    /// <summary>Presentation-only framing and edge warning for an explicitly relevant nearby threat.</summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PrototypeCameraRig), typeof(Camera))]
    public sealed class CombatCameraAwareness : MonoBehaviour
    {
        const float NearbyDistance = 14f;
        const float RelevanceLifetime = 2.2f;
        const float MaxBias = 1f;

        PrototypeCameraRig rig;
        Camera view;
        CombatEntity controlled;
        CombatEntity threat;
        float threatReportedAt = float.NegativeInfinity;
        Text indicator;
        Text targetPlate;
        CombatEntity displayedPlateTarget;
        float displayedPlateHealth = float.NaN;
        float displayedPlateMaximum = float.NaN;

        public bool IndicatorVisible => indicator && indicator.gameObject.activeSelf;
        public bool TargetPlateVisible => targetPlate && targetPlate.gameObject.activeSelf;
        public bool TargetPlateRaycastTarget => targetPlate && targetPlate.raycastTarget;
        public string TargetPlateText => TargetPlateVisible ? targetPlate.text : string.Empty;
        public int IndicatorDirection { get; private set; }
        public bool HasEligibleThreat => IsEligible(threat);

        void Awake()
        {
            rig = GetComponent<PrototypeCameraRig>();
            view = GetComponent<Camera>();
            CreateIndicator();
        }

        void OnEnable() => CreatureBrain.HostileIntentChanged += ObserveHostileIntent;

        public void SetControlled(CombatEntity entity)
        {
            if (controlled == entity) return;
            if (controlled && controlled.Health != null) { controlled.Health.Damaged -= ObserveDamage; controlled.Health.Died -= Clear; }
            controlled = entity;
            if (controlled && controlled.Health != null) { controlled.Health.Damaged += ObserveDamage; controlled.Health.Died += Clear; }
            ClearThreat();
            if (controlled) FindExistingHostileIntent();
        }

        public void ReportThreat(CombatEntity candidate)
        {
            if (!candidate || candidate == controlled || candidate.Health == null || candidate.Health.IsDead) return;
            threat = candidate;
            threatReportedAt = Time.unscaledTime;
        }

        void ObserveDamage(DamageInfo hit)
        {
            var attacker = hit.Source ? hit.Source.GetComponent<CombatEntity>() : null;
            ReportThreat(attacker);
        }

        void ObserveHostileIntent(CreatureBrain brain)
        {
            if (!brain || !controlled || brain.Target != controlled || !IsHostileIntent(brain)) return;
            var candidate = brain.GetComponent<CombatEntity>();
            if (!candidate || candidate == controlled) return;

            // Explicit player reports remain preferred until this attacker has lost active intent.
            if (!threat || threat == candidate || !HasHostileIntent(threat) || !HasFreshReport(threat))
            {
                threat = candidate;
                threatReportedAt = float.NegativeInfinity;
            }
        }

        void FindExistingHostileIntent()
        {
            foreach (var brain in CreatureBrain.ActiveBrains) ObserveHostileIntent(brain);
        }

        void LateUpdate()
        {
            if (!IsEligible(threat))
            {
                ClearThreat();
                // A second pursuer may already be in Chase/Attack without producing a new state transition.
                // Reconcile only after cleanup; this is a bounded registry pass, never a per-frame scene scan.
                FindExistingHostileIntent();
                if (!IsEligible(threat)) return;
            }
            var delta = threat.transform.position - controlled.transform.position; delta.y = 0;
            // Even a close flank should be perceptible on a phone, while the rig keeps the hero primary.
            var focus = Mathf.Lerp(.5f, 1f, Mathf.Clamp01(delta.magnitude / NearbyDistance)) * MaxBias;
            rig.RequestCombatFocus(threat.transform, focus);
            UpdateIndicator();
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

        bool HasFreshReport(CombatEntity candidate) => candidate == threat && Time.unscaledTime - threatReportedAt <= RelevanceLifetime;

        bool HasHostileIntent(CombatEntity candidate)
        {
            if (!candidate) return false;
            return IsHostileIntent(candidate.GetComponent<CreatureBrain>());
        }

        bool IsHostileIntent(CreatureBrain brain) => brain && brain.IsActive && brain.Target == controlled && (brain.State == BrainState.Chase || brain.State == BrainState.Attack);

        void UpdateIndicator()
        {
            var viewport = view.WorldToViewportPoint(threat.transform.position + Vector3.up);
            bool offScreen = viewport.z <= 0 || viewport.x < .04f || viewport.x > .96f || viewport.y < .04f || viewport.y > .96f;
            indicator.gameObject.SetActive(offScreen);
            if (!offScreen) { UpdateTargetPlate(viewport); return; }
            ClearTargetPlate();
            IndicatorDirection = IndicatorDirectionFor(viewport, view.transform.right, threat.transform.position - view.transform.position);
            indicator.text = IndicatorDirection < 0 ? "◀  ATTACKER" : "ATTACKER  ▶";
            var rect = indicator.rectTransform;
            var safe = Screen.safeArea;
            var safeEdge = IndicatorDirection < 0 ? safe.xMin / Screen.width : safe.xMax / Screen.width;
            rect.anchorMin = rect.anchorMax = new Vector2(safeEdge, .54f);
            rect.pivot = new Vector2(IndicatorDirection < 0 ? 0 : 1, .5f);
            rect.anchoredPosition = new Vector2(IndicatorDirection < 0 ? 34 : -34, 0);
        }

        void UpdateTargetPlate(Vector3 viewport)
        {
            if (!targetPlate) return;
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
            var anchor = TargetPlateAnchorFor(viewport, Screen.safeArea, new Vector2(Screen.width, Screen.height));
            var rect = targetPlate.rectTransform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = new Vector2(.5f, 0);
            rect.anchoredPosition = new Vector2(0, 28);
            targetPlate.gameObject.SetActive(true);
        }

        /// <summary>Clamps a visible threat annotation inside the device safe area without changing its target.</summary>
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

        /// <summary>Maps a projected off-screen threat to its visible horizontal edge.</summary>
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
            threat = null; threatReportedAt = float.NegativeInfinity; IndicatorDirection = 0;
            if (indicator) indicator.gameObject.SetActive(false);
            ClearTargetPlate();
            if (rig) rig.ClearCombatFocus();
        }

        void ClearTargetPlate()
        {
            displayedPlateTarget = null;
            displayedPlateHealth = float.NaN;
            displayedPlateMaximum = float.NaN;
            if (targetPlate) targetPlate.gameObject.SetActive(false);
        }

        void OnDisable() { CreatureBrain.HostileIntentChanged -= ObserveHostileIntent; ClearThreat(); }
        void OnDestroy()
        {
            CreatureBrain.HostileIntentChanged -= ObserveHostileIntent;
            if (controlled && controlled.Health != null) { controlled.Health.Damaged -= ObserveDamage; controlled.Health.Died -= Clear; }
            if (rig) rig.ClearCombatFocus();
        }

        void CreateIndicator()
        {
            var canvas = new GameObject("Combat Threat Indicator", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
            canvas.transform.SetParent(transform, false);
            var screenCanvas = canvas.GetComponent<Canvas>(); screenCanvas.renderMode = RenderMode.ScreenSpaceOverlay; screenCanvas.sortingOrder = 12;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1080, 1920);
            var label = new GameObject("Threat Direction", typeof(RectTransform), typeof(Text)); label.transform.SetParent(canvas.transform, false);
            indicator = label.GetComponent<Text>(); indicator.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); indicator.fontSize = 30; indicator.fontStyle = FontStyle.Bold; indicator.alignment = TextAnchor.MiddleCenter; indicator.color = new Color(1f, .72f, .2f, .96f); indicator.raycastTarget = false;
            indicator.rectTransform.sizeDelta = new Vector2(245, 66); indicator.gameObject.SetActive(false);
            var plate = new GameObject("Threat Target Plate", typeof(RectTransform), typeof(Text)); plate.transform.SetParent(canvas.transform, false);
            targetPlate = plate.GetComponent<Text>(); targetPlate.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); targetPlate.fontSize = 24; targetPlate.fontStyle = FontStyle.Bold; targetPlate.alignment = TextAnchor.MiddleCenter; targetPlate.color = new Color(1f, .9f, .55f, .96f); targetPlate.raycastTarget = false;
            targetPlate.rectTransform.sizeDelta = new Vector2(340, 58); targetPlate.gameObject.SetActive(false);
        }
    }
}
