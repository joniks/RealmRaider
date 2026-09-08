using System.Collections;
using NUnit.Framework;
using RealmRaiders.AI;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using UnityEngine;
using UnityEngine.TestTools;

namespace RealmRaiders.Tests
{
    public sealed class RaidInvaderRecoveryFlowTests
    {
        [UnityTest]
        public IEnumerator CharacterControllerRoutesAroundSingleBlockerWithoutSkippingOrLeavingCorridor()
        {
            GameplayInput.SetTerminalState(false);
            var existingColliders = Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
            foreach (var existingCollider in existingColliders) existingCollider.enabled = false;
            var route = new[] { new Vector3(0, 1, 6), new Vector3(0, 1, 10) };
            var fixture = new InvaderFixture("Recovery Invader", new Vector3(0, 1, 0), 4, route);
            var ground = CreateGround();
            var blocker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blocker.name = "Recovery Route Blocker"; blocker.transform.position = new Vector3(0, 1, 2.3f); blocker.transform.localScale = new Vector3(1.8f, 2, 1.1f);
            try
            {
                Physics.SyncTransforms();
                var sawRecovery = false;
                var sawControllerReset = false;
                var rootApplied = false;
                var rootHeldRecovery = false;
                var rootResumedRecovery = false;
                var rootedSide = 0;
                var rootedPosition = Vector3.zero;
                var wasRooted = false;
                var previousIndex = fixture.Brain.WaypointIndex;
                var previousPosition = fixture.Root.transform.position;
                var timeout = Time.time + 9f;
                while (fixture.Brain.WaypointIndex < route.Length && Time.time < timeout)
                {
                    yield return null;
                    var position = fixture.Root.transform.position;
                    var horizontalStep = Vector2.Distance(new Vector2(previousPosition.x, previousPosition.z), new Vector2(position.x, position.z));
                    Assert.That(horizontalStep, Is.LessThanOrEqualTo(fixture.MoveSpeed * Mathf.Max(Time.deltaTime, .05f) + .08f), "Recovery must never teleport the authoritative root.");
                    Assert.That(fixture.Brain.WaypointIndex, Is.InRange(previousIndex, previousIndex + 1), "Waypoint order must advance one existing slot at a time.");
                    var segmentStart = fixture.Brain.WaypointIndex > 0 ? route[fixture.Brain.WaypointIndex - 1] : fixture.Start;
                    var waypoint = fixture.Brain.WaypointIndex < route.Length ? route[fixture.Brain.WaypointIndex] : route[route.Length - 1];
                    Assert.That(Mathf.Abs(InvaderStuckRecovery.CorridorOffset(position, segmentStart, waypoint)), Is.LessThanOrEqualTo(InvaderStuckRecovery.CorridorHalfWidth + .08f));
                    if (fixture.Brain.IsRecovering)
                    {
                        sawRecovery = true;
                        if (!sawControllerReset)
                        {
                            fixture.Brain.SetControl(false);
                            Assert.That(fixture.Brain.IsRecovering, Is.False);
                            fixture.Brain.SetControl(true);
                            Assert.That(fixture.Brain.IsRecovering, Is.False);
                            sawControllerReset = true;
                        }
                        else if (!rootApplied)
                        {
                            rootedSide = fixture.Brain.RecoverySide;
                            rootedPosition = Horizontal(position);
                            fixture.Entity.ApplyRoot(.2f);
                            rootApplied = true;
                        }
                    }
                    if (fixture.Entity.IsRooted)
                    {
                        rootHeldRecovery = true;
                        Assert.That(fixture.Brain.IsRecovering, Is.True);
                        Assert.That(fixture.Brain.RecoverySide, Is.EqualTo(rootedSide));
                        Assert.That(Vector3.Distance(Horizontal(position), rootedPosition), Is.LessThan(.001f));
                    }
                    if (wasRooted && !fixture.Entity.IsRooted) rootResumedRecovery = fixture.Brain.IsRecovering;
                    wasRooted = fixture.Entity.IsRooted;
                    previousIndex = fixture.Brain.WaypointIndex;
                    previousPosition = position;
                }

                Assert.That(sawRecovery, Is.True, "The primitive must exercise the stuck-recovery path.");
                Assert.That(sawControllerReset, Is.True);
                Assert.That(rootApplied && rootHeldRecovery && rootResumedRecovery, Is.True, "Root must pause and then resume the same recovery state.");
                Assert.That(fixture.Brain.WaypointIndex, Is.EqualTo(route.Length), $"Final root position {fixture.Root.transform.position}, recovery={fixture.Brain.IsRecovering}, side={fixture.Brain.RecoverySide}, stuck={fixture.Brain.StuckSeconds:0.00}.");
                Assert.That(fixture.Root.transform.position.z, Is.GreaterThan(9.3f));
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(fixture.Entity.Health.Maximum));
            }
            finally
            {
                Object.Destroy(blocker);
                Object.Destroy(ground);
                fixture.Dispose();
                foreach (var existingCollider in existingColliders)
                    if (existingCollider) existingCollider.enabled = true;
                GameplayInput.SetTerminalState(false);
            }
        }

