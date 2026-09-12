using System;
using System.Linq;
using RealmRaiders.AI;
using RealmRaiders.CameraSystem;
using RealmRaiders.Characters;
using RealmRaiders.Combat;
using RealmRaiders.Controllers;
using RealmRaiders.Modules.InfernalEncounters;
using RealmRaiders.Raid;
using RealmRaiders.Realm;
using RealmRaiders.Traps;
using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RealmRaiders.Core
{
    public static class InfernalRaidBootstrap
    {
        public const string SceneName = "InfernalRaid";
        public const string RootName = "Infernal Ent Raid";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != SceneName || UnityEngine.Object.FindFirstObjectByType<RaidManager>()) return;
            Application.targetFrameRate = 60; QualitySettings.vSyncCount = 0; Build();
        }

        public static void ValidateSelectedRecipeForTests()
            => Validate(StarterInfernalRaidPacingCatalogue.BruteFinale, StarterInfernalEntTrialSpatialRecipes.BruteFinale);

        static void Build()
        {
            var pacing = StarterInfernalRaidPacingCatalogue.BruteFinale;
            var spatial = StarterInfernalEntTrialSpatialRecipes.BruteFinale;
            Validate(pacing, spatial);
            var houndA = Point(spatial, StarterInfernalRaidPacingCatalogue.BruteFinaleHellhoundABeatId, StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId);
            var houndB = Point(spatial, StarterInfernalRaidPacingCatalogue.BruteFinaleHellhoundBBeatId, StarterInfernalRaidPacingCatalogue.HellhoundArchetypeId);
            var brutePoint = Point(spatial, StarterInfernalRaidPacingCatalogue.BruteFinaleInfernalBruteBeatId, StarterInfernalRaidPacingCatalogue.InfernalBruteArchetypeId);
            var heartPoint = Point(spatial, StarterInfernalRaidPacingCatalogue.BruteFinaleInfernalHeartBeatId, StarterInfernalRaidPacingCatalogue.InfernalHeartContentId);
            var flamePoint = spatial.FlameTrap;

            var root = new GameObject(RootName);
            var rig = PrototypeRuntimeFactory.Camera(new Color(.08f, .012f, .008f), 48, new Vector3(0, 46, -33), Quaternion.Euler(57, 0, 0));
            PrototypeRuntimeFactory.DirectionalLight("Infernal Raid Glow", new Color(1, .25f, .08f), 1.4f, new Vector3(50, -30, 0)); RenderSettings.ambientLight = new Color(.22f, .06f, .03f);

            var laneLength = heartPoint.Z - spatial.Hero.Z + 8f;
            var laneCenter = (heartPoint.Z + spatial.Hero.Z) * .5f;
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name = "Infernal Raid Floor"; ground.transform.position = new Vector3(0, -.25f, laneCenter); ground.transform.localScale = new Vector3(spatial.LaneHalfWidth * 2, .5f, laneLength); RealmRoutePresentation.BuildDefenseLane(ground.transform, RealmRouteStyle.InfernalFractured);
            PrototypeArenaBoundaryBuilder.BuildRectangle(root.transform, new Vector3(0, 0, laneCenter), new Vector2(spatial.LaneHalfWidth * 2, laneLength), 0, PrototypeArenaBoundaryStyle.InfernalBasalt, 1.35f, 1, 5, .4f);

            var ent = Entity(spatial.Hero.ArchetypeId, "Guardian Ent", spatial.Hero.X, spatial.Hero.Z, CombatStats.Ent, new Color(.18f, .38f, .12f), true, spatial.Hero.Scale, EntAbilities());
            ent.SetController(ent.Controller<PlayerController>()); rig.SnapTo(ent, CameraMode.HeroCombat);

            var houndStats = new CombatStats { MaxHealth = 60, AttackDamage = 12, AttackSpeed = 1.5f, MoveSpeed = 7.2f, Armor = 2, AbilityPower = 5 };
            var firstHound = Entity(houndA.ContentId, "Hellhound A", houndA.X, houndA.Z, houndStats, new Color(.38f, .07f, .025f), false, .7f, HoundAbilities());
            var secondHound = Entity(houndB.ContentId, "Hellhound B", houndB.X, houndB.Z, houndStats, new Color(.48f, .1f, .025f), false, .7f, HoundAbilities());
            var bruteStats = CombatStats.Ent; bruteStats.MaxHealth = 380; bruteStats.AttackDamage = 38; bruteStats.MoveSpeed = 2.8f;
            var brute = Entity(brutePoint.ContentId, "Infernal Brute", brutePoint.X, brutePoint.Z, bruteStats, new Color(.3f, .08f, .045f), true, 1.45f,
                new[] { PrototypeRuntimeFactory.Ability("Smash", AbilityKind.Melee, 40, 2.7f, 1.3f, .5f, .9f) });
            foreach (var enemy in new[] { firstHound, secondHound, brute }) { var brain = enemy.Controller<CreatureBrain>(); brain.Target = ent; enemy.SetController(brain); }

            var flameObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder); flameObject.name = "Infernal Raid Flame Trap"; flameObject.transform.position = new Vector3(flamePoint.X, .1f, flamePoint.Z); flameObject.transform.localScale = new Vector3(flamePoint.TriggerRadius * 2, .1f, flamePoint.TriggerRadius * 2); RealmLandmarkPresentation.Build(flameObject.transform, RealmLandmarkRecipe.InfernalFlameTrap);
            foreach (var collider in flameObject.GetComponentsInChildren<Collider>(true)) collider.enabled = false;
            var flame = flameObject.AddComponent<FlameTrap>(); flame.Initialize(ent); flame.TriggerRadius = flamePoint.TriggerRadius; flame.Automatic = flamePoint.AutomaticAfterInitialize;

            var heartObject = InfernalHeart(new Vector3(heartPoint.X, 2.5f, heartPoint.Z));
            var core = heartObject.GetComponent<RealmCore>(); core.Initialize(ent); core.enabled = false;
            var raid = root.AddComponent<RaidManager>(); raid.Initialize(ent, Array.Empty<RealmNodeView>(), new[] { firstHound, secondHound, brute }, heartObject.transform.position);
            void UnlockHeart()
            {
                if (!core || core.enabled || !raid || !raid.AllowsRecovery) return;
                core.enabled = true;
            }
            brute.Health.Died += UnlockHeart;
            core.InteractionStarted += raid.BeginObjective; core.Completed += raid.CompleteObjective;

            PrototypeRuntimeFactory.EventSystem(root.transform);
            var hudObject = new GameObject("Infernal Raid HUD", typeof(RaidHUD)); hudObject.transform.SetParent(root.transform);
            var hud = hudObject.GetComponent<RaidHUD>(); hud.Initialize(raid, ent, core, rig.GetComponent<Camera>(), null, RaidHudConfig.InfernalEnt, brute); core.ProgressChanged += hud.SetObjectiveProgress;
            rig.BindCombatHud(hud.GetComponent<ResponsiveHudRoot>(), hud.ObjectiveCompassRect);
        }

        static void Validate(InfernalRaidPacingComposition pacing, InfernalEntTrialSpatialRecipe spatial)
        {
            if (pacing == null || spatial == null || pacing.CompositionId != spatial.CompositionId)
                throw new InvalidOperationException("Infernal Brute Finale pacing and spatial recipes must share one identity.");
            if (pacing.HeroArchetypeId != PrototypeCharacterRoster.GuardianEntId || spatial.Hero == null || spatial.Hero.ArchetypeId != pacing.HeroArchetypeId)
                throw new InvalidOperationException("Infernal Brute Finale must use the exact Guardian Ent hero recipe.");
            if (pacing.CompletionPrerequisiteGateId != StarterInfernalRaidPacingCatalogue.InfernalBruteDefeatedGateId || pacing.Beats.Count != 5 || spatial.EncounterPoints.Count != 4 || spatial.FlameTrap == null)
                throw new InvalidOperationException("Infernal Brute Finale recipe is incomplete or has an unsupported completion gate.");
            if (!spatial.FlameTrap.AutomaticAfterInitialize || !spatial.FlameTrap.RequiresNonBlockingPresentation || spatial.FlameTrap.TriggerRadius <= 0 || spatial.FlameTrap.LeftBypassX >= -spatial.FlameTrap.TriggerRadius || spatial.FlameTrap.RightBypassX <= spatial.FlameTrap.TriggerRadius)
                throw new InvalidOperationException("Infernal Flame Trap must remain automatic, nonblocking and bypassable on both sides.");
            foreach (var beat in pacing.Beats)
            {
                var matchesPoint = spatial.EncounterPoints.Any(point => point.BeatId == beat.BeatId && point.ContentId == beat.ContentId);
                var matchesHazard = spatial.FlameTrap.BeatId == beat.BeatId && spatial.FlameTrap.ContentId == beat.ContentId;
                if (!matchesPoint && !matchesHazard) throw new InvalidOperationException($"Infernal recipe beat '{beat.BeatId}' has no exact spatial fact.");
            }
        }

        static InfernalEntTrialEncounterPoint Point(InfernalEntTrialSpatialRecipe recipe, string beatId, string contentId)
        {
            var matches = recipe.EncounterPoints.Where(point => point.BeatId == beatId && point.ContentId == contentId).ToArray();
            if (matches.Length != 1) throw new InvalidOperationException($"Infernal recipe requires one exact '{beatId}' point; found {matches.Length}.");
            return matches[0];
        }

        static CombatEntity Entity(string archetypeId, string name, float x, float z, CombatStats stats, Color color, bool heavy, float scale, AbilityDefinition[] abilities)
            => PrototypeRuntimeFactory.CreateEntity(archetypeId, name, new Vector3(x, heavy ? 1.5f : .7f, z), stats, color, heavy, Vector3.one * scale, abilities, heavy);

        static AbilityDefinition[] EntAbilities() => new[]
        {
            PrototypeRuntimeFactory.Ability("Smash", AbilityKind.Melee, 34, 2.7f, 1.3f, .45f, .9f),
            PrototypeRuntimeFactory.Ability("Charge", AbilityKind.Dash, 24, 1.8f, 3, .2f, .9f, 5),
            PrototypeRuntimeFactory.Ability("Ground Slam", AbilityKind.Area, 38, 1, 4, .75f, recovery: .8f)
        };

        static AbilityDefinition[] HoundAbilities() => new[] { PrototypeRuntimeFactory.Ability("Leap", AbilityKind.Melee, 12, 2.4f, 1, .14f, .9f) };

        static GameObject InfernalHeart(Vector3 position)
        {
            var root = new GameObject("Infernal Heart"); root.transform.position = position; RealmLandmarkPresentation.Build(root.transform, RealmLandmarkRecipe.InfernalHeart); root.AddComponent<RealmCore>(); return root;
        }
    }
}
