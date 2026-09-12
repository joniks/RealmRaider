using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using NUnit.Framework;
using RealmRaiders.Characters;
using RealmRaiders.Core;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace RealmRaiders.Tests
{
    public sealed class GuardianEntTree01IntakeTests
    {
        const string Root = "Assets/Game/Art/ThirdParty/Tennessippi/FreeTreantPack";
        const string ModelPath = Root + "/Tree01/source/Tree01_FBX.fbx";
        const string AlbedoPath = Root + "/Tree01/textures/Tree01 Albedo Light.png";
        const string MaterialPath = Root + "/Tree01/materials/GuardianEntTree01Mobile.mat";
        const string PrefabPath = "Assets/Game/Resources/Characters/GuardianEntTree01.prefab";

        [TestCase("source/Tree01_FBX.fbx", 1937020L, "bd90b4f8dd823334cbb226229ef731c1280ae601ab942d16193b96f5f674a02e")]
        [TestCase("textures/Tree01 Albedo Light.png", 3991821L, "cd50e1a9179f1242a862f6fb7f8448cff5cbbd56d599e687d916dc8583625573")]
        [TestCase("textures/Treant_1_LP_DefaultMaterial_Normal.png", 3387442L, "1b0372d5b2797d520c33cfa2839b46f4536f2ce15104e23f5c50cb4a1b503d2f")]
        [TestCase("textures/Treant_1_LP_DefaultMaterial_MaskMap.png", 3694246L, "72b6fe38c9ca4f1b6a7f6f457a73b4900a7897eb3129a4079472eec5c48e093c")]
        public void Tree01_ExactSelectedSourceAndProvenance(string member, long bytes, string expectedHash)
        {
            using var input = File.OpenRead(Root + "/Tree01/" + member);
            Assert.That(input.Length, Is.EqualTo(bytes));
            using var sha = SHA256.Create();
            Assert.That(BitConverter.ToString(sha.ComputeHash(input)).Replace("-", "").ToLowerInvariant(), Is.EqualTo(expectedHash));
            var provenance = File.ReadAllText(Root + "/LICENSE_PROVENANCE.txt");
            Assert.That(provenance, Does.Contain(expectedHash).And.Contain(member));
            Assert.That(provenance, Does.Contain("https://tennessippistudios.itch.io/treant-pack").And
                .Contain("https://creativecommons.org/publicdomain/zero/1.0/").And.Contain("NO embedded licence/readme"));
            Assert.That(provenance, Does.Contain("1fcddcbd1fbbc8ec84f6001b2192d22794a927c9be2a929457a537e908862cd8")
                .And.Contain("5,438").And.Contain("373").And.Contain("PILOT ONLY"));
        }

        [Test]
        public void Tree01_UsesAcceptedStaticMobileImportSettings()
        {
            var model = AssetImporter.GetAtPath(ModelPath) as ModelImporter;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.globalScale, Is.EqualTo(.7452205f).Within(.0000001f));
            Assert.That(model.useFileScale && model.useFileUnits && model.bakeAxisConversion, Is.True);
            Assert.That(model.meshCompression, Is.EqualTo(ModelImporterMeshCompression.Medium));
            Assert.That(model.isReadable || model.addCollider || model.importCameras || model.importLights ||
                model.importVisibility || model.importBlendShapes || model.importAnimation, Is.False);
            Assert.That(model.animationType, Is.EqualTo(ModelImporterAnimationType.None));
            Assert.That(model.weldVertices && model.optimizeMeshPolygons && model.optimizeMeshVertices, Is.True);
            Assert.That(model.importNormals, Is.EqualTo(ModelImporterNormals.Import));
            Assert.That(model.importTangents, Is.EqualTo(ModelImporterTangents.None));
            Assert.That(model.skinWeights, Is.EqualTo(ModelImporterSkinWeights.Custom));
            Assert.That(model.maxBonesPerVertex, Is.EqualTo(4));
            Assert.That(model.materialImportMode, Is.EqualTo(ModelImporterMaterialImportMode.None));
            Assert.That(AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<AnimationClip>(), Is.Empty);
            Assert.That(AssetDatabase.LoadAllAssetsAtPath(ModelPath).OfType<Material>(), Is.Empty);
        }

        [TestCase("Tree01 Albedo Light.png", TextureImporterType.Default, true, TextureImporterAlphaSource.None)]
        [TestCase("Treant_1_LP_DefaultMaterial_Normal.png", TextureImporterType.NormalMap, false, TextureImporterAlphaSource.None)]
        [TestCase("Treant_1_LP_DefaultMaterial_MaskMap.png", TextureImporterType.Default, false, TextureImporterAlphaSource.FromInput)]
        public void Tree01_TexturesUseExplicitMobileAndColorSpaceSettings(string name, TextureImporterType type, bool srgb, TextureImporterAlphaSource alpha)
        {
            var importer = AssetImporter.GetAtPath(Root + "/Tree01/textures/" + name) as TextureImporter;
            Assert.That(importer, Is.Not.Null);
            Assert.That(importer.textureType, Is.EqualTo(type));
            Assert.That(importer.sRGBTexture, Is.EqualTo(srgb));
            Assert.That(importer.alphaSource, Is.EqualTo(alpha));
            Assert.That(importer.isReadable || importer.flipGreenChannel, Is.False);
            Assert.That(importer.mipmapEnabled, Is.True);
            Assert.That(importer.maxTextureSize, Is.EqualTo(1024));
            Assert.That(importer.filterMode, Is.EqualTo(FilterMode.Bilinear));
            Assert.That(importer.anisoLevel, Is.EqualTo(1));
            var android = importer.GetPlatformTextureSettings("Android");
            Assert.That(android.overridden, Is.True);
            Assert.That(android.maxTextureSize, Is.EqualTo(1024));
            Assert.That(android.format, Is.EqualTo(TextureImporterFormat.ASTC_6x6));
        }

        [Test]
        public void Tree01_ProjectPrefabHasOnlyOwnedVisualHierarchyAndOneAlbedoMaterial()
        {
            var prefab = Resources.Load<GameObject>("Characters/GuardianEntTree01");
            Assert.That(prefab, Is.Not.Null, "QA must run Realm Raiders > Art Intake > Build Guardian Ent Tree01 Pilot before this gate.");
            Assert.That(AssetDatabase.GetAssetPath(prefab), Is.EqualTo(PrefabPath));
            Assert.That(prefab.transform.localPosition, Is.EqualTo(Vector3.zero));
            Assert.That(prefab.transform.localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(prefab.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(prefab.transform.childCount, Is.EqualTo(1));
            var fit = prefab.transform.Find("Tree01 Fit");
            Assert.That(fit, Is.Not.Null);
            Assert.That(fit.localPosition, Is.EqualTo(new Vector3(0, .004083f, 0)));
            Assert.That(fit.localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(fit.localScale, Is.EqualTo(Vector3.one));
            Assert.That(fit.childCount, Is.EqualTo(1));
            foreach (var component in prefab.GetComponentsInChildren<Component>(true))
                Assert.That(component && (component is Transform || component is MeshFilter || component is MeshRenderer), Is.True,
                    "Tree01 must contain no missing scripts, SkinnedMeshRenderer, Animator, Collider, Rigidbody, camera, lights or gameplay components.");
            var renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            var filters = prefab.GetComponentsInChildren<MeshFilter>(true);
            Assert.That(renderers, Has.Length.EqualTo(1));
            Assert.That(filters, Has.Length.EqualTo(1));
            var renderer = renderers[0];
            Assert.That(renderer.gameObject, Is.SameAs(filters[0].gameObject));
            Assert.That(renderer.transform.IsChildOf(fit), Is.True);
            var mesh = filters[0].sharedMesh;
            Assert.That(mesh, Is.Not.Null);
            Assert.That(AssetDatabase.GetAssetPath(mesh), Is.EqualTo(ModelPath));
            Assert.That(mesh.subMeshCount, Is.EqualTo(1));
            Assert.That(mesh.GetTopology(0), Is.EqualTo(MeshTopology.Triangles));
            Assert.That(mesh.GetIndexCount(0), Is.EqualTo(5438 * 3));
            Assert.That(renderer.sharedMaterials, Has.Length.EqualTo(1));
            var material = renderer.sharedMaterial;
            Assert.That(AssetDatabase.GetAssetPath(material), Is.EqualTo(MaterialPath));
            Assert.That(material.shader.name, Is.EqualTo("Universal Render Pipeline/Simple Lit"));
            Assert.That(material.GetColor("_BaseColor"), Is.EqualTo(Color.white));
            Assert.That(material.GetFloat("_Surface"), Is.Zero);
            Assert.That(material.GetColor("_EmissionColor"), Is.EqualTo(Color.black));
            Assert.That(material.GetTexture("_BaseMap"), Is.EqualTo(AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoPath)));
            Assert.That(material.GetTexture("_MainTex"), Is.EqualTo(AssetDatabase.LoadAssetAtPath<Texture2D>(AlbedoPath)));
            foreach (var property in material.GetTexturePropertyNames())
                if (property != "_BaseMap" && property != "_MainTex") Assert.That(material.GetTexture(property), Is.Null, "Unbound provenance map: " + property);
            var dependencies = AssetDatabase.GetDependencies(PrefabPath);
            Assert.That(dependencies, Does.Contain(ModelPath).And.Contain(AlbedoPath));
            Assert.That(dependencies.Any(path => path.EndsWith("_Normal.png") || path.EndsWith("_MaskMap.png")), Is.False);
        }

        [Test]
        public void Tree01_RecipeCachesResourceAndAssemblesWithoutPrimitiveOverlays()
        {
            var recipe = PrototypeRuntimeFactory.GuardianEntRecipe;
            Assert.That(recipe, Is.SameAs(PrototypeRuntimeFactory.GuardianEntRecipe));
            Assert.That(recipe.BaseBodyPrefab, Is.Not.Null);
            Assert.That(recipe.BaseBodyPrefab, Is.SameAs(Resources.Load<GameObject>("Characters/GuardianEntTree01")));
            Assert.That(recipe.Family, Is.EqualTo(CharacterVisualFamily.LargeCreature));
            Assert.That(recipe.BaseBodyLocalEulerAngles, Is.EqualTo(new Vector3(0, 180, 0)));
            Assert.That(recipe.AlignBaseBodyToControllerSupportPlane, Is.True);
            Assert.That(recipe.BaseBodyGroundingAnchorNames, Is.EqualTo(new[] { "heel.02.L", "heel.02.R", "toe.L", "toe.R" }));
            Assert.That(new[] { recipe.Head, recipe.Back, recipe.Arms, recipe.Accent }, Is.All.EqualTo(VisualModuleStyle.None));
            var host = GameObject.CreatePrimitive(PrimitiveType.Cube);
            host.transform.localScale = Vector3.one * 1.4f;
            var assembler = host.AddComponent<CharacterVisualAssembler>();
            try
            {
                Assert.That(assembler.Assemble(recipe), Is.True);
                Assert.That(assembler.PresentationPivot.childCount, Is.EqualTo(1));
                Assert.That(assembler.PresentationPivot.Find("Base Body/Tree01 Fit"), Is.Not.Null);
                Assert.That(assembler.VisualRoot.GetComponentsInChildren<Collider>(true), Is.Empty);
                Assert.That(host.GetComponent<CharacterProceduralMotionAdapter>().IsBound, Is.False,
                    "Tree01 must not opt into the exact Bip01 Blood Knight adapter.");
                Assert.That(host.transform.localScale, Is.EqualTo(Vector3.one * 1.4f));
                assembler.Clear();
                Assert.That(host.transform.Find("Character Visual Modules"), Is.Null);
                Assert.That(host.GetComponent<Renderer>().enabled, Is.True);
            }
            finally { Object.DestroyImmediate(host); }
        }

        [Test]
        public void Tree01_CachedFallbackPromotesWithoutReloadAndKeepsStableIdentity()
        {
            var prefab = Resources.Load<GameObject>("Characters/GuardianEntTree01");
            Assert.That(prefab, Is.Not.Null, "QA must build the accepted prefab before this gate.");
            var cache = typeof(PrototypeRuntimeFactory).GetField("guardianEntRecipe", BindingFlags.NonPublic | BindingFlags.Static);
            var create = typeof(PrototypeRuntimeFactory).GetMethod("CreateGuardianEntRecipe", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(cache, Is.Not.Null); Assert.That(create, Is.Not.Null);
            var previous = cache.GetValue(null);
            var fallback = (CharacterVisualRecipe)create.Invoke(null, new object[] { null });
            var host = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                // Simulate a getter that ran before the explicit QA builder in this domain.
                cache.SetValue(null, fallback);
                Assert.That(fallback.BaseBodyPrefab, Is.Null);
                Assert.That(fallback.Head, Is.EqualTo(VisualModuleStyle.Bark));
                var assembler = host.AddComponent<CharacterVisualAssembler>();
                Assert.That(assembler.Assemble(fallback), Is.True);
                var pivot = assembler.PresentationPivot;
                var body = pivot.Find("Base Body");
                Assert.That(pivot.childCount, Is.EqualTo(6));

                var promoted = PrototypeRuntimeFactory.GuardianEntRecipe;
                Assert.That(promoted, Is.SameAs(fallback), "Promotion must not allocate a replacement recipe.");
                Assert.That(promoted.BaseBodyPrefab, Is.SameAs(prefab));
                Assert.That(promoted.BaseBodyLocalEulerAngles, Is.EqualTo(new Vector3(0, 180, 0)));
                Assert.That(promoted.AlignBaseBodyToControllerSupportPlane, Is.True);
                Assert.That(promoted.BaseBodyGroundingAnchorNames, Is.EqualTo(new[] { "heel.02.L", "heel.02.R", "toe.L", "toe.R" }));
                Assert.That(new[] { promoted.Head, promoted.Back, promoted.Arms, promoted.Accent }, Is.All.EqualTo(VisualModuleStyle.None));
                Assert.That(PrototypeRuntimeFactory.GuardianEntRecipe, Is.SameAs(promoted));
                Assert.That(PrototypeRuntimeFactory.GuardianEntRecipe, Is.SameAs(promoted));
                Assert.That(promoted.BaseBodyPrefab, Is.SameAs(prefab));
                Assert.That(assembler.PresentationPivot, Is.SameAs(pivot));
                Assert.That(pivot.Find("Base Body"), Is.SameAs(body));
                Assert.That(pivot.childCount, Is.EqualTo(6), "Cache promotion must not reassemble existing visuals or change live gameplay.");
            }
            finally
            {
                cache.SetValue(null, previous);
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(fallback);
            }
        }

        [Test]
        public void Tree01_UnavailableResourceRetainsTheExactPrimitiveRecipeAndModules()
        {
            // Exercise the same recipe-construction branch as the cached production getter, without mutating its cache.
            var create = typeof(PrototypeRuntimeFactory).GetMethod("CreateGuardianEntRecipe", BindingFlags.NonPublic | BindingFlags.Static);
            Assert.That(create, Is.Not.Null);
            var recipe = (CharacterVisualRecipe)create.Invoke(null, new object[] { null });
            var host = GameObject.CreatePrimitive(PrimitiveType.Cube);
            try
            {
                Assert.That(recipe.BaseBodyPrefab, Is.Null);
                Assert.That(recipe.Family, Is.EqualTo(CharacterVisualFamily.LargeCreature));
                Assert.That(recipe.Primary, Is.EqualTo(new Color(.16f, .38f, .11f)));
                Assert.That(recipe.Secondary, Is.EqualTo(new Color(.25f, .16f, .07f)));
                Assert.That(recipe.AccentColor, Is.EqualTo(new Color(.65f, .95f, .28f)));
                Assert.That(new[] { recipe.Head, recipe.Back, recipe.Arms, recipe.Accent }, Is.EqualTo(new[] {
                    VisualModuleStyle.Bark, VisualModuleStyle.Bark, VisualModuleStyle.Claws, VisualModuleStyle.Mane }));
                var assembler = host.AddComponent<CharacterVisualAssembler>();
                Assert.That(assembler.Assemble(recipe), Is.True);
                var pivot = assembler.PresentationPivot;
                Assert.That(Enumerable.Range(0, pivot.childCount).Select(i => pivot.GetChild(i).name),
                    Is.EqualTo(new[] { "Base Body", "Head", "Back", "Arms Left", "Arms Right", "Accent" }));
                Assert.That(pivot.Find("Base Body").localScale, Is.EqualTo(new Vector3(1.45f, 1.25f, .85f)));
                foreach (var collider in pivot.GetComponentsInChildren<Collider>(true)) Assert.That(collider.enabled, Is.False);
                assembler.Clear();
                Assert.That(host.transform.Find("Character Visual Modules"), Is.Null);
                Assert.That(host.GetComponent<Renderer>().enabled, Is.True);
            }
            finally { Object.DestroyImmediate(host); Object.DestroyImmediate(recipe); }
        }
    }
}
