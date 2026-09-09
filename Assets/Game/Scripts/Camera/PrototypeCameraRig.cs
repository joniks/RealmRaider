using System.Collections;
using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using RealmRaiders.UI;
using UnityEngine;

namespace RealmRaiders.CameraSystem
{
    public enum CameraMode { KeeperOverview, HeroCombat, PossessedCreature }

    [RequireComponent(typeof(Camera))]
    public sealed class PrototypeCameraRig : MonoBehaviour
    {
        public CameraMode Mode { get; private set; }
        public bool IsTransitioning { get; private set; }
        Transform target;
        Transform combatThreat;
        float requestedFocus;
        float focusWeight;
        float requestedControlYaw;
        float controlYaw;
        bool hasControlYaw;
        Vector3 lastLocomotionDirection;
        Vector3 overviewPosition = new(0, 22, -11);
        Quaternion overviewRotation = Quaternion.Euler(60, 0, 0);
        public const float ManualYawDegreesPerPixel = .16f;
        public const float MaximumManualYawStep = 18f;
        public const float ControlYawDegreesPerSecond = 180f;

        public void ConfigureOverview(Vector3 position, Quaternion rotation)
        { overviewPosition = position; overviewRotation = rotation; }

        public void SnapToOverview()
        { ClearCombatFocus(); ClearControlYaw(); target = null; Mode = CameraMode.KeeperOverview; transform.SetPositionAndRotation(overviewPosition, overviewRotation); }

