using System.Collections;
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
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;

namespace RealmRaiders.Tests
{
    public sealed class PossessionFlowTests
    {
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
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>(); var threatDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            try
            {
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                threatDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                var player = playerObject.GetComponent<CombatEntity>(); var threat = threatObject.GetComponent<CombatEntity>(); player.Initialize(definition); threat.Initialize(threatDefinition);
                threatObject.transform.position = new Vector3(9, 0, 2);
                var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.SnapTo(player, CameraMode.HeroCombat);
                player.SetController(playerObject.GetComponent<PlayerController>());
                var awareness = cameraObject.GetComponent<CombatCameraAwareness>(); awareness.SetControlled(player); awareness.ReportThreat(threat);
                yield return null; yield return null;
                Assert.That(awareness.HasEligibleThreat, Is.True);
                Assert.That(rig.HasRequestedCombatFocus, Is.True);

                threatObject.transform.position = new Vector3(30, 0, 2);
                yield return null;
                Assert.That(awareness.HasEligibleThreat, Is.False);
                Assert.That(awareness.IndicatorVisible, Is.False);
                Assert.That(rig.HasCombatFocus, Is.False);

                awareness.ReportThreat(threat); threatObject.transform.position = new Vector3(9, 0, 2); yield return null;
                GameplayInput.SetTerminalState(true); yield return null;
                Assert.That(awareness.HasEligibleThreat, Is.False);
                Assert.That(awareness.IndicatorVisible, Is.False);
                GameplayInput.SetTerminalState(false);
            }
            finally { GameplayInput.SetTerminalState(false); Object.Destroy(cameraObject); Object.Destroy(playerObject); Object.Destroy(threatObject); Object.Destroy(definition); Object.Destroy(threatDefinition); }
        }

        [UnityTest]
        public IEnumerator CombatCameraAwareness_TracksActiveCreatureIntentAcrossEdgesAndCleansUp()
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig), typeof(CombatCameraAwareness)); cameraObject.tag = "MainCamera";
            var playerObject = new GameObject("Player", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(PlayerController));
            var threatObject = new GameObject("Hostile Creature", typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(CreatureBrain));
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>(); var threatDefinition = ScriptableObject.CreateInstance<CharacterDefinition>();
            try
            {
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                threatDefinition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 1 };
                var player = playerObject.GetComponent<CombatEntity>(); var threat = threatObject.GetComponent<CombatEntity>();
                player.Initialize(definition); threat.Initialize(threatDefinition);
                var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.SnapTo(player, CameraMode.HeroCombat);
                var testView = cameraObject.GetComponent<Camera>(); testView.fieldOfView = 1f; testView.aspect = .5f;
                // Keep the camera basis fixed while testing the presentation indicator's real left/right mapping.
                rig.enabled = false;
                player.SetController(playerObject.GetComponent<PlayerController>());
                var awareness = cameraObject.GetComponent<CombatCameraAwareness>(); awareness.SetControlled(player);
                var brain = threatObject.GetComponent<CreatureBrain>(); brain.DetectionRange = 14f; threat.SetController(brain);

                // A narrow local test projection makes this a genuine left/right edge case on every host viewport.
                var horizontalRight = Vector3.ProjectOnPlane(cameraObject.transform.right, Vector3.up).normalized;
                var rightEdge = playerObject.transform.position + horizontalRight * 10f;
                threatObject.transform.position = rightEdge; brain.Target = player; brain.Tick();
                // Awareness updates its indicator in LateUpdate, so assert after that phase.
                yield return new WaitForEndOfFrame();
                Assert.That(brain.State, Is.EqualTo(BrainState.Chase).Or.EqualTo(BrainState.Attack));
                Assert.That(awareness.HasEligibleThreat, Is.True);
                Assert.That(rig.HasRequestedCombatFocus, Is.True);
                Assert.That(awareness.IndicatorVisible, Is.True);
                var rightExpected = CombatCameraAwareness.IndicatorDirectionFor(testView.WorldToViewportPoint(threatObject.transform.position + Vector3.up), testView.transform.right, threatObject.transform.position - testView.transform.position);
                Assert.That(awareness.IndicatorDirection, Is.EqualTo(rightExpected));
                var indicator = cameraObject.GetComponentInChildren<UnityEngine.UI.Text>(true);
                Assert.That(indicator, Is.Not.Null); Assert.That(indicator.raycastTarget, Is.False);
                Assert.That(cameraObject.GetComponentsInChildren<UnityEngine.UI.Text>(true), Has.Length.EqualTo(1));

                brain.Target = null; brain.Tick();
                yield return new WaitForEndOfFrame();
                horizontalRight = Vector3.ProjectOnPlane(cameraObject.transform.right, Vector3.up).normalized;
                var leftEdge = playerObject.transform.position - horizontalRight * 10f;
                threatObject.transform.position = leftEdge; brain.Target = player; brain.Tick();
                yield return new WaitForEndOfFrame();
                Assert.That(awareness.IndicatorVisible, Is.True);
                var leftExpected = CombatCameraAwareness.IndicatorDirectionFor(testView.WorldToViewportPoint(threatObject.transform.position + Vector3.up), testView.transform.right, threatObject.transform.position - testView.transform.position);
                Assert.That(awareness.IndicatorDirection, Is.EqualTo(leftExpected));
                Assert.That(leftExpected, Is.Not.EqualTo(rightExpected));

                threatObject.transform.position = playerObject.transform.position + Vector3.forward * 2f; brain.Tick();
                yield return new WaitForEndOfFrame();
                Assert.That(brain.State, Is.EqualTo(BrainState.Attack));
                Assert.That(awareness.HasEligibleThreat, Is.True);

                brain.Target = null; brain.Tick();
                yield return new WaitForEndOfFrame();
                Assert.That(awareness.HasEligibleThreat, Is.False);
                Assert.That(awareness.IndicatorVisible, Is.False);
                Assert.That(rig.HasCombatFocus, Is.False);

                horizontalRight = Vector3.ProjectOnPlane(cameraObject.transform.right, Vector3.up).normalized;
                rightEdge = playerObject.transform.position + horizontalRight * 10f;
                threatObject.transform.position = rightEdge; brain.Target = player; brain.Tick();
                yield return new WaitForEndOfFrame();
                GameplayInput.SetTerminalState(true); yield return new WaitForEndOfFrame();
                Assert.That(awareness.HasEligibleThreat, Is.False);
                Assert.That(awareness.IndicatorVisible, Is.False);
                GameplayInput.SetTerminalState(false);

                brain.Target = null; brain.Tick(); brain.Target = player; brain.Tick();
                yield return new WaitForEndOfFrame();
                player.SetController(null); yield return new WaitForEndOfFrame();
                Assert.That(awareness.HasEligibleThreat, Is.False);
                Assert.That(awareness.IndicatorVisible, Is.False);
                Assert.That(rig.HasCombatFocus, Is.False);
            }
            finally { GameplayInput.SetTerminalState(false); Object.Destroy(cameraObject); Object.Destroy(playerObject); Object.Destroy(threatObject); Object.Destroy(definition); Object.Destroy(threatDefinition); }
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
    }
}
