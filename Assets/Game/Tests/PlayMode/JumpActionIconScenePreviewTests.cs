using System.Collections;
using System.Linq;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class JumpActionIconScenePreviewTests
    {
        [UnityTearDown]
        public IEnumerator UnloadPreviewScene()
        {
            var active = SceneManager.GetActiveScene();
            if (!active.IsValid() || active.name != "SylvanRealm") yield break;
            var cleanup = SceneManager.CreateScene("Jump Icon Preview Cleanup");
            SceneManager.SetActiveScene(cleanup);
            yield return SceneManager.UnloadSceneAsync(active);
        }

        [UnityTest]
        public IEnumerator RaidJumpPreviewFollowsExistingJoystickVisibilityAndKeepsStateLabels()
        {
            var previousStyle = PrototypeSave.ControlStylePreference;
            GameplayInput.ResetForTests();
            try
            {
                PrototypeSave.SetControlStyle("Joystick");
                SceneManager.LoadScene("SylvanRealm");
                yield return null;
                yield return null;

                var hud = Object.FindAnyObjectByType<RaidHUD>();
                var hero = GameObject.Find("Blood Knight").GetComponent<CombatEntity>();
                var button = hud.JumpButtonRect.GetComponent<Button>();
                var icon = button.transform.Find(HudPresentation.JumpIconName)?.GetComponent<Image>();
                Assert.That(icon, Is.Not.Null);
                Assert.That(icon.raycastTarget, Is.False);
                Assert.That(hud.GetComponentsInChildren<Image>(true).Count(image => image.name == HudPresentation.JumpIconName), Is.EqualTo(1));
                Assert.That(hud.JumpButtonVisible, Is.True);
                Assert.That(icon.gameObject.activeInHierarchy, Is.True);
                Assert.That(hud.JumpButtonText, Does.StartWith("JUMP"));

                PrototypeSave.SetControlStyle("Fingertap");
                hud.SendMessage("RefreshJumpButton", SendMessageOptions.RequireReceiver);
                Assert.That(hud.JumpButtonVisible, Is.False);
                Assert.That(icon.gameObject.activeInHierarchy, Is.False);

                PrototypeSave.SetControlStyle("Joystick");
                hud.SendMessage("RefreshJumpButton", SendMessageOptions.RequireReceiver);
                Assert.That(hud.JumpButtonVisible, Is.True);
                Assert.That(button.transform.Cast<Transform>().Count(child => child.name == HudPresentation.JumpIconName), Is.EqualTo(1));
                hero.ApplyRoot(1);
                hud.SendMessage("RefreshJumpButton", SendMessageOptions.RequireReceiver);
                Assert.That(hud.JumpButtonText, Is.EqualTo("JUMP — ROOTED"));
                Assert.That(icon.gameObject.activeInHierarchy, Is.True);
                hero.BreakRoot();
            }
            finally
            {
                PrototypeSave.SetControlStyle(previousStyle);
                GameplayInput.ResetForTests();
            }
        }
    }
}
