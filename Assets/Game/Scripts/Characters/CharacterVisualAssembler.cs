using System.Collections.Generic;
using UnityEngine;

namespace RealmRaiders.Characters
{
    /// <summary>Owns visual-only children; never adds gameplay colliders, movement, health or controllers.</summary>
    [DisallowMultipleComponent]
    public sealed class CharacterVisualAssembler : MonoBehaviour
    {
        static readonly Dictionary<Color, Material> materials = new();
        public CharacterVisualRecipe Recipe { get; private set; }
        Transform visualRoot;
        Transform presentationPivot;

        public Transform VisualRoot => visualRoot;
        public Transform PresentationPivot => presentationPivot;

        public bool Assemble(CharacterVisualRecipe recipe)
        {
            Clear(); Recipe = recipe;
            if (!recipe || !recipe.IsValid) return false;
            var baseRenderer = GetComponent<Renderer>(); if (baseRenderer) baseRenderer.enabled = false;
            visualRoot = new GameObject("Character Visual Modules").transform; visualRoot.SetParent(transform, false);
            presentationPivot = new GameObject("Presentation Pivot").transform; presentationPivot.SetParent(visualRoot, false);
            var motion = GetComponent<CharacterVisualMotion>() ?? gameObject.AddComponent<CharacterVisualMotion>(); motion.Bind(presentationPivot);
            BuildBase(recipe); BuildSlot("Head", recipe.Head, recipe.HeadPrefab, new Vector3(0, BodyHeight(recipe) * .55f, 0), recipe.AccentColor);
            BuildSlot("Back", recipe.Back, recipe.BackPrefab, new Vector3(0, .35f, -.28f), recipe.Secondary);
            BuildSlot("Arms", recipe.Arms, recipe.ArmsPrefab, new Vector3(0, .05f, .1f), recipe.Secondary);
            BuildSlot("Accent", recipe.Accent, recipe.AccentPrefab, new Vector3(0, .1f, .38f), recipe.AccentColor);
            return true;
        }

        public void Clear()
        {
            GetComponent<CharacterVisualMotion>()?.Bind(null);
            if (visualRoot) { if (Application.isPlaying) Destroy(visualRoot.gameObject); else DestroyImmediate(visualRoot.gameObject); } visualRoot = null; presentationPivot = null; Recipe = null;
            var baseRenderer = GetComponent<Renderer>(); if (baseRenderer) baseRenderer.enabled = true;
        }

        void BuildBase(CharacterVisualRecipe recipe)
        {
            if (recipe.BaseBodyPrefab) { AddPrefab("Base Body", recipe.BaseBodyPrefab, Vector3.zero); return; }
            var type = recipe.Family == CharacterVisualFamily.Humanoid ? PrimitiveType.Capsule : recipe.Family == CharacterVisualFamily.LargeCreature ? PrimitiveType.Cube : PrimitiveType.Sphere;
            var scale = recipe.Family == CharacterVisualFamily.Humanoid ? new Vector3(.75f, 1.25f, .55f) : recipe.Family == CharacterVisualFamily.LargeCreature ? new Vector3(1.45f, 1.25f, .85f) : new Vector3(1.15f, .7f, .75f);
            AddPrimitive("Base Body", type, Vector3.zero, scale, recipe.Primary);
        }
        float BodyHeight(CharacterVisualRecipe recipe) => recipe.Family == CharacterVisualFamily.LargeCreature ? 1.35f : recipe.Family == CharacterVisualFamily.Humanoid ? 1.2f : .65f;
        void BuildSlot(string slot, VisualModuleStyle style, GameObject prefab, Vector3 position, Color color)
        {
            if (prefab) { AddPrefab(slot, prefab, position); return; }
            if (style == VisualModuleStyle.None) return;
            var type = style is VisualModuleStyle.Blade or VisualModuleStyle.Spikes ? PrimitiveType.Cube : PrimitiveType.Sphere;
            var scale = style is VisualModuleStyle.Blade or VisualModuleStyle.Claws ? new Vector3(.18f, .7f, .18f) : style == VisualModuleStyle.ShoulderPads ? new Vector3(1.3f, .28f, .5f) : new Vector3(.42f, .42f, .42f);
            if (slot == "Arms") { AddPrimitive(slot + " Left", type, position + Vector3.left * .7f, scale, color); AddPrimitive(slot + " Right", type, position + Vector3.right * .7f, scale, color); }
            else AddPrimitive(slot, type, position, scale, color);
        }
        void AddPrefab(string name, GameObject prefab, Vector3 position)
        { var item = Instantiate(prefab, presentationPivot); item.name = name; item.transform.localPosition = position; DisableColliders(item); }
        void AddPrimitive(string name, PrimitiveType type, Vector3 position, Vector3 scale, Color color)
        {
            var item = GameObject.CreatePrimitive(type); item.name = name; item.transform.SetParent(presentationPivot, false); item.transform.localPosition = position; item.transform.localScale = scale;
            var collider = item.GetComponent<Collider>(); if (collider) collider.enabled = false;
            item.GetComponent<Renderer>().sharedMaterial = Material(color);
        }
        static void DisableColliders(GameObject item) { foreach (var collider in item.GetComponentsInChildren<Collider>(true)) collider.enabled = false; }
        static Material Material(Color color)
        {
            if (materials.TryGetValue(color, out var material) && material) return material;
            material = new Material(Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard")); material.name = "Character Visual Shared"; material.color = color; materials[color] = material; return material;
        }
        void OnDestroy() => Clear();
    }
}
