using System;
using UnityEngine;

namespace RealmRaiders.Characters
{
    /// <summary>Explicit, Editor-validated skeleton-only pilot data. No controller or gameplay callbacks.</summary>
    public sealed class LargeCreatureMotionBinding : ScriptableObject
    {
        public const int CurrentValidationVersion = 1;
        public int ValidationVersion;
        public GameObject VisualPrefab;
        public AnimationClip Idle, Run, Attack, Death;
        public string AnimatorPath;
        public string SkeletonRootPath;
        public string[] AnimatedBonePaths;

        public bool IsValid => TryValidate(out _);

        /// <summary>Same fail-closed contract for runtime, builder and cold-asset tests; reports the first failing fact in fixed order.</summary>
        public bool TryValidate(out string issue)
        {
            if (ValidationVersion != CurrentValidationVersion) return Fail($"ValidationVersion={ValidationVersion}; expected {CurrentValidationVersion}", out issue);
            if (!VisualPrefab) return Fail("VisualPrefab missing", out issue);
            if (!TryValidateClip(Idle, out issue)) { issue = "Idle: " + issue; return false; }
            if (!TryValidateClip(Run, out issue)) { issue = "Run: " + issue; return false; }
            if (!TryValidateClip(Attack, out issue)) { issue = "Attack: " + issue; return false; }
            if (!TryValidateClip(Death, out issue)) { issue = "Death: " + issue; return false; }
            return TryValidateHierarchy(VisualPrefab.transform, out issue);
        }

        public static bool SafeClip(AnimationClip clip) => TryValidateClip(clip, out _);

        public static bool TryValidateClip(AnimationClip clip, out string issue)
        {
            if (!clip) return Fail("clip missing", out issue);
            if (clip.legacy) return Fail("clip.legacy=true", out issue);
            if (clip.humanMotion) return Fail("clip.humanMotion=true", out issue);
            if (clip.empty) return Fail("clip.empty=true", out issue);
            if (!(clip.length > 0)) return Fail("clip.length is not positive", out issue);
            if (!CharacterCombatPresentationTimeline.Finite(clip.length)) return Fail("clip.length is not finite", out issue);
            if (clip.events.Length != 0) return Fail($"clip.events.Length={clip.events.Length}; expected 0", out issue);
            if (clip.hasRootCurves) return Fail("clip.hasRootCurves=true", out issue);
            if (clip.hasMotionCurves) return Fail("clip.hasMotionCurves=true", out issue);
            if (clip.hasGenericRootTransform) return Fail("clip.hasGenericRootTransform=true", out issue);
            if (clip.hasMotionFloatCurves) return Fail("clip.hasMotionFloatCurves=true", out issue);
            issue = null;
            return true;
        }

        public bool ValidateHierarchy(Transform body) => TryValidateHierarchy(body, out _);

        public bool TryValidateHierarchy(Transform body, out string issue)
        {
            if (!body) return Fail("Hierarchy body missing", out issue);
            if (string.IsNullOrEmpty(AnimatorPath)) return Fail("AnimatorPath empty", out issue);
            if (string.IsNullOrEmpty(SkeletonRootPath)) return Fail("SkeletonRootPath empty", out issue);
            if (AnimatedBonePaths == null) return Fail("AnimatedBonePaths null", out issue);
            if (AnimatedBonePaths.Length == 0) return Fail("AnimatedBonePaths empty", out issue);
            var owner = body.Find(AnimatorPath);
            if (!owner) return Fail("AnimatorPath not found: " + AnimatorPath, out issue);
            var animator = owner ? owner.GetComponent<Animator>() : null;
            var skeleton = owner ? owner.Find(SkeletonRootPath) : null;
            if (!animator) return Fail("Animator missing at " + AnimatorPath, out issue);
            if (!skeleton) return Fail("SkeletonRootPath not found under Animator: " + SkeletonRootPath, out issue);
            if (skeleton == owner) return Fail("Skeleton root equals Animator owner", out issue);
            if (!animator.avatar) return Fail("Animator.avatar missing", out issue);
            if (!animator.avatar.isValid) return Fail("Animator.avatar.isValid=false", out issue);
            if (animator.avatar.isHuman) return Fail("Animator.avatar.isHuman=true", out issue);
            if (animator.applyRootMotion) return Fail("Animator.applyRootMotion=true", out issue);
            if (animator.runtimeAnimatorController) return Fail("Animator.runtimeAnimatorController present", out issue);
            // fireEvents is not persisted in prefab data; the adapter disables it before any playback.
            foreach (var component in body.GetComponentsInChildren<Component>(true))
            {
                if (!component) return Fail("Hierarchy contains missing component/script", out issue);
                if (!(component is Transform || component is SkinnedMeshRenderer || component is Animator))
                    return Fail("Forbidden component: " + component.GetType().FullName + " on " + component.name, out issue);
            }
            var renderers = body.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            if (renderers.Length != 1) return Fail($"SkinnedMeshRenderer count={renderers.Length}; expected 1", out issue);
            var animators = body.GetComponentsInChildren<Animator>(true);
            if (animators.Length != 1) return Fail($"Animator count={animators.Length}; expected 1", out issue);
            var skin = renderers[0];
            if (!skin.sharedMesh) return Fail("Skin.sharedMesh missing", out issue);
            if (!skin.rootBone) return Fail("Skin.rootBone missing", out issue);
            if (!skin.rootBone.IsChildOf(skeleton)) return Fail("Skin.rootBone outside owned skeleton: " + skin.rootBone.name, out issue);
            var skinBones = skin.bones;
            if (skinBones.Length == 0) return Fail("Skin.bones empty", out issue);
            var bindposeCount = skin.sharedMesh.bindposes.Length;
            if (bindposeCount != skinBones.Length) return Fail($"Skin bindposes={bindposeCount}; bones={skinBones.Length}", out issue);
            for (var i = 0; i < skinBones.Length; i++)
            {
                if (!skinBones[i]) return Fail($"Skin.bones[{i}] missing", out issue);
                if (!skinBones[i].IsChildOf(skeleton)) return Fail($"Skin.bones[{i}] outside owned skeleton: {skinBones[i].name}", out issue);
            }
            for (var i = 0; i < AnimatedBonePaths.Length; i++)
            {
                var path = AnimatedBonePaths[i];
                if (string.IsNullOrEmpty(path)) return Fail($"AnimatedBonePaths[{i}] empty", out issue);
                if (!path.StartsWith(SkeletonRootPath + "/", StringComparison.Ordinal)) return Fail($"AnimatedBonePaths[{i}] outside skeleton prefix: {path}", out issue);
                var bone = owner.Find(path);
                if (!bone) return Fail($"AnimatedBonePaths[{i}] not found: {path}", out issue);
                if (bone == skeleton) return Fail($"AnimatedBonePaths[{i}] targets protected skeleton root: {path}", out issue);
                if (!bone.IsChildOf(skeleton)) return Fail($"AnimatedBonePaths[{i}] outside owned skeleton: {path}", out issue);
            }
            issue = null;
            return true;
        }

        static bool Fail(string reason, out string issue) { issue = reason; return false; }
    }
}
