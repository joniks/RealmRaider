# Realm Raiders — Third-Party Assets

This register records the source, licence, and project-local changes for every third-party asset included in the repository.

## 3DRT.com — Fantasy Warrior

- **Project files:** `Assets/Game/Art/ThirdParty/3DRT/FantasyWarrior/3drt-fantasy-warrior.zip`, `source/warrior_animated-armed.fbx`, and `textures/warrior.jpg`.
- **Asset:** `3DRT - Fantasy Warrior`.
- **Creator:** 3DRT.com.
- **Official source:** <https://sketchfab.com/3d-models/3drt-fantasy-warrior-d39a0dee0f054c21b6751f7821aa7a8e>.
- **Licence:** Creative Commons Attribution 4.0 International (CC BY 4.0): <https://creativecommons.org/licenses/by/4.0/>. Commercial use and adaptation are permitted provided that the original creator is credited.
- **Acquired:** 2026-09-05, downloaded from the creator-published Sketchfab page.
- **Delivered source inspection:** original FBX (`warrior_animated-armed.fbx`, 11.24 MB) plus one 1024×1024 RGB JPEG texture (`warrior.jpg`, 574 KB). Sketchfab publishes approximately 2.5k triangles, 1.4k vertices, a rig, and one animation (`Take 001`). The FBX has no imported cameras or lights and imports without colliders.
- **Rig validation:** Unity Humanoid auto-mapping was attempted, but this FBX does not provide a generated human-bone mapping in Unity 6000.6. It is deliberately imported as **Generic**, retaining its clip while the project continues to use its non-Animator presentation-motion path. No Humanoid avatar, Animator Controller, retargeting, or gameplay animation graph is used.
- **`Take 001` animation proof (2026-09-06):** Unity reports one Generic clip, 33.367 seconds at 30 FPS (frames 0–1001). `Loop Time` and `Loop Pose` are both off, and its Motion section has no Root Motion Node. Preview sampling at the opening, early, middle, and late portions shows a long authored sequence with materially different poses rather than a compact looping idle; it is not coherently readable as an idle beat from the live Sylvan camera distance. The clip is therefore intentionally **not played**. No Animator, controller, clip override, root motion, or gameplay change was added; Blood Knight retains the existing bounded `CharacterVisualMotion` presentation layer.
- **Local modifications:** the original archive and extracted files are retained unchanged. Unity applies Medium mesh compression, disabled Read/Write, disabled camera/light import, and a 1024 px Android texture override. The model is used solely under the visual-only `CharacterVisualAssembler` presentation child; no gameplay, colliders, camera, light, AudioListener, Animator Controller, or animation graph is added.
- **Attribution:** `“3DRT - Fantasy Warrior” by 3DRT.com is licensed under CC BY 4.0.` This credit must appear in any in-game third-party notices screen and distributions that include the asset.

## Quaternius — Animated Knight Pack

- **Project file:** `Assets/Game/Art/ThirdParty/Quaternius/AnimatedKnight/KnightCharacter.fbx`
- **Asset:** `KnightCharacter.fbx` from Animated Knight Pack
- **Creator:** Quaternius
- **Official source:** <https://quaternius.com/packs/knightcharacter.html>
- **Original public file folder:** <https://drive.google.com/drive/folders/1QVyfCJkq70mAwMIh1cGq1xfHp2LN5GmK>
- **Licence:** Creative Commons Zero 1.0 Universal (CC0), confirmed on the creator's official pack page.
- **Acquired:** 2026-09-05
- **Local modifications:** source geometry is unchanged. Unity imports the FBX as a Generic rig because its Humanoid avatar validation fails; mesh compression is Medium, Read/Write is disabled, and colliders, cameras, lights, and animation clips are not imported. `Assets/Game/Resources/Characters/BloodKnightHero_QuaterniusFallback.prefab` is the retained visual-only fallback prefab that references this source.
- **Attribution:** not required by CC0. The project keeps this credit record as provenance.

## Kenney — UI Pack

- **Project files:** `Assets/Game/Resources/ThirdParty/Kenney/InterfacePolish/Sprites/`
- **Selected assets:** `button_rectangle_depth_gradient.png`, `icon_checkmark.png`, and `icon_cross.png`.
- **Creator:** Kenney
- **Official source:** <https://kenney.nl/assets/ui-pack>
- **Licence:** Creative Commons Zero 1.0 Universal (CC0), confirmed on the creator's official asset page.
- **Acquired:** 2026-09-05
- **Local modifications:** only a deliberately small selection is included; the source pixels are unchanged. The button is intended to be used as a tintable UGUI background, so established realm colours and accessibility contrast remain under game control.
- **Attribution:** not required by CC0. The project keeps this credit record as provenance.

## Kenney — Interface Sounds

- **Project files:** `Assets/Game/Resources/ThirdParty/Kenney/InterfacePolish/Audio/` and `Assets/Game/Resources/ThirdParty/Kenney/InterfacePolish/InterfaceSounds_LICENSE.txt`
- **Selected assets:** `click_001.ogg`, `select_004.ogg`, `confirmation_003.ogg`, `error_001.ogg`, and `bong_001.ogg`.
- **Creator:** Kenney
- **Official source:** <https://kenney.nl/assets/interface-sounds>
- **Licence:** Creative Commons Zero 1.0 Universal (CC0); the downloaded package licence is retained beside the files.
- **Acquired:** 2026-09-05
- **Local modifications:** only five short feedback cues are included; source audio is unchanged. The gameplay pass must keep their use sparse and respect the device's normal game-audio settings.
- **Attribution:** not required by CC0. The project keeps this credit record as provenance.
