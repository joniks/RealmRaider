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
            var identity = SylvanStarterRealmIdentity.LoadOrCreate();
            var layout = SylvanRealmLayoutMaterializer.Create(identity);
            var pacingSnapshot = SylvanPacingRunTracker.ResolveSnapshot(layout.LayoutId);
            var composition = SylvanRaidCompositionSelection.Current;
            ValidateComposition(composition);
            var boundaryNodes = layout.CreateNodeFootprints();
            var boundaryPaths = layout.CreatePathFootprints();
            var root = new GameObject("Sylvan Realm Raid");
            try { PrototypeArenaBoundaryBuilder.BuildSylvan(root.transform, boundaryNodes, boundaryPaths); }
            catch
            {
                Object.DestroyImmediate(root);
                throw;
            }
            var pacing = pacingSnapshot != null ? SylvanPacingRunTracker.Attach(root.transform, layout.LayoutId, pacingSnapshot) : null;
            var cameraRig = PrototypeRuntimeFactory.Camera(new Color(.018f, .055f, .035f), 52, new Vector3(0, 22, -11), Quaternion.Euler(60, 0, 0));
            PrototypeRuntimeFactory.DirectionalLight("Forest Moon", new Color(.68f, .86f, .76f), 1.25f, new Vector3(52, -28, 0));
            RenderSettings.ambientLight = new Color(.14f, .21f, .17f);

            var hero = Entity(PrototypeCharacterRoster.BloodKnightId, "Blood Knight", At(layout.PortalStart, 1), CombatStats.BloodKnight, new Color(.7f, .055f, .07f), false);
            hero.SetController(hero.Controller<PlayerController>()); cameraRig.SnapTo(hero, CameraMode.HeroCombat);

            var wolfStats = new CombatStats { MaxHealth = 58, AttackDamage = 11, AttackSpeed = 1.5f, MoveSpeed = 7, Armor = 2, AbilityPower = 5 };
            var nodeCenters = new Dictionary<string, Vector3>
            {
                [StarterSylvanRaidCompositions.WolfGroveNodeId] = At(layout.WolfGroveEncounter),
                [StarterSylvanRaidCompositions.EntGroveNodeId] = At(layout.EntGroveEncounter),
                [StarterSylvanRaidCompositions.MoonwellNodeId] = At(layout.MoonwellRecovery)
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

            var graph = layout.BuildGraph();

            var nodeViews = new List<RealmNodeView>();
            nodeViews.Add(Node(root, graph.Nodes[layout.PortalStart.GraphId], hero, At(layout.PortalStart), layout.PortalStart.Label));
            nodeViews.Add(Node(root, graph.Nodes[layout.LandmarkJunction.GraphId], hero, At(layout.LandmarkJunction), layout.LandmarkJunction.Label));
            var wolfView = Node(root, graph.Nodes[layout.WolfGroveEncounter.GraphId], hero, At(layout.WolfGroveEncounter), layout.WolfGroveEncounter.Label, nodeContents[StarterSylvanRaidCompositions.WolfGroveNodeId].ToArray());
            nodeViews.Add(wolfView);
            var entView = Node(root, graph.Nodes[layout.EntGroveEncounter.GraphId], hero, At(layout.EntGroveEncounter), layout.EntGroveEncounter.Label, nodeContents[StarterSylvanRaidCompositions.EntGroveNodeId].ToArray());
            nodeViews.Add(entView);

            var trapObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder); trapObject.name = "Root Trap"; trapObject.transform.SetParent(root.transform); trapObject.transform.position = At(layout.RootPathHazard, .12f); trapObject.transform.localScale = new Vector3(2.4f, .12f, 2.4f); RealmLandmarkPresentation.Build(trapObject.transform, RealmLandmarkRecipe.SylvanRootTrap); trapObject.AddComponent<RootTrap>().Initialize(hero);
            var rootPathView = Node(root, graph.Nodes[layout.RootPathHazard.GraphId], hero, At(layout.RootPathHazard), layout.RootPathHazard.Label, trapObject);
            nodeViews.Add(rootPathView);
            var moonwellObject = CreateMoonwell(root.transform, At(layout.MoonwellRecovery), out var moonwellRenderer);
            var moonwellContents = nodeContents[StarterSylvanRaidCompositions.MoonwellNodeId];
            moonwellContents.Insert(0, moonwellObject);
            var moonwellView = Node(root, graph.Nodes[layout.MoonwellRecovery.GraphId], hero, At(layout.MoonwellRecovery), layout.MoonwellRecovery.Label, moonwellContents.ToArray());
            nodeViews.Add(moonwellView);

            var coreObject = CreateHeartTree(root.transform, At(layout.HeartTreeObjective, 2.5f));
            nodeViews.Add(Node(root, graph.Nodes[layout.HeartTreeObjective.GraphId], hero, At(layout.HeartTreeObjective), layout.HeartTreeObjective.Label, coreObject));
            foreach (var path in boundaryPaths) CreatePath(root.transform, new Vector3(path.Center.x, 0, path.Center.y), path.Size, path.Yaw);

            var manager = root.AddComponent<RaidManager>(); manager.Initialize(hero, nodeViews.ToArray(), enemies.ToArray(), coreObject.transform.position, ent);
            if (pacing)
            {
                pacing.BindNode(layout.WolfGroveEncounter.RoleId, wolfView);
                pacing.BindNode(layout.RootPathHazard.RoleId, rootPathView);
                pacing.BindNode(layout.EntGroveEncounter.RoleId, entView);
                pacing.BindNode(layout.MoonwellRecovery.RoleId, moonwellView);
                if (!pacing.SealBindings()) pacing = null;
                else pacing.BindRaid(manager);
            }
            var moonwell = moonwellObject.AddComponent<MoonwellRecovery>(); moonwell.Initialize(hero, manager, moonwellRenderer);
            var core = coreObject.GetComponent<RealmCore>(); core.Initialize(hero); core.SetInteractionWard(pacing ? () => pacing.IsWarded : null); core.InteractionStarted += manager.BeginObjective; core.Completed += manager.CompleteObjective;
            PrototypeRuntimeFactory.EventSystem(root.transform);
            var hudObject = new GameObject("Raid HUD", typeof(RaidHUD)); hudObject.transform.SetParent(root.transform); var hud = hudObject.GetComponent<RaidHUD>(); hud.Initialize(manager, hero, core, cameraRig.GetComponent<Camera>(), moonwell, variantDisplayName: composition.DisplayName, objectiveWardCopy: () => core.HeroInRange && pacing && pacing.IsWarded ? $"WARD: {pacing.NextRequirementCopy}" : null); if (pacing) pacing.Changed += hud.RefreshObjectiveCopy; cameraRig.BindCombatHud(hud.GetComponent<ResponsiveHudRoot>(), hud.ObjectiveCompassRect); core.ProgressChanged += hud.SetObjectiveProgress;
            graph.Nodes[layout.PortalStart.GraphId].Visit();
        }

        static Vector3 At(SylvanRealmMaterializationNode node, float y = 0) => new(node.Center.x, y, node.Center.y);

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

        static void CreatePath(Transform owner, Vector3 center, Vector2 size, float yaw = 0)
        {
            var path = GameObject.CreatePrimitive(PrimitiveType.Cube); path.name = "Living Path"; path.transform.SetParent(owner); path.transform.position = center + Vector3.down * .06f; path.transform.rotation = Quaternion.Euler(0, yaw, 0); path.transform.localScale = new Vector3(size.x, .12f, size.y); path.isStatic = true; path.GetComponent<Collider>().isTrigger = false; RealmRoutePresentation.BuildSegment(path.transform, RealmRouteStyle.SylvanOrganic);
        }

        static GameObject CreateHeartTree(Transform owner, Vector3 position)
        {
            var root = new GameObject("Heart Tree"); root.transform.SetParent(owner); root.transform.position = position;
            RealmLandmarkPresentation.Build(root.transform, RealmLandmarkRecipe.SylvanHeartTree);
            root.AddComponent<RealmCore>(); return root;
        }

        static GameObject CreateMoonwell(Transform owner, Vector3 position, out Renderer recoveryRenderer)
        {
            var root = new GameObject("Moonwell Recovery"); root.transform.SetParent(owner); root.transform.position = position;
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
