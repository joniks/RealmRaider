using NUnit.Framework;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.UI;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class InRunControlStyleSelectorTests
    {
        const string PreferenceKey = "realmraiders.controlStyle.v1";

        [Test]
        public void CycleOrderIsExactAndWrapsToContextual()
        {
            Assert.That(InRunControlStyleSelector.NextPreference(InRunControlStyleSelector.Contextual), Is.EqualTo(InRunControlStyleSelector.Fingertap));
            Assert.That(InRunControlStyleSelector.NextPreference(InRunControlStyleSelector.Fingertap), Is.EqualTo(InRunControlStyleSelector.Joystick));
            Assert.That(InRunControlStyleSelector.NextPreference(InRunControlStyleSelector.Joystick), Is.EqualTo(InRunControlStyleSelector.Contextual));
        }

        [Test]
        public void PreferenceMappingIsOrdinalAndCopyIsFactual()
        {
            Assert.That(InRunControlStyleSelector.NormalizePreference("contextual"), Is.EqualTo(InRunControlStyleSelector.Contextual));
            Assert.That(InRunControlStyleSelector.NormalizePreference("JOYSTICK"), Is.EqualTo(InRunControlStyleSelector.Contextual));
            Assert.That(InRunControlStyleSelector.NextPreference("contextual"), Is.EqualTo(InRunControlStyleSelector.Contextual));
            Assert.That(InRunControlStyleSelector.NextPreference("Fingertap "), Is.EqualTo(InRunControlStyleSelector.Contextual));
            Assert.That(InRunControlStyleSelector.CopyFor(InRunControlStyleSelector.Contextual), Is.EqualTo("CONTROL: AUTO"));
            Assert.That(InRunControlStyleSelector.CopyFor(InRunControlStyleSelector.Fingertap), Is.EqualTo("CONTROL: TAP"));
            Assert.That(InRunControlStyleSelector.CopyFor(InRunControlStyleSelector.Joystick), Is.EqualTo("CONTROL: STICK"));
            Assert.That(InRunControlStyleSelector.EffectiveStyleFor(InRunControlStyleSelector.Contextual, false), Is.EqualTo(InRunControlStyleSelector.Fingertap));
            Assert.That(InRunControlStyleSelector.EffectiveStyleFor(InRunControlStyleSelector.Contextual, true), Is.EqualTo(InRunControlStyleSelector.Joystick));
        }

        [Test]
        public void ApplyingCyclePersistsAndResetsAllTransientInput()
        {
            var hadPreference = PlayerPrefs.HasKey(PreferenceKey);
            var previousPreference = PlayerPrefs.GetString(PreferenceKey, InRunControlStyleSelector.Contextual);
            try
            {
                GameplayInput.ResetForTests();
                PrototypeSave.SetControlStyle(InRunControlStyleSelector.Contextual);
                GameplayInput.SetMovement(new Vector2(.8f, .4f));
                GameplayInput.ClaimUiPointer(1100);
                var revision = GameplayInput.InteractionRevision;

                PrototypeSave.SetControlStyle(InRunControlStyleSelector.NextPreference(PrototypeSave.ControlStylePreference));

                Assert.That(PrototypeSave.ControlStylePreference, Is.EqualTo(InRunControlStyleSelector.Fingertap));
                Assert.That(PlayerPrefs.GetString(PreferenceKey), Is.EqualTo(InRunControlStyleSelector.Fingertap));
                Assert.That(GameplayInput.Movement, Is.EqualTo(Vector2.zero));
                Assert.That(GameplayInput.HasUiOwnership, Is.False);
                Assert.That(GameplayInput.InteractionRevision, Is.GreaterThan(revision));
            }
            finally
            {
                PrototypeSave.SetControlStyle(previousPreference);
                if (!hadPreference) { PlayerPrefs.DeleteKey(PreferenceKey); PlayerPrefs.Save(); }
                GameplayInput.ResetForTests();
            }
        }
    }
}
