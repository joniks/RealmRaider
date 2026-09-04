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
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); cameraObject.tag = "MainCamera"; cameraObject.transform.position = new Vector3(0, 0, -10); cameraObject.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor; cameraObject.GetComponent<Camera>().backgroundColor = new Color(.025f, .06f, .045f);
            PrototypeRuntimeFactory.DirectionalLight("Hub Light", Color.white, 1.2f, new Vector3(45, -30, 0));
            var platform = GameObject.CreatePrimitive(PrimitiveType.Cylinder); platform.name = "Realm Platform"; platform.transform.position = new Vector3(0, -1.5f, 0); platform.transform.localScale = new Vector3(8, .7f, 8); platform.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(new Color(.12f, .3f, .18f));
            for (int i = 0; i < 8; i++) { var pillar = GameObject.CreatePrimitive(PrimitiveType.Cube); pillar.name = "Realm Pillar"; float angle = i * Mathf.PI * 2 / 8; pillar.transform.position = new Vector3(Mathf.Sin(angle) * 6, 1, Mathf.Cos(angle) * 6); pillar.transform.localScale = new Vector3(.7f, 3, .7f); pillar.GetComponent<Renderer>().material = PrototypeRuntimeFactory.Material(i % 2 == 0 ? new Color(.18f, .5f, .25f) : new Color(.5f, .16f, .06f)); }
            PrototypeRuntimeFactory.EventSystem(root.transform);
            var hud = new GameObject("Hub HUD", typeof(HubHUD)); hud.transform.SetParent(root.transform); hud.GetComponent<HubHUD>().Initialize();
        }
    }
}
