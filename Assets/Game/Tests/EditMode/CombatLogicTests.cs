using NUnit.Framework;
using RealmRaiders.Combat;
using RealmRaiders.Characters;
using UnityEngine;

namespace RealmRaiders.Tests
{
    public sealed class CombatLogicTests
    {
        GameObject subject;
        Health health;

        [SetUp]
        public void SetUp()
        { subject = new GameObject("Health test", typeof(Health)); health = subject.GetComponent<Health>(); health.Initialize(100); }

        [TearDown]
        public void TearDown() => Object.DestroyImmediate(subject);

        [Test]
        public void Damage_IsReducedByArmor()
        {
            health.TakeDamage(new DamageInfo(100, null, Vector3.zero), 100);
            Assert.That(health.Current, Is.EqualTo(50).Within(.01f));
        }

        [Test]
        public void Death_FiresOnce_AndHealthDoesNotGoNegative()
        {
            int deaths = 0; health.Died += () => deaths++;
            health.TakeDamage(new DamageInfo(500, null, Vector3.zero), 0);
            health.TakeDamage(new DamageInfo(500, null, Vector3.zero), 0);
            Assert.That(health.Current, Is.Zero); Assert.That(deaths, Is.EqualTo(1));
        }

        [Test]
        public void RestoreFull_UsesConfiguredMaximum()
        {
            health.TakeDamage(new DamageInfo(20, null, Vector3.zero), 0); health.RestoreFull();
            Assert.That(health.Current, Is.EqualTo(100));
        }

        [Test]
        public void ActionState_RejectsOverlapAndReturnsToIdle()
        {
            var state = new CombatActionState();
            Assert.That(state.TryBegin(), Is.True);
            Assert.That(state.Phase, Is.EqualTo(CombatActionPhase.Windup));
            Assert.That(state.TryBegin(), Is.False);
            state.Impact(); Assert.That(state.Phase, Is.EqualTo(CombatActionPhase.Impact));
            state.Recover(); Assert.That(state.Phase, Is.EqualTo(CombatActionPhase.Recovery));
            state.Complete(); Assert.That(state.Phase, Is.EqualTo(CombatActionPhase.Idle));
        }

        [Test]
        public void VisualRecipe_AssemblerIsDeterministicAndFallbackKeepsBaseVisual()
        {
            var recipe = ScriptableObject.CreateInstance<CharacterVisualRecipe>(); recipe.Family = CharacterVisualFamily.Beast; recipe.Head = VisualModuleStyle.Horns; recipe.Arms = VisualModuleStyle.Claws; recipe.Primary = Color.red; recipe.Secondary = Color.black; recipe.AccentColor = Color.yellow;
            var first = GameObject.CreatePrimitive(PrimitiveType.Capsule); var second = GameObject.CreatePrimitive(PrimitiveType.Capsule); var firstAssembler = first.AddComponent<CharacterVisualAssembler>(); var secondAssembler = second.AddComponent<CharacterVisualAssembler>();
            Assert.That(firstAssembler.Assemble(recipe), Is.True); Assert.That(secondAssembler.Assemble(recipe), Is.True);
            var firstModules = ModuleNames(first.transform); var secondModules = ModuleNames(second.transform);
            Assert.That(firstModules, Is.EqualTo(secondModules)); Assert.That(firstModules.Length, Is.GreaterThan(1));
            foreach (var collider in first.GetComponentsInChildren<Collider>(true)) if (collider.transform != first.transform) Assert.That(collider.enabled, Is.False);
            firstAssembler.Clear(); Assert.That(firstAssembler.Assemble(null), Is.False); Assert.That(first.GetComponent<Renderer>().enabled, Is.True);
            Object.DestroyImmediate(recipe); Object.DestroyImmediate(first); Object.DestroyImmediate(second);
        }

        static string[] ModuleNames(Transform entity)
        {
            var root = entity.Find("Character Visual Modules"); var names = new string[root.childCount];
            for (int i = 0; i < root.childCount; i++) names[i] = root.GetChild(i).name;
            return names;
        }
    }
}
