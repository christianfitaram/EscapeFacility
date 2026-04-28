using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using StarterAssets;

public static class MixamoCharacterSetup
{
    private const string MixamoFolder = "Assets/Characters/Mixamo";
    private const string MixamoMaterialsFolder = "Assets/Characters/Mixamo/GeneratedMaterials";
    private const string BasePlayerPrefabPath = "Assets/StarterAssets/ThirdPersonController/Prefabs/PlayerArmature.prefab";
    private const string PrimarySourceModelPath = "Assets/Characters/Mixamo/character.fbx";
    private const string ExternalPreferredSourceModelPath = "Assets/Characters/Mixamo/Swat.fbx";
    private const string ExternalFallbackSourceModelPath = "Assets/Characters/Mixamo/character_external.fbx";
    private const string PrimaryOutputPlayerPrefabPath = "Assets/Characters/Mixamo/PlayerArmature_Mixamo.prefab";
    private const string ExternalOutputPlayerPrefabPath = "Assets/Characters/Mixamo/PlayerArmature_Mixamo_External.prefab";

    [MenuItem("Tools/Escape Facility/Setup Mixamo Character")]
    public static void SetupMixamoCharacter()
    {
        SetupCharacterVariant(PrimarySourceModelPath, PrimaryOutputPlayerPrefabPath, "MixamoCharacter");
    }

    [MenuItem("Tools/Escape Facility/Setup Mixamo Character (External)")]
    public static void SetupExternalMixamoCharacter()
    {
        string externalSourceModelPath = GetExternalSourceModelPath();
        if (string.IsNullOrEmpty(externalSourceModelPath))
        {
            Debug.LogError($"No external Mixamo source model found. Expected {ExternalPreferredSourceModelPath} or {ExternalFallbackSourceModelPath}.");
            return;
        }

        SetupCharacterVariant(externalSourceModelPath, ExternalOutputPlayerPrefabPath, "MixamoCharacterExternal");
    }

