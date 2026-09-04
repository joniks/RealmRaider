using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RealmRaiders.Core
{
    public static class HubBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register() { SceneManager.sceneLoaded -= OnSceneLoaded; SceneManager.sceneLoaded += OnSceneLoaded; }
        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "PrototypeHub" || Object.FindFirstObjectByType<HubHUD>()) return;
            Application.targetFrameRate = 60; QualitySettings.vSyncCount = 0;
            var root = new GameObject("Prototype Hub");
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 0, -10); camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor; camera.GetComponent<Camera>().backgroundColor = new Color(.025f, .06f, .045f);
            var light = new GameObject("Hub Light", typeof(Light)).GetComponent<Light>(); light.type = LightType.Directional; light.intensity = 1.2f; light.transform.rotation = Quaternion.Euler(45, -30, 0);
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder); platform.name = "Realm Platform"; platform.transform.position = new Vector3(0, -1.5f, 0); platform.transform.localScale = new Vector3(8, .7f, 8); platform.GetComponent<Renderer>().material = Material(new Color(.12f, .3f, .18f));
            for (int i = 0; i < 8; i++) { var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube); pillar.name = "Realm Pillar"; float angle = i * Mathf.PI * 2 / 8; pillar.transform.position = new Vector3(Mathf.Sin(angle) * 6, 1, Mathf.Cos(angle) * 6); pillar.transform.localScale = new Vector3(.7f, 3, .7f); pillar.GetComponent<Renderer>().material = Material(i % 2 == 0 ? new Color(.18f, .5f, .25f) : new Color(.5f, .16f, .06f)); }
            var eventSystem = new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule)); eventSystem.transform.SetParent(root.transform);
            var hud = new GameObject("Hub HUD", typeof(HubHUD)); hud.transform.SetParent(root.transform); hud.GetComponent<HubHUD>().Initialize();
        }
        static Material Material(Color color) { var value = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); value.color = color; return value; }
    }
}
