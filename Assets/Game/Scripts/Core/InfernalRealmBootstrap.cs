using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Possession;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using RealmRaiders.Traps;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RealmRaiders.Core
{
    public static class InfernalRealmBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register() { SceneManager.sceneLoaded -= OnSceneLoaded; SceneManager.sceneLoaded += OnSceneLoaded; }
        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        { if (scene.name != "InfernalRealm" || Object.FindFirstObjectByType<DefenseManager>()) return; Application.targetFrameRate = 60; QualitySettings.vSyncCount = 0; Build(); }

        static void Build()
        {
            var root = new GameObject("Infernal Realm Defense");
            var rig = PrototypeRuntimeFactory.Camera(new Color(.08f, .012f, .008f), 48, new Vector3(0, 46, -33), Quaternion.Euler(57, 0, 0));
            PrototypeRuntimeFactory.DirectionalLight("Lava Glow", new Color(1, .25f, .08f), 1.4f, new Vector3(50, -30, 0)); RenderSettings.ambientLight = new Color(.22f, .06f, .03f);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name = "Volcanic Floor"; ground.transform.position = new Vector3(0, -.25f, 0); ground.transform.localScale = new Vector3(14, .5f, 68); ground.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.16f, .035f, .02f));
            for (int i = 0; i < 18; i++) { var rock = GameObject.CreatePrimitive(PrimitiveType.Cube); rock.name = "Volcanic Rock"; rock.transform.position = new Vector3(i % 2 == 0 ? -8 : 8, 1.5f, -30 + i * 3.5f); rock.transform.localScale = new Vector3(1.1f, 2 + i % 3, 1.4f); rock.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.09f, .025f, .02f)); }

            var invaderStats = CombatStats.BloodKnight; invaderStats.MaxHealth = 240; invaderStats.MoveSpeed = 3.8f;
            var invader = PrototypeRuntimeFactory.CreateEntity("Invading Blood Knight", new Vector3(0, 1, -30), invaderStats, new Color(.7f, .04f, .03f), false, Vector3.one * .95f, new[] { PrototypeRuntimeFactory.Ability("Basic Slash", AbilityKind.Melee, 22, 2.3f, .9f, .18f, .9f), PrototypeRuntimeFactory.Ability("Blood Rush", AbilityKind.Dash, 25, 1.8f, 3, .15f, .9f, 6), PrototypeRuntimeFactory.Ability("Heavy Cleave", AbilityKind.Area, 35, 1.8f, 2.8f, .65f) }, false, PrototypeRuntimeFactory.BloodKnightRecipe);
            var bruteStats = CombatStats.Ent; bruteStats.MaxHealth = 380; bruteStats.AttackDamage = 38; bruteStats.MoveSpeed = 2.8f;
            var brute = PrototypeRuntimeFactory.CreateEntity("Infernal Brute", new Vector3(0, 1.5f, 10), bruteStats, new Color(.3f, .08f, .045f), true, Vector3.one * 1.45f, new[] { PrototypeRuntimeFactory.Ability("Smash", AbilityKind.Melee, 40, 2.7f, 1.3f, .5f, .9f), PrototypeRuntimeFactory.Ability("Charge", AbilityKind.Dash, 28, 1.8f, 3, .22f, .9f, 5), PrototypeRuntimeFactory.Ability("Ground Slam", AbilityKind.Area, 46, 1, 4, .8f) }, true, PrototypeRuntimeFactory.InfernalBruteRecipe);
            var houndStats = new CombatStats { MaxHealth = 60, AttackDamage = 12, AttackSpeed = 1.5f, MoveSpeed = 7.2f, Armor = 2, AbilityPower = 5 };
            var houndOne = PrototypeRuntimeFactory.CreateEntity("Hellhound A", new Vector3(-3.5f, .7f, -2), houndStats, new Color(.38f, .07f, .025f), false, Vector3.one * .7f, new[] { PrototypeRuntimeFactory.Ability("Leap", AbilityKind.Melee, 12, 2.4f, 1, .14f, .9f) }, false, PrototypeRuntimeFactory.InfernalBeastRecipe); var houndTwo = PrototypeRuntimeFactory.CreateEntity("Hellhound B", new Vector3(3.5f, .7f, 3), houndStats, new Color(.48f, .1f, .025f), false, Vector3.one * .7f, new[] { PrototypeRuntimeFactory.Ability("Leap", AbilityKind.Melee, 12, 2.4f, 1, .14f, .9f) }, false, PrototypeRuntimeFactory.InfernalBeastRecipe);
            foreach (var defender in new[] { brute, houndOne, houndTwo }) { var brain = defender.Controller<CreatureBrain>(); brain.Target = invader; defender.SetController(brain); }
            var invaderBrain = invader.gameObject.AddComponent<RaidInvaderBrain>(); invader.RefreshControllers(); invaderBrain.Configure(new[] { new Vector3(0, 1, -20), new Vector3(0, 1, -7), new Vector3(0, 1, 5), new Vector3(0, 1, 18), new Vector3(0, 1, 29) }, new[] { brute, houndOne, houndTwo }); invader.SetController(invaderBrain);

            var flameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder); flameObject.name = "Flame Trap"; flameObject.transform.position = new Vector3(0, .1f, -7); flameObject.transform.localScale = new Vector3(2.7f, .1f, 2.7f); flameObject.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.95f, .2f, .02f)); var flame = flameObject.AddComponent<FlameTrap>(); flame.Initialize(invader);
            var gateObject = GameObject.CreatePrimitive(PrimitiveType.Cube); gateObject.name = "Lava Gate"; gateObject.transform.position = new Vector3(0, 1.8f, 5); gateObject.transform.localScale = new Vector3(10, 3.6f, .7f); gateObject.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.72f, .12f, .015f)); var gate = gateObject.AddComponent<LavaGate>(); gate.Initialize(invader); gate.Automatic = true; gate.TriggerRadius = 4.5f; gateObject.GetComponent<Collider>().enabled = false;
            var heart = InfernalHeart(new Vector3(0, 2.5f, 30)); var core = heart.GetComponent<RealmCore>(); core.Initialize(invader);
            var possession = root.AddComponent<PossessionManager>(); var energy = new PossessionEnergy(30); possession.Initialize(rig); possession.ConfigureEnergy(energy); possession.Register(brute);
            var defense = root.AddComponent<DefenseManager>(); defense.Initialize(invader, core, possession);
            var hudObject = new GameObject("Infernal HUD", typeof(DefenderHUD)); hudObject.transform.SetParent(root.transform); hudObject.GetComponent<DefenderHUD>().Initialize(defense, possession, energy, invader, brute, flame, core, DefenseHudConfig.Infernal);
            PrototypeRuntimeFactory.EventSystem(root.transform);
        }
        static GameObject InfernalHeart(Vector3 position) { var root = new GameObject("Infernal Heart"); root.transform.position = position; var rock = GameObject.CreatePrimitive(PrimitiveType.Sphere); rock.transform.SetParent(root.transform); rock.transform.localScale = new Vector3(3.5f, 4.5f, 3.5f); rock.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.85f, .08f, .015f)); root.AddComponent<RealmCore>(); return root; }
    }
}
