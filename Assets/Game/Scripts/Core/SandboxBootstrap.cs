using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Possession;
using RealmRaiders.UI;
using UnityEngine;
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
            var rig = PrototypeRuntimeFactory.Camera(new Color(.035f, .055f, .075f), 48, new Vector3(0, 22, -11), Quaternion.Euler(60, 0, 0));

            PrototypeRuntimeFactory.DirectionalLight("Sun", new Color(1, .86f, .7f), 1.4f, new Vector3(48, -35, 0));
            RenderSettings.ambientLight = new Color(.23f, .27f, .32f);

            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane); ground.name = "Arena Ground"; ground.transform.localScale = new Vector3(2.2f, 1, 2.8f); ground.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.12f, .18f, .16f));
            for (int i = 0; i < 10; i++)
            {
                float angle = i * Mathf.PI * 2 / 10; var stone = GameObject.CreatePrimitive(PrimitiveType.Cube); stone.name = "Boundary Stone";
                stone.transform.position = new Vector3(Mathf.Sin(angle) * 10, .6f, Mathf.Cos(angle) * 13); stone.transform.localScale = new Vector3(1.2f, 1.2f + i % 3, 1.2f); stone.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.16f, .2f, .19f));
            }

            var hero = CreateEntity("Blood Knight", new Vector3(-4, 1, 0), CombatStats.BloodKnight, false, new Color(.62f, .06f, .08f), false, PrototypeRuntimeFactory.BloodKnightRecipe);
            var ent = CreateEntity("Ent", new Vector3(4, 1.5f, 0), CombatStats.Ent, true, new Color(.2f, .42f, .16f), true, PrototypeRuntimeFactory.GuardianEntRecipe);
            hero.GetComponent<CreatureBrain>().Target = ent; ent.GetComponent<CreatureBrain>().Target = hero;

            var possession = root.AddComponent<PossessionManager>(); possession.Initialize(rig); possession.Register(ent);
            var director = root.AddComponent<SandboxDirector>(); director.Initialize(hero, ent, possession, rig);
            var hud = new GameObject("Prototype HUD", typeof(PrototypeHUD)); hud.transform.SetParent(root.transform); hud.GetComponent<PrototypeHUD>().Initialize(possession, director, hero, ent);
            PrototypeRuntimeFactory.EventSystem(root.transform);
        }

        static CombatEntity CreateEntity(string displayName, Vector3 position, CombatStats stats, bool possessable, Color color, bool heavy, CharacterVisualRecipe recipe)
        {
            var visualScale = heavy ? new Vector3(1.5f, 1.8f, 1.5f) : Vector3.one;
            var abilities = heavy ? new[] { PrototypeRuntimeFactory.Ability("Smash", AbilityKind.Melee, 36, 2.7f, 1.2f, .45f), PrototypeRuntimeFactory.Ability("Charge", AbilityKind.Dash, 26, 1.8f, 3.5f, .2f, 1, 6), PrototypeRuntimeFactory.Ability("Ground Slam", AbilityKind.Area, 42, 1, 4, .8f) } : new[] { PrototypeRuntimeFactory.Ability("Basic Slash", AbilityKind.Melee, 23, 2.3f, .9f, .18f), PrototypeRuntimeFactory.Ability("Blood Rush", AbilityKind.Dash, 25, 1.8f, 3, .15f, 1, 6), PrototypeRuntimeFactory.Ability("Heavy Cleave", AbilityKind.Area, 35, 1.8f, 2.8f, .65f) };
            return PrototypeRuntimeFactory.CreateEntity(displayName, position, stats, color, possessable, visualScale, abilities, heavy, recipe);
        }

    }
}
