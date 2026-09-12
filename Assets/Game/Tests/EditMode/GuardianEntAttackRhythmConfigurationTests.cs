using NUnit.Framework;
using RealmRaiders.Combat;
using RealmRaiders.Core;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class GuardianEntAttackRhythmConfigurationTests
    {
        [Test]
        public void AbilityRecovery_DefaultsToLegacyAndFactoryCanConfigureEntSlam()
        {
            var legacy = ScriptableObject.CreateInstance<AbilityDefinition>();
            var slam = PrototypeRuntimeFactory.Ability(
                "Ground Slam", AbilityKind.Area, 38, 1, 4, .75f, recovery: .8f);
            try
            {
                Assert.That(legacy.Recovery, Is.EqualTo(.12f).Within(.0001f));
                Assert.That(slam.Recovery, Is.EqualTo(.8f).Within(.0001f));
            }
            finally
            {
                Object.DestroyImmediate(legacy);
                Object.DestroyImmediate(slam);
            }
        }
    }
}
