using NUnit.Framework;
using RealmRaiders.Combat;
using RealmRaiders.Characters;
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

        [Test]
        public void BloodKnightHeroPrefab_BindsAsNonBlockingVisualAndFallsBackSafely()
        {
            var heroRecipe = PrototypeRuntimeFactory.BloodKnightRecipe;
            Assert.That(heroRecipe.BaseBodyPrefab, Is.Not.Null, "The project-owned Resources hero prefab must be available.");
            var dependencies = UnityEditor.AssetDatabase.GetDependencies(UnityEditor.AssetDatabase.GetAssetPath(heroRecipe.BaseBodyPrefab));
            Assert.That(dependencies, Has.Some.EndsWith("3DRT/FantasyWarrior/source/warrior_animated-armed.fbx"), "Blood Knight must bind the authorised 3DRT visual source.");
            Assert.That(dependencies, Has.None.EndsWith("Quaternius/AnimatedKnight/KnightCharacter.fbx"), "The old visual remains a named fallback, not the active hero binding.");
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
    }
}
