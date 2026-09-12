using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Raid;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class RootShatterComboTests
    {
        GameObject entObject, otherEntObject, invaderObject, otherTargetObject, otherSource;
        CombatEntity ent, otherEnt, invader, otherTarget;
        AbilityDefinition slam, otherAbility, wrongName, wrongKind;

        [SetUp]
        public void SetUp()
        {
            entObject = new GameObject("Exact Ent", typeof(CombatEntity));
            otherEntObject = new GameObject("Other Ent", typeof(CombatEntity));
            invaderObject = new GameObject("Exact Invader", typeof(CombatEntity));
            otherTargetObject = new GameObject("Other Target", typeof(CombatEntity));
            otherSource = new GameObject("Other Damage Source");
            ent = entObject.GetComponent<CombatEntity>();
            otherEnt = otherEntObject.GetComponent<CombatEntity>();
            invader = invaderObject.GetComponent<CombatEntity>();
            otherTarget = otherTargetObject.GetComponent<CombatEntity>();
            slam = Ability("Ground Slam", AbilityKind.Area);
            otherAbility = Ability("Smash", AbilityKind.Melee);
            wrongName = Ability("Thorn Burst", AbilityKind.Area);
            wrongKind = Ability("Ground Slam", AbilityKind.Melee);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(entObject);
            Object.DestroyImmediate(otherEntObject);
            Object.DestroyImmediate(invaderObject);
            Object.DestroyImmediate(otherTargetObject);
            Object.DestroyImmediate(otherSource);
            Object.DestroyImmediate(slam);
            Object.DestroyImmediate(otherAbility);
            Object.DestroyImmediate(wrongName);
            Object.DestroyImmediate(wrongKind);
        }

        [Test]
        public void EligibilityRequiresExactRevisionImpactIdentityControllerRootAndOrdinaryHit()
        {
            Assert.That(RootShatterCombo.IsEligible(Facts()), Is.True);
            Assert.That(RootShatterCombo.IsEligible(Facts(revision: 0)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(actionId: 0)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(actor: otherEnt)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(target: otherTarget)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(impactAbility: otherAbility)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(slotTwo: wrongName, impactAbility: wrongName)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(slotTwo: wrongKind, impactAbility: wrongKind)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(source: otherSource)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(amount: 0)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(phase: CombatActionPhase.Windup)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(end: CombatPresentationEnd.ControllerChanged)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(direct: false)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(rooted: false)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(entAlive: false)), Is.False);
            Assert.That(RootShatterCombo.IsEligible(Facts(terminal: true)), Is.False);
        }

        RootShatterEligibility Facts(long revision = 1, long actionId = 7, CombatEntity actor = null,
            CombatEntity target = null, AbilityDefinition slotTwo = null, AbilityDefinition impactAbility = null,
            GameObject source = null, float amount = 20, CombatActionPhase phase = CombatActionPhase.Impact,
            CombatPresentationEnd end = CombatPresentationEnd.None, bool direct = true, bool rooted = true,
            bool entAlive = true, bool terminal = false)
        {
            actor ??= ent;
            target ??= invader;
            slotTwo ??= slam;
            impactAbility ??= slam;
            source ??= entObject;
            var impact = new CombatPresentationFact(actionId, phase, end, Vector3.forward, Quaternion.identity,
                impactAbility, .1f, 1, 1);
            return new RootShatterEligibility(revision, ent, actor, invader, target, slotTwo, impact,
                new DamageInfo(amount, source, Vector3.forward), direct, rooted, entAlive, terminal);
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