    private static void SetupCharacterVariant(string sourceModelPath, string outputPlayerPrefabPath, string childName)
    {
        EnsureFolder("Assets", "Characters");
        EnsureFolder("Assets/Characters", "Mixamo");
        EnsureFolder("Assets/Characters/Mixamo", "GeneratedMaterials");

        if (!File.Exists(sourceModelPath))
        {
            Debug.LogError($"Mixamo source model not found at {sourceModelPath}.");
            return;
        }

        ConfigureModelImporter(sourceModelPath);

        GameObject basePlayerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(BasePlayerPrefabPath);
        GameObject mixamoModelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(sourceModelPath);
        Avatar mixamoAvatar = LoadMainAvatar(sourceModelPath);

        if (basePlayerPrefab == null || mixamoModelPrefab == null || mixamoAvatar == null)
        {
            Debug.LogError("Mixamo setup failed because the base player, imported model, or generated avatar could not be loaded.");
            return;
        }

        GameObject playerRoot = PrefabUtility.LoadPrefabContents(BasePlayerPrefabPath);

        try
        {
            DisableExistingVisuals(playerRoot);
            DestroyExistingMixamoCharacter(playerRoot, childName);

            GameObject modelInstance = Object.Instantiate(mixamoModelPrefab);
            modelInstance.name = childName;
            modelInstance.transform.SetParent(playerRoot.transform, false);
            modelInstance.transform.localPosition = Vector3.zero;
            modelInstance.transform.localRotation = Quaternion.identity;
            modelInstance.transform.localScale = Vector3.one;

            foreach (Animator childAnimator in modelInstance.GetComponentsInChildren<Animator>(true))
            {
                Object.DestroyImmediate(childAnimator);
            }

            ApplyCatalogMaterials(modelInstance, outputPlayerPrefabPath);

            Animator rootAnimator = playerRoot.GetComponent<Animator>();
            if (rootAnimator == null)
            {
                Debug.LogError("Base player prefab is missing its Animator component.");
                return;
            }

            rootAnimator.avatar = mixamoAvatar;
            rootAnimator.applyRootMotion = false;

            ThirdPersonController thirdPersonController = playerRoot.GetComponent<ThirdPersonController>();
            if (thirdPersonController != null)
            {
                thirdPersonController.UseReadabilityLights = false;
            }

            PrefabUtility.SaveAsPrefabAsset(playerRoot, outputPlayerPrefabPath);
        }
        finally
        {
            PrefabUtility.UnloadPrefabContents(playerRoot);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log($"Mixamo player prefab generated at {outputPlayerPrefabPath}.");
    }

    private static void ApplyCatalogMaterials(GameObject modelInstance, string outputPlayerPrefabPath)
    {
        string variantName = Path.GetFileNameWithoutExtension(outputPlayerPrefabPath);
        bool hasUsableTextureData = ModelHasUsableTextureData(modelInstance);

        if (!hasUsableTextureData && UsesSwatMaterialLayout(modelInstance))
        {
            ApplySwatCatalogFallback(modelInstance, variantName);
            return;
        }

        foreach (SkinnedMeshRenderer renderer in modelInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            string rendererName = SanitizeAssetName(renderer.gameObject.name);
            Material[] preservedMaterials = new Material[renderer.sharedMaterials.Length];

            for (int materialIndex = 0; materialIndex < preservedMaterials.Length; materialIndex++)
            {
                Material sourceMaterial = renderer.sharedMaterials[materialIndex];
                if (sourceMaterial == null)
                {
                    preservedMaterials[materialIndex] = null;
                    continue;
                }

                string materialAssetPath = $"{MixamoMaterialsFolder}/{variantName}_{rendererName}_{materialIndex}_{SanitizeAssetName(sourceMaterial.name)}.mat";
                preservedMaterials[materialIndex] = GetOrCreateMaterialCopy(materialAssetPath, sourceMaterial);
            }

            renderer.sharedMaterials = preservedMaterials;
        }
    }

    private static bool ModelHasUsableTextureData(GameObject modelInstance)
    {
        foreach (SkinnedMeshRenderer renderer in modelInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            foreach (Material material in renderer.sharedMaterials)
            {
                if (material == null)
                {
                    continue;
                }

                if (HasTexture(material, "_BaseMap") ||
                    HasTexture(material, "_MainTex") ||
                    HasTexture(material, "_EmissionMap") ||
                    HasTexture(material, "_BumpMap") ||
                    HasTexture(material, "_MetallicGlossMap"))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private static bool HasTexture(Material material, string propertyName)
    {
        return material.HasProperty(propertyName) && material.GetTexture(propertyName) != null;
    }

    private static bool UsesSwatMaterialLayout(GameObject modelInstance)
    {
        return modelInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true)
            .Any(renderer => renderer.gameObject.name.StartsWith("Soldier_", System.StringComparison.OrdinalIgnoreCase));
    }

    private static void ApplySwatCatalogFallback(GameObject modelInstance, string variantName)
    {
        Material bodyMaterial = GetOrCreateLitMaterial(
            $"{MixamoMaterialsFolder}/{variantName}_Soldier_body_0_Soldier_body1.mat",
            new Color(0.26f, 0.33f, 0.42f),
            0.08f,
            0f);

        Material helmetMaterial = GetOrCreateLitMaterial(
            $"{MixamoMaterialsFolder}/{variantName}_Soldier_head_0_Soldier_body1.mat",
            new Color(0.76f, 0.73f, 0.68f),
            0.18f,
            0f);

        Material faceMaterial = GetOrCreateLitMaterial(
            $"{MixamoMaterialsFolder}/{variantName}_Soldier_head_1_Soldier_head6.mat",
            new Color(0.58f, 0.48f, 0.41f),
            0.05f,
            0f);

        foreach (SkinnedMeshRenderer renderer in modelInstance.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            string rendererName = renderer.gameObject.name;

            if (rendererName.Equals("Soldier_body", System.StringComparison.OrdinalIgnoreCase))
            {
                renderer.sharedMaterials = new[] { bodyMaterial };
                continue;
            }

            if (rendererName.Equals("Soldier_head", System.StringComparison.OrdinalIgnoreCase))
            {
                Material[] headMaterials = new Material[renderer.sharedMaterials.Length];
                for (int materialIndex = 0; materialIndex < headMaterials.Length; materialIndex++)
                {
                    headMaterials[materialIndex] = materialIndex == 0 ? helmetMaterial : faceMaterial;
                }

                renderer.sharedMaterials = headMaterials;
            }
        }
    }

    private static void ConfigureModelImporter(string assetPath)
    {
        ModelImporter importer = AssetImporter.GetAtPath(assetPath) as ModelImporter;
        if (importer == null)
        {
            return;
        }

        bool needsReimport = false;

        if (importer.animationType != ModelImporterAnimationType.Human)
        {
            importer.animationType = ModelImporterAnimationType.Human;
            needsReimport = true;
        }

        if (importer.avatarSetup != ModelImporterAvatarSetup.CreateFromThisModel)
        {
            importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
            needsReimport = true;
        }

        if (importer.optimizeGameObjects)
        {
            importer.optimizeGameObjects = false;
            needsReimport = true;
        }

        if (needsReimport)
        {
            importer.SaveAndReimport();
        }
    }

    private static Avatar LoadMainAvatar(string assetPath)
    {
        return AssetDatabase.LoadAllAssetsAtPath(assetPath)
            .OfType<Avatar>()
            .FirstOrDefault(avatar => avatar.isHuman && avatar.isValid);
    }

    private static Material GetOrCreateMaterialCopy(string assetPath, Material sourceMaterial)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);

        if (material == null)
        {
            material = new Material(sourceMaterial);
            AssetDatabase.CreateAsset(material, assetPath);
        }

        material.shader = sourceMaterial.shader;
        material.CopyPropertiesFromMaterial(sourceMaterial);
        material.shaderKeywords = sourceMaterial.shaderKeywords;
        material.renderQueue = sourceMaterial.renderQueue;
        material.globalIlluminationFlags = sourceMaterial.globalIlluminationFlags;
        material.doubleSidedGI = sourceMaterial.doubleSidedGI;
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material GetOrCreateLitMaterial(string assetPath, Color baseColor, float smoothness, float metallic)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        if (shader == null)
        {
            return material;
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, assetPath);
        }

        material.shader = shader;
        material.SetColor("_BaseColor", baseColor);
        material.SetColor("_Color", baseColor);

        if (material.HasProperty("_Smoothness"))
        {
            material.SetFloat("_Smoothness", smoothness);
        }

        if (material.HasProperty("_Metallic"))
        {
            material.SetFloat("_Metallic", metallic);
        }

        if (material.HasProperty("_SpecColor"))
        {
            material.SetColor("_SpecColor", Color.Lerp(baseColor, Color.white, 0.18f));
        }

        material.SetColor("_EmissionColor", Color.black);
        material.DisableKeyword("_EMISSION");
        EditorUtility.SetDirty(material);
        return material;
    }

