using NUnit.Framework;
using RealmRaiders.Possession;

namespace RealmRaiders.Tests
{
    public sealed class PossessionEnergyTests
    {
        [Test]
        public void Consume_DepletesAndClampsAtZero()
        {
            var energy = new PossessionEnergy(30);
            Assert.That(energy.Consume(12), Is.True);
            Assert.That(energy.Consume(25), Is.False);
            Assert.That(energy.Remaining, Is.Zero);
            Assert.That(energy.IsDepleted, Is.True);
        }

        [Test]
        public void Refill_RestoresMaximum()
        {
            var energy = new PossessionEnergy(30); energy.Consume(14); energy.Refill();
            Assert.That(energy.Remaining, Is.EqualTo(30));
        }

        [Test]
        public void NonPositiveConsumption_DoesNotChangeEnergy()
        {
            var energy = new PossessionEnergy(30); energy.Consume(-2);
            Assert.That(energy.Remaining, Is.EqualTo(30));
        }
    }
}
