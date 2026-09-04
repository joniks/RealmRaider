using NUnit.Framework;
using RealmRaiders.UI;
using UnityEditor;

namespace RealmRaiders.Tests
{
    public sealed class ProjectConfigurationTests
    {
        [Test]
        public void BuildStartsAtPrototypeHub()
        {
            Assert.That(EditorBuildSettings.scenes, Is.Not.Empty);
            Assert.That(EditorBuildSettings.scenes[0].path, Is.EqualTo("Assets/Game/Scenes/PrototypeHub.unity"));
            Assert.That(EditorBuildSettings.scenes[0].enabled, Is.True);
        }

        [Test]
        public void DefenseHudConfigsUseRealmSpecificNames()
        {
            Assert.That(DefenseHudConfig.Sylvan.DefenderName, Is.EqualTo("Ent"));
            Assert.That(DefenseHudConfig.Sylvan.CoreName, Is.EqualTo("Heart Tree"));
            Assert.That(DefenseHudConfig.Infernal.DefenderName, Is.EqualTo("Brute"));
            Assert.That(DefenseHudConfig.Infernal.CoreName, Is.EqualTo("Infernal Heart"));
            Assert.That(DefenseHudConfig.Infernal.RetryScene, Is.EqualTo("InfernalRealm"));
        }
    }
}
