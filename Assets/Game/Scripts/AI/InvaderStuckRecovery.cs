using UnityEngine;

namespace RealmRaiders.AI
{
    /// <summary>Pure, scene-independent progress watchdog and bounded route sidestep.</summary>
    public sealed class InvaderStuckRecovery
    {
        public const float StuckWindow = .8f;
        public const float MinimumProgressSpeed = .08f;
        public const float RecoveryExitProgress = .18f;
        public const float FailedAttemptWindow = .9f;
        public const float SideBlend = .8f;
        public const float CorridorHalfWidth = 2.2f;

        Vector3 lastPosition;
        Vector3 recoveryReferencePosition;
        float stuckSeconds;
        float attemptSeconds;
        bool hasObservation;
        int side;

        public bool IsRecovering => side != 0;
        public int Side => side;
        public float StuckSeconds => stuckSeconds;
        public float AttemptSeconds => attemptSeconds;

        public void Reset(Vector3 position)
        {
            lastPosition = Horizontal(position);
            recoveryReferencePosition = lastPosition;
            stuckSeconds = 0;
            attemptSeconds = 0;
            side = 0;
            hasObservation = true;
        }

        public void Pause(Vector3 position)
        {
            lastPosition = Horizontal(position);
            hasObservation = true;
        }

        public Vector3 Evaluate(Vector3 position, Vector3 segmentStart, Vector3 waypoint, float moveSpeed, float deltaTime, bool measureProgress)
        {
            position = Horizontal(position);
            waypoint = Horizontal(waypoint);
            if (!hasObservation) Reset(position);

            var elapsed = Mathf.Max(0, deltaTime);
            var segmentForward = SegmentDirection(segmentStart, waypoint, position);
            var actualDelta = position - lastPosition;
            var actualDisplacement = actualDelta.magnitude;
            var routeProgress = Vector3.Dot(actualDelta, segmentForward);
            lastPosition = position;

            if (!measureProgress)
            {
                Pause(position);
                return RouteDirection(position, waypoint);
            }

            var progressed = actualDisplacement > 0 && routeProgress >= MinimumProgressSpeed * elapsed;
            if (IsRecovering && Vector3.Dot(position - recoveryReferencePosition, segmentForward) >= RecoveryExitProgress)
            {
                stuckSeconds = 0;
                attemptSeconds = 0;
                side = 0;
            }
            else if (IsRecovering)
            {
                attemptSeconds += elapsed;
                if (attemptSeconds >= FailedAttemptWindow)
                {
                    side = -side;
                    attemptSeconds = 0;
                    recoveryReferencePosition = position;
                }
            }
            else if (progressed) stuckSeconds = 0;
            else
            {
                stuckSeconds += elapsed;
                if (stuckSeconds >= StuckWindow)
                {
                    side = 1;
                    attemptSeconds = 0;
                    recoveryReferencePosition = position;
                }
            }

            return IsRecovering
                ? RecoveryDirection(position, segmentStart, waypoint, Mathf.Max(0, moveSpeed), elapsed)
                : RouteDirection(position, waypoint);
        }

        public static float CorridorOffset(Vector3 position, Vector3 segmentStart, Vector3 waypoint)
        {
            var forward = SegmentDirection(segmentStart, waypoint, position);
            var right = Vector3.Cross(Vector3.up, forward);
            return Vector3.Dot(Horizontal(position - segmentStart), right);
        }

        Vector3 RecoveryDirection(Vector3 position, Vector3 segmentStart, Vector3 waypoint, float moveSpeed, float deltaTime)
        {
            var forward = RouteDirection(position, waypoint);
            if (forward.sqrMagnitude <= .0001f) return Vector3.zero;
            var segmentForward = SegmentDirection(segmentStart, waypoint, position);
            var right = Vector3.Cross(Vector3.up, segmentForward);
            var lateral = Vector3.Dot(Horizontal(position - segmentStart), right);
            var desiredLateralSpeed = side * moveSpeed * SideBlend;
            if (deltaTime > 0)
            {
                var boundedNext = Mathf.Clamp(lateral + desiredLateralSpeed * deltaTime, -CorridorHalfWidth, CorridorHalfWidth);
                desiredLateralSpeed = (boundedNext - lateral) / deltaTime;
            }
            else desiredLateralSpeed = 0;

            var desiredVelocity = forward * moveSpeed + right * desiredLateralSpeed;
            return desiredVelocity.sqrMagnitude > .0001f ? desiredVelocity.normalized : forward;
        }

        static Vector3 SegmentDirection(Vector3 segmentStart, Vector3 waypoint, Vector3 fallbackPosition)
        {
            var direction = Horizontal(waypoint - segmentStart);
            if (direction.sqrMagnitude <= .0001f) direction = Horizontal(waypoint - fallbackPosition);
            if (direction.sqrMagnitude <= .0001f) direction = Vector3.forward;
            return direction.normalized;
        }

        static Vector3 RouteDirection(Vector3 position, Vector3 waypoint)
        {
            var direction = Horizontal(waypoint - position);
            return direction.sqrMagnitude > .0001f ? direction.normalized : Vector3.zero;
        }

        static Vector3 Horizontal(Vector3 value) { value.y = 0; return value; }
    }
}
