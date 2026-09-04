using NUnit.Framework;
using RealmRaiders.Core;

namespace RealmRaiders.Tests
{
    public sealed class PrototypeSaveTests
    {
        [Test]
        public void SelectRealm_PersistsCurrentSelection()
        {
            PrototypeSave.SelectRealm("Infernal");
            Assert.That(PrototypeSave.SelectedRealm, Is.EqualTo("Infernal"));
            PrototypeSave.SelectRealm("Sylvan");
        }
    }
}