    private static string SanitizeAssetName(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "Unnamed";
        }

        return new string(value.Select(character => char.IsLetterOrDigit(character) ? character : '_').ToArray());
    }

    private static void DisableExistingVisuals(GameObject playerRoot)
    {
        foreach (SkinnedMeshRenderer renderer in playerRoot.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            renderer.gameObject.SetActive(false);
        }
    }

    private static void DestroyExistingMixamoCharacter(GameObject playerRoot, string childName)
    {
        string[] possibleChildren = { "MixamoCharacter", "MixamoCharacterExternal", childName };
        foreach (string possibleChild in possibleChildren)
        {
            Transform existingMixamoCharacter = playerRoot.transform.Find(possibleChild);
            if (existingMixamoCharacter != null)
            {
                Object.DestroyImmediate(existingMixamoCharacter.gameObject);
            }
        }
    }

    private static void EnsureFolder(string parentPath, string childName)
    {
        string folderPath = $"{parentPath}/{childName}";
        if (!AssetDatabase.IsValidFolder(folderPath))
        {
            AssetDatabase.CreateFolder(parentPath, childName);
        }
    }

    public static string GetExternalSourceModelPath()
    {
        if (File.Exists(ExternalPreferredSourceModelPath))
        {
            return ExternalPreferredSourceModelPath;
        }

        if (File.Exists(ExternalFallbackSourceModelPath))
        {
            return ExternalFallbackSourceModelPath;
        }

        return null;
    }
}
