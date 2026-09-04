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
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
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
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener), typeof(PrototypeCameraRig)); cameraObject.tag = "MainCamera"; var camera = cameraObject.GetComponent<Camera>(); camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(.018f, .055f, .035f); camera.fieldOfView = 48;
            var cameraRig = cameraObject.GetComponent<PrototypeCameraRig>(); cameraRig.ConfigureOverview(new Vector3(0, 46, -33), Quaternion.Euler(57, 0, 0)); cameraRig.SnapToOverview();
            var sun = new GameObject("Forest Moon", typeof(Light)); var light = sun.GetComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.3f; light.color = new Color(.7f, .9f, .78f); sun.transform.rotation = Quaternion.Euler(50, -32, 0); RenderSettings.ambientLight = new Color(.14f, .22f, .16f);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name = "Sylvan Path"; ground.transform.position = new Vector3(0, -.25f, 0); ground.transform.localScale = new Vector3(14, .5f, 68); ground.GetComponent<Renderer>().material = Material(new Color(.08f, .24f, .1f));
            for (int i = 0; i < 16; i++) CreateTree(new Vector3(i % 2 == 0 ? -8 : 8, 2, -30 + i * 4));

            var invaderStats = CombatStats.BloodKnight; invaderStats.MaxHealth = 220; invaderStats.MoveSpeed = 3.8f;
            var invader = Entity("Invading Blood Knight", new Vector3(0, 1, -30), invaderStats, new Color(.72f, .05f, .07f), false, .95f);
            var wolfStats = new CombatStats { MaxHealth = 52, AttackDamage = 9, AttackSpeed = 1.5f, MoveSpeed = 6.5f, Armor = 2, AbilityPower = 4 };
            var wolfOne = Entity("Realm Wolf A", new Vector3(-3.2f, .7f, -4), wolfStats, new Color(.42f, .45f, .4f), false, .7f);
            var wolfTwo = Entity("Realm Wolf B", new Vector3(3.2f, .7f, 2), wolfStats, new Color(.34f, .37f, .33f), false, .7f);
            var ent = Entity("Guardian Ent", new Vector3(0, 1.5f, 11), CombatStats.Ent, new Color(.18f, .43f, .14f), true, 1.4f);
            foreach (var defender in new[] { wolfOne, wolfTwo, ent }) { var brain = defender.Controller<CreatureBrain>(); brain.Target = invader; defender.SetController(brain); }

            var invaderBrain = invader.gameObject.AddComponent<RaidInvaderBrain>();
            invader.RefreshControllers();
            invaderBrain.Configure(new[] { new Vector3(0, 1, -20), new Vector3(0, 1, -7), new Vector3(0, 1, 5), new Vector3(0, 1, 18), new Vector3(0, 1, 29) }, new[] { wolfOne, wolfTwo, ent }); invader.SetController(invaderBrain);

            var trapObject = GameObject.CreatePrimitive(PrimitiveType.Cylinder); trapObject.name = "Manual Root Trap"; trapObject.transform.position = new Vector3(0, .1f, -7); trapObject.transform.localScale = new Vector3(2.7f, .1f, 2.7f); trapObject.GetComponent<Renderer>().material = Material(new Color(.2f, .75f, .28f)); var trap = trapObject.AddComponent<RootTrap>(); trap.Initialize(invader); trap.Automatic = false; trap.TriggerRadius = 3.5f;
            var heart = HeartTree(new Vector3(0, 2.5f, 30)); var core = heart.GetComponent<RealmCore>(); core.Initialize(invader);

            var possession = root.AddComponent<PossessionManager>(); var energy = new PossessionEnergy(30); possession.Initialize(cameraRig); possession.ConfigureEnergy(energy); possession.Register(ent);
            var defense = root.AddComponent<DefenseManager>(); defense.Initialize(invader, core, possession);
            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule)); eventSystem.transform.SetParent(root.transform);
            var hudObject = new GameObject("Defender HUD", typeof(DefenderHUD)); hudObject.transform.SetParent(root.transform); hudObject.GetComponent<DefenderHUD>().Initialize(defense, possession, energy, invader, ent, trap, core, DefenseHudConfig.Sylvan);
        }

        static CombatEntity Entity(string name, Vector3 position, CombatStats stats, Color color, bool possessable, float scale)
        {
            var go = GameObject.CreatePrimitive(possessable ? PrimitiveType.Cube : PrimitiveType.Capsule); go.name = name; go.transform.position = position; Object.Destroy(go.GetComponent<Collider>()); var motor = go.AddComponent<CharacterController>(); motor.height = possessable ? 3 : 2; motor.radius = possessable ? .8f : .5f; go.AddComponent<Health>(); var entity = go.AddComponent<CombatEntity>(); go.AddComponent<PlayerController>(); var ai = go.AddComponent<CreatureBrain>(); go.GetComponent<Renderer>().material = Material(color); go.transform.localScale *= scale;
            var definition = ScriptableObject.CreateInstance<CharacterDefinition>(); definition.DisplayName = name; definition.Stats = stats; definition.Possessable = possessable; definition.PlaceholderColor = color;
            definition.Abilities = possessable ? new[] { Ability("Smash", AbilityKind.Melee, 34, 2.7f, 1.3f, .45f), Ability("Charge", AbilityKind.Dash, 24, 1.8f, 3, .2f, 5), Ability("Ground Slam", AbilityKind.Area, 38, 1, 4, .75f) } : name.Contains("Wolf") ? new[] { Ability("Leap", AbilityKind.Melee, 9, 2.4f, 1, .14f) } : new[] { Ability("Basic Slash", AbilityKind.Melee, 21, 2.3f, .9f, .18f), Ability("Blood Rush", AbilityKind.Dash, 24, 1.8f, 3, .15f, 6), Ability("Heavy Cleave", AbilityKind.Area, 34, 1.8f, 2.8f, .65f) };
            entity.Initialize(definition); entity.SetController(ai); return entity;
        }

        static void CreateTree(Vector3 position) { var tree = GameObject.CreatePrimitive(PrimitiveType.Cylinder); tree.name = "Ancient Tree"; tree.transform.position = position; tree.transform.localScale = new Vector3(.7f, 3.5f, .7f); tree.GetComponent<Renderer>().material = Material(new Color(.18f, .27f, .09f)); }
        static GameObject HeartTree(Vector3 position) { var root = new GameObject("Heart Tree"); root.transform.position = position; var trunk = GameObject.CreatePrimitive(PrimitiveType.Cylinder); trunk.transform.SetParent(root.transform); trunk.transform.localScale = new Vector3(1.8f, 2.5f, 1.8f); trunk.GetComponent<Renderer>().material = Material(new Color(.28f, .15f, .06f)); var crown = GameObject.CreatePrimitive(PrimitiveType.Sphere); crown.transform.SetParent(root.transform); crown.transform.localPosition = new Vector3(0, 3.5f, 0); crown.transform.localScale = new Vector3(4.5f, 3.5f, 4.5f); crown.GetComponent<Renderer>().material = Material(new Color(.12f, .72f, .26f)); root.AddComponent<RealmCore>(); return root; }
        static AbilityDefinition Ability(string name, AbilityKind kind, float damage, float range, float radius, float windup, float dash = 0) { var value = ScriptableObject.CreateInstance<AbilityDefinition>(); value.DisplayName = name; value.Kind = kind; value.Damage = damage; value.Range = range; value.Radius = radius; value.Windup = windup; value.Cooldown = kind == AbilityKind.Area ? 4 : .9f; value.DashDistance = dash; return value; }
        static Material Material(Color color) { var value = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); value.color = color; return value; }
    }
}
