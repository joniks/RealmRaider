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
        const float MaxBias = .85f;

        PrototypeCameraRig rig;
        Camera view;
        CombatEntity controlled;
        CombatEntity threat;
        float threatReportedAt = float.NegativeInfinity;
        Text indicator;

        public bool IndicatorVisible => indicator && indicator.gameObject.activeSelf;
        public int IndicatorDirection { get; private set; }
        public bool HasEligibleThreat => IsEligible(threat);

        void Awake()
        {
            rig = GetComponent<PrototypeCameraRig>();
            view = GetComponent<Camera>();
            CreateIndicator();
        }

        public void SetControlled(CombatEntity entity)
        {
            if (controlled == entity) return;
            if (controlled && controlled.Health != null) { controlled.Health.Damaged -= ObserveDamage; controlled.Health.Died -= Clear; }
            controlled = entity;
            if (controlled && controlled.Health != null) { controlled.Health.Damaged += ObserveDamage; controlled.Health.Died += Clear; }
            ClearThreat();
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

        void LateUpdate()
        {
            if (!IsEligible(threat)) { ClearThreat(); return; }
            var delta = threat.transform.position - controlled.transform.position; delta.y = 0;
            rig.RequestCombatFocus(threat.transform, Mathf.Clamp01(delta.magnitude / NearbyDistance) * MaxBias);
            UpdateIndicator();
        }

        bool IsEligible(CombatEntity candidate)
        {
            if (!controlled || !candidate || !controlled.Health || !candidate.Health || controlled.Health.IsDead || candidate.Health.IsDead) return false;
            if (GameplayInput.TerminalState || rig.IsTransitioning || rig.Mode == CameraMode.KeeperOverview) return false;
            var player = controlled.Controller<PlayerController>();
            if (player == null || !player.IsActive || Time.unscaledTime - threatReportedAt > RelevanceLifetime) return false;
            var delta = candidate.transform.position - controlled.transform.position; delta.y = 0;
            return delta.sqrMagnitude <= NearbyDistance * NearbyDistance;
        }

        void UpdateIndicator()
        {
            var viewport = view.WorldToViewportPoint(threat.transform.position + Vector3.up);
            bool offScreen = viewport.z <= 0 || viewport.x < .04f || viewport.x > .96f || viewport.y < .04f || viewport.y > .96f;
            indicator.gameObject.SetActive(offScreen);
            if (!offScreen) return;
            IndicatorDirection = viewport.z > 0 ? (viewport.x < .5f ? -1 : 1) : (Vector3.Dot(view.transform.right, threat.transform.position - view.transform.position) < 0 ? -1 : 1);
            indicator.text = IndicatorDirection < 0 ? "◀  THREAT" : "THREAT  ▶";
            var rect = indicator.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(IndicatorDirection < 0 ? 0 : 1, .54f);
            rect.pivot = new Vector2(IndicatorDirection < 0 ? 0 : 1, .5f);
            rect.anchoredPosition = new Vector2(IndicatorDirection < 0 ? 28 : -28, 0);
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
            if (rig) rig.ClearCombatFocus();
        }

        void OnDisable() => ClearThreat();
        void OnDestroy()
        {
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
            indicator = label.GetComponent<Text>(); indicator.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); indicator.fontSize = 25; indicator.alignment = TextAnchor.MiddleCenter; indicator.color = new Color(1f, .72f, .2f, .92f); indicator.raycastTarget = false;
            indicator.rectTransform.sizeDelta = new Vector2(185, 56); indicator.gameObject.SetActive(false);
        }
    }
}
