using System.Collections;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.Raid;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class PrototypeJourneyFlowTests
    {
        [UnityTest]
        public IEnumerator PrimaryJourneyRunsBuildRaidDefenseBuildAndCarriesFirstMinuteHandoff()
        {
            var saved = new SavedState();
            try
            {
                PrepareFreshState();
                SceneManager.LoadScene("PrototypeHub"); yield return null; yield return null;
                Assert.That(GameObject.Find("Label " + HubHUD.JourneyExplanation).GetComponent<Text>().text, Is.EqualTo("1. BUILD DEFENCES  →  2. RAID THE ENEMY  →  3. DEFEND YOUR REALM"));
                AssertSceneSingletons();
                GameObject.Find(HubHUD.SylvanRaidVariantAction).GetComponent<Button>().onClick.Invoke();
                Assert.That(Object.FindFirstObjectByType<HubHUD>().SylvanRaidVariantText, Is.EqualTo("NEXT SYLVAN RAID: WOLF PRESSURE — TAP TO CHANGE"));

                GameObject.Find("START SYLVAN JOURNEY").GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("RealmBuild"));
                Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Build));
                var build = Object.FindFirstObjectByType<BuildHUD>();
                Assert.That(build.SaveActionText, Is.EqualTo(BuildHUD.SaveAndRaidAction));
                Assert.That(GameObject.Find(BuildHUD.SaveAndRaidAction), Is.Not.Null);
                AssertActionResponsive(build.GetComponent<ResponsiveHudRoot>(), GameObject.Find(BuildHUD.SaveAndRaidAction).GetComponent<RectTransform>());

                build.CycleSlotForTests(2);
                build.CycleSlotForTests(1);
                Assert.That(build.GuideStep, Is.EqualTo(BuildGuideStep.Save));
                Assert.That(build.GuideText, Is.EqualTo("PLAN READY — SAVE & RAID"));
                GameObject.Find(BuildHUD.SaveAndRaidAction).GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;

                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("SylvanRealm"));
                Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Raid));
                Assert.That(SylvanRaidCompositionSelection.DisplayName, Is.EqualTo("Wolf Pressure"));
                Assert.That(Object.FindFirstObjectByType<RaidHUD>().StateText, Does.StartWith("SYLVAN RAID • WOLF PRESSURE"));
                Assert.That(Object.FindObjectsByType<CombatEntity>(FindObjectsInactive.Include, FindObjectsSortMode.None), Has.Length.EqualTo(5));
                Assert.That(FirstPlayableMinute.ChangedBuildAcceptedForSession, Is.True, "Raid must not consume the changed-BUILD handoff.");
                Assert.That(Object.FindFirstObjectByType<FirstPlayableMinuteDefenseGuide>(), Is.Null);
                var raid = Object.FindFirstObjectByType<RaidManager>();
                raid.BeginObjective();
                raid.CompleteObjective();
                yield return new WaitForSeconds(1.35f); yield return null;

                var raidHud = Object.FindFirstObjectByType<RaidHUD>();
                Assert.That(raid.State, Is.EqualTo(RaidState.RaidResult));
                Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.RaidResult));
                Assert.That(raidHud.ResultPrimaryActionText, Is.EqualTo(RaidHUD.DefendYourRealmAction));
                Assert.That(raidHud.ResultText, Does.StartWith("VICTORY\n\nThe Heart Tree fell. Your built Realm is under attack — defend it now."));
                Assert.That(raidHud.ResultText, Does.Not.Contain("plan the next defense"));
                Assert.That(raidHud.ResultText, Does.Contain("Gold collected:").And.Contain("Secured for your Realm:"));
                Assert.That(FirstPlayableMinute.ChangedBuildAcceptedForSession, Is.True);
                var earnedProgress = RealmProgress.Load();
                Assert.That(earnedProgress.CompletedRaids, Is.EqualTo(1));
                Assert.That(GuardianEntCultivationAffordance.From(earnedProgress).Status, Is.EqualTo(GuardianEntCultivationStatus.Ready));
                AssertResultActionsResponsive(raidHud.GetComponent<ResponsiveHudRoot>(), "Raid Result", RaidHUD.DefendYourRealmAction, "RAID AGAIN", "MY REALM");

                GameObject.Find(RaidHUD.DefendYourRealmAction).GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("DefenderTest"));
                Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Defense));
                Assert.That(FirstPlayableMinute.ChangedBuildAcceptedForSession, Is.False);
                var defenderHud = Object.FindFirstObjectByType<DefenderHUD>();
                Assert.That(defenderHud.FirstMinuteGuide, Is.Not.Null, "The changed-BUILD guide begins only after the raid reaches Sylvan defense.");
                AssertSceneSingletons();

                GameObject.Find("Invading Blood Knight").GetComponent<CombatEntity>().Health.TakeDamage(
                    new DamageInfo(10000, null, Vector3.zero), 0);
                yield return null;
                Assert.That(Object.FindFirstObjectByType<DefenseManager>().State, Is.EqualTo(DefenseState.DefenderVictory));
                Assert.That(defenderHud.JourneyCompletedForResult, Is.True);
                Assert.That(defenderHud.ResultPrimaryActionText, Is.EqualTo("RETURN TO BUILD"));
                Assert.That(defenderHud.ResultText, Does.Contain(DefenderHUD.CultivationReadyResultCopy));
                Assert.That(Occurrences(defenderHud.ResultText, DefenderHUD.CultivationReadyResultCopy), Is.EqualTo(1));
                Assert.That(defenderHud.ResultText, Does.EndWith("TRY THE CONTROL LOOP — DEFEND AGAIN"), "The cultivation handoff must compose before the existing first-minute result suffix.");
                Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Inactive));
                Assert.That(FirstPlayableMinute.Load(), Is.EqualTo(FirstPlayableMinuteStatus.Active), "An early guide result may offer its existing retry but cannot be falsely completed.");
                AssertResultActionsResponsive(defenderHud.GetComponent<ResponsiveHudRoot>(), "Defense Result", "DEFEND AGAIN", "RETURN TO BUILD", "MY REALM");
                AssertResultCopyResponsive(defenderHud.GetComponent<ResponsiveHudRoot>(), defenderHud.ResultRect, "Defense Result", "DEFEND AGAIN", "RETURN TO BUILD", "MY REALM");

                var frozenResultCopy = defenderHud.ResultText;
                var earnedProgressJson = PlayerPrefs.GetString(RealmProgress.KeyForTests);
                try
                {
                    RealmProgress.ResetForTests();
                    defenderHud.SendMessage("OnDefenseState", DefenseState.DefenderVictory, SendMessageOptions.RequireReceiver);
                    Assert.That(defenderHud.ResultText, Is.EqualTo(frozenResultCopy), "Later callbacks and progress changes cannot rewrite the frozen result.");
                    Assert.That(Occurrences(defenderHud.ResultText, DefenderHUD.CultivationReadyResultCopy), Is.EqualTo(1));
                }
                finally
                {
                    PlayerPrefs.SetString(RealmProgress.KeyForTests, earnedProgressJson);
                    PlayerPrefs.Save();
                }

                GameObject.Find("RETURN TO BUILD").GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("RealmBuild"));
                Assert.That(PrototypeJourney.IsActive, Is.False);
                var returnedBuild = Object.FindFirstObjectByType<BuildHUD>();
                Assert.That(returnedBuild.SaveActionText, Is.EqualTo(BuildHUD.SaveAndDefendAction), "The completed journey cannot leak into the next BUILD entry.");
                Assert.That(returnedBuild.GuardianEntUpgradeStatus, Is.EqualTo(GuardianEntCultivationStatus.Ready));
                Assert.That(returnedBuild.GuardianEntUpgradeInteractable, Is.True);
                var beforePurchase = RealmProgress.Load();
                Assert.That(beforePurchase.CompletedRaids, Is.EqualTo(1), "Defense and route transitions cannot duplicate raid credit.");
                Assert.That(beforePurchase.Gold, Is.EqualTo(earnedProgress.Gold));
                Assert.That(beforePurchase.RareMaterials, Is.EqualTo(earnedProgress.RareMaterials));
                Assert.That(beforePurchase.GuardianEntVitalityRank, Is.EqualTo(earnedProgress.GuardianEntVitalityRank), "The handoff must not auto-purchase cultivation.");

                Assert.That(returnedBuild.PurchaseGuardianEntVitalityForTests(), Is.True, "Cultivation remains an explicit Build action.");
                var afterPurchase = RealmProgress.Load();
                Assert.That(afterPurchase.Gold, Is.EqualTo(beforePurchase.Gold - RealmProgress.GuardianEntVitalityGoldCost));
                Assert.That(afterPurchase.RareMaterials, Is.EqualTo(beforePurchase.RareMaterials - RealmProgress.GuardianEntVitalityRareMaterialCost));
                Assert.That(afterPurchase.GuardianEntVitalityRank, Is.EqualTo(beforePurchase.GuardianEntVitalityRank + 1));
            }
            finally { saved.Restore(); }
        }

        [UnityTest]
        public IEnumerator RealRaidDefeatMayContinueToDefenseAndUnexpectedHubEntryCancels()
        {
            var saved = new SavedState();
            try
            {
                PrepareFreshState();
                StartAtRaid();
                SceneManager.LoadScene("SylvanRealm"); yield return null; yield return null;
                var hero = GameObject.Find("Blood Knight").GetComponent<CombatEntity>();
                hero.Health.TakeDamage(new DamageInfo(10000, null, hero.transform.position), 0);
                yield return new WaitForSeconds(1.35f); yield return null;

                var hud = Object.FindFirstObjectByType<RaidHUD>();
                Assert.That(Object.FindFirstObjectByType<RaidManager>().State, Is.EqualTo(RaidState.RaidResult));
                Assert.That(hud.ResultText, Does.StartWith("DEFEAT\n\nYour Realm still needs defense — defend it now or retry this raid."));
                Assert.That(hud.ResultText, Does.Not.Contain("Revise the next defense"));
                Assert.That(hud.ResultText, Does.Contain("Gold collected:").And.Contain("Secured for your Realm:"));
                Assert.That(hud.ResultPrimaryActionText, Is.EqualTo(RaidHUD.DefendYourRealmAction));
                Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.RaidResult));
                GameObject.Find(RaidHUD.DefendYourRealmAction).GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("DefenderTest"));
                Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Defense));
                Assert.That(Object.FindFirstObjectByType<FirstPlayableMinuteDefenseGuide>(), Is.Null);

                SceneManager.LoadScene("PrototypeHub"); yield return null; yield return null;
                Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Inactive));
            }
            finally { saved.Restore(); }
        }

        [UnityTest]
        public IEnumerator JourneyRaidRetryIsExplicitAndHubResultActionCancelsWithoutDuplicateCredit()
        {
            var saved = new SavedState();
            try
            {
                PrepareFreshState();
                StartAtRaid();
                SceneManager.LoadScene("SylvanRealm"); yield return null; yield return null;
                var firstHud = Object.FindFirstObjectByType<RaidHUD>();
                var firstResult = new RaidResult(false, 20, 0, 0, 1, 12, false);
                firstHud.SendMessage("ShowResult", firstResult, SendMessageOptions.RequireReceiver); yield return null;
                var firstCopy = firstHud.ResultText;
                Assert.That(firstCopy, Does.StartWith("DEFEAT\n\nYour Realm still needs defense — defend it now or retry this raid."));
                firstHud.SendMessage("ShowResult", firstResult, SendMessageOptions.RequireReceiver); yield return null;
                Assert.That(firstHud.ResultText, Is.EqualTo(firstCopy), "Repeated result delivery must keep the journey copy identical.");
                Assert.That(RealmProgress.Load().CompletedRaids, Is.EqualTo(1));
                Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.RaidResult));

                GameObject.Find("RAID AGAIN").GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("SylvanRealm"));
                Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Raid));
                var retryHud = Object.FindFirstObjectByType<RaidHUD>();
                retryHud.SendMessage("ShowResult", firstResult, SendMessageOptions.RequireReceiver); yield return null;
                Assert.That(RealmProgress.Load().CompletedRaids, Is.EqualTo(2), "A retried scene is a new raid, while each raid result remains exact-once.");

                GameObject.Find("MY REALM").GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo("PrototypeHub"));
                Assert.That(PrototypeJourney.Stage, Is.EqualTo(PrototypeJourneyStage.Inactive));
            }
            finally { saved.Restore(); }
        }

        [UnityTest]
        public IEnumerator LegacyHubRoutesAndDirectRaidResultRemainIndependent()
        {
            var saved = new SavedState();
            try
            {
                PrepareFreshState();
                var routes = new[]
                {
                    new[] { "BUILD SYLVAN", "RealmBuild" },
                    new[] { "DEFEND SYLVAN", "DefenderTest" },
                    new[] { "RAID SYLVAN", "SylvanRealm" },
                    new[] { "RAID INFERNAL — ENT", "InfernalRaid" },
                    new[] { "DEFEND INFERNAL", "InfernalRealm" },
                    new[] { "CHARACTER SANDBOX", "CharacterSandbox" }
                };
                foreach (var route in routes)
                {
                    SceneManager.LoadScene("PrototypeHub"); yield return null; yield return null;
                    GameObject.Find(route[0]).GetComponent<Button>().onClick.Invoke();
                    yield return null; yield return null;
                    Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(route[1]), route[0]);
                    Assert.That(PrototypeJourney.IsActive, Is.False, route[0]);
                }

                SceneManager.LoadScene("SylvanRealm"); yield return null; yield return null;
                var directHud = Object.FindFirstObjectByType<RaidHUD>();
                directHud.SendMessage("ShowResult", new RaidResult(true, 10, 0, 0, 0, 5, true), SendMessageOptions.RequireReceiver); yield return null;
                Assert.That(directHud.ResultPrimaryActionText, Is.EqualTo(RaidHUD.PlanNextDefenseAction));
                Assert.That(directHud.ResultText, Does.StartWith("VICTORY\n\nThe Heart Tree fell. Return to your Realm and plan the next defense."));
                Assert.That(directHud.ResultText, Does.Not.Contain("Your built Realm is under attack"));
                GameObject.Find(RaidHUD.PlanNextDefenseAction).GetComponent<Button>().onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(RaidHUD.PlanNextDefenseScene));
                Assert.That(Object.FindFirstObjectByType<BuildHUD>().SaveActionText, Is.EqualTo(BuildHUD.SaveAndDefendAction));
                Assert.That(PrototypeJourney.IsActive, Is.False);
            }
            finally { saved.Restore(); }
        }

        static void StartAtRaid()
        {
            Assert.That(PrototypeJourney.TryStart(out var token), Is.True);
            Assert.That(PrototypeJourney.TryBeginRaid(token), Is.True);
        }

        static void PrepareFreshState()
        {
            Time.timeScale = 1;
            PrototypeJourney.ResetForTests();
            FirstPlayableMinute.ResetForTests();
            RealmProgress.ResetForTests();
            DefenseLayoutSave.Save(DefenseLayout.Default());
            PrototypeSave.SelectRealm("Sylvan");
            PrototypeSave.SetOrientation("Auto");
            PrototypeSave.SetControlStyle("Contextual");
            SylvanRaidCompositionSelection.ResetForTests();
            GameplayInput.ResetForTests();
        }

        static void AssertActionResponsive(ResponsiveHudRoot responsive, RectTransform action)
        {
            foreach (var orientation in new[] { PrototypeOrientation.Portrait, PrototypeOrientation.Landscape })
            {
                responsive.SetOrientationForTests(orientation);
                var reference = responsive.GetComponent<CanvasScaler>().referenceResolution;
                var bounds = DesignRect(action, reference);
                Assert.That(bounds.xMin, Is.GreaterThanOrEqualTo(0), action.name);
                Assert.That(bounds.yMin, Is.GreaterThanOrEqualTo(0), action.name);
                Assert.That(bounds.xMax, Is.LessThanOrEqualTo(reference.x), action.name);
                Assert.That(bounds.yMax, Is.LessThanOrEqualTo(reference.y), action.name);
            }
            AssertSceneSingletons();
        }

        static void AssertResultActionsResponsive(ResponsiveHudRoot responsive, string panelName, params string[] actionNames)
        {
            foreach (var orientation in new[] { PrototypeOrientation.Portrait, PrototypeOrientation.Landscape })
            {
                responsive.SetOrientationForTests(orientation);
                var panelTransform = GameObject.Find(panelName).GetComponent<RectTransform>();
                var reference = responsive.GetComponent<CanvasScaler>().referenceResolution;
                var referenceRect = new Rect(Vector2.zero, reference);
                var panel = DesignRect(panelTransform, reference);
                Assert.That(referenceRect.Contains(panel.min) && referenceRect.Contains(panel.max), Is.True,
                    $"{panelName} | orientation={orientation} | reference={referenceRect} | panel={panel}");
                var localPanel = new Rect(Vector2.zero, panel.size);
                var actions = new Rect[actionNames.Length];
                for (var index = 0; index < actionNames.Length; index++)
                {
                    var rect = GameObject.Find(actionNames[index]).GetComponent<RectTransform>();
                    actions[index] = DesignRect(rect, panel.size);
                    Assert.That(actions[index].xMin > localPanel.xMin && actions[index].yMin > localPanel.yMin &&
                        actions[index].xMax < localPanel.xMax && actions[index].yMax < localPanel.yMax, Is.True,
                        $"{actionNames[index]} | orientation={orientation} | panel={localPanel} | action={actions[index]}");
                }
                for (var first = 0; first < actions.Length; first++)
                    for (var second = first + 1; second < actions.Length; second++)
                        Assert.That(actions[first].Overlaps(actions[second]), Is.False, $"{actionNames[first]}/{actionNames[second]}");
            }
            AssertSceneSingletons();
        }

        static void AssertResultCopyResponsive(ResponsiveHudRoot responsive, RectTransform result, string panelName, params string[] actionNames)
        {
            foreach (var orientation in new[] { PrototypeOrientation.Portrait, PrototypeOrientation.Landscape })
            {
                responsive.SetOrientationForTests(orientation);
                var panelTransform = GameObject.Find(panelName).GetComponent<RectTransform>();
                var reference = responsive.GetComponent<CanvasScaler>().referenceResolution;
                var panel = DesignRect(panelTransform, reference);
                var localPanel = new Rect(Vector2.zero, panel.size);
                var resultBounds = DesignRect(result, panel.size);
                Assert.That(localPanel.Contains(resultBounds.min) && localPanel.Contains(resultBounds.max), Is.True,
                    $"{result.name} | orientation={orientation} | panel={localPanel} | result={resultBounds}");
                Canvas.ForceUpdateCanvases();
                var resultText = result.GetComponent<Text>();
                var settings = resultText.GetGenerationSettings(new Vector2(resultBounds.width, 0));
                var preferredHeight = resultText.cachedTextGeneratorForLayout.GetPreferredHeight(resultText.text, settings) / resultText.pixelsPerUnit;
                Assert.That(preferredHeight, Is.LessThanOrEqualTo(resultBounds.height + 1),
                    $"Result text is vertically truncated | orientation={orientation} | preferred={preferredHeight:0.##} | allocated={resultBounds.height:0.##} | font={resultText.fontSize}");
                foreach (var actionName in actionNames)
                {
                    var action = DesignRect(GameObject.Find(actionName).GetComponent<RectTransform>(), panel.size);
                    Assert.That(resultBounds.Overlaps(action), Is.False,
                        $"{result.name}/{actionName} | orientation={orientation} | result={resultBounds} | action={action}");
                }
            }
            AssertSceneSingletons();
        }

        static int Occurrences(string value, string fragment)
        {
            var count = 0;
            var index = 0;
            while ((index = value.IndexOf(fragment, index, System.StringComparison.Ordinal)) >= 0)
            {
                count++;
                index += fragment.Length;
            }
            return count;
        }

        static Rect DesignRect(RectTransform rect, Vector2 parentSize)
        {
            var anchorMin = Vector2.Scale(rect.anchorMin, parentSize);
            var anchorSize = Vector2.Scale(rect.anchorMax - rect.anchorMin, parentSize);
            var size = anchorSize + rect.sizeDelta;
            var pivotPoint = anchorMin + Vector2.Scale(anchorSize, rect.pivot) + rect.anchoredPosition;
            return new Rect(pivotPoint - Vector2.Scale(size, rect.pivot), size);
        }

        static void AssertSceneSingletons()
        {
            Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
            Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
        }

        sealed class SavedState
        {
            readonly bool hadGuide = PlayerPrefs.HasKey(FirstPlayableMinute.KeyForTests);
            readonly string guide = PlayerPrefs.GetString(FirstPlayableMinute.KeyForTests, string.Empty);
            readonly bool hadLayout = PlayerPrefs.HasKey(DefenseLayoutSave.KeyForTests);
            readonly string layout = PlayerPrefs.GetString(DefenseLayoutSave.KeyForTests, string.Empty);
            readonly bool hadProgress = PlayerPrefs.HasKey(RealmProgress.KeyForTests);
            readonly string progress = PlayerPrefs.GetString(RealmProgress.KeyForTests, string.Empty);
            readonly string realm = PrototypeSave.SelectedRealm;
            readonly string orientation = PrototypeSave.OrientationPreference;
            readonly string control = PrototypeSave.ControlStylePreference;

            public void Restore()
            {
                RestoreKey(FirstPlayableMinute.KeyForTests, hadGuide, guide);
                RestoreKey(DefenseLayoutSave.KeyForTests, hadLayout, layout);
                RestoreKey(RealmProgress.KeyForTests, hadProgress, progress);
                PlayerPrefs.Save();
                PrototypeSave.SelectRealm(realm);
                PrototypeSave.SetOrientation(orientation);
                PrototypeSave.SetControlStyle(control);
                PrototypeJourney.ResetForTests();
                SylvanRaidCompositionSelection.ResetForTests();
                FirstPlayableMinute.ResetBuildHandoff();
                GameplayInput.ResetForTests();
                Time.timeScale = 1;
            }

            static void RestoreKey(string key, bool hadValue, string value)
            {
                if (hadValue) PlayerPrefs.SetString(key, value); else PlayerPrefs.DeleteKey(key);
            }
        }
    }
}
