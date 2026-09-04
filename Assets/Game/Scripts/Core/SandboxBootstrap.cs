using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Possession;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace RealmRaiders.Core
{
    public static class SandboxBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "CharacterSandbox") return;
            if (Object.FindFirstObjectByType<SandboxDirector>()) return;
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
            CreateWorld();
        }

        static void CreateWorld()
        {
            var root = new GameObject("Realm Raiders Sandbox");
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig));
            cameraObject.tag = "MainCamera";
            var camera = cameraObject.GetComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.035f, .055f, .075f); camera.fieldOfView = 48;
            var rig = cameraObject.GetComponent<PrototypeCameraRig>(); rig.SnapToOverview();

            var lightObject = new GameObject("Sun", typeof(Light));
            var light = lightObject.GetComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.4f; light.color = new Color(1, .86f, .7f); lightObject.transform.rotation = Quaternion.Euler(48, -35, 0);
            RenderSettings.ambientLight = new Color(.23f, .27f, .32f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane); ground.name = "Arena Ground"; ground.transform.localScale = new Vector3(2.2f, 1, 2.8f); ground.GetComponent<Renderer>().material = Material(new Color(.12f, .18f, .16f));
            for (int i = 0; i < 10; i++)
            {
                float angle = i * Mathf.PI * 2 / 10; var stone = GameObject.CreatePrimitive(PrimitiveType.Cube); stone.name = "Boundary Stone";
                stone.transform.position = new Vector3(Mathf.Sin(angle) * 10, .6f, Mathf.Cos(angle) * 13); stone.transform.localScale = new Vector3(1.2f, 1.2f + i % 3, 1.2f); stone.GetComponent<Renderer>().material = Material(new Color(.16f, .2f, .19f));
            }

            var hero = CreateEntity("Blood Knight", new Vector3(-4, 1, 0), CombatStats.BloodKnight, false, new Color(.62f, .06f, .08f), false);
            var ent = CreateEntity("Ent", new Vector3(4, 1.5f, 0), CombatStats.Ent, true, new Color(.2f, .42f, .16f), true);
            hero.GetComponent<CreatureBrain>().Target = ent; ent.GetComponent<CreatureBrain>().Target = hero;

            var possession = root.AddComponent<PossessionManager>(); possession.Initialize(rig); possession.Register(ent);
            var director = root.AddComponent<SandboxDirector>(); director.Initialize(hero, ent, possession, rig);
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)); eventSystem.transform.SetParent(root.transform);
            var hud = new GameObject("Prototype HUD", typeof(PrototypeHUD)); hud.transform.SetParent(root.transform); hud.GetComponent<PrototypeHUD>().Initialize(possession, director, hero, ent);
        }

        static CombatEntity CreateEntity(string displayName, Vector3 position, CombatStats stats, bool possessable, Color color, bool heavy)
        {
            var go = GameObject.CreatePrimitive(heavy ? PrimitiveType.Cube : PrimitiveType.Capsule); go.name = displayName; go.transform.position = position;
            Object.Destroy(go.GetComponent<Collider>());
            var motor = go.AddComponent<CharacterController>(); motor.height = heavy ? 3 : 2; motor.radius = heavy ? .8f : .5f; motor.center = new Vector3(0, heavy ? 0 : 0, 0);
            go.AddComponent<Health>();
            var entity = go.AddComponent<CombatEntity>();
            go.AddComponent<PlayerController>();
            var ai = go.AddComponent<CreatureBrain>();
            go.GetComponent<Renderer>().material = Material(color);
            if (heavy) go.transform.localScale = new Vector3(1.5f, 1.8f, 1.5f);
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>(); definition.DisplayName = displayName; definition.Stats = stats; definition.Possessable = possessable; definition.PlaceholderColor = color;
            definition.Abilities = heavy
                ? new[] { Ability("Smash", AbilityKind.Melee, 36, 2.7f, 1.2f, .45f), Ability("Charge", AbilityKind.Dash, 26, 1.8f, 3.5f, .2f, 6), Ability("Ground Slam", AbilityKind.Area, 42, 1, 4, .8f) }
                : new[] { Ability("Basic Slash", AbilityKind.Melee, 23, 2.3f, .9f, .18f), Ability("Blood Rush", AbilityKind.Dash, 25, 1.8f, 3, .15f, 6), Ability("Heavy Cleave", AbilityKind.Area, 35, 1.8f, 2.8f, .65f) };
            entity.Initialize(definition); entity.SetController(ai);
            return entity;
        }

        static AbilityDefinition Ability(string name, AbilityKind kind, float damage, float range, float radius, float windup, float dash = 0)
        { var ability = ScriptableObject.CreateInstance<AbilityDefinition>(); ability.DisplayName = name; ability.Kind = kind; ability.Damage = damage; ability.Range = range; ability.Radius = radius; ability.Windup = windup; ability.Cooldown = kind == AbilityKind.Area ? 4 : 1; ability.DashDistance = dash; return ability; }
        static Material Material(Color color) { var material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); material.color = color; return material; }
    }
}
