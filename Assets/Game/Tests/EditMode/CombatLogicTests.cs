using NUnit.Framework;
using RealmRaiders.Combat;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class CombatLogicTests
    {
        GameObject subject;
        Health health;

        [SetUp]
        public void SetUp()
        { subject = new GameObject("Health test", typeof(Health)); health = subject.GetComponent<Health>(); health.Initialize(100); }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(subject);

        [Test]
        public void Damage_IsReducedByArmor()
        {
            health.TakeDamage(new DamageInfo(100, null, Vector3.zero), 100);
            Assert.That(health.Current, Is.EqualTo(50).Within(.01f));
        }

        [Test]
        public void Death_FiresOnce_AndHealthDoesNotGoNegative()
        {
            int deaths = 0; health.Died += () => deaths++;
            health.TakeDamage(new DamageInfo(500, null, Vector3.zero), 0);
            health.TakeDamage(new DamageInfo(500, null, Vector3.zero), 0);
            Assert.That(health.Current, Is.Zero); Assert.That(deaths, Is.EqualTo(1));
        }

        [Test]
        public void RestoreFull_UsesConfiguredMaximum()
        {
            health.TakeDamage(new DamageInfo(20, null, Vector3.zero), 0); health.RestoreFull();
            Assert.That(health.Current, Is.EqualTo(100));
        }
    }
}
