# Realm Raiders — Third-Party Assets

This register records the source, licence, and project-local changes for every third-party asset included in the repository.

## Quaternius — Animated Knight Pack

- **Project file:** `Assets/Game/Art/ThirdParty/Quaternius/AnimatedKnight/KnightCharacter.fbx`
- **Asset:** `KnightCharacter.fbx` from Animated Knight Pack
- **Creator:** Quaternius
- **Official source:** <https://quaternius.com/packs/knightcharacter.html>
- **Original public file folder:** <https://drive.google.com/drive/folders/1QVyfCJkq70mAwMIh1cGq1xfHp2LN5GmK>
- **Licence:** Creative Commons Zero 1.0 Universal (CC0), confirmed on the creator's official pack page.
- **Acquired:** 2026-09-05
- **Local modifications:** source geometry is unchanged. Unity imports the FBX as a Generic rig because its Humanoid avatar validation fails; mesh compression is Medium, Read/Write is disabled, and colliders, cameras, lights, and animation clips are not imported. `Assets/Game/Resources/Characters/BloodKnightHero.prefab` is a visual-only prefab that references this source.
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
