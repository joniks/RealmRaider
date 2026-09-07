using NUnit.Framework;
using RealmRaiders.CameraSystem;
using RealmRaiders.UI;
using UnityEditor;
using UnityEngine;

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
            Assert.That(DefenseHudConfig.Sylvan.NextActionScene, Is.EqualTo("RealmBuild"));
            Assert.That(DefenseHudConfig.Infernal.DefenderName, Is.EqualTo("Brute"));
            Assert.That(DefenseHudConfig.Infernal.CoreName, Is.EqualTo("Infernal Heart"));
            Assert.That(DefenseHudConfig.Infernal.RetryScene, Is.EqualTo("InfernalRealm"));
            Assert.That(DefenseHudConfig.Sylvan.OpeningRouteStatus, Does.Contain("ROOT GATE"));
            Assert.That(DefenseHudConfig.Sylvan.RouteStatus(1), Does.Contain("ROOT GATE"));
            Assert.That(DefenseHudConfig.Sylvan.RouteStatus(2), Does.Contain("GUARD LINE"));
            Assert.That(DefenseHudConfig.Sylvan.RouteStatus(3), Does.Contain("INNER ROOT").And.Contain("HEART GUARD"));
            Assert.That(DefenseHudConfig.Sylvan.RouteStatus(4), Does.Contain("HEART TREE"));
            Assert.That(DefenseHudConfig.Infernal.RouteStatus(1), Does.Contain("FLAME TRAP LINE"));
            Assert.That(DefenseHudConfig.Infernal.RouteStatus(2), Does.Contain("HOUND LINE").And.Contain("LAVA GATE"));
            Assert.That(DefenseHudConfig.Infernal.RouteStatus(3), Does.Contain("LAVA GATE").And.Contain("BRUTE GUARD"));
        }

        [Test]
        public void ThreatPlateAnchorRemainsInsidePortraitAndLandscapeSafeAreas()
        {
            var portrait = CombatCameraAwareness.TargetPlateAnchorFor(new Vector3(1.2f, -.1f, 1), new Rect(0, 0, 1080, 1920), new Vector2(1080, 1920));
            Assert.That(portrait.x, Is.InRange(.17f, .83f));
            Assert.That(portrait.y, Is.InRange(.06f, .94f));

            var landscapeSafe = new Rect(80, 20, 1760, 1040);
            var landscape = CombatCameraAwareness.TargetPlateAnchorFor(new Vector3(-.2f, 1.2f, 1), landscapeSafe, new Vector2(1920, 1080));
            Assert.That(landscape.x, Is.InRange(landscapeSafe.xMin / 1920f + .17f, landscapeSafe.xMax / 1920f - .17f));
            Assert.That(landscape.y, Is.InRange(landscapeSafe.yMin / 1080f + .06f, landscapeSafe.yMax / 1080f - .06f));
        }
    }
}
