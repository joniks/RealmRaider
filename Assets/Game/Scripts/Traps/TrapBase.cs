using RealmRaiders.Characters;
using UnityEngine;

namespace RealmRaiders.Traps
{
    public enum TrapState { Ready, Triggered, Cooldown, Disabled }

    public abstract class TrapBase : MonoBehaviour
    {
        public TrapState State { get; protected set; } = TrapState.Ready;
        public bool Automatic { get; set; } = true;
        public float TriggerRadius { get; set; } = 2.7f;
        protected CombatEntity Target { get; private set; }
        protected Renderer Visual { get; private set; }
        float readyAt;
        public virtual void Initialize(CombatEntity target) { Target = target; Visual = GetComponent<Renderer>(); }
        protected virtual void Update()
        {
            if (!Target || Target.Health.IsDead || State == TrapState.Disabled) return;
            if (State == TrapState.Cooldown && Time.time >= readyAt) SetState(TrapState.Ready);
            if (Automatic && State == TrapState.Ready) TryActivate();
        }
        public bool TryActivate()
        {
            if (!Target || Target.Health.IsDead || State != TrapState.Ready) return false;
            var delta = Target.transform.position - transform.position; delta.y = 0;
            if (delta.sqrMagnitude > TriggerRadius * TriggerRadius) return false;
            SetState(TrapState.Triggered); ActivateEffect(Target); readyAt = Time.time + CooldownSeconds; SetState(TrapState.Cooldown); return true;
        }
        protected virtual float CooldownSeconds => 8;
        protected abstract void ActivateEffect(CombatEntity target);
        protected void SetState(TrapState next) { State = next; if (Visual) Visual.material.color = next == TrapState.Ready ? ReadyColor : CooldownColor; }
        protected virtual Color ReadyColor => new(.2f, .75f, .28f);
        protected virtual Color CooldownColor => new(.28f, .14f, .08f);
    }
}
