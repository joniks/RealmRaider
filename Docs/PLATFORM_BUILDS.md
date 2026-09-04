# Realm Raiders — Android Studio and Xcode

## Required local tools

- Unity 6000.6.0f1.
- Android Studio for the exported Gradle project.
- Xcode on macOS for the exported iOS project.
- Unity Android Build Support with Android SDK & NDK Tools and OpenJDK.
- Unity iOS Build Support.

Verified on 2026-09-04: Android Studio, Xcode, Android Build Support (including SDK/NDK and OpenJDK), and iOS Build Support are installed. Both native project exports completed successfully.

Current generated projects:

- `Builds/AndroidStudio`
- `Builds/Xcode/Unity-iPhone.xcodeproj`

## Export to Android Studio

1. Open the project in Unity.
2. Choose **Realm Raiders → Build → Export Android Studio Project**.
3. Wait for Unity to generate `Builds/AndroidStudio`.
4. Open Android Studio and choose **Open**.
5. Select the generated `Builds/AndroidStudio` directory.
6. Let Gradle sync finish, select an emulator or connected Android device, and run the launcher module.

The export is a development build and uses `com.realmraiders.prototype`. The generated directory is ignored by Git and can be regenerated from Unity.

The export script pins Gradle to Unity's bundled OpenJDK 17. This avoids Android Studio using its newer bundled JDK for Unity's native CMake/Prefab configuration.

Verified on 2026-09-04: `:launcher:assembleDebug` completed successfully for `arm64-v8a`, including CMake and IL2CPP. The generated debug APK is located at `Builds/AndroidStudio/launcher/build/outputs/apk/debug/launcher-debug.apk`.

## Export to Xcode

### Physical iPhone

1. Open the project in Unity.
2. Choose **Realm Raiders → Build → Export Xcode Project**.
3. Wait for Unity to generate `Builds/Xcode`.
4. Open `Builds/Xcode/Unity-iPhone.xcodeproj` in Xcode.
5. Select the Unity-iPhone target.
6. In **Signing & Capabilities**, choose your Apple Development team and allow Xcode to manage signing.
7. Select an iPhone simulator or connected device and press Run.

The project uses the bundle identifier `com.realmraiders.prototype` and an iOS 15.0 deployment target. A physical device requires an Apple development team and valid signing.

### iPhone Simulator

1. Choose **Realm Raiders → Build → Export Xcode Simulator Project**.
2. Open `Builds/XcodeSimulator/Unity-iPhone.xcodeproj`.
3. Select a named simulator such as **iPhone 16 Pro** instead of **Any iOS Device**.
4. Press Run. Simulator builds do not require a connected iPhone or an Apple Development Team.

The device and simulator projects are separate because Unity generates different native libraries for `iphoneos` and `iphonesimulator`.

## Build content

Both exports start at `PrototypeHub` and include:

1. Prototype Hub
2. Character Sandbox
3. Sylvan Raid
4. Sylvan Defender Test
5. Infernal Defense

## Command-line entry points

After platform support is installed, automation can invoke:

```text
RealmRaiders.Editor.PlatformBuild.ExportAndroidStudioProject
RealmRaiders.Editor.PlatformBuild.ExportXcodeProject
RealmRaiders.Editor.PlatformBuild.ExportXcodeSimulatorProject
```

Unity officially generates a Gradle project for Android Studio and an Xcode project for iOS; the native tools then build and deploy those exported projects.
