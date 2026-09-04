using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using UnityEngine;

namespace RealmRaiders.AI
{
    [RequireComponent(typeof(CombatEntity))]
    public sealed class RaidInvaderBrain : MonoBehaviour, IEntityController
    {
        public bool IsActive { get; private set; }
        public int WaypointIndex { get; private set; }
        public CombatEntity CurrentTarget { get; private set; }
        CombatEntity entity;
        CombatEntity[] defenders;
        Vector3[] waypoints;
        float pauseUntil;

        void Awake() => entity = GetComponent<CombatEntity>();
        public void Configure(Vector3[] route, CombatEntity[] realmDefenders)
        { waypoints = route; defenders = realmDefenders; WaypointIndex = 0; }
        public void SetControl(bool active) { IsActive = active; CurrentTarget = null; }

        public void Tick()
        {
            if (Time.time < pauseUntil) { entity.Move(Vector3.zero); return; }
            CurrentTarget = ClosestDefender(7.5f);
            if (CurrentTarget)
            {
                var delta = CurrentTarget.transform.position - transform.position; delta.y = 0;
                if (delta.magnitude > 2.7f) entity.Move(delta.normalized * entity.Stats.MoveSpeed);
                else { entity.Move(Vector3.zero); entity.TryUse(0, delta); }
                return;
            }

            if (waypoints == null || waypoints.Length == 0 || WaypointIndex >= waypoints.Length)
            { entity.Move(Vector3.zero); return; }
            var pathDelta = waypoints[WaypointIndex] - transform.position; pathDelta.y = 0;
            if (pathDelta.magnitude < .6f) { WaypointIndex++; pauseUntil = Time.time + 1.1f; }
            else entity.Move(pathDelta.normalized * entity.Stats.MoveSpeed);
        }

        CombatEntity ClosestDefender(float radius)
        {
            CombatEntity nearest = null; float best = radius * radius;
            if (defenders == null) return null;
            foreach (var defender in defenders)
            {
                if (!defender || defender.Health.IsDead) continue;
                float distance = (defender.transform.position - transform.position).sqrMagnitude;
                if (distance < best) { best = distance; nearest = defender; }
            }
            return nearest;
        }
    }
}
