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
