using System;
using System.Collections;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using UnityEngine;

namespace RealmRaiders.Traps
{
    public sealed class FlameTrap : TrapBase
    {
        const float PulseDamage = 8;
        const float PulseInterval = 1.5f;
        Coroutine burnRoutine;
        CombatEntity burnTarget;

        public event Action<long> Activated;
        public event Action BurnChanged;

        public long ActivationRevision { get; private set; }
        public int BurnPulsesRemaining { get; private set; }
        protected override float CooldownSeconds => 6;

        public override bool TryActivate()
        {
            if (!base.TryActivate()) return false;
            ActivationRevision++;
            Activated?.Invoke(ActivationRevision);
            return true;
        }

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
            SetRemainingPulses(2);
            burnRoutine = StartCoroutine(ContinueBurn(target));
        }

        IEnumerator ContinueBurn(CombatEntity target)
        {
            for (var pulse = 0; pulse < 2; pulse++)
            {
                yield return new WaitForSeconds(PulseInterval);
                if (!CanContinueBurn(target)) break;
                SetRemainingPulses(BurnPulsesRemaining - 1);
                ApplyPulse(target);
                if (!CanContinueBurn(target)) break;
            }
            burnRoutine = null;
            SetRemainingPulses(0);
        }

        public bool TryDetonateRemaining(CombatEntity exactTarget, long activationRevision, out int claimedPulses)
        {
            claimedPulses = 0;
            if (!isActiveAndEnabled || !exactTarget || exactTarget != burnTarget || exactTarget.Health == null ||
                exactTarget.Health.IsDamageImmune || activationRevision <= 0 || activationRevision != ActivationRevision ||
                BurnPulsesRemaining <= 0) return false;

            claimedPulses = BurnPulsesRemaining;
            if (burnRoutine != null) StopCoroutine(burnRoutine);
            burnRoutine = null;
            SetRemainingPulses(0);

            if (exactTarget.Health.IsDead) return true;
            for (var pulse = 0; pulse < claimedPulses && !exactTarget.Health.IsDead; pulse++) ApplyPulse(exactTarget);
            return true;
        }

        void ApplyPulse(CombatEntity target)
        {
            target.Health.TakeDamage(new DamageInfo(PulseDamage, gameObject, target.transform.position), target.Stats.Armor);
        }

        bool CanContinueBurn(CombatEntity target) => isActiveAndEnabled && target && target.Health != null && !target.Health.IsDead;

        void SetRemainingPulses(int value)
        {
            value = Mathf.Max(0, value);
            if (BurnPulsesRemaining == value) return;
            BurnPulsesRemaining = value;
            BurnChanged?.Invoke();
        }

        void StopBurn()
        {
            if (burnRoutine != null) StopCoroutine(burnRoutine);
            burnRoutine = null;
            SetRemainingPulses(0);
        }

        void OnDisable() => StopBurn();
        void OnDestroy()
        {
            if (burnTarget && burnTarget.Health != null) burnTarget.Health.Died -= StopBurn;
            StopBurn();
            Activated = null;
            BurnChanged = null;
        }

        protected override Color ReadyColor => new(.95f, .25f, .04f);
        protected override Color CooldownColor => new(.3f, .08f, .02f);
    }
}