        [UnityTest]
        public IEnumerator SuppressedStatesNeverAccumulateRecoveryOrChangeCombatFacts()
        {
            GameplayInput.SetTerminalState(false);
            var route = new[] { new Vector3(0, 1, 100) };
            var fixture = new InvaderFixture("Suppressed Invader", new Vector3(0, 1, 0), 4, route, true);
            var defender = new InvaderFixture("Detection Defender", new Vector3(4, 1, 0), 0, System.Array.Empty<Vector3>());
            var ground = CreateGround();
            try
            {
                Physics.SyncTransforms();
                fixture.Brain.Configure(route, new[] { defender.Entity }, 10);
                fixture.Entity.SetController(fixture.Brain);
                var start = Horizontal(fixture.Root.transform.position);
                TickMany(fixture.Brain);
                Assert.That(fixture.Brain.IsOpeningHold, Is.True);
                Assert.That(fixture.Brain.IsRecovering, Is.False);
                Assert.That(Vector3.Distance(Horizontal(fixture.Root.transform.position), start), Is.LessThan(.001f));

                var pauseRoute = new[] { fixture.Root.transform.position, fixture.Root.transform.position + Vector3.forward * 100 };
                fixture.Brain.Configure(pauseRoute, System.Array.Empty<CombatEntity>(), 0);
                fixture.Entity.SetController(fixture.Brain);
                fixture.Brain.Tick();
                Assert.That(fixture.Brain.WaypointIndex, Is.EqualTo(1));
                TickMany(fixture.Brain);
                Assert.That(fixture.Brain.IsRecovering, Is.False, "The existing waypoint pause cannot count as stuck time.");

                fixture.Brain.Configure(route, System.Array.Empty<CombatEntity>(), 0);
                fixture.Entity.SetController(fixture.Brain);
                fixture.Entity.ApplyRoot(10);
                start = Horizontal(fixture.Root.transform.position);
                TickMany(fixture.Brain);
                Assert.That(fixture.Entity.IsRooted, Is.True);
                Assert.That(fixture.Brain.IsRecovering, Is.False);
                Assert.That(Vector3.Distance(Horizontal(fixture.Root.transform.position), start), Is.LessThan(.001f));

                fixture.Entity.BreakRoot();
                fixture.Brain.Configure(route, System.Array.Empty<CombatEntity>(), 0);
                fixture.Entity.SetController(fixture.Brain);
                Assert.That(fixture.Entity.TryUse(0, Vector3.forward), Is.True);
                var healthBefore = fixture.Entity.Health.Current;
                TickMany(fixture.Brain);
                Assert.That(fixture.Entity.IsActionResolving, Is.True);
                Assert.That(fixture.Brain.IsRecovering, Is.False);
                Assert.That(fixture.Entity.Health.Current, Is.EqualTo(healthBefore));

                fixture.Entity.SetController(null);
                defender.Brain.Configure(route, System.Array.Empty<CombatEntity>(), 0);
                defender.Entity.SetController(defender.Brain);
                TickMany(defender.Brain);
                Assert.That(defender.Brain.IsRecovering, Is.False, "Zero movement speed is zero route intent, not a stuck condition.");
                defender.Root.transform.position = fixture.Root.transform.position + Vector3.right * 4;
                Physics.SyncTransforms();
                fixture.Brain.Configure(route, new[] { defender.Entity }, 0);
                fixture.Entity.SetController(fixture.Brain);
                TickMany(fixture.Brain);
                Assert.That(fixture.Brain.CurrentTarget, Is.EqualTo(defender.Entity));
                Assert.That(fixture.Brain.IsRecovering, Is.False);
                Assert.That(defender.Entity.Health.Current, Is.EqualTo(defender.Entity.Health.Maximum));

                GameplayInput.SetTerminalState(true);
                start = Horizontal(fixture.Root.transform.position);
                TickMany(fixture.Brain);
                Assert.That(fixture.Brain.IsRecovering, Is.False);
                Assert.That(Vector3.Distance(Horizontal(fixture.Root.transform.position), start), Is.LessThan(.001f));

                GameplayInput.SetTerminalState(false);
                fixture.Entity.Health.TakeDamage(new DamageInfo(1000, null, fixture.Root.transform.position), 0);
                Assert.That(fixture.Entity.Health.IsDead, Is.True);
                Assert.That(fixture.Brain.IsRecovering, Is.False);
                fixture.Brain.Tick();
                Assert.That(fixture.Brain.CurrentTarget, Is.Null);
            }
            finally
            {
                fixture.Dispose();
                defender.Dispose();
                Object.Destroy(ground);
                GameplayInput.SetTerminalState(false);
            }
            yield return null;
        }

