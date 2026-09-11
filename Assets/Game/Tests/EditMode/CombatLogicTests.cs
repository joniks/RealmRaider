using NUnit.Framework;
using RealmRaiders.Combat;
using RealmRaiders.Characters;
using RealmRaiders.Controllers;
using RealmRaiders.Core;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.UI;

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
            Assert.That(health.TakeDamage(new DamageInfo(100, null, Vector3.zero), 100), Is.True);
            Assert.That(health.Current, Is.EqualTo(50).Within(.01f));
        }

        [Test]
        public void Death_FiresOnce_AndHealthDoesNotGoNegative()
        {
            var events = new System.Collections.Generic.List<string>();
            health.Damaged += _ => events.Add("Damaged"); health.Changed += (_, _) => events.Add("Changed"); health.Died += () => events.Add("Died");
            Assert.That(health.TakeDamage(new DamageInfo(500, null, Vector3.zero), 0), Is.True);
            Assert.That(health.TakeDamage(new DamageInfo(500, null, Vector3.zero), 0), Is.False);
            Assert.That(health.Current, Is.Zero);
            Assert.That(events, Is.EqualTo(new[] { "Damaged", "Changed", "Died" }));
        }

        [Test]
        public void DamageResult_RejectsImmunityWithoutChangingHealthOrPublishingEvents()
        {
            var eventCount = 0; health.Damaged += _ => eventCount++; health.Changed += (_, _) => eventCount++; health.Died += () => eventCount++;
            var beginImmunity = typeof(Health).GetMethod("BeginDamageImmunity", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(beginImmunity, Is.Not.Null); beginImmunity.Invoke(health, new object[] { 1f });
            Assert.That(health.TakeDamage(new DamageInfo(25, null, Vector3.zero), 0), Is.False);
            Assert.That(health.Current, Is.EqualTo(100)); Assert.That(eventCount, Is.Zero);
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
        public void CombatInputBuffer_RejectsEmptyConsumesOnceAndExpiresAtExactBoundary()
        {
            var buffer = new CombatInputBuffer();
            Assert.That(buffer.TryConsume(0, out _), Is.False);
            Assert.That(buffer.TryQueue(-1, 1, 0, 0, 0), Is.False);
            Assert.That(buffer.HasPending, Is.False);

            Assert.That(buffer.TryQueue(2, 3, 0, 4, 10), Is.True);
            Assert.That(buffer.TryConsume(10 + CombatInputBuffer.WindowSeconds - .0001f, out var request), Is.True);
            Assert.That(request.AbilityIndex, Is.EqualTo(2));
            Assert.That(request.DirectionX, Is.EqualTo(.6f).Within(.0001f));
            Assert.That(request.DirectionY, Is.Zero);
            Assert.That(request.DirectionZ, Is.EqualTo(.8f).Within(.0001f));
            Assert.That(buffer.TryConsume(10 + CombatInputBuffer.WindowSeconds - .0001f, out _), Is.False);
            Assert.That(buffer.TryQueue(2, 1, 0, 0, 20), Is.True);
            Assert.That(buffer.TryConsume(20 + CombatInputBuffer.WindowSeconds, out _), Is.False, "The request expires at exactly 0.20 seconds.");
        }

        [Test]
        public void CombatInputBuffer_NewestRequestReplacesAndExpiryClears()
        {
            var buffer = new CombatInputBuffer();
            Assert.That(buffer.TryQueue(0, 1, 0, 0, 1), Is.True);
            Assert.That(buffer.TryQueue(-1, 0, 0, 1, 1.05f), Is.False);
            Assert.That(buffer.PendingAbilityIndex, Is.EqualTo(0), "A rejected request cannot replace a valid one.");
            Assert.That(buffer.TryQueue(1, 0, 0, -5, 1.1f), Is.True);
            Assert.That(buffer.PendingAbilityIndex, Is.EqualTo(1));
            Assert.That(buffer.TryConsume(1.1f + CombatInputBuffer.WindowSeconds - .0001f, out var replacement), Is.True);
            Assert.That(replacement.AbilityIndex, Is.EqualTo(1));
            Assert.That(replacement.DirectionZ, Is.EqualTo(-1));

            Assert.That(buffer.TryQueue(0, 1, 0, 0, 2), Is.True);
            Assert.That(buffer.Expire(2 + CombatInputBuffer.WindowSeconds - .0001f), Is.False);
            Assert.That(buffer.HasPending, Is.True);
            Assert.That(buffer.Expire(2 + CombatInputBuffer.WindowSeconds), Is.True);
            Assert.That(buffer.HasPending, Is.False);
            Assert.That(buffer.TryConsume(2, out _), Is.False);
        }

        [Test]
        public void CombatInputBuffer_SanitizesDirectionAndOwnsNoUnityAuthority()
        {
            var buffer = new CombatInputBuffer();
            Assert.That(buffer.TryQueue(0, float.NaN, 2, 3, 0), Is.True);
            Assert.That(buffer.TryConsume(0, out var request), Is.True);
            Assert.That(request.DirectionX, Is.Zero);
            Assert.That(request.DirectionY, Is.Zero);
            Assert.That(request.DirectionZ, Is.EqualTo(1));
            Assert.That(CombatInputBuffer.WindowSeconds, Is.EqualTo(.20f));
            Assert.That(typeof(UnityEngine.Object).IsAssignableFrom(typeof(CombatInputBuffer)), Is.False);
            foreach (var field in typeof(CombatInputBuffer).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
                Assert.That(field.FieldType.Namespace, Is.Not.EqualTo("UnityEngine"));
        }

        [Test]
        public void DodgePrototypeTuning_UsesTheBoundedEscapeContract()
        {
            Assert.That(CombatEntity.DodgeDistance, Is.EqualTo(2.6f));
            Assert.That(CombatEntity.DodgeDuration, Is.EqualTo(.18f));
            Assert.That(CombatEntity.DodgeImmunityDuration, Is.EqualTo(.18f));
            Assert.That(CombatEntity.DodgeCooldown, Is.EqualTo(1.5f));
        }

        [Test]
        public void JumpState_UsesBoundedDeterministicTuningAndRejectsOverlap()
        {
            Assert.That(CharacterJumpState.TakeoffSpeed, Is.EqualTo(6.4f));
            Assert.That(CharacterJumpState.Gravity, Is.EqualTo(-18f));
            Assert.That(CharacterJumpState.MaximumFallSpeed, Is.EqualTo(-20f));
            Assert.That(CharacterJumpState.MaximumAirborneSeconds, Is.EqualTo(2f));
            var jump = new CharacterJumpState();
            Assert.That(jump.TryBegin(false), Is.False);
            Assert.That(jump.TryBegin(true), Is.True);
            Assert.That(jump.TryBegin(true), Is.False);
            Assert.That(jump.VerticalVelocity, Is.EqualTo(CharacterJumpState.TakeoffSpeed));
            Assert.That(jump.Step(.1f), Is.EqualTo(4.6f).Within(.0001f));
            Assert.That(jump.ObserveGrounded(true), Is.False, "Positive takeoff velocity cannot immediately count as a landing.");
            for (var step = 0; step < 14; step++) jump.Step(.1f);
            Assert.That(jump.VerticalVelocity, Is.EqualTo(CharacterJumpState.MaximumFallSpeed));
            Assert.That(jump.ObserveGrounded(true), Is.True);
            Assert.That(jump.IsActive, Is.False);
            Assert.That(jump.VerticalVelocity, Is.Zero);
            var timedOut = new CharacterJumpState();
            Assert.That(timedOut.TryBegin(true), Is.True);
            Assert.That(timedOut.Step(CharacterJumpState.MaximumAirborneSeconds), Is.Zero);
            Assert.That(timedOut.IsActive, Is.False, "An unresolved jump cannot retain vertical state indefinitely.");
            Assert.That(typeof(UnityEngine.Object).IsAssignableFrom(typeof(CharacterJumpState)), Is.False);
        }

        [TestCase(false, 1f, PossessionEnergyReadabilityLevel.Normal, "Possession energy  1.0/30s")]
        [TestCase(true, 5.1f, PossessionEnergyReadabilityLevel.Normal, "Possession energy  5.1/30s")]
        [TestCase(true, 5f, PossessionEnergyReadabilityLevel.Warning, "POSSESSION ENDING  5.0s")]
        [TestCase(true, 2.1f, PossessionEnergyReadabilityLevel.Warning, "POSSESSION ENDING  2.1s")]
        [TestCase(true, 2f, PossessionEnergyReadabilityLevel.Critical, "RETURN TO KEEPER  2.0s")]
        [TestCase(true, .1f, PossessionEnergyReadabilityLevel.Critical, "RETURN TO KEEPER  0.1s")]
        [TestCase(true, 0f, PossessionEnergyReadabilityLevel.Normal, "Possession energy  0.0/30s")]
        public void PossessionEnergyReadability_MapsPossessionBoundaries(bool possessing, float remaining, PossessionEnergyReadabilityLevel level, string copy)
        {
            var state = PossessionEnergyReadability.Map(possessing, remaining, 30);
            Assert.That(state.Level, Is.EqualTo(level));
            Assert.That(state.Copy, Is.EqualTo(copy));
        }

        [Test]
        public void PossessionEnergyReadability_ClampsMalformedValues()
        {
            var negative = PossessionEnergyReadability.Map(true, -4, 30);
            Assert.That(negative.DisplayedTenths, Is.Zero);
            Assert.That(negative.NormalizedRemaining, Is.Zero);
            Assert.That(negative.Level, Is.EqualTo(PossessionEnergyReadabilityLevel.Normal));

            var overMaximum = PossessionEnergyReadability.Map(true, 90, 30);
            Assert.That(overMaximum.DisplayedTenths, Is.EqualTo(300));
            Assert.That(overMaximum.NormalizedRemaining, Is.EqualTo(1));
            Assert.That(overMaximum.Copy, Is.EqualTo("Possession energy  30.0/30s"));

            var invalidMaximum = PossessionEnergyReadability.Map(true, 5, -30);
            Assert.That(invalidMaximum.DisplayedTenths, Is.Zero);
            Assert.That(invalidMaximum.Maximum, Is.Zero);
            Assert.That(invalidMaximum.NormalizedRemaining, Is.Zero);
        }

        [Test]
        public void PossessionEnergyReadability_UrgencyPulseIsBoundedDeterministicAndRejectsInvalidTime()
        {
            const float startedAt = 10;
            Assert.That(PossessionEnergyReadability.UrgencyPulseDuration, Is.EqualTo(.24f));
            Assert.That(PossessionEnergyReadability.UrgencyPulseMaximumScale, Is.EqualTo(1.06f));
            Assert.That(PossessionEnergyReadability.UrgencyPulseScaleAt(startedAt, startedAt), Is.EqualTo(1));
            Assert.That(PossessionEnergyReadability.UrgencyPulseScaleAt(startedAt, startedAt + .06f), Is.GreaterThan(1).And.LessThan(1.06f));
            Assert.That(PossessionEnergyReadability.UrgencyPulseScaleAt(startedAt, startedAt + .12f), Is.EqualTo(1.06f).Within(.0001f));
            Assert.That(PossessionEnergyReadability.UrgencyPulseScaleAt(startedAt, startedAt + .18f), Is.GreaterThan(1).And.LessThan(1.06f));
            Assert.That(PossessionEnergyReadability.UrgencyPulseScaleAt(startedAt, startedAt + .24f), Is.EqualTo(1));
            Assert.That(PossessionEnergyReadability.UrgencyPulseScaleAt(startedAt, startedAt - .01f), Is.EqualTo(1));
            Assert.That(PossessionEnergyReadability.UrgencyPulseScaleAt(float.NaN, startedAt), Is.EqualTo(1));
            Assert.That(PossessionEnergyReadability.UrgencyPulseScaleAt(startedAt, float.PositiveInfinity), Is.EqualTo(1));
        }

        [Test]
        public void PossessionEnergyReadability_IsCultureStableAndOwnsNoRuntimeAuthority()
        {
            var previousCulture = System.Globalization.CultureInfo.CurrentCulture;
            try
            {
                System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("lv-LV");
                Assert.That(PossessionEnergyReadability.Map(true, 1.5f, 30).Copy, Is.EqualTo("RETURN TO KEEPER  1.5s"));
            }
            finally { System.Globalization.CultureInfo.CurrentCulture = previousCulture; }

            Assert.That(typeof(UnityEngine.Object).IsAssignableFrom(typeof(PossessionEnergyReadability)), Is.False);
            Assert.That(typeof(PossessionEnergyReadability).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic), Is.Empty);
            foreach (var field in typeof(PossessionEnergyReadabilityState).GetFields(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic))
                Assert.That(field.IsInitOnly, Is.True, $"{field.Name} must remain immutable.");
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

        [Test]
        public void BloodKnightHeroPrefab_BindsAsNonBlockingVisualAndFallsBackSafely()
        {
            var heroRecipe = PrototypeRuntimeFactory.BloodKnightRecipe;
            Assert.That(heroRecipe.BaseBodyPrefab, Is.Not.Null, "The project-owned Resources hero prefab must be available.");
            var dependencies = UnityEditor.AssetDatabase.GetDependencies(UnityEditor.AssetDatabase.GetAssetPath(heroRecipe.BaseBodyPrefab));
            Assert.That(dependencies, Has.Some.EndsWith("3DRT/FantasyWarrior/source/warrior_animated-armed.fbx"), "Blood Knight must bind the authorised 3DRT visual source.");
            Assert.That(dependencies, Has.None.EndsWith("Quaternius/AnimatedKnight/KnightCharacter.fbx"), "The old visual remains a named fallback, not the active hero binding.");
            Assert.That(heroRecipe.BaseBodyPrefab.GetComponentInChildren<Animator>(true), Is.Null, "The rejected Take 001 sequence must not add an Animator to the Blood Knight visual.");
            var fallback = Resources.Load<GameObject>("Characters/BloodKnightHero_QuaterniusFallback");
            Assert.That(fallback, Is.Not.Null);
            var fallbackAnimator = fallback.GetComponentInChildren<Animator>(true);
            Assert.That(fallbackAnimator, Is.Not.Null, "The retained Quaternius prefab has its pre-existing Animator component.");
            Assert.That(fallbackAnimator.runtimeAnimatorController, Is.Null, "Fallback visuals must remain independent of the rejected 3DRT animation decision.");
            Assert.That(fallbackAnimator.applyRootMotion, Is.False, "Fallback Animator must not gain locomotion authority.");
            var host = GameObject.CreatePrimitive(PrimitiveType.Capsule); var assembler = host.AddComponent<CharacterVisualAssembler>();
            Assert.That(assembler.Assemble(heroRecipe), Is.True);
            var root = host.transform.Find("Character Visual Modules");
            Assert.That(root, Is.Not.Null); Assert.That(root.Find("Presentation Pivot/Base Body"), Is.Not.Null);
            foreach (var collider in root.GetComponentsInChildren<Collider>(true)) Assert.That(collider.enabled, Is.False);

            var unavailableRecipe = Object.Instantiate(heroRecipe); unavailableRecipe.BaseBodyPrefab = null;
            Assert.That(assembler.Assemble(unavailableRecipe), Is.True);
            Assert.That(host.transform.Find("Character Visual Modules/Presentation Pivot/Base Body"), Is.Not.Null);
            foreach (var collider in host.GetComponentsInChildren<Collider>(true)) if (collider.transform != host.transform) Assert.That(collider.enabled, Is.False);
            Object.DestroyImmediate(unavailableRecipe); Object.DestroyImmediate(host);
        }

        [Test]
        public void ThreeDrtBloodKnightSource_UsesMobileVisualOnlyImportSettings()
        {
            const string modelPath = "Assets/Game/Art/ThirdParty/3DRT/FantasyWarrior/source/warrior_animated-armed.fbx";
            const string texturePath = "Assets/Game/Art/ThirdParty/3DRT/FantasyWarrior/textures/warrior.jpg";
            var model = UnityEditor.AssetImporter.GetAtPath(modelPath) as UnityEditor.ModelImporter;
            var texture = UnityEditor.AssetImporter.GetAtPath(texturePath) as UnityEditor.TextureImporter;

            Assert.That(model, Is.Not.Null);
            Assert.That(model.meshCompression, Is.EqualTo(UnityEditor.ModelImporterMeshCompression.Medium));
            Assert.That(model.isReadable, Is.False);
            Assert.That(model.addCollider, Is.False);
            Assert.That(model.importCameras, Is.False);
            Assert.That(model.importLights, Is.False);
            Assert.That(model.importAnimation, Is.True);
            Assert.That(model.animationType, Is.EqualTo(UnityEditor.ModelImporterAnimationType.Generic));

            Assert.That(texture, Is.Not.Null);
            Assert.That(texture.isReadable, Is.False);
            var android = texture.GetPlatformTextureSettings("Android");
            Assert.That(android.overridden, Is.True);
            Assert.That(android.maxTextureSize, Is.LessThanOrEqualTo(1024));
        }

        [Test]
        public void VisualMotion_UsesBoundedPivotWithoutChangingHostRoot()
        {
            var recipe = ScriptableObject.CreateInstance<CharacterVisualRecipe>(); recipe.Family = CharacterVisualFamily.Humanoid; recipe.Primary = Color.red; recipe.Secondary = Color.black; recipe.AccentColor = Color.yellow;
            var host = GameObject.CreatePrimitive(PrimitiveType.Capsule); host.transform.position = new Vector3(3, 2, 1);
            var assembler = host.AddComponent<CharacterVisualAssembler>(); Assert.That(assembler.Assemble(recipe), Is.True);
            var motion = host.GetComponent<CharacterVisualMotion>(); var rootPosition = host.transform.position; var rootRotation = host.transform.rotation;
            motion.Sample(1f, .1f, new Vector3(3, 0, 2), CombatActionPhase.Windup);
            Assert.That(host.transform.position, Is.EqualTo(rootPosition)); Assert.That(host.transform.rotation, Is.EqualTo(rootRotation));
            Assert.That(Vector3.Distance(motion.PresentationPivot.localPosition, motion.BasePosition), Is.LessThan(.12f));
            Assert.That(Quaternion.Angle(motion.PresentationPivot.localRotation, motion.BaseRotation), Is.GreaterThan(.1f));
            Assert.That(motion.PresentationPivot.localScale.x, Is.EqualTo(motion.BaseScale.x).Within(.01f));
            motion.ClearTransientReaction();
            Assert.That(motion.PresentationPivot.localPosition, Is.EqualTo(motion.BasePosition)); Assert.That(motion.PresentationPivot.localRotation, Is.EqualTo(motion.BaseRotation)); Assert.That(motion.PresentationPivot.localScale, Is.EqualTo(motion.BaseScale));
            Object.DestroyImmediate(recipe); Object.DestroyImmediate(host);
        }

        [Test]
        public void VisualMotion_JumpTransitionsStayBoundedComposeAndResetExactly()
        {
            var recipe = ScriptableObject.CreateInstance<CharacterVisualRecipe>(); recipe.Family = CharacterVisualFamily.Humanoid; recipe.Primary = Color.red; recipe.Secondary = Color.black; recipe.AccentColor = Color.yellow;
            var host = GameObject.CreatePrimitive(PrimitiveType.Capsule); host.transform.position = new Vector3(4, 2, 1); host.transform.localScale = new Vector3(1.2f, .9f, 1.1f);
            try
            {
                var assembler = host.AddComponent<CharacterVisualAssembler>(); Assert.That(assembler.Assemble(recipe), Is.True);
                var motion = host.GetComponent<CharacterVisualMotion>(); var pivot = motion.PresentationPivot; var rootPosition = host.transform.position; var rootScale = host.transform.localScale;
                var jumpTimeline = host.GetComponent<CharacterJumpPresentationTimeline>();
                jumpTimeline.Configure(4f, 3f);
                motion.Sample(0f, 1f, Vector3.zero, CombatActionPhase.Idle, false, true, true);
                motion.Sample(0f, 1f, Vector3.zero, CombatActionPhase.Idle, true, false, true);
                motion.Sample(.30f, 1f, Vector3.zero, CombatActionPhase.Idle, true, false, true);
                var pushPosition = pivot.localPosition;
                var pushScale = pivot.localScale;
                motion.Sample(.40f, 1f, Vector3.zero, CombatActionPhase.Idle, true, false, true);
                Assert.That(Vector3.Distance(pivot.localPosition, pushPosition), Is.LessThan(.005f), "Takeoff(1) and Falling(0) retain the same bounded pivot lift.");
                Assert.That(Vector3.Distance(pivot.localScale, pushScale), Is.LessThan(.002f), "Takeoff(1) and Falling(0) retain the same bounded pivot stretch.");
                motion.Sample(.80f, 1f, Vector3.zero, CombatActionPhase.Idle, true, false, true);
                Assert.That(pivot.localScale.y, Is.LessThan(pushScale.y - .04f), "Falling(1) releases the takeoff pivot stretch.");
                motion.Bind(pivot);
                jumpTimeline.Configure(1f, 1f);

                motion.Sample(0, .01f, Vector3.zero, CombatActionPhase.Idle, false, true, true);
                motion.Sample(.01f, .01f, Vector3.zero, CombatActionPhase.Windup, true, false, true);
                motion.Sample(.06f, .01f, Vector3.zero, CombatActionPhase.Windup, true, false, true);
                Assert.That(pivot.localScale.y, Is.GreaterThan(motion.BaseScale.y), "One inactive-to-jumping transition produces a takeoff stretch.");
                Assert.That(Quaternion.Angle(pivot.localRotation, motion.BaseRotation), Is.GreaterThan(.1f), "Action and takeoff presentation compose on the pivot.");
                AssertVisualMotionBounds(motion);
                Assert.That(host.transform.position, Is.EqualTo(rootPosition)); Assert.That(host.transform.localScale, Is.EqualTo(rootScale));

                motion.Sample(.20f, .01f, Vector3.zero, CombatActionPhase.Idle, true, false, true);
                motion.Sample(.21f, .01f, Vector3.zero, CombatActionPhase.Recovery, false, true, true);
                motion.Sample(.28f, .01f, Vector3.zero, CombatActionPhase.Recovery, false, true, true);
                Assert.That(pivot.localScale.y, Is.LessThan(motion.BaseScale.y), "Only a factual observed grounded jump end produces the landing settle.");
                AssertVisualMotionBounds(motion);
                motion.Sample(.40f, .01f, Vector3.zero, CombatActionPhase.Idle, false, true, true);
                Assert.That(pivot.localScale.y, Is.EqualTo(motion.BaseScale.y).Within(.01f), "The landing pulse does not repeat during ordinary grounded motion.");

                motion.ShowHitReaction();
                motion.Sample(Time.time, .01f, Vector3.zero, CombatActionPhase.Idle, false, true, true);
                Assert.That(Quaternion.Angle(pivot.localRotation, motion.BaseRotation), Is.GreaterThan(.1f), "Hit reaction remains composable with jump presentation.");
                motion.ClearTransientReaction();
                Assert.That(pivot.localPosition, Is.EqualTo(motion.BasePosition)); Assert.That(pivot.localRotation, Is.EqualTo(motion.BaseRotation)); Assert.That(pivot.localScale, Is.EqualTo(motion.BaseScale));

                motion.Sample(.5f, .01f, Vector3.zero, CombatActionPhase.Idle, false, true, true);
                motion.Sample(.51f, .01f, Vector3.zero, CombatActionPhase.Idle, true, false, true);
                motion.Sample(.52f, .01f, Vector3.zero, CombatActionPhase.Idle, false, false, true);
                motion.Sample(.53f, .01f, Vector3.zero, CombatActionPhase.Idle, false, true, true);
                Assert.That(pivot.localScale.y, Is.GreaterThan(motion.BaseScale.y * .97f), "An airborne cancellation followed by ordinary grounding cannot create a delayed landing settle.");

                var replacement = new GameObject("Replacement Presentation Pivot").transform; replacement.SetParent(host.transform, false);
                motion.Bind(replacement);
                Assert.That(pivot.localPosition, Is.EqualTo(motion.BasePosition), "Rebinding restores the old pivot before replacing it.");
                Assert.That(replacement.localPosition, Is.EqualTo(Vector3.zero)); Assert.That(replacement.localRotation, Is.EqualTo(Quaternion.identity)); Assert.That(replacement.localScale, Is.EqualTo(Vector3.one));
                motion.Sample(.6f, .01f, Vector3.zero, CombatActionPhase.Idle, false, true, true);
                Assert.That(replacement.localScale.y, Is.GreaterThan(motion.BaseScale.y * .97f), "A new binding cannot receive a delayed landing settle.");
                AssertVisualMotionBounds(motion);
            }
            finally { Object.DestroyImmediate(recipe); Object.DestroyImmediate(host); }
        }

        [Test]
        public void VisualMotion_PossessionArrivalIsSingleBoundedUnscaledAndCleansUpWithoutSyntheticEntry()
        {
            var recipe = ScriptableObject.CreateInstance<CharacterVisualRecipe>(); recipe.Family = CharacterVisualFamily.Humanoid; recipe.Primary = Color.red; recipe.Secondary = Color.black; recipe.AccentColor = Color.yellow;
            var host = GameObject.CreatePrimitive(PrimitiveType.Capsule); host.transform.position = new Vector3(2, 1, -3);
            try
            {
                var assembler = host.AddComponent<CharacterVisualAssembler>(); Assert.That(assembler.Assemble(recipe), Is.True);
                var motion = host.GetComponent<CharacterVisualMotion>(); var pivot = motion.PresentationPivot;
                var rootPosition = host.transform.position; var rootRotation = host.transform.rotation; var rootScale = host.transform.localScale;

                motion.Sample(2f, 2f, .016f, Vector3.zero, CombatActionPhase.Idle);
                Assert.That(motion.IsPossessionArrivalActive, Is.False, "Ordinary direct presentation sampling cannot synthesize a possession arrival.");
                Assert.That(motion.StartPossessionArrival(), Is.True);
                var firstEnd = motion.PossessionArrivalEndsAt;
                Assert.That(firstEnd - Time.unscaledTime, Is.LessThanOrEqualTo(.24f));
                Assert.That(motion.StartPossessionArrival(), Is.False, "One factual possession cannot restart its own arrival accent.");
                Assert.That(motion.PossessionArrivalEndsAt, Is.EqualTo(firstEnd));

                motion.Sample(2f, Time.unscaledTime + .05f, .016f, Vector3.zero, CombatActionPhase.Windup);
                Assert.That(pivot.localScale.y, Is.LessThan(motion.BaseScale.y), "The opening accent is a readable squash.");
                AssertVisualMotionBounds(motion);
                Assert.That(host.transform.position, Is.EqualTo(rootPosition)); Assert.That(host.transform.rotation, Is.EqualTo(rootRotation)); Assert.That(host.transform.localScale, Is.EqualTo(rootScale));

                motion.Sample(2f, firstEnd, .016f, Vector3.zero, CombatActionPhase.Windup);
                var settledPosition = pivot.localPosition; var settledRotation = pivot.localRotation; var settledScale = pivot.localScale;
                Assert.That(motion.IsPossessionArrivalActive, Is.False, "The exact unscaled end boundary removes the response.");
                Assert.That(motion.StartPossessionArrival(), Is.True, "A later factual possession may begin one new accent.");
                var secondEnd = motion.PossessionArrivalEndsAt;
                motion.Sample(2f, Time.unscaledTime + .05f, .016f, Vector3.zero, CombatActionPhase.Windup);
                motion.Sample(2f, secondEnd, .016f, Vector3.zero, CombatActionPhase.Windup);
                Assert.That(pivot.localPosition, Is.EqualTo(settledPosition));
                Assert.That(pivot.localRotation, Is.EqualTo(settledRotation));
                Assert.That(pivot.localScale, Is.EqualTo(settledScale), "Completion restores the ordinary composed pose exactly.");

                Assert.That(motion.StartPossessionArrival(), Is.True);
                motion.Sample(2f, Time.unscaledTime + .05f, .016f, Vector3.zero, CombatActionPhase.Windup);
                motion.ClearPossessionArrival();
                Assert.That(motion.IsPossessionArrivalActive, Is.False);
                Assert.That(pivot.localPosition, Is.EqualTo(settledPosition)); Assert.That(pivot.localRotation, Is.EqualTo(settledRotation)); Assert.That(pivot.localScale, Is.EqualTo(settledScale));

                var replacement = new GameObject("Replacement Possession Pivot").transform; replacement.SetParent(host.transform, false);
                Assert.That(motion.StartPossessionArrival(), Is.True);
                motion.Bind(replacement);
                Assert.That(motion.IsPossessionArrivalActive, Is.False, "Visual rebinding clears an unfinished arrival response.");
                Assert.That(host.transform.position, Is.EqualTo(rootPosition)); Assert.That(host.transform.rotation, Is.EqualTo(rootRotation)); Assert.That(host.transform.localScale, Is.EqualTo(rootScale));
            }
            finally { Object.DestroyImmediate(recipe); Object.DestroyImmediate(host); }
        }

        [Test]
        public void GuardianEntGrowth_BuildsRanksUnderThePresentationPivotAndCleansSafely()
        {
            var recipe = ScriptableObject.CreateInstance<CharacterVisualRecipe>(); recipe.Family = CharacterVisualFamily.LargeCreature; recipe.Primary = new Color(.18f, .43f, .14f); recipe.AccentColor = new Color(.4f, .8f, .3f);
            var host = new GameObject("Guardian Ent Visual Test", typeof(Health)); host.GetComponent<Health>().Initialize(10); var assembler = host.AddComponent<CharacterVisualAssembler>(); Assert.That(assembler.Assemble(recipe), Is.True);
            var growth = host.AddComponent<GuardianEntGrowthPresentation>(); var rootPosition = host.transform.position; var rootRotation = host.transform.rotation; var rootScale = host.transform.localScale;
            try
            {
                for (var rank = 0; rank <= 3; rank++)
                {
                    growth.Configure(rank);
                    Assert.That(growth.Rank, Is.EqualTo(rank));
                    Assert.That(growth.TierCount, Is.EqualTo(rank));
                    if (rank == 0) Assert.That(growth.MarkerRoot, Is.Null);
                    else
                    {
                        Assert.That(growth.MarkerRoot.parent, Is.EqualTo(assembler.PresentationPivot));
                        for (var tier = 0; tier < rank; tier++) Assert.That(growth.MarkerRoot.GetChild(tier).name, Is.EqualTo($"Cultivation Tier {tier + 1}"));
                        foreach (var collider in growth.MarkerRoot.GetComponentsInChildren<Collider>(true)) Assert.That(collider.enabled, Is.False);
                        Assert.That(growth.MarkerRoot.GetComponentInChildren<Canvas>(true), Is.Null);
                        Assert.That(growth.MarkerRoot.GetComponentInChildren<Rigidbody>(true), Is.Null);
                    }
                    Assert.That(host.transform.position, Is.EqualTo(rootPosition)); Assert.That(host.transform.rotation, Is.EqualTo(rootRotation)); Assert.That(host.transform.localScale, Is.EqualTo(rootScale));
                }

                Object.DestroyImmediate(growth);
                Assert.That(assembler.PresentationPivot.Find("Guardian Ent Cultivation") == null, Is.True);
                growth = host.AddComponent<GuardianEntGrowthPresentation>(); growth.Configure(3);
                host.GetComponent<Health>().TakeDamage(new DamageInfo(100, null, host.transform.position), 0);
                Assert.That(growth.MarkerRoot, Is.Null); Assert.That(growth.Rank, Is.EqualTo(3));
                growth.Clear();
                Assert.That(growth.MarkerRoot, Is.Null); Assert.That(growth.TierCount, Is.Zero); Assert.That(growth.Rank, Is.Zero);
                var missingVisualHost = new GameObject("Missing Visual Guardian Ent");
                try { var missing = missingVisualHost.AddComponent<GuardianEntGrowthPresentation>(); Assert.DoesNotThrow(() => missing.Configure(2)); Assert.That(missing.MarkerRoot, Is.Null); }
                finally { Object.DestroyImmediate(missingVisualHost); }
            }
            finally { Object.DestroyImmediate(recipe); Object.DestroyImmediate(host); }
        }

        [TestCase(0, "GUARDIAN ENT — UNTENDED")]
        [TestCase(1, "GUARDIAN ENT — RANK 1/3 • +10% MAX HEALTH")]
        [TestCase(2, "GUARDIAN ENT — RANK 2/3 • +20% MAX HEALTH")]
        [TestCase(3, "GUARDIAN ENT — RANK 3/3 • +30% MAX HEALTH")]
        public void GuardianEntVitalityHudCopy_MapsEachRankTruthfully(int rank, string expected)
        {
            Assert.That(DefenderHUD.GuardianEntVitalityCopy(rank), Is.EqualTo(expected));
            if (rank == 0) Assert.That(DefenderHUD.GuardianEntVitalityCopy(rank), Does.Not.Contain("%"));
        }

        [Test]
        public void HudPresentation_LoadsTheButtonSpriteAndKeepsItsHudRootListenerFree()
        {
            var hud = new GameObject("HUD Presentation Test");
            var imageObject = new GameObject("Button Background", typeof(RectTransform), typeof(Image));
            var image = imageObject.GetComponent<Image>(); image.color = new Color(.12f, .3f, .18f, .96f);
            var presentation = hud.AddComponent<HudPresentation>();
            presentation.ApplyButton(image);

            Assert.That(image.sprite, Is.Not.Null);
            Assert.That(image.type, Is.EqualTo(Image.Type.Sliced));
            Assert.That(image.color, Is.EqualTo(new Color(.12f, .3f, .18f, .96f)));
            Assert.That(hud.GetComponents<AudioSource>(), Has.Length.EqualTo(1));
            Assert.That(hud.GetComponent<AudioListener>(), Is.Null);

            Object.DestroyImmediate(imageObject); Object.DestroyImmediate(hud);
        }

        static string[] ModuleNames(Transform entity)
        {
            var root = entity.Find("Character Visual Modules/Presentation Pivot"); var names = new string[root.childCount];
            for (int i = 0; i < root.childCount; i++) names[i] = root.GetChild(i).name;
            return names;
        }

        static void AssertVisualMotionBounds(CharacterVisualMotion motion)
        {
            var offset = motion.PresentationPivot.localPosition - motion.BasePosition;
            Assert.That(offset.magnitude, Is.LessThanOrEqualTo(.080001f));
            Assert.That(motion.PresentationPivot.localScale.x / motion.BaseScale.x, Is.InRange(.90f, 1.10f));
            Assert.That(motion.PresentationPivot.localScale.y / motion.BaseScale.y, Is.InRange(.90f, 1.10f));
            Assert.That(motion.PresentationPivot.localScale.z / motion.BaseScale.z, Is.InRange(.90f, 1.10f));
        }
    }
}
