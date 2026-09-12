using System;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Possession;
using RealmRaiders.Traps;
using UnityEngine;

namespace RealmRaiders.Raid
{
    public readonly struct FlameRushEligibility
    {
        public FlameRushEligibility(long activationRevision, int remainingBurnPulses,
            CombatEntity configuredBrute, CombatEntity impactActor, CombatEntity configuredInvader,
            CombatEntity damagedTarget, AbilityDefinition configuredSlotOne, CombatPresentationFact impact,
            DamageInfo ordinaryDamage, bool directControl, bool bruteAlive, bool targetDamageImmune, bool terminal)
        {
            ActivationRevision = activationRevision;
            RemainingBurnPulses = remainingBurnPulses;
            ConfiguredBrute = configuredBrute;
            ImpactActor = impactActor;
            ConfiguredInvader = configuredInvader;
            DamagedTarget = damagedTarget;
            ConfiguredSlotOne = configuredSlotOne;
            Impact = impact;
            OrdinaryDamage = ordinaryDamage;
            DirectControl = directControl;
            BruteAlive = bruteAlive;
            TargetDamageImmune = targetDamageImmune;
            Terminal = terminal;
        }

        public long ActivationRevision { get; }
        public int RemainingBurnPulses { get; }
        public CombatEntity ConfiguredBrute { get; }
        public CombatEntity ImpactActor { get; }
        public CombatEntity ConfiguredInvader { get; }
        public CombatEntity DamagedTarget { get; }
        public AbilityDefinition ConfiguredSlotOne { get; }
        public CombatPresentationFact Impact { get; }
        public DamageInfo OrdinaryDamage { get; }
        public bool DirectControl { get; }
        public bool BruteAlive { get; }
        public bool TargetDamageImmune { get; }
        public bool Terminal { get; }
    }

    [DisallowMultipleComponent]
    public sealed class FlameRushCombo : MonoBehaviour
    {
        public const string OpportunityCopy = "IGNITED — POSSESS BRUTE, THEN CHARGE";
        public const string ConfirmationCopy = "FLAME RUSH!";

        public event Action<int> Resolved;
        public event Action Cleared;

        FlameTrap trap;
        CombatEntity invader;
        CombatEntity brute;
        PossessionManager possession;
        DefenseManager defense;
        long armedRevision;
        long impactActionId;
        AbilityDefinition impactAbility;
        bool operational;
        bool directControlObserved;
        bool feedbackActive;

        public bool IsOperational => operational;
        public bool IsArmed => operational && armedRevision > 0 && trap && trap.BurnPulsesRemaining > 0;
        public long ArmedRevision => IsArmed ? armedRevision : 0;

        public void Initialize(FlameTrap exactTrap, CombatEntity exactInvader, CombatEntity exactBrute,
            PossessionManager possessionManager, DefenseManager defenseManager)
        {
            Shutdown(false);
            if (!exactTrap || !exactInvader || exactInvader.Health == null || exactInvader.Health.IsDead ||
                !exactBrute || exactBrute.Health == null || exactBrute.Health.IsDead || possessionManager == null ||
                !defenseManager) return;

            trap = exactTrap;
            invader = exactInvader;
            brute = exactBrute;
            possession = possessionManager;
            defense = defenseManager;
            trap.Activated += OnTrapActivated;
            trap.BurnChanged += OnBurnChanged;
            invader.Health.Damaged += OnInvaderDamaged;
            invader.Health.Died += OnParticipantDied;
            brute.Health.Died += OnParticipantDied;
            brute.PresentationChanged += OnBrutePresentation;
            possession.PossessionChanged += OnPossessionChanged;
            possession.Released += OnReleased;
            defense.StateChanged += OnDefenseState;
            operational = true;
        }

        public static bool IsEligible(FlameRushEligibility facts)
        {
            var ability = facts.ConfiguredSlotOne;
            var amount = facts.OrdinaryDamage.Amount;
            return facts.ActivationRevision > 0 && facts.RemainingBurnPulses > 0 &&
                   facts.Impact.ActionId > 0 && facts.ConfiguredBrute &&
                   facts.ImpactActor == facts.ConfiguredBrute && facts.ConfiguredInvader &&
                   facts.DamagedTarget == facts.ConfiguredInvader && ability && facts.Impact.Ability == ability &&
                   ability.Kind == AbilityKind.Dash && string.Equals(ability.DisplayName, "Charge", StringComparison.Ordinal) &&
                   facts.Impact.Phase == CombatActionPhase.Impact && facts.Impact.End == CombatPresentationEnd.None &&
                   facts.OrdinaryDamage.Source == facts.ConfiguredBrute.gameObject && amount > 0 &&
                   !float.IsNaN(amount) && !float.IsInfinity(amount) && facts.DirectControl && facts.BruteAlive &&
                   !facts.TargetDamageImmune && !facts.Terminal;
        }

        void Update()
        {
            if (!operational) return;
            if (IsTerminal())
            {
                ClearState();
                return;
            }
            if (armedRevision > 0 && (!trap || trap.BurnPulsesRemaining <= 0))
            {
                ClearState();
                return;
            }
            if (directControlObserved && !HasExactDirectControl()) ClearState();
        }