        static void TickMany(RaidInvaderBrain brain)
        {
            for (var step = 0; step < 100; step++) brain.Tick();
        }

        static Vector3 Horizontal(Vector3 value) { value.y = 0; return value; }

        static GameObject CreateGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Recovery Test Ground";
            ground.transform.localScale = new Vector3(3, 1, 5);
            return ground;
        }

        sealed class InvaderFixture
        {
            public readonly GameObject Root;
            public readonly CombatEntity Entity;
            public readonly RaidInvaderBrain Brain;
            public readonly CharacterDefinition Definition;
            public readonly AbilityDefinition Ability;
            public readonly Vector3 Start;
            public readonly float MoveSpeed;

            public InvaderFixture(string name, Vector3 position, float moveSpeed, Vector3[] route, bool withAbility = false)
            {
                Start = position; MoveSpeed = moveSpeed;
                Root = new GameObject(name, typeof(CharacterController), typeof(Health), typeof(CombatEntity), typeof(RaidInvaderBrain));
                Root.transform.position = position;
                Definition = ScriptableObject.CreateInstance<CharacterDefinition>();
                Definition.DisplayName = name;
                Definition.Stats = new CombatStats { MaxHealth = 100, MoveSpeed = moveSpeed, AttackSpeed = 1 };
                if (withAbility)
                {
                    Ability = ScriptableObject.CreateInstance<AbilityDefinition>();
                    Ability.DisplayName = "Recovery Gate Attack"; Ability.Damage = 10; Ability.Range = 2; Ability.Radius = 1; Ability.Cooldown = 0; Ability.Windup = 10;
                    Definition.Abilities = new[] { Ability };
                }
                else Definition.Abilities = System.Array.Empty<AbilityDefinition>();
                Entity = Root.GetComponent<CombatEntity>(); Entity.Initialize(Definition);
                Brain = Root.GetComponent<RaidInvaderBrain>(); Brain.Configure(route, System.Array.Empty<CombatEntity>(), 0); Entity.SetController(Brain);
            }

            public void Dispose()
            {
                Object.Destroy(Root);
                Object.Destroy(Definition);
                if (Ability) Object.Destroy(Ability);
            }
        }
    }
}
