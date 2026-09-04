using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RealmRaiders.Editor
{
    public static class PlatformBuild
    {
        const string AndroidOutput = "Builds/AndroidStudio";
        const string IosOutput = "Builds/Xcode";
        const string IosSimulatorOutput = "Builds/XcodeSimulator";

        static readonly string[] Scenes =
        {
            "Assets/Game/Scenes/PrototypeHub.unity",
            "Assets/Game/Scenes/RealmBuild.unity",
            "Assets/Game/Scenes/CharacterSandbox.unity",
            "Assets/Game/Scenes/SylvanRealm.unity",
            "Assets/Game/Scenes/DefenderTest.unity",
            "Assets/Game/Scenes/InfernalRealm.unity"
        };

        [MenuItem("Realm Raiders/Build/Export Android Studio Project")]
        public static void ExportAndroidStudioProject()
        {
            EnsurePlatformSupport(BuildTargetGroup.Android, BuildTarget.Android, "Android Build Support (including SDK, NDK and OpenJDK)");
            SwitchPlatform(BuildTargetGroup.Android, BuildTarget.Android);

            bool previousExportSetting = EditorUserBuildSettings.exportAsGoogleAndroidProject;
            bool previousBundleSetting = EditorUserBuildSettings.buildAppBundle;
            try
            {
                EditorUserBuildSettings.exportAsGoogleAndroidProject = true;
                EditorUserBuildSettings.buildAppBundle = false;
                Build(AndroidOutput, BuildTarget.Android, BuildOptions.Development);
                ConfigureAndroidStudioJdk();
            }
            finally
            {
                EditorUserBuildSettings.exportAsGoogleAndroidProject = previousExportSetting;
                EditorUserBuildSettings.buildAppBundle = previousBundleSetting;
            }
        }

        [MenuItem("Realm Raiders/Build/Export Xcode Project")]
        public static void ExportXcodeProject()
        {
            ExportXcode(IosOutput, iOSSdkVersion.DeviceSDK);
        }

        [MenuItem("Realm Raiders/Build/Export Xcode Simulator Project")]
        public static void ExportXcodeSimulatorProject()
        {
            ExportXcode(IosSimulatorOutput, iOSSdkVersion.SimulatorSDK);
        }

        [MenuItem("Realm Raiders/Build/Reveal Android Studio Export")]
        public static void RevealAndroidExport() => Reveal(AndroidOutput);

        [MenuItem("Realm Raiders/Build/Reveal Xcode Export")]
        public static void RevealXcodeExport() => Reveal(IosOutput);

        [MenuItem("Realm Raiders/Build/Reveal Xcode Simulator Export")]
        public static void RevealXcodeSimulatorExport() => Reveal(IosSimulatorOutput);

        static void ExportXcode(string output, iOSSdkVersion sdkVersion)
        {
            EnsurePlatformSupport(BuildTargetGroup.iOS, BuildTarget.iOS, "iOS Build Support");
            SwitchPlatform(BuildTargetGroup.iOS, BuildTarget.iOS);

            iOSSdkVersion previousSdk = PlayerSettings.iOS.sdkVersion;
            try
            {
                PlayerSettings.iOS.sdkVersion = sdkVersion;
                Build(output, BuildTarget.iOS, BuildOptions.Development | BuildOptions.SymlinkSources);
            }
            finally
            {
                PlayerSettings.iOS.sdkVersion = previousSdk;
            }
        }

        static void EnsurePlatformSupport(BuildTargetGroup group, BuildTarget target, string moduleName)
        {
            if (BuildPipeline.IsBuildTargetSupported(group, target)) return;
            throw new BuildFailedException($"{moduleName} is not installed for this Unity Editor. Add it in Unity Hub, then run this export again.");
        }

        static void SwitchPlatform(BuildTargetGroup group, BuildTarget target)
        {
            if (EditorUserBuildSettings.activeBuildTarget == target) return;
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(group, target))
                throw new BuildFailedException($"Unity could not switch the active build target to {target}.");
        }

        static void Build(string relativeOutput, BuildTarget target, BuildOptions options)
        {
            string output = Path.GetFullPath(relativeOutput);
            Directory.CreateDirectory(output);
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = output,
                target = target,
                options = options
            });

            if (report.summary.result != BuildResult.Succeeded)
                throw new BuildFailedException($"{target} export failed with {report.summary.totalErrors} error(s). See the Unity Console for details.");

            Debug.Log($"Realm Raiders {target} export completed: {output}");
            if (!Application.isBatchMode) EditorUtility.RevealInFinder(output);
        }

        static void Reveal(string relativeOutput)
        {
            string output = Path.GetFullPath(relativeOutput);
            if (!Directory.Exists(output))
                throw new DirectoryNotFoundException($"No export exists at {output}. Run the matching export command first.");
            EditorUtility.RevealInFinder(output);
        }

        static void ConfigureAndroidStudioJdk()
        {
            string editorRoot = Path.GetFullPath(Path.Combine(EditorApplication.applicationContentsPath, "..", ".."));
            string jdkPath = Path.Combine(editorRoot, "PlaybackEngines", "AndroidPlayer", "OpenJDK");
            if (!Directory.Exists(jdkPath))
                throw new BuildFailedException($"Unity OpenJDK was not found at {jdkPath}.");

            string output = Path.GetFullPath(AndroidOutput);
            string gradleProperties = Path.Combine(output, "gradle.properties");
            SetGradleProperty(gradleProperties, "org.gradle.java.home", jdkPath);

            string localGradleDirectory = Path.Combine(output, ".gradle");
            Directory.CreateDirectory(localGradleDirectory);
            File.WriteAllText(Path.Combine(localGradleDirectory, "config.properties"), $"java.home={jdkPath}{Environment.NewLine}");
        }

        static void SetGradleProperty(string filePath, string key, string value)
        {
            string prefix = key + "=";
            string[] lines = File.Exists(filePath) ? File.ReadAllLines(filePath) : Array.Empty<string>();
            bool replaced = false;
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].StartsWith(prefix, StringComparison.Ordinal)) continue;
                lines[i] = prefix + value;
                replaced = true;
            }

            if (!replaced)
            {
                Array.Resize(ref lines, lines.Length + 1);
                lines[lines.Length - 1] = prefix + value;
            }

            File.WriteAllLines(filePath, lines);
        }
    }
}
