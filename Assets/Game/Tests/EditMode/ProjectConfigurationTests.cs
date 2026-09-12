using NUnit.Framework;
using RealmRaiders.CameraSystem;
using RealmRaiders.Core;
using RealmRaiders.Raid;
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

            Assert.That(CombatCameraAwareness.EdgeSizeFor(PrototypeOrientation.Portrait), Is.EqualTo(new Vector2(300, 80)));
            Assert.That(CombatCameraAwareness.EdgeSizeFor(PrototypeOrientation.Landscape), Is.EqualTo(new Vector2(260, 68)));
            Assert.That(CombatCameraAwareness.TargetPlateSizeFor(PrototypeOrientation.Portrait), Is.EqualTo(new Vector2(380, 64)));
            Assert.That(CombatCameraAwareness.TargetPlateSizeFor(PrototypeOrientation.Landscape), Is.EqualTo(new Vector2(340, 58)));

            var portraitSize = CombatCameraAwareness.TargetPlateSizeFor(PrototypeOrientation.Portrait);
            var portraitPosition = CombatCameraAwareness.TargetPlatePositionFor(new Vector3(1.2f, -.1f, 1), new Rect(0, 80, 1080, 1760), new Vector2(1080, 1920), new Vector2(1080, 1920), portraitSize);
            Assert.That(portraitPosition.x - portraitSize.x * .5f, Is.GreaterThanOrEqualTo(20));
            Assert.That(portraitPosition.x + portraitSize.x * .5f, Is.LessThanOrEqualTo(1060));
            Assert.That(portraitPosition.y, Is.GreaterThanOrEqualTo(20));
            Assert.That(portraitPosition.y + portraitSize.y, Is.LessThanOrEqualTo(1900));

            var landscapeSize = CombatCameraAwareness.TargetPlateSizeFor(PrototypeOrientation.Landscape);
            var landscapePosition = CombatCameraAwareness.TargetPlatePositionFor(new Vector3(-.2f, 1.2f, 1), landscapeSafe, new Vector2(1920, 1080), new Vector2(1920, 1080), landscapeSize);
            Assert.That(landscapePosition.x - landscapeSize.x * .5f, Is.GreaterThanOrEqualTo(20));
            Assert.That(landscapePosition.x + landscapeSize.x * .5f, Is.LessThanOrEqualTo(1900));
            Assert.That(landscapePosition.y, Is.GreaterThanOrEqualTo(20));
            Assert.That(landscapePosition.y + landscapeSize.y, Is.LessThanOrEqualTo(1060));
        }

        [Test]
        public void ThreatEdgeUsesSixAndNinePercentHysteresisAndMapsBehindCamera()
        {
            Assert.That(CombatCameraAwareness.ShouldUseEdge(new Vector3(.05f, .5f, 1), false), Is.True);
            Assert.That(CombatCameraAwareness.ShouldUseEdge(new Vector3(.07f, .5f, 1), false), Is.False);
            Assert.That(CombatCameraAwareness.ShouldUseEdge(new Vector3(.07f, .5f, 1), true), Is.True);
            Assert.That(CombatCameraAwareness.ShouldUseEdge(new Vector3(.1f, .5f, 1), true), Is.False);
            Assert.That(CombatCameraAwareness.ShouldUseEdge(new Vector3(.5f, .5f, -1), false), Is.True);
            Assert.That(CombatCameraAwareness.IndicatorDirectionFor(new Vector3(.5f, .5f, -1), Vector3.right, Vector3.left), Is.EqualTo(-1));
            Assert.That(CombatCameraAwareness.IndicatorDirectionFor(new Vector3(.5f, .5f, -1), Vector3.right, Vector3.right), Is.EqualTo(1));
        }

        [Test]
        public void RaidResultCopyRetainsActualValuesAndKeepsLoopRoutesHonest()
        {
            var victory = new RaidResult(true, 115, 2, 4, 3, 46.8f, true);
            var defeat = new RaidResult(false, 25, 0, 1, 2, 18.2f, false);

            Assert.That(RaidHUD.ResultCopy(victory), Does.Contain("The Heart Tree fell").And.Contain("115").And.Contain("2").And.Contain("4").And.Contain("3").And.Contain("47s").And.Contain("yes"));
            Assert.That(RaidHUD.ResultCopy(defeat), Does.Contain("Revise the next defense").And.Contain("25").And.Contain("0").And.Contain("1").And.Contain("2").And.Contain("18s").And.Contain("no"));
            Assert.That(RaidHUD.ResultActionDestination(RaidHUD.PlanNextDefenseAction), Is.EqualTo("RealmBuild"));
            Assert.That(RaidHUD.ResultActionDestination("RAID AGAIN"), Is.EqualTo("SylvanRealm"));
            Assert.That(RaidHUD.ResultActionDestination("MY REALM"), Is.EqualTo("PrototypeHub"));
        }

        [Test]
        public void InfernalEntRaidConfigUsesExactRecipeAndStandaloneRoutes()
        {
            InfernalRaidBootstrap.ValidateSelectedRecipeForTests();
            var config = RaidHudConfig.InfernalEnt;
            Assert.That(config.RealmIdentity, Is.EqualTo(HudPresentation.InfernalRealmIdentity));
            Assert.That(config.HeroName, Is.EqualTo("Guardian Ent"));
            Assert.That(config.AbilityLabel(0), Is.EqualTo("SMASH"));
            Assert.That(config.AbilityLabel(1), Is.EqualTo("CHARGE"));
            Assert.That(config.AbilityLabel(2), Is.EqualTo("GROUND SLAM"));
            Assert.That(config.SupportsJourney, Is.False); Assert.That(config.ShowPlanNextDefense, Is.False);
            Assert.That(RaidHUD.ResultActionDestination("RAID AGAIN", config), Is.EqualTo("InfernalRaid"));
            Assert.That(RaidHUD.ResultActionDestination("MY REALM", config), Is.EqualTo("PrototypeHub"));
            Assert.That(RaidHUD.ResultCopy(new RaidResult(true, 130, 1, 3, 0, 80, true), config), Does.Contain("The Infernal Heart fell"));
            Assert.That(System.Array.Exists(EditorBuildSettings.scenes, scene => scene.enabled && scene.path == "Assets/Game/Scenes/InfernalRaid.unity"), Is.True);
        }
    }
}
