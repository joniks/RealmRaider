using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using UnityEngine;

namespace RealmRaiders.AI
{
    [RequireComponent(typeof(CombatEntity))]
    public sealed class RaidInvaderBrain : MonoBehaviour, IEntityController
    {
        public const float DefaultOpeningHoldDuration = 3f;
        public bool IsActive { get; private set; }
        public int WaypointIndex { get; private set; }
        public CombatEntity CurrentTarget { get; private set; }
        public bool IsOpeningHold => IsActive && openingStarted && !openingReleased && Time.time < openingEndsAt;
        public float OpeningSecondsRemaining => IsOpeningHold ? Mathf.Max(0, openingEndsAt - Time.time) : 0;
        public bool IsRecovering => recovery.IsRecovering;
        public int RecoverySide => recovery.Side;
        public float StuckSeconds => recovery.StuckSeconds;
        CombatEntity entity;
        Health health;
        CombatEntity[] defenders;
        Vector3[] waypoints;
        Vector3 routeStart;
        float pauseUntil;
        float openingEndsAt;
        bool openingStarted;
        bool openingReleased;
        readonly InvaderStuckRecovery recovery = new();

        void Awake()
        {
            entity = GetComponent<CombatEntity>();
            health = GetComponent<Health>();
            health.Died += ResetRecovery;
        }
        public void Configure(Vector3[] route, CombatEntity[] realmDefenders, float openingHoldDuration = DefaultOpeningHoldDuration)
        {
            waypoints = route; defenders = realmDefenders; WaypointIndex = 0; pauseUntil = 0;
            routeStart = transform.position;
            openingEndsAt = 0; openingStarted = false; openingReleased = false;
            OpeningHoldDuration = Mathf.Max(0, openingHoldDuration);
            ResetRecovery();
        }

        public float OpeningHoldDuration { get; private set; } = DefaultOpeningHoldDuration;

        public void SetControl(bool active)
        {
            IsActive = active; CurrentTarget = null;
            ResetRecovery();
            if (active && !openingStarted)
            {
                openingStarted = true;
                openingEndsAt = Time.time + OpeningHoldDuration;
            }
        }

        public void Tick()
        {
            if (!IsActive || !isActiveAndEnabled || !health || health.IsDead || GameplayInput.TerminalState)
            { CurrentTarget = null; ResetRecovery(); return; }
            if (IsOpeningHold) { CurrentTarget = null; ResetRecovery(); entity.Move(Vector3.zero); return; }
            if (openingStarted && !openingReleased) openingReleased = true;
            if (Time.time < pauseUntil) { ResetRecovery(); entity.Move(Vector3.zero); return; }
            CurrentTarget = ClosestDefender(7.5f);
            if (CurrentTarget)
            {
                ResetRecovery();
                var delta = CurrentTarget.transform.position - transform.position; delta.y = 0;
                if (delta.magnitude > 2.7f) entity.Move(delta.normalized * entity.Stats.MoveSpeed);
                else { entity.Move(Vector3.zero); entity.TryUse(0, delta); }
                return;
            }

            if (waypoints == null || waypoints.Length == 0 || WaypointIndex >= waypoints.Length)
            { ResetRecovery(); entity.Move(Vector3.zero); return; }
            var pathDelta = waypoints[WaypointIndex] - transform.position; pathDelta.y = 0;
            if (pathDelta.magnitude < .6f)
            {
                WaypointIndex++; pauseUntil = Time.time + 1.1f; ResetRecovery();
                return;
            }
            if (entity.Stats.MoveSpeed <= .01f) { ResetRecovery(); entity.Move(Vector3.zero); return; }

            var waypoint = waypoints[WaypointIndex];
            var segmentStart = WaypointIndex > 0 ? waypoints[WaypointIndex - 1] : routeStart;
            var suspended = entity.IsRooted || entity.IsActionResolving;
            var direction = recovery.Evaluate(transform.position, segmentStart, waypoint, entity.Stats.MoveSpeed, Time.deltaTime, !suspended);
            entity.Move(direction * entity.Stats.MoveSpeed);
        }

        void ResetRecovery()
        {
            recovery.Reset(transform.position);
        }

        void OnDisable() => ResetRecovery();
        void OnDestroy()
        {
            if (health) health.Died -= ResetRecovery;
            ResetRecovery();
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
