using RealmRaiders.Characters;
using RealmRaiders.Combat;

namespace RealmRaiders.Traps
{
    public sealed class FlameTrap : TrapBase
    {
        protected override float CooldownSeconds => 6;
        protected override void ActivateEffect(CombatEntity target)
        { target.Health.TakeDamage(new DamageInfo(24, gameObject, target.transform.position), target.Stats.Armor); target.ApplyRoot(.65f); }
        protected override UnityEngine.Color ReadyColor => new(.95f, .25f, .04f);
        protected override UnityEngine.Color CooldownColor => new(.3f, .08f, .02f);
    }
}
