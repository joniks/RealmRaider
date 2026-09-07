using System.Collections;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using UnityEngine;

namespace RealmRaiders.Traps
{
    public sealed class FlameTrap : TrapBase
    {
        const float PulseDamage = 8;
        const float PulseInterval = .35f;
        Coroutine burnRoutine;
        CombatEntity burnTarget;

        public int BurnPulsesRemaining { get; private set; }
        protected override float CooldownSeconds => 6;

        public override void Initialize(CombatEntity target)
        {
            if (burnTarget && burnTarget.Health != null) burnTarget.Health.Died -= StopBurn;
            StopBurn();
            base.Initialize(target);
            Automatic = false;
            burnTarget = target;
            if (burnTarget && burnTarget.Health != null) burnTarget.Health.Died += StopBurn;
        }

        protected override void ActivateEffect(CombatEntity target)
        {
            StopBurn();
            ApplyPulse(target);
            if (!CanContinueBurn(target)) return;
            BurnPulsesRemaining = 2;
            burnRoutine = StartCoroutine(ContinueBurn(target));
        }

        IEnumerator ContinueBurn(CombatEntity target)
        {
            for (var pulse = 0; pulse < 2; pulse++)
            {
                yield return new WaitForSeconds(PulseInterval);
                if (!CanContinueBurn(target)) break;
                BurnPulsesRemaining--;
                ApplyPulse(target);
                if (!CanContinueBurn(target)) break;
            }
            burnRoutine = null;
            BurnPulsesRemaining = 0;
        }

        void ApplyPulse(CombatEntity target)
        {
            target.Health.TakeDamage(new DamageInfo(PulseDamage, gameObject, target.transform.position), target.Stats.Armor);
        }

        bool CanContinueBurn(CombatEntity target) => isActiveAndEnabled && target && target.Health != null && !target.Health.IsDead;

        void StopBurn()
        {
            if (burnRoutine != null) StopCoroutine(burnRoutine);
            burnRoutine = null;
            BurnPulsesRemaining = 0;
        }

        void OnDisable() => StopBurn();
        void OnDestroy()
        {
            if (burnTarget && burnTarget.Health != null) burnTarget.Health.Died -= StopBurn;
            StopBurn();
        }

        protected override Color ReadyColor => new(.95f, .25f, .04f);
        protected override Color CooldownColor => new(.3f, .08f, .02f);
    }
}
