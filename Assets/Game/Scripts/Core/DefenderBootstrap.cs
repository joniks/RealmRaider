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
    public static class DefenderBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        { SceneManager.sceneLoaded -= OnSceneLoaded; SceneManager.sceneLoaded += OnSceneLoaded; }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "DefenderTest" || Object.FindFirstObjectByType<DefenseManager>()) return;
            Application.targetFrameRate = 60; QualitySettings.vSyncCount = 0; Build();
        }

        static void Build()
        {
            var root = new GameObject("Sylvan Defense Simulation");
            var cameraRig = PrototypeRuntimeFactory.Camera(new Color(.018f, .055f, .035f), 48, new Vector3(0, 46, -33), Quaternion.Euler(57, 0, 0));
            PrototypeRuntimeFactory.DirectionalLight("Forest Moon", new Color(.7f, .9f, .78f), 1.3f, new Vector3(50, -32, 0)); RenderSettings.ambientLight = new Color(.14f, .22f, .16f);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name = "Sylvan Path"; ground.transform.position = new Vector3(0, -.25f, 0); ground.transform.localScale = new Vector3(14, .5f, 68); RealmRoutePresentation.BuildDefenseLane(ground.transform, RealmRouteStyle.SylvanOrganic);
            PrototypeArenaBoundaryBuilder.BuildRectangle(root.transform, Vector3.zero, new Vector2(14, 68), 0,
                PrototypeArenaBoundaryStyle.SylvanRoots, 1.2f, 1, 5, .4f);
            for (int i = 0; i < 16; i++) CreateTree(new Vector3(i % 2 == 0 ? -8 : 8, 2, -30 + i * 4));

            var invaderStats = CombatStats.BloodKnight; invaderStats.MaxHealth = 220; invaderStats.MoveSpeed = 3.8f;
            var invader = Entity(PrototypeCharacterRoster.BloodKnightId, "Invading Blood Knight", new Vector3(0, 1, -30), invaderStats, new Color(.72f, .05f, .07f), false, .95f);
            var layout = DefenseLayoutSave.Load();
            var guardianEntRank = RealmProgress.Load().GuardianEntVitalityRank;
            var wolfStats = new CombatStats { MaxHealth = 52, AttackDamage = 9, AttackSpeed = 1.5f, MoveSpeed = 6.5f, Armor = 2, AbilityPower = 4 };
            var defenders = new System.Collections.Generic.List<CombatEntity>();
            CombatEntity ent = null; RootTrap trap = null;
            var creaturePositions = new[] { new Vector3(-3.2f, 0, -4), new Vector3(3.2f, 0, 2), new Vector3(0, 0, 11) };
            int wolfIndex = 0;
            for (int i = 0; i < 3; i++)
            {
                var piece = layout.Slots[i].Piece;
                var spawnPosition = creaturePositions[i];
                if (piece == DefensePieceType.Wolf) { spawnPosition.y = .7f; var wolf = Entity(PrototypeCharacterRoster.SylvanWolfId, $"Realm Wolf {(char)('A' + wolfIndex++)}", spawnPosition, wolfStats, new Color(.42f - wolfIndex * .04f, .45f - wolfIndex * .04f, .4f - wolfIndex * .035f), false, .7f); defenders.Add(wolf); }
                else if (piece == DefensePieceType.Ent)
                {
                    spawnPosition.y = 2.1f;
                    var entStats = CombatStats.Ent;
                    entStats.MaxHealth = RealmProgress.GuardianEntMaximumHealth(entStats.MaxHealth);
                    ent = Entity(PrototypeCharacterRoster.GuardianEntId, "Guardian Ent", spawnPosition, entStats, new Color(.18f, .43f, .14f), true, 1.4f);
                    ent.gameObject.AddComponent<GuardianEntGrowthPresentation>().Configure(guardianEntRank);
                    defenders.Add(ent);
                }
            }
            foreach (var defender in defenders) { var brain = defender.Controller<CreatureBrain>(); brain.Target = invader; defender.SetController(brain); }

            var invaderBrain = invader.gameObject.AddComponent<RaidInvaderBrain>();
            invader.RefreshControllers();
            invaderBrain.Configure(new[] { new Vector3(0, 1, -20), new Vector3(0, 1, -7), new Vector3(0, 1, 5), new Vector3(0, 1, 18), new Vector3(0, 1, 29) }, defenders.ToArray()); invader.SetController(invaderBrain);

            for (int slot = 3; slot < 5; slot++) if (layout.Slots[slot].Piece == DefensePieceType.RootTrap) { var trapObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder); trapObject.name = "Manual Root Trap"; trapObject.transform.position = slot == 3 ? new Vector3(0, .1f, -7) : new Vector3(6, .1f, 8); trapObject.transform.localScale = new Vector3(2.7f, .1f, 2.7f); RealmLandmarkPresentation.Build(trapObject.transform, RealmLandmarkRecipe.SylvanRootTrap); trap = trapObject.AddComponent<RootTrap>(); trap.Initialize(invader); trap.Automatic = false; trap.TriggerRadius = 3.5f; }
            var heart = HeartTree(new Vector3(0, 2.5f, 30)); var core = heart.GetComponent<RealmCore>(); core.Initialize(invader);

            var possession = root.AddComponent<PossessionManager>(); var energy = new PossessionEnergy(30); possession.Initialize(cameraRig); possession.ConfigureEnergy(energy); if (ent) possession.Register(ent);
            var defense = root.AddComponent<DefenseManager>(); defense.Initialize(invader, core, possession);
            var hudObject = new GameObject("Defender HUD", typeof(DefenderHUD)); hudObject.transform.SetParent(root.transform); hudObject.GetComponent<DefenderHUD>().Initialize(defense, possession, energy, invader, ent, trap, core, DefenseHudConfig.Sylvan); cameraRig.BindCombatHud(hudObject.GetComponent<ResponsiveHudRoot>());
            PrototypeRuntimeFactory.EventSystem(root.transform);
        }

        static CombatEntity Entity(string archetypeId, string name, Vector3 position, CombatStats stats, Color color, bool possessable, float scale)
        {
            var abilities = archetypeId switch
            {
                PrototypeCharacterRoster.GuardianEntId => new[] { PrototypeRuntimeFactory.Ability("Smash", AbilityKind.Melee, 34, 2.7f, 1.3f, .45f, .9f), PrototypeRuntimeFactory.Ability("Charge", AbilityKind.Dash, 24, 1.8f, 3, .2f, .9f, 5), PrototypeRuntimeFactory.Ability("Ground Slam", AbilityKind.Area, 38, 1, 4, .75f) },
                PrototypeCharacterRoster.SylvanWolfId => new[] { PrototypeRuntimeFactory.Ability("Leap", AbilityKind.Melee, 9, 2.4f, 1, .14f, .9f) },
                PrototypeCharacterRoster.BloodKnightId => new[] { PrototypeRuntimeFactory.Ability("Basic Slash", AbilityKind.Melee, 21, 2.3f, .9f, .18f, .9f), PrototypeRuntimeFactory.Ability("Blood Rush", AbilityKind.Dash, 24, 1.8f, 3, .15f, .9f, 6), PrototypeRuntimeFactory.Ability("Heavy Cleave", AbilityKind.Area, 34, 1.8f, 2.8f, .65f) },
                _ => throw new System.InvalidOperationException($"Sylvan defense has no ability set for archetype '{archetypeId}'.")
            };
            return PrototypeRuntimeFactory.CreateEntity(archetypeId, name, position, stats, color, possessable, Vector3.one * scale, abilities, possessable);
        }

        static void CreateTree(Vector3 position) { var tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder); tree.name = "Ancient Tree"; tree.transform.position = position; tree.transform.localScale = new Vector3(.7f, 3.5f, .7f); tree.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.18f, .27f, .09f)); }
        static GameObject HeartTree(Vector3 position) { var root = new GameObject("Heart Tree"); root.transform.position = position; RealmLandmarkPresentation.Build(root.transform, RealmLandmarkRecipe.SylvanHeartTree); root.AddComponent<RealmCore>(); return root; }
    }
}
