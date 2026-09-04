using System.Collections;
using RealmRaiders.Characters;
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
        Vector3 overviewPosition = new(0, 22, -11);
        Quaternion overviewRotation = Quaternion.Euler(60, 0, 0);

        public void ConfigureOverview(Vector3 position, Quaternion rotation)
        { overviewPosition = position; overviewRotation = rotation; }

        public void SnapToOverview()
        { target = null; Mode = CameraMode.KeeperOverview; transform.SetPositionAndRotation(overviewPosition, overviewRotation); }

        public void SnapTo(CombatEntity entity, CameraMode mode)
        {
            target = entity ? entity.transform : null;
            Mode = mode;
            IsTransitioning = false;
            var pose = DesiredPose(target, mode);
            transform.SetPositionAndRotation(pose.position, pose.rotation);
        }

        public void TransitionTo(CombatEntity entity, CameraMode mode, float duration = .65f)
        { StopAllCoroutines(); StartCoroutine(Blend(entity ? entity.transform : null, mode, duration)); }

        public bool FocusTrap(Transform trap, CombatEntity trapped, float easeIn = .25f, float hold = 1f, float easeOut = .4f)
        {
            if (!trap || !trapped || Mode != CameraMode.KeeperOverview || IsTransitioning || target) return false;
            StartCoroutine(TrapFocus(trap, trapped.transform, easeIn, hold, easeOut));
            return true;
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
            if (IsTransitioning || !target) return;
            var pose = DesiredPose(target, Mode);
            transform.position = Vector3.Lerp(transform.position, pose.position, 8 * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, pose.rotation, 8 * Time.deltaTime);
        }

        Pose DesiredPose(Transform follow, CameraMode mode)
        {
            if (!follow || mode == CameraMode.KeeperOverview) return new Pose(overviewPosition, overviewRotation);
            float scale = mode == CameraMode.PossessedCreature ? 1.25f : 1;
            var offset = new Vector3(0, 7 * scale, -7 * scale);
            var position = follow.position + offset;
            return new Pose(position, Quaternion.LookRotation(follow.position + Vector3.up * 1.3f - position));
        }
    }
}
