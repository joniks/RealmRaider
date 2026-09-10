using System.Collections;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealmRaiders.Tests
{
    public sealed class CharacterProceduralMotionAdapterPlayModeTests
    {
        [UnityTest]
        public IEnumerator BloodKnightAdapter_BindsActualHeroMapsFactsAndCleansUp()
        {
            var host = new GameObject("Procedural Motion Play Host");
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            var recipe = ScriptableObject.CreateInstance<CharacterVisualRecipe>();
            var ability = ScriptableObject.CreateInstance<AbilityDefinition>();
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            host.AddComponent<CharacterController>();
            host.AddComponent<Health>();
            var entity = host.AddComponent<CombatEntity>();
            var previousTerminal = GameplayInput.TerminalState;
            try
            {
                GameplayInput.SetTerminalState(false);
                ground.name = "Procedural Motion Test Ground";
                ground.transform.localScale = Vector3.one * 8f;
                host.transform.position = Vector3.up;
                recipe.Family = CharacterVisualFamily.Humanoid;
                recipe.Primary = Color.red;
                recipe.Secondary = Color.black;
                recipe.AccentColor = Color.yellow;
                recipe.BaseBodyPrefab = Resources.Load<GameObject>("Characters/BloodKnightHero");
                Assert.That(recipe.BaseBodyPrefab, Is.Not.Null);
                ability.Kind = AbilityKind.Melee;
                ability.Windup = 0f;
                definition.Stats = CombatStats.BloodKnight;
                definition.VisualRecipe = recipe;
                definition.Abilities = new[] { ability };
                entity.Initialize(definition);

                var adapter = host.GetComponent<CharacterProceduralMotionAdapter>();
                var pivot = host.transform.Find("Character Visual Modules/Presentation Pivot");
                var baseBody = host.transform.Find("Character Visual Modules/Presentation Pivot/Base Body");
                Assert.That(adapter, Is.Not.Null);
                Assert.That(adapter.IsBound, Is.True, "The active Blood Knight must bind its exact six factual Bip01 descendants.");
                Assert.That(pivot, Is.Not.Null);
                Assert.That(baseBody, Is.Not.Null);
                var pivotPose = Pose.Of(pivot);
                var bodyPose = Pose.Of(baseBody);
                var rootPosition = host.transform.position;
                var controller = host.GetComponent<CharacterController>();
                var controllerHeight = controller.height;
                var controllerRadius = controller.radius;
                var controllerCenter = controller.center;
                var bones = RequiredBones(baseBody);
                host.GetComponent<CharacterVisualMotion>().enabled = false;

                var baseline = Snapshot(bones);
                var locomotionObserved = false;
                for (var sample = 0; sample < 3; sample++)
                {
                    host.transform.position += Vector3.forward * .3f;
                    adapter.SamplePresentation(sample * .05f, 100f + sample, .05f, false, true, true);
                    locomotionObserved |= RotationDelta(bones[2], baseline[2]) > .001f;
                }
                Assert.That(locomotionObserved, Is.True, "Factual horizontal root displacement must map to locomotion.");
                Assert.That(Pose.Of(pivot), Is.EqualTo(pivotPose));
                Assert.That(Pose.Of(baseBody), Is.EqualTo(bodyPose));
                Assert.That(host.transform.localRotation, Is.EqualTo(Quaternion.identity));
                Assert.That(host.transform.localScale, Is.EqualTo(Vector3.one));
                Assert.That(controller.height, Is.EqualTo(controllerHeight));
                Assert.That(controller.radius, Is.EqualTo(controllerRadius));
                Assert.That(controller.center, Is.EqualTo(controllerCenter));
                Assert.That(Vector3.Distance(host.transform.position, rootPosition + Vector3.forward * .9f), Is.LessThan(.001f));

                Reset(adapter, baseBody);
                baseline = Snapshot(bones);
                host.GetComponent<Health>().TakeDamage(new DamageInfo(1, null, host.transform.position), 0);
                host.transform.position += Vector3.forward * .1f;
                adapter.SamplePresentation(Time.unscaledTime, 500f, .05f, false, true, true);
                Assert.That(RotationDelta(bones[0], baseline[0]), Is.EqualTo(14f).Within(.01f), "Hit presentation outranks a factual stride.");
                yield return null;
                Assert.That(RotationDelta(bones[0], baseline[0]), Is.EqualTo(14f).Within(.01f), "Only Health.Damaged may drive the hit pose.");

                Reset(adapter, baseBody);
                baseline = Snapshot(bones);
                yield return new WaitForSecondsRealtime(.13f);
                Assert.That(entity.TryUse(0, Vector3.forward), Is.True);
                host.transform.position += Vector3.forward * .1f;
                adapter.SamplePresentation(Time.unscaledTime, 500f, .05f, false, true, true);
                Assert.That(RotationDelta(bones[1], baseline[1]), Is.EqualTo(30f).Within(.01f), "Action presentation outranks factual travel.");
                yield return null;
                Assert.That(entity.ActionPhase, Is.Not.EqualTo(CombatActionPhase.Idle));
                Assert.That(RotationDelta(bones[1], baseline[1]), Is.EqualTo(30f).Within(.01f), "A non-idle action phase must map to generic primary attack after the hit window expires.");

                yield return new WaitForSecondsRealtime(.15f);
                Assert.That(entity.ActionPhase, Is.EqualTo(CombatActionPhase.Idle));
                Reset(adapter, baseBody);
                baseline = Snapshot(bones);

                adapter.enabled = false;
                yield return null;
                Assert.That(adapter.IsBound, Is.False);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline), "The actual disable lifecycle must restore cached bone transforms.");
                adapter.enabled = true;
                yield return null;
                Assert.That(adapter.IsBound, Is.True, "The actual enable lifecycle must rebind the retained Base Body.");

                Reset(adapter, baseBody);
                baseline = Snapshot(bones);
                var timeline = host.GetComponent<CharacterJumpPresentationTimeline>();
                Assert.That(timeline.StraightenSeconds, Is.EqualTo(.30f).Within(.0001f));
                Assert.That(timeline.TakeoffSeconds, Is.EqualTo(.40f).Within(.0001f));
                Assert.That(timeline.LandingSeconds, Is.EqualTo(.56f).Within(.0001f));

                adapter.SamplePresentation(0f, 0f, .016f, false, true, true);
                host.transform.position += Vector3.forward * entity.Stats.MoveSpeed * .1f;
                adapter.SamplePresentation(.01f, .2f, .1f, false, true, true);
                var stride = Mathf.Sin(.1f * 7.5f) * (1f - Mathf.Exp(-CharacterMotionDynamics.SpeedResponse * .1f));
                AssertLocalRotation(bones[0], baseline[0], Vector3.forward, 56f * stride);
                AssertLocalRotation(bones[1], baseline[1], Vector3.forward, -56f * stride);
                AssertLocalRotation(bones[2], baseline[2], Vector3.forward, -50f * stride);
                AssertLocalRotation(bones[3], baseline[3], Vector3.forward, 50f * stride);
                AssertLocalRotation(bones[4], baseline[4], Vector3.forward, 26f * stride);
                AssertLocalRotation(bones[5], baseline[5], Vector3.forward, -26f * stride);
                Assert.That(Pose.Of(pivot), Is.EqualTo(pivotPose));
                Assert.That(Pose.Of(baseBody), Is.EqualTo(bodyPose));
                Assert.That(controller.height, Is.EqualTo(controllerHeight));
                Assert.That(controller.radius, Is.EqualTo(controllerRadius));
                Assert.That(controller.center, Is.EqualTo(controllerCenter));

                Reset(adapter, baseBody);
                baseline = Snapshot(bones);
                adapter.SamplePresentation(0f, 0f, .016f, false, true, true);
                adapter.SamplePresentation(0f, 0f, .016f, true, false, true);
                AssertJumpPose(bones, baseline, -30f, -88f, -88f, 80f, 80f, "Takeoff starts in the module's deep bilateral crouch.");
                var crouch = Snapshot(bones);
                adapter.SamplePresentation(0f, 0f, .016f, true, false, true);
                Assert.That(Snapshot(bones), Is.EqualTo(crouch), "Two consumers at the same factual timestamp must receive the same jump sample.");
                host.transform.position += Vector3.forward * .1f;
                adapter.SamplePresentation(.15f, .15f, .016f, true, false, true);
                AssertJumpPose(bones, baseline, -24f, -55f, -38f, 50f, 35f, "Halfway through .30 seconds, crouch continuously straightens.");
                adapter.SamplePresentation(.30f, .30f, .016f, true, false, true);
                AssertJumpPose(bones, baseline, -18f, -22f, 12f, 20f, -10f, "At .30 seconds the planted and free legs form the one-leg push.");
                var push = Snapshot(bones);
                adapter.SamplePresentation(.40f, .40f, .016f, true, false, true);
                Assert.That(Snapshot(bones), Is.EqualTo(push), "The final .10 seconds of the .40-second takeoff holds the push.");
                adapter.SamplePresentation(.60f, .60f, .016f, true, false, true);
                AssertJumpPose(bones, baseline, 2f, -5f, 12f, 10f, -5f, "Falling continuously blends from push toward the fall pose.");
                adapter.SamplePresentation(.80f, .80f, .016f, true, false, true);
                AssertJumpPose(bones, baseline, 22f, 12f, 12f, 0f, 0f, "The bounded fall blend reaches its factual fall pose.");
                var fall = Snapshot(bones);
                adapter.SamplePresentation(.80f, .80f, .016f, false, true, true);
                Assert.That(Snapshot(bones), Is.EqualTo(fall), "Landing begins continuously from the fall pose.");
                adapter.SamplePresentation(1.08f, 1.08f, .016f, false, true, true);
                AssertJumpPose(bones, baseline, -10f, -22f, -22f, 24f, 24f, "At the .56-second landing midpoint, compression is exact.");
                adapter.SamplePresentation(1.36f, 1.36f, .016f, false, true, true);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline), "Landing ends at the exact cached baseline.");

                adapter.SamplePresentation(1.37f, 1.37f, .016f, true, false, true);
                adapter.SamplePresentation(1.38f, 0f, 0f, true, false, false);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline), "Controller loss clears jump presentation immediately.");

                Reset(adapter, baseBody);
                baseline = Snapshot(bones);
                adapter.SamplePresentation(0f, 0f, .016f, false, true, true);
                adapter.SamplePresentation(0f, 0f, .016f, true, false, true);
                adapter.SamplePresentation(.40f, .40f, .016f, true, false, true);
                adapter.SamplePresentation(.60f, .60f, .016f, false, true, true);
                AssertJumpPose(bones, baseline, 2f, -5f, 12f, 10f, -5f, "Early factual grounding remains on the partial fall pose until the scheduled visual boundary.");
                adapter.SamplePresentation(.7999f, .7999f, .016f, false, true, true);
                var immediatelyBeforeLanding = Snapshot(bones);
                adapter.SamplePresentation(.80f, .80f, .016f, false, true, true);
                AssertJumpPose(bones, baseline, 22f, 12f, 12f, 0f, 0f, "The scheduled landing boundary begins from the full fall pose.");
                AssertBoneRotationsContinuous(bones, immediatelyBeforeLanding, "Early factual grounding cannot snap from a partial fall pose into landing.");

                var player = host.AddComponent<PlayerController>();
                entity.RefreshControllers();
                entity.SetController(player);
                var settleDeadline = Time.realtimeSinceStartup + 1f;
                while (!entity.IsGrounded && Time.realtimeSinceStartup < settleDeadline)
                {
                    entity.Move(Vector3.zero);
                    yield return null;
                }
                Assert.That(entity.IsGrounded, Is.True, "The factual PlayerController jump path requires a grounded CharacterController.");

                Reset(adapter, baseBody);
                yield return null;
                baseline = Snapshot(bones);
                var factualRootPosition = host.transform.position;
                var factualRootRotation = host.transform.rotation;
                var factualRootScale = host.transform.localScale;
                Assert.That(entity.TryJump(), Is.True, "The factual direct-controlled entity must enter CharacterJumpState through TryJump.");
                entity.Move(Vector3.zero);
                yield return null;
                Assert.That(entity.IsJumping, Is.True);
                var factualTimeline = host.GetComponent<CharacterJumpPresentationTimeline>();
                var factualAirSample = factualTimeline.Observe(Time.unscaledTime, entity.IsJumping, entity.IsGrounded, CharacterJumpPresentationTimeline.HasFactualDirectControl(entity));
                Assert.That(factualAirSample.Phase, Is.Not.EqualTo(CharacterJumpPresentationPhase.None), "The live PlayerController/CharacterController jump must reach presentation sampling.");
                Assert.That(AnyRotationChanged(bones, baseline), Is.True, "The live jump must visibly move only the bound presentation bones.");
                Assert.That(host.transform.position.x, Is.EqualTo(factualRootPosition.x).Within(.001f));
                Assert.That(host.transform.position.z, Is.EqualTo(factualRootPosition.z).Within(.001f));
                Assert.That(host.transform.rotation, Is.EqualTo(factualRootRotation));
                Assert.That(host.transform.localScale, Is.EqualTo(factualRootScale));
                Assert.That(controller.height, Is.EqualTo(controllerHeight));
                Assert.That(controller.radius, Is.EqualTo(controllerRadius));
                Assert.That(controller.center, Is.EqualTo(controllerCenter));

                var jumpDeadline = Time.realtimeSinceStartup + 2f;
                var lastAirborneSample = factualAirSample;
                while (entity.IsJumping && Time.realtimeSinceStartup < jumpDeadline)
                {
                    lastAirborneSample = factualTimeline.Observe(Time.unscaledTime, entity.IsJumping, entity.IsGrounded, CharacterJumpPresentationTimeline.HasFactualDirectControl(entity));
                    entity.Move(Vector3.zero);
                    yield return null;
                }
                Assert.That(entity.IsJumping, Is.False, "CharacterController gravity and grounding must settle the factual jump.");
                Assert.That(entity.IsGrounded, Is.True);
                var factualLandingSample = factualTimeline.Observe(Time.unscaledTime, entity.IsJumping, entity.IsGrounded, CharacterJumpPresentationTimeline.HasFactualDirectControl(entity));
                Assert.That(factualLandingSample.Phase, Is.EqualTo(CharacterJumpPresentationPhase.Falling).Or.EqualTo(CharacterJumpPresentationPhase.Landing), "Grounding must preserve the factual fall presentation or begin its landing response.");
                if (factualLandingSample.Phase == CharacterJumpPresentationPhase.Falling)
                {
                    Assert.That(lastAirborneSample.Phase, Is.EqualTo(CharacterJumpPresentationPhase.Falling));
                    Assert.That(factualLandingSample.Progress, Is.GreaterThanOrEqualTo(lastAirborneSample.Progress));
                    Assert.That(factualLandingSample.Progress, Is.LessThan(1f), "Early factual grounding must not snap straight to the full fall pose.");
                }
                Assert.That(host.transform.position.x, Is.EqualTo(factualRootPosition.x).Within(.001f));
                Assert.That(host.transform.position.z, Is.EqualTo(factualRootPosition.z).Within(.001f));
                Assert.That(host.transform.rotation, Is.EqualTo(factualRootRotation));
                Assert.That(host.transform.localScale, Is.EqualTo(factualRootScale));
                Assert.That(controller.height, Is.EqualTo(controllerHeight));
                Assert.That(controller.radius, Is.EqualTo(controllerRadius));
                Assert.That(controller.center, Is.EqualTo(controllerCenter));

                // Exercise the shared factual yaw/displacement path under the same real PlayerController.
                var visualMotion = host.GetComponent<CharacterVisualMotion>();
                Reset(adapter, baseBody);
                baseline = Snapshot(bones);
                host.transform.position += Vector3.forward * .2f;
                host.transform.rotation = Quaternion.Euler(0, 30f, 0);
                var drivenRootPosition = host.transform.position;
                var drivenRootRotation = host.transform.rotation;
                visualMotion.SampleFactualPose(10f, 10f, .1f);
                var sharedDynamics = visualMotion.SampleFactualDynamics(10f, .1f, true, CharacterJumpPresentationSample.None);
                var sharedPhase = sharedDynamics.Phase;
                Assert.That(sharedDynamics.TurnLean, Is.LessThan(0).And.GreaterThanOrEqualTo(-CharacterMotionDynamics.MaximumTurnLean));
                Assert.That(Mathf.DeltaAngle(0, pivot.localEulerAngles.z), Is.LessThan(0), "Factual right yaw leans the presentation into the turn.");
                adapter.SamplePresentation(10f, 1000f, .1f, false, true, true);
                Assert.That(sharedDynamics.Phase, Is.EqualTo(sharedPhase), "Bone sampling cannot advance the pivot's shared gait a second time.");
                Assert.That(AnyRotationChanged(bones, baseline), Is.True);
                for (var i = 1; i <= 20; i++)
                {
                    visualMotion.SampleFactualPose(10f + i * .1f, 10f + i * .1f, .1f);
                    adapter.SamplePresentation(10f + i * .1f, 1000f + i, .1f, false, true, true);
                }
                Assert.That(sharedDynamics.Speed, Is.Zero);
                Assert.That(sharedDynamics.TurnLean, Is.Zero);
                Assert.That(Quaternion.Angle(pivot.localRotation, pivotPose.Rotation), Is.LessThan(.01f));
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
                Assert.That(host.transform.position, Is.EqualTo(drivenRootPosition));
                Assert.That(host.transform.rotation, Is.EqualTo(drivenRootRotation));
                Assert.That(host.transform.localScale, Is.EqualTo(factualRootScale));
                Assert.That(Pose.Of(baseBody), Is.EqualTo(bodyPose));
                Assert.That(controller.height, Is.EqualTo(controllerHeight));
                Assert.That(controller.radius, Is.EqualTo(controllerRadius));
                Assert.That(controller.center, Is.EqualTo(controllerCenter));
                Assert.That(entity.ActiveController, Is.SameAs(player));
                host.transform.position += Vector3.forward * .2f;
                host.transform.rotation = Quaternion.Euler(0, 60f, 0);
                visualMotion.SampleFactualPose(12.5f, 12.5f, .1f);
                adapter.SamplePresentation(12.5f, 2000f, .1f, false, true, true);
                Assert.That(sharedDynamics.Speed, Is.GreaterThan(0));
                var beforeHitRotation = pivot.localRotation;
                visualMotion.ShowHitReaction();
                host.transform.rotation = Quaternion.Euler(0, 90f, 0);
                visualMotion.SampleFactualPose(12.55f, 12.55f, .05f);
                var hitRotation = Quaternion.Slerp(beforeHitRotation, pivotPose.Rotation * Quaternion.Euler(0, 0, 5f), 1f - Mathf.Exp(-.05f * 14f));
                Assert.That(Quaternion.Angle(pivot.localRotation, hitRotation), Is.LessThan(.01f), "Turn/weight accents cannot cancel the factual hit response.");
                visualMotion.ClearTransientReaction();
                host.transform.position += Vector3.forward * .2f;
                adapter.SamplePresentation(12.6f, 2000f, .1f, false, true, true);
                Assert.That(sharedDynamics.Speed, Is.GreaterThan(0));
                entity.SetController(null);
                adapter.SamplePresentation(13f, 3000f, .1f, false, true, false);
                Assert.That(sharedDynamics.Phase, Is.Zero);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
                Assert.That(Pose.Of(pivot), Is.EqualTo(pivotPose), "Controller loss restores the pivot exactly.");
                entity.SetController(player);

                Reset(adapter, baseBody);
                host.transform.position += Vector3.forward * .2f;
                adapter.SamplePresentation(14f, 2000f, .1f, false, true, true);
                Assert.That(sharedDynamics.Speed, Is.GreaterThan(0));
                entity.ApplyRoot(1f);
                adapter.SamplePresentation(14.1f, 2001f, .1f, false, true, false);
                Assert.That(sharedDynamics.Speed, Is.Zero);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline), "Root clears an active gait.");
                entity.BreakRoot();
                host.transform.position += Vector3.forward * .2f;
                adapter.SamplePresentation(15f, 2002f, .1f, false, true, true);
                Assert.That(sharedDynamics.Speed, Is.GreaterThan(0));
                GameplayInput.SetTerminalState(true);
                adapter.SamplePresentation(15.1f, 2003f, .1f, false, true, true);
                Assert.That(sharedDynamics.Speed, Is.Zero);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline), "Terminal state clears an active gait.");
                GameplayInput.SetTerminalState(false);

                Reset(adapter, baseBody);
                baseline = Snapshot(bones);
                host.transform.position += Vector3.forward * .2f;
                adapter.SamplePresentation(16f, 2004f, .1f, false, true, true);
                Assert.That(sharedDynamics.Speed, Is.GreaterThan(0));
                host.GetComponent<Health>().TakeDamage(new DamageInfo(10000, null, host.transform.position), 0);
                yield return null;
                Assert.That(sharedDynamics.Speed, Is.Zero, "Death clears gait before its dominant pose is sampled.");
                Assert.That(host.GetComponent<Health>().IsDead, Is.True);
                Assert.That(RotationDelta(bones[0], baseline[0]), Is.EqualTo(16f).Within(.01f), "Health.IsDead must retain death priority.");
                Assert.That(Pose.Of(pivot), Is.EqualTo(pivotPose));
                Assert.That(Pose.Of(baseBody), Is.EqualTo(bodyPose));

                adapter.Clear();
                Assert.That(adapter.IsBound, Is.False);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
            }
            finally
            {
                GameplayInput.SetTerminalState(previousTerminal);
                Object.Destroy(host);
                Object.Destroy(ground);
                Object.Destroy(definition);
                Object.Destroy(recipe);
                Object.Destroy(ability);
            }
        }

        private static Transform[] RequiredBones(Transform baseBody)
        {
            var names = new[]
            {
                "Bip01 L UpperArm", "Bip01 R UpperArm", "Bip01 L Thigh",
                "Bip01 R Thigh", "Bip01 L Calf", "Bip01 R Calf"
            };
            var bones = new Transform[names.Length];
            for (var index = 0; index < names.Length; index++)
            {
                bones[index] = FindDescendant(baseBody, names[index]);
                Assert.That(bones[index], Is.Not.Null, names[index]);
            }
            return bones;
        }

        private static Transform FindDescendant(Transform root, string name)
        {
            foreach (var candidate in root.GetComponentsInChildren<Transform>(true))
                if (candidate != root && candidate.name == name) return candidate;
            return null;
        }

        private static Pose[] Snapshot(Transform[] bones)
        {
            var result = new Pose[bones.Length];
            for (var index = 0; index < bones.Length; index++) result[index] = Pose.Of(bones[index]);
            return result;
        }

        private static bool AnyRotationChanged(Transform[] bones, Pose[] baseline)
        {
            for (var index = 0; index < bones.Length; index++)
                if (Quaternion.Angle(bones[index].localRotation, baseline[index].Rotation) > .001f) return true;
            return false;
        }

        private static float RotationDelta(Transform bone, Pose baseline)
        {
            return Quaternion.Angle(bone.localRotation, baseline.Rotation);
        }

        private static void AssertJumpPose(Transform[] bones, Pose[] baseline, float arm, float leftThigh, float rightThigh, float leftCalf, float rightCalf, string message)
        {
            AssertLocalRotation(bones[0], baseline[0], Vector3.forward, arm, message);
            AssertLocalRotation(bones[1], baseline[1], Vector3.forward, arm, message);
            AssertLocalRotation(bones[2], baseline[2], Vector3.forward, leftThigh, message);
            AssertLocalRotation(bones[3], baseline[3], Vector3.forward, rightThigh, message);
            AssertLocalRotation(bones[4], baseline[4], Vector3.forward, leftCalf, message);
            AssertLocalRotation(bones[5], baseline[5], Vector3.forward, rightCalf, message);
        }

        private static void AssertLocalRotation(Transform bone, Pose baseline, Vector3 axis, float degrees, string message = null)
        {
            var expected = baseline.Rotation * Quaternion.AngleAxis(degrees, axis);
            Assert.That(Quaternion.Angle(bone.localRotation, expected), Is.LessThan(.01f), message);
        }

        private static void AssertBoneRotationsContinuous(Transform[] bones, Pose[] before, string message)
        {
            for (var index = 0; index < bones.Length; index++)
                Assert.That(Quaternion.Angle(bones[index].localRotation, before[index].Rotation), Is.LessThan(.05f), message);
        }

        private static void Reset(CharacterProceduralMotionAdapter adapter, Transform baseBody)
        {
            adapter.Clear();
            Assert.That(adapter.Bind(baseBody), Is.True);
        }

        private readonly struct Pose : System.IEquatable<Pose>
        {
            public Pose(Vector3 position, Quaternion rotation, Vector3 scale) { Position = position; Rotation = rotation; Scale = scale; }
            public Vector3 Position { get; }
            public Quaternion Rotation { get; }
            public Vector3 Scale { get; }
            public static Pose Of(Transform target) => new(target.localPosition, target.localRotation, target.localScale);
            public bool Equals(Pose other) => Position == other.Position && Rotation == other.Rotation && Scale == other.Scale;
            public override bool Equals(object value) => value is Pose other && Equals(other);
            public override int GetHashCode() => Position.GetHashCode() ^ Rotation.GetHashCode() ^ Scale.GetHashCode();
        }
    }
}
