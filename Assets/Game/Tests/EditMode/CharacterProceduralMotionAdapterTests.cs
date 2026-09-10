using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class CharacterProceduralMotionAdapterTests
    {
        [Test]
        public void Adapter_BindsExactBonesFailsClosedAndRestoresWithoutMovingRootOrPivot()
        {
            var fixture = CreateFixture();
            try
            {
                var pivot = new GameObject("Adapter Pivot").transform;
                pivot.SetParent(fixture.Host.transform, false);
                var body = CreateBody(pivot, true, out var bones);
                var rootPosition = fixture.Host.transform.position;
                var pivotPose = Pose.Of(pivot);
                var bodyPose = Pose.Of(body);
                var baseline = Snapshot(bones);
                var collidersBefore = body.GetComponentsInChildren<Collider>(true).Length;

                Assert.That(fixture.Adapter.Bind(body), Is.True);
                Assert.That(fixture.Host.transform.position, Is.EqualTo(rootPosition));
                Assert.That(Pose.Of(pivot), Is.EqualTo(pivotPose));
                Assert.That(Pose.Of(body), Is.EqualTo(bodyPose));
                Assert.That(body.GetComponentsInChildren<Collider>(true).Length, Is.EqualTo(collidersBefore));

                fixture.Adapter.Clear();
                Assert.That(fixture.Adapter.IsBound, Is.False);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));
                Assert.That(fixture.Adapter.Bind(body), Is.True, "An explicit rebind must restore the same exact binding without a hierarchy scan.");

                fixture.Adapter.Clear();
                Assert.That(fixture.Adapter.IsBound, Is.False);
                Assert.That(Snapshot(bones), Is.EqualTo(baseline));

                var incomplete = CreateBody(pivot, false, out var incompleteBones);
                var incompleteBaseline = Snapshot(incompleteBones);
                Assert.That(fixture.Adapter.Bind(incomplete), Is.False);
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

        private static Pose[] Snapshot(Transform[] bones)
        {
            var result = new Pose[bones.Length];
            for (var index = 0; index < bones.Length; index++) result[index] = Pose.Of(bones[index]);
            return result;
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