        void OnTrapActivated(long revision)
        {
            var directNow = HasExactDirectControl();
            ClearState();
            directControlObserved = directNow;
            if (!operational || revision <= 0 || !trap || trap.BurnPulsesRemaining <= 0 ||
                !invader || invader.Health == null || invader.Health.IsDead || !brute || brute.Health == null ||
                brute.Health.IsDead || IsTerminal()) return;
            armedRevision = revision;
        }

        void OnBurnChanged()
        {
            if (operational && armedRevision > 0 && (!trap || trap.BurnPulsesRemaining <= 0)) ClearState();
        }

        void OnBrutePresentation(CombatPresentationFact fact)
        {
            if (!operational) return;
            if (fact.End is CombatPresentationEnd.ControllerChanged or CombatPresentationEnd.Death or
                CombatPresentationEnd.Terminal or CombatPresentationEnd.Disabled or CombatPresentationEnd.Destroyed)
            {
                impactActionId = 0;
                impactAbility = null;
                if (directControlObserved) ClearState();
                return;
            }
            if (fact.Phase == CombatActionPhase.Impact && fact.End == CombatPresentationEnd.None)
            {
                impactActionId = fact.ActionId;
                impactAbility = fact.Ability;
            }
            else if (fact.Phase != CombatActionPhase.Impact)
            {
                impactActionId = 0;
                impactAbility = null;
            }
        }

        void OnInvaderDamaged(DamageInfo hit)
        {
            if (!operational || armedRevision <= 0 || impactActionId <= 0) return;
            var slotOne = ExactSlotOneCharge();
            var fact = new CombatPresentationFact(impactActionId, brute ? brute.ActionPhase : CombatActionPhase.Idle,
                CombatPresentationEnd.None, Vector3.zero, Quaternion.identity, impactAbility, 0,
                Time.time, Time.unscaledTime);
            var eligible = IsEligible(new FlameRushEligibility(armedRevision, trap ? trap.BurnPulsesRemaining : 0,
                brute, brute, invader, invader, slotOne, fact, hit, HasExactDirectControl(),
                brute && brute.Health != null && !brute.Health.IsDead,
                invader && invader.Health != null && invader.Health.IsDamageImmune, IsTerminal()));
            if (!eligible) return;

            var revision = armedRevision;
            armedRevision = 0;
            impactActionId = 0;
            impactAbility = null;
            if (!trap.TryDetonateRemaining(invader, revision, out var claimedPulses))
            {
                ClearState();
                return;
            }

            feedbackActive = true;
            Resolved?.Invoke(claimedPulses);
        }

        AbilityDefinition ExactSlotOneCharge()
        {
            if (!brute || brute.Abilities == null || brute.Abilities.Count <= 1 || brute.Definition == null ||
                brute.Definition.Abilities == null || brute.Definition.Abilities.Length <= 1) return null;
            var runtime = brute.Abilities[1]?.Definition;
            return runtime && runtime == brute.Definition.Abilities[1] ? runtime : null;
        }

        bool HasExactDirectControl()
        {
            if (!operational || possession == null || possession.Possessed != brute || !brute) return false;
            var player = brute.ActiveController as PlayerController;
            return player && player.IsActive && player.isActiveAndEnabled;
        }

        bool IsTerminal() => GameplayInput.TerminalState || !defense || defense.IsFinished;

        void OnPossessionChanged(CombatEntity value)
        {
            if (!operational) return;
            if (value == brute && HasExactDirectControl())
            {
                directControlObserved = true;
                return;
            }
            if (directControlObserved) ClearState();
        }

        void OnReleased(bool _) { if (armedRevision > 0 || directControlObserved || feedbackActive) ClearState(); }
        void OnParticipantDied() => ClearState();
        void OnDefenseState(DefenseState state)
        {
            if (state is DefenseState.DefenderVictory or DefenseState.RealmLost) ClearState();
        }

        void ClearState()
        {
            var changed = armedRevision > 0 || impactActionId > 0 || directControlObserved || feedbackActive;
            ResetState();
            if (changed) Cleared?.Invoke();
        }

        void ResetState()
        {
            armedRevision = 0;
            impactActionId = 0;
            impactAbility = null;
            directControlObserved = false;
            feedbackActive = false;
        }

        void Shutdown(bool notify)
        {
            if (notify) ClearState();
            else ResetState();
            if (trap)
            {
                trap.Activated -= OnTrapActivated;
                trap.BurnChanged -= OnBurnChanged;
            }
            if (invader && invader.Health != null)
            {
                invader.Health.Damaged -= OnInvaderDamaged;
                invader.Health.Died -= OnParticipantDied;
            }
            if (brute)
            {
                brute.PresentationChanged -= OnBrutePresentation;
                if (brute.Health != null) brute.Health.Died -= OnParticipantDied;
            }
            if (possession != null)
            {
                possession.PossessionChanged -= OnPossessionChanged;
                possession.Released -= OnReleased;
            }
            if (defense) defense.StateChanged -= OnDefenseState;
            trap = null;
            invader = null;
            brute = null;
            possession = null;
            defense = null;
            operational = false;
        }

        void OnDisable() => Shutdown(true);

        void OnDestroy()
        {
            Shutdown(true);
            Resolved = null;
            Cleared = null;
        }
    }
}
