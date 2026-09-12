using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using UnityEngine;

namespace RealmRaiders.Characters
{
    public enum CharacterJumpPresentationPhase
    {
        None = 0,
        Takeoff = 1,
        Falling = 2,
        Landing = 3
    }

    /// <summary>One factual, presentation-only jump timeline shared by pivot and procedural-bone consumers.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterJumpPresentationTimeline : MonoBehaviour
    {
        public const float BaseTakeoffSeconds = .10f;
        public const float BaseLandingSeconds = .14f;

        float takeoffSeconds = BaseTakeoffSeconds;
        float straightenSeconds = BaseTakeoffSeconds;
        float landingSeconds = BaseLandingSeconds;
        float fallingBlendSeconds = BaseTakeoffSeconds;
        float takeoffStartedAt = float.NegativeInfinity;
        float pendingLandingAt = float.NegativeInfinity;
        float landingStartedAt = float.NegativeInfinity;
        bool observed;
        bool observedJumping;
        bool observedGrounded;

        public float TakeoffSeconds => takeoffSeconds;
        public float StraightenSeconds => straightenSeconds;
        public float LandingSeconds => landingSeconds;

        public void Configure(float jumpPresentationDurationMultiplier, float takeoffStraightenDurationMultiplier)
        {
            var jumpMultiplier = ClampDurationMultiplier(jumpPresentationDurationMultiplier);
            var straightenMultiplier = ClampDurationMultiplier(takeoffStraightenDurationMultiplier);
            var nextTakeoff = BaseTakeoffSeconds * jumpMultiplier;
            var nextStraighten = BaseTakeoffSeconds * straightenMultiplier;
            var nextLanding = BaseLandingSeconds * jumpMultiplier;
            if (Mathf.Approximately(takeoffSeconds, nextTakeoff) && Mathf.Approximately(straightenSeconds, nextStraighten) && Mathf.Approximately(landingSeconds, nextLanding)) return;

            takeoffSeconds = nextTakeoff;
            straightenSeconds = nextStraighten;
            landingSeconds = nextLanding;
            fallingBlendSeconds = nextTakeoff;
            ResetTimeline();
        }

        /// <summary>Consumes factual state at an explicit timestamp; duplicate consumers at that timestamp receive the same sample.</summary>
        public CharacterJumpPresentationSample Observe(float unscaledClock, bool isJumping, bool isGrounded, bool hasDirectControl)
        {
            var now = IsFinite(unscaledClock) ? unscaledClock : 0f;
            if (!hasDirectControl)
            {
                ResetTimeline();
                return CharacterJumpPresentationSample.None;
            }

            if (!observed)
            {
                observed = true;
                observedJumping = isJumping;
                observedGrounded = isGrounded;
                return Evaluate(now, isJumping);
            }

            if (!observedJumping && isJumping)
            {
                takeoffStartedAt = now;
                pendingLandingAt = float.NegativeInfinity;
                landingStartedAt = float.NegativeInfinity;
            }
            else if (observedGrounded && !isGrounded && !isJumping && !IsFinite(takeoffStartedAt))
            {
                // A factual walk-off enters the settled fall pose immediately: no invented takeoff and no
                // post-contact wait for a visual air blend before landing can begin.
                takeoffStartedAt = now - takeoffSeconds - fallingBlendSeconds;
                pendingLandingAt = float.NegativeInfinity;
                landingStartedAt = float.NegativeInfinity;
            }
            if (!observedGrounded && isGrounded && !isJumping)
            {
                BeginFactualLanding(now, isGrounded);
            }
            observedJumping = isJumping;
            observedGrounded = isGrounded;
            return Evaluate(now, isJumping);
        }

        public void ResetTimeline()
        {
            takeoffStartedAt = float.NegativeInfinity;
            pendingLandingAt = float.NegativeInfinity;
            landingStartedAt = float.NegativeInfinity;
            observed = false;
            observedJumping = false;
            observedGrounded = false;
        }

        CharacterJumpPresentationSample Evaluate(float now, bool isJumping)
        {
            if (IsFinite(landingStartedAt))
            {
                var landingProgress = Mathf.Clamp01((now - landingStartedAt) / landingSeconds);
                if (now <= landingStartedAt + landingSeconds) return new CharacterJumpPresentationSample(CharacterJumpPresentationPhase.Landing, landingProgress);
                landingStartedAt = float.NegativeInfinity;
                return CharacterJumpPresentationSample.None;
            }

            if (IsFinite(pendingLandingAt))
            {
                if (now < pendingLandingAt) return AirSample(now);
                landingStartedAt = pendingLandingAt;
                pendingLandingAt = float.NegativeInfinity;
                takeoffStartedAt = float.NegativeInfinity;
                return Evaluate(now, false);
            }

            if (!IsFinite(takeoffStartedAt)) return CharacterJumpPresentationSample.None;
            if (!isJumping)
            {
                if (!observedGrounded) return AirSample(now);
                takeoffStartedAt = float.NegativeInfinity;
                return CharacterJumpPresentationSample.None;
            }

            return AirSample(now);
        }

        void BeginFactualLanding(float now, bool isGrounded)
        {
            if (!isGrounded || !IsFinite(takeoffStartedAt))
            {
                takeoffStartedAt = float.NegativeInfinity;
                pendingLandingAt = float.NegativeInfinity;
                landingStartedAt = float.NegativeInfinity;
                return;
            }

            var fallCompletesAt = takeoffStartedAt + takeoffSeconds + fallingBlendSeconds;
            if (now < fallCompletesAt)
            {
                pendingLandingAt = fallCompletesAt;
                return;
            }

            takeoffStartedAt = float.NegativeInfinity;
            pendingLandingAt = float.NegativeInfinity;
            landingStartedAt = now;
        }

        CharacterJumpPresentationSample FallingSample(float now)
        {
            var elapsed = Mathf.Max(0f, now - takeoffStartedAt);
            var fallingProgress = Mathf.Clamp01((elapsed - takeoffSeconds) / fallingBlendSeconds);
            return new CharacterJumpPresentationSample(CharacterJumpPresentationPhase.Falling, fallingProgress);
        }

        CharacterJumpPresentationSample AirSample(float now)
        {
            var elapsed = Mathf.Max(0f, now - takeoffStartedAt);
            if (elapsed < takeoffSeconds)
            {
                var straightenProgress = Mathf.Clamp01(elapsed / straightenSeconds);
                return new CharacterJumpPresentationSample(CharacterJumpPresentationPhase.Takeoff, straightenProgress);
            }
            return FallingSample(now);
        }

        public static bool HasFactualDirectControl(CombatEntity entity)
        {
            return entity && entity.Health != null && !entity.Health.IsDead && entity.Motor && entity.Motor.enabled &&
                   !entity.IsRooted && !GameplayInput.TerminalState && entity.ActiveController is PlayerController player &&
                   player.IsActive && player.isActiveAndEnabled;
        }

        static float ClampDurationMultiplier(float value)
        {
            if (!IsFinite(value) || value < 1f) return 1f;
            return Mathf.Min(4f, value);
        }

        static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        void OnDisable() => ResetTimeline();
        void OnDestroy() => ResetTimeline();
    }

    public readonly struct CharacterJumpPresentationSample
    {
        public static readonly CharacterJumpPresentationSample None = new(CharacterJumpPresentationPhase.None, 0f);

        public CharacterJumpPresentationSample(CharacterJumpPresentationPhase phase, float progress)
        {
            Phase = phase;
            Progress = Mathf.Clamp01(float.IsNaN(progress) || float.IsInfinity(progress) ? 0f : progress);
        }

        public CharacterJumpPresentationPhase Phase { get; }
        public float Progress { get; }
    }
}
