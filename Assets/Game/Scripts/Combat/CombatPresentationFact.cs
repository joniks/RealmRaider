using UnityEngine;

namespace RealmRaiders.Combat
{
    public enum CombatPresentationEnd { None, Completed, ControllerChanged, Death, Terminal, Disabled, Destroyed }

    /// <summary>Passive snapshot of an accepted action, never an instruction to gameplay.</summary>
    public readonly struct CombatPresentationFact
    {
        public readonly long ActionId;
        public readonly CombatActionPhase Phase;
        public readonly CombatPresentationEnd End;
        public readonly Vector3 WorldDirection;
        public readonly Quaternion FacingBeforeAction;
        public readonly AbilityDefinition Ability;
        public readonly float WindupSeconds;
        public readonly float ScaledTime;
        public readonly float UnscaledTime;

        public CombatPresentationFact(long actionId, CombatActionPhase phase, CombatPresentationEnd end,
            Vector3 worldDirection, Quaternion facingBeforeAction, AbilityDefinition ability,
            float windupSeconds, float scaledTime, float unscaledTime)
        {
            ActionId = actionId; Phase = phase; End = end; WorldDirection = worldDirection;
            FacingBeforeAction = facingBeforeAction; Ability = ability; WindupSeconds = windupSeconds;
            ScaledTime = scaledTime; UnscaledTime = unscaledTime;
        }
    }
}
