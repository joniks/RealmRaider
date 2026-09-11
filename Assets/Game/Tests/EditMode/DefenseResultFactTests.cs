using NUnit.Framework;
using RealmRaiders.Raid;
using RealmRaiders.UI;

namespace RealmRaiders.Tests
{
    public sealed class DefenseResultFactTests
    {
        [Test]
        public void VictoryFactRetainsExactValuesAndFormatsWithoutCausalInference()
        {
            var fact = new DefenseResultFact(DefenseState.DefenderVictory, 12.4f, 0, 220, .374f);

            Assert.That(fact.Outcome, Is.EqualTo(DefenseState.DefenderVictory));
            Assert.That(fact.Duration, Is.EqualTo(12.4f));
            Assert.That(fact.InvaderHealth, Is.Zero);
            Assert.That(fact.InvaderMaximumHealth, Is.EqualTo(220));
            Assert.That(fact.CoreProgress, Is.EqualTo(.374f));
            Assert.That(DefenderHUD.DefenseResultDebriefCopy(fact),
                Is.EqualTo("DEFENSE FACTS — INVADER DEFEATED • CORE DANGER 37% • 12s"));
        }

        [Test]
        public void LossFactReportsOnlyCapturedCoreRemainingHealthAndTime()
        {
            var fact = new DefenseResultFact(DefenseState.RealmLost, 31.6f, 18.4f, 220, 1);

            Assert.That(DefenderHUD.DefenseResultDebriefCopy(fact),
                Is.EqualTo("DEFENSE FACTS — CORE CAPTURED • INVADER 18.4/220 HP • 32s"));
        }

        [Test]
        public void FactRejectsNonTerminalOrImpossibleMeasurements()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new DefenseResultFact(DefenseState.Watching, 1, 0, 100, 0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new DefenseResultFact(DefenseState.DefenderVictory, float.NaN, 0, 100, 0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new DefenseResultFact(DefenseState.DefenderVictory, 1, 0, 0, 0));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new DefenseResultFact(DefenseState.RealmLost, 1, 101, 100, 1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new DefenseResultFact(DefenseState.RealmLost, 1, 10, 100, 1.01f));
        }
    }
}
