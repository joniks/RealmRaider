using RealmRaiders.Combat;
using RealmRaiders.Modules.CharacterProceduralMotion;
using UnityEngine;

namespace RealmRaiders.Characters
{
    /// <summary>Per-entity visual clock. Facts can share a frame; presentation never changes their timing.</summary>
    public sealed class CharacterCombatPresentationTimeline
    {
        public const float MinimumWindupSeconds = .06f;
        public const float ImpactSeconds = .08f;
        public const float RecoverySeconds = .16f;
        public const float HitRiseSeconds = .06f;
        public const float HitRecoverySeconds = .12f;
        long actionId;
        bool attack, hit;
        float started, scaledStarted, windup, attackDirection;
        float impactAt = float.PositiveInfinity, recoveryAt = float.PositiveInfinity;
        float hitAt, hitStartWeight, hitStartDirection, hitDirection;
        float lastScaled = float.NaN, lastUnscaled = float.NaN;
        ProceduralHumanoidCombatPoseSample cached;
        public bool HasAttack { get; private set; }
        public bool HasHit { get; private set; }
        public long ActionId => actionId;
        public float AttackStartedAt => started;
        public float AttackEndsAt => recoveryAt + RecoverySeconds;
        public float HitStartedAt => hitAt;
        public float HitEndsAt => hitAt + HitRiseSeconds + HitRecoverySeconds;

        public void OnAction(CombatPresentationFact fact)
        {
            if (fact.End != CombatPresentationEnd.None && fact.End != CombatPresentationEnd.Completed) { Clear(); return; }
            if (!Finite(fact.ScaledTime) || !Finite(fact.UnscaledTime)) return;
            if (fact.End == CombatPresentationEnd.Completed) return; // Allow only the visual follow-through to finish.
            if (fact.Phase == CombatActionPhase.Windup)
            {
                if (fact.ActionId <= actionId) return;
                actionId = fact.ActionId; attack = true;
                started = fact.UnscaledTime; scaledStarted = fact.ScaledTime;
                windup = Finite(fact.WindupSeconds) ? Mathf.Max(0, fact.WindupSeconds) : 0;
                attackDirection = SignedDirection(fact.WorldDirection, fact.FacingBeforeAction);
                impactAt = recoveryAt = float.PositiveInfinity;
            }
            else if (attack && fact.ActionId == actionId)
            {
                if (fact.Phase == CombatActionPhase.Impact && float.IsPositiveInfinity(impactAt))
                    impactAt = Mathf.Max(fact.UnscaledTime, started + MinimumWindupSeconds);
                if (fact.Phase == CombatActionPhase.Recovery && float.IsPositiveInfinity(recoveryAt) && !float.IsPositiveInfinity(impactAt))
                    recoveryAt = Mathf.Max(fact.UnscaledTime, impactAt + ImpactSeconds);
            }
            Invalidate();
        }

        public void OnHit(float unscaledClock, float signedDirection)
        {
            if (!Finite(unscaledClock)) return;
            EvaluateHit(unscaledClock, out var weight, out var direction);
            hitStartWeight = weight; hitStartDirection = direction;
            hitDirection = Finite(signedDirection) ? Mathf.Clamp(signedDirection, -1, 1) : 0;
            if (hitStartWeight == 0) hitStartDirection = hitDirection;
            hitAt = unscaledClock; hit = true;
            Invalidate();
        }

        public ProceduralHumanoidCombatPoseSample Observe(float scaledClock, float unscaledClock)
        {
            if (!Finite(scaledClock) || !Finite(unscaledClock)) { Clear(); return default; }
            if (lastScaled == scaledClock && lastUnscaled == unscaledClock) return cached;
            var stage = ProceduralHumanoidAttackStage.Windup;
            float progress = 0;
            HasAttack = attack && unscaledClock < recoveryAt + RecoverySeconds;
            if (HasAttack)
            {
                if (unscaledClock < impactAt)
                {
                    progress = float.IsPositiveInfinity(impactAt)
                        ? Mathf.Clamp01((scaledClock - scaledStarted) / Mathf.Max(MinimumWindupSeconds, windup))
                        : Mathf.Clamp01((unscaledClock - started) / Mathf.Max(MinimumWindupSeconds, impactAt - started));
                }
                else if (unscaledClock < recoveryAt)
                {
                    stage = ProceduralHumanoidAttackStage.Impact;
                    progress = Mathf.Clamp01((unscaledClock - impactAt) / ImpactSeconds);
                }
                else
                {
                    stage = ProceduralHumanoidAttackStage.Recovery;
                    progress = Mathf.Clamp01((unscaledClock - recoveryAt) / RecoverySeconds);
                }
            }
            EvaluateHit(unscaledClock, out var weight, out var direction);
            HasHit = hit && unscaledClock < hitAt + HitRiseSeconds + HitRecoverySeconds;
            // Hit amplitude is an envelope; holding the module's peak at .5 lets a repeat start at the current weight.
            cached = new ProceduralHumanoidCombatPoseSample(stage, progress, HasAttack ? 1 : 0,
                attackDirection, .5f, weight, direction);
            lastScaled = scaledClock; lastUnscaled = unscaledClock;
            return cached;
        }

        void EvaluateHit(float clock, out float weight, out float direction)
        {
            weight = 0; direction = 0;
            if (!hit) return;
            var elapsed = Mathf.Max(0, clock - hitAt);
            if (elapsed >= HitRiseSeconds + HitRecoverySeconds) return;
            if (elapsed < HitRiseSeconds)
            {
                var t = Mathf.SmoothStep(0, 1, elapsed / HitRiseSeconds);
                weight = Mathf.Lerp(hitStartWeight, 1, t);
                // Interpolate weighted direction so both left/right limb strengths remain continuous on repeated hits.
                direction = weight > 0 ? Mathf.Lerp(hitStartWeight * hitStartDirection, hitDirection, t) / weight : hitStartDirection;
            }
            else
            {
                weight = 1 - Mathf.SmoothStep(0, 1, (elapsed - HitRiseSeconds) / HitRecoverySeconds);
                direction = hitDirection;
            }
        }

        public void Clear()
        {
            attack = hit = HasAttack = HasHit = false;
            impactAt = recoveryAt = float.PositiveInfinity;
            started = scaledStarted = windup = attackDirection = hitAt = hitStartWeight = hitStartDirection = hitDirection = 0;
            cached = default;
            Invalidate();
            // Keep the accepted ID watermark: stale phase callbacks cannot restart a cleared action.
        }

        void Invalidate() { lastScaled = lastUnscaled = float.NaN; }
        public static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
        static bool Horizontal(Vector3 value)
        {
            var length = value.x * value.x + value.z * value.z;
            return Finite(length) && length > .000001f;
        }
        public static float SignedDirection(Vector3 worldDirection, Quaternion reference)
        {
            worldDirection.y = 0;
            if (!Horizontal(worldDirection)) return 0;
            var local = Quaternion.Inverse(reference) * worldDirection.normalized;
            return Finite(local.x) ? Mathf.Clamp(local.x, -1, 1) : 0;
        }

        /// <summary>Snapshots recoil now; never retains the damage source or its transform.</summary>
        public static float RecoilDirection(DamageInfo hit, Vector3 victim, Quaternion reference)
        {
            var direction = hit.Source ? victim - hit.Source.transform.position : Vector3.zero;
            if (!Horizontal(direction)) direction = victim - hit.Point;
            return SignedDirection(direction, reference);
        }
    }
}
