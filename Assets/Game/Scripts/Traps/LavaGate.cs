using RealmRaiders.Characters;
using RealmRaiders.Combat;
using UnityEngine;

namespace RealmRaiders.Traps
{
    public sealed class LavaGate : TrapBase
    {
        public bool IsRaised { get; private set; }
        Collider gateCollider;
        protected override float CooldownSeconds => 10;
        public override void Initialize(CombatEntity target) { base.Initialize(target); gateCollider = GetComponent<Collider>(); }
        protected override void ActivateEffect(CombatEntity target)
        { IsRaised = true; if (gateCollider) gateCollider.enabled = true; target.Health.TakeDamage(new DamageInfo(16, gameObject, target.transform.position), target.Stats.Armor); target.ApplyRoot(1.2f); }
        protected override void Update()
        { base.Update(); if (State == TrapState.Ready && IsRaised) { IsRaised = false; if (gateCollider) gateCollider.enabled = false; } }
        protected override UnityEngine.Color ReadyColor => new(.75f, .14f, .03f);
        protected override UnityEngine.Color CooldownColor => new(.18f, .06f, .02f);
    }
}