        public void SnapTo(CombatEntity entity, CameraMode mode)
        {
            ClearCombatFocus(); ClearControlYaw();
            target = entity ? entity.transform : null;
            Mode = mode;
            IsTransitioning = false;
            var pose = DesiredPose(target, mode);
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        public void TransitionTo(CombatEntity entity, CameraMode mode, float duration = .65f)
        { ClearCombatFocus(); ClearControlYaw(); StopAllCoroutines(); IsTransitioning = false; StartCoroutine(Blend(entity ? entity.transform : null, mode, duration)); }

        public bool FocusTrap(Transform trap, CombatEntity trapped, float easeIn = .25f, float hold = 1f, float easeOut = .4f)
        {
            if (!trap || !trapped || Mode != CameraMode.KeeperOverview || IsTransitioning || target) return false;
            ClearCombatFocus(); ClearControlYaw();
            StartCoroutine(TrapFocus(trap, trapped.transform, easeIn, hold, easeOut));
            return true;
        }

        public bool HasCombatFocus => combatThreat && focusWeight > .001f;
        public bool HasRequestedCombatFocus => combatThreat && requestedFocus > .001f;
        public float CombatFocusWeight => focusWeight;
        public bool HasControlYaw => hasControlYaw;
        public float ControlYaw => controlYaw;
        public float RequestedControlYaw => requestedControlYaw;
        public Vector3 LastLocomotionDirection => lastLocomotionDirection;
        public CombatCameraAwareness BindCombatHud(ResponsiveHudRoot hud, RectTransform competingEdgeCue = null)
        {
            var awareness = GetComponent<CombatCameraAwareness>() ?? gameObject.AddComponent<CombatCameraAwareness>();
            awareness.BindHud(hud, competingEdgeCue);
            return awareness;
        }
        public void RequestCombatFocus(Transform threat, float weight)
        {
            if (IsTransitioning || Mode == CameraMode.KeeperOverview || !target || !threat) return;
            combatThreat = threat; requestedFocus = Mathf.Clamp01(weight);
        }
        public void ClearCombatFocus() { combatThreat = null; requestedFocus = 0; }
        public bool RequestManualYaw(float screenDeltaX)
        {
            if (!CanAcceptControlYaw()) return false;
            var step = Mathf.Clamp(screenDeltaX * ManualYawDegreesPerPixel, -MaximumManualYawStep, MaximumManualYawStep);
            if (Mathf.Abs(step) < .01f) return false;
            requestedControlYaw = Mathf.DeltaAngle(0, requestedControlYaw + step);
            hasControlYaw = true;
            return true;
        }

        public bool RequestLocomotionYaw(Vector3 factualDisplacement)
        {
            factualDisplacement.y = 0;
            if (!CanAcceptControlYaw() || factualDisplacement.sqrMagnitude <= .000001f) return false;
            lastLocomotionDirection = factualDisplacement.normalized;
            requestedControlYaw = Mathf.Atan2(lastLocomotionDirection.x, lastLocomotionDirection.z) * Mathf.Rad2Deg;
            hasControlYaw = true;
            return true;
        }

        public void ClearControlYaw()
        {
            requestedControlYaw = 0;
            controlYaw = 0;
            hasControlYaw = false;
            lastLocomotionDirection = Vector3.zero;
        }

        bool CanAcceptControlYaw()
        {
            if (GameplayInput.TerminalState || IsTransitioning || Mode == CameraMode.KeeperOverview || !target) return false;
            var entity = target.GetComponent<CombatEntity>();
            return entity && entity.Health != null && !entity.Health.IsDead && entity.ActiveController is PlayerController player && player.IsActive;
        }

        IEnumerator Blend(Transform next, CameraMode mode, float duration)
        {
            IsTransitioning = true;
            var fromPosition = transform.position; var fromRotation = transform.rotation;
            for (float t = 0; t < duration; t += Time.unscaledDeltaTime)
            {
                Pose desired = DesiredPose(next, mode);
                float eased = Mathf.SmoothStep(0, 1, t / duration);
                transform.SetPositionAndRotation(Vector3.Lerp(fromPosition, desired.position, eased), Quaternion.Slerp(fromRotation, desired.rotation, eased));
                yield return null;
            }
            target = next; Mode = mode; IsTransitioning = false;
        }

        IEnumerator TrapFocus(Transform trap, Transform trapped, float easeIn, float hold, float easeOut)
        {
            IsTransitioning = true;
            var fromPosition = transform.position; var fromRotation = transform.rotation;
            var point = (trap.position + trapped.position) * .5f + Vector3.up * 1.1f;
            var direction = (fromPosition - point).normalized;
            var focusPosition = point + direction * Mathf.Min(Vector3.Distance(fromPosition, point), 22f);
            var focusRotation = Quaternion.LookRotation(point - focusPosition);
            for (float t = 0; t < easeIn; t += Time.unscaledDeltaTime)
            {
                var eased = Mathf.SmoothStep(0, 1, t / easeIn);
                transform.SetPositionAndRotation(Vector3.Lerp(fromPosition, focusPosition, eased), Quaternion.Slerp(fromRotation, focusRotation, eased));
                yield return null;
            }
            transform.SetPositionAndRotation(focusPosition, focusRotation);
            yield return new WaitForSecondsRealtime(hold);
            for (float t = 0; t < easeOut; t += Time.unscaledDeltaTime)
            {
                var eased = Mathf.SmoothStep(0, 1, t / easeOut);
                transform.SetPositionAndRotation(Vector3.Lerp(focusPosition, fromPosition, eased), Quaternion.Slerp(focusRotation, fromRotation, eased));
                yield return null;
            }
            transform.SetPositionAndRotation(fromPosition, fromRotation);
            IsTransitioning = false;
        }

        void LateUpdate()
        {
            focusWeight = Mathf.MoveTowards(focusWeight, combatThreat ? requestedFocus : 0, 2.8f * Time.deltaTime);
            var followedEntity = target ? target.GetComponent<CombatEntity>() : null;
            if (GameplayInput.TerminalState || followedEntity && followedEntity.Health != null && followedEntity.Health.IsDead) ClearControlYaw();
            if (IsTransitioning || !target) return;
            controlYaw = Mathf.MoveTowardsAngle(controlYaw, hasControlYaw ? requestedControlYaw : 0, ControlYawDegreesPerSecond * Time.deltaTime);
            var pose = DesiredPose(target, Mode, combatThreat, focusWeight);
            transform.position = Vector3.Lerp(transform.position, pose.position, 8 * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, pose.rotation, 8 * Time.deltaTime);
        }

        Pose DesiredPose(Transform follow, CameraMode mode, Transform threat = null, float threatWeight = 0)
        {
            if (!follow || mode == CameraMode.KeeperOverview) return new Pose(overviewPosition, overviewRotation);
            float scale = mode == CameraMode.PossessedCreature ? 1.25f : 1;
            var offset = Quaternion.Euler(0, controlYaw, 0) * new Vector3(0, 7 * scale, -7 * scale);
            var position = follow.position + offset;
            var lookAt = follow.position + Vector3.up * 1.3f;
            if (threat && threatWeight > 0)
            {
                var direction = threat.position - follow.position; direction.y = 0;
                if (direction.sqrMagnitude > .01f)
                {
                    var bias = direction.normalized * Mathf.Min(2.1f, direction.magnitude * .16f) * threatWeight;
                    position += bias * .32f;
                    lookAt += bias;
                }
            }
            return new Pose(position, Quaternion.LookRotation(lookAt - position));
        }

        void OnDisable() => ClearControlYaw();
    }
}
