using System.Collections.Generic;
using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using RealmRaiders.Traps;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RealmRaiders.Core
{
    public static class SylvanRealmBootstrap
    {
        static readonly Color Forest = new(.08f, .22f, .1f);
        static readonly Color Moss = new(.14f, .4f, .16f);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "SylvanRealm" || Object.FindFirstObjectByType<RaidManager>()) return;
            Application.targetFrameRate = 60; QualitySettings.vSyncCount = 0; BuildRealm();
        }

        static void BuildRealm()
        {
            var root = new GameObject("Sylvan Realm Raid");
            var cameraRig = PrototypeRuntimeFactory.Camera(new Color(.018f, .055f, .035f), 52, new Vector3(0, 22, -11), Quaternion.Euler(60, 0, 0));
            PrototypeRuntimeFactory.DirectionalLight("Forest Moon", new Color(.68f, .86f, .76f), 1.25f, new Vector3(52, -28, 0));
            RenderSettings.ambientLight = new Color(.14f, .21f, .17f);

            var hero = Entity("Blood Knight", new Vector3(0, 1, -50), CombatStats.BloodKnight, new Color(.7f, .055f, .07f), false, PrototypeRuntimeFactory.BloodKnightRecipe);
            hero.SetController(hero.Controller<PlayerController>()); cameraRig.SnapTo(hero, CameraMode.HeroCombat);

            var wolfStats = new CombatStats { MaxHealth = 58, AttackDamage = 11, AttackSpeed = 1.5f, MoveSpeed = 7, Armor = 2, AbilityPower = 5 };
            var wolfOne = Entity("Wolf Alpha", new Vector3(-13, .65f, -11), wolfStats, new Color(.36f, .39f, .35f), false, PrototypeRuntimeFactory.SylvanBeastRecipe, .75f);
            var wolfTwo = Entity("Wolf Scout", new Vector3(-16, .65f, -7), wolfStats, new Color(.46f, .49f, .43f), false, PrototypeRuntimeFactory.SylvanBeastRecipe, .68f);
            var ent = Entity("Sylvan Ent", new Vector3(14, 1.5f, 4), CombatStats.Ent, new Color(.18f, .38f, .12f), true, PrototypeRuntimeFactory.GuardianEntRecipe, 1.45f);
            foreach (var enemy in new[] { wolfOne, wolfTwo, ent }) { enemy.Controller<CreatureBrain>().Target = hero; enemy.SetController(enemy.Controller<CreatureBrain>()); }

            var graph = new RealmGraph();
            foreach (var id in new[] { "Portal", "Crossroads", "Wolf Grove", "Ent Grove", "Root Path", "Moonwell", "Heart Tree" }) graph.Add(id);
            graph.Connect("Portal", "Crossroads"); graph.Connect("Crossroads", "Wolf Grove"); graph.Connect("Crossroads", "Ent Grove"); graph.Connect("Crossroads", "Root Path"); graph.Connect("Root Path", "Moonwell"); graph.Connect("Moonwell", "Heart Tree");

            var nodeViews = new List<RealmNodeView>();
            nodeViews.Add(Node(root, graph.Nodes["Portal"], hero, new Vector3(0, 0, -50), "PORTAL"));
            nodeViews.Add(Node(root, graph.Nodes["Crossroads"], hero, new Vector3(0, 0, -30), "CROSSROADS"));
            nodeViews.Add(Node(root, graph.Nodes["Wolf Grove"], hero, new Vector3(-14, 0, -10), "WOLF GROVE", wolfOne.gameObject, wolfTwo.gameObject));
            nodeViews.Add(Node(root, graph.Nodes["Ent Grove"], hero, new Vector3(14, 0, 4), "ENT GROVE", ent.gameObject));

            var trapObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder); trapObject.name = "Root Trap"; trapObject.transform.position = new Vector3(0, .12f, 4); trapObject.transform.localScale = new Vector3(2.4f, .12f, 2.4f); trapObject.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.2f, .75f, .28f)); trapObject.AddComponent<RootTrap>().Initialize(hero);
            nodeViews.Add(Node(root, graph.Nodes["Root Path"], hero, new Vector3(0, 0, 5), "ROOT PATH", trapObject));
            nodeViews.Add(Node(root, graph.Nodes["Moonwell"], hero, new Vector3(10, 0, 27), "MOONWELL"));

            var coreObject = CreateHeartTree(new Vector3(0, 2.5f, 50));
            nodeViews.Add(Node(root, graph.Nodes["Heart Tree"], hero, new Vector3(0, 0, 50), "HEART TREE", coreObject));
            CreatePath(new Vector3(0, 0, -40), new Vector2(7, 20)); CreatePath(new Vector3(-7, 0, -20), new Vector2(6, 28), -35); CreatePath(new Vector3(7, 0, -13), new Vector2(6, 38), 25); CreatePath(new Vector3(0, 0, -12), new Vector2(7, 36)); CreatePath(new Vector3(5, 0, 16), new Vector2(7, 26), -22); CreatePath(new Vector3(5, 0, 39), new Vector2(7, 25), 24);

            var manager = root.AddComponent<RaidManager>(); manager.Initialize(hero, nodeViews.ToArray(), new[] { wolfOne, wolfTwo, ent });
            var core = coreObject.GetComponent<RealmCore>(); core.Initialize(hero); core.InteractionStarted += manager.BeginObjective; core.Completed += manager.CompleteObjective;
            PrototypeRuntimeFactory.EventSystem(root.transform);
            var hudObject = new GameObject("Raid HUD", typeof(RaidHUD)); hudObject.transform.SetParent(root.transform); var hud = hudObject.GetComponent<RaidHUD>(); hud.Initialize(manager, hero); core.ProgressChanged += hud.SetObjectiveProgress;
            graph.Nodes["Portal"].Visit();
        }

        static RealmNodeView Node(GameObject root, RealmNode node, CombatEntity hero, Vector3 position, string label, params GameObject[] contents)
        {
            var area = new GameObject(label); area.transform.SetParent(root.transform); area.transform.position = position;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder); floor.name = label + " Ground"; floor.transform.SetParent(area.transform); floor.transform.localPosition = Vector3.zero; floor.transform.localScale = new Vector3(6.5f, .08f, 6.5f); floor.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(Moss);
            var revealables = new List<GameObject>(contents);
            for (int i = 0; i < 7; i++)
            {
                float angle = i * Mathf.PI * 2 / 7; var tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder); tree.name = "Tree"; tree.transform.SetParent(area.transform); tree.transform.localPosition = new Vector3(Mathf.Sin(angle) * 5.4f, 1.5f, Mathf.Cos(angle) * 5.4f); tree.transform.localScale = new Vector3(.45f, 2.2f + i % 2, .45f); tree.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.19f, .28f, .1f));
                revealables.Add(tree);
            }
            var view = area.AddComponent<RealmNodeView>(); view.Initialize(node, hero, floor.GetComponent<Renderer>(), revealables.ToArray()); return view;
        }

        static void CreatePath(Vector3 center, Vector2 size, float yaw = 0)
        {
            var path = GameObject.CreatePrimitive(PrimitiveType.Cube); path.name = "Living Path"; path.transform.position = center + Vector3.down * .06f; path.transform.rotation = Quaternion.Euler(0, yaw, 0); path.transform.localScale = new Vector3(size.x, .12f, size.y); path.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(Forest);
        }

        static GameObject CreateHeartTree(Vector3 position)
        {
            var root = new GameObject("Heart Tree"); root.transform.position = position;
            var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder); trunk.transform.SetParent(root.transform); trunk.transform.localPosition = Vector3.zero; trunk.transform.localScale = new Vector3(1.7f, 2.5f, 1.7f); trunk.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.27f, .15f, .07f));
            var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere); crown.transform.SetParent(root.transform); crown.transform.localPosition = new Vector3(0, 3.5f, 0); crown.transform.localScale = new Vector3(4.5f, 3.5f, 4.5f); crown.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.16f, .7f, .3f));
            root.AddComponent<RealmCore>(); return root;
        }

        static CombatEntity Entity(string name, Vector3 position, CombatStats stats, Color color, bool heavy, CharacterVisualRecipe recipe, float scale = 1)
        {
            var abilities = heavy ? new[] { PrototypeRuntimeFactory.Ability("Smash", AbilityKind.Melee, 34, 2.7f, 1.3f, .45f, .9f), PrototypeRuntimeFactory.Ability("Charge", AbilityKind.Dash, 24, 1.8f, 3, .2f, .9f, 5), PrototypeRuntimeFactory.Ability("Ground Slam", AbilityKind.Area, 38, 1, 4, .75f) } : name.StartsWith("Wolf") ? new[] { PrototypeRuntimeFactory.Ability("Leap", AbilityKind.Melee, 11, 2.4f, 1, .14f, .9f) } : new[] { PrototypeRuntimeFactory.Ability("Basic Slash", AbilityKind.Melee, 23, 2.3f, .9f, .18f, .9f), PrototypeRuntimeFactory.Ability("Blood Rush", AbilityKind.Dash, 25, 1.8f, 3, .15f, .9f, 6), PrototypeRuntimeFactory.Ability("Heavy Cleave", AbilityKind.Area, 35, 1.8f, 2.8f, .65f) };
            return PrototypeRuntimeFactory.CreateEntity(name, position, stats, color, heavy, Vector3.one * scale, abilities, heavy, recipe);
        }
    }
}
