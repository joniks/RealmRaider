using RealmRaiders.Characters;
using RealmRaiders.Combat;

namespace RealmRaiders.Traps
{
    public sealed class RootTrap : TrapBase
    {
        protected override void ActivateEffect(CombatEntity target)
        { target.ApplyRoot(2.25f); target.Health.TakeDamage(new DamageInfo(12, gameObject, target.transform.position), target.Stats.Armor); }
    }
}
