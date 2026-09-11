using System.Collections.Generic;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Modules.CharacterProceduralMotion;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class CharacterProceduralMotionAdapterTests
    {
        [Test]
        public void BloodKnightTuning_ResolvesPreferredExplicitFallbackAndRejectedCatalogueFailClosed()
        {
            var preferredBuild = ProceduralHumanoidTuningCatalogue.Build(
                new IProceduralHumanoidTuningProvider[] { new StarterProceduralHumanoidTuningProvider() });
            Assert.That(preferredBuild.Succeeded, Is.True);
            var preferred = CharacterProceduralMotionAdapter.ResolveBloodKnightTuning(preferredBuild.Catalogue);
            Assert.That(preferred, Is.SameAs(ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable));
            Assert.That(CharacterProceduralMotionAdapter.ResolveBloodKnightTuning(preferredBuild.Catalogue), Is.SameAs(preferred), "Repeated explicit resolution is deterministic.");

            var fallbackBuild = ProceduralHumanoidTuningCatalogue.Build(new IProceduralHumanoidTuningProvider[]
            {
                new TestTuningProvider("tests.procedural-humanoid.fallback",
                    new ProceduralHumanoidTuningProfile(StarterProceduralHumanoidTuningProvider.CompatibilityProfileId,
                        ProceduralHumanoidMotionTuning.CompatibilityDefault))
            });
            Assert.That(fallbackBuild.Succeeded, Is.True);
            Assert.That(CharacterProceduralMotionAdapter.ResolveBloodKnightTuning(fallbackBuild.Catalogue),
                Is.SameAs(ProceduralHumanoidMotionTuning.CompatibilityDefault), "The assignment's explicit Compatibility profile is its factual fallback.");

            var unavailableBuild = ProceduralHumanoidTuningCatalogue.Build(new IProceduralHumanoidTuningProvider[]
            {
                new TestTuningProvider("tests.procedural-humanoid.unassigned",
                    new ProceduralHumanoidTuningProfile("tests.procedural-humanoid.other", ProceduralHumanoidMotionTuning.BloodKnightDeviceReadable))
            });
            Assert.That(unavailableBuild.Succeeded, Is.True);
            Assert.That(CharacterProceduralMotionAdapter.ResolveBloodKnightTuning(unavailableBuild.Catalogue),
                Is.SameAs(ProceduralHumanoidMotionTuning.CompatibilityDefault), "A rejected assignment fails closed to the exact compatibility object.");

            var malformedBuild = ProceduralHumanoidTuningCatalogue.Build(new IProceduralHumanoidTuningProvider[]
            {
                new TestTuningProvider("tests.procedural-humanoid.malformed",
                    new ProceduralHumanoidTuningProfile(StarterProceduralHumanoidTuningProvider.BloodKnightDeviceReadableProfileId, null))
            });
            Assert.That(malformedBuild.Succeeded, Is.False);
            Assert.That(CharacterProceduralMotionAdapter.ResolveBloodKnightTuning(malformedBuild.Catalogue),
                Is.SameAs(ProceduralHumanoidMotionTuning.CompatibilityDefault));
            Assert.That(CharacterProceduralMotionAdapter.ResolveBloodKnightTuning(null),
                Is.SameAs(ProceduralHumanoidMotionTuning.CompatibilityDefault));
        }

        [TestCase(0f)]
        [TestCase(73f)]
        public void Adapter_OptionalTorsoUsesOwnedSemanticChainAndPreservesSixLimbLocalOutput(float yaw)
        {
            var fixture = CreateFixture();
            var control = CreateFixture();
            try
            {
                var pivot = fixture.Host.GetComponent<CharacterVisualAssembler>().PresentationPivot;
                var controlPivot = control.Host.GetComponent<CharacterVisualAssembler>().PresentationPivot;
                pivot.localRotation = controlPivot.localRotation = Quaternion.Euler(0, yaw, 0);
                var body = CreateBody(pivot, true, out var bones);
                var controlBody = CreateBody(controlPivot, true, out var controlBones);
                body.localRotation = controlBody.localRotation = Quaternion.Euler(0, 180, 0);
                var torso = AddTorsoChain(body, bones);
                var controlTorso = AddTorsoChain(controlBody, controlBones);
                controlTorso.name = "Torso Not Opted In";
                var baseline = Pose.Of(torso);
                var bodyPose = Pose.Of(body); var pivotPose = Pose.Of(pivot);
                var motor = fixture.Host.GetComponent<CharacterController>();
                var center = motor.center; var height = motor.height; var radius = motor.radius;
                Assert.That(fixture.Adapter.Bind(body, pivot), Is.True);
                Assert.That(fixture.Adapter.HasUpperTorso, Is.True);
                Assert.That(control.Adapter.Bind(controlBody, controlPivot), Is.True);
                Assert.That(control.Adapter.HasUpperTorso, Is.False);
                fixture.Host.transform.position += Vector3.forward * .2f;
                control.Host.transform.position += Vector3.forward * .2f;
                var root = Pose.Of(fixture.Host.transform);
                fixture.Adapter.SamplePresentation(1, 1, .05f, false, true, true);
                control.Adapter.SamplePresentation(1, 1, .05f, false, true, true);
                Assert.That(Snapshot(bones), Is.EqualTo(Snapshot(controlBones)), "Optional torso must preserve all six accepted LOCAL limb poses.");
                for (var i = 2; i < 6; i++)
                {
                    Assert.That(bones[i].position, Is.EqualTo(controlBones[i].position));
                    Assert.That(Quaternion.Angle(bones[i].rotation, controlBones[i].rotation), Is.LessThan(.01f), "Torso cannot rotate the separate leg branch.");
                }
                var dynamics = fixture.Host.GetComponent<CharacterVisualMotion>().SampleFactualDynamics(1, .05f, true, CharacterJumpPresentationSample.None);
                AssertBoneAngle(torso, baseline, -2f * Mathf.Sin(dynamics.Phase) * dynamics.Speed);
                Assert.That(Quaternion.Angle(torso.localRotation, baseline.Rotation), Is.GreaterThan(.01f).And.LessThanOrEqualTo(2.01f));
                var first = Pose.Of(torso);
                fixture.Adapter.SamplePresentation(1, 1, .05f, false, true, true);
                Assert.That(Pose.Of(torso), Is.EqualTo(first));
                Assert.That(Pose.Of(fixture.Host.transform), Is.EqualTo(root));
                Assert.That(Pose.Of(body), Is.EqualTo(bodyPose)); Assert.That(Pose.Of(pivot), Is.EqualTo(pivotPose));
                Assert.That(torso.localPosition, Is.EqualTo(baseline.Position)); Assert.That(torso.localScale, Is.EqualTo(baseline.Scale));
                Assert.That(motor.center, Is.EqualTo(center)); Assert.That(motor.height, Is.EqualTo(height)); Assert.That(motor.radius, Is.EqualTo(radius));
                fixture.Adapter.Clear();
                Assert.That(Pose.Of(torso), Is.EqualTo(baseline));
                Assert.That(fixture.Adapter.HasUpperTorso, Is.False);
                Assert.That(fixture.Adapter.Bind(body, pivot), Is.True);
                fixture.Host.transform.position += Vector3.forward * .2f;
                fixture.Adapter.SamplePresentation(2, 2, .05f, false, true, true);
                InvokeAdapter(fixture.Adapter, "OnDisable");
                Assert.That(Pose.Of(torso), Is.EqualTo(baseline));
                Assert.That(fixture.Adapter.HasUpperTorso, Is.False);
                InvokeAdapter(fixture.Adapter, "OnEnable");
                Assert.That(fixture.Adapter.HasUpperTorso, Is.True);
            }
            finally { fixture.Dispose(); control.Dispose(); }
        }

        [TestCase("missing")]
        [TestCase("duplicate")]
        [TestCase("wrong-parent")]
        [TestCase("reflected")]
        [TestCase("unsupported-scale")]
        public void Adapter_InvalidOptionalTorsoRetainsSixBoneMotion(string invalid)
        {
            var fixture = CreateFixture();
            try
            {
                var pivot = fixture.Host.GetComponent<CharacterVisualAssembler>().PresentationPivot;
                var body = CreateBody(pivot, true, out var bones);
                var torso = AddTorsoChain(body, bones);
                Assert.That(fixture.Adapter.Bind(body, pivot), Is.True);
                var baseline = Pose.Of(torso);
                fixture.Host.transform.position += Vector3.forward * .2f;
                fixture.Adapter.SamplePresentation(1, 1, .05f, false, true, true);
                fixture.Adapter.Clear();
                Assert.That(Pose.Of(torso), Is.EqualTo(baseline));
                if (invalid == "missing") torso.name = "Not Spine1";
                if (invalid == "duplicate") new GameObject("Bip01 Spine1").transform.SetParent(body, false);
                if (invalid == "wrong-parent") torso.SetParent(body, false);
                if (invalid == "reflected")
                {
                    torso.localScale = new Vector3(-1, 1, 1);
                    // Compensate below torso so the six required limbs remain supported; only torso is reflected.
                    torso.GetChild(0).localScale = new Vector3(-1, 1, 1);
                }
                if (invalid == "unsupported-scale")
                {
                    torso.localScale = Vector3.one * .00005f;
                    torso.GetChild(0).localScale = Vector3.one * 20000f;
                }
                baseline = Pose.Of(torso);
                var limbs = Snapshot(bones);
                Assert.That(fixture.Adapter.Bind(body, pivot), Is.True, "An invalid OPTIONAL transform cannot disable valid required limbs.");
                Assert.That(fixture.Adapter.HasUpperTorso, Is.False);
                fixture.Host.transform.position += Vector3.forward * .2f;
                fixture.Adapter.SamplePresentation(2, 2, .05f, false, true, true);
                Assert.That(Quaternion.Angle(bones[2].localRotation, limbs[2].Rotation), Is.GreaterThan(.01f));
                Assert.That(Pose.Of(torso), Is.EqualTo(baseline));
                Assert.That(body.gameObject.activeSelf, Is.True);
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void Adapter_TorsoParticipatesInCombatCrossfadeAndRestoresOnTerminalDeathAndFailedRebind()
        {
            var fixture = CreateFixture();
            var terminal = RealmRaiders.Controllers.GameplayInput.TerminalState;
            try
            {
                RealmRaiders.Controllers.GameplayInput.SetTerminalState(false);
                var pivot = fixture.Host.GetComponent<CharacterVisualAssembler>().PresentationPivot;
                var body = CreateBody(pivot, true, out var bones);
                var torso = AddTorsoChain(body, bones);
                var baseline = Pose.Of(torso);
                Assert.That(fixture.Adapter.Bind(body, pivot), Is.True);
                var clock = Time.unscaledTime;
                InvokeAdapter(fixture.Adapter, "OnAction", new CombatPresentationFact(1, CombatActionPhase.Windup, CombatPresentationEnd.None,
                    Vector3.forward, Quaternion.identity, null, .2f, Time.time - .2f, clock - .2f));
                fixture.Adapter.SamplePresentation(clock, Time.time, .016f, false, true, true);
                AssertBoneAngle(torso, baseline, -4f);
                var windup = torso.localRotation;
                fixture.Health.TakeDamage(new DamageInfo(1, null, fixture.Host.transform.position), 0);
                fixture.Adapter.SamplePresentation(clock, Time.time, .016f, false, true, true);
                Assert.That(Quaternion.Angle(torso.localRotation, windup), Is.LessThan(.01f), "Torso shares the existing combat crossfade snapshot.");
                fixture.Adapter.SamplePresentation(clock + .02f, Time.time + .02f, .016f, false, true, true);
                var hitWeight = Mathf.SmoothStep(0, 1, .02f / .06f);
                var expected = Quaternion.Slerp(windup, baseline.Rotation * Quaternion.AngleAxis(-5f * hitWeight, baseline.SemanticAxis), .5f);
                Assert.That(Quaternion.Angle(torso.localRotation, expected), Is.LessThan(.01f));
                fixture.Adapter.SamplePresentation(clock + .06f, Time.time + .06f, .016f, false, true, true);
                AssertBoneAngle(torso, baseline, -5f);
                RealmRaiders.Controllers.GameplayInput.SetTerminalState(true);
                fixture.Adapter.SamplePresentation(clock + .07f, Time.time + .07f, .016f, false, true, false);
                Assert.That(Pose.Of(torso), Is.EqualTo(baseline));
                RealmRaiders.Controllers.GameplayInput.SetTerminalState(false);
                fixture.Health.TakeDamage(new DamageInfo(10000, null, fixture.Host.transform.position), 0);
                fixture.Adapter.SamplePresentation(clock + .08f, Time.time + .08f, .016f, false, true, false);
                Assert.That(Pose.Of(torso), Is.EqualTo(baseline), "No new torso death accent.");
                Assert.That(fixture.Adapter.Bind(body, null), Is.False);
                Assert.That(Pose.Of(torso), Is.EqualTo(baseline));
                Assert.That(fixture.Adapter.HasUpperTorso, Is.False);
            }
            finally { RealmRaiders.Controllers.GameplayInput.SetTerminalState(terminal); fixture.Dispose(); }
        }

        [Test]
        public void Adapter_HitOverridesAttackAndJumpIsIdempotentAndTerminalDeathClearTransientMotion()
        {
            var fixture = CreateFixture();
            var terminal = RealmRaiders.Controllers.GameplayInput.TerminalState;
            try
            {
                RealmRaiders.Controllers.GameplayInput.SetTerminalState(false);
                var pivot = fixture.Host.GetComponent<CharacterVisualAssembler>().PresentationPivot;
                var body = CreateBody(pivot, true, out var bones);
                var baseline = Snapshot(bones);
                Assert.That(fixture.Adapter.Bind(body, pivot), Is.True);
                var clock = Time.unscaledTime;
                InvokeAdapter(fixture.Adapter, "OnAction", new CombatPresentationFact(1, CombatActionPhase.Windup, CombatPresentationEnd.None,
                    Vector3.forward, Quaternion.identity, null, .2f, Time.time, clock));
                fixture.Adapter.SamplePresentation(clock, Time.time, .016f, false, true, true);
                fixture.Health.TakeDamage(new DamageInfo(1, null, fixture.Host.transform.position), 0);
                fixture.Adapter.SamplePresentation(clock + .06f, Time.time + .06f, .016f, true, false, true);
                AssertBoneAngle(bones[0], baseline[0], 16f);
                var hitPose = Snapshot(bones);
                fixture.Adapter.SamplePresentation(clock + .06f, Time.time + .06f, .016f, true, false, true);
                Assert.That(Snapshot(bones), Is.EqualTo(hitPose));
                RealmRaiders.Controllers.GameplayInput.SetTerminalState(true);
                fixture.Adapter.SamplePresentation(clock + .07f, Time.time + .07f, .016f, false, true, false);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
                RealmRaiders.Controllers.GameplayInput.SetTerminalState(false);
                Assert.That(fixture.Adapter.ObserveCombat(Time.time + .08f, clock + .08f).AttackBlend, Is.Zero);
                fixture.Health.TakeDamage(new DamageInfo(10000, null, fixture.Host.transform.position), 0);
                fixture.Adapter.SamplePresentation(clock + .09f, Time.time + .09f, .016f, true, false, true);
                Assert.That(Quaternion.Angle(bones[0].localRotation, baseline[0].Rotation), Is.EqualTo(16).Within(.01f));
                fixture.Adapter.Clear();
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
            }
            finally { RealmRaiders.Controllers.GameplayInput.SetTerminalState(terminal); fixture.Dispose(); }
        }

        [TestCase(0f)]
        [TestCase(73f)]
        public void Adapter_SagittalReferenceSurvivesYaw180BodyAndRejectsForeignOrNullReference(float yaw)
        {
            var fixture = CreateFixture();
            var foreign = new GameObject("Foreign Reference");
            try
            {
                var assembler = fixture.Host.GetComponent<CharacterVisualAssembler>();
                var pivot = assembler.PresentationPivot;
                pivot.localRotation = Quaternion.Euler(0, yaw, 0);
                var body = CreateBody(pivot, true, out var bones);
                body.localRotation = Quaternion.Euler(0, 180, 0);
                var baseline = Snapshot(bones);
                var rootPose = Pose.Of(fixture.Host.transform);
                var pivotPose = Pose.Of(pivot); var bodyPose = Pose.Of(body);
                Assert.That(fixture.Adapter.Bind(body, pivot), Is.True);
                fixture.Host.transform.position += Vector3.forward * .2f;
                fixture.Adapter.SamplePresentation(1, 1, .05f, false, true, true);
                var dynamics = fixture.Host.GetComponent<CharacterVisualMotion>().SampleFactualDynamics(1, .05f, true, CharacterJumpPresentationSample.None);
                var swing = Mathf.Sin(dynamics.Phase) * dynamics.Speed;
                AssertBoneAngle(bones[2], baseline[2], -50 * swing);
                AssertBoneAngle(bones[3], baseline[3], 50 * swing);
                Assert.That(Pose.Of(pivot), Is.EqualTo(pivotPose));
                Assert.That(Pose.Of(body), Is.EqualTo(bodyPose));
                Assert.That(fixture.Host.transform.rotation, Is.EqualTo(rootPose.Rotation));

                Assert.That(fixture.Adapter.Bind(body, foreign.transform), Is.False);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
                Assert.That(fixture.Adapter.IsBound, Is.False);
                Assert.That(assembler.VisualRoot.gameObject.activeSelf, Is.True);
                Assert.That(pivot.GetComponentsInChildren<Renderer>().Length, Is.GreaterThan(0), "Failure leaves the assembled fallback present.");
                Assert.That(fixture.Adapter.Bind(body, null), Is.False);
                Assert.That(fixture.Adapter.Bind(null, pivot), Is.False);
                Assert.That(fixture.Adapter.Bind(body, body), Is.False);
                Assert.That(fixture.Adapter.Bind(body, pivot), Is.True);
                fixture.Adapter.Clear();
                InvokeAdapter(fixture.Adapter, "OnEnable");
                Assert.That(fixture.Adapter.IsBound, Is.False, "Clear drops both references and cannot resurrect a binding.");
            }
            finally { Object.DestroyImmediate(foreign); fixture.Dispose(); }
        }
        [Test]
        public void VisualMotion_BreathingCannotOverrideProgressiveJumpScale()
        {
            var fixture = CreateFixture();
            try
            {
                var motion = fixture.Host.GetComponent<CharacterVisualMotion>();
                var pivot = motion.PresentationPivot;
                var timeline = fixture.Host.GetComponent<CharacterJumpPresentationTimeline>();
                timeline.Configure(4f, 3f);
                for (var cycle = 0; cycle < 4; cycle++)
                {
                    motion.Restore();
                    var start = cycle * 2f;
                    motion.Sample(start, .01f, Vector3.zero, CombatActionPhase.Idle, false, true, true);
                    Assert.That(pivot.localScale, Is.EqualTo(motion.BaseScale), "Ordinary breathing translates the pivot without changing its neutral scale.");
                    motion.Sample(start, .01f, Vector3.zero, CombatActionPhase.Idle, true, false, true);
                    Assert.That(pivot.localScale, Is.EqualTo(motion.BaseScale), "Takeoff/0 joins the neutral scale exactly.");
                    motion.Sample(start + .0001f, .01f, Vector3.zero, CombatActionPhase.Idle, true, false, true);
                    Assert.That(pivot.localScale.y, Is.GreaterThan(motion.BaseScale.y), "Even small positive takeoff progress stretches upward, independently of the breathing clock.");
                    var earlyScale = pivot.localScale.y;
                    motion.Sample(start + .15f, .01f, Vector3.zero, CombatActionPhase.Idle, true, false, true);
                    Assert.That(pivot.localScale.y, Is.GreaterThan(earlyScale));
                    motion.Sample(start + .30f, .01f, Vector3.zero, CombatActionPhase.Idle, true, false, true);
                    var pushScale = pivot.localScale;
                    motion.Sample(start + .40f, .01f, Vector3.zero, CombatActionPhase.Idle, true, false, true);
                    Assert.That(Vector3.Distance(pivot.localScale, pushScale), Is.LessThan(.00001f));
                    motion.Sample(start + .80f, .01f, Vector3.zero, CombatActionPhase.Idle, true, false, true);
                    Assert.That(Vector3.Distance(pivot.localScale, motion.BaseScale), Is.LessThan(.00001f));
                    motion.Sample(start + .80f, .01f, Vector3.zero, CombatActionPhase.Idle, false, true, true);
                    Assert.That(Vector3.Distance(pivot.localScale, motion.BaseScale), Is.LessThan(.00001f), "Landing/0 joins Falling/1 continuously.");
                    motion.Sample(start + 1.08f, .01f, Vector3.zero, CombatActionPhase.Idle, false, true, true);
                    Assert.That(pivot.localScale.y, Is.LessThan(motion.BaseScale.y));
                    motion.Sample(start + 1.361f, .01f, Vector3.zero, CombatActionPhase.Idle, false, true, true);
                    Assert.That(pivot.localScale, Is.EqualTo(motion.BaseScale), "Recovery returns to the same neutral scale without a breathing-scale step.");
                }
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void MotionDynamics_DistanceDrivesPhaseAndStopSettlesBeforeNeutralRestart()
        {
            var motion = new CharacterMotionDynamics();
            motion.Step(Vector3.zero, 0, 20f, 4f);
            Assert.That(motion.Phase, Is.Zero);
            Assert.That(motion.Speed, Is.Zero);
            motion.Step(Vector3.forward * .2f, 0, .05f, 4f);
            Assert.That(motion.Phase, Is.EqualTo(.375f).Within(.00001f));
            Assert.That(motion.Speed, Is.InRange(.1f, .9f), "Visual onset ramps rather than jumping to full stride.");
            Assert.That(motion.WeightPitch, Is.GreaterThan(0));
            var phase = motion.Phase;
            var weight = motion.Speed;
            motion.Step(Vector3.zero, 0, .05f, 4f);
            Assert.That(motion.Phase, Is.EqualTo(phase), "A stopped body cannot keep marching during settle.");
            Assert.That(motion.Speed, Is.GreaterThan(0).And.LessThan(weight));
            motion.Step(Vector3.zero, 0, 2f, 4f);
            Assert.That(motion.Speed, Is.Zero);
            Assert.That(motion.Phase, Is.Zero);
            Assert.That(motion.WeightPitch, Is.Zero);
            motion.Step(Vector3.forward * .2f, 0, .05f, 4f);
            Assert.That(motion.Phase, Is.EqualTo(phase));
            Assert.That(motion.Speed, Is.EqualTo(weight));
            motion.Step(Vector3.up * 5, 0, .05f, 4f);
            Assert.That(motion.Phase, Is.EqualTo(phase), "Vertical physics cannot advance the horizontal gait.");
        }

        [Test]
        public void MotionDynamics_EqualTravelAndTimeMatchAcrossFrameRatesWithSignedBoundedTurnAndBrake()
        {
            var coarse = new CharacterMotionDynamics();
            var fine = new CharacterMotionDynamics();
            for (var i = 0; i < 6; i++) coarse.Step(Vector3.forward * .2f, 4.5f, .05f, 4f);
            for (var i = 0; i < 36; i++) fine.Step(Vector3.forward / 30f, .75f, 1f / 120f, 4f);
            AssertDynamicsEqual(coarse, fine);
            Assert.That(coarse.TurnLean, Is.LessThan(0).And.GreaterThanOrEqualTo(-CharacterMotionDynamics.MaximumTurnLean));
            for (var i = 0; i < 2; i++) coarse.Step(Vector3.zero, 0, .05f, 4f);
            for (var i = 0; i < 12; i++) fine.Step(Vector3.zero, 0, 1f / 120f, 4f);
            AssertDynamicsEqual(coarse, fine);
            Assert.That(coarse.WeightPitch, Is.LessThan(0).And.GreaterThanOrEqualTo(-CharacterMotionDynamics.MaximumWeightPitch));
            var left = new CharacterMotionDynamics();
            left.Step(Vector3.zero, -180f, .1f, 4f);
            Assert.That(left.TurnLean, Is.GreaterThan(0).And.LessThanOrEqualTo(CharacterMotionDynamics.MaximumTurnLean));
            left.Step(Vector3.zero, 0, 2f, 4f);
            Assert.That(left.TurnLean, Is.Zero);
            left.Step(new Vector3(float.NaN, 0, 0), 0, .1f, 4f);
            Assert.That(left.Phase, Is.Zero);
            Assert.That(left.Speed, Is.Zero);
        }

        static void AssertDynamicsEqual(CharacterMotionDynamics a, CharacterMotionDynamics b)
        {
            Assert.That(a.Phase, Is.EqualTo(b.Phase).Within(.00002f));
            Assert.That(a.Speed, Is.EqualTo(b.Speed).Within(.00002f));
            Assert.That(a.ForwardSpeed, Is.EqualTo(b.ForwardSpeed).Within(.00002f));
            Assert.That(a.WeightPitch, Is.EqualTo(b.WeightPitch).Within(.00002f));
            Assert.That(a.TurnLean, Is.EqualTo(b.TurnLean).Within(.00002f));
        }

        [Test]
        public void Adapter_SharedFactualGaitIgnoresGlobalClockCounterSwingsAndClearsOnControlChange()
        {
            var fixture = CreateFixture();
            try
            {
                var motion = fixture.Host.GetComponent<CharacterVisualMotion>();
                var pivot = motion.PresentationPivot;
                var body = CreateBody(pivot, true, out var bones);
                var baseline = Snapshot(bones);
                var pivotPose = Pose.Of(pivot);
                var bodyPose = Pose.Of(body);
                var motor = fixture.Host.GetComponent<CharacterController>();
                var center = motor.center; var height = motor.height; var radius = motor.radius;
                Assert.That(fixture.Adapter.Bind(body, pivot), Is.True);
                fixture.Host.transform.position += Vector3.forward * .2f;
                fixture.Adapter.SamplePresentation(1f, 999f, .05f, false, true, true);
                var first = Snapshot(bones);
                var shared = motion.SampleFactualDynamics(1f, .05f, true, CharacterJumpPresentationSample.None);
                var phase = shared.Phase; var speed = shared.Speed;
                fixture.Adapter.SamplePresentation(1f, -1000f, .05f, false, true, true);
                Assert.That(Snapshot(bones), Is.EqualTo(first), "Global clock and second consumer cannot advance the per-entity gait.");
                Assert.That(shared.Phase, Is.EqualTo(phase));
                Assert.That(shared.Speed, Is.EqualTo(speed));
                var swing = Mathf.Sin(phase) * speed;
                AssertBoneAngle(bones[0], baseline[0], 56f * swing);
                AssertBoneAngle(bones[1], baseline[1], -56f * swing);
                AssertBoneAngle(bones[2], baseline[2], -50f * swing);
                AssertBoneAngle(bones[3], baseline[3], 50f * swing);
                AssertBoneAngle(bones[4], baseline[4], 26f * swing);
                AssertBoneAngle(bones[5], baseline[5], -26f * swing);
                Assert.That(Pose.Of(pivot), Is.EqualTo(pivotPose));
                Assert.That(Pose.Of(body), Is.EqualTo(bodyPose));
                Assert.That(motor.center, Is.EqualTo(center)); Assert.That(motor.height, Is.EqualTo(height)); Assert.That(motor.radius, Is.EqualTo(radius));

                fixture.Adapter.SamplePresentation(2f, 500f, .05f, false, true, true);
                Assert.That(shared.Phase, Is.EqualTo(phase));
                Assert.That(shared.Speed, Is.LessThan(speed));
                fixture.Adapter.SamplePresentation(3f, 500f, 2f, false, true, true);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline), "Stopped limbs settle exactly, including idle arms.");
                fixture.Host.transform.position += Vector3.forward * .2f;
                fixture.Adapter.SamplePresentation(4f, 123f, .05f, false, true, true);
                Assert.That(Snapshot(bones), Is.EqualTo(first), "Restart begins from the same neutral phase regardless of global clock.");
                fixture.Adapter.SamplePresentation(5f, 124f, .05f, false, true, false);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
                Assert.That(shared.Phase, Is.Zero);
                fixture.Host.transform.position += Vector3.forward * .2f;
                fixture.Adapter.SamplePresentation(6f, 125f, .05f, false, true, true);
                Assert.That(shared.Speed, Is.GreaterThan(0));
                fixture.Adapter.Clear();
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
                Assert.That(shared.Speed, Is.Zero);
                Assert.That(shared.Phase, Is.Zero);
                Assert.That(fixture.Adapter.Bind(body, pivot), Is.True);
                fixture.Adapter.SamplePresentation(7f, 126f, .05f, false, true, true);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline), "Rebinding cannot resume stale gait travel.");
            }
            finally { fixture.Dispose(); }
        }

        static void AssertBoneAngle(Transform bone, Pose baseline, float angle)
        {
            Assert.That(Quaternion.Angle(bone.localRotation, baseline.Rotation * Quaternion.AngleAxis(angle, baseline.SemanticAxis)), Is.LessThan(.01f));
        }

        [Test]
        public void Adapter_BindsExactBonesFailsClosedAndRestoresWithoutMovingRootOrPivot()
        {
            var fixture = CreateFixture();
            try
            {
                var pivot = fixture.Host.GetComponent<CharacterVisualAssembler>().PresentationPivot;
                var body = CreateBody(pivot, true, out var bones);
                var rootPosition = fixture.Host.transform.position;
                var pivotPose = Pose.Of(pivot);
                var bodyPose = Pose.Of(body);
                var baseline = Snapshot(bones);
                var collidersBefore = body.GetComponentsInChildren<Collider>(true).Length;

                Assert.That(fixture.Adapter.Bind(body, pivot), Is.True);
                Assert.That(fixture.Host.transform.position, Is.EqualTo(rootPosition));
                Assert.That(Pose.Of(pivot), Is.EqualTo(pivotPose));
                Assert.That(Pose.Of(body), Is.EqualTo(bodyPose));
                Assert.That(body.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(collidersBefore));

                fixture.Adapter.Clear();
                Assert.That(fixture.Adapter.IsBound, Is.False);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
                Assert.That(fixture.Adapter.Bind(body, pivot), Is.True, "An explicit rebind must restore the same exact binding without a hierarchy scan.");

                fixture.Adapter.Clear();
                Assert.That(fixture.Adapter.IsBound, Is.False);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));

                var incomplete = CreateBody(pivot, false, out var incompleteBones);
                var incompleteBaseline = Snapshot(incompleteBones);
                Assert.That(fixture.Adapter.Bind(incomplete, pivot), Is.False);
                Assert.That(Snapshot(incompleteBones), Is.EqualTo(incompleteBaseline));
            }
            finally { fixture.Dispose(); }
        }

        [Test]
        public void JumpPresentationTimeline_UsesSharedDeterministicFourXDurationsAndResets()
        {
            var host = new GameObject("Jump Presentation Timeline Test");
            try
            {
                var timeline = host.AddComponent<CharacterJumpPresentationTimeline>();
                timeline.Configure(4f, 3f);
                Assert.That(timeline.StraightenSeconds, Is.EqualTo(.30f).Within(.0001f));
                Assert.That(timeline.TakeoffSeconds, Is.EqualTo(.40f).Within(.0001f));
                Assert.That(timeline.LandingSeconds, Is.EqualTo(.56f).Within(.0001f));

                Assert.That(timeline.Observe(0f, false, true, true).Phase, Is.EqualTo(CharacterJumpPresentationPhase.None));
                AssertSample(timeline.Observe(0f, true, false, true), CharacterJumpPresentationPhase.Takeoff, 0f);
                AssertSample(timeline.Observe(0f, true, false, true), CharacterJumpPresentationPhase.Takeoff, 0f);
                AssertSample(timeline.Observe(.15f, true, false, true), CharacterJumpPresentationPhase.Takeoff, .5f);
                AssertSample(timeline.Observe(.30f, true, false, true), CharacterJumpPresentationPhase.Takeoff, 1f);
                AssertSample(timeline.Observe(.40f, true, false, true), CharacterJumpPresentationPhase.Falling, 0f);
                AssertSample(timeline.Observe(.60f, true, false, true), CharacterJumpPresentationPhase.Falling, .5f);
                AssertSample(timeline.Observe(.80f, true, false, true), CharacterJumpPresentationPhase.Falling, 1f);
                AssertSample(timeline.Observe(.80f, false, true, true), CharacterJumpPresentationPhase.Landing, 0f);
                AssertSample(timeline.Observe(1.08f, false, true, true), CharacterJumpPresentationPhase.Landing, .5f);
                AssertSample(timeline.Observe(1.36f, false, true, true), CharacterJumpPresentationPhase.Landing, 1f);
                AssertSample(timeline.Observe(1.36f, false, true, true), CharacterJumpPresentationPhase.Landing, 1f);
                Assert.That(timeline.Observe(1.361f, false, true, true).Phase, Is.EqualTo(CharacterJumpPresentationPhase.None));

                timeline.ResetTimeline();
                Assert.That(timeline.Observe(0f, false, true, true).Phase, Is.EqualTo(CharacterJumpPresentationPhase.None));
                AssertSample(timeline.Observe(0f, true, false, true), CharacterJumpPresentationPhase.Takeoff, 0f);
                AssertSample(timeline.Observe(.40f, true, false, true), CharacterJumpPresentationPhase.Falling, 0f);
                AssertSample(timeline.Observe(.60f, false, true, true), CharacterJumpPresentationPhase.Falling, .5f);
                AssertSample(timeline.Observe(.70f, false, true, true), CharacterJumpPresentationPhase.Falling, .75f);
                AssertSample(timeline.Observe(.80f, false, true, true), CharacterJumpPresentationPhase.Landing, 0f);
                AssertSample(timeline.Observe(.80f, false, true, true), CharacterJumpPresentationPhase.Landing, 0f);

                Assert.That(timeline.Observe(1.37f, true, false, false).Phase, Is.EqualTo(CharacterJumpPresentationPhase.None));
                Assert.That(timeline.Observe(1.38f, true, false, true).Phase, Is.EqualTo(CharacterJumpPresentationPhase.None), "Control loss must not resume an old visual jump.");

                timeline.Configure(1f, 1f);
                timeline.Observe(0f, false, true, true);
                timeline.Observe(.01f, true, false, true);
                AssertSample(timeline.Observe(.20f, false, true, true), CharacterJumpPresentationPhase.Falling, .9f);
                // Skip the scheduled .21 landing boundary: both consumers must catch up at .28.
                AssertSample(timeline.Observe(.28f, false, true, true), CharacterJumpPresentationPhase.Landing, .5f);
                AssertSample(timeline.Observe(.28f, false, true, true), CharacterJumpPresentationPhase.Landing, .5f);
            }
            finally { Object.DestroyImmediate(host); }
        }

        static void AssertSample(CharacterJumpPresentationSample actual, CharacterJumpPresentationPhase phase, float progress)
        {
            Assert.That(actual.Phase, Is.EqualTo(phase));
            Assert.That(actual.Progress, Is.EqualTo(progress).Within(.0001f));
        }

        // EditMode does not run this ordinary MonoBehaviour through Unity's SendMessage dispatch.
        // Invoke only the named presentation handler; real lifecycle dispatch remains PlayMode-covered.
        static void InvokeAdapter(CharacterProceduralMotionAdapter adapter, string method, params object[] arguments)
        {
            var handler = typeof(CharacterProceduralMotionAdapter).GetMethod(method,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(handler, Is.Not.Null, method);
            handler.Invoke(adapter, arguments);
        }

        private static Fixture CreateFixture()
        {
            var host = new GameObject("Procedural Motion Adapter Edit Host");
            host.AddComponent<CharacterController>();
            host.AddComponent<Health>();
            var entity = host.AddComponent<CombatEntity>();
            var recipe = ScriptableObject.CreateInstance<CharacterVisualRecipe>();
            recipe.Family = CharacterVisualFamily.Humanoid;
            recipe.Primary = Color.red;
            recipe.Secondary = Color.black;
            recipe.AccentColor = Color.yellow;
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>();
            definition.Stats = CombatStats.BloodKnight;
            definition.VisualRecipe = recipe;
            entity.Initialize(definition);
            return new Fixture(host, host.GetComponent<Health>(), host.GetComponent<CharacterProceduralMotionAdapter>(), definition, recipe);
        }

        private static Transform CreateBody(Transform parent, bool complete, out Transform[] bones)
        {
            var body = new GameObject("Base Body").transform;
            body.SetParent(parent, false);
            var names = new[]
            {
                "Bip01 L UpperArm", "Bip01 R UpperArm", "Bip01 L Thigh",
                "Bip01 R Thigh", "Bip01 L Calf", "Bip01 R Calf"
            };
            var count = complete ? names.Length : names.Length - 1;
            bones = new Transform[count];
            for (var index = 0; index < count; index++)
            {
                bones[index] = new GameObject(names[index]).transform;
                bones[index].SetParent(body, false);
                bones[index].localRotation = Quaternion.Euler(index, index * 2, index * 3);
            }
            return body;
        }

        static Transform AddTorsoChain(Transform body, Transform[] bones)
        {
            Transform Child(string name, Transform parent)
            {
                var child = new GameObject(name).transform; child.SetParent(parent, false); return child;
            }
            var skeleton = Child("Bip01", body);
            var pelvis = Child("Bip01 Pelvis", skeleton);
            var spine = Child("Bip01 Spine", pelvis);
            var torso = Child("Bip01 Spine1", spine);
            torso.localRotation = Quaternion.Euler(17, -11, 6);
            var neck = Child("Bip01 Neck", torso);
            bones[0].SetParent(neck, false); bones[1].SetParent(neck, false);
            bones[2].SetParent(pelvis, false); bones[3].SetParent(pelvis, false);
            bones[4].SetParent(bones[2], false); bones[5].SetParent(bones[3], false);
            return torso;
        }

        private static Pose[] Snapshot(Transform[] bones)
        {
            var result = new Pose[bones.Length];
            for (var index = 0; index < bones.Length; index++) result[index] = Pose.Of(bones[index]);
            return result;
        }

        private readonly struct Pose : System.IEquatable<Pose>
        {
            public Pose(Vector3 position, Quaternion rotation, Vector3 scale, Vector3 semanticAxis) { Position = position; Rotation = rotation; Scale = scale; SemanticAxis = semanticAxis; }
            public Vector3 SemanticAxis { get; }
            public Vector3 Position { get; }
            public Quaternion Rotation { get; }
            public Vector3 Scale { get; }
            public static Pose Of(Transform target)
            {
                var reference = target.GetComponentInParent<CharacterVisualAssembler>().PresentationPivot;
                return new(target.localPosition, target.localRotation, target.localScale, Quaternion.Inverse(target.rotation) * reference.right);
            }
            public bool Equals(Pose other) => Position == other.Position && Rotation == other.Rotation && Scale == other.Scale;
            public override bool Equals(object value) => value is Pose other && Equals(other);
            public override int GetHashCode() => Position.GetHashCode() ^ Rotation.GetHashCode() ^ Scale.GetHashCode();
        }

        sealed class TestTuningProvider : IProceduralHumanoidTuningProvider
        {
            public TestTuningProvider(string providerId, params ProceduralHumanoidTuningProfile[] profiles)
            { ProviderId = providerId; Profiles = profiles; }

            public string ProviderId { get; }
            public IReadOnlyList<ProceduralHumanoidTuningProfile> Profiles { get; }
        }

        private sealed class Fixture
        {
            public Fixture(GameObject host, Health health, CharacterProceduralMotionAdapter adapter, CharacterDefinition definition, CharacterVisualRecipe recipe)
            { Host = host; Health = health; Adapter = adapter; Definition = definition; Recipe = recipe; }
            public GameObject Host { get; }
            public Health Health { get; }
            public CharacterProceduralMotionAdapter Adapter { get; }
            CharacterDefinition Definition { get; }
            CharacterVisualRecipe Recipe { get; }
            public void Dispose()
            {
                Object.DestroyImmediate(Host);
                Object.DestroyImmediate(Definition);
                Object.DestroyImmediate(Recipe);
            }
        }
    }
}
