using System.Collections;
using System.Linq;
using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.TestTools;

namespace RealmRaiders.Tests
{
    public sealed class GuardianEntGroundSlamVfxFlowTests
    {
        [UnityTearDown]
        public IEnumerator ResetState()
        {
            GameplayInput.ResetForTests();
            Time.timeScale = 1;
            yield return null;
        }

        [UnityTest]
        public IEnumerator GroundSlam_WhiffAndHitUseOneBoundedFactualPresentation()
        {
            GameplayInput.ResetForTests();
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera";
            var eventObject = new GameObject("Event System", typeof(EventSystem));
            using var ent = new EntityFixture(PrototypeCharacterRoster.GuardianEntId);
            EntityFixture target = null;
            EntityFixture wrong = null;
            try
            {
                var cameraPosition = cameraObject.transform.position;
                var cameraRotation = cameraObject.transform.rotation;
                var rootPosition = ent.Root.transform.position;
                var rootCollider = ent.Root.GetComponent<CharacterController>();
                var health = ent.Entity.Health.Current;
                var cameraCount = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Length;
                var listenerCount = Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Length;
                var eventCount = Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length;

                Assert.That(ent.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return WaitForRing(ent.Feedback);
                var firstRing = ent.Feedback.GroundSlamImpactObject;
                var firstRenderer = firstRing.GetComponent<SpriteRenderer>();
                var sharedMaterial = firstRenderer.sharedMaterial;
                var expectedCenter = rootPosition + Vector3.forward * Mathf.Max(1, ent.Ability.Range * .55f) + Vector3.up * .03f;
                Assert.That(Vector3.Distance(firstRing.transform.position, expectedCenter), Is.LessThan(.001f));
                Assert.That(Mathf.Abs(Vector3.Dot(firstRing.transform.forward, Vector3.up)), Is.GreaterThan(.999f), "The SpriteRenderer must lie horizontally on the factual impact plane.");
                Assert.That(firstRing.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(firstRenderer.shadowCastingMode, Is.EqualTo(ShadowCastingMode.Off));
                Assert.That(firstRenderer.receiveShadows, Is.False);
                Assert.That(firstRenderer.sharedMaterial.shader.name, Is.EqualTo("Universal Render Pipeline/Unlit"));
                Assert.That(firstRenderer.sharedMaterial.GetFloat("_ZWrite"), Is.Zero);
                Assert.That(firstRenderer.sharedMaterial.GetShaderPassEnabled("ShadowCaster"), Is.False);
                Assert.That(firstRenderer.HasPropertyBlock(), Is.True);
                Assert.That(ent.Feedback.GroundSlamImpactMaximumDiameter, Is.EqualTo(ent.Ability.Radius * 2));
                Assert.That(WorldDiameter(firstRenderer), Is.InRange(ent.Ability.Radius * 2 * CombatFeedback.GroundSlamInitialScale, ent.Ability.Radius * 2));
                Assert.That(ent.Feedback.GroundSlamImpactAlpha, Is.InRange(0f, 1f));
                Assert.That(GameObject.Find("Ability Impact"), Is.Null, "A whiff keeps the existing connected-hit pulse absent.");
                Assert.That(ent.Entity.Health.Current, Is.EqualTo(health));
                Assert.That(Vector3.Distance(ent.Root.transform.position, rootPosition), Is.LessThan(.001f));
                Assert.That(Vector3.Dot(ent.Root.transform.forward, Vector3.forward), Is.GreaterThan(.999f));
                Assert.That(rootCollider.enabled, Is.True);
                Assert.That(GameplayInput.Movement, Is.EqualTo(Vector2.zero));

                Assert.That(CombatFeedback.GroundSlamScaleAt(0), Is.EqualTo(.7f).Within(.0001f));
                Assert.That(CombatFeedback.GroundSlamScaleAt(CombatFeedback.GroundSlamScaleDuration), Is.EqualTo(1).Within(.0001f));
                Assert.That(CombatFeedback.GroundSlamAlphaAt(.02f), Is.EqualTo(.5f).Within(.0001f));
                Assert.That(CombatFeedback.GroundSlamAlphaAt(.04f), Is.EqualTo(1).Within(.0001f));
                Assert.That(CombatFeedback.GroundSlamAlphaAt(CombatFeedback.GroundSlamImpactDuration), Is.Zero);
                Assert.That(CombatFeedback.GroundSlamImpactDuration, Is.LessThanOrEqualTo(.35f));

                yield return new WaitForSecondsRealtime(CombatFeedback.GroundSlamScaleDuration + .02f);
                Assert.That(ent.Feedback.GroundSlamImpactVisible, Is.True);
                Assert.That(WorldDiameter(firstRenderer), Is.EqualTo(ent.Ability.Radius * 2).Within(.001f), "The visible settled ring equals, and never exceeds, the factual Area diameter.");

                yield return WaitForIdle(ent.Entity);
                Assert.That(ent.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return WaitForReplacement(ent.Feedback, firstRing);
                var replacement = ent.Feedback.GroundSlamImpactObject;
                Assert.That(ActiveRingCount(), Is.EqualTo(1), "A repeated factual slam replaces rather than stacks its ring.");
                Assert.That(firstRing == null || !firstRing.activeSelf, Is.True);
                Assert.That(replacement.GetComponent<SpriteRenderer>().sharedMaterial, Is.SameAs(sharedMaterial), "Repeated rings reuse one shared material.");

                yield return WaitForIdle(ent.Entity);
                target = new EntityFixture("realmraiders.vfx-test-target");
                target.Root.transform.position = rootPosition + Vector3.forward * Mathf.Max(1, ent.Ability.Range * .55f);
                Physics.SyncTransforms();
                var targetHealth = target.Entity.Health.Current;
                Assert.That(ent.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return WaitForReplacement(ent.Feedback, replacement);
                Assert.That(target.Entity.Health.Current, Is.LessThan(targetHealth));
                Assert.That(GameObject.Find("Ability Impact"), Is.Not.Null, "A connected hit keeps the existing generic impact as a separate object.");
                Assert.That(ActiveRingCount(), Is.EqualTo(1));

                yield return new WaitForSecondsRealtime(CombatFeedback.GroundSlamImpactDuration + .04f);
                Assert.That(ent.Feedback.GroundSlamImpactVisible, Is.False, "The ring must be removed within its 0.34-second presentation plus one bounded frame allowance.");

                yield return WaitForIdle(ent.Entity);
                wrong = new EntityFixture("realmraiders.infernal-brute.prototype");
                wrong.Root.transform.position = Vector3.right * 20;
                Assert.That(wrong.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return new WaitForSecondsRealtime(.06f);
                Assert.That(wrong.Feedback.GroundSlamImpactVisible, Is.False, "Matching ability copy on another archetype must not create Sylvan VFX.");
                Assert.That(ActiveRingCount(), Is.Zero);
                Assert.That(Vector3.Distance(cameraObject.transform.position, cameraPosition), Is.LessThan(.001f));
                Assert.That(Quaternion.Angle(cameraObject.transform.rotation, cameraRotation), Is.LessThan(.001f));
                Assert.That(Object.FindObjectsByType<Camera>(FindObjectsSortMode.None), Has.Length.EqualTo(cameraCount));
                Assert.That(Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None), Has.Length.EqualTo(listenerCount));
                Assert.That(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None), Has.Length.EqualTo(eventCount));
            }
            finally
            {
                target?.Dispose();
                wrong?.Dispose();
                Object.Destroy(eventObject);
                Object.Destroy(cameraObject);
            }
        }

        [UnityTest]
        public IEnumerator GroundSlam_CleansControllerDeathTerminalDisableDestroyAndSceneTeardown()
        {
            GameplayInput.ResetForTests();
            var controller = new EntityFixture(PrototypeCharacterRoster.GuardianEntId);
            var death = new EntityFixture(PrototypeCharacterRoster.GuardianEntId);
            var terminal = new EntityFixture(PrototypeCharacterRoster.GuardianEntId);
            var entityDisable = new EntityFixture(PrototypeCharacterRoster.GuardianEntId);
            var feedbackDisable = new EntityFixture(PrototypeCharacterRoster.GuardianEntId);
            var destroy = new EntityFixture(PrototypeCharacterRoster.GuardianEntId);
            var activeScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            var teardownScene = UnityEngine.SceneManagement.SceneManager.CreateScene("Ground Slam VFX Teardown");
            EntityFixture teardown = null;
            try
            {
                controller.Root.transform.position = Vector3.zero;
                death.Root.transform.position = Vector3.right * 10;
                terminal.Root.transform.position = Vector3.right * 20;
                entityDisable.Root.transform.position = Vector3.right * 30;
                feedbackDisable.Root.transform.position = Vector3.right * 40;
                destroy.Root.transform.position = Vector3.right * 50;
                Physics.SyncTransforms();

                controller.Entity.SetController(controller.Player);
                Assert.That(controller.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return WaitForRing(controller.Feedback);
                controller.Entity.SetController(controller.Brain);
                Assert.That(controller.Feedback.GroundSlamImpactVisible, Is.False, "Controller swap uses existing action cleanup.");

                Assert.That(death.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return WaitForRing(death.Feedback);
                death.Entity.Health.TakeDamage(new DamageInfo(1000, null, death.Root.transform.position), 0);
                Assert.That(death.Feedback.GroundSlamImpactVisible, Is.False);

                Assert.That(terminal.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return WaitForRing(terminal.Feedback);
                GameplayInput.SetTerminalState(true);
                terminal.Entity.SendMessage("Update", SendMessageOptions.RequireReceiver);
                Assert.That(terminal.Feedback.GroundSlamImpactVisible, Is.False);
                GameplayInput.SetTerminalState(false);

                Assert.That(entityDisable.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return WaitForRing(entityDisable.Feedback);
                entityDisable.Entity.enabled = false;
                Assert.That(entityDisable.Feedback.GroundSlamImpactVisible, Is.False);

                Assert.That(feedbackDisable.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return WaitForRing(feedbackDisable.Feedback);
                feedbackDisable.Feedback.enabled = false;
                Assert.That(feedbackDisable.Feedback.GroundSlamImpactVisible, Is.False);

                Assert.That(destroy.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return WaitForRing(destroy.Feedback);
                var destroyedRing = destroy.Feedback.GroundSlamImpactObject;
                Object.Destroy(destroy.Root);
                yield return null;
                Assert.That(destroyedRing == null, Is.True, "Host destroy cannot orphan a detached ring.");

                UnityEngine.SceneManagement.SceneManager.SetActiveScene(teardownScene);
                teardown = new EntityFixture(PrototypeCharacterRoster.GuardianEntId);
                Assert.That(teardown.Entity.TryUse(0, Vector3.forward), Is.True);
                yield return WaitForRing(teardown.Feedback);
                var teardownRing = teardown.Feedback.GroundSlamImpactObject;
                Assert.That(teardownRing.scene, Is.EqualTo(teardown.Root.scene));
                UnityEngine.SceneManagement.SceneManager.SetActiveScene(activeScene);
                yield return UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(teardownScene);
                Assert.That(teardownRing == null, Is.True, "Scene teardown removes the detached same-scene VFX object.");
                Assert.That(ActiveRingCount(), Is.Zero);
            }
            finally
            {
                GameplayInput.ResetForTests();
                if (teardownScene.IsValid() && teardownScene.isLoaded)
                {
                    UnityEngine.SceneManagement.SceneManager.SetActiveScene(activeScene);
                    UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync(teardownScene);
                }
                controller.Dispose(); death.Dispose(); terminal.Dispose(); entityDisable.Dispose(); feedbackDisable.Dispose(); destroy.Dispose(); teardown?.Dispose();
            }
        }

        static IEnumerator WaitForRing(CombatFeedback feedback)
        {
            var deadline = Time.realtimeSinceStartup + 1;
            while (!feedback.GroundSlamImpactVisible && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(feedback.GroundSlamImpactVisible, Is.True, "Timed out waiting for Ground Slam impact presentation.");
        }

        static IEnumerator WaitForReplacement(CombatFeedback feedback, GameObject previous)
        {
            var deadline = Time.realtimeSinceStartup + 1;
            while ((!feedback.GroundSlamImpactVisible || feedback.GroundSlamImpactObject == previous) && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(feedback.GroundSlamImpactVisible, Is.True);
            Assert.That(feedback.GroundSlamImpactObject, Is.Not.SameAs(previous));
        }

        static IEnumerator WaitForIdle(CombatEntity entity)
        {
            var deadline = Time.realtimeSinceStartup + 1;
            while (entity && entity.ActionPhase != CombatActionPhase.Idle && Time.realtimeSinceStartup < deadline) yield return null;
            Assert.That(entity.ActionPhase, Is.EqualTo(CombatActionPhase.Idle));
        }

        static float WorldDiameter(SpriteRenderer renderer) => renderer.sprite.bounds.size.x * renderer.transform.lossyScale.x;

        static int ActiveRingCount() => Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
            .Count(renderer => renderer.name == CombatFeedback.GroundSlamImpactObjectName && renderer.gameObject.activeSelf);

        sealed class EntityFixture : System.IDisposable
        {
            readonly CharacterDefinition definition;
            public readonly GameObject Root;
            public readonly CombatEntity Entity;
            public readonly AbilityDefinition Ability;
            public readonly PlayerController Player;
            public readonly CreatureBrain Brain;
            public readonly CombatFeedback Feedback;

            public EntityFixture(string archetypeId)
            {
                Root = new GameObject($"VFX Fixture {archetypeId}", typeof(CharacterController), typeof(Health),
                    typeof(CombatEntity), typeof(PlayerController), typeof(CreatureBrain));
                definition = ScriptableObject.CreateInstance<CharacterDefinition>();
                Ability = ScriptableObject.CreateInstance<AbilityDefinition>();
                Ability.DisplayName = "Ground Slam";
                Ability.Kind = AbilityKind.Area;
                Ability.Damage = 10;
                Ability.Range = 2;
                Ability.Radius = 2;
                Ability.Windup = .01f;
                Ability.Recovery = .01f;
                Ability.Cooldown = 0;
                definition.ArchetypeId = archetypeId;
                definition.DisplayName = "VFX Fixture";
                definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = 3 };
                definition.Abilities = new[] { Ability };
                Entity = Root.GetComponent<CombatEntity>();
                Entity.Initialize(definition);
                Player = Root.GetComponent<PlayerController>();
                Brain = Root.GetComponent<CreatureBrain>();
                Feedback = Root.GetComponent<CombatFeedback>();
            }

            public void Dispose()
            {
                if (Root) Object.Destroy(Root);
                if (definition) Object.Destroy(definition);
                if (Ability) Object.Destroy(Ability);
            }
        }
    }
}
