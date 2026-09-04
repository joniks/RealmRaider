namespace RealmRaiders.Combat
{
    public enum CombatActionPhase { Idle, Windup, Impact, Recovery }

    /// <summary>Small action gate shared by all entity controllers.</summary>
    public sealed class CombatActionState
    {
        public CombatActionPhase Phase { get; private set; } = CombatActionPhase.Idle;
        public bool IsResolving => Phase != CombatActionPhase.Idle;

        public bool TryBegin()
        {
            if (IsResolving) return false;
            Phase = CombatActionPhase.Windup;
            return true;
        }

        public void Impact() { if (IsResolving) Phase = CombatActionPhase.Impact; }
        public void Recover() { if (IsResolving) Phase = CombatActionPhase.Recovery; }
        public void Complete() => Phase = CombatActionPhase.Idle;
    }
}
