using RealmRaiders.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RealmRaiders.Core
{
    public static class RealmBuildBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register() { SceneManager.sceneLoaded -= OnSceneLoaded; SceneManager.sceneLoaded += OnSceneLoaded; }
        static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (scene.name != "RealmBuild" || Object.FindFirstObjectByType<BuildHUD>()) return;
            var root = new GameObject("Sylvan Build");
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)); camera.tag = "MainCamera"; camera.transform.position = new Vector3(0, 0, -10); camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor; camera.GetComponent<Camera>().backgroundColor = new Color(.025f, .06f, .045f);
            PrototypeRuntimeFactory.DirectionalLight("Build Light", Color.white, 1.2f, new Vector3(45, -30, 0));
            PrototypeRuntimeFactory.EventSystem(root.transform);
            var hud = new GameObject("BUILD HUD", typeof(BuildHUD)); hud.transform.SetParent(root.transform); hud.GetComponent<BuildHUD>().Initialize();
        }
    }
}
