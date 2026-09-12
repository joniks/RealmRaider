using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class MoonwellRecoveryTests
    {
        GameObject heroObject, managerObject, wellObject;
        CharacterDefinition definition;
        CombatEntity hero;
        RaidManager raid;
        MoonwellRecovery well;

        [SetUp]
        public void SetUp()
        {
            definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3 };
            heroObject = new GameObject("Exact Raid Hero", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            hero = heroObject.GetComponent<CombatEntity>(); hero.Initialize(definition);
            managerObject = new GameObject("Raid Manager", typeof(RaidManager));
            raid = managerObject.GetComponent<RaidManager>(); raid.Initialize(hero, System.Array.Empty<RealmNodeView>(), System.Array.Empty<CombatEntity>(), Vector3.forward * 20);
            wellObject = new GameObject("Moonwell", typeof(MoonwellRecovery));
            well = wellObject.GetComponent<MoonwellRecovery>(); well.Initialize(hero, raid, null);
        }

        [TearDown]
        public void TearDown()
        {
            Object.DestroyImmediate(wellObject); Object.DestroyImmediate(managerObject); Object.DestroyImmediate(heroObject); Object.DestroyImmediate(definition);
        }

        [Test]
        public void OneCharge_HealsThirtyPercentWithoutWasteAndRejectsReentry()
        {
            heroObject.transform.position = wellObject.transform.position;
            hero.Health.TakeDamage(new DamageInfo(60, null, heroObject.transform.position), 0);
            float nestedResult = -1;
            hero.Health.Changed += (_, _) => nestedResult = well.TryUse();

            Assert.That(well.TryUse(), Is.EqualTo(30f).Within(.001f));
            Assert.That(nestedResult, Is.Zero, "Changed callbacks cannot consume or duplicate an in-progress use.");
            Assert.That(hero.Health.Current, Is.EqualTo(70));
            Assert.That(well.HasCharge, Is.False); Assert.That(well.State, Is.EqualTo(MoonwellRecoveryState.Spent));
            well.Initialize(hero, raid, null);
            Assert.That(well.HasCharge, Is.False, "Idempotent initialization cannot refill the same raid charge.");
            Assert.That(well.TryUse(), Is.Zero); Assert.That(hero.Health.Current, Is.EqualTo(70));
        }

        [Test]
        public void FullFarAndDisabledRequestsPreserveChargeForAValidReturn()
        {
            heroObject.transform.position = wellObject.transform.position;
            Assert.That(well.State, Is.EqualTo(MoonwellRecoveryState.FullHealth));
            Assert.That(well.TryUse(), Is.Zero); Assert.That(well.HasCharge, Is.True);

            hero.Health.TakeDamage(new DamageInfo(60, null, heroObject.transform.position), 0);
            heroObject.transform.position = wellObject.transform.position + Vector3.right * (MoonwellRecovery.InteractionRadius + .1f);
            Assert.That(well.State, Is.EqualTo(MoonwellRecoveryState.OutOfRange));
            Assert.That(well.TryUse(), Is.Zero); Assert.That(well.HasCharge, Is.True);

            heroObject.transform.position = wellObject.transform.position;
            well.enabled = false;
            Assert.That(well.TryUse(), Is.Zero); Assert.That(well.HasCharge, Is.True);
            well.enabled = true;
            Assert.That(well.TryUse(), Is.EqualTo(30f).Within(.001f)); Assert.That(well.HasCharge, Is.False);
        }

        [Test]
        public void DeadHeroCannotConsumeOrBeResurrected()
        {
            heroObject.transform.position = wellObject.transform.position;
            raid.enabled = false;
            hero.Health.TakeDamage(new DamageInfo(1000, null, heroObject.transform.position), 0);
            Assert.That(hero.Health.IsDead, Is.True);
            Assert.That(well.State, Is.EqualTo(MoonwellRecoveryState.Unavailable));
            Assert.That(well.TryUse(), Is.Zero); Assert.That(well.HasCharge, Is.True); Assert.That(hero.Health.IsDead, Is.True);
        }
    }
}
