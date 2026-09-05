using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;

namespace RealmRaiders.Core
{
    /// <summary>Small construction helpers shared by the runtime-authored prototype scenes.</summary>
    public static class PrototypeRuntimeFactory
    {
        static CharacterVisualRecipe bloodKnightRecipe, guardianEntRecipe, infernalBruteRecipe, sylvanBeastRecipe, infernalBeastRecipe;
        static GameObject bloodKnightHeroPrefab;
        static CharacterVisualRecipe Recipe(CharacterVisualFamily family, VisualModuleStyle head, VisualModuleStyle back, VisualModuleStyle arms, VisualModuleStyle accent, Color primary, Color secondary, Color accentColor)
        {
            var recipe = ScriptableObject.CreateInstance<CharacterVisualRecipe>(); recipe.Family = family; recipe.Head = head; recipe.Back = back; recipe.Arms = arms; recipe.Accent = accent; recipe.Primary = primary; recipe.Secondary = secondary; recipe.AccentColor = accentColor; return recipe;
        }
        // Explicit roster recipe data; callers choose these directly, never from character names.
        public static CharacterVisualRecipe BloodKnightRecipe
        {
            get
            {
                if (bloodKnightRecipe) return bloodKnightRecipe;
                bloodKnightRecipe = Recipe(CharacterVisualFamily.Humanoid, VisualModuleStyle.None, VisualModuleStyle.None, VisualModuleStyle.None, VisualModuleStyle.None, new Color(.55f, .04f, .06f), new Color(.16f, .12f, .14f), new Color(.95f, .65f, .2f));
                bloodKnightHeroPrefab = Resources.Load<GameObject>("Characters/BloodKnightHero");
                bloodKnightRecipe.BaseBodyPrefab = bloodKnightHeroPrefab;
                return bloodKnightRecipe;
            }
        }
        public static CharacterVisualRecipe GuardianEntRecipe => guardianEntRecipe ? guardianEntRecipe : guardianEntRecipe = Recipe(CharacterVisualFamily.LargeCreature, VisualModuleStyle.Bark, VisualModuleStyle.Bark, VisualModuleStyle.Claws, VisualModuleStyle.Mane, new Color(.16f, .38f, .11f), new Color(.25f, .16f, .07f), new Color(.65f, .95f, .28f));
        public static CharacterVisualRecipe InfernalBruteRecipe => infernalBruteRecipe ? infernalBruteRecipe : infernalBruteRecipe = Recipe(CharacterVisualFamily.LargeCreature, VisualModuleStyle.Horns, VisualModuleStyle.Spikes, VisualModuleStyle.Claws, VisualModuleStyle.Spikes, new Color(.28f, .055f, .03f), new Color(.12f, .025f, .02f), new Color(1f, .28f, .05f));
        public static CharacterVisualRecipe SylvanBeastRecipe => sylvanBeastRecipe ? sylvanBeastRecipe : sylvanBeastRecipe = Recipe(CharacterVisualFamily.Beast, VisualModuleStyle.Mane, VisualModuleStyle.None, VisualModuleStyle.Claws, VisualModuleStyle.None, new Color(.34f, .4f, .32f), new Color(.15f, .2f, .14f), new Color(.75f, .9f, .5f));
        public static CharacterVisualRecipe InfernalBeastRecipe => infernalBeastRecipe ? infernalBeastRecipe : infernalBeastRecipe = Recipe(CharacterVisualFamily.Beast, VisualModuleStyle.Horns, VisualModuleStyle.Spikes, VisualModuleStyle.Claws, VisualModuleStyle.None, new Color(.35f, .06f, .025f), new Color(.12f, .02f, .01f), new Color(1f, .25f, .04f));
        public static Material Material(Color color)
        { var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); material.color = color; return material; }

        public static AbilityDefinition Ability(string name, AbilityKind kind, float damage, float range, float radius, float windup, float cooldown = -1, float dash = 0)
        { var ability = ScriptableObject.CreateInstance<AbilityDefinition>(); ability.DisplayName = name; ability.Kind = kind; ability.Damage = damage; ability.Range = range; ability.Radius = radius; ability.Windup = windup; ability.Cooldown = cooldown >= 0 ? cooldown : kind == AbilityKind.Area ? 4 : 1; ability.DashDistance = dash; return ability; }

        public static CombatEntity CreateEntity(string name, Vector3 position, CombatStats stats, Color color, bool possessable, Vector3 visualScale, AbilityDefinition[] abilities, bool cube, CharacterVisualRecipe visualRecipe = null)
        {
            var go = GameObject.CreatePrimitive(cube ? PrimitiveType.Cube : PrimitiveType.Capsule); go.name = name; go.transform.position = position; Object.Destroy(go.GetComponent<Collider>());
            var motor = go.AddComponent<CharacterController>(); motor.height = cube ? 3 : 2; motor.radius = cube ? .8f : .5f;
            go.AddComponent<Health>(); var entity = go.AddComponent<CombatEntity>(); go.AddComponent<PlayerController>(); var ai = go.AddComponent<CreatureBrain>(); go.GetComponent<Renderer>().material = Material(color); go.transform.localScale = visualScale;
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>(); definition.DisplayName = name; definition.Stats = stats; definition.Possessable = possessable; definition.PlaceholderColor = color; definition.Abilities = abilities; definition.VisualRecipe = visualRecipe;
            entity.Initialize(definition); entity.SetController(ai); return entity;
        }

        public static PrototypeCameraRig Camera(Color background, float fieldOfView, Vector3 overviewPosition, Quaternion overviewRotation)
        {
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig)); cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = background; camera.fieldOfView = fieldOfView;
            var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.ConfigureOverview(overviewPosition, overviewRotation); rig.SnapToOverview(); return rig;
        }

        public static Light DirectionalLight(string name, Color color, float intensity, Vector3 eulerAngles)
        { var lightObject = new GameObject(name, typeof(Light)); var light = lightObject.GetComponent<Light>(); light.type = LightType.Directional; light.intensity = intensity; light.color = color; lightObject.transform.rotation = Quaternion.Euler(eulerAngles); return light; }

        public static EventSystem EventSystem(Transform parent)
        { var eventObject = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)); eventObject.transform.SetParent(parent); return eventObject.GetComponent<EventSystem>(); }
    }
}
