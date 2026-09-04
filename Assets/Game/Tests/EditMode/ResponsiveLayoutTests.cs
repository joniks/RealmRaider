using NUnit.Framework;
using RealmRaiders.UI;
using RealmRaiders.Core;
using RealmRaiders.Controllers;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class ResponsiveLayoutTests
    {
        [Test] public void ClassifiesPortraitAndLandscape() { Assert.That(ResponsiveLayout.Classify(new Vector2(1080, 1920)), Is.EqualTo(PrototypeOrientation.Portrait)); Assert.That(ResponsiveLayout.Classify(new Vector2(1920, 1080)), Is.EqualTo(PrototypeOrientation.Landscape)); }
        [Test] public void JoystickOutputHonorsDeadZoneAndClamp() { Assert.That(ResponsiveLayout.NormalizeJoystick(new Vector2(.1f, 0), .2f), Is.EqualTo(Vector2.zero)); var output = ResponsiveLayout.NormalizeJoystick(new Vector2(4, 0), .1f); Assert.That(output.magnitude, Is.EqualTo(1).Within(.0001f)); GameplayInput.SetMovement(new Vector2(.35f, 0)); Assert.That(GameplayInput.Movement.x, Is.EqualTo(.35f).Within(.001f)); GameplayInput.SetMovement(new Vector2(2, 0)); Assert.That(GameplayInput.Movement.magnitude, Is.EqualTo(1).Within(.001f)); GameplayInput.ClearMovement(); }
        [Test] public void SafeAreaConversionUsesUnitRectangleForFullScreen() { var area = ResponsiveLayout.NormalizeSafeArea(new Rect(0, 0, 1920, 1080), new Vector2(1920, 1080)); Assert.That(area, Is.EqualTo(new Rect(0, 0, 1, 1))); }
        [Test] public void ProjectOrientationAllowsBothLandscapes() { var text = System.IO.File.ReadAllText("ProjectSettings/ProjectSettings.asset"); Assert.That(text, Does.Contain("defaultScreenOrientation: 4")); Assert.That(text, Does.Contain("allowedAutorotateToPortraitUpsideDown: 0")); Assert.That(text, Does.Contain("allowedAutorotateToLandscapeLeft: 1")); Assert.That(text, Does.Contain("allowedAutorotateToLandscapeRight: 1")); }
        [Test] public void InvalidOrientationPreferenceFallsBackToAuto() { var previous = PrototypeSave.OrientationPreference; try { PrototypeSave.SetOrientation("invalid"); Assert.That(PrototypeSave.OrientationPreference, Is.EqualTo("Auto")); } finally { PrototypeSave.SetOrientation(previous); } }
        [Test] public void ControlStyleFallsBackAndSelectsByOrientation() { var previous = PrototypeSave.ControlStylePreference; try { GameplayInput.SetMovement(new Vector2(.5f, 0)); var before = GameplayInput.InteractionRevision; PrototypeSave.SetControlStyle("invalid"); Assert.That(PrototypeSave.ControlStylePreference, Is.EqualTo("Contextual")); Assert.That(GameplayInput.InteractionRevision, Is.GreaterThan(before)); Assert.That(GameplayInput.Movement, Is.EqualTo(Vector2.zero)); PrototypeSave.SetControlStyle("Contextual"); Assert.That(PrototypeSave.EffectiveControlStyle(false), Is.EqualTo("Fingertap")); Assert.That(PrototypeSave.EffectiveControlStyle(true), Is.EqualTo("Joystick")); } finally { PrototypeSave.SetControlStyle(previous); } }
        [Test] public void UiPointerOwnershipIsPointerSpecific() { GameplayInput.ClearPointers(); GameplayInput.ClaimUiPointer(3); GameplayInput.ClaimUiPointer(4); Assert.That(GameplayInput.IsUiOwned(3), Is.True); GameplayInput.ReleaseUiPointer(3); Assert.That(GameplayInput.IsUiOwned(3), Is.False); Assert.That(GameplayInput.IsUiOwned(4), Is.True); GameplayInput.ClearPointers(); }
    }
}
