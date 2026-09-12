using System;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Possession;
using RealmRaiders.Traps;
using UnityEngine;

namespace RealmRaiders.Raid
{
    public readonly struct RootShatterEligibility
    {
        public RootShatterEligibility(long activationRevision, CombatEntity configuredEnt, CombatEntity impactActor,
            CombatEntity configuredInvader, CombatEntity damagedTarget, AbilityDefinition configuredSlotTwo,
            CombatPresentationFact impact, DamageInfo ordinaryDamage, bool directControl, bool rooted,
            bool entAlive, bool terminal)
        {
            ActivationRevision = activationRevision;
            ConfiguredEnt = configuredEnt;
            ImpactActor = impactActor;
            ConfiguredInvader = configuredInvader;
            DamagedTarget = damagedTarget;
            ConfiguredSlotTwo = configuredSlotTwo;
            Impact = impact;
            OrdinaryDamage = ordinaryDamage;
            DirectControl = directControl;
            Rooted = rooted;
            EntAlive = entAlive;
            Terminal = terminal;
        }

        public long ActivationRevision { get; }
        public CombatEntity ConfiguredEnt { get; }
        public CombatEntity ImpactActor { get; }
        public CombatEntity ConfiguredInvader { get; }
        public CombatEntity DamagedTarget { get; }
        public AbilityDefinition ConfiguredSlotTwo { get; }
        public CombatPresentationFact Impact { get; }
        public DamageInfo OrdinaryDamage { get; }
        public bool DirectControl { get; }
        public bool Rooted { get; }
        public bool EntAlive { get; }
        public bool Terminal { get; }
    }

    [DisallowMultipleComponent]
    public sealed class RootShatterCombo : MonoBehaviour
    {
        public const float BonusRawDamage = 18f;
        public const string ConfirmationCopy = "ROOT SHATTER!";

        public event Action Resolved;
        public event Action Cleared;

        RootTrap trap;
        CombatEntity invader;
        CombatEntity ent;
        PossessionManager possession;
        DefenseManager defense;
        long armedRevision;
        long impactActionId;
        AbilityDefinition impactAbility;
        bool operational;
        bool directControlObserved;
        bool feedbackActive;

        public bool IsOperational => operational;
        public bool IsArmed => operational && armedRevision > 0;
        public long ArmedRevision => IsArmed ? armedRevision : 0;

        public void Initialize(RootTrap exactTrap, CombatEntity exactInvader, CombatEntity exactEnt,
            PossessionManager possessionManager, DefenseManager defenseManager)
        {
            Shutdown(false);
            if (!exactTrap || !exactInvader || exactInvader.Health == null || exactInvader.Health.IsDead ||
                !exactEnt || exactEnt.Health == null || exactEnt.Health.IsDead || possessionManager == null || !defenseManager) return;
            trap = exactTrap;
            invader = exactInvader;
            ent = exactEnt;
            possession = possessionManager;
            defense = defenseManager;
            trap.Activated += OnTrapActivated;
            invader.Health.Damaged += OnInvaderDamaged;
            invader.Health.Died += OnParticipantDied;
            ent.Health.Died += OnParticipantDied;
            ent.PresentationChanged += OnEntPresentation;
            possession.PossessionChanged += OnPossessionChanged;
            possession.Released += OnReleased;
            defense.StateChanged += OnDefenseState;
            operational = true;
        }

        public static bool IsEligible(RootShatterEligibility facts)
        {
            var ability = facts.ConfiguredSlotTwo;
            var amount = facts.OrdinaryDamage.Amount;
            return facts.ActivationRevision > 0 && facts.Impact.ActionId > 0 &&
                   facts.ConfiguredEnt && facts.ImpactActor == facts.ConfiguredEnt &&
                   facts.ConfiguredInvader && facts.DamagedTarget == facts.ConfiguredInvader &&
                   ability && facts.Impact.Ability == ability &&
                   ability.Kind == AbilityKind.Area && string.Equals(ability.DisplayName, "Ground Slam", StringComparison.Ordinal) &&
                   facts.Impact.Phase == CombatActionPhase.Impact && facts.Impact.End == CombatPresentationEnd.None &&
                   facts.OrdinaryDamage.Source == facts.ConfiguredEnt.gameObject && amount > 0 &&
                   !float.IsNaN(amount) && !float.IsInfinity(amount) && facts.DirectControl && facts.Rooted &&
                   facts.EntAlive && !facts.Terminal;
        }

        void Update()
        {
            if (!operational) return;
            if (IsTerminal())
            {
                ClearState();
                return;
            }
            if (armedRevision > 0 && (!invader || invader.Health == null || invader.Health.IsDead || !invader.IsRooted))
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
            if (!operational || revision <= 0 || !invader || invader.Health == null || invader.Health.IsDead ||
                !invader.IsRooted || IsTerminal()) return;
            armedRevision = revision;
        }

        void OnEntPresentation(CombatPresentationFact fact)
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
            var slotTwo = ExactSlotTwoGroundSlam();
            var fact = new CombatPresentationFact(impactActionId, ent ? ent.ActionPhase : CombatActionPhase.Idle,
                CombatPresentationEnd.None, Vector3.zero, Quaternion.identity, impactAbility, 0, Time.time, Time.unscaledTime);
            var eligible = IsEligible(new RootShatterEligibility(armedRevision, ent, ent, invader, invader,
                slotTwo, fact, hit, HasExactDirectControl(), invader && invader.IsRooted,
                ent && ent.Health != null && !ent.Health.IsDead, IsTerminal()));
            if (!eligible) return;

            armedRevision = 0;
            impactActionId = 0;
            impactAbility = null;
            invader.BreakRoot();
            feedbackActive = true;
            if (!invader.Health.IsDead)
                invader.Health.TakeDamage(new DamageInfo(BonusRawDamage, ent.gameObject, hit.Point), invader.Stats.Armor);
            Resolved?.Invoke();
        }

        AbilityDefinition ExactSlotTwoGroundSlam()
        {
            if (!ent || ent.Abilities == null || ent.Abilities.Count <= 2 || ent.Definition == null ||
                ent.Definition.Abilities == null || ent.Definition.Abilities.Length <= 2) return null;
            var runtime = ent.Abilities[2]?.Definition;
            return runtime && runtime == ent.Definition.Abilities[2] ? runtime : null;
        }

        bool HasExactDirectControl()
        {
            if (!operational || possession == null || possession.Possessed != ent || !ent) return false;
            var player = ent.ActiveController as PlayerController;
            return player && player.IsActive && player.isActiveAndEnabled;
        }

        bool IsTerminal() => GameplayInput.TerminalState || !defense || defense.IsFinished;

        void OnPossessionChanged(CombatEntity value)
        {
            if (!operational) return;
            if (value == ent && HasExactDirectControl()) { directControlObserved = true; return; }
            if (directControlObserved) ClearState();
        }

        void OnReleased(bool _) { if (directControlObserved || feedbackActive) ClearState(); }
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
            if (trap) trap.Activated -= OnTrapActivated;
            if (invader && invader.Health != null)
            {
                invader.Health.Damaged -= OnInvaderDamaged;
                invader.Health.Died -= OnParticipantDied;
            }
            if (ent)
            {
                ent.PresentationChanged -= OnEntPresentation;
                if (ent.Health != null) ent.Health.Died -= OnParticipantDied;
            }
            if (possession != null)
            {
                possession.PossessionChanged -= OnPossessionChanged;
                possession.Released -= OnReleased;
            }
            if (defense) defense.StateChanged -= OnDefenseState;
            trap = null;
            invader = null;
            ent = null;
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
