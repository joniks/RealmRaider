using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Raid;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class FlameRushComboTests
    {
        GameObject bruteObject, otherBruteObject, invaderObject, otherTargetObject, otherSource;
        CombatEntity brute, otherBrute, invader, otherTarget;
        AbilityDefinition charge, otherAbility, wrongName, wrongKind;

        [SetUp]
        public void SetUp()
        {
            bruteObject = new GameObject("Exact Brute", typeof(CombatEntity));
            otherBruteObject = new GameObject("Other Brute", typeof(CombatEntity));
            invaderObject = new GameObject("Exact Invader", typeof(CombatEntity));
            otherTargetObject = new GameObject("Other Target", typeof(CombatEntity));
            otherSource = new GameObject("Other Damage Source");
            brute = bruteObject.GetComponent<CombatEntity>();
            otherBrute = otherBruteObject.GetComponent<CombatEntity>();
            invader = invaderObject.GetComponent<CombatEntity>();
            otherTarget = otherTargetObject.GetComponent<CombatEntity>();
            charge = Ability("Charge", AbilityKind.Dash);
            otherAbility = Ability("Smash", AbilityKind.Melee);
            wrongName = Ability("Flame Lunge", AbilityKind.Dash);
            wrongKind = Ability("Charge", AbilityKind.Area);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(bruteObject);
            Object.DestroyImmediate(otherBruteObject);
            Object.DestroyImmediate(invaderObject);
            Object.DestroyImmediate(otherTargetObject);
            Object.DestroyImmediate(otherSource);
            Object.DestroyImmediate(charge);
            Object.DestroyImmediate(otherAbility);
            Object.DestroyImmediate(wrongName);
            Object.DestroyImmediate(wrongKind);
        }

        [Test]
        public void EligibilityRequiresExactRevisionBurnActorTargetSlotChargeControllerAndAppliedHit()
        {
            Assert.That(FlameRushCombo.IsEligible(Facts()), Is.True);
            Assert.That(FlameRushCombo.IsEligible(Facts(revision: 0)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(remaining: 0)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(actionId: 0)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(actor: otherBrute)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(target: otherTarget)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(impactAbility: otherAbility)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(slotOne: wrongName, impactAbility: wrongName)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(slotOne: wrongKind, impactAbility: wrongKind)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(source: otherSource)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(amount: 0)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(amount: float.NaN)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(phase: CombatActionPhase.Windup)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(end: CombatPresentationEnd.ControllerChanged)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(direct: false)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(bruteAlive: false)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(targetDamageImmune: true)), Is.False);
            Assert.That(FlameRushCombo.IsEligible(Facts(terminal: true)), Is.False);
        }

        FlameRushEligibility Facts(long revision = 1, int remaining = 2, long actionId = 7,
            CombatEntity actor = null, CombatEntity target = null, AbilityDefinition slotOne = null,
            AbilityDefinition impactAbility = null, GameObject source = null, float amount = 28,
            CombatActionPhase phase = CombatActionPhase.Impact, CombatPresentationEnd end = CombatPresentationEnd.None,
            bool direct = true, bool bruteAlive = true, bool targetDamageImmune = false, bool terminal = false)
        {
            actor ??= brute;
            target ??= invader;
            slotOne ??= charge;
            impactAbility ??= charge;
            source ??= bruteObject;
            var impact = new CombatPresentationFact(actionId, phase, end, Vector3.forward, Quaternion.identity,
                impactAbility, .1f, 1, 1);
            return new FlameRushEligibility(revision, remaining, brute, actor, invader, target, slotOne, impact,
                new DamageInfo(amount, source, Vector3.forward), direct, bruteAlive, targetDamageImmune, terminal);
        }

        static AbilityDefinition Ability(string name, AbilityKind kind)
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            ability.DisplayName = name;
            ability.Kind = kind;
            return ability;
        }
    }
}
