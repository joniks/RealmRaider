using System.Collections;
using System.Linq;
using NUnit.Framework;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.AI;
using RealmRaiders.Possession;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using RealmRaiders.Core;
using RealmRaiders.Traps;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace RealmRaiders.Tests
{
    public sealed class PossessionFlowTests
    {
        private static int cleanupSceneIndex;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            GameplayInput.SetTerminalState(false);
            var previous = SceneManager.GetActiveScene();
            if (!ShouldUnload(previous))
            {
                yield return null;
                yield break;
            }

            var cleanup = SceneManager.CreateScene($"PossessionFlowTests Setup {++cleanupSceneIndex}");
            SceneManager.SetActiveScene(cleanup);
            yield return SceneManager.UnloadSceneAsync(previous);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            GameplayInput.SetTerminalState(false);
            var testScene = SceneManager.GetActiveScene();
            if (!ShouldUnload(testScene))
            {
                yield return null;
                yield break;
            }

            var cleanup = SceneManager.CreateScene($"PossessionFlowTests Teardown {++cleanupSceneIndex}");
            SceneManager.SetActiveScene(cleanup);
            yield return SceneManager.UnloadSceneAsync(testScene);
        }

        private static bool ShouldUnload(Scene scene)
        {
            return scene.IsValid() && scene.isLoaded &&
                   (scene.buildIndex >= 0 || scene.name.StartsWith("PossessionFlowTests "));
        }

        [UnityTest]
        public IEnumerator HudPresentation_UsesOneSceneLocalSourceAndGuardsResultCue()
        {
            var hud = new GameObject("HUD Presentation Test");
            var listener = new GameObject("Test Audio Listener", typeof(AudioListener));
            try
            {
                var presentation = hud.AddComponent<HudPresentation>();
                yield return null;

                Assert.That(hud.GetComponents<HudPresentation>(), Has.Length.EqualTo(1));
                Assert.That(hud.GetComponents<AudioSource>(), Has.Length.EqualTo(1));
                Assert.That(hud.GetComponent<AudioListener>(), Is.Null);
                presentation.PlayResult(); presentation.PlayResult();
                Assert.That(presentation.ResultCuePlayed, Is.True);
            }
            finally { Object.Destroy(listener); Object.Destroy(hud); }
        }

        [UnityTest]
        public IEnumerator PossessionEnergyReadability_SharedDefenseHudClearsAcrossReturnPaths()
        {
            var hadGuide = PlayerPrefs.HasKey(FirstPlayableMinute.KeyForTests);
            var previousGuide = PlayerPrefs.GetString(FirstPlayableMinute.KeyForTests, string.Empty);
            var previousControlStyle = PrototypeSave.ControlStylePreference;
            FirstPlayableMinute.ResetForTests();
            GameplayInput.ResetForTests();
            PrototypeSave.SetControlStyle(InRunControlStyleSelector.Fingertap);
            try
            {
                var configs = new[] { DefenseHudConfig.Sylvan, DefenseHudConfig.Infernal };
                foreach (var config in configs)
                {
                    var fixture = EnergyHudFixture.Create(config);
                    GameObject[] trackedAbilityIcons = System.Array.Empty<GameObject>();
                    GameObject trackedChargeAffordance = null;
                    GameObject trackedRealmMark = null;
                    GameObject trackedEnergyMeter = null;
                    try
                    {
                        var guardian = config.RealmTitle == DefenseHudConfig.Sylvan.RealmTitle;
                        var familyPrefix = guardian ? HudPresentation.GuardianEntAbilityIconNamePrefix : HudPresentation.InfernalBruteAbilityIconNamePrefix;
                        var otherFamilyPrefix = guardian ? HudPresentation.InfernalBruteAbilityIconNamePrefix : HudPresentation.GuardianEntAbilityIconNamePrefix;
                        var responsive = fixture.Hud.GetComponent<ResponsiveHudRoot>();
                        responsive.SetOrientationForTests(PrototypeOrientation.Portrait);
                        fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                        trackedAbilityIcons = AbilityIcons(fixture.Hud.transform, true, familyPrefix).Select(image => image.gameObject).ToArray();
                        trackedChargeAffordance = fixture.Hud.ChargeAffordanceRect ? fixture.Hud.ChargeAffordanceRect.gameObject : null;
                        trackedRealmMark = AssertDefenseRealmIdentityPresentation(fixture.Hud, guardian, "KEEPER OVERVIEW").gameObject;
                        Assert.That(trackedAbilityIcons, Has.Length.EqualTo(3));
                        Assert.That(AbilityIcons(fixture.Hud.transform, true, otherFamilyPrefix), Is.Empty, "A defender must not receive the other realm family's icons.");
                        Assert.That(fixture.Hud.ChargeAffordanceVisible, Is.False, "Keeper view must not show a direct-control gesture affordance.");
                        Assert.That(fixture.Hud.ChargeAffordanceRect, Is.Not.Null);
                        Assert.That(fixture.Hud.PossessionEnergyLevel, Is.EqualTo(PossessionEnergyReadabilityLevel.Normal));
                        Assert.That(fixture.Hud.PossessionEnergyText, Is.EqualTo("Possession energy  30.0/30s"));
                        Assert.That(CountNamed(fixture.Hud.transform, "Possession Energy Meter"), Is.EqualTo(1));
                        var meter = fixture.Hud.PossessionEnergyMeterRect;
                        trackedEnergyMeter = meter.gameObject;
                        var meterAnchorMin = meter.anchorMin;
                        var meterAnchorMax = meter.anchorMax;
                        var meterPivot = meter.pivot;
                        var meterPosition = meter.anchoredPosition;
                        var meterSize = meter.sizeDelta;
                        Assert.That(meter.childCount, Is.EqualTo(1));
                        Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one));

                        fixture.Possession.Select(fixture.Defender);
                        Assert.That(fixture.Possession.PossessSelected(), Is.True);
                        fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                        Assert.That(fixture.Hud.PossessionEnergyLevel, Is.EqualTo(PossessionEnergyReadabilityLevel.Normal));
                        Assert.That(fixture.Hud.AbilityButtonText(0), Is.EqualTo("SMASH"));
                        Assert.That(fixture.Hud.AbilityButtonText(1), Is.EqualTo("GROUND SLAM"));
                        Assert.That(fixture.Hud.transform.Find("CHARGE"), Is.Null, "Charge remains a world-swipe gesture, not a new button.");
                        AssertThemedAbilityPresentation(fixture.Hud, PrototypeOrientation.Portrait, true, guardian);
                        var player = fixture.Defender.Controller<PlayerController>();
                        var smashReadyAt = fixture.Defender.Abilities[0].ReadyAt;
                        fixture.Hud.transform.Find("SMASH").GetComponent<Button>().onClick.Invoke();
                        fixture.Hud.SendMessage("RefreshAbilityButtons", SendMessageOptions.RequireReceiver);
                        Assert.That(fixture.Defender.Abilities[0].ReadyAt, Is.GreaterThan(smashReadyAt));
                        Assert.That(fixture.Hud.AbilityButtonText(0), Is.EqualTo("SMASH — ACTING"));
                        yield return WaitForActionPhase(fixture.Defender, CombatActionPhase.Recovery);
                        fixture.Hud.SendMessage("RefreshAbilityButtons", SendMessageOptions.RequireReceiver);
                        Assert.That(fixture.Hud.AbilityButtonText(0), Does.StartWith("SMASH  "));
                        Assert.That(fixture.Hud.AbilityButtonText(1), Is.EqualTo("GROUND SLAM — NEXT"));

                        var slamReadyAt = fixture.Defender.Abilities[2].ReadyAt;
                        fixture.Hud.transform.Find("GROUND SLAM").GetComponent<Button>().onClick.Invoke();
                        fixture.Hud.SendMessage("RefreshAbilityButtons", SendMessageOptions.RequireReceiver);
                        Assert.That(player.IsAbilityBuffered(2), Is.True);
                        Assert.That(fixture.Defender.Abilities[2].ReadyAt, Is.EqualTo(slamReadyAt), "Queueing cannot consume the cooldown before execution.");
                        Assert.That(fixture.Hud.AbilityButtonText(1), Is.EqualTo("GROUND SLAM — QUEUED"));
                        yield return WaitForAbilityConsumption(fixture.Defender.Abilities[2], slamReadyAt);
                        yield return WaitForActionPhase(fixture.Defender, CombatActionPhase.Idle);

                        responsive.SetOrientationForTests(PrototypeOrientation.Landscape);
                        fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                        AssertThemedAbilityPresentation(fixture.Hud, PrototypeOrientation.Landscape, true, guardian);
                        Assert.That(AssertDefenseRealmIdentityPresentation(fixture.Hud, guardian, "POSSESSED CREATURE").gameObject, Is.SameAs(trackedRealmMark));

                        PrototypeSave.SetControlStyle(InRunControlStyleSelector.Joystick);
                        fixture.Hud.ControlStyleSelector.RefreshNow();
                        fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                        AssertThemedAbilityPresentation(fixture.Hud, PrototypeOrientation.Landscape, false, guardian);
                        Assert.That(AbilityIcons(fixture.Hud.transform, false, familyPrefix), Has.Length.EqualTo(2), "Joystick keeps only the two truthful button icons visible.");

                        PrototypeSave.SetControlStyle(InRunControlStyleSelector.Fingertap);
                        fixture.Hud.ControlStyleSelector.RefreshNow();
                        fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                        AssertThemedAbilityPresentation(fixture.Hud, PrototypeOrientation.Landscape, true, guardian);
                        Assert.That(player.IsActive, Is.True);

                        fixture.Energy.Refill();
                        Assert.That(fixture.Energy.Remaining, Is.EqualTo(30));
                        fixture.Energy.Consume(25);
                        Assert.That(fixture.Hud.PossessionEnergyLevel, Is.EqualTo(PossessionEnergyReadabilityLevel.Warning));
                        Assert.That(fixture.Hud.PossessionEnergyText, Is.EqualTo("POSSESSION ENDING  5.0s"));
                        Assert.That(fixture.Hud.PossessionEnergyFill, Is.EqualTo(5f / 30f).Within(.001f));

                        fixture.Energy.Consume(3);
                        Assert.That(fixture.Hud.PossessionEnergyLevel, Is.EqualTo(PossessionEnergyReadabilityLevel.Critical));
                        Assert.That(fixture.Hud.PossessionEnergyText, Is.EqualTo("RETURN TO KEEPER  2.0s"));

                        fixture.Possession.Release();
                        fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                        Assert.That(fixture.Hud.PossessionEnergyLevel, Is.EqualTo(PossessionEnergyReadabilityLevel.Normal));
                        Assert.That(fixture.Hud.PossessionEnergyText, Is.EqualTo("Possession energy  2.0/30s"));
                        Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one));
                        Assert.That(fixture.Hud.ChargeAffordanceVisible, Is.False);
                        Assert.That(AbilityIcons(fixture.Hud.transform, false, familyPrefix), Is.Empty, "Release hides all direct-control themed icons.");

                        fixture.Possession.Select(fixture.Defender);
                        Assert.That(fixture.Possession.PossessSelected(), Is.True);
                        Assert.That(fixture.Hud.PossessionEnergyLevel, Is.EqualTo(PossessionEnergyReadabilityLevel.Critical));
                        Assert.That(fixture.Hud.PossessionEnergyText, Is.EqualTo("RETURN TO KEEPER  2.0s"));
                        Assert.That(CountNamed(fixture.Hud.transform, "Possession Energy Meter"), Is.EqualTo(1), "Re-entry must reuse the existing meter.");

                        responsive.SetOrientationForTests(PrototypeOrientation.Portrait);
                        responsive.SetOrientationForTests(PrototypeOrientation.Landscape);
                        Assert.That(fixture.Energy.Remaining, Is.EqualTo(2));
                        Assert.That(fixture.Hud.PossessionEnergyText, Is.EqualTo("RETURN TO KEEPER  2.0s"));
                        Assert.That(meter.anchorMin, Is.EqualTo(meterAnchorMin));
                        Assert.That(meter.anchorMax, Is.EqualTo(meterAnchorMax));
                        Assert.That(meter.pivot, Is.EqualTo(meterPivot));
                        Assert.That(meter.anchoredPosition, Is.EqualTo(meterPosition));
                        Assert.That(meter.sizeDelta, Is.EqualTo(meterSize));

                        fixture.Defender.SetController(fixture.Defender.Controller<CreatureBrain>());
                        fixture.Hud.SendMessage("RefreshPossessionEnergy", SendMessageOptions.RequireReceiver);
                        Assert.That(fixture.Hud.PossessionEnergyLevel, Is.EqualTo(PossessionEnergyReadabilityLevel.Normal), "A controller swap must clear possession urgency.");
                        Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one));
                        fixture.Defender.SetController(fixture.Defender.Controller<PlayerController>());
                        fixture.Hud.SendMessage("RefreshPossessionEnergy", SendMessageOptions.RequireReceiver);
                        Assert.That(fixture.Hud.PossessionEnergyLevel, Is.EqualTo(PossessionEnergyReadabilityLevel.Critical));

                        string moment = null;
                        fixture.Possession.MomentFeedback += value => moment = value;
                        fixture.Energy.Consume(2);
                        fixture.Possession.SendMessage("Update", SendMessageOptions.RequireReceiver);
                        Assert.That(fixture.Possession.Possessed, Is.Null);
                        Assert.That(moment, Is.EqualTo("POSSESSION ENERGY DEPLETED — RETURNING TO KEEPER"));
                        Assert.That(fixture.Hud.PossessionEnergyLevel, Is.EqualTo(PossessionEnergyReadabilityLevel.Normal));
                        Assert.That(fixture.Hud.PossessionEnergyText, Is.EqualTo("Possession energy  0.0/30s"));
                        Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one));

                        fixture.Energy.Refill();
                        fixture.Possession.Select(fixture.Defender);
                        Assert.That(fixture.Possession.PossessSelected(), Is.True);
                        fixture.Energy.Consume(25);
                        Assert.That(fixture.Hud.PossessionEnergyLevel, Is.EqualTo(PossessionEnergyReadabilityLevel.Warning));
                        fixture.Invader.Health.TakeDamage(new DamageInfo(1000, null, fixture.Invader.transform.position), 0);
                        Assert.That(fixture.Defense.State, Is.EqualTo(DefenseState.DefenderVictory));
                        Assert.That(fixture.Possession.Possessed, Is.Null);
                        Assert.That(fixture.Hud.PossessionEnergyLevel, Is.EqualTo(PossessionEnergyReadabilityLevel.Normal));
                        Assert.That(fixture.Hud.PossessionEnergyText, Is.EqualTo("Possession energy  5.0/30s"));
                        Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one));
                        Assert.That(fixture.Hud.ChargeAffordanceVisible, Is.False);
                        Assert.That(AbilityIcons(fixture.Hud.transform, false, familyPrefix), Is.Empty, "Terminal result hides all direct-control themed icon presentation.");
                        Assert.That(AssertDefenseRealmIdentityPresentation(fixture.Hud, guardian, "DEFENSE COMPLETE").gameObject, Is.SameAs(trackedRealmMark), "Terminal state must preserve the canonical realm title mark.");
                    }
                    finally
                    {
                        fixture.Destroy();
                        GameplayInput.ResetForTests();
                    }
                    yield return null;
                    foreach (var icon in trackedAbilityIcons) Assert.That(icon == null, Is.True, "HUD teardown must destroy themed ability icon children.");
                    Assert.That(trackedChargeAffordance == null, Is.True, "HUD teardown must destroy the Charge affordance root.");
                    Assert.That(trackedRealmMark == null, Is.True, "HUD teardown must destroy the realm identity mark with its canonical title.");
                    Assert.That(trackedEnergyMeter == null, Is.True, "HUD teardown must destroy the reused energy meter with no orphan presentation.");
                }
            }
            finally
            {
                FirstPlayableMinute.ResetForTests();
                if (hadGuide) PlayerPrefs.SetString(FirstPlayableMinute.KeyForTests, previousGuide); else PlayerPrefs.DeleteKey(FirstPlayableMinute.KeyForTests);
                PrototypeSave.SetControlStyle(previousControlStyle);
                PlayerPrefs.Save();
                GameplayInput.ResetForTests();
            }
        }

        [UnityTest]
        public IEnumerator PossessionEnergyReadability_PulsesOnlyOnUrgencyTransitionsAndRestoresEveryReturn()
        {
            GameplayInput.ResetForTests();
            var fixture = EnergyHudFixture.Create(DefenseHudConfig.Infernal);
            try
            {
                fixture.Possession.Select(fixture.Defender);
                Assert.That(fixture.Possession.PossessSelected(), Is.True);
                fixture.Energy.Consume(25);
                Assert.That(fixture.Hud.PossessionEnergyLevel, Is.EqualTo(PossessionEnergyReadabilityLevel.Warning));
                yield return new WaitForSecondsRealtime(.06f);
                fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale.x, Is.GreaterThan(1.02f), "Entering Warning starts one bounded pulse.");

                fixture.Energy.Consume(.1f);
                fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale.x, Is.GreaterThan(1.02f), "Same-level tenth and fill updates cannot restart the Warning pulse.");
                yield return new WaitForSecondsRealtime(PossessionEnergyReadability.UrgencyPulseDuration - .06f + .01f);
                fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one), "The original Warning deadline must end the pulse before a same-level refresh could have ended a restarted pulse.");

                fixture.Energy.Consume(3);
                Assert.That(fixture.Hud.PossessionEnergyLevel, Is.EqualTo(PossessionEnergyReadabilityLevel.Critical));
                yield return new WaitForSecondsRealtime(.06f);
                fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale.x, Is.GreaterThan(1.02f), "Entering Critical starts one new bounded pulse.");
                yield return new WaitForSecondsRealtime(.20f);
                fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one), "The 0.24 second pulse ends at exact identity scale.");

                fixture.Possession.Release();
                Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one));
                fixture.Possession.Select(fixture.Defender);
                Assert.That(fixture.Possession.PossessSelected(), Is.True);
                yield return new WaitForSecondsRealtime(.06f);
                fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale.x, Is.GreaterThan(1.02f), "Low-energy re-possession receives one fresh, not delayed, Critical pulse.");

                fixture.Defender.SetController(fixture.Defender.Controller<CreatureBrain>());
                fixture.Hud.SendMessage("RefreshPossessionEnergy", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one), "Controller loss clears the meter immediately.");
                fixture.Defender.SetController(fixture.Defender.Controller<PlayerController>());
                fixture.Hud.SendMessage("RefreshPossessionEnergy", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one), "Ordinary PlayerController restoration is not a new possession or urgency transition.");
                fixture.Energy.Consume(5);
                fixture.Possession.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Possession.Possessed, Is.Null, "Depletion forces the existing return flow.");
                Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one));

                fixture.Energy.Refill();
                fixture.Possession.Select(fixture.Defender);
                Assert.That(fixture.Possession.PossessSelected(), Is.True);
                fixture.Energy.Consume(25);
                yield return new WaitForSecondsRealtime(.06f);
                fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale.x, Is.GreaterThan(1.02f));
                fixture.Invader.Health.TakeDamage(new DamageInfo(1000, null, fixture.Invader.transform.position), 0);
                Assert.That(fixture.Defense.State, Is.EqualTo(DefenseState.DefenderVictory));
                Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one), "Terminal result clears a live urgency pulse.");
            }
            finally
            {
                fixture.Destroy();
                GameplayInput.ResetForTests();
            }
        }

        [UnityTest]
        public IEnumerator PossessionEnergyReadability_DeathAndComponentDisableImmediatelyRestoreMeterScale()
        {
            GameplayInput.ResetForTests();
            var fixture = EnergyHudFixture.Create(DefenseHudConfig.Sylvan);
            try
            {
                fixture.Possession.Select(fixture.Defender);
                Assert.That(fixture.Possession.PossessSelected(), Is.True);
                fixture.Energy.Consume(25);
                yield return new WaitForSecondsRealtime(.06f);
                fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale.x, Is.GreaterThan(1.02f));

                fixture.HudObject.SetActive(false);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one), "Disabling the HUD cannot preserve a partial pulse scale.");
                fixture.HudObject.SetActive(true);
                yield return null;
                fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one), "An ordinary refresh after re-enable cannot replay a stale pulse.");

                fixture.Energy.Refill();
                fixture.Energy.Consume(25);
                yield return new WaitForSecondsRealtime(.06f);
                fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale.x, Is.GreaterThan(1.02f));
                fixture.Defender.Health.TakeDamage(new DamageInfo(1000, null, fixture.Defender.transform.position), 0);
                yield return null;
                fixture.Hud.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(fixture.Possession.Possessed, Is.Null);
                Assert.That(fixture.Hud.PossessionEnergyMeterScale, Is.EqualTo(Vector3.one), "A possessed defender death immediately clears pulse presentation.");
            }
            finally
            {
                fixture.Destroy();
                GameplayInput.ResetForTests();
            }
        }

        [UnityTest]
        public IEnumerator PossessionPreservesEntityStateAndRestoresAiWithSingleView()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig));
            cameraObject.tag = "MainCamera";
            var entityObject = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            entityObject.name = "Possessable";
            var primitiveCollider = entityObject.GetComponent<Collider>();
            primitiveCollider.enabled = false;
            Object.Destroy(primitiveCollider);
            entityObject.AddComponent<CharacterController>();
            entityObject.AddComponent<Health>();
            entityObject.AddComponent<CombatEntity>();
            entityObject.AddComponent<PlayerController>();
            entityObject.AddComponent<CreatureBrain>();
            var managerObject = new GameObject("Possession Manager", typeof(PossessionManager));
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            var recipe = ScriptableObject.CreateInstance<CharacterVisualRecipe>();
            try
            {
                ability.DisplayName = "Test Strike"; ability.Cooldown = 10; ability.Windup = .1f;
                definition.DisplayName = "Possessable";
                definition.Possessable = true;
                recipe.Family = CharacterVisualFamily.LargeCreature; recipe.Head = VisualModuleStyle.Bark; recipe.Arms = VisualModuleStyle.Claws; recipe.Primary = Color.green; recipe.Secondary = new Color(.2f, .12f, .06f); recipe.AccentColor = Color.yellow; definition.VisualRecipe = recipe;
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1, AttackSpeed = 1 };
                definition.Abilities = new[] { ability };
                var entity = entityObject.GetComponent<CombatEntity>();
                entity.Initialize(definition);
                var ai = entityObject.GetComponent<CreatureBrain>();
                var player = entityObject.GetComponent<PlayerController>();
                entity.SetController(ai);
                var rig = cameraObject.GetComponent<PrototypeCameraRig>();
                rig.ConfigureOverview(new Vector3(0, 5, -8), Quaternion.identity);
                rig.SnapToOverview();
                var manager = managerObject.GetComponent<PossessionManager>();
                manager.Initialize(rig); manager.Register(entity); manager.Select(entity);
                entity.Health.TakeDamage(new DamageInfo(17, null, entity.transform.position), 0);
                var healthBefore = entity.Health.Current;
                Assert.That(entity.TryUse(0, Vector3.forward), Is.True);
                var readyBefore = entity.Abilities[0].IsReady;

                Assert.That(manager.PossessSelected(), Is.True);
                yield return null;
                Assert.That(manager.Possessed, Is.SameAs(entity));
                Assert.That(manager.Possessed.gameObject, Is.SameAs(entity.gameObject));
                Assert.That(entity.GetComponent<CharacterVisualAssembler>(), Is.Not.Null);
                foreach (var collider in entity.GetComponentsInChildren<Collider>(true)) if (collider.transform != entity.transform) Assert.That(collider.enabled, Is.False);
                Assert.That(entity.Health.Current, Is.EqualTo(healthBefore));
                Assert.That(entity.Abilities[0].IsReady, Is.EqualTo(readyBefore));
                Assert.That(player.IsActive, Is.True); Assert.That(ai.IsActive, Is.False);
                Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1));

                manager.Release();
                yield return null;
                Assert.That(manager.Possessed, Is.Null);
                Assert.That(entity.Health.Current, Is.EqualTo(healthBefore));
                Assert.That(entity.Abilities[0].IsReady, Is.EqualTo(readyBefore));
                Assert.That(player.IsActive, Is.False); Assert.That(ai.IsActive, Is.True);
                Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Has.Length.EqualTo(1));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(1));

                var assembler = entity.GetComponent<CharacterVisualAssembler>();
                assembler.Clear();
                yield return null;
                Assert.That(entity.transform.Find("Character Visual Modules"), Is.Null);
                Assert.That(entity.GetComponent<Renderer>().enabled, Is.True);
            }
            finally
            {
                Object.Destroy(managerObject); Object.Destroy(entityObject); Object.Destroy(cameraObject);
                Object.Destroy(definition); Object.Destroy(ability); Object.Destroy(recipe);
            }
        }

        [UnityTest]
        public IEnumerator PossessionPresentation_BillboardsAndCleansUpWithoutInputArtifacts()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig));
            cameraObject.tag = "MainCamera";
            var entityObject = new GameObject("Guardian Ent", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
            var managerObject = new GameObject("Possession Manager", typeof(PossessionManager));
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var initialCanvasCount = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
            var initialEventSystemCount = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
            var initialListenerCount = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
            try
            {
                definition.DisplayName = "Guardian Ent"; definition.Possessable = true; definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 2 };
                var entity = entityObject.GetComponent<CombatEntity>(); entity.Initialize(definition);
                var ai = entityObject.GetComponent<CreatureBrain>(); var player = entityObject.GetComponent<PlayerController>(); entity.SetController(ai);
                var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.ConfigureOverview(new Vector3(0, 6, -8), Quaternion.identity); rig.SnapToOverview();
                var manager = managerObject.GetComponent<PossessionManager>(); manager.Initialize(rig); manager.Register(entity);
                string moment = null; manager.MomentFeedback += value => moment = value;

                manager.Select(entity); yield return new WaitForEndOfFrame();
                var selection = entity.transform.Find("Possession Selection Presentation");
                Assert.That(selection, Is.Not.Null);
                var marker = selection.GetComponent<PossessionSelectionPresentation>();
                Assert.That(marker, Is.Not.Null);
                Assert.That(marker.LabelTransform.GetComponent<TextMesh>().text, Does.Contain("GUARDIAN ENT SELECTED"));
                foreach (var collider in selection.GetComponentsInChildren<Collider>(true)) Assert.That(collider.enabled, Is.False);
                yield return new WaitForEndOfFrame();
                var toCamera = (cameraObject.transform.position - marker.LabelTransform.position).normalized;
                Assert.That(Vector3.Dot(-marker.LabelTransform.forward, toCamera), Is.GreaterThan(.98f));

                Assert.That(manager.PossessSelected(), Is.True); yield return null;
                Assert.That(moment, Is.EqualTo("YOU CONTROL: GUARDIAN ENT"));
                Assert.That(manager.Possessed, Is.SameAs(entity)); Assert.That(player.IsActive, Is.True); Assert.That(ai.IsActive, Is.False);
                Assert.That(entity.transform.Find("Possession Selection Presentation"), Is.Null);
                var canvasCountAfterTakeover = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
                Assert.That(canvasCountAfterTakeover, Is.LessThanOrEqualTo(initialCanvasCount + 1));

                manager.Release(); yield return null;
                Assert.That(moment, Is.EqualTo("RELEASED — KEEPER OVERVIEW"));
                Assert.That(manager.Possessed, Is.Null); Assert.That(player.IsActive, Is.False); Assert.That(ai.IsActive, Is.True);
                Assert.That(Object.FindObjectsByType<PossessionSelectionPresentation>(FindObjectsInactive.Include, FindObjectsSortMode.None), Is.Empty);
                Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(canvasCountAfterTakeover));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(initialEventSystemCount));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(initialListenerCount));
            }
            finally { Object.Destroy(managerObject); Object.Destroy(entityObject); Object.Destroy(cameraObject); Object.Destroy(definition); }
        }

        [UnityTest]
        public IEnumerator ForcedReleaseRestoresTimeAndDirectControl()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig)); cameraObject.tag = "MainCamera";
            var entityObject = new GameObject("Possessable", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
            var managerObject = new GameObject("Possession Manager", typeof(PossessionManager)); var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            try
            {
                definition.DisplayName = "Possessable"; definition.Possessable = true; definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1, AttackSpeed = 1 };
                var entity = entityObject.GetComponent<CombatEntity>(); entity.Initialize(definition); var ai = entityObject.GetComponent<CreatureBrain>(); var player = entityObject.GetComponent<PlayerController>(); entity.SetController(ai);
                var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.SnapToOverview(); var manager = managerObject.GetComponent<PossessionManager>(); manager.Initialize(rig); manager.ConfigureEnergy(new PossessionEnergy(.01f)); manager.Register(entity); string moment = null; manager.MomentFeedback += value => moment = value; manager.Select(entity);
                Assert.That(manager.PossessSelected(), Is.True); yield return new WaitForSecondsRealtime(1.1f);
                Assert.That(manager.Possessed, Is.Null); Assert.That(moment, Is.EqualTo("POSSESSION ENERGY DEPLETED — RETURNING TO KEEPER")); Assert.That(player.IsActive, Is.False); Assert.That(ai.IsActive, Is.True); Assert.That(Time.timeScale, Is.EqualTo(1).Within(.001f)); Assert.That(rig.IsTransitioning, Is.False);
                Assert.That(Object.FindObjectsByType<PossessionSelectionPresentation>(FindObjectsInactive.Include, FindObjectsSortMode.None), Is.Empty);
            }
            finally { Time.timeScale = 1; Time.fixedDeltaTime = .02f; Object.Destroy(managerObject); Object.Destroy(entityObject); Object.Destroy(cameraObject); Object.Destroy(definition); }
        }

        [UnityTest]
        public IEnumerator BloodKnightHeroPrefab_BuildsAsVisualOnlyChild()
        {
            var heroRecipe = PrototypeRuntimeFactory.BloodKnightRecipe;
            Assert.That(heroRecipe.BaseBodyPrefab, Is.Not.Null);
            var host = GameObject.CreatePrimitive(PrimitiveType.Capsule); var assembler = host.AddComponent<CharacterVisualAssembler>();
            try
            {
                Assert.That(assembler.Assemble(heroRecipe), Is.True);
                yield return null;
                var root = host.transform.Find("Character Visual Modules");
                Assert.That(root, Is.Not.Null); Assert.That(root.Find("Presentation Pivot/Base Body"), Is.Not.Null);
                foreach (var collider in root.GetComponentsInChildren<Collider>(true)) Assert.That(collider.enabled, Is.False);
            }
            finally { Object.Destroy(host); }
        }

        [UnityTest]
        public IEnumerator VisualMotion_MovesOnlyPresentationPivotAndRestoresAfterFeedbackCleanup()
        {
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>(); var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var host = GameObject.CreatePrimitive(PrimitiveType.Capsule); host.GetComponent<Collider>().enabled = false; host.AddComponent<CharacterController>(); host.AddComponent<Health>(); host.AddComponent<CombatEntity>();
            try
            {
                ability.Kind = AbilityKind.Melee; ability.Damage = 5; ability.Range = 2; ability.Radius = 1; ability.Windup = .1f; ability.Cooldown = 0;
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3 }; definition.Abilities = new[] { ability };
                definition.VisualRecipe = ScriptableObject.CreateInstance<CharacterVisualRecipe>(); definition.VisualRecipe.Family = CharacterVisualFamily.Beast; definition.VisualRecipe.Primary = Color.green; definition.VisualRecipe.Secondary = Color.black; definition.VisualRecipe.AccentColor = Color.yellow;
                var entity = host.GetComponent<CombatEntity>(); entity.Initialize(definition);
                var motion = host.GetComponent<CharacterVisualMotion>(); var rootPosition = host.transform.position;
                entity.Move(Vector3.forward * 3); yield return null;
                Assert.That(host.transform.position.z, Is.GreaterThan(rootPosition.z));
                Assert.That(Vector3.Distance(motion.PresentationPivot.localPosition, motion.BasePosition), Is.LessThan(.12f));
                Assert.That(entity.TryUse(0, Vector3.forward), Is.True); yield return null;
                Assert.That(Quaternion.Angle(motion.PresentationPivot.localRotation, motion.BaseRotation), Is.GreaterThan(.1f));
                host.GetComponent<CombatFeedback>().ShowHit(3, host.transform.position, host.transform.position + Vector3.left);
                yield return null;
                host.GetComponent<CombatFeedback>().Cleanup();
                Assert.That(motion.PresentationPivot.localPosition, Is.EqualTo(motion.BasePosition)); Assert.That(motion.PresentationPivot.localRotation, Is.EqualTo(motion.BaseRotation));
                foreach (var collider in motion.PresentationPivot.GetComponentsInChildren<Collider>(true)) Assert.That(collider.enabled, Is.False);
            }
            finally { Object.Destroy(host); Object.Destroy(definition.VisualRecipe); Object.Destroy(definition); Object.Destroy(ability); }
        }

        [UnityTest]
        public IEnumerator AbilityAction_GatesOverlapAndCleansTransientFeedback()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera)); cameraObject.tag = "MainCamera";
            var attackerObject = new GameObject("Attacker", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var targetObject = new GameObject("Target", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>(); var targetDefinition = ScriptableObject.CreateInstance<CharacterDefinition>(); var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            try
            {
                ability.Kind = AbilityKind.Melee; ability.Damage = 20; ability.Range = 2; ability.Radius = 1.2f; ability.Windup = .15f; ability.Cooldown = 0;
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 }; definition.Abilities = new[] { ability };
                targetDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                var attacker = attackerObject.GetComponent<CombatEntity>(); var target = targetObject.GetComponent<CombatEntity>(); attacker.Initialize(definition); target.Initialize(targetDefinition); targetObject.transform.position = Vector3.forward * 1.1f;
                Assert.That(attacker.TryUse(0, Vector3.forward), Is.True);
                Assert.That(attacker.ActionPhase, Is.EqualTo(CombatActionPhase.Windup));
                Assert.That(attacker.TryUse(0, Vector3.forward), Is.False);
                var actionDeadline = Time.realtimeSinceStartup + 1f;
                while (attacker.IsActionResolving && Time.realtimeSinceStartup < actionDeadline) yield return null;
                Assert.That(target.Health.Current, Is.LessThan(target.Health.Maximum));
                Assert.That(attacker.IsActionResolving, Is.False);
                yield return new WaitForSecondsRealtime(.8f);
                foreach (var marker in Object.FindObjectsByType<CameraFacingMarker>(FindObjectsSortMode.None)) Assert.That(marker, Is.Null);
                Assert.That(attackerObject.GetComponent<CharacterController>().enabled, Is.True);
            }
            finally { Object.Destroy(cameraObject); Object.Destroy(attackerObject); Object.Destroy(targetObject); Object.Destroy(definition); Object.Destroy(targetDefinition); Object.Destroy(ability); }
        }

        [UnityTest]
        public IEnumerator CombatCameraAwareness_TracksOnlyNearbyReportedThreatAndCleansUp()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig), typeof(CombatCameraAwareness)); cameraObject.tag = "MainCamera";
            var playerObject = new GameObject("Player", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController));
            var threatObject = new GameObject("Threat", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var hudObject = new GameObject("Test Gameplay HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(ResponsiveHudRoot));
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>(); var threatDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            try
            {
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                threatDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                var player = playerObject.GetComponent<CombatEntity>(); var threat = threatObject.GetComponent<CombatEntity>(); player.Initialize(definition); threat.Initialize(threatDefinition);
                threatObject.transform.position = new Vector3(9, 0, 2);
                var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.SnapTo(player, CameraMode.HeroCombat);
                hudObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
                var responsive = hudObject.GetComponent<ResponsiveHudRoot>(); responsive.Initialize(false); rig.BindCombatHud(responsive);
                player.SetController(playerObject.GetComponent<PlayerController>());
                var awareness = cameraObject.GetComponent<CombatCameraAwareness>(); awareness.SetControlled(player); awareness.ReportThreat(threat);
                yield return null; yield return null;
                Assert.That(awareness.HasEligibleThreat, Is.True);
                Assert.That(rig.HasRequestedCombatFocus, Is.True);
                Assert.That(awareness.IndicatorVisible || awareness.TargetPlateVisible, Is.True);

                threatObject.transform.position = new Vector3(30, 0, 2);
                yield return null;
                Assert.That(awareness.HasEligibleThreat, Is.False);
                Assert.That(awareness.IndicatorVisible, Is.False);
                Assert.That(awareness.TargetPlateVisible, Is.False);
                Assert.That(rig.HasCombatFocus, Is.False);

                awareness.ReportThreat(threat); threatObject.transform.position = new Vector3(9, 0, 2); yield return null;
                GameplayInput.SetTerminalState(true); yield return null;
                Assert.That(awareness.HasEligibleThreat, Is.False);
                Assert.That(awareness.IndicatorVisible, Is.False);
                Assert.That(awareness.TargetPlateVisible, Is.False);
                GameplayInput.SetTerminalState(false);

                awareness.ReportThreat(threat); yield return null;
                threat.Health.TakeDamage(new DamageInfo(1000, playerObject, threatObject.transform.position), 0);
                yield return null;
                Assert.That(awareness.HasEligibleThreat, Is.False);
                Assert.That(awareness.IndicatorVisible, Is.False);
                Assert.That(awareness.TargetPlateVisible, Is.False);
            }
            finally { GameplayInput.SetTerminalState(false); Object.Destroy(hudObject); Object.Destroy(cameraObject); Object.Destroy(playerObject); Object.Destroy(threatObject); Object.Destroy(definition); Object.Destroy(threatDefinition); }
        }

        [UnityTest]
        public IEnumerator CombatCameraAwareness_TracksActiveCreatureIntentAcrossEdgesAndCleansUp()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig), typeof(CombatCameraAwareness)); cameraObject.tag = "MainCamera";
            var playerObject = new GameObject("Player", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController));
            var threatObject = new GameObject("Hostile Creature", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(CreatureBrain));
            var hudObject = new GameObject("Test Gameplay HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(ResponsiveHudRoot));
            var eventObject = new GameObject("Test EventSystem", typeof(EventSystem));
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>(); var threatDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            try
            {
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                threatDefinition.DisplayName = "Test Attacker"; threatDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                var player = playerObject.GetComponent<CombatEntity>(); var threat = threatObject.GetComponent<CombatEntity>();
                player.Initialize(definition); threat.Initialize(threatDefinition);
                var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.SnapTo(player, CameraMode.HeroCombat);
                var canvas = hudObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var responsive = hudObject.GetComponent<ResponsiveHudRoot>(); responsive.Initialize(false);
                var objectiveObject = new GameObject("Test Objective Cue", typeof(RectTransform)); objectiveObject.transform.SetParent(hudObject.transform, false);
                var objectiveRect = (RectTransform)objectiveObject.transform; objectiveRect.anchorMin = objectiveRect.anchorMax = new Vector2(1, .5f); objectiveRect.anchoredPosition = new Vector2(-28, 0); objectiveRect.sizeDelta = new Vector2(220, 64);
                rig.BindCombatHud(responsive, objectiveRect);
                var testView = cameraObject.GetComponent<Camera>(); testView.transform.SetPositionAndRotation(new Vector3(0, 2, -10), Quaternion.LookRotation(Vector3.forward)); testView.fieldOfView = 60f; testView.aspect = 1f;
                // Keep the camera basis fixed while testing the presentation indicator's real left/right mapping.
                rig.enabled = false;
                player.SetController(playerObject.GetComponent<PlayerController>());
                var awareness = cameraObject.GetComponent<CombatCameraAwareness>(); awareness.SetControlled(player);
                var brain = threatObject.GetComponent<CreatureBrain>(); brain.DetectionRange = 14f; threat.SetController(brain);

                var pointAhead = testView.transform.position + testView.transform.forward * 10f;
                var behindPoint = testView.transform.position - testView.transform.forward * 2f;
                var rightEdge = behindPoint + testView.transform.right * 5f;
                threatObject.transform.position = pointAhead; Physics.SyncTransforms(); brain.Target = player; brain.Tick();
                awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                Assert.That(awareness.TargetPlateVisible, Is.True);
                Assert.That(awareness.IndicatorVisible, Is.False);

                // Keep edge points behind the camera so direction is independent of the runner's aspect ratio.
                threatObject.transform.position = rightEdge; Physics.SyncTransforms(); brain.Tick();
                awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                Assert.That(brain.State, Is.EqualTo(BrainState.Chase).Or.EqualTo(BrainState.Attack));
                Assert.That(awareness.HasEligibleThreat, Is.True);
                Assert.That(rig.HasRequestedCombatFocus, Is.True);
                Assert.That(CombatCameraAwareness.IndicatorDirectionFor(new Vector3(1.1f, .5f, 1), Vector3.right, Vector3.right), Is.EqualTo(1));
                var indicator = awareness.IndicatorRect.GetComponentInChildren<UnityEngine.UI.Text>(true);
                Assert.That(indicator, Is.Not.Null); Assert.That(awareness.IndicatorRaycastTarget, Is.False);
                Assert.That(awareness.IndicatorVisible, Is.True);
                Assert.That(awareness.TargetPlateVisible, Is.False);
                Assert.That(awareness.IndicatorText, Is.EqualTo("ATTACKER  ▶"));
                Assert.That(awareness.Urgency, Is.EqualTo(CombatThreatUrgency.Attacker));
                Assert.That(awareness.EdgePulseCount, Is.EqualTo(1));
                Assert.That(objectiveRect.anchoredPosition.y, Is.EqualTo(-76));
                Assert.That(awareness.PresentationRoot.parent, Is.EqualTo(hudObject.transform));
                Assert.That(cameraObject.GetComponentsInChildren<UnityEngine.UI.Text>(true), Is.Empty);
                Assert.That(cameraObject.GetComponentsInChildren<Canvas>(true), Is.Empty);
                Assert.That(hudObject.GetComponentsInChildren<Canvas>(true), Has.Length.EqualTo(1));
                Assert.That(cameraObject.GetComponentsInChildren<EventSystem>(true), Is.Empty);
                Assert.That(cameraObject.GetComponentsInChildren<AudioListener>(true), Has.Length.EqualTo(1));
                responsive.SetOrientationForTests(PrototypeOrientation.Portrait); awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                Assert.That(awareness.IndicatorRect.sizeDelta, Is.EqualTo(new Vector2(300, 80)));
                Assert.That(awareness.IndicatorRect.anchorMin.y, Is.EqualTo(.54f));
                Assert.That(awareness.IndicatorRect.anchoredPosition.x, Is.EqualTo(-24));
                responsive.SetOrientationForTests(PrototypeOrientation.Landscape); awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                Assert.That(awareness.IndicatorRect.sizeDelta, Is.EqualTo(new Vector2(260, 68)));
                Assert.That(awareness.IndicatorRect.anchorMin.y, Is.EqualTo(.5f));
                Assert.That(awareness.IndicatorRect.anchoredPosition.x, Is.EqualTo(-28));

                threatObject.transform.position = pointAhead; Physics.SyncTransforms(); brain.Tick(); awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                Assert.That(awareness.IndicatorVisible, Is.False);
                Assert.That(awareness.TargetPlateVisible, Is.True);
                Assert.That(awareness.TargetPlateRaycastTarget, Is.False);
                Assert.That(awareness.TargetPlateText, Is.EqualTo("ATTACKER  TEST ATTACKER  100/100 HP"));
                Assert.That(awareness.TargetPlateRect.sizeDelta, Is.EqualTo(new Vector2(340, 58)));
                Assert.That(objectiveRect.anchoredPosition.y, Is.Zero);
                threat.Health.TakeDamage(new DamageInfo(20, playerObject, threatObject.transform.position), 0);
                awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                Assert.That(awareness.TargetPlateText, Is.EqualTo("ATTACKER  TEST ATTACKER  80/100 HP"));

                brain.Target = null; brain.Tick();
                var leftEdge = behindPoint - testView.transform.right * 5f;
                threatObject.transform.position = leftEdge; Physics.SyncTransforms(); brain.Target = player; brain.Tick(); awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                Assert.That(awareness.HasEligibleThreat, Is.True);
                Assert.That(CombatCameraAwareness.IndicatorDirectionFor(new Vector3(-.1f, .5f, 1), Vector3.right, -Vector3.right), Is.EqualTo(-1));
                Assert.That(awareness.IndicatorVisible, Is.True);
                Assert.That(awareness.TargetPlateVisible, Is.False);
                Assert.That(awareness.IndicatorText, Is.EqualTo("◀  ATTACKER"));
                Assert.That(awareness.EdgePulseCount, Is.EqualTo(1), "Returning across the edge boundary must not replay arrival.");

                player.Health.TakeDamage(new DamageInfo(1, threatObject, playerObject.transform.position), 0);
                awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                Assert.That(awareness.IndicatorText, Is.EqualTo("◀  ATTACKING"));
                Assert.That(awareness.Urgency, Is.EqualTo(CombatThreatUrgency.Attacking));
                Assert.That(awareness.EdgePulseCount, Is.EqualTo(2));
                yield return new WaitForSecondsRealtime(.82f);
                awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                Assert.That(awareness.IndicatorText, Is.EqualTo("◀  ATTACKER"));
                Assert.That(awareness.EdgePulsePlaying, Is.False);
                player.Health.TakeDamage(new DamageInfo(1, threatObject, playerObject.transform.position), 0);
                awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                Assert.That(awareness.EdgePulseCount, Is.EqualTo(2), "Later urgency changes must not replay the one-shot pulse.");
                yield return new WaitForSecondsRealtime(.82f);

                threatObject.transform.position = playerObject.transform.position + Vector3.forward * 2f; Physics.SyncTransforms(); brain.Tick(); awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                Assert.That(brain.State, Is.EqualTo(BrainState.Attack));
                Assert.That(awareness.HasEligibleThreat, Is.True);
                Assert.That(awareness.Urgency, Is.EqualTo(CombatThreatUrgency.Attacking), "Active Attack intent is independently immediate after damage recency expires.");

                brain.Target = null; brain.Tick();
                yield return new WaitForSecondsRealtime(2.25f);
                Assert.That(awareness.HasEligibleThreat, Is.False);
                Assert.That(awareness.IndicatorVisible, Is.False);
                Assert.That(awareness.TargetPlateVisible, Is.False);
                Assert.That(rig.HasCombatFocus, Is.False);

                rightEdge = behindPoint + testView.transform.right * 5f;
                threatObject.transform.position = rightEdge; Physics.SyncTransforms(); brain.Target = player; brain.Tick(); awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                GameplayInput.SetTerminalState(true); awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                Assert.That(awareness.HasEligibleThreat, Is.False);
                Assert.That(awareness.IndicatorVisible, Is.False);
                Assert.That(awareness.TargetPlateVisible, Is.False);
                GameplayInput.SetTerminalState(false);

                brain.Target = null; brain.Tick(); brain.Target = player; brain.Tick(); awareness.SendMessage("LateUpdate", SendMessageOptions.RequireReceiver);
                player.SetController(null);
                Assert.That(awareness.HasEligibleThreat, Is.False);
                Assert.That(awareness.IndicatorVisible, Is.False);
                Assert.That(awareness.TargetPlateVisible, Is.False);
                Assert.That(rig.HasCombatFocus, Is.False);
            }
            finally { GameplayInput.SetTerminalState(false); Object.Destroy(eventObject); Object.Destroy(hudObject); Object.Destroy(cameraObject); Object.Destroy(playerObject); Object.Destroy(threatObject); Object.Destroy(definition); Object.Destroy(threatDefinition); }
        }

        [UnityTest]
        public IEnumerator CombatCameraAwareness_TracksRaidInvaderIntentDuringPossessedDefenseAndCleansUp()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig), typeof(CombatCameraAwareness)); cameraObject.tag = "MainCamera";
            var defenderObject = new GameObject("Possessed Defender", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
            var alternateObject = new GameObject("Alternate Defender", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var invaderObject = new GameObject("Raid Invader", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(RaidInvaderBrain));
            var managerObject = new GameObject("Possession Manager", typeof(PossessionManager));
            var hudObject = new GameObject("Possessed Defense HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(ResponsiveHudRoot));
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name = "Possessed Defense Ground";
            var defenderDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var alternateDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var invaderDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var attack = ScriptableObject.CreateInstance<AbilityDefinition>();
            try
            {
                GameplayInput.ResetForTests();
                defenderDefinition.DisplayName = "Guardian Ent"; defenderDefinition.Possessable = true; defenderDefinition.Stats = new CombatStats { MaxHealth = 120, MoveSpeed = 3 };
                alternateDefinition.DisplayName = "Realm Wolf"; alternateDefinition.Stats = new CombatStats { MaxHealth = 80, MoveSpeed = 3 };
                attack.DisplayName = "Invader Rush"; attack.Kind = AbilityKind.Dash; attack.Damage = 12; attack.Range = 1; attack.Radius = .1f; attack.Windup = .1f; attack.Cooldown = 0; attack.DashDistance = .3f;
                invaderDefinition.DisplayName = "Blood Knight"; invaderDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 0 }; invaderDefinition.Abilities = new[] { attack };

                defenderObject.transform.position = new Vector3(0, 1, 0);
                alternateObject.transform.position = new Vector3(40, 1, 0);
                invaderObject.transform.position = new Vector3(0, 1, 5);
                ground.transform.position = new Vector3(0, -.25f, 0); ground.transform.localScale = new Vector3(80, .5f, 80);
                var defender = defenderObject.GetComponent<CombatEntity>(); defender.Initialize(defenderDefinition); defender.SetController(defender.Controller<CreatureBrain>());
                var alternate = alternateObject.GetComponent<CombatEntity>(); alternate.Initialize(alternateDefinition);
                var invader = invaderObject.GetComponent<CombatEntity>(); invader.Initialize(invaderDefinition);

                var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.SnapToOverview();
                var canvas = hudObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                var responsive = hudObject.GetComponent<ResponsiveHudRoot>(); responsive.Initialize(false);
                var awareness = rig.BindCombatHud(responsive);
                var brain = invaderObject.GetComponent<RaidInvaderBrain>(); brain.Configure(System.Array.Empty<Vector3>(), new[] { defender, alternate }, 0); invader.SetController(brain); brain.Tick();
                Assert.That(brain.CurrentTarget, Is.SameAs(defender), "The factual invader intent exists before the camera subscribes to a controlled defender.");

                var possession = managerObject.GetComponent<PossessionManager>(); possession.Initialize(rig); possession.Register(defender); possession.Select(defender);
                Assert.That(possession.PossessSelected(), Is.True);
                // Possession transition/slow-beat timing is covered elsewhere; this fixture isolates the resulting factual controller/camera state.
                possession.StopAllCoroutines();
                rig.StopAllCoroutines();
                Time.timeScale = 1; Time.fixedDeltaTime = .02f;
                rig.SnapTo(defender, CameraMode.PossessedCreature);
                Assert.That(rig.Mode, Is.EqualTo(CameraMode.PossessedCreature));

                var view = cameraObject.GetComponent<Camera>();
                rig.enabled = false;
                view.transform.SetPositionAndRotation(new Vector3(0, 3, -5), Quaternion.LookRotation(Vector3.forward));
                view.fieldOfView = 60; view.aspect = 1;
                invaderObject.transform.position = defenderObject.transform.position + Vector3.forward * 5;
                Physics.SyncTransforms(); brain.Tick(); RefreshAwareness(awareness);
                Assert.That(defender.Health.Current, Is.EqualTo(defender.Health.Maximum), "Pre-damage intent must be sufficient for awareness.");
                Assert.That(awareness.HasEligibleThreat, Is.True);
                Assert.That(awareness.TargetPlateVisible, Is.True);
                Assert.That(awareness.IndicatorVisible, Is.False);
                Assert.That(awareness.TargetPlateText, Is.EqualTo("ATTACKER  BLOOD KNIGHT  100/100 HP"));
                Assert.That(rig.HasRequestedCombatFocus, Is.True);

                responsive.SetOrientationForTests(PrototypeOrientation.Portrait); RefreshAwareness(awareness);
                Assert.That(awareness.TargetPlateRect.sizeDelta, Is.EqualTo(new Vector2(380, 64)));
                var behind = view.transform.position - view.transform.forward;
                behind.y = defenderObject.transform.position.y;
                invaderObject.transform.position = behind + view.transform.right * 3; Physics.SyncTransforms(); brain.Tick();
                var rightViewport = view.WorldToViewportPoint(invaderObject.transform.position + Vector3.up);
                Assert.That(rightViewport.z, Is.LessThanOrEqualTo(0), "The fixed fixture must place the raid invader behind the camera before cue refresh.");
                Assert.That(CombatCameraAwareness.ShouldUseEdge(rightViewport, false), Is.True);
                RefreshAwareness(awareness);
                Assert.That(awareness.IndicatorVisible, Is.True);
                Assert.That(awareness.IndicatorDirection, Is.EqualTo(1));
                Assert.That(awareness.IndicatorText, Is.EqualTo("ATTACKER  ▶"));
                Assert.That(awareness.IndicatorRect.sizeDelta, Is.EqualTo(new Vector2(300, 80)));

                responsive.SetOrientationForTests(PrototypeOrientation.Landscape);
                invaderObject.transform.position = behind - view.transform.right * 3; Physics.SyncTransforms(); brain.Tick(); RefreshAwareness(awareness);
                Assert.That(awareness.IndicatorVisible, Is.True);
                Assert.That(awareness.IndicatorDirection, Is.EqualTo(-1));
                Assert.That(awareness.IndicatorText, Is.EqualTo("◀  ATTACKER"));
                Assert.That(awareness.IndicatorRect.sizeDelta, Is.EqualTo(new Vector2(260, 68)));

                invaderObject.transform.position = defenderObject.transform.position + Vector3.forward * 15; Physics.SyncTransforms(); RefreshAwareness(awareness);
                Assert.That(awareness.HasEligibleThreat, Is.False, "The existing 14 m bound must clear tracked raid intent.");
                Assert.That(awareness.IndicatorVisible, Is.False); Assert.That(awareness.TargetPlateVisible, Is.False);
                brain.Tick();
                Assert.That(brain.CurrentTarget, Is.Null);
                invaderObject.transform.position = defenderObject.transform.position + Vector3.forward * 5; Physics.SyncTransforms(); brain.Tick(); RefreshAwareness(awareness);
                Assert.That(awareness.HasEligibleThreat, Is.True, "A factual nearby target event must restore intent without a scene scan.");

                defender.Motor.enabled = false;
                view.transform.rotation = Quaternion.LookRotation(Vector3.back);
                invaderObject.transform.position = defenderObject.transform.position + Vector3.right * 2; Physics.SyncTransforms(); brain.Tick(); RefreshAwareness(awareness);
                Assert.That(invader.ActionPhase, Is.EqualTo(CombatActionPhase.Windup));
                Assert.That(awareness.IndicatorVisible, Is.True);
                Assert.That(awareness.Urgency, Is.EqualTo(CombatThreatUrgency.Attacking));
                Assert.That(awareness.IndicatorText, Does.Contain("ATTACKING"));
                yield return WaitForActionPhase(invader, CombatActionPhase.Impact);
                RefreshAwareness(awareness);
                Assert.That(awareness.Urgency, Is.EqualTo(CombatThreatUrgency.Attacking), "Accepted Impact remains factual attacking urgency.");
                yield return WaitForActionPhase(invader, CombatActionPhase.Recovery);
                RefreshAwareness(awareness);
                Assert.That(awareness.Urgency, Is.EqualTo(CombatThreatUrgency.Attacker), "Recovery alone must not claim ATTACKING without recent damage.");

                defenderObject.transform.position = new Vector3(30, 1, 0);
                alternateObject.transform.position = invaderObject.transform.position + Vector3.forward;
                Physics.SyncTransforms(); brain.Tick();
                Assert.That(brain.CurrentTarget, Is.SameAs(alternate));
                Assert.That(awareness.HasEligibleThreat, Is.False, "Factual retarget must clear the possessed defender's cue synchronously.");
                Assert.That(awareness.IndicatorVisible, Is.False); Assert.That(awareness.TargetPlateVisible, Is.False); Assert.That(rig.HasCombatFocus, Is.False);

                alternateObject.transform.position = new Vector3(40, 1, 0);
                defenderObject.transform.position = invaderObject.transform.position + Vector3.forward * 5;
                Physics.SyncTransforms(); brain.Tick(); RefreshAwareness(awareness);
                Assert.That(brain.CurrentTarget, Is.SameAs(defender)); Assert.That(awareness.HasEligibleThreat, Is.True);
                invader.SetController(null);
                Assert.That(awareness.HasEligibleThreat, Is.False, "Invader control loss must clear intent synchronously.");
                Assert.That(awareness.IndicatorVisible, Is.False); Assert.That(awareness.TargetPlateVisible, Is.False);

                brain.Configure(System.Array.Empty<Vector3>(), new[] { defender }, 0); invader.SetController(brain); brain.Tick(); RefreshAwareness(awareness);
                Assert.That(awareness.HasEligibleThreat, Is.True);
                rig.enabled = true;
                possession.Release();
                rig.StopAllCoroutines(); rig.SnapToOverview();
                Assert.That(possession.Possessed, Is.Null);
                Assert.That(awareness.HasEligibleThreat, Is.False, "Possession/controller loss must clear awareness immediately.");
                Assert.That(awareness.IndicatorVisible, Is.False); Assert.That(awareness.TargetPlateVisible, Is.False); Assert.That(rig.HasCombatFocus, Is.False);
                invader.SetController(null); invader.SetController(brain); brain.Tick(); RefreshAwareness(awareness);
                Assert.That(awareness.HasEligibleThreat, Is.False, "Later invader intent cannot reactivate after release without a direct-controlled defender.");
            }
            finally
            {
                GameplayInput.ResetForTests(); Time.timeScale = 1; Time.fixedDeltaTime = .02f;
                Object.Destroy(hudObject); Object.Destroy(managerObject); Object.Destroy(cameraObject); Object.Destroy(defenderObject); Object.Destroy(alternateObject); Object.Destroy(invaderObject); Object.Destroy(ground);
                Object.Destroy(defenderDefinition); Object.Destroy(alternateDefinition); Object.Destroy(invaderDefinition); Object.Destroy(attack);
            }
        }

        [UnityTest]
        public IEnumerator AbilityReadiness_RaidHudReflectsAuthoritativeCooldownAndActionState()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); cameraObject.tag = "MainCamera";
            var heroObject = new GameObject("Raid Hero", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController));
            var coreObject = new GameObject("Heart Tree", typeof(RealmCore));
            var raidObject = new GameObject("Raid Manager", typeof(RaidManager));
            var hudObject = new GameObject("Raid HUD", typeof(RaidHUD));
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var first = ScriptableObject.CreateInstance<AbilityDefinition>(); var second = ScriptableObject.CreateInstance<AbilityDefinition>(); var third = ScriptableObject.CreateInstance<AbilityDefinition>();
            var initialCanvasCount = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
            var initialEventSystemCount = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
            var initialListenerCount = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
            GameplayInput.SetTerminalState(false);
            try
            {
                first.DisplayName = "Slash"; first.Kind = AbilityKind.Melee; first.Windup = .05f; first.Cooldown = .3f;
                second.DisplayName = "Blood Rush"; second.Kind = AbilityKind.Dash; second.Windup = .08f; second.Cooldown = .3f; second.DashDistance = 2;
                third.DisplayName = "Cleave"; third.Kind = AbilityKind.Area; third.Windup = .05f; third.Cooldown = .3f;
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3 };
                definition.Abilities = new[] { first, second, third };
                var hero = heroObject.GetComponent<CombatEntity>(); hero.Initialize(definition); var player = hero.Controller<PlayerController>(); hero.SetController(player);
                var core = coreObject.GetComponent<RealmCore>(); core.Initialize(hero);
                var raid = raidObject.GetComponent<RaidManager>(); raid.Initialize(hero, System.Array.Empty<RealmNodeView>(), System.Array.Empty<CombatEntity>());
                var hud = hudObject.GetComponent<RaidHUD>(); hud.Initialize(raid, hero, core, cameraObject.GetComponent<Camera>());
                yield return null;

                Assert.That(hud.AbilityButtonText(0), Is.EqualTo("SLASH"));
                Assert.That(hud.AbilityButtonText(1), Is.EqualTo("BLOOD RUSH"));
                Assert.That(hud.AbilityButtonText(2), Is.EqualTo("CLEAVE"));
                Assert.That(hud.AbilityButtonInteractable(0), Is.True);
                Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(initialCanvasCount + 1));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(initialEventSystemCount));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(initialListenerCount));

                Assert.That(player.UseAbility(0), Is.True);
                hud.SendMessage("RefreshAbilityButtons", SendMessageOptions.RequireReceiver);
                Assert.That(hud.AbilityButtonText(0), Does.Contain("ACTING"));
                Assert.That(hud.AbilityButtonText(1), Does.Contain("ACTING"));
                Assert.That(hud.AbilityButtonInteractable(0), Is.False);
                Assert.That(player.UseAbility(1), Is.False, "Windup must reject rather than queue.");
                Assert.That(player.HasBufferedAbility, Is.False);

                yield return WaitForActionPhase(hero, CombatActionPhase.Recovery);
                hud.SendMessage("RefreshAbilityButtons", SendMessageOptions.RequireReceiver);
                Assert.That(hud.AbilityButtonText(0), Does.StartWith("SLASH  "));
                Assert.That(hud.AbilityButtonText(1), Is.EqualTo("BLOOD RUSH — NEXT"));
                Assert.That(hud.AbilityButtonInteractable(1), Is.True);
                Assert.That(player.UseAbility(0), Is.False, "The current ability cooldown must not enter the buffer.");
                Assert.That(player.HasBufferedAbility, Is.False);
                var bloodRushReadyAt = hero.Abilities[1].ReadyAt;
                hudObject.transform.Find("BLOOD RUSH").GetComponent<Button>().onClick.Invoke();
                hud.SendMessage("RefreshAbilityButtons", SendMessageOptions.RequireReceiver);
                Assert.That(player.IsAbilityBuffered(1), Is.True);
                Assert.That(hero.Abilities[1].ReadyAt, Is.EqualTo(bloodRushReadyAt), "Queueing cannot consume cooldown early.");
                Assert.That(hud.AbilityButtonText(1), Is.EqualTo("BLOOD RUSH — QUEUED"));
                Assert.That(hud.AbilityButtonInteractable(1), Is.False);
                Assert.That(hud.AbilityButtonText(2), Is.EqualTo("CLEAVE — NEXT"));
                var cleaveReadyAt = hero.Abilities[2].ReadyAt;
                hudObject.transform.Find("CLEAVE").GetComponent<Button>().onClick.Invoke();
                hud.SendMessage("RefreshAbilityButtons", SendMessageOptions.RequireReceiver);
                Assert.That(player.IsAbilityBuffered(2), Is.True, "The newest eligible request replaces the previous one.");
                Assert.That(hud.AbilityButtonText(2), Is.EqualTo("CLEAVE — QUEUED"));
                Assert.That(hud.AbilityButtonText(1), Is.EqualTo("BLOOD RUSH — NEXT"));
                hudObject.transform.Find("BLOOD RUSH").GetComponent<Button>().onClick.Invoke();
                hud.SendMessage("RefreshAbilityButtons", SendMessageOptions.RequireReceiver);
                Assert.That(player.IsAbilityBuffered(1), Is.True);

                yield return WaitForAbilityConsumption(hero.Abilities[1], bloodRushReadyAt);
                var consumedBloodRushReadyAt = hero.Abilities[1].ReadyAt;
                Assert.That(player.HasBufferedAbility, Is.False);
                Assert.That(hero.Abilities[2].ReadyAt, Is.EqualTo(cleaveReadyAt), "The replaced request must never execute.");
                hud.SendMessage("RefreshAbilityButtons", SendMessageOptions.RequireReceiver);
                Assert.That(hud.AbilityButtonText(0), Does.Contain("ACTING"));
                yield return WaitForActionPhase(hero, CombatActionPhase.Impact);
                Assert.That(player.RequestAbility(2, Vector3.left), Is.False, "Impact must reject rather than queue.");
                Assert.That(player.HasBufferedAbility, Is.False);
                yield return WaitForActionPhase(hero, CombatActionPhase.Idle);
                yield return new WaitForSecondsRealtime(.05f);
                Assert.That(hero.Abilities[1].ReadyAt, Is.EqualTo(consumedBloodRushReadyAt), "A consumed request must not execute twice.");

                Assert.That(player.UseAbility(2), Is.True);
                yield return WaitForActionPhase(hero, CombatActionPhase.Recovery);
                var slashReadyAt = hero.Abilities[0].ReadyAt;
                Assert.That(player.RequestAbility(0, Vector3.right), Is.True);
                Assert.That(player.IsAbilityBuffered(0), Is.True);
                GameplayInput.SetTerminalState(true);
                player.Tick();
                Assert.That(player.HasBufferedAbility, Is.False);
                GameplayInput.SetTerminalState(false);
                yield return WaitForActionPhase(hero, CombatActionPhase.Idle);
                Assert.That(hero.Abilities[0].ReadyAt, Is.EqualTo(slashReadyAt), "Terminal cleanup must prevent stale execution.");

                Assert.That(player.UseAbility(0), Is.True);
                yield return WaitForActionPhase(hero, CombatActionPhase.Recovery);
                var secondReadyAtBeforeReset = hero.Abilities[1].ReadyAt;
                Assert.That(player.RequestAbility(1, Vector3.left), Is.True);
                hud.GetComponent<ResponsiveHudRoot>().SetOrientationForTests(PrototypeOrientation.Portrait);
                player.Tick();
                Assert.That(player.HasBufferedAbility, Is.False);
                yield return WaitForActionPhase(hero, CombatActionPhase.Idle);
                Assert.That(hero.Abilities[1].ReadyAt, Is.EqualTo(secondReadyAtBeforeReset), "Interaction reset must prevent stale execution.");

                Assert.That(player.UseAbility(2), Is.True);
                yield return WaitForActionPhase(hero, CombatActionPhase.Recovery);
                var expiringReadyAt = hero.Abilities[1].ReadyAt;
                Assert.That(player.RequestAbility(1, Vector3.back), Is.True);
                hero.enabled = false;
                yield return new WaitForSecondsRealtime(CombatInputBuffer.WindowSeconds + .03f);
                player.Tick();
                Assert.That(player.HasBufferedAbility, Is.False);
                Assert.That(hero.Abilities[1].ReadyAt, Is.EqualTo(expiringReadyAt), "An expired request must not execute.");
                hero.enabled = true;

                yield return WaitForActionPhase(hero, CombatActionPhase.Idle);
                Assert.That(player.UseAbility(0), Is.True);
                yield return WaitForActionPhase(hero, CombatActionPhase.Recovery);
                var deathReadyAt = hero.Abilities[1].ReadyAt;
                Assert.That(player.RequestAbility(1, Vector3.forward), Is.True);
                hero.Health.TakeDamage(new DamageInfo(1000, null, hero.transform.position), 0);
                Assert.That(hero.Health.IsDead, Is.True);
                Assert.That(player.HasBufferedAbility, Is.False);
                Assert.That(hero.Abilities[1].ReadyAt, Is.EqualTo(deathReadyAt), "Death cleanup must prevent stale execution.");

                hero.SetController(null);
                yield return null;
                Assert.That(hud.AbilityButtonText(0), Is.EqualTo("SLASH"));
                Assert.That(hud.AbilityButtonInteractable(0), Is.False);
            }
            finally
            {
                GameplayInput.SetTerminalState(false);
                Object.Destroy(hudObject); Object.Destroy(raidObject); Object.Destroy(coreObject); Object.Destroy(heroObject); Object.Destroy(cameraObject);
                Object.Destroy(definition); Object.Destroy(first); Object.Destroy(second); Object.Destroy(third);
            }
        }

        [UnityTest]
        public IEnumerator AbilityReadiness_PrototypeHudBindsPossessionAndClearsOnRelease()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig)); cameraObject.tag = "MainCamera";
            var heroObject = new GameObject("Blood Knight", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
            var entObject = new GameObject("Guardian Ent", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
            var managerObject = new GameObject("Possession Manager", typeof(PossessionManager));
            var directorObject = new GameObject("Sandbox Director", typeof(SandboxDirector));
            var hudObject = new GameObject("Prototype HUD", typeof(PrototypeHUD));
            var heroDefinition = ScriptableObject.CreateInstance<CharacterDefinition>(); var entDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var smash = ScriptableObject.CreateInstance<AbilityDefinition>(); var charge = ScriptableObject.CreateInstance<AbilityDefinition>(); var slam = ScriptableObject.CreateInstance<AbilityDefinition>();
            GameplayInput.SetTerminalState(false);
            try
            {
                heroDefinition.DisplayName = "Blood Knight"; heroDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 4 };
                entDefinition.DisplayName = "Guardian Ent"; entDefinition.Possessable = true; entDefinition.Stats = new CombatStats { MaxHealth = 120, MoveSpeed = 3 };
                smash.DisplayName = "Smash"; smash.Windup = .05f; smash.Cooldown = .3f;
                charge.DisplayName = "Charge"; charge.Windup = .05f; charge.Cooldown = .3f;
                slam.DisplayName = "Ground Slam"; slam.Windup = .08f; slam.Cooldown = .3f;
                entDefinition.Abilities = new[] { smash, charge, slam };
                var hero = heroObject.GetComponent<CombatEntity>(); hero.Initialize(heroDefinition);
                var ent = entObject.GetComponent<CombatEntity>(); ent.Initialize(entDefinition);
                var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.SnapToOverview();
                var possession = managerObject.GetComponent<PossessionManager>(); possession.Initialize(rig); possession.Register(ent);
                var director = directorObject.GetComponent<SandboxDirector>(); director.Initialize(hero, ent, possession, rig);
                var hud = hudObject.GetComponent<PrototypeHUD>(); hud.Initialize(possession, director, hero, ent);
                possession.Select(ent); Assert.That(possession.PossessSelected(), Is.True);
                yield return null;

                Assert.That(hud.AbilityButtonText(0), Is.EqualTo("SMASH"));
                Assert.That(hud.AbilityButtonText(1), Is.EqualTo("GROUND SLAM"));
                Assert.That(hud.AbilityButtonInteractable(0), Is.True);
                var player = ent.Controller<PlayerController>();
                Assert.That(player.IsActive, Is.True);

                Assert.That(player.UseAbility(0), Is.True);
                yield return WaitForActionPhase(ent, CombatActionPhase.Recovery);
                hud.SendMessage("RefreshAbilityButtons", SendMessageOptions.RequireReceiver);
                Assert.That(hud.AbilityButtonText(1), Is.EqualTo("GROUND SLAM — NEXT"));
                var slamReadyAt = ent.Abilities[2].ReadyAt;
                hudObject.transform.Find("GROUND SLAM").GetComponent<Button>().onClick.Invoke();
                hud.SendMessage("RefreshAbilityButtons", SendMessageOptions.RequireReceiver);
                Assert.That(player.IsAbilityBuffered(2), Is.True);
                Assert.That(hud.AbilityButtonText(1), Is.EqualTo("GROUND SLAM — QUEUED"));
                yield return WaitForAbilityConsumption(ent.Abilities[2], slamReadyAt);
                var consumedSlamReadyAt = ent.Abilities[2].ReadyAt;
                yield return WaitForActionPhase(ent, CombatActionPhase.Idle);
                Assert.That(ent.Abilities[2].ReadyAt, Is.EqualTo(consumedSlamReadyAt));

                Assert.That(player.UseAbility(0), Is.True);
                yield return WaitForActionPhase(ent, CombatActionPhase.Recovery);
                var chargeReadyAt = ent.Abilities[1].ReadyAt;
                Assert.That(player.RequestAbility(1, Vector3.right), Is.True);
                Assert.That(player.IsAbilityBuffered(1), Is.True);

                possession.Release();
                yield return null;
                Assert.That(hud.AbilityButtonText(0), Is.EqualTo("SMASH"));
                Assert.That(hud.AbilityButtonInteractable(0), Is.False);
                Assert.That(player.IsActive, Is.False);
                Assert.That(player.HasBufferedAbility, Is.False);
                yield return new WaitForSecondsRealtime(.2f);
                Assert.That(ent.Abilities[1].ReadyAt, Is.EqualTo(chargeReadyAt), "Possession release must not execute a stale swipe request.");
            }
            finally
            {
                GameplayInput.SetTerminalState(false);
                Object.Destroy(hudObject); Object.Destroy(directorObject); Object.Destroy(managerObject); Object.Destroy(entObject); Object.Destroy(heroObject); Object.Destroy(cameraObject);
                Object.Destroy(heroDefinition); Object.Destroy(entDefinition); Object.Destroy(smash); Object.Destroy(charge); Object.Destroy(slam);
            }
        }

        [UnityTest]
        public IEnumerator RaidObjectiveCompass_MapsCoreProjectionAndCleansUpWithoutInputArtifacts()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); cameraObject.tag = "MainCamera";
            var heroObject = new GameObject("Raid Hero", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var coreObject = new GameObject("Heart Tree", typeof(RealmCore));
            var raidObject = new GameObject("Raid Manager", typeof(RaidManager));
            var hudObject = new GameObject("Raid HUD", typeof(RaidHUD));
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var initialCanvasCount = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
            var initialEventSystemCount = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
            var initialListenerCount = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
            try
            {
                var camera = cameraObject.GetComponent<Camera>(); camera.transform.position = new Vector3(0, 2, -10); camera.transform.rotation = Quaternion.identity; camera.fieldOfView = 60; camera.aspect = 1;
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3 };
                var hero = heroObject.GetComponent<CombatEntity>(); hero.Initialize(definition);
                var core = coreObject.GetComponent<RealmCore>(); core.Initialize(hero);
                var raid = raidObject.GetComponent<RaidManager>(); raid.Initialize(hero, new RealmNodeView[0], new CombatEntity[0]);
                var hud = hudObject.GetComponent<RaidHUD>(); hud.Initialize(raid, hero, core, camera);

                coreObject.transform.position = new Vector3(-30, 0, 20); hud.SendMessage("UpdateObjectiveCompass", SendMessageOptions.RequireReceiver); yield return new WaitForEndOfFrame();
                Assert.That(camera.WorldToViewportPoint(coreObject.transform.position + Vector3.up * 2).x, Is.LessThan(0));
                Assert.That(hud.ObjectiveCompassVisible, Is.True); Assert.That(hud.ObjectiveCompassDirection, Is.EqualTo(-1)); Assert.That(hud.ObjectiveCompassRaycastTarget, Is.False);

                coreObject.transform.position = new Vector3(30, 0, 20); hud.SendMessage("UpdateObjectiveCompass", SendMessageOptions.RequireReceiver); yield return new WaitForEndOfFrame();
                Assert.That(camera.WorldToViewportPoint(coreObject.transform.position + Vector3.up * 2).x, Is.GreaterThan(1));
                Assert.That(hud.ObjectiveCompassVisible, Is.True); Assert.That(hud.ObjectiveCompassDirection, Is.EqualTo(1));

                coreObject.transform.position = new Vector3(0, 0, 20); hud.SendMessage("UpdateObjectiveCompass", SendMessageOptions.RequireReceiver); yield return new WaitForEndOfFrame();
                Assert.That(hud.ObjectiveCompassVisible, Is.False);

                coreObject.transform.position = new Vector3(-30, 0, 20); hud.SendMessage("UpdateObjectiveCompass", SendMessageOptions.RequireReceiver); yield return new WaitForEndOfFrame();
                hud.SetObjectiveProgress(.25f); hud.SendMessage("UpdateObjectiveCompass", SendMessageOptions.RequireReceiver); yield return new WaitForEndOfFrame(); Assert.That(hud.ObjectiveCompassVisible, Is.False);
                hud.SetObjectiveProgress(0); hud.SendMessage("UpdateObjectiveCompass", SendMessageOptions.RequireReceiver); yield return new WaitForEndOfFrame(); Assert.That(hud.ObjectiveCompassVisible, Is.True);
                GameplayInput.SetTerminalState(true); hud.SendMessage("UpdateObjectiveCompass", SendMessageOptions.RequireReceiver); yield return new WaitForEndOfFrame(); Assert.That(hud.ObjectiveCompassVisible, Is.False);
                Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(initialCanvasCount + 1));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(initialEventSystemCount));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(initialListenerCount));
            }
            finally
            {
                GameplayInput.SetTerminalState(false);
                Object.Destroy(hudObject); Object.Destroy(raidObject); Object.Destroy(coreObject); Object.Destroy(heroObject); Object.Destroy(cameraObject); Object.Destroy(definition);
            }
        }

        [UnityTest]
        public IEnumerator RaidResult_PlanNextDefenseClosesLoopWithoutPresentationArtifacts()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); cameraObject.tag = "MainCamera";
            var eventSystemObject = new GameObject("Event System", typeof(EventSystem));
            var heroObject = new GameObject("Raid Hero", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
            var coreObject = new GameObject("Heart Tree", typeof(RealmCore));
            var raidObject = new GameObject("Raid Manager", typeof(RaidManager));
            var hudObject = new GameObject("Raid HUD", typeof(RaidHUD));
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var initialCanvasCount = Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None).Length;
            var initialEventSystemCount = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;
            var initialListenerCount = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
            var hadProgress = PlayerPrefs.HasKey(RealmProgress.KeyForTests);
            var previousProgress = PlayerPrefs.GetString(RealmProgress.KeyForTests, string.Empty);
            try
            {
                RealmProgress.ResetForTests();
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3 };
                var hero = heroObject.GetComponent<CombatEntity>(); hero.Initialize(definition);
                var core = coreObject.GetComponent<RealmCore>(); core.Initialize(hero);
                var raid = raidObject.GetComponent<RaidManager>(); raid.Initialize(hero, System.Array.Empty<RealmNodeView>(), System.Array.Empty<CombatEntity>());
                var hud = hudObject.GetComponent<RaidHUD>(); hud.Initialize(raid, hero, core, cameraObject.GetComponent<Camera>());
                hud.SendMessage("ShowResult", new RaidResult(true, 115, 2, 4, 3, 46.8f, true), SendMessageOptions.RequireReceiver);
                yield return null;

                Assert.That(hud.ResultPanelVisible, Is.True);
                Assert.That(hud.ResultText, Does.Contain("The Heart Tree fell").And.Contain("115").And.Contain("2").And.Contain("4").And.Contain("3").And.Contain("47s").And.Contain("yes"));
                var credited = RealmProgress.Load();
                Assert.That(credited.Gold, Is.EqualTo(115)); Assert.That(credited.RareMaterials, Is.EqualTo(2)); Assert.That(credited.CompletedRaids, Is.EqualTo(1)); Assert.That(credited.Victories, Is.EqualTo(1));
                hud.SendMessage("ShowResult", new RaidResult(true, 115, 2, 4, 3, 46.8f, true), SendMessageOptions.RequireReceiver);
                Assert.That(RealmProgress.Load().CompletedRaids, Is.EqualTo(1), "A result refresh must not duplicate stored rewards.");
                var actions = new[] { GameObject.Find(RaidHUD.PlanNextDefenseAction).GetComponent<UnityEngine.UI.Button>(), GameObject.Find("RAID AGAIN").GetComponent<UnityEngine.UI.Button>(), GameObject.Find("MY REALM").GetComponent<UnityEngine.UI.Button>() };
                foreach (var action in actions) Assert.That(action.GetComponent<UiPointerOwnership>(), Is.Not.Null);
                Assert.That(Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None), Has.Length.EqualTo(initialCanvasCount + 1));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(initialEventSystemCount));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(initialListenerCount));

                var root = hud.GetComponent<ResponsiveHudRoot>();
                root.SetOrientationForTests(PrototypeOrientation.Portrait); yield return null; AssertResultActionsClear((RectTransform)GameObject.Find("Raid Result").transform, actions);
                root.SetOrientationForTests(PrototypeOrientation.Landscape); yield return null; AssertResultActionsClear((RectTransform)GameObject.Find("Raid Result").transform, actions);

                actions[0].onClick.Invoke();
                yield return null; yield return null;
                Assert.That(SceneManager.GetActiveScene().name, Is.EqualTo(RaidHUD.PlanNextDefenseScene));
                var build = Object.FindFirstObjectByType<BuildHUD>(); Assert.That(build, Is.Not.Null); Assert.That(build.RealmStoresText, Is.EqualTo("REALM STORES  •  115 GOLD  •  2 RARE MATERIALS"));
                var buildStores = GameObject.Find("Realm Stores").GetComponent<Text>(); Assert.That(buildStores.raycastTarget, Is.False);
                SceneManager.LoadScene("PrototypeHub"); yield return null; yield return null;
                var hub = Object.FindFirstObjectByType<HubHUD>(); Assert.That(hub, Is.Not.Null); Assert.That(hub.RealmStoresText, Is.EqualTo("REALM STORES  •  115 GOLD  •  2 RARE MATERIALS"));
                Assert.That(GameObject.Find("Realm Stores").GetComponent<Text>().raycastTarget, Is.False);
            }
            finally
            {
                GameplayInput.SetTerminalState(false);
                if (hadProgress) PlayerPrefs.SetString(RealmProgress.KeyForTests, previousProgress); else PlayerPrefs.DeleteKey(RealmProgress.KeyForTests); PlayerPrefs.Save();
                Object.Destroy(hudObject); Object.Destroy(raidObject); Object.Destroy(coreObject); Object.Destroy(heroObject); Object.Destroy(eventSystemObject); Object.Destroy(cameraObject); Object.Destroy(definition);
            }
        }

        static void AssertResultActionsClear(RectTransform panel, UnityEngine.UI.Button[] actions)
        {
            var rectangles = new Rect[actions.Length];
            for (var i = 0; i < actions.Length; i++)
            {
                var corners = new Vector3[4]; ((RectTransform)actions[i].transform).GetWorldCorners(corners);
                var min = panel.InverseTransformPoint(corners[0]); var max = panel.InverseTransformPoint(corners[2]); rectangles[i] = Rect.MinMaxRect(min.x, min.y, max.x, max.y);
                Assert.That(panel.rect.Contains(rectangles[i].min) && panel.rect.Contains(rectangles[i].max), Is.True, $"Result action outside panel: {actions[i].name}");
            }
            for (var i = 0; i < rectangles.Length; i++) for (var j = i + 1; j < rectangles.Length; j++) Assert.That(rectangles[i].Overlaps(rectangles[j]), Is.False, $"Result actions overlap: {actions[i].name}/{actions[j].name}");
        }

        static Image AssertDefenseRealmIdentityPresentation(DefenderHUD hud, bool sylvan, string expectedStateCopy)
        {
            var marks = hud.GetComponentsInChildren<Image>(true)
                .Where(image => image.name == HudPresentation.RealmIdentityIconName).ToArray();
            Assert.That(marks, Has.Length.EqualTo(1), "Defense HUD must own exactly one realm identity mark.");
            var mark = marks[0];
            var resource = sylvan ? HudPresentation.SylvanRealmIdentityIconResource : HudPresentation.InfernalRealmIdentityIconResource;
            var otherResource = sylvan ? HudPresentation.InfernalRealmIdentityIconResource : HudPresentation.SylvanRealmIdentityIconResource;
            var expected = Resources.Load<Sprite>(resource);
            var other = Resources.Load<Sprite>(otherResource);
            Assert.That(expected, Is.Not.Null);
            Assert.That(other, Is.Not.Null);
            Assert.That(mark.sprite, Is.SameAs(expected));
            Assert.That(mark.sprite, Is.Not.SameAs(other));
            Assert.That(mark.raycastTarget, Is.False);
            Assert.That(mark.preserveAspect, Is.True);
            Assert.That(mark.rectTransform.sizeDelta, Is.EqualTo(new Vector2(48, 48)));
            Assert.That(mark.GetComponent<Button>(), Is.Null);
            Assert.That(mark.GetComponent<EventTrigger>(), Is.Null);
            Assert.That(mark.GetComponent<UiPointerOwnership>(), Is.Null);
            var label = mark.GetComponentInParent<Text>();
            Assert.That(label, Is.Not.Null);
            Assert.That(label.text, Is.EqualTo(expectedStateCopy));
            Assert.That(mark.rectTransform.anchoredPosition.x, Is.GreaterThanOrEqualTo(label.preferredWidth * .5f + 7.9f), "Realm identity mark must not cover defense state copy.");
            foreach (var button in hud.GetComponentsInChildren<Button>(false))
                Assert.That(WorldRect(mark.rectTransform).Overlaps(WorldRect((RectTransform)button.transform)), Is.False, $"Realm identity mark overlaps {button.name}.");
            return mark;
        }

        static void AssertThemedAbilityPresentation(DefenderHUD hud, PrototypeOrientation orientation, bool chargeVisible, bool guardian)
        {
            var prefix = guardian ? HudPresentation.GuardianEntAbilityIconNamePrefix : HudPresentation.InfernalBruteAbilityIconNamePrefix;
            var smashResource = guardian ? HudPresentation.GuardianEntSmashIconResource : HudPresentation.InfernalBruteSmashIconResource;
            var chargeResource = guardian ? HudPresentation.GuardianEntChargeIconResource : HudPresentation.InfernalBruteChargeIconResource;
            var slamResource = guardian ? HudPresentation.GuardianEntGroundSlamIconResource : HudPresentation.InfernalBruteGroundSlamIconResource;
            var icons = AbilityIcons(hud.transform, true, prefix);
            Assert.That(icons, Has.Length.EqualTo(3), "The active defender family must contain exactly its accepted icon trio.");
            var smashButton = hud.transform.Find("SMASH");
            var slamButton = hud.transform.Find("GROUND SLAM");
            var chargeAffordance = hud.ChargeAffordanceRect;
            Assert.That(smashButton, Is.Not.Null);
            Assert.That(slamButton, Is.Not.Null);
            Assert.That(chargeAffordance, Is.Not.Null);
            Assert.That(chargeAffordance.name, Is.EqualTo(guardian ? "Guardian Ent Charge Affordance" : "Infernal Brute Charge Affordance"));
            Assert.That(hud.ChargeAffordanceText, Is.EqualTo("SWIPE: CHARGE"));
            Assert.That(hud.ChargeAffordanceVisible, Is.EqualTo(chargeVisible));
            Assert.That(chargeAffordance.GetComponent<Button>(), Is.Null);
            Assert.That(chargeAffordance.GetComponent<UiPointerOwnership>(), Is.Null);
            Assert.That(chargeAffordance.GetComponentInChildren<Text>().raycastTarget, Is.False);

            var smashIcon = smashButton.Find(prefix + "0").GetComponent<Image>();
            var chargeIcon = chargeAffordance.Find(prefix + "1").GetComponent<Image>();
            var slamIcon = slamButton.Find(prefix + "2").GetComponent<Image>();
            AssertThemedIcon(smashIcon, smashButton.GetComponentInChildren<Text>(), smashResource);
            AssertThemedIcon(chargeIcon, chargeAffordance.GetComponentInChildren<Text>(), chargeResource);
            AssertThemedIcon(slamIcon, slamButton.GetComponentInChildren<Text>(), slamResource);
            Assert.That(smashIcon.gameObject.activeInHierarchy, Is.True);
            Assert.That(slamIcon.gameObject.activeInHierarchy, Is.True);
            Assert.That(chargeIcon.gameObject.activeInHierarchy, Is.EqualTo(chargeVisible));

            Assert.That(chargeAffordance.anchorMin, Is.EqualTo(orientation == PrototypeOrientation.Portrait ? new Vector2(.5f, 0) : new Vector2(1, 0)));
            Assert.That(chargeAffordance.anchorMax, Is.EqualTo(chargeAffordance.anchorMin));
            Assert.That(chargeAffordance.pivot, Is.EqualTo(orientation == PrototypeOrientation.Portrait ? new Vector2(.5f, 0) : new Vector2(1, 0)));
            Assert.That(chargeAffordance.anchoredPosition, Is.EqualTo(orientation == PrototypeOrientation.Portrait ? new Vector2(355, 410) : new Vector2(-70, 434)));
            Assert.That(chargeAffordance.sizeDelta, Is.EqualTo(orientation == PrototypeOrientation.Portrait ? new Vector2(300, 64) : new Vector2(340, 64)));

            if (!chargeVisible) return;
            Canvas.ForceUpdateCanvases();
            var chargeBounds = WorldRect(chargeAffordance);
            foreach (var button in hud.GetComponentsInChildren<Button>(true))
            {
                if (!button.gameObject.activeInHierarchy) continue;
                Assert.That(chargeBounds.Overlaps(WorldRect((RectTransform)button.transform)), Is.False,
                    $"Charge affordance overlaps active control {button.name} in {orientation}.");
            }
        }

        static void AssertThemedIcon(Image icon, Text label, string resourcePath)
        {
            Assert.That(icon, Is.Not.Null);
            Assert.That(icon.sprite, Is.SameAs(Resources.Load<Sprite>(resourcePath)));
            Assert.That(icon.raycastTarget, Is.False);
            Assert.That(icon.preserveAspect, Is.True);
            Assert.That(icon.rectTransform.sizeDelta, Is.EqualTo(new Vector2(44, 44)));
            Assert.That(icon.rectTransform.anchoredPosition, Is.EqualTo(new Vector2(10, 0)));
            Assert.That(icon.GetComponent<Button>(), Is.Null);
            Assert.That(icon.GetComponent<EventTrigger>(), Is.Null);
            Assert.That(label, Is.Not.Null);
            Assert.That(label.rectTransform.offsetMin.x, Is.GreaterThanOrEqualTo(58));
            Assert.That(icon.rectTransform.anchoredPosition.x + icon.rectTransform.sizeDelta.x, Is.LessThanOrEqualTo(label.rectTransform.offsetMin.x));
        }

        static Image[] AbilityIcons(Transform root, bool includeInactive, string prefix) => root.GetComponentsInChildren<Image>(includeInactive)
            .Where(image => image.name.StartsWith(prefix)).ToArray();

        static Rect WorldRect(RectTransform rect)
        {
            var corners = new Vector3[4];
            rect.GetWorldCorners(corners);
            return Rect.MinMaxRect(corners[0].x, corners[0].y, corners[2].x, corners[2].y);
        }

        static IEnumerator WaitForActionPhase(CombatEntity entity, CombatActionPhase phase)
        {
            var deadline = Time.realtimeSinceStartup + 2f;
            while (entity && entity.ActionPhase != phase && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(entity.ActionPhase, Is.EqualTo(phase), $"Timed out waiting for {phase}.");
        }

        static void RefreshAwareness(CombatCameraAwareness awareness)
        {
            var lateUpdate = typeof(CombatCameraAwareness).GetMethod("LateUpdate", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(lateUpdate, Is.Not.Null);
            lateUpdate.Invoke(awareness, null);
        }

        static IEnumerator WaitForAbilityConsumption(AbilityRuntime ability, float previousReadyAt)
        {
            var deadline = Time.realtimeSinceStartup + 2f;
            while (ability.ReadyAt <= previousReadyAt && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(ability.ReadyAt, Is.GreaterThan(previousReadyAt), "Timed out waiting for the buffered ability to execute.");
        }

        static int CountNamed(Transform root, string objectName)
        {
            var count = root.name == objectName ? 1 : 0;
            for (var index = 0; index < root.childCount; index++) count += CountNamed(root.GetChild(index), objectName);
            return count;
        }

        sealed class EnergyHudFixture
        {
            public readonly GameObject CameraObject;
            public readonly GameObject InvaderObject;
            public readonly GameObject DefenderObject;
            public readonly GameObject CoreObject;
            public readonly GameObject PossessionObject;
            public readonly GameObject DefenseObject;
            public readonly GameObject TrapObject;
            public readonly GameObject HudObject;
            public readonly CharacterDefinition InvaderDefinition;
            public readonly CharacterDefinition DefenderDefinition;
            public readonly AbilityDefinition SmashDefinition;
            public readonly AbilityDefinition ChargeDefinition;
            public readonly AbilityDefinition GroundSlamDefinition;
            public readonly CombatEntity Invader;
            public readonly CombatEntity Defender;
            public readonly PossessionManager Possession;
            public readonly PossessionEnergy Energy;
            public readonly DefenseManager Defense;
            public readonly DefenderHUD Hud;

            EnergyHudFixture(DefenseHudConfig config)
            {
                CameraObject = new GameObject($"{config.RealmTitle} Energy Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig));
                CameraObject.tag = "MainCamera";
                InvaderObject = new GameObject($"{config.RealmTitle} Invader", typeof(CharacterController), typeof(Health), typeof(CombatEntity));
                DefenderObject = new GameObject(config.DefenderName, typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
                CoreObject = new GameObject(config.CoreName, typeof(RealmCore));
                PossessionObject = new GameObject($"{config.RealmTitle} Possession", typeof(PossessionManager));
                DefenseObject = new GameObject($"{config.RealmTitle} Defense", typeof(DefenseManager));
                TrapObject = new GameObject(config.TrapName, typeof(RootTrap));
                HudObject = new GameObject($"{config.RealmTitle} HUD", typeof(DefenderHUD));
                InvaderDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
                DefenderDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
                SmashDefinition = ScriptableObject.CreateInstance<AbilityDefinition>();
                ChargeDefinition = ScriptableObject.CreateInstance<AbilityDefinition>();
                GroundSlamDefinition = ScriptableObject.CreateInstance<AbilityDefinition>();

                InvaderDefinition.DisplayName = "Test Invader";
                InvaderDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3, AttackSpeed = 1 };
                InvaderDefinition.Abilities = System.Array.Empty<AbilityDefinition>();
                DefenderDefinition.DisplayName = config.DefenderName;
                DefenderDefinition.ArchetypeId = config.RealmTitle == DefenseHudConfig.Sylvan.RealmTitle
                    ? PrototypeCharacterRoster.GuardianEntId
                    : PrototypeCharacterRoster.InfernalBruteId;
                DefenderDefinition.Possessable = true;
                DefenderDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3, AttackSpeed = 1 };
                ConfigureAbility(SmashDefinition, "Smash");
                ConfigureAbility(ChargeDefinition, "Charge");
                ConfigureAbility(GroundSlamDefinition, "Ground Slam");
                DefenderDefinition.Abilities = new[] { SmashDefinition, ChargeDefinition, GroundSlamDefinition };

                Invader = InvaderObject.GetComponent<CombatEntity>();
                Invader.Initialize(InvaderDefinition);
                DefenderObject.transform.position = new Vector3(40, 0, 0);
                Defender = DefenderObject.GetComponent<CombatEntity>();
                Defender.Initialize(DefenderDefinition);
                Defender.SetController(Defender.Controller<CreatureBrain>());

                var rig = CameraObject.GetComponent<PrototypeCameraRig>();
                rig.ConfigureOverview(new Vector3(0, 12, -12), Quaternion.Euler(45, 0, 0));
                rig.SnapToOverview();
                Possession = PossessionObject.GetComponent<PossessionManager>();
                Possession.Initialize(rig);
                Energy = new PossessionEnergy(30);
                Possession.ConfigureEnergy(Energy);
                Possession.Register(Defender);

                CoreObject.transform.position = new Vector3(100, 0, 100);
                var core = CoreObject.GetComponent<RealmCore>();
                core.Initialize(Invader);
                Defense = DefenseObject.GetComponent<DefenseManager>();
                Defense.Initialize(Invader, core, Possession);
                TrapObject.transform.position = new Vector3(100, 0, -100);
                var trap = TrapObject.GetComponent<RootTrap>();
                trap.Automatic = false;
                trap.Initialize(Invader);
                Hud = HudObject.GetComponent<DefenderHUD>();
                Hud.Initialize(Defense, Possession, Energy, Invader, Defender, trap, core, config);
            }

            public static EnergyHudFixture Create(DefenseHudConfig config) => new(config);

            public void Destroy()
            {
                Object.Destroy(HudObject);
                Object.Destroy(TrapObject);
                Object.Destroy(DefenseObject);
                Object.Destroy(PossessionObject);
                Object.Destroy(CoreObject);
                Object.Destroy(DefenderObject);
                Object.Destroy(InvaderObject);
                Object.Destroy(CameraObject);
                Object.Destroy(InvaderDefinition);
                Object.Destroy(DefenderDefinition);
                Object.Destroy(SmashDefinition);
                Object.Destroy(ChargeDefinition);
                Object.Destroy(GroundSlamDefinition);
            }

            static void ConfigureAbility(AbilityDefinition ability, string displayName)
            {
                ability.DisplayName = displayName;
                ability.Kind = AbilityKind.Melee;
                ability.Damage = 0;
                ability.Range = .1f;
                ability.Radius = .1f;
                ability.Windup = .01f;
                ability.Cooldown = .3f;
            }
        }
    }
}
