using System.Collections.Generic;
using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Modules.SylvanEncounters;
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
            var composition = SylvanRaidCompositionSelection.Current;
            ValidateComposition(composition);
            var root = new GameObject("Sylvan Realm Raid");
            var cameraRig = PrototypeRuntimeFactory.Camera(new Color(.018f, .055f, .035f), 52, new Vector3(0, 22, -11), Quaternion.Euler(60, 0, 0));
            PrototypeRuntimeFactory.DirectionalLight("Forest Moon", new Color(.68f, .86f, .76f), 1.25f, new Vector3(52, -28, 0));
            RenderSettings.ambientLight = new Color(.14f, .21f, .17f);

            var hero = Entity(PrototypeCharacterRoster.BloodKnightId, "Blood Knight", new Vector3(0, 1, -50), CombatStats.BloodKnight, new Color(.7f, .055f, .07f), false);
            hero.SetController(hero.Controller<PlayerController>()); cameraRig.SnapTo(hero, CameraMode.HeroCombat);

            var wolfStats = new CombatStats { MaxHealth = 58, AttackDamage = 11, AttackSpeed = 1.5f, MoveSpeed = 7, Armor = 2, AbilityPower = 5 };
            var nodeCenters = new Dictionary<string, Vector3>
            {
                [StarterSylvanRaidCompositions.WolfGroveNodeId] = new Vector3(-14, 0, -10),
                [StarterSylvanRaidCompositions.EntGroveNodeId] = new Vector3(14, 0, 4),
                [StarterSylvanRaidCompositions.MoonwellNodeId] = new Vector3(10, 0, 27)
            };
            var nodeContents = new Dictionary<string, List<GameObject>>
            {
                [StarterSylvanRaidCompositions.WolfGroveNodeId] = new List<GameObject>(),
                [StarterSylvanRaidCompositions.EntGroveNodeId] = new List<GameObject>(),
                [StarterSylvanRaidCompositions.MoonwellNodeId] = new List<GameObject>()
            };
            var enemies = new List<CombatEntity>();
            CombatEntity ent = null;
            var wolfIndex = 0;
            foreach (var spawn in composition.Spawns)
            {
                var isEnt = spawn.ArchetypeId == StarterSylvanRaidCompositions.GuardianEntArchetypeId;
                var position = nodeCenters[spawn.NodeId] + new Vector3(spawn.LocalOffsetX, isEnt ? 1.5f : .65f, spawn.LocalOffsetZ);
                var enemy = Entity(spawn.ArchetypeId, spawn.DisplayName, position, isEnt ? CombatStats.Ent : wolfStats,
                    isEnt ? new Color(.18f, .38f, .12f) : WolfColor(wolfIndex++), isEnt, spawn.Scale);
                var brain = enemy.Controller<CreatureBrain>();
                brain.Target = hero;
                if (isEnt)
                {
                    brain.ConfigureGuardianEntHeavyAttackRhythm(new[] { hero });
                    ent = enemy;
                }
                enemy.SetController(brain);
                enemies.Add(enemy);
                nodeContents[spawn.NodeId].Add(enemy.gameObject);
            }

            var graph = new RealmGraph();
            foreach (var id in new[] { "Portal", "Crossroads", "Wolf Grove", "Ent Grove", "Root Path", "Moonwell", "Heart Tree" }) graph.Add(id);
            graph.Connect("Portal", "Crossroads"); graph.Connect("Crossroads", "Wolf Grove"); graph.Connect("Crossroads", "Ent Grove"); graph.Connect("Crossroads", "Root Path"); graph.Connect("Root Path", "Moonwell"); graph.Connect("Moonwell", "Heart Tree");

            var nodeViews = new List<RealmNodeView>();
            nodeViews.Add(Node(root, graph.Nodes["Portal"], hero, new Vector3(0, 0, -50), "PORTAL"));
            nodeViews.Add(Node(root, graph.Nodes["Crossroads"], hero, new Vector3(0, 0, -30), "CROSSROADS"));
            nodeViews.Add(Node(root, graph.Nodes["Wolf Grove"], hero, new Vector3(-14, 0, -10), "WOLF GROVE", nodeContents[StarterSylvanRaidCompositions.WolfGroveNodeId].ToArray()));
            nodeViews.Add(Node(root, graph.Nodes["Ent Grove"], hero, new Vector3(14, 0, 4), "ENT GROVE", nodeContents[StarterSylvanRaidCompositions.EntGroveNodeId].ToArray()));

            var trapObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder); trapObject.name = "Root Trap"; trapObject.transform.position = new Vector3(0, .12f, 4); trapObject.transform.localScale = new Vector3(2.4f, .12f, 2.4f); RealmLandmarkPresentation.Build(trapObject.transform, RealmLandmarkRecipe.SylvanRootTrap); trapObject.AddComponent<RootTrap>().Initialize(hero);
            nodeViews.Add(Node(root, graph.Nodes["Root Path"], hero, new Vector3(0, 0, 5), "ROOT PATH", trapObject));
            var moonwellObject = CreateMoonwell(new Vector3(10, 0, 27), out var moonwellRenderer);
            var moonwellContents = nodeContents[StarterSylvanRaidCompositions.MoonwellNodeId];
            moonwellContents.Insert(0, moonwellObject);
            nodeViews.Add(Node(root, graph.Nodes["Moonwell"], hero, new Vector3(10, 0, 27), "MOONWELL", moonwellContents.ToArray()));

            var coreObject = CreateHeartTree(new Vector3(0, 2.5f, 50));
            nodeViews.Add(Node(root, graph.Nodes["Heart Tree"], hero, new Vector3(0, 0, 50), "HEART TREE", coreObject));
            var boundaryNodes = new[]
            {
                new ArenaCircleFootprint(new Vector2(0, -50), 3.25f),
                new ArenaCircleFootprint(new Vector2(0, -30), 3.25f),
                new ArenaCircleFootprint(new Vector2(-14, -10), 3.25f),
                new ArenaCircleFootprint(new Vector2(14, 4), 3.25f),
                new ArenaCircleFootprint(new Vector2(0, 5), 3.25f),
                new ArenaCircleFootprint(new Vector2(10, 27), 3.25f),
                new ArenaCircleFootprint(new Vector2(0, 50), 3.25f)
            };
            var boundaryPaths = new[]
            {
                new ArenaPathFootprint(new Vector2(0, -40), new Vector2(7, 20)),
                new ArenaPathFootprint(new Vector2(-7, -20), new Vector2(6, 28), -35),
                new ArenaPathFootprint(new Vector2(7, -13), new Vector2(6, 38), 25),
                new ArenaPathFootprint(new Vector2(0, -12), new Vector2(7, 36)),
                new ArenaPathFootprint(new Vector2(5, 16), new Vector2(7, 26), 22),
                new ArenaPathFootprint(new Vector2(5, 39), new Vector2(7, 25), -24)
            };
            foreach (var path in boundaryPaths) CreatePath(new Vector3(path.Center.x, 0, path.Center.y), path.Size, path.Yaw);
            PrototypeArenaBoundaryBuilder.BuildSylvan(root.transform, boundaryNodes, boundaryPaths);

            var manager = root.AddComponent<RaidManager>(); manager.Initialize(hero, nodeViews.ToArray(), enemies.ToArray(), coreObject.transform.position, ent);
            var moonwell = moonwellObject.AddComponent<MoonwellRecovery>(); moonwell.Initialize(hero, manager, moonwellRenderer);
            var core = coreObject.GetComponent<RealmCore>(); core.Initialize(hero); core.InteractionStarted += manager.BeginObjective; core.Completed += manager.CompleteObjective;
            PrototypeRuntimeFactory.EventSystem(root.transform);
            var hudObject = new GameObject("Raid HUD", typeof(RaidHUD)); hudObject.transform.SetParent(root.transform); var hud = hudObject.GetComponent<RaidHUD>(); hud.Initialize(manager, hero, core, cameraRig.GetComponent<Camera>(), moonwell, variantDisplayName: composition.DisplayName); cameraRig.BindCombatHud(hud.GetComponent<ResponsiveHudRoot>(), hud.ObjectiveCompassRect); core.ProgressChanged += hud.SetObjectiveProgress;
            graph.Nodes["Portal"].Visit();
        }

        static void ValidateComposition(SylvanRaidComposition composition)
        {
            if (composition == null || string.IsNullOrWhiteSpace(composition.CompositionId) ||
                string.IsNullOrWhiteSpace(composition.DisplayName) || composition.Spawns == null)
                throw new System.InvalidOperationException("The selected Sylvan raid composition is malformed.");
            var spawnIds = new HashSet<string>(System.StringComparer.Ordinal);
            var entCount = 0;
            foreach (var spawn in composition.Spawns)
            {
                if (spawn == null || string.IsNullOrWhiteSpace(spawn.SpawnId) || !spawnIds.Add(spawn.SpawnId) ||
                    string.IsNullOrWhiteSpace(spawn.DisplayName) ||
                    (spawn.ArchetypeId != StarterSylvanRaidCompositions.SylvanWolfArchetypeId && spawn.ArchetypeId != StarterSylvanRaidCompositions.GuardianEntArchetypeId) ||
                    (spawn.NodeId != StarterSylvanRaidCompositions.WolfGroveNodeId && spawn.NodeId != StarterSylvanRaidCompositions.EntGroveNodeId && spawn.NodeId != StarterSylvanRaidCompositions.MoonwellNodeId) ||
                    !IsFinite(spawn.LocalOffsetX) || !IsFinite(spawn.LocalOffsetZ) || !IsFinite(spawn.Scale) || spawn.Scale <= 0)
                    throw new System.InvalidOperationException($"Sylvan raid composition '{composition.CompositionId}' contains an invalid spawn.");
                if (spawn.ArchetypeId == StarterSylvanRaidCompositions.GuardianEntArchetypeId) entCount++;
            }
            if (entCount != 1)
                throw new System.InvalidOperationException($"Sylvan raid composition '{composition.CompositionId}' must contain exactly one Guardian Ent.");
        }

        public static void ValidateSelectedCompositionForTests() => ValidateComposition(SylvanRaidCompositionSelection.Current);

        static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        static Color WolfColor(int index) => index % 2 == 0
            ? new Color(.36f, .39f, .35f)
            : new Color(.46f, .49f, .43f);

        static RealmNodeView Node(GameObject root, RealmNode node, CombatEntity hero, Vector3 position, string label, params GameObject[] contents)
        {
            var area = new GameObject(label); area.transform.SetParent(root.transform); area.transform.position = position;
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cylinder); floor.name = label + " Ground"; floor.transform.SetParent(area.transform); floor.transform.localPosition = Vector3.zero; floor.transform.localScale = new Vector3(6.5f, .08f, 6.5f);
            var floorRenderer = floor.GetComponent<Renderer>(); RealmNodeSurfacePresentation.Bind(floorRenderer, Moss);
            var primitiveCollider = floor.GetComponent<Collider>(); primitiveCollider.enabled = false; Object.Destroy(primitiveCollider);
            var groundColliderObject = new GameObject("Node Ground Collider", typeof(MeshCollider)); groundColliderObject.transform.SetParent(floor.transform, false); groundColliderObject.transform.localPosition = Vector3.down; groundColliderObject.isStatic = true;
            var groundCollider = groundColliderObject.GetComponent<MeshCollider>(); groundCollider.sharedMesh = floor.GetComponent<MeshFilter>().sharedMesh; groundCollider.convex = false; groundCollider.isTrigger = false;
            var revealables = new List<GameObject>(contents);
            for (int i = 0; i < 7; i++)
            {
                float angle = i * Mathf.PI * 2 / 7; var tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder); tree.name = "Tree"; tree.transform.SetParent(area.transform); tree.transform.localPosition = new Vector3(Mathf.Sin(angle) * 5.4f, 1.5f, Mathf.Cos(angle) * 5.4f); tree.transform.localScale = new Vector3(.45f, 2.2f + i % 2, .45f); tree.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.19f, .28f, .1f));
                var collider = tree.GetComponent<Collider>();
                collider.enabled = false;
                Object.Destroy(collider);
                revealables.Add(tree);
            }
            var view = area.AddComponent<RealmNodeView>(); view.Initialize(node, hero, floorRenderer, revealables.ToArray()); return view;
        }

        static void CreatePath(Vector3 center, Vector2 size, float yaw = 0)
        {
            var path = GameObject.CreatePrimitive(PrimitiveType.Cube); path.name = "Living Path"; path.transform.position = center + Vector3.down * .06f; path.transform.rotation = Quaternion.Euler(0, yaw, 0); path.transform.localScale = new Vector3(size.x, .12f, size.y); RealmRoutePresentation.BuildSegment(path.transform, RealmRouteStyle.SylvanOrganic);
        }

        static GameObject CreateHeartTree(Vector3 position)
        {
            var root = new GameObject("Heart Tree"); root.transform.position = position;
            RealmLandmarkPresentation.Build(root.transform, RealmLandmarkRecipe.SylvanHeartTree);
            root.AddComponent<RealmCore>(); return root;
        }

        static GameObject CreateMoonwell(Vector3 position, out Renderer recoveryRenderer)
        {
            var root = new GameObject("Moonwell Recovery"); root.transform.position = position;
            var basin = GameObject.CreatePrimitive(PrimitiveType.Cylinder); basin.name = "Moonwell Basin"; basin.transform.SetParent(root.transform, false); basin.transform.localPosition = new Vector3(0, .18f, 0); basin.transform.localScale = new Vector3(1.35f, .18f, 1.35f); basin.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.18f, .32f, .28f));
            var water = GameObject.CreatePrimitive(PrimitiveType.Cylinder); water.name = "Moonwell Water"; water.transform.SetParent(root.transform, false); water.transform.localPosition = new Vector3(0, .38f, 0); water.transform.localScale = new Vector3(.95f, .05f, .95f); recoveryRenderer = water.GetComponent<Renderer>(); recoveryRenderer.material = PrototypeRuntimeFactory.Material(new Color(.28f, .82f, .72f));
            foreach (var collider in root.GetComponentsInChildren<Collider>()) { collider.enabled = false; Object.Destroy(collider); }
            return root;
        }

        static CombatEntity Entity(string archetypeId, string name, Vector3 position, CombatStats stats, Color color, bool heavy, float scale = 1)
        {
            var abilities = archetypeId switch
            {
                PrototypeCharacterRoster.GuardianEntId => new[] { PrototypeRuntimeFactory.Ability("Smash", AbilityKind.Melee, 34, 2.7f, 1.3f, .45f, .9f), PrototypeRuntimeFactory.Ability("Charge", AbilityKind.Dash, 24, 1.8f, 3, .2f, .9f, 5), PrototypeRuntimeFactory.Ability("Ground Slam", AbilityKind.Area, 38, 1, 4, .75f, recovery: .8f) },
                PrototypeCharacterRoster.SylvanWolfId => new[] { PrototypeRuntimeFactory.Ability("Leap", AbilityKind.Melee, 11, 2.4f, 1, .14f, .9f) },
                PrototypeCharacterRoster.BloodKnightId => new[] { PrototypeRuntimeFactory.Ability("Basic Slash", AbilityKind.Melee, 23, 2.3f, .9f, .18f, .9f), PrototypeRuntimeFactory.Ability("Blood Rush", AbilityKind.Dash, 25, 1.8f, 3, .15f, .9f, 6), PrototypeRuntimeFactory.Ability("Heavy Cleave", AbilityKind.Area, 35, 1.8f, 2.8f, .65f) },
                _ => throw new System.InvalidOperationException($"Sylvan raid has no ability set for archetype '{archetypeId}'.")
            };
            return PrototypeRuntimeFactory.CreateEntity(archetypeId, name, position, stats, color, heavy, Vector3.one * scale, abilities, heavy);
        }
    }
}
