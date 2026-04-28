using System;
using System.Collections.Generic;
using System.IO;
using Cinemachine;
using EscapeFacility;
using StarterAssets;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static partial class EscapeFacilitySceneBuilder
{
    private const string ScenesFolder = "Assets/Scenes";
    private const string GeneratedMaterialsFolder = "Assets/Materials/Generated";
    private const string GeneratedCompatibleMaterialsFolder = GeneratedMaterialsFolder + "/Imported";
    private const string GeneratedTiledMaterialsFolder = GeneratedMaterialsFolder + "/Tiled";
    private const string TerrainLayersFolder = "Assets/Terrain/Layers";
    private const string TerrainTexturesFolder = "Assets/Terrain/Textures";
    private const string FreeFireVfxMaterialsFolder = "Assets/Vefects/Free Fire VFX URP/Materials";
    private const string LunarRockMaterialPath = "Assets/Lunar Landscape 3D/Resources/Materials/Rocks.mat";
    private const string StylizedHotLavaMaterialPath = "Assets/Stylized Lava Materials/Lava03/M_Lava03.mat";
    private const string StylizedColdLavaMaterialPath = "Assets/Stylized Lava Materials/Lava04/M_Lava04.mat";
    private const string StylizedWallRockMaterialPath = "Assets/PolyOne/Rocks Stylized/Materials/Rocks Stylized_M.mat";
    private const string MixamoPrimarySourcePath = "Assets/Characters/Mixamo/character.fbx";
    private const string MixamoPrimaryPrefabPath = "Assets/Characters/Mixamo/PlayerArmature_Mixamo.prefab";
    private const string MixamoExternalPrefabPath = "Assets/Characters/Mixamo/PlayerArmature_Mixamo_External.prefab";
    private const string DefaultPlayerPrefabPath = "Assets/StarterAssets/ThirdPersonController/Prefabs/PlayerArmature.prefab";
    private const string MainCameraPrefabPath = "Assets/StarterAssets/ThirdPersonController/Prefabs/MainCamera.prefab";
    private const string FollowCameraPrefabPath = "Assets/StarterAssets/ThirdPersonController/Prefabs/PlayerFollowCamera.prefab";

    private static readonly Color MenuBackground = new Color(0.015f, 0.02f, 0.045f, 1f);
    private static readonly Color AccentColor = new Color(0.2f, 0.86f, 1f, 1f);
    private static readonly Color SecondaryColor = new Color(0.07f, 0.11f, 0.16f, 0.94f);
    private static readonly Color PanelColor = new Color(0.03f, 0.05f, 0.09f, 0.9f);
    private static readonly Color TextPrimary = new Color(0.92f, 0.97f, 1f, 1f);
    private static readonly Color TextMuted = new Color(0.62f, 0.74f, 0.86f, 1f);
    private static readonly Color WarningColor = new Color(1f, 0.42f, 0.12f, 1f);
    private static readonly Color MetalColor = new Color(0.14f, 0.17f, 0.22f, 1f);
    private static readonly Color MetalHighlightColor = new Color(0.19f, 0.24f, 0.31f, 1f);
    private static readonly string[] LunarTerrainLayerPaths =
    {
        "Assets/Lunar Landscape 3D/Resources/Materials/Ground_00.terrainlayer",
        "Assets/Lunar Landscape 3D/Resources/Materials/Ground_01.terrainlayer",
        "Assets/Lunar Landscape 3D/Resources/Materials/Ground_02.terrainlayer",
        "Assets/Lunar Landscape 3D/Resources/Materials/Ground_03.terrainlayer"
    };
    private static readonly string[] StylizedWallRockPrefabPaths =
    {
        "Assets/PolyOne/Rocks Stylized/Prefabs/SM_Rocks_01.prefab",
        "Assets/PolyOne/Rocks Stylized/Prefabs/SM_Rocks_03.prefab",
        "Assets/PolyOne/Rocks Stylized/Prefabs/SM_Rocks_05.prefab",
        "Assets/PolyOne/Rocks Stylized/Prefabs/SM_Rocks_07.prefab",
        "Assets/PolyOne/Rocks Stylized/Prefabs/SM_Rocks_09.prefab",
        "Assets/PolyOne/Rocks Stylized/Prefabs/SM_Rocks_11.prefab"
    };
    private static readonly string[] FreeFireLavaMaterialPaths =
    {
        "Assets/Vefects/Free Fire VFX URP/Materials/M_VFX_Fire_Quad_01.mat",
        "Assets/Vefects/Free Fire VFX URP/Materials/M_VFX_Fire_Quad_02.mat",
        "Assets/Vefects/Free Fire VFX URP/Materials/M_VFX_Fire_01.mat",
        "Assets/Vefects/Free Fire VFX URP/Materials/M_VFX_Fire_02.mat"
    };
    private static readonly string[] FreeFireLavaPrefabPaths =
    {
        "Assets/Vefects/Free Fire VFX URP/Particles/VFX_Fire_Floor_02_Simple.prefab",
        "Assets/Vefects/Free Fire VFX URP/Particles/VFX_Fire_Floor_01_Simple.prefab",
        "Assets/Vefects/Free Fire VFX URP/Particles/VFX_Fire_Floor_02.prefab",
        "Assets/Vefects/Free Fire VFX URP/Particles/VFX_Fire_Floor_01.prefab"
    };
    private const StaticEditorFlags EnvironmentStaticFlags =
        StaticEditorFlags.BatchingStatic
        | StaticEditorFlags.OccluderStatic
        | StaticEditorFlags.OccludeeStatic
        | StaticEditorFlags.ContributeGI
        | StaticEditorFlags.ReflectionProbeStatic;
    private static string _generationStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

    [MenuItem("Tools/Escape Facility/Generate Required Game Content")]
    public static void GenerateRequiredGameContent()
    {
        // Rebuild every generated scene and support asset from code so the project stays reproducible.
        _generationStamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        EnsureFolder("Assets", "Terrain");
        EnsureFolder("Assets/Terrain", "Layers");
        EnsureFolder("Assets/Terrain", "Textures");
        EnsureFolder("Assets/Materials", "Generated");
        EnsureFolder(GeneratedMaterialsFolder, "Imported");
        EnsureFolder(GeneratedMaterialsFolder, "Tiled");

        BuildMainMenuScene();
        BuildOptionsScene();
        BuildOutdoorScene();
        BuildIndoorScene();
        BuildEndScene();
        UpdateBuildSettings();

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Escape Facility scenes and build settings generated.");
    }

    public static void GenerateRequiredGameContentBatchmode()
    {
        GenerateRequiredGameContent();
        EditorApplication.Exit(0);
    }

    private static void BuildMainMenuScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateSceneVersionMarker(SceneNameRegistry.MainMenu);

        CreateSimpleCamera("Menu Camera", MenuBackground);
        MainMenuController controller = new GameObject("MainMenuController").AddComponent<MainMenuController>();
        Canvas canvas = CreateCanvas("Canvas");
        CreateFullScreenPanel(canvas.transform, "Background", MenuBackground);
        CreateAccentBar(canvas.transform, "TopHorizon", new Vector2(0f, 360f), new Vector2(920f, 4f), AccentColor);
        CreateAccentBar(canvas.transform, "BottomHorizon", new Vector2(0f, -360f), new Vector2(920f, 2f), new Color(0.12f, 0.2f, 0.28f, 1f));

        GameObject heroPanel = CreateWindowPanel(canvas.transform, "HeroPanel", new Vector2(860f, 560f), new Vector2(0f, 10f));
        CreateText(heroPanel.transform, "SectionTag", "LUNAR CONTAINMENT COMPLEX", 18, new Vector2(0f, 205f), new Vector2(700f, 32f), FontStyle.Bold, TextAnchor.MiddleCenter, AccentColor);
        CreateText(heroPanel.transform, "Title", "Escape the Facility", 66, new Vector2(0f, 145f), new Vector2(900f, 100f), FontStyle.Bold, TextAnchor.MiddleCenter, TextPrimary);
        CreateText(heroPanel.transform, "Subtitle", "Recover the power cores, reach the indoor installation, recharge them, and transmit the rescue call.", 24, new Vector2(0f, 90f), new Vector2(720f, 70f), FontStyle.Normal, TextAnchor.MiddleCenter, TextMuted);
        CreateText(heroPanel.transform, "StatusLine", "NIGHT SHIFT ACTIVE  //  CONTAINMENT LEVEL 04", 18, new Vector2(0f, 35f), new Vector2(700f, 30f), FontStyle.Bold, TextAnchor.MiddleCenter, WarningColor);

        Button startButton = CreateButton(heroPanel.transform, "StartButton", "Begin Run", new Vector2(0f, -40f), new Vector2(340f, 74f));
        Button optionsButton = CreateButton(heroPanel.transform, "OptionsButton", "Mission Settings", new Vector2(0f, -128f), new Vector2(340f, 74f));
        Button exitButton = CreateButton(heroPanel.transform, "ExitButton", "Abort Shift", new Vector2(0f, -216f), new Vector2(340f, 74f));
        CreateText(heroPanel.transform, "FooterText", "Thirty minutes. One maze yard. One rescue signal. Keep moving.", 18, new Vector2(0f, -248f), new Vector2(720f, 30f), FontStyle.Italic, TextAnchor.MiddleCenter, TextMuted);

        UnityEventTools.AddPersistentListener(startButton.onClick, controller.StartGame);
        UnityEventTools.AddPersistentListener(optionsButton.onClick, controller.OpenOptions);
        UnityEventTools.AddPersistentListener(exitButton.onClick, controller.ExitGame);

        CreateEventSystem();
        SaveScene(scene, $"{ScenesFolder}/{SceneNameRegistry.MainMenu}.unity");
    }

    private static void BuildOptionsScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateSceneVersionMarker(SceneNameRegistry.OptionsMenu);

        CreateSimpleCamera("Options Camera", MenuBackground);
        OptionsMenuController controller = new GameObject("OptionsMenuController").AddComponent<OptionsMenuController>();
        Canvas canvas = CreateCanvas("Canvas");
        CreateFullScreenPanel(canvas.transform, "Background", MenuBackground);

        GameObject controlPanel = CreateWindowPanel(canvas.transform, "ControlPanel", new Vector2(760f, 560f), new Vector2(0f, 10f));
        CreateText(controlPanel.transform, "Title", "Mission Settings", 58, new Vector2(0f, 185f), new Vector2(700f, 90f), FontStyle.Bold, TextAnchor.MiddleCenter, TextPrimary);
        CreateText(controlPanel.transform, "Subtitle", "Tune the facility systems before dropping back into the complex.", 22, new Vector2(0f, 130f), new Vector2(640f, 54f), FontStyle.Normal, TextAnchor.MiddleCenter, TextMuted);
        Text musicLabel = CreateText(controlPanel.transform, "MusicLabel", "Audio Feed: ON", 28, new Vector2(0f, 60f), new Vector2(420f, 50f), FontStyle.Bold, TextAnchor.MiddleCenter, AccentColor);

        SerializedObject controllerObject = new SerializedObject(controller);
        controllerObject.FindProperty("musicStatusText").objectReferenceValue = musicLabel;
        controllerObject.ApplyModifiedPropertiesWithoutUndo();

        Button musicButton = CreateButton(controlPanel.transform, "ToggleMusicButton", "Toggle Audio Feed", new Vector2(0f, -25f), new Vector2(340f, 70f));
        Button level1Button = CreateButton(controlPanel.transform, "Level1Button", "Deploy To Yard", new Vector2(0f, -113f), new Vector2(340f, 70f));
        Button level2Button = CreateButton(controlPanel.transform, "Level2Button", "Deploy To Reactor", new Vector2(0f, -201f), new Vector2(340f, 70f));
        Button backButton = CreateButton(controlPanel.transform, "BackButton", "Return To Hub", new Vector2(0f, -259f), new Vector2(340f, 70f));

        UnityEventTools.AddPersistentListener(musicButton.onClick, controller.ToggleMusic);
        UnityEventTools.AddPersistentListener(level1Button.onClick, controller.LoadLevel1);
        UnityEventTools.AddPersistentListener(level2Button.onClick, controller.LoadLevel2);
        UnityEventTools.AddPersistentListener(backButton.onClick, controller.BackToMainMenu);

        CreateEventSystem();
        SaveScene(scene, $"{ScenesFolder}/{SceneNameRegistry.OptionsMenu}.unity");
    }

    private static void BuildOutdoorScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateSceneVersionMarker(SceneNameRegistry.Level1);

        // The outdoor level is terrain-driven and combines traversal with embedded lava killzones.
        TerrainData terrainData = GetOrCreateTerrainData();
        TerrainLayer[] terrainLayers = GetOutdoorTerrainLayers();
        terrainData.terrainLayers = terrainLayers;
        ShapeOutdoorTerrain(terrainData);
        PaintOutdoorTerrain(terrainData, terrainLayers.Length);

        GameObject terrainObject = Terrain.CreateTerrainGameObject(terrainData);
        terrainObject.name = "Terrain";
        Terrain terrain = terrainObject.GetComponent<Terrain>();
        terrain.materialTemplate = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/TerrainURP.mat");
        terrain.drawInstanced = true;

        CreateGameplayLights();
        SetupGameplaySceneContext(
            "OutdoorContext",
            requiredCollectibles: 3,
            nextSceneName: SceneNameRegistry.Level2,
            musicTheme: MusicTheme.Outdoor,
            objectiveMode: SceneObjectiveMode.CollectAndExit,
            objectiveProgressTarget: 3,
            progressObjectiveTemplate: "Objective: Collect power cores ({0}/{1})",
            postProgressObjectiveText: "Objective: Reach the indoor installation",
            postActivationObjectiveText: "",
            completedObjectiveText: "Objective: Outdoor route secured");
        SetupGameplayUI();
        SetupPlayerRig(new Vector3(12f, 3f, 12f));
        CreateNightSky();

        Material outdoorMetalMaterial = GetOrCreateLitMaterial($"{GeneratedMaterialsFolder}/OutdoorMetal.mat", MetalColor, new Color(0f, 0f, 0f), 0.42f);
        Material techTrimMaterial = GetOrCreateLitMaterial($"{GeneratedMaterialsFolder}/TechTrim.mat", MetalHighlightColor, new Color(0.0f, 0.42f, 0.58f), 0.26f);
        Material warningMaterial = GetOrCreateLitMaterial($"{GeneratedMaterialsFolder}/WarningTrim.mat", new Color(0.35f, 0.13f, 0.05f), new Color(1.75f, 0.48f, 0.08f), 0.12f);
        Material killzoneMaterial = GetOutdoorKillzoneMaterial();
        Material coreMaterial = GetOrCreateLitMaterial($"{GeneratedMaterialsFolder}/EnergyCore.mat", new Color(0.18f, 0.8f, 1f), new Color(0.2f, 1f, 2f), 0.1f);
        Material doorMaterial = GetOrCreateLitMaterial($"{GeneratedMaterialsFolder}/Door.mat", new Color(0.18f, 0.2f, 0.24f), new Color(0.0f, 0.35f, 0.5f), 0.4f);
        Material moonRockMaterial = GetOutdoorRockMaterial();
        Material wallRockMaterial = GetOutdoorWallRockMaterial();

        CreateLandingPad("ArrivalPad", new Vector3(12f, 0.15f, 12f), new Vector3(16f, 0.3f, 16f), outdoorMetalMaterial, techTrimMaterial);
        CreateLandingPad("ExtractionPad", new Vector3(102f, 0.15f, 96f), new Vector3(20f, 0.3f, 20f), outdoorMetalMaterial, techTrimMaterial);
        CreateLightPylon("YardBeacon_A", new Vector3(18f, 0f, 18f), techTrimMaterial, AccentColor, 10f, 3.5f, solid: true);
        CreateLightPylon("YardBeacon_B", new Vector3(18f, 0f, 84f), techTrimMaterial, AccentColor, 10f, 3.5f, solid: true);
        CreateLightPylon("YardBeacon_C", new Vector3(64f, 0f, 84f), techTrimMaterial, AccentColor, 10f, 3.5f, solid: true);
        CreateLightPylon("YardBeacon_D", new Vector3(100f, 0f, 82f), techTrimMaterial, AccentColor, 12f, 3.5f, solid: true);
        CreateWarningBeacon("KillzoneWarning_A", new Vector3(12f, 0f, 108f), warningMaterial, 12f);
        CreateWarningBeacon("KillzoneWarning_B", new Vector3(108f, 0f, 12f), warningMaterial, 12f);

        CreateCollectible("Core_01", new Vector3(16f, 2f, 78f), coreMaterial);
        CreateCollectible("Core_02", new Vector3(43f, 2f, 24f), coreMaterial);
        CreateCollectible("Core_03", new Vector3(104f, 2f, 76f), coreMaterial);

        CreateMountainMaze(terrain, wallRockMaterial);

        CreateRockOutcrop("RockCluster_A", new Vector3(26f, 0.6f, 88f), new Vector3(7f, 2.6f, 5.5f), wallRockMaterial, 18f);
        CreateRockOutcrop("RockCluster_B", new Vector3(86f, 0.55f, 18f), new Vector3(6.5f, 2.2f, 4.5f), wallRockMaterial, -22f);
        CreateRockOutcrop("RockCluster_C", new Vector3(100f, 0.75f, 56f), new Vector3(8.5f, 3f, 6f), wallRockMaterial, 11f);
        CreateRockOutcrop("RockCluster_D", new Vector3(44f, 0.45f, 106f), new Vector3(5.8f, 2f, 4.2f), wallRockMaterial, -9f);

        CreateRockEntranceFacade(terrain, "FacilityRockFace", new Vector3(102f, 0f, 100f), wallRockMaterial);
        CreateOutdoorFacilityEntrance("FacilityEntrance", new Vector3(102f, 3f, 100.8f), doorMaterial);

        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = "ExitDoor";
        door.transform.position = new Vector3(102f, 3f, 100.8f);
        door.transform.localScale = new Vector3(4f, 6f, 1f);
        MeshRenderer doorRenderer = door.GetComponent<MeshRenderer>();
        doorRenderer.sharedMaterial = doorMaterial;
        doorRenderer.enabled = false;
        DoorController doorController = door.AddComponent<DoorController>();
        SerializedObject doorObject = new SerializedObject(doorController);
        doorObject.FindProperty("nextSceneName").stringValue = SceneNameRegistry.Level2;
        doorObject.FindProperty("completesRun").boolValue = false;
        doorObject.FindProperty("requiresSceneObjective").boolValue = false;
        doorObject.ApplyModifiedPropertiesWithoutUndo();
        AttachDoorVisual(door.transform, "Assets/Sci Fi Modular Pack/Prefabs/Door.prefab", new Vector3(0f, -3f, -0.2f), new Vector3(0f, 180f, 0f), new Vector3(1.15f, 1.15f, 1.15f));

        CreateKillzoneMaze(terrain, killzoneMaterial, moonRockMaterial, warningMaterial);
        CreatePerimeterKillzone(terrain, killzoneMaterial, moonRockMaterial, warningMaterial);

        Light outdoorPointLight = CreateLight("Outdoor Beacon", LightType.Point, new Vector3(96f, 6f, 96f), new Color(0.5f, 0.9f, 1f), 7f, 18f, LightmapBakeType.Baked);
        outdoorPointLight.shadows = LightShadows.None;

        Light outdoorSpotLight = CreateLight("Exit Spotlight", LightType.Spot, new Vector3(102f, 10f, 88f), new Color(0.8f, 0.95f, 1f), 8f, 24f, LightmapBakeType.Mixed);
        outdoorSpotLight.transform.rotation = Quaternion.Euler(55f, 180f, 0f);
        outdoorSpotLight.spotAngle = 42f;

        SaveScene(scene, $"{ScenesFolder}/{SceneNameRegistry.Level1}.unity");
    }

    private static void BuildIndoorScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateSceneVersionMarker(SceneNameRegistry.Level2);

        // The indoor level shifts from open traversal to a denser containment-facility layout.
        EnsureSciFiCorridorMaterialsCompatible();
        SetupGameplaySceneContext(
            "IndoorContext",
            requiredCollectibles: 0,
            nextSceneName: SceneNameRegistry.EndScreen,
            musicTheme: MusicTheme.Indoor,
            objectiveMode: SceneObjectiveMode.ChargeAndBroadcast,
            objectiveProgressTarget: 3,
            progressObjectiveTemplate: "Objective: Search the yard for power cores ({0}/{1})",
            postProgressObjectiveText: "Objective: Charge the power cores at the control panel",
            postActivationObjectiveText: "Objective: Broadcast the rescue message",
            completedObjectiveText: "Objective: Rescue signal transmitted");
        SetupGameplayUI();
        SetupPlayerRig(new Vector3(0f, 2f, -15f));
        ConfigureIndoorAtmosphere();

        Material panelMaterial = GetOrCreateLitMaterial($"{GeneratedMaterialsFolder}/IndoorPanel.mat", new Color(0.1f, 0.12f, 0.16f), new Color(0f, 0f, 0f), 0.28f);
        Material trimMaterial = GetOrCreateLitMaterial($"{GeneratedMaterialsFolder}/IndoorTrim.mat", MetalHighlightColor, new Color(0.0f, 0.45f, 0.6f), 0.24f);
        Material accentPanelMaterial = GetOrCreateLitMaterial($"{GeneratedMaterialsFolder}/IndoorAccentPanel.mat", new Color(0.11f, 0.18f, 0.23f), new Color(0.0f, 0.42f, 0.58f), 0.16f);
        Material industrialMaterial = GetOrCreateLitMaterial($"{GeneratedMaterialsFolder}/IndoorIndustrial.mat", new Color(0.29f, 0.22f, 0.16f), new Color(0f, 0f, 0f), 0.11f);
        Material hazardMaterial = GetOrCreateLitMaterial($"{GeneratedMaterialsFolder}/Hazard.mat", new Color(0.5f, 0.08f, 0.08f), new Color(1.4f, 0.05f, 0.02f), 0.1f);
        Material doorMaterial = GetOrCreateLitMaterial($"{GeneratedMaterialsFolder}/FinalDoor.mat", new Color(0.15f, 0.16f, 0.2f), new Color(0.1f, 0.9f, 1.2f), 0.45f);

        CreateIndoorCollisionShell();
        CreateIndoorDecor(trimMaterial, panelMaterial, accentPanelMaterial, industrialMaterial);
        CreateSciFiCorridorDecor();

        GameObject hazard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        hazard.name = "ReactorCoreSurface";
        hazard.transform.position = new Vector3(12f, 0.6f, 14f);
        hazard.transform.localScale = new Vector3(8f, 1.2f, 8f);
        hazard.GetComponent<MeshRenderer>().sharedMaterial = hazardMaterial;
        MarkEnvironmentStatic(hazard);
        CreateHazardFrame("ReactorFrame", new Vector3(12f, 0.05f, 14f), new Vector3(11.5f, 0.2f, 11.5f), trimMaterial, hazardMaterial);

        CreateMissionTerminalStation(
            "ChargePanel",
            new Vector3(12f, 0f, 14f),
            180f,
            panelMaterial,
            accentPanelMaterial,
            trimMaterial,
            MissionTerminal.TerminalAction.ChargePowerCores);

        CreateMissionTerminalStation(
            "BroadcastConsole",
            new Vector3(0f, 0f, 16.1f),
            180f,
            doorMaterial,
            accentPanelMaterial,
            trimMaterial,
            MissionTerminal.TerminalAction.BroadcastRescue);

        CreateBroadcastArray("BroadcastArray", new Vector3(0f, 0f, 18.1f), industrialMaterial, trimMaterial);
        CreateDoorFrameLights("BroadcastFrame", new Vector3(0f, 3f, 18f), trimMaterial, AccentColor);
        CreateIndoorTransferDoor("YardAirlock", new Vector3(0f, 3f, -18f), doorMaterial, trimMaterial, SceneNameRegistry.Level1);
        CreateIndoorFacilityLights();

        SaveScene(scene, $"{ScenesFolder}/{SceneNameRegistry.Level2}.unity");
    }

    private static void BuildEndScene()
    {
        Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        CreateSceneVersionMarker(SceneNameRegistry.EndScreen);

        CreateSimpleCamera("End Camera", MenuBackground);
        EndScreenController controller = new GameObject("EndScreenController").AddComponent<EndScreenController>();
        Canvas canvas = CreateCanvas("Canvas");
        CreateFullScreenPanel(canvas.transform, "Background", MenuBackground);

        GameObject resultPanel = CreateWindowPanel(canvas.transform, "ResultPanel", new Vector2(760f, 520f), new Vector2(0f, 10f));
        Text title = CreateText(resultPanel.transform, "ResultTitle", "Victory", 62, new Vector2(0f, 170f), new Vector2(900f, 90f), FontStyle.Bold, TextAnchor.MiddleCenter, TextPrimary);
        Text subtitle = CreateText(resultPanel.transform, "ResultSubtitle", "Rescue signal transmitted. Extraction is inbound.", 24, new Vector2(0f, 118f), new Vector2(640f, 46f), FontStyle.Normal, TextAnchor.MiddleCenter, AccentColor);
        Text score = CreateText(resultPanel.transform, "ScoreText", "Score: 0", 28, new Vector2(0f, 40f), new Vector2(500f, 50f), FontStyle.Bold, TextAnchor.MiddleCenter, TextPrimary);
        Text time = CreateText(resultPanel.transform, "TimeText", "Time Left: 00:00", 28, new Vector2(0f, -5f), new Vector2(500f, 50f), FontStyle.Bold, TextAnchor.MiddleCenter, TextPrimary);
        Text collected = CreateText(resultPanel.transform, "CollectedText", "Collected: 0", 28, new Vector2(0f, -50f), new Vector2(500f, 50f), FontStyle.Bold, TextAnchor.MiddleCenter, TextPrimary);

        SerializedObject controllerObject = new SerializedObject(controller);
        controllerObject.FindProperty("titleText").objectReferenceValue = title;
        controllerObject.FindProperty("subtitleText").objectReferenceValue = subtitle;
        controllerObject.FindProperty("scoreText").objectReferenceValue = score;
        controllerObject.FindProperty("timeText").objectReferenceValue = time;
        controllerObject.FindProperty("collectedText").objectReferenceValue = collected;
        controllerObject.ApplyModifiedPropertiesWithoutUndo();

        Button replayButton = CreateButton(resultPanel.transform, "ReplayButton", "Run Again", new Vector2(0f, -145f), new Vector2(300f, 70f));
        Button exitButton = CreateButton(resultPanel.transform, "ExitButton", "Leave Facility", new Vector2(0f, -214f), new Vector2(300f, 70f));

        UnityEventTools.AddPersistentListener(replayButton.onClick, controller.Replay);
        UnityEventTools.AddPersistentListener(exitButton.onClick, controller.ExitGame);

        CreateEventSystem();
        SaveScene(scene, $"{ScenesFolder}/{SceneNameRegistry.EndScreen}.unity");
    }

    private static void UpdateBuildSettings()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene($"{ScenesFolder}/{SceneNameRegistry.MainMenu}.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/{SceneNameRegistry.OptionsMenu}.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/{SceneNameRegistry.Level1}.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/{SceneNameRegistry.Level2}.unity", true),
            new EditorBuildSettingsScene($"{ScenesFolder}/{SceneNameRegistry.EndScreen}.unity", true)
        };
    }

    private static void SetupGameplayUI()
    {
        Canvas canvas = CreateCanvas("GameplayCanvas");
        GameplayHUD hud = new GameObject("GameplayHUD").AddComponent<GameplayHUD>();
        hud.transform.SetParent(canvas.transform, false);

        GameObject statusPanel = CreateWindowPanel(canvas.transform, "StatusPanel", new Vector2(360f, 170f), Vector2.zero, new Color(0.03f, 0.05f, 0.09f, 0.84f));
        RectTransform statusRect = statusPanel.GetComponent<RectTransform>();
        statusRect.anchorMin = new Vector2(0f, 1f);
        statusRect.anchorMax = new Vector2(0f, 1f);
        statusRect.pivot = new Vector2(0f, 1f);
        statusRect.anchoredPosition = new Vector2(30f, -30f);
        CreateText(statusPanel.transform, "StatusTag", "OPS FEED", 16, new Vector2(0f, 54f), new Vector2(280f, 28f), FontStyle.Bold, TextAnchor.MiddleCenter, AccentColor);
        Text scoreText = CreateText(statusPanel.transform, "ScoreText", "Score: 0", 24, new Vector2(0f, 12f), new Vector2(280f, 38f), FontStyle.Bold, TextAnchor.MiddleCenter, TextPrimary);
        Text timeText = CreateText(statusPanel.transform, "TimeText", "Time Left: 30:00", 24, new Vector2(0f, -24f), new Vector2(280f, 38f), FontStyle.Bold, TextAnchor.MiddleCenter, TextPrimary);
        Text objectiveText = CreateText(statusPanel.transform, "ObjectiveText", "Objective: Collect power cores (0/3)", 21, new Vector2(0f, -60f), new Vector2(320f, 46f), FontStyle.Bold, TextAnchor.MiddleCenter, WarningColor);
        Text interactionPromptText = CreateText(canvas.transform, "InteractionPrompt", "Press E to interact", 24, new Vector2(0f, 90f), new Vector2(520f, 48f), FontStyle.Bold, TextAnchor.MiddleCenter, AccentColor);
        RectTransform interactionRect = interactionPromptText.GetComponent<RectTransform>();
        interactionRect.anchorMin = new Vector2(0.5f, 0f);
        interactionRect.anchorMax = new Vector2(0.5f, 0f);
        interactionRect.pivot = new Vector2(0.5f, 0f);
        interactionRect.anchoredPosition = new Vector2(0f, 70f);
        interactionPromptText.gameObject.SetActive(false);

        GameObject pausePanel = CreateWindowPanel(canvas.transform, "PausePanel", new Vector2(560f, 430f), Vector2.zero);
        RectTransform pauseRect = pausePanel.GetComponent<RectTransform>();
        pauseRect.anchorMin = new Vector2(0.5f, 0.5f);
        pauseRect.anchorMax = new Vector2(0.5f, 0.5f);
        pauseRect.pivot = new Vector2(0.5f, 0.5f);
        pauseRect.anchoredPosition = Vector2.zero;
        CreateText(pausePanel.transform, "PauseTag", "SYSTEM HOLD", 18, new Vector2(0f, 138f), new Vector2(320f, 30f), FontStyle.Bold, TextAnchor.MiddleCenter, AccentColor);
        CreateText(pausePanel.transform, "PauseTitle", "Paused", 48, new Vector2(0f, 90f), new Vector2(300f, 70f), FontStyle.Bold, TextAnchor.MiddleCenter, TextPrimary);
        Button resumeButton = CreateButton(pausePanel.transform, "ResumeButton", "Resume Run", new Vector2(0f, 10f), new Vector2(270f, 64f));
        Button mainMenuButton = CreateButton(pausePanel.transform, "MainMenuButton", "Return To Hub", new Vector2(0f, -72f), new Vector2(270f, 64f));
        Button exitButton = CreateButton(pausePanel.transform, "ExitButton", "Exit Facility", new Vector2(0f, -154f), new Vector2(270f, 64f));

        SerializedObject hudObject = new SerializedObject(hud);
        hudObject.FindProperty("scoreText").objectReferenceValue = scoreText;
        hudObject.FindProperty("timerText").objectReferenceValue = timeText;
        hudObject.FindProperty("objectiveText").objectReferenceValue = objectiveText;
        hudObject.FindProperty("interactionPromptText").objectReferenceValue = interactionPromptText;
        hudObject.FindProperty("pauseRoot").objectReferenceValue = pausePanel;
        hudObject.ApplyModifiedPropertiesWithoutUndo();

        UnityEventTools.AddPersistentListener(resumeButton.onClick, hud.ResumeGame);
        UnityEventTools.AddPersistentListener(mainMenuButton.onClick, hud.ReturnToMainMenu);
        UnityEventTools.AddPersistentListener(exitButton.onClick, hud.ExitGame);

        CreateEventSystem();
    }

    private static void SetupPlayerRig(Vector3 startPosition)
    {
        GameObject playerPrefab = GetPreferredPlayerPrefab();
        GameObject cameraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MainCameraPrefabPath);
        GameObject followCameraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(FollowCameraPrefabPath);

        if (playerPrefab == null || cameraPrefab == null || followCameraPrefab == null)
        {
            throw new System.InvalidOperationException("Starter Assets prefabs are missing.");
        }

        GameObject player = (GameObject)PrefabUtility.InstantiatePrefab(playerPrefab);
        player.name = "PlayerArmature";
        player.transform.position = startPosition;

        GameObject mainCamera = (GameObject)PrefabUtility.InstantiatePrefab(cameraPrefab);
        mainCamera.name = "Main Camera";
        ConfigureGameplayCamera(mainCamera);

        GameObject followCamera = (GameObject)PrefabUtility.InstantiatePrefab(followCameraPrefab);
        followCamera.name = "PlayerFollowCamera";

        ThirdPersonController controller = player.GetComponent<ThirdPersonController>();
        CinemachineVirtualCamera virtualCamera = followCamera.GetComponent<CinemachineVirtualCamera>();
        if (controller != null && controller.CinemachineCameraTarget != null && virtualCamera != null)
        {
            virtualCamera.Follow = controller.CinemachineCameraTarget.transform;
            virtualCamera.LookAt = controller.CinemachineCameraTarget.transform;
        }
    }

    private static void ConfigureGameplayCamera(GameObject mainCamera)
    {
        Camera camera = mainCamera.GetComponent<Camera>();
        if (camera == null)
        {
            return;
        }

        // Use a deterministic fallback color so gameplay scenes never render a bright white void.
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.015f, 0.02f, 0.045f, 1f);
    }

    private static GameObject GetPreferredPlayerPrefab()
    {
        // Never fall back to the default Starter Assets mesh: the project must always use a custom protagonist.
        GameObject playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MixamoExternalPrefabPath);
        if (playerPrefab != null)
        {
            return playerPrefab;
        }

        string externalSourceModelPath = MixamoCharacterSetup.GetExternalSourceModelPath();
        if (!string.IsNullOrEmpty(externalSourceModelPath))
        {
            MixamoCharacterSetup.SetupExternalMixamoCharacter();
            playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MixamoExternalPrefabPath);
            if (playerPrefab != null)
            {
                return playerPrefab;
            }
        }

        playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MixamoPrimaryPrefabPath);
        if (playerPrefab != null)
        {
            return playerPrefab;
        }

        if (File.Exists(MixamoPrimarySourcePath))
        {
            MixamoCharacterSetup.SetupMixamoCharacter();
            playerPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(MixamoPrimaryPrefabPath);
            if (playerPrefab != null)
            {
                return playerPrefab;
            }
        }

        throw new System.InvalidOperationException(
            "No custom player prefab could be resolved. The project requires a Mixamo-based protagonist to satisfy the technical evaluation criteria.");
    }

    private static void CreateGameplayLights()
    {
        Light moon = CreateLight("Moon Light", LightType.Directional, new Vector3(0f, 10f, 0f), new Color(0.72f, 0.8f, 1f), 0.38f, 0f, LightmapBakeType.Realtime);
        moon.transform.rotation = Quaternion.Euler(18f, -28f, 0f);
        moon.shadows = LightShadows.Soft;
        RenderSettings.sun = moon;
    }

    private static void CreateNightSky()
    {
        Material nightSkybox = GetOrCreateProceduralSkyboxMaterial(
            $"{GeneratedMaterialsFolder}/NightSkybox.mat",
            new Color(0.06f, 0.1f, 0.22f),
            new Color(0.01f, 0.01f, 0.02f),
            0.55f,
            0.55f,
            0.03f,
            8f);

        RenderSettings.skybox = nightSkybox;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.09f, 0.12f, 0.2f);
        RenderSettings.ambientEquatorColor = new Color(0.03f, 0.04f, 0.08f);
        RenderSettings.ambientGroundColor = new Color(0.01f, 0.01f, 0.02f);
        RenderSettings.reflectionIntensity = 0.35f;
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.03f, 0.05f, 0.09f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 10f;
        RenderSettings.fogEndDistance = 160f;

        new GameObject("NightSky").AddComponent<NightSkyController>();
    }

    private static void ConfigureIndoorAtmosphere()
    {
        Material indoorSkybox = AssetDatabase.LoadAssetAtPath<Material>($"{GeneratedMaterialsFolder}/NightSkybox.mat");
        if (indoorSkybox == null)
        {
            indoorSkybox = GetOrCreateProceduralSkyboxMaterial(
                $"{GeneratedMaterialsFolder}/NightSkybox.mat",
                new Color(0.06f, 0.1f, 0.22f),
                new Color(0.01f, 0.01f, 0.02f),
                0.55f,
                0.5f,
                0.03f,
                8f);
        }

        RenderSettings.skybox = indoorSkybox;
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.14f, 0.18f, 0.28f);
        RenderSettings.ambientEquatorColor = new Color(0.07f, 0.09f, 0.14f);
        RenderSettings.ambientGroundColor = new Color(0.03f, 0.03f, 0.05f);
        RenderSettings.reflectionIntensity = 0.55f;

        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.06f, 0.08f, 0.12f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 14f;
        RenderSettings.fogEndDistance = 110f;
    }

    private static void CreateIndoorCollisionShell()
    {
        CreateInvisibleCollisionBlock("Floor", new Vector3(0f, -0.5f, 0f), new Vector3(40f, 1f, 40f));
        CreateInvisibleCollisionBlock("NorthWall", new Vector3(0f, 5f, 20f), new Vector3(40f, 10f, 1f));
        CreateInvisibleCollisionBlock("SouthWall", new Vector3(0f, 5f, -20f), new Vector3(40f, 10f, 1f));
        CreateInvisibleCollisionBlock("EastWall", new Vector3(20f, 5f, 0f), new Vector3(1f, 10f, 40f));
        CreateInvisibleCollisionBlock("WestWall", new Vector3(-20f, 5f, 0f), new Vector3(1f, 10f, 40f));
        CreateInvisibleCollisionBlock("Ceiling", new Vector3(0f, 10f, 0f), new Vector3(40f, 1f, 40f));
    }

    private static void CreateMissionTerminalStation(
        string objectName,
        Vector3 position,
        float yaw,
        Material baseMaterial,
        Material accentMaterial,
        Material trimMaterial,
        MissionTerminal.TerminalAction terminalAction)
    {
        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        GameObject root = new GameObject(objectName);
        root.transform.position = position;
        root.transform.rotation = rotation;

        GameObject consoleBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
        consoleBase.name = $"{objectName}_Base";
        consoleBase.transform.SetParent(root.transform, false);
        consoleBase.transform.localPosition = new Vector3(0f, 1.3f, -0.35f);
        consoleBase.transform.localScale = new Vector3(3.2f, 2.6f, 1.4f);
        consoleBase.GetComponent<MeshRenderer>().sharedMaterial = baseMaterial;

        GameObject consoleFace = GameObject.CreatePrimitive(PrimitiveType.Cube);
        consoleFace.name = $"{objectName}_Face";
        consoleFace.transform.SetParent(root.transform, false);
        consoleFace.transform.localPosition = new Vector3(0f, 1.9f, 0.42f);
        consoleFace.transform.localScale = new Vector3(2.4f, 1.1f, 0.12f);
        consoleFace.GetComponent<MeshRenderer>().sharedMaterial = accentMaterial;
        RemoveCollider(consoleFace);

        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Cube);
        indicator.name = $"{objectName}_Indicator";
        indicator.transform.SetParent(root.transform, false);
        indicator.transform.localPosition = new Vector3(0f, 2.15f, 0.5f);
        indicator.transform.localScale = new Vector3(1.25f, 0.32f, 0.08f);
        MeshRenderer indicatorRenderer = indicator.GetComponent<MeshRenderer>();
        indicatorRenderer.sharedMaterial = trimMaterial;
        RemoveCollider(indicator);

        GameObject overhead = GameObject.CreatePrimitive(PrimitiveType.Cube);
        overhead.name = $"{objectName}_Header";
        overhead.transform.SetParent(root.transform, false);
        overhead.transform.localPosition = new Vector3(0f, 3.35f, -0.05f);
        overhead.transform.localScale = new Vector3(2.8f, 0.22f, 0.22f);
        overhead.GetComponent<MeshRenderer>().sharedMaterial = trimMaterial;
        RemoveCollider(overhead);

        GameObject triggerZone = new GameObject($"{objectName}_Trigger");
        triggerZone.transform.SetParent(root.transform, false);
        triggerZone.transform.localPosition = new Vector3(0f, 1.5f, 1.3f);
        BoxCollider triggerCollider = triggerZone.AddComponent<BoxCollider>();
        triggerCollider.size = new Vector3(3.2f, 3f, 2.4f);
        MissionTerminal terminal = triggerZone.AddComponent<MissionTerminal>();

        SerializedObject terminalObject = new SerializedObject(terminal);
        terminalObject.FindProperty("terminalAction").enumValueIndex = (int)terminalAction;
        terminalObject.FindProperty("indicatorRenderer").objectReferenceValue = indicatorRenderer;
        terminalObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateBroadcastArray(string objectName, Vector3 position, Material mastMaterial, Material accentMaterial)
    {
        GameObject mast = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        mast.name = objectName;
        mast.transform.position = position + new Vector3(0f, 4.5f, 0f);
        mast.transform.localScale = new Vector3(0.18f, 4.5f, 0.18f);
        mast.GetComponent<MeshRenderer>().sharedMaterial = mastMaterial;
        RemoveCollider(mast);
        MarkEnvironmentStatic(mast);

        CreateLightStrip($"{objectName}_CrossBeamA", position + new Vector3(0f, 6.2f, 0f), new Vector3(3.4f, 0.08f, 0.08f), accentMaterial);
        CreateLightStrip($"{objectName}_CrossBeamB", position + new Vector3(0f, 7.6f, 0f), new Vector3(2.4f, 0.08f, 0.08f), accentMaterial);

        Light uplinkGlow = CreateLight($"{objectName}_Glow", LightType.Point, position + new Vector3(0f, 7.2f, 0f), AccentColor, 3.8f, 11f, LightmapBakeType.Realtime);
        uplinkGlow.shadows = LightShadows.None;
    }

    private static void CreateIndoorTransferDoor(string objectName, Vector3 position, Material doorMaterial, Material trimMaterial, string nextSceneName)
    {
        GameObject door = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door.name = objectName;
        door.transform.position = position;
        door.transform.localScale = new Vector3(4f, 6f, 1f);
        door.GetComponent<MeshRenderer>().sharedMaterial = doorMaterial;
        CreateDoorFrameLights($"{objectName}_Frame", position, trimMaterial, AccentColor);

        DoorController doorController = door.AddComponent<DoorController>();
        SerializedObject doorObject = new SerializedObject(doorController);
        doorObject.FindProperty("nextSceneName").stringValue = nextSceneName;
        doorObject.FindProperty("completesRun").boolValue = false;
        doorObject.FindProperty("requiresSceneObjective").boolValue = false;
        doorObject.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateOutdoorFacilityEntrance(string objectName, Vector3 doorPosition, Material trimMaterial)
    {
        PlaceDecorPrefab($"{objectName}_Frame", "Assets/Sci Fi Modular Pack/Prefabs/DoorFrame.prefab", doorPosition + new Vector3(0f, -3f, -0.35f), new Vector3(0f, 180f, 0f), new Vector3(1.2f, 1.2f, 1.2f));
        PlaceDecorPrefab($"{objectName}_Wall", "Assets/Sci Fi Modular Pack/Prefabs/WallDoor.prefab", doorPosition + new Vector3(0f, -3f, 0.55f), new Vector3(0f, 180f, 0f), new Vector3(1.2f, 1.2f, 1.2f));
        CreateDoorFrameLights($"{objectName}_Lights", doorPosition, trimMaterial, AccentColor);
    }

    private static void CreateIndoorDecor(Material trimMaterial, Material panelMaterial, Material accentPanelMaterial, Material industrialMaterial)
    {
        // The corridor pack now defines the main shell, so keep this layer focused on support details only.
        CreateLightStrip("NorthTrim", new Vector3(0f, 7.5f, 19.3f), new Vector3(22f, 0.18f, 0.18f), trimMaterial);
        CreateLightStrip("SouthTrim", new Vector3(0f, 7.5f, -19.3f), new Vector3(22f, 0.18f, 0.18f), trimMaterial);
        CreateLightStrip("EastTrim", new Vector3(19.3f, 7.5f, 0f), new Vector3(0.18f, 0.18f, 22f), trimMaterial);
        CreateLightStrip("WestTrim", new Vector3(-19.3f, 7.5f, 0f), new Vector3(0.18f, 0.18f, 22f), trimMaterial);
        CreateLightStrip("DoorApproachStrip", new Vector3(0f, 0.05f, 11f), new Vector3(2.5f, 0.08f, 10f), trimMaterial);
        CreateLightPylon("InteriorPylon_A", new Vector3(-16f, 0f, -16f), trimMaterial, AccentColor, 9f, 2.8f);
        CreateLightPylon("InteriorPylon_B", new Vector3(16f, 0f, -16f), trimMaterial, AccentColor, 9f, 2.8f);

        CreateConsoleBank("ConsoleBank_A", new Vector3(-16.6f, 1.2f, 6f), 90f, panelMaterial, accentPanelMaterial);
        CreateConsoleBank("ConsoleBank_B", new Vector3(16.6f, 1.2f, -4f), -90f, panelMaterial, accentPanelMaterial);
        CreateConsoleBank("ConsoleBank_C", new Vector3(-6f, 1.2f, 16.6f), 180f, panelMaterial, accentPanelMaterial);

        CreatePipeRun("PipeRun_North", new Vector3(0f, 8.35f, 18.35f), new Vector3(30f, 0.34f, 0.34f), industrialMaterial);
        CreatePipeRun("PipeRun_South", new Vector3(0f, 8.35f, -18.35f), new Vector3(30f, 0.34f, 0.34f), industrialMaterial);
        CreatePipeRun("PipeRun_East", new Vector3(18.35f, 8.35f, 0f), new Vector3(0.34f, 0.34f, 30f), industrialMaterial);
        CreatePipeRun("PipeRun_West", new Vector3(-18.35f, 8.35f, 0f), new Vector3(0.34f, 0.34f, 30f), industrialMaterial);

        CreateServiceColumn("ServiceColumn_A", new Vector3(-17.4f, 3.2f, 14f), new Vector3(1.1f, 6.4f, 1.1f), industrialMaterial, accentPanelMaterial);
        CreateServiceColumn("ServiceColumn_B", new Vector3(17.4f, 3.2f, -14f), new Vector3(1.1f, 6.4f, 1.1f), industrialMaterial, accentPanelMaterial);
        CreateServiceColumn("ServiceColumn_C", new Vector3(17.4f, 3.2f, 14f), new Vector3(1.1f, 6.4f, 1.1f), industrialMaterial, accentPanelMaterial);

        CreateRaisedPlatform("ReactorDais", new Vector3(12f, 0.2f, 14f), new Vector3(13f, 0.4f, 13f), panelMaterial, trimMaterial);
        CreateRaisedPlatform("DoorThreshold", new Vector3(0f, 0.12f, 15.7f), new Vector3(7f, 0.24f, 4f), panelMaterial, accentPanelMaterial);
    }

    private static void CreateSciFiCorridorDecor()
    {
        const string prefabRoot = "Assets/Sci Fi Modular Pack/Prefabs";

        PlaceDecorPrefab("SciFiFloor_A", $"{prefabRoot}/Floor1.prefab", new Vector3(0f, 0.02f, -10f), Vector3.zero);
        PlaceDecorPrefab("SciFiFloor_B", $"{prefabRoot}/Floor2.prefab", new Vector3(0f, 0.02f, 0f), Vector3.zero);
        PlaceDecorPrefab("SciFiFloor_C", $"{prefabRoot}/Floor4.prefab", new Vector3(0f, 0.02f, 10f), Vector3.zero);
        PlaceDecorPrefab("SciFiSideFloor_L", $"{prefabRoot}/SideFloor1.prefab", new Vector3(-8.8f, 0.02f, 2f), Vector3.zero);
        PlaceDecorPrefab("SciFiSideFloor_R", $"{prefabRoot}/SideFloor2.prefab", new Vector3(8.8f, 0.02f, -2f), Vector3.zero);
        PlaceDecorPrefab("SciFiFloorNode", $"{prefabRoot}/IntersectionSideFloor.prefab", new Vector3(12f, 0.02f, 14f), Vector3.zero);

        PlaceDecorPrefab("SciFiNorthWall_A", $"{prefabRoot}/WallDoor.prefab", new Vector3(-10f, 0f, 19.35f), new Vector3(0f, 180f, 0f));
        PlaceDecorPrefab("SciFiNorthWall_B", $"{prefabRoot}/ProfileWindow.prefab", new Vector3(10f, 0f, 19.3f), new Vector3(0f, 180f, 0f));
        PlaceDecorPrefab("SciFiSouthWall_A", $"{prefabRoot}/Wall12.prefab", new Vector3(-10f, 0f, -19.3f), Vector3.zero);
        PlaceDecorPrefab("SciFiSouthWall_B", $"{prefabRoot}/WallIntersection.prefab", new Vector3(10f, 0f, -19.3f), Vector3.zero);
        PlaceDecorPrefab("SciFiEastWall_A", $"{prefabRoot}/SideWall1.1.prefab", new Vector3(19.2f, 0f, -10f), new Vector3(0f, -90f, 0f));
        PlaceDecorPrefab("SciFiEastWall_B", $"{prefabRoot}/ProfileWindow.prefab", new Vector3(19.15f, 0f, 10f), new Vector3(0f, -90f, 0f));
        PlaceDecorPrefab("SciFiWestWall_A", $"{prefabRoot}/SideWall1.1.prefab", new Vector3(-19.2f, 0f, -10f), new Vector3(0f, 90f, 0f));
        PlaceDecorPrefab("SciFiWestWall_B", $"{prefabRoot}/WallIntersection3.prefab", new Vector3(-19.15f, 0f, 10f), new Vector3(0f, 90f, 0f));

        PlaceDecorPrefab("SciFiCeiling_A", $"{prefabRoot}/TopWall4.prefab", new Vector3(0f, 8.55f, -8f), new Vector3(180f, 0f, 0f));
        PlaceDecorPrefab("SciFiCeiling_B", $"{prefabRoot}/TopWall4.prefab", new Vector3(0f, 8.55f, 8f), new Vector3(180f, 180f, 0f));
        PlaceDecorPrefab("SciFiLight_A", $"{prefabRoot}/Light1.prefab", new Vector3(-8f, 8.15f, 0f), new Vector3(180f, 0f, 0f));
        PlaceDecorPrefab("SciFiLight_B", $"{prefabRoot}/Light2.prefab", new Vector3(8f, 8.15f, 0f), new Vector3(180f, 180f, 0f));

        PlaceDecorPrefab("SciFiBroadcastDoorFrame", $"{prefabRoot}/DoorFrame.prefab", new Vector3(0f, 0f, 17.4f), Vector3.zero);
        PlaceDecorPrefab("SciFiChargeFrame", $"{prefabRoot}/Starter/ProfileDoor.prefab", new Vector3(12f, 0f, 12.1f), new Vector3(0f, 180f, 0f));
        PlaceDecorPrefab("SciFiCrate_A", $"{prefabRoot}/Box1.prefab", new Vector3(-14.5f, 0f, 13f), new Vector3(0f, 25f, 0f));
        PlaceDecorPrefab("SciFiCrate_B", $"{prefabRoot}/Box3.prefab", new Vector3(14.5f, 0f, -11f), new Vector3(0f, -20f, 0f));
        PlaceDecorPrefab("SciFiEndCap_A", $"{prefabRoot}/ProfileEnd.prefab", new Vector3(-6f, 0f, 17.6f), Vector3.zero);
        PlaceDecorPrefab("SciFiEndCap_B", $"{prefabRoot}/ProfileEnd.prefab", new Vector3(6f, 0f, 17.6f), new Vector3(0f, 180f, 0f));
    }

    private static void CreateIndoorFacilityLights()
    {
        Vector3[] ceilingLightPositions =
        {
            new Vector3(0f, 8.2f, 0f),
            new Vector3(-10f, 8.2f, -10f),
            new Vector3(10f, 8.2f, -10f),
            new Vector3(-10f, 8.2f, 10f),
            new Vector3(10f, 8.2f, 10f)
        };

        for (int i = 0; i < ceilingLightPositions.Length; i++)
        {
            Light ceilingLight = CreateLight(
                $"Indoor Ceiling Light {i + 1:00}",
                LightType.Point,
                ceilingLightPositions[i],
                new Color(0.68f, 0.88f, 1f),
                6.2f,
                15f,
                LightmapBakeType.Realtime);
            ceilingLight.shadows = LightShadows.None;
        }

        Light chargeLight = CreateLight("Charge Terminal Light", LightType.Spot, new Vector3(12f, 7.5f, 10.8f), new Color(0.35f, 0.92f, 1f), 10.5f, 22f, LightmapBakeType.Realtime);
        chargeLight.transform.rotation = Quaternion.Euler(72f, 180f, 0f);
        chargeLight.spotAngle = 44f;

        Light broadcastLight = CreateLight("Broadcast Terminal Light", LightType.Spot, new Vector3(0f, 8.2f, 14.5f), new Color(0.55f, 0.9f, 1f), 11f, 24f, LightmapBakeType.Realtime);
        broadcastLight.transform.rotation = Quaternion.Euler(74f, 180f, 0f);
        broadcastLight.spotAngle = 48f;

        Light reactorGlow = CreateLight("Reactor Fill Light", LightType.Point, new Vector3(12f, 4.8f, 14f), new Color(1f, 0.42f, 0.22f), 8.8f, 18f, LightmapBakeType.Realtime);
        reactorGlow.shadows = LightShadows.None;

        Light corridorFill = CreateLight("Indoor Corridor Fill", LightType.Spot, new Vector3(0f, 9f, -2f), new Color(0.92f, 0.96f, 1f), 9f, 30f, LightmapBakeType.Realtime);
        corridorFill.transform.rotation = Quaternion.Euler(82f, 180f, 0f);
        corridorFill.spotAngle = 60f;
    }

    private static void EnsureSciFiCorridorMaterialsCompatible()
    {
        const string materialsFolder = "Assets/Sci Fi Modular Pack/Materials";
        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null)
        {
            return;
        }

        string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { materialsFolder });
        foreach (string guid in materialGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Material material = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (material == null)
            {
                continue;
            }
            GetCompatibleImportedLitMaterial(assetPath);
        }
    }

    private static void CreateInsetWallPanel(string objectName, Vector3 position, Vector3 scale, Material material)
    {
        GameObject panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = objectName;
        panel.transform.position = position;
        panel.transform.localScale = scale;
        panel.GetComponent<MeshRenderer>().sharedMaterial = material;
        RemoveCollider(panel);
        MarkEnvironmentStatic(panel);
    }

    private static void CreateCeilingBeam(string objectName, Vector3 position, Vector3 scale, Material material)
    {
        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beam.name = objectName;
        beam.transform.position = position;
        beam.transform.localScale = scale;
        beam.GetComponent<MeshRenderer>().sharedMaterial = material;
        RemoveCollider(beam);
        MarkEnvironmentStatic(beam);
    }

    private static void CreateConsoleBank(string objectName, Vector3 position, float yaw, Material baseMaterial, Material accentMaterial)
    {
        Quaternion rotation = Quaternion.Euler(0f, yaw, 0f);
        CreateDecorBlock(objectName, position, new Vector3(3.2f, 2.4f, 1.2f), rotation, baseMaterial);
        CreateDecorBlock($"{objectName}_Display", position + rotation * new Vector3(0f, 0.58f, 0.63f), new Vector3(2.4f, 0.12f, 0.08f), rotation, accentMaterial);
        CreateDecorBlock($"{objectName}_Lower", position + rotation * new Vector3(0f, -0.18f, 0.63f), new Vector3(1.8f, 0.08f, 0.08f), rotation, accentMaterial);
    }

    private static void CreatePipeRun(string objectName, Vector3 position, Vector3 scale, Material material)
    {
        GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pipe.name = objectName;
        pipe.transform.position = position;
        pipe.transform.localScale = scale;
        pipe.GetComponent<MeshRenderer>().sharedMaterial = material;
        RemoveCollider(pipe);
        MarkEnvironmentStatic(pipe);
    }

    private static void CreateServiceColumn(string objectName, Vector3 position, Vector3 scale, Material baseMaterial, Material accentMaterial)
    {
        GameObject column = GameObject.CreatePrimitive(PrimitiveType.Cube);
        column.name = objectName;
        column.transform.position = position;
        column.transform.localScale = scale;
        column.GetComponent<MeshRenderer>().sharedMaterial = baseMaterial;
        RemoveCollider(column);
        MarkEnvironmentStatic(column);

        CreateLightStrip($"{objectName}_Accent", position + new Vector3(0f, scale.y * 0.28f, 0.58f), new Vector3(scale.x * 0.44f, 0.08f, 0.08f), accentMaterial);
    }

    private static void CreateRaisedPlatform(string objectName, Vector3 position, Vector3 scale, Material baseMaterial, Material accentMaterial)
    {
        GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platform.name = objectName;
        platform.transform.position = position;
        platform.transform.localScale = scale;
        platform.GetComponent<MeshRenderer>().sharedMaterial = baseMaterial;
        RemoveCollider(platform);
        MarkEnvironmentStatic(platform);

        CreateLightStrip($"{objectName}_AccentX", position + new Vector3(0f, scale.y * 0.52f, 0f), new Vector3(scale.x * 0.78f, 0.06f, 0.12f), accentMaterial);
        CreateLightStrip($"{objectName}_AccentZ", position + new Vector3(0f, scale.y * 0.52f, 0f), new Vector3(0.12f, 0.06f, scale.z * 0.78f), accentMaterial);
    }

    private static void CreateDecorBlock(string objectName, Vector3 position, Vector3 scale, Quaternion rotation, Material material)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = objectName;
        block.transform.position = position;
        block.transform.rotation = rotation;
        block.transform.localScale = scale;
        block.GetComponent<MeshRenderer>().sharedMaterial = material;
        RemoveCollider(block);
        MarkEnvironmentStatic(block);
    }

    private static void CreateCollectible(string objectName, Vector3 position, Material material)
    {
        GameObject collectible = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        collectible.name = objectName;
        collectible.transform.position = position;
        collectible.transform.localScale = Vector3.one * 1.2f;
        collectible.GetComponent<MeshRenderer>().sharedMaterial = material;
        Collectible collectibleComponent = collectible.AddComponent<Collectible>();
        SerializedObject collectibleData = new SerializedObject(collectibleComponent);
        collectibleData.FindProperty("collectibleId").stringValue = objectName;
        collectibleData.ApplyModifiedPropertiesWithoutUndo();

        ParticleSystem particles = collectible.AddComponent<ParticleSystem>();
        var main = particles.main;
        main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.5f, 1f, 1f, 0.9f));
        main.startLifetime = 0.6f;
        main.startSpeed = 0.2f;
        main.startSize = 0.18f;
        main.maxParticles = 35;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = particles.emission;
        emission.rateOverTime = 18f;

        var shape = particles.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.45f;

        ParticleSystemRenderer particleRenderer = collectible.GetComponent<ParticleSystemRenderer>();
        particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
        particleRenderer.sharedMaterial = GetOrCreateCollectibleParticleMaterial();
        particleRenderer.shadowCastingMode = ShadowCastingMode.Off;
        particleRenderer.receiveShadows = false;
    }

    private static void CreateOutdoorObstacle(string objectName, string prefabPath, Vector3 position, Vector3 rotation)
    {
        PlacePrefab(objectName, prefabPath, position, rotation);
    }

    private static void CreateLandingPad(string objectName, Vector3 position, Vector3 scale, Material baseMaterial, Material trimMaterial)
    {
        GameObject pad = GameObject.CreatePrimitive(PrimitiveType.Cube);
        pad.name = objectName;
        pad.transform.position = position;
        pad.transform.localScale = scale;
        pad.GetComponent<MeshRenderer>().sharedMaterial = baseMaterial;
        RemoveCollider(pad);
        MarkEnvironmentStatic(pad);

        CreateLightStrip($"{objectName}_TrimX", position + new Vector3(0f, 0.2f, 0f), new Vector3(scale.x * 0.82f, 0.05f, 0.18f), trimMaterial);
        CreateLightStrip($"{objectName}_TrimZ", position + new Vector3(0f, 0.2f, 0f), new Vector3(0.18f, 0.05f, scale.z * 0.82f), trimMaterial);
    }

    private static void CreateLightPylon(string objectName, Vector3 position, Material structureMaterial, Color lightColor, float lightRange, float height, bool solid = false)
    {
        GameObject pylon = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        pylon.name = objectName;
        pylon.transform.position = position + new Vector3(0f, height * 0.5f, 0f);
        pylon.transform.localScale = new Vector3(0.35f, height * 0.5f, 0.35f);
        pylon.GetComponent<MeshRenderer>().sharedMaterial = structureMaterial;
        if (!solid)
        {
            RemoveCollider(pylon);
        }
        MarkEnvironmentStatic(pylon);

        GameObject cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cap.name = $"{objectName}_Cap";
        cap.transform.position = position + new Vector3(0f, height + 0.2f, 0f);
        cap.transform.localScale = new Vector3(0.8f, 0.18f, 0.8f);
        cap.GetComponent<MeshRenderer>().sharedMaterial = structureMaterial;
        if (!solid)
        {
            RemoveCollider(cap);
        }
        MarkEnvironmentStatic(cap);

        Light beacon = CreateLight($"{objectName}_Light", LightType.Point, position + new Vector3(0f, height + 0.35f, 0f), lightColor, 3.3f, lightRange, LightmapBakeType.Realtime);
        beacon.shadows = LightShadows.None;
    }

    private static void CreateWarningBeacon(string objectName, Vector3 position, Material warningMaterial, float lightRange)
    {
        GameObject beacon = GameObject.CreatePrimitive(PrimitiveType.Cube);
        beacon.name = objectName;
        beacon.transform.position = position + new Vector3(0f, 1.8f, 0f);
        beacon.transform.localScale = new Vector3(0.8f, 3.6f, 0.8f);
        beacon.GetComponent<MeshRenderer>().sharedMaterial = warningMaterial;
        RemoveCollider(beacon);
        MarkEnvironmentStatic(beacon);

        Light warningLight = CreateLight($"{objectName}_Light", LightType.Point, position + new Vector3(0f, 3.8f, 0f), WarningColor, 4.2f, lightRange, LightmapBakeType.Realtime);
        warningLight.shadows = LightShadows.None;
    }

    private static void CreateHazardFrame(string objectName, Vector3 position, Vector3 outerScale, Material frameMaterial, Material warningMaterial)
    {
        CreateLightStrip($"{objectName}_North", position + new Vector3(0f, 0f, outerScale.z * 0.5f), new Vector3(outerScale.x, 0.1f, 0.25f), frameMaterial);
        CreateLightStrip($"{objectName}_South", position + new Vector3(0f, 0f, -outerScale.z * 0.5f), new Vector3(outerScale.x, 0.1f, 0.25f), frameMaterial);
        CreateLightStrip($"{objectName}_East", position + new Vector3(outerScale.x * 0.5f, 0f, 0f), new Vector3(0.25f, 0.1f, outerScale.z), frameMaterial);
        CreateLightStrip($"{objectName}_West", position + new Vector3(-outerScale.x * 0.5f, 0f, 0f), new Vector3(0.25f, 0.1f, outerScale.z), frameMaterial);

        CreateWarningBeacon($"{objectName}_WarningA", position + new Vector3(outerScale.x * 0.5f, 0f, outerScale.z * 0.5f), warningMaterial, 9f);
        CreateWarningBeacon($"{objectName}_WarningB", position + new Vector3(-outerScale.x * 0.5f, 0f, outerScale.z * 0.5f), warningMaterial, 9f);
    }

    private static void CreateDoorFrameLights(string objectName, Vector3 doorPosition, Material trimMaterial, Color lightColor)
    {
        CreateLightStrip($"{objectName}_Top", doorPosition + new Vector3(0f, 3.4f, 0.58f), new Vector3(4.8f, 0.16f, 0.16f), trimMaterial);
        CreateLightStrip($"{objectName}_Left", doorPosition + new Vector3(-2.3f, 0.2f, 0.58f), new Vector3(0.16f, 6.2f, 0.16f), trimMaterial);
        CreateLightStrip($"{objectName}_Right", doorPosition + new Vector3(2.3f, 0.2f, 0.58f), new Vector3(0.16f, 6.2f, 0.16f), trimMaterial);

        Light doorLight = CreateLight($"{objectName}_Glow", LightType.Point, doorPosition + new Vector3(0f, 3.6f, -0.5f), lightColor, 4.6f, 10f, LightmapBakeType.Realtime);
        doorLight.shadows = LightShadows.None;
    }

    private static void CreateLightStrip(string objectName, Vector3 position, Vector3 scale, Material material)
    {
        GameObject strip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        strip.name = objectName;
        strip.transform.position = position;
        strip.transform.localScale = scale;
        MeshRenderer renderer = strip.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;
        RemoveCollider(strip);
        MarkEnvironmentStatic(strip);
    }

    private static void PlacePrefab(string objectName, string prefabPath, Vector3 position, Vector3 rotation)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = objectName;
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(rotation);
        ApplyCompatibleGeneratedMaterialsRecursive(instance);
    }

    private static void PlaceDecorPrefab(string objectName, string prefabPath, Vector3 position, Vector3 rotation)
    {
        PlaceDecorPrefab(objectName, prefabPath, position, rotation, Vector3.one);
    }

    private static void PlaceDecorPrefab(string objectName, string prefabPath, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = objectName;
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(rotation);
        instance.transform.localScale = scale;
        ApplyCompatibleGeneratedMaterialsRecursive(instance);
        RemoveCollidersRecursive(instance);
        MarkEnvironmentStatic(instance);
    }

    private static void AttachDoorVisual(Transform parent, string prefabPath, Vector3 localPosition, Vector3 localEulerAngles, Vector3 localScale)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = $"{parent.name}_Visual";
        instance.transform.SetParent(parent, false);
        instance.transform.localPosition = localPosition;
        instance.transform.localRotation = Quaternion.Euler(localEulerAngles);
        instance.transform.localScale = localScale;
        ApplyCompatibleGeneratedMaterialsRecursive(instance);
        RemoveCollidersRecursive(instance);
    }

    private static TerrainData GetOrCreateTerrainData()
    {
        TerrainData terrainData = AssetDatabase.LoadAssetAtPath<TerrainData>("Assets/New Terrain.asset");
        if (terrainData == null)
        {
            terrainData = new TerrainData();
            AssetDatabase.CreateAsset(terrainData, "Assets/New Terrain.asset");
        }

        terrainData.heightmapResolution = Mathf.Max(terrainData.heightmapResolution, 257);
        terrainData.size = new Vector3(120f, 42f, 120f);
        terrainData.baseMapResolution = 1024;
        terrainData.alphamapResolution = 512;
        return terrainData;
    }

    private static TerrainLayer[] GetOutdoorTerrainLayers()
    {
        List<TerrainLayer> layers = new List<TerrainLayer>(LunarTerrainLayerPaths.Length);
        for (int i = 0; i < LunarTerrainLayerPaths.Length; i++)
        {
            TerrainLayer layer = AssetDatabase.LoadAssetAtPath<TerrainLayer>(LunarTerrainLayerPaths[i]);
            if (layer != null)
            {
                layers.Add(layer);
            }
        }

        if (layers.Count > 0)
        {
            return layers.ToArray();
        }

        return new[] { GetOrCreateFallbackTerrainLayer() };
    }

    private static TerrainLayer GetOrCreateFallbackTerrainLayer()
    {
        TerrainLayer terrainLayer = AssetDatabase.LoadAssetAtPath<TerrainLayer>($"{TerrainLayersFolder}/BaseGround.terrainlayer");
        Texture2D diffuse = GetOrCreateMoonGroundTexture();

        if (terrainLayer == null)
        {
            terrainLayer = new TerrainLayer();
            AssetDatabase.CreateAsset(terrainLayer, $"{TerrainLayersFolder}/BaseGround.terrainlayer");
        }

        terrainLayer.diffuseTexture = diffuse;
        terrainLayer.normalMapTexture = null;
        terrainLayer.tileSize = new Vector2(32f, 32f);
        terrainLayer.normalScale = 0f;
        terrainLayer.metallic = 0f;
        terrainLayer.smoothness = 0.015f;
        terrainLayer.diffuseRemapMin = Vector4.zero;
        terrainLayer.diffuseRemapMax = Vector4.one;
        terrainLayer.maskMapRemapMin = Vector4.zero;
        terrainLayer.maskMapRemapMax = Vector4.one;
        EditorUtility.SetDirty(terrainLayer);
        return terrainLayer;
    }

    private static Material GetOutdoorKillzoneMaterial()
    {
        Material freeFireMaterial = GetFreeFireLavaMaterial();
        if (freeFireMaterial != null)
        {
            return freeFireMaterial;
        }

        const string path = GeneratedMaterialsFolder + "/LavaKillzone.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader defaultShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
            material = new Material(defaultShader)
            {
                name = "LavaKillzone"
            };
            AssetDatabase.CreateAsset(material, path);
        }

        Texture2D lavaTexture = GetOrCreateLavaKillzoneTexture();
        Shader unlitShader = Shader.Find("Universal Render Pipeline/Unlit") ?? Shader.Find("Unlit/Texture");
        if (unlitShader != null && material.shader != unlitShader)
        {
            material.shader = unlitShader;
        }

        material.SetTexture("_BaseMap", lavaTexture);
        material.SetTexture("_MainTex", lavaTexture);
        material.SetTextureScale("_BaseMap", new Vector2(2.5f, 2.5f));
        material.SetColor("_BaseColor", Color.white);
        material.SetColor("_Color", Color.white);

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 0f);
        }

        if (material.HasProperty("_Cull"))
        {
            material.SetFloat("_Cull", 0f);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material GetFreeFireLavaMaterial()
    {
        foreach (string materialPath in FreeFireLavaMaterialPaths)
        {
            Material lavaMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (lavaMaterial != null)
            {
                return lavaMaterial;
            }
        }

        string[] materialGuids = AssetDatabase.FindAssets("M_VFX_Fire t:Material", new[] { FreeFireVfxMaterialsFolder });
        foreach (string guid in materialGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            Material lavaMaterial = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            if (lavaMaterial != null)
            {
                return lavaMaterial;
            }
        }

        return null;
    }

    private static GameObject GetFreeFireLavaPrefab()
    {
        foreach (string prefabPath in FreeFireLavaPrefabPaths)
        {
            GameObject lavaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (lavaPrefab != null)
            {
                return lavaPrefab;
            }
        }

        string[] prefabGuids = AssetDatabase.FindAssets("VFX_Fire_Floor t:Prefab", new[] { "Assets/Vefects/Free Fire VFX URP/Particles" });
        foreach (string guid in prefabGuids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            GameObject lavaPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (lavaPrefab != null)
            {
                return lavaPrefab;
            }
        }

        return null;
    }

    private static Material GetOutdoorRockMaterial()
    {
        Material stylizedColdLava = GetCompatibleImportedLitMaterial(StylizedColdLavaMaterialPath);
        if (stylizedColdLava != null)
        {
            Material generatedMoonRock = GetOrCreateGeneratedMaterialCopy($"{GeneratedMaterialsFolder}/MoonRock.mat", "MoonRock", stylizedColdLava);
            if (generatedMoonRock != null && generatedMoonRock.HasProperty("_EmissionColor"))
            {
                generatedMoonRock.SetColor("_EmissionColor", Color.black);
                EditorUtility.SetDirty(generatedMoonRock);
            }

            return generatedMoonRock;
        }

        Material importedRockMaterial = GetCompatibleImportedLitMaterial(LunarRockMaterialPath);
        if (importedRockMaterial != null)
        {
            return GetOrCreateGeneratedMaterialCopy($"{GeneratedMaterialsFolder}/MoonRock.mat", "MoonRock", importedRockMaterial);
        }

        return GetOrCreateLitMaterial($"{GeneratedMaterialsFolder}/MoonRock.mat", new Color(0.28f, 0.3f, 0.33f), new Color(0f, 0f, 0f), 0.04f);
    }

    private static Material GetOutdoorWallRockMaterial()
    {
        Material stylizedWallRock = GetCompatibleImportedLitMaterial(StylizedWallRockMaterialPath);
        if (stylizedWallRock != null)
        {
            return GetOrCreateGeneratedMaterialCopy($"{GeneratedMaterialsFolder}/WallRock.mat", "WallRock", stylizedWallRock);
        }

        return GetOutdoorRockMaterial();
    }

    private static Material GetOrCreateCollectibleParticleMaterial()
    {
        const string path = GeneratedMaterialsFolder + "/CollectibleParticles.mat";
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Particles/Unlit")
                         ?? Shader.Find("Particles/Standard Unlit")
                         ?? Shader.Find("Sprites/Default");
            if (shader == null)
            {
                return null;
            }

            material = new Material(shader)
            {
                name = "CollectibleParticles"
            };
            AssetDatabase.CreateAsset(material, path);
        }

        Texture2D texture = GetOrCreateCollectibleParticleTexture();
        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);
        }

        if (material.HasProperty("_MainTex"))
        {
            material.SetTexture("_MainTex", texture);
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", Color.white);
        }

        if (material.HasProperty("_Color"))
        {
            material.SetColor("_Color", Color.white);
        }

        if (material.HasProperty("_Surface"))
        {
            material.SetFloat("_Surface", 1f);
        }

        if (material.HasProperty("_Blend"))
        {
            material.SetFloat("_Blend", 0f);
        }

        if (material.HasProperty("_Cull"))
        {
            material.SetFloat("_Cull", 0f);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static Texture2D GetOrCreateMoonGroundTexture()
    {
        const string texturePath = TerrainTexturesFolder + "/MoonGround.asset";

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture == null)
        {
            texture = new Texture2D(128, 128, TextureFormat.RGBA32, false)
            {
                name = "MoonGround",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            AssetDatabase.CreateAsset(texture, texturePath);
        }

        Color[] pixels = new Color[texture.width * texture.height];
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float nx = x / (float)(texture.width - 1);
                float ny = y / (float)(texture.height - 1);
                float broad = Mathf.PerlinNoise(nx * 3.4f + 0.11f, ny * 3.1f + 0.27f);
                float medium = Mathf.PerlinNoise(nx * 8.9f + 1.9f, ny * 9.6f + 2.3f);
                float fine = Mathf.PerlinNoise(nx * 23.4f + 5.1f, ny * 21.7f + 3.8f);
                float crater = Mathf.PerlinNoise(nx * 15.2f + 4.2f, ny * 14.7f + 6.6f);

                float value = 0.42f;
                value += (broad - 0.5f) * 0.22f;
                value += (medium - 0.5f) * 0.14f;
                value += (fine - 0.5f) * 0.06f;
                value -= Mathf.Max(0f, crater - 0.68f) * 0.18f;
                value = Mathf.Clamp01(value);

                Color darkRock = new Color(0.16f, 0.17f, 0.2f, 1f);
                Color lightDust = new Color(0.39f, 0.4f, 0.44f, 1f);
                pixels[y * texture.width + x] = Color.Lerp(darkRock, lightDust, value);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        EditorUtility.SetDirty(texture);
        return texture;
    }

    private static Texture2D GetOrCreateLavaKillzoneTexture()
    {
        const string texturePath = TerrainTexturesFolder + "/LavaKillzone.asset";

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture == null)
        {
            texture = new Texture2D(128, 128, TextureFormat.RGBA32, false)
            {
                name = "LavaKillzone",
                wrapMode = TextureWrapMode.Repeat,
                filterMode = FilterMode.Bilinear
            };
            AssetDatabase.CreateAsset(texture, texturePath);
        }

        Color[] pixels = new Color[texture.width * texture.height];
        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float nx = x / (float)(texture.width - 1);
                float ny = y / (float)(texture.height - 1);
                float broad = Mathf.PerlinNoise(nx * 3.8f + 0.17f, ny * 3.5f + 0.29f);
                float medium = Mathf.PerlinNoise(nx * 9.6f + 1.7f, ny * 8.8f + 2.1f);
                float fine = Mathf.PerlinNoise(nx * 24.4f + 4.6f, ny * 23.1f + 3.2f);
                float crackMask = Mathf.Abs(medium - 0.5f) * 1.25f + Mathf.Abs(fine - 0.5f) * 0.45f;
                float glow = 0.62f + (broad - 0.5f) * 0.5f - crackMask * 0.42f;
                glow = Mathf.Clamp01(glow);
                glow = Mathf.SmoothStep(0.08f, 1f, glow);

                Color crust = new Color(0.08f, 0.03f, 0.015f, 1f);
                Color ember = new Color(0.95f, 0.2f, 0.03f, 1f);
                Color core = new Color(1f, 0.9f, 0.28f, 1f);
                Color hotLava = Color.Lerp(ember, core, glow * 0.82f);
                pixels[y * texture.width + x] = Color.Lerp(crust, hotLava, Mathf.Lerp(0.25f, 1f, glow));
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        EditorUtility.SetDirty(texture);
        return texture;
    }

    private static Texture2D GetOrCreateCollectibleParticleTexture()
    {
        const string texturePath = TerrainTexturesFolder + "/CollectibleParticle.asset";

        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture == null)
        {
            texture = new Texture2D(64, 64, TextureFormat.RGBA32, false)
            {
                name = "CollectibleParticle",
                wrapMode = TextureWrapMode.Clamp,
                filterMode = FilterMode.Bilinear
            };
            AssetDatabase.CreateAsset(texture, texturePath);
        }

        Color[] pixels = new Color[texture.width * texture.height];
        Vector2 center = new Vector2((texture.width - 1) * 0.5f, (texture.height - 1) * 0.5f);
        float radius = texture.width * 0.5f;

        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / radius;
                float alpha = Mathf.Clamp01(1f - distance);
                alpha = Mathf.SmoothStep(0f, 1f, alpha);
                alpha *= alpha;

                Color edge = new Color(0.25f, 0.95f, 1f, 0f);
                Color core = new Color(0.9f, 1f, 1f, 1f);
                pixels[y * texture.width + x] = Color.Lerp(edge, core, alpha);
                pixels[y * texture.width + x].a = alpha;
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        EditorUtility.SetDirty(texture);
        return texture;
    }

    private static void ShapeOutdoorTerrain(TerrainData terrainData)
    {
        int resolution = terrainData.heightmapResolution;
        float[,] heights = new float[resolution, resolution];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float nx = x / (float)(resolution - 1);
                float nz = z / (float)(resolution - 1);

                float warpX = Mathf.PerlinNoise(nx * 1.7f + 7.3f, nz * 1.9f + 4.1f) - 0.5f;
                float warpZ = Mathf.PerlinNoise(nx * 1.9f + 2.8f, nz * 1.6f + 9.4f) - 0.5f;
                float wx = nx + warpX * 0.075f;
                float wz = nz + warpZ * 0.075f;

                float broadNoise = Mathf.PerlinNoise(wx * 1.7f + 0.17f, wz * 1.7f + 0.31f);
                float mediumNoise = Mathf.PerlinNoise(wx * 4.9f + 1.4f, wz * 4.5f + 2.2f);
                float fineNoise = Mathf.PerlinNoise(wx * 12.4f + 3.6f, wz * 11.7f + 1.1f);

                float height = 0.033f;
                height += (broadNoise - 0.5f) * 0.13f;
                height += (mediumNoise - 0.5f) * 0.06f;
                height += (fineNoise - 0.5f) * 0.025f;
                height += CreateCraterField(wx, wz) * 1.4f;

                height -= CreateDepression(wx, wz, 0.52f, 0.5f, 0.24f, 0.068f);
                height -= CreateDepression(wx, wz, 0.26f, 0.68f, 0.13f, 0.035f);
                height -= CreateDepression(wx, wz, 0.78f, 0.28f, 0.13f, 0.03f);
                height -= CreateDepression(wx, wz, 0.86f, 0.62f, 0.11f, 0.027f);
                height += CreateHill(wx, wz, 0.15f, 0.84f, 0.2f, 0.042f);
                height += CreateHill(wx, wz, 0.82f, 0.18f, 0.18f, 0.036f);
                height += CreateHill(wx, wz, 0.72f, 0.78f, 0.16f, 0.03f);
                height += CreateRidge(wx, wz, new Vector2(0.06f, 0.62f), new Vector2(0.34f, 0.96f), 0.085f, 0.022f);
                height += CreateRidge(wx, wz, new Vector2(0.78f, 0.06f), new Vector2(1.0f, 0.34f), 0.08f, 0.02f);

                height = FlattenArea(height, nx, nz, new Vector2(0.1f, 0.1f), 0.06f, 0.034f);
                height = FlattenArea(height, nx, nz, new Vector2(0.85f, 0.8f), 0.075f, 0.036f);
                height = FlattenPath(height, nx, nz);

                heights[z, x] = Mathf.Clamp01(height);
            }
        }

        terrainData.SetHeights(0, 0, heights);
    }

    private static void PaintOutdoorTerrain(TerrainData terrainData, int layerCount)
    {
        if (layerCount <= 0)
        {
            return;
        }

        int resolution = terrainData.alphamapResolution;
        float[,,] alphamaps = new float[resolution, resolution, layerCount];

        for (int z = 0; z < resolution; z++)
        {
            for (int x = 0; x < resolution; x++)
            {
                float nx = x / (float)(resolution - 1);
                float nz = z / (float)(resolution - 1);
                float terrainHeight = terrainData.GetInterpolatedHeight(nx, nz) / terrainData.size.y;
                float steepness = terrainData.GetSteepness(nx, nz) / 90f;

                float broadNoise = Mathf.PerlinNoise(nx * 2.4f + 0.12f, nz * 2.1f + 0.44f);
                float detailNoise = Mathf.PerlinNoise(nx * 8.3f + 1.8f, nz * 8.9f + 3.2f);
                float craterNoise = Mathf.PerlinNoise(nx * 5.2f + 4.3f, nz * 5.7f + 0.9f);

                float[] weights = new float[layerCount];
                if (layerCount == 1)
                {
                    weights[0] = 1f;
                }
                else if (layerCount == 2)
                {
                    weights[0] = Mathf.Clamp01(0.72f - steepness * 0.45f + (broadNoise - 0.5f) * 0.18f);
                    weights[1] = Mathf.Clamp01(0.28f + steepness * 0.65f + Mathf.Abs(detailNoise - 0.5f) * 0.35f);
                }
                else if (layerCount == 3)
                {
                    weights[0] = Mathf.Clamp01(0.5f - steepness * 0.28f + (broadNoise - 0.5f) * 0.15f);
                    weights[1] = Mathf.Clamp01(0.3f + steepness * 0.5f + Mathf.Abs(detailNoise - 0.5f) * 0.26f);
                    weights[2] = Mathf.Clamp01(0.18f + Mathf.Max(0f, craterNoise - 0.58f) * 1.45f + terrainHeight * 0.2f);
                }
                else
                {
                    // Blend the imported lunar terrain layers by slope, crater noise, and lighter dust deposits.
                    weights[0] = Mathf.Clamp01(0.38f + (1f - steepness) * 0.24f + (broadNoise - 0.5f) * 0.16f);
                    weights[1] = Mathf.Clamp01(0.22f + steepness * 0.62f + Mathf.Abs(detailNoise - 0.5f) * 0.42f);
                    weights[2] = Mathf.Clamp01(0.16f + Mathf.Max(0f, craterNoise - 0.56f) * 1.55f + terrainHeight * 0.18f);
                    weights[3] = Mathf.Clamp01(0.14f + Mathf.Max(0f, broadNoise - 0.62f) * 0.9f + Mathf.Max(0f, 0.07f - terrainHeight) * 2.1f);
                }

                float sum = 0f;
                for (int i = 0; i < layerCount; i++)
                {
                    sum += weights[i];
                }

                if (sum <= Mathf.Epsilon)
                {
                    weights[0] = 1f;
                    sum = 1f;
                }

                for (int i = 0; i < layerCount; i++)
                {
                    alphamaps[z, x, i] = weights[i] / sum;
                }
            }
        }

        terrainData.SetAlphamaps(0, 0, alphamaps);
    }

    private static float CreateHill(float x, float z, float centerX, float centerZ, float radius, float height)
    {
        float distance = Vector2.Distance(new Vector2(x, z), new Vector2(centerX, centerZ));
        if (distance >= radius)
        {
            return 0f;
        }

        float falloff = 1f - distance / radius;
        return falloff * falloff * height;
    }

    private static float CreateDepression(float x, float z, float centerX, float centerZ, float radius, float depth)
    {
        return CreateHill(x, z, centerX, centerZ, radius, depth);
    }

    private static float CreateRidge(float x, float z, Vector2 start, Vector2 end, float radius, float height)
    {
        return ComputePathInfluence(new Vector2(x, z), start, end, radius) * height;
    }

    private static float CreateCraterField(float x, float z)
    {
        float value = 0f;

        for (int i = 0; i < 34; i++)
        {
            float seed = i * 17.137f;
            float centerX = Hash01(seed + 0.11f);
            float centerZ = Hash01(seed + 3.73f);
            float radius = Mathf.Lerp(0.045f, 0.16f, Hash01(seed + 7.29f));
            float depth = Mathf.Lerp(0.008f, 0.03f, Hash01(seed + 9.83f));
            float rimHeight = depth * Mathf.Lerp(0.55f, 0.95f, Hash01(seed + 12.41f));
            value += CreateCrater(x, z, centerX, centerZ, radius, depth, rimHeight);
        }

        return value;
    }

    private static float CreateCrater(float x, float z, float centerX, float centerZ, float radius, float depth, float rimHeight)
    {
        float distance = Vector2.Distance(new Vector2(x, z), new Vector2(centerX, centerZ));
        float normalized = distance / Mathf.Max(radius, 0.0001f);

        if (normalized >= 1.9f)
        {
            return 0f;
        }

        float bowl = normalized < 1f ? -Mathf.Pow(1f - normalized, 1.6f) * depth : 0f;
        float rim = Mathf.Exp(-Mathf.Pow((normalized - 1.05f) * 3.3f, 2f)) * rimHeight;
        float ejectaRing = Mathf.Exp(-Mathf.Pow((normalized - 1.48f) * 3.9f, 2f)) * rimHeight * 0.24f;
        return bowl + rim + ejectaRing;
    }

    private static float Hash01(float seed)
    {
        return Mathf.Repeat(Mathf.Sin(seed * 12.9898f + 78.233f) * 43758.5453f, 1f);
    }

    private static float FlattenArea(float currentHeight, float x, float z, Vector2 center, float radius, float targetHeight)
    {
        float distance = Vector2.Distance(new Vector2(x, z), center);
        if (distance >= radius)
        {
            return currentHeight;
        }

        float blend = 1f - distance / radius;
        blend = Mathf.SmoothStep(0f, 1f, blend);
        return Mathf.Lerp(currentHeight, targetHeight, blend);
    }

    private static float FlattenPath(float currentHeight, float x, float z)
    {
        Vector2 pathStart = new Vector2(0.1f, 0.1f);
        Vector2 pathMid = new Vector2(0.42f, 0.32f);
        Vector2 pathEnd = new Vector2(0.85f, 0.8f);

        float influence = Mathf.Max(
            ComputePathInfluence(new Vector2(x, z), pathStart, pathMid, 0.055f),
            ComputePathInfluence(new Vector2(x, z), pathMid, pathEnd, 0.06f));

        if (influence <= 0f)
        {
            return currentHeight;
        }

        return Mathf.Lerp(currentHeight, 0.032f, influence * 0.72f);
    }

    private static void CreateRockOutcrop(string objectName, Vector3 position, Vector3 scale, Material material, float yaw)
    {
        GameObject baseRock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        baseRock.name = objectName;
        baseRock.transform.position = position;
        baseRock.transform.rotation = Quaternion.Euler(0f, yaw, -12f);
        baseRock.transform.localScale = scale;
        baseRock.GetComponent<MeshRenderer>().sharedMaterial = material;
        RemoveCollider(baseRock);
        MarkEnvironmentStatic(baseRock);

        GameObject secondaryRock = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        secondaryRock.name = $"{objectName}_Secondary";
        secondaryRock.transform.position = position + new Vector3(scale.x * 0.24f, scale.y * 0.06f, scale.z * 0.18f);
        secondaryRock.transform.rotation = Quaternion.Euler(18f, yaw + 28f, 22f);
        secondaryRock.transform.localScale = scale * 0.58f;
        secondaryRock.GetComponent<MeshRenderer>().sharedMaterial = material;
        RemoveCollider(secondaryRock);
        MarkEnvironmentStatic(secondaryRock);

        GameObject shard = GameObject.CreatePrimitive(PrimitiveType.Cube);
        shard.name = $"{objectName}_Shard";
        shard.transform.position = position + new Vector3(-scale.x * 0.18f, scale.y * 0.18f, scale.z * 0.1f);
        shard.transform.rotation = Quaternion.Euler(16f, yaw - 12f, 31f);
        shard.transform.localScale = new Vector3(scale.x * 0.36f, scale.y * 0.42f, scale.z * 0.24f);
        shard.GetComponent<MeshRenderer>().sharedMaterial = material;
        RemoveCollider(shard);
        MarkEnvironmentStatic(shard);
    }

    private static void CreateRockEntranceFacade(Terrain terrain, string objectName, Vector3 doorwayCenter, Material material)
    {
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_BackMass_A", doorwayCenter, new Vector3(-9.5f, 1.1f, 6.4f), 0, 3.6f, material);
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_BackMass_B", doorwayCenter, new Vector3(0f, 1.45f, 7.1f), 2, 4.0f, material);
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_BackMass_C", doorwayCenter, new Vector3(9.5f, 1.15f, 6.4f), 4, 3.7f, material);

        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_LeftToe", doorwayCenter, new Vector3(-14.6f, 0.55f, 3.9f), 1, 3.1f, material);
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_RightToe", doorwayCenter, new Vector3(14.6f, 0.55f, 3.9f), 5, 3.1f, material);
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_CenterApron", doorwayCenter, new Vector3(0f, 0.7f, 8.4f), 2, 3.2f, material);
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_LeftApron", doorwayCenter, new Vector3(-7.2f, 0.65f, 8.1f), 0, 2.9f, material);
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_RightApron", doorwayCenter, new Vector3(7.2f, 0.65f, 8.1f), 4, 2.9f, material);

        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_LeftWall_A", doorwayCenter, new Vector3(-12.6f, 1.3f, 2.8f), 1, 3.5f, material);
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_LeftWall_B", doorwayCenter, new Vector3(-10.8f, 3.2f, 3.8f), 3, 3.0f, material);
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_RightWall_A", doorwayCenter, new Vector3(12.6f, 1.3f, 2.8f), 5, 3.5f, material);
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_RightWall_B", doorwayCenter, new Vector3(10.8f, 3.2f, 3.8f), 0, 3.0f, material);

        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_Lintel_A", doorwayCenter, new Vector3(-5.1f, 5.3f, 3.8f), 2, 2.9f, material);
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_Lintel_B", doorwayCenter, new Vector3(0f, 5.8f, 4.05f), 4, 3.15f, material);
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_Lintel_C", doorwayCenter, new Vector3(5.1f, 5.3f, 3.8f), 1, 2.9f, material);

        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_LeftShoulder", doorwayCenter, new Vector3(-14.2f, 2.1f, 1.3f), 3, 3.45f, material);
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_RightShoulder", doorwayCenter, new Vector3(14.2f, 2.1f, 1.3f), 5, 3.45f, material);
        CreateGroundedEntranceRockCluster(terrain, $"{objectName}_TopCrown", doorwayCenter, new Vector3(0f, 6.6f, 4.8f), 0, 3.2f, material);
    }

    private static void CreateGroundedEntranceRockCluster(
        Terrain terrain,
        string objectName,
        Vector3 doorwayCenter,
        Vector3 localOffset,
        int variantOffset,
        float scale,
        Material material)
    {
        Vector3 supportPoint = doorwayCenter + new Vector3(localOffset.x, 0f, localOffset.z);
        float groundHeight = SampleTerrainHeight(terrain, supportPoint);
        Vector3 center = new Vector3(supportPoint.x, groundHeight + localOffset.y, supportPoint.z);
        CreateEntranceRockCluster(objectName, center, variantOffset, scale, material);
    }

    private static void CreateEntranceRockCluster(string objectName, Vector3 center, int variantOffset, float scale, Material material)
    {
        string basePrefab = StylizedWallRockPrefabPaths[Mathf.Abs(variantOffset) % StylizedWallRockPrefabPaths.Length];
        string sidePrefab = StylizedWallRockPrefabPaths[(Mathf.Abs(variantOffset) + 2) % StylizedWallRockPrefabPaths.Length];
        string crownPrefab = StylizedWallRockPrefabPaths[(Mathf.Abs(variantOffset) + 4) % StylizedWallRockPrefabPaths.Length];

        PlaceRockDecorPrefab(
            $"{objectName}_Toe",
            crownPrefab,
            center + new Vector3(0f, -scale * 0.62f, scale * 0.02f),
            new Vector3(0f, 12f + variantOffset * 9f, 0f),
            Vector3.one * (scale * 0.84f),
            material);

        PlaceRockDecorPrefab(
            $"{objectName}_Base",
            basePrefab,
            center + new Vector3(0f, -scale * 0.34f, 0f),
            new Vector3(0f, 20f + variantOffset * 11f, 0f),
            Vector3.one * scale,
            material);

        PlaceRockDecorPrefab(
            $"{objectName}_Left",
            sidePrefab,
            center + new Vector3(-scale * 0.34f, -scale * 0.12f, scale * 0.18f),
            new Vector3(0f, -18f - variantOffset * 9f, 0f),
            Vector3.one * (scale * 0.88f),
            material);

        PlaceRockDecorPrefab(
            $"{objectName}_Right",
            crownPrefab,
            center + new Vector3(scale * 0.3f, -scale * 0.04f, -scale * 0.08f),
            new Vector3(0f, 58f + variantOffset * 7f, 0f),
            Vector3.one * (scale * 0.92f),
            material);

        PlaceRockDecorPrefab(
            $"{objectName}_Top",
            sidePrefab,
            center + new Vector3(0f, scale * 0.16f, scale * 0.08f),
            new Vector3(0f, 92f - variantOffset * 5f, 0f),
            Vector3.one * (scale * 0.66f),
            material);

        PlaceRockDecorPrefab(
            $"{objectName}_RearFill",
            basePrefab,
            center + new Vector3(0f, -scale * 0.08f, scale * 0.3f),
            new Vector3(0f, -24f + variantOffset * 8f, 0f),
            Vector3.one * (scale * 0.9f),
            material);

        PlaceRockDecorPrefab(
            $"{objectName}_MidSeal",
            sidePrefab,
            center + new Vector3(0f, scale * 0.04f, scale * 0.06f),
            new Vector3(0f, 44f - variantOffset * 6f, 0f),
            Vector3.one * (scale * 0.82f),
            material);
    }

    private static void CreateMountainMaze(Terrain terrain, Material material)
    {
        CreateMountainWall(terrain, "MountainWall_A", new Vector3(24f, 0f, 22f), new Vector3(24f, 0f, 48f), 8.8f, 14.8f, material);
        CreateMountainWall(terrain, "MountainWall_B", new Vector3(24f, 0f, 60f), new Vector3(24f, 0f, 96f), 8.8f, 15.2f, material);
        CreateMountainWall(terrain, "MountainWall_C", new Vector3(24f, 0f, 36f), new Vector3(58f, 0f, 36f), 8.2f, 13.2f, material);
        CreateMountainWall(terrain, "MountainWall_D", new Vector3(50f, 0f, 36f), new Vector3(50f, 0f, 72f), 8.7f, 14.2f, material);
        CreateMountainWall(terrain, "MountainWall_E", new Vector3(50f, 0f, 72f), new Vector3(92f, 0f, 72f), 8.5f, 13.8f, material);
        CreateMountainWall(terrain, "MountainWall_F", new Vector3(78f, 0f, 18f), new Vector3(78f, 0f, 72f), 8.4f, 14.0f, material);
        CreateMountainWall(terrain, "MountainWall_G", new Vector3(38f, 0f, 96f), new Vector3(94f, 0f, 96f), 8.9f, 15.0f, material);
        CreateMountainWall(terrain, "MountainWall_H", new Vector3(96f, 0f, 72f), new Vector3(96f, 0f, 96f), 8.2f, 13.4f, material);
    }

    private static void CreateMountainWall(
        Terrain terrain,
        string objectName,
        Vector3 start,
        Vector3 end,
        float width,
        float height,
        Material material)
    {
        float length = Vector3.Distance(start, end);
        int segments = Mathf.Max(4, Mathf.CeilToInt(length / (width * 0.62f)));
        Vector3 direction = (end - start).normalized;
        Vector3 perpendicular = Vector3.Cross(Vector3.up, direction).normalized;
        Vector3 midpoint = (start + end) * 0.5f;
        float midpointGroundHeight = SampleTerrainHeight(terrain, midpoint);
        Material shadowBackerMaterial = GetOrCreateLitMaterial(
            $"{GeneratedMaterialsFolder}/MountainWallShadow.mat",
            new Color(0.035f, 0.04f, 0.05f),
            Color.black,
            0.02f);

        GameObject colliderCore = GameObject.CreatePrimitive(PrimitiveType.Cube);
        colliderCore.name = $"{objectName}_Core";
        colliderCore.transform.position = new Vector3(midpoint.x, midpointGroundHeight + height * 0.38f, midpoint.z);
        colliderCore.transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        colliderCore.transform.localScale = new Vector3(width * 1.4f, height * 0.92f, length + width * 0.35f);
        MeshRenderer coreRenderer = colliderCore.GetComponent<MeshRenderer>();
        coreRenderer.enabled = false;
        MarkEnvironmentStatic(colliderCore);

        CreateMountainWallShadowBackers(terrain, objectName, start, end, direction, perpendicular, width, height, shadowBackerMaterial);
        CreateMountainWallFillSpine(terrain, objectName, start, end, direction, perpendicular, width, height);

        int bodySegments = Mathf.Max(8, Mathf.CeilToInt(length / (width * 0.42f)));
        for (int i = 0; i < bodySegments; i++)
        {
            float bodyT = (i + 0.5f) / bodySegments;
            Vector3 bodyPoint = Vector3.Lerp(start, end, bodyT);
            CreateMountainWallBodyCluster(terrain, objectName, i, bodyPoint, direction, perpendicular, width, height);
        }

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            Vector3 point = Vector3.Lerp(start, end, t);
            float groundHeight = SampleTerrainHeight(terrain, point);
            float heightVariance = 0.9f + Mathf.Cos((i + 2) * 0.53f) * 0.16f;
            float sideOffset = ((i % 3) - 1) * width * 0.12f;
            float ridgeHeightOffset = height * 0.22f * heightVariance;

            Vector3 ridgeBase = new Vector3(point.x, groundHeight + ridgeHeightOffset, point.z) + perpendicular * sideOffset;
            Vector3 ridgeRotation = new Vector3(-9f + (i % 2) * 4f, Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg + ((i % 2 == 0) ? -6f : 6f), (i % 3 - 1) * 5f);
            float sideSign = i % 2 == 0 ? -1f : 1f;

            Vector3 basePosition = ridgeBase + Vector3.up * (height * 0.06f) + perpendicular * (sideSign * width * 0.1f);
            basePosition.y = SampleTerrainHeight(terrain, basePosition) + ridgeHeightOffset + height * 0.06f;
            Vector3 baseRotation = new Vector3(0f, ridgeRotation.y + 25f + i * 9f, 0f);
            Vector3 baseScale = Vector3.one * (1.35f + (i % 3) * 0.18f);
            string basePrefab = StylizedWallRockPrefabPaths[i % StylizedWallRockPrefabPaths.Length];
            PlaceRockDecorPrefab($"{objectName}_{i:00}_BaseRock", basePrefab, basePosition, baseRotation, baseScale, null);

            Vector3 shoulderPosition = ridgeBase + Vector3.up * (height * 0.12f) + perpendicular * (-sideSign * width * 0.2f);
            shoulderPosition.y = SampleTerrainHeight(terrain, shoulderPosition) + ridgeHeightOffset + height * 0.12f;
            Vector3 shoulderRotation = new Vector3(0f, ridgeRotation.y - 18f - i * 7f, 0f);
            Vector3 shoulderScale = Vector3.one * (1.05f + (i % 2) * 0.16f);
            string shoulderPrefab = StylizedWallRockPrefabPaths[(i + 2) % StylizedWallRockPrefabPaths.Length];
            PlaceRockDecorPrefab($"{objectName}_{i:00}_ShoulderRock", shoulderPrefab, shoulderPosition, shoulderRotation, shoulderScale, null);

            Vector3 crownPosition = ridgeBase + Vector3.up * (height * 0.34f);
            crownPosition.y = SampleTerrainHeight(terrain, crownPosition) + ridgeHeightOffset + height * 0.34f;
            Vector3 crownRotation = new Vector3(0f, ridgeRotation.y + 52f - i * 5f, 0f);
            Vector3 crownScale = Vector3.one * (0.92f + (i % 3) * 0.1f);
            string crownPrefab = StylizedWallRockPrefabPaths[(i + 4) % StylizedWallRockPrefabPaths.Length];
            PlaceRockDecorPrefab($"{objectName}_{i:00}_CrownRock", crownPrefab, crownPosition, crownRotation, crownScale, null);

            if (i % 2 == 0)
            {
                Vector3 fillerPosition = ridgeBase + direction * (((i % 2 == 0) ? -1f : 1f) * length / Mathf.Max(segments, 1) * 0.18f) + Vector3.up * (height * 0.16f);
                fillerPosition.y = SampleTerrainHeight(terrain, fillerPosition) + ridgeHeightOffset + height * 0.16f;
                Vector3 fillerRotation = new Vector3(0f, ridgeRotation.y + 90f, 0f);
                Vector3 fillerScale = Vector3.one * 0.86f;
                string fillerPrefab = StylizedWallRockPrefabPaths[(i + 1) % StylizedWallRockPrefabPaths.Length];
                PlaceRockDecorPrefab($"{objectName}_{i:00}_FillerRock", fillerPrefab, fillerPosition, fillerRotation, fillerScale, null);
            }
        }
    }

    private static void CreateKillzoneMaze(Terrain terrain, Material lavaMaterial, Material rockMaterial, Material warningMaterial)
    {
        CreateLavaTrench(terrain, "LavaTrench_A", new Vector3(34f, 0f, 54f), new Vector3(6f, 2.2f, 18f), lavaMaterial, rockMaterial, warningMaterial);
        CreateLavaTrench(terrain, "LavaTrench_B", new Vector3(64f, 0f, 54f), new Vector3(20f, 2.2f, 6f), lavaMaterial, rockMaterial, warningMaterial);
        CreateLavaTrench(terrain, "LavaTrench_C", new Vector3(88f, 0f, 46f), new Vector3(6f, 2.2f, 22f), lavaMaterial, rockMaterial, warningMaterial);
        CreateLavaTrench(terrain, "LavaTrench_D", new Vector3(76f, 0f, 88f), new Vector3(28f, 2.2f, 6f), lavaMaterial, rockMaterial, warningMaterial);
        CreateLavaTrench(terrain, "LavaTrench_E", new Vector3(42f, 0f, 82f), new Vector3(16f, 2.2f, 6f), lavaMaterial, rockMaterial, warningMaterial);
    }

    private static void CreatePerimeterKillzone(Terrain terrain, Material lavaMaterial, Material rockMaterial, Material warningMaterial)
    {
        if (terrain == null)
        {
            return;
        }

        Vector3 terrainOrigin = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;
        float borderThickness = 6f;

        CreateLavaTrench(terrain,
            "PerimeterKillzone_West",
            new Vector3(terrainOrigin.x + borderThickness * 0.5f, 0f, terrainOrigin.z + terrainSize.z * 0.5f),
            new Vector3(borderThickness, 2.4f, terrainSize.z),
            lavaMaterial,
            rockMaterial,
            warningMaterial);

        CreateLavaTrench(terrain,
            "PerimeterKillzone_East",
            new Vector3(terrainOrigin.x + terrainSize.x - borderThickness * 0.5f, 0f, terrainOrigin.z + terrainSize.z * 0.5f),
            new Vector3(borderThickness, 2.4f, terrainSize.z),
            lavaMaterial,
            rockMaterial,
            warningMaterial);

        CreateLavaTrench(terrain,
            "PerimeterKillzone_South",
            new Vector3(terrainOrigin.x + terrainSize.x * 0.5f, 0f, terrainOrigin.z + borderThickness * 0.5f),
            new Vector3(terrainSize.x - borderThickness * 2f, 2.4f, borderThickness),
            lavaMaterial,
            rockMaterial,
            warningMaterial);

        CreateLavaTrench(terrain,
            "PerimeterKillzone_North",
            new Vector3(terrainOrigin.x + terrainSize.x * 0.5f, 0f, terrainOrigin.z + terrainSize.z - borderThickness * 0.5f),
            new Vector3(terrainSize.x - borderThickness * 2f, 2.4f, borderThickness),
            lavaMaterial,
            rockMaterial,
            warningMaterial);
    }

    private static void CreateLavaTrench(Terrain terrain, string objectName, Vector3 center, Vector3 scale, Material lavaMaterial, Material rockMaterial, Material warningMaterial)
    {
        SampleHazardHeightRange(terrain, center, scale, out float minHeight, out float maxHeight);
        float visualHeight = maxHeight;
        float colliderTop = visualHeight + 0.35f;
        float colliderBottom = minHeight - 1.25f;
        float colliderHeight = Mathf.Max(scale.y, colliderTop - colliderBottom);
        float colliderCenterY = (colliderTop + colliderBottom) * 0.5f;

        GameObject trench = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trench.name = objectName;
        trench.transform.position = new Vector3(center.x, colliderCenterY, center.z);
        trench.transform.localScale = new Vector3(scale.x, colliderHeight, scale.z);
        MeshRenderer trenchRenderer = trench.GetComponent<MeshRenderer>();
        trenchRenderer.enabled = false;
        trenchRenderer.shadowCastingMode = ShadowCastingMode.Off;
        trenchRenderer.receiveShadows = false;
        trench.AddComponent<Killzone>();
        MarkEnvironmentStatic(trench);

        CreateLavaSurfaceTiles(terrain, objectName, center, scale, lavaMaterial);
        CreateLavaVfxInstances(terrain, trench.transform, objectName, center, scale, maxHeight);

        float lipHeight = 0.28f;
        float lipWidth = 0.7f;

        CreateTrenchLip($"{objectName}_LipNorth",
            new Vector3(center.x, visualHeight + 0.08f, center.z + scale.z * 0.5f + lipWidth * 0.2f),
            new Vector3(scale.x + 0.7f, lipHeight, lipWidth),
            rockMaterial);
        CreateTrenchLip($"{objectName}_LipSouth",
            new Vector3(center.x, visualHeight + 0.08f, center.z - scale.z * 0.5f - lipWidth * 0.2f),
            new Vector3(scale.x + 0.7f, lipHeight, lipWidth),
            rockMaterial);
        CreateTrenchLip($"{objectName}_LipEast",
            new Vector3(center.x + scale.x * 0.5f + lipWidth * 0.2f, visualHeight + 0.08f, center.z),
            new Vector3(lipWidth, lipHeight, scale.z + 0.7f),
            rockMaterial);
        CreateTrenchLip($"{objectName}_LipWest",
            new Vector3(center.x - scale.x * 0.5f - lipWidth * 0.2f, visualHeight + 0.08f, center.z),
            new Vector3(lipWidth, lipHeight, scale.z + 0.7f),
            rockMaterial);

        CreateLightStrip($"{objectName}_WarningNorth",
            new Vector3(center.x, visualHeight + 0.23f, center.z + scale.z * 0.5f + 0.42f),
            new Vector3(Mathf.Max(2f, scale.x - 1f), 0.05f, 0.12f),
            warningMaterial);
        CreateLightStrip($"{objectName}_WarningSouth",
            new Vector3(center.x, visualHeight + 0.23f, center.z - scale.z * 0.5f - 0.42f),
            new Vector3(Mathf.Max(2f, scale.x - 1f), 0.05f, 0.12f),
            warningMaterial);
    }

    private static void CreateLavaVfxInstances(Terrain terrain, Transform parent, string objectName, Vector3 center, Vector3 scale, float fallbackHeight)
    {
        GameObject lavaVfxPrefab = GetFreeFireLavaPrefab();
        if (lavaVfxPrefab == null)
        {
            return;
        }

        bool alongX = scale.x >= scale.z;
        float length = alongX ? scale.x : scale.z;
        int vfxCount = Mathf.Clamp(Mathf.RoundToInt(length / 8f), 1, 8);

        for (int i = 0; i < vfxCount; i++)
        {
            float t = vfxCount == 1 ? 0.5f : i / (float)(vfxCount - 1);
            float offsetLong = Mathf.Lerp(-length * 0.5f + 1.2f, length * 0.5f - 1.2f, t);
            float localX = alongX ? offsetLong : 0f;
            float localZ = alongX ? 0f : offsetLong;
            Vector3 worldPosition = center + new Vector3(localX, 0f, localZ);
            float surfaceHeight = terrain != null
                ? SampleHazardVisualHeight(terrain, worldPosition, new Vector3(2f, 0f, 2f))
                : fallbackHeight;

            GameObject vfxInstance = (GameObject)PrefabUtility.InstantiatePrefab(lavaVfxPrefab);
            if (vfxInstance == null)
            {
                continue;
            }

            vfxInstance.name = $"{objectName}_VFX_{i:00}";
            vfxInstance.transform.SetParent(parent, false);
            vfxInstance.transform.position = new Vector3(worldPosition.x, surfaceHeight + 0.03f, worldPosition.z);
            vfxInstance.transform.rotation = Quaternion.Euler(0f, i * 31f, 0f);
            vfxInstance.transform.localScale = Vector3.one * Mathf.Lerp(0.7f, 1.1f, vfxCount == 1 ? 0.5f : t);
            RemoveCollidersRecursive(vfxInstance);
        }
    }

    private static float SampleHazardVisualHeight(Terrain terrain, Vector3 center, Vector3 scale)
    {
        SampleHazardHeightRange(terrain, center, scale, out _, out float maxHeight);
        return maxHeight;
    }

    private static void SampleHazardHeightRange(Terrain terrain, Vector3 center, Vector3 scale, out float minHeight, out float maxHeight)
    {
        if (terrain == null)
        {
            minHeight = center.y;
            maxHeight = center.y;
            return;
        }

        int xSamples = Mathf.Clamp(Mathf.CeilToInt(scale.x / 8f) + 1, 3, 16);
        int zSamples = Mathf.Clamp(Mathf.CeilToInt(scale.z / 8f) + 1, 3, 16);

        minHeight = float.MaxValue;
        maxHeight = float.MinValue;

        for (int ix = 0; ix < xSamples; ix++)
        {
            float tx = xSamples == 1 ? 0.5f : ix / (float)(xSamples - 1);
            float localX = Mathf.Lerp(-scale.x * 0.5f, scale.x * 0.5f, tx);

            for (int iz = 0; iz < zSamples; iz++)
            {
                float tz = zSamples == 1 ? 0.5f : iz / (float)(zSamples - 1);
                float localZ = Mathf.Lerp(-scale.z * 0.5f, scale.z * 0.5f, tz);

                float sampledHeight = SampleTerrainHeight(terrain, center + new Vector3(localX, 0f, localZ));
                minHeight = Mathf.Min(minHeight, sampledHeight);
                maxHeight = Mathf.Max(maxHeight, sampledHeight);
            }
        }
    }

    private static void CreateLavaSurfaceTiles(Terrain terrain, string objectName, Vector3 center, Vector3 scale, Material lavaMaterial)
    {
        int xTiles = Mathf.Max(1, Mathf.CeilToInt(scale.x / 12f));
        int zTiles = Mathf.Max(1, Mathf.CeilToInt(scale.z / 12f));
        float tileWidth = scale.x / xTiles;
        float tileLength = scale.z / zTiles;
        Material tiledLavaMaterial = CreateTiledSceneMaterial(
            lavaMaterial,
            "LavaSurface",
            new Vector2(Mathf.Max(2f, tileWidth * 0.6f), Mathf.Max(2f, tileLength * 0.6f)));

        for (int x = 0; x < xTiles; x++)
        {
            for (int z = 0; z < zTiles; z++)
            {
                float localX = -scale.x * 0.5f + tileWidth * (x + 0.5f);
                float localZ = -scale.z * 0.5f + tileLength * (z + 0.5f);
                Vector3 tileCenter = center + new Vector3(localX, 0f, localZ);
                float tileHeight = SampleHazardVisualHeight(terrain, tileCenter, new Vector3(tileWidth, 0f, tileLength));

                GameObject lavaTile = GameObject.CreatePrimitive(PrimitiveType.Quad);
                lavaTile.name = $"{objectName}_Surface_{x:00}_{z:00}";
                lavaTile.transform.position = new Vector3(tileCenter.x, tileHeight + 0.015f, tileCenter.z);
                lavaTile.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
                lavaTile.transform.localScale = new Vector3(tileWidth * 1.08f, tileLength * 1.08f, 1f);

                MeshRenderer renderer = lavaTile.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = tiledLavaMaterial;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                RemoveCollider(lavaTile);
                MarkEnvironmentStatic(lavaTile);
            }
        }
    }

    private static void CreateMountainWallBodyCluster(
        Terrain terrain,
        string objectName,
        int index,
        Vector3 center,
        Vector3 direction,
        Vector3 perpendicular,
        float width,
        float height)
    {
        float groundHeight = SampleTerrainHeight(terrain, center);
        float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        float sideSign = index % 2 == 0 ? -1f : 1f;

        Vector3 leftBase = center + perpendicular * (-width * 0.3f) + direction * (width * 0.05f * sideSign);
        leftBase.y = groundHeight - height * 0.025f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_Left",
            StylizedWallRockPrefabPaths[index % StylizedWallRockPrefabPaths.Length],
            leftBase,
            new Vector3(0f, yaw + 18f + index * 7f, 0f),
            Vector3.one * (2.65f + (index % 3) * 0.18f),
            null);

        Vector3 rightBase = center + perpendicular * (width * 0.28f) - direction * (width * 0.04f * sideSign);
        rightBase.y = groundHeight - height * 0.02f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_Right",
            StylizedWallRockPrefabPaths[(index + 2) % StylizedWallRockPrefabPaths.Length],
            rightBase,
            new Vector3(0f, yaw - 24f - index * 6f, 0f),
            Vector3.one * (2.55f + (index % 2) * 0.22f),
            null);

        Vector3 centerMass = center + perpendicular * (sideSign * width * 0.06f);
        centerMass.y = groundHeight + height * 0.14f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_Center",
            StylizedWallRockPrefabPaths[(index + 4) % StylizedWallRockPrefabPaths.Length],
            centerMass,
            new Vector3(0f, yaw + 48f - index * 5f, 0f),
            Vector3.one * (2.95f + (index % 3) * 0.16f),
            null);

        Vector3 crownMass = center + perpendicular * (-sideSign * width * 0.1f);
        crownMass.y = groundHeight + height * 0.36f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_Crown",
            StylizedWallRockPrefabPaths[(index + 1) % StylizedWallRockPrefabPaths.Length],
            crownMass,
            new Vector3(0f, yaw + 82f + index * 4f, 0f),
            Vector3.one * (2.3f + (index % 2) * 0.16f),
            null);

        Vector3 toeLeft = center + perpendicular * (-width * 0.34f) + direction * (width * 0.02f);
        toeLeft.y = groundHeight - height * 0.055f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_ToeLeft",
            StylizedWallRockPrefabPaths[(index + 6) % StylizedWallRockPrefabPaths.Length],
            toeLeft,
            new Vector3(0f, yaw + 10f + index * 3f, 0f),
            Vector3.one * (1.95f + (index % 2) * 0.12f),
            null);

        Vector3 toeRight = center + perpendicular * (width * 0.32f) - direction * (width * 0.02f);
        toeRight.y = groundHeight - height * 0.05f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_ToeRight",
            StylizedWallRockPrefabPaths[(index + 1) % StylizedWallRockPrefabPaths.Length],
            toeRight,
            new Vector3(0f, yaw - 12f - index * 3f, 0f),
            Vector3.one * (1.9f + (index % 3) * 0.1f),
            null);

        Vector3 toeCenter = center + perpendicular * (sideSign * width * 0.01f);
        toeCenter.y = groundHeight - height * 0.04f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_ToeCenter",
            StylizedWallRockPrefabPaths[(index + 3) % StylizedWallRockPrefabPaths.Length],
            toeCenter,
            new Vector3(0f, yaw + 26f - index * 2f, 0f),
            Vector3.one * (2.05f + (index % 2) * 0.1f),
            null);

        Vector3 frontFill = center + direction * (width * 0.18f) + perpendicular * (sideSign * width * 0.04f);
        frontFill.y = groundHeight + height * 0.18f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_FrontFill",
            StylizedWallRockPrefabPaths[(index + 3) % StylizedWallRockPrefabPaths.Length],
            frontFill,
            new Vector3(0f, yaw + 36f + index * 3f, 0f),
            Vector3.one * (2.35f + (index % 3) * 0.14f),
            null);

        Vector3 rearFill = center - direction * (width * 0.16f) + perpendicular * (-sideSign * width * 0.03f);
        rearFill.y = groundHeight + height * 0.16f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_RearFill",
            StylizedWallRockPrefabPaths[(index + 5) % StylizedWallRockPrefabPaths.Length],
            rearFill,
            new Vector3(0f, yaw - 42f - index * 4f, 0f),
            Vector3.one * (2.3f + (index % 2) * 0.16f),
            null);

        Vector3 midSeal = center + perpendicular * (-sideSign * width * 0.02f);
        midSeal.y = groundHeight + height * 0.24f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_MidSeal",
            StylizedWallRockPrefabPaths[(index + 2) % StylizedWallRockPrefabPaths.Length],
            midSeal,
            new Vector3(0f, yaw + 8f - index * 2f, 0f),
            Vector3.one * (2.2f + (index % 3) * 0.12f),
            null);

        Vector3 leftBridge = center + perpendicular * (-width * 0.17f) + direction * (width * 0.12f);
        leftBridge.y = groundHeight + height * 0.2f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_LeftBridge",
            StylizedWallRockPrefabPaths[(index + 5) % StylizedWallRockPrefabPaths.Length],
            leftBridge,
            new Vector3(0f, yaw + 28f + index * 2f, 0f),
            Vector3.one * (2.15f + (index % 2) * 0.14f),
            null);

        Vector3 rightBridge = center + perpendicular * (width * 0.16f) - direction * (width * 0.1f);
        rightBridge.y = groundHeight + height * 0.22f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_RightBridge",
            StylizedWallRockPrefabPaths[(index + 1) % StylizedWallRockPrefabPaths.Length],
            rightBridge,
            new Vector3(0f, yaw - 18f - index * 3f, 0f),
            Vector3.one * (2.1f + (index % 3) * 0.12f),
            null);

        Vector3 frontCap = center + direction * (width * 0.26f);
        frontCap.y = groundHeight + height * 0.27f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_FrontCap",
            StylizedWallRockPrefabPaths[(index + 4) % StylizedWallRockPrefabPaths.Length],
            frontCap,
            new Vector3(0f, yaw + 64f + index * 2f, 0f),
            Vector3.one * (2.05f + (index % 2) * 0.12f),
            null);

        Vector3 rearCap = center - direction * (width * 0.24f);
        rearCap.y = groundHeight + height * 0.25f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_RearCap",
            StylizedWallRockPrefabPaths[(index + 6) % StylizedWallRockPrefabPaths.Length],
            rearCap,
            new Vector3(0f, yaw - 56f - index * 2f, 0f),
            Vector3.one * (2.0f + (index % 3) * 0.1f),
            null);

        Vector3 upperBridge = center + perpendicular * (sideSign * width * 0.03f) + direction * (width * 0.08f);
        upperBridge.y = groundHeight + height * 0.42f;
        PlaceRockDecorPrefab(
            $"{objectName}_Body_{index:00}_UpperBridge",
            StylizedWallRockPrefabPaths[(index + 3) % StylizedWallRockPrefabPaths.Length],
            upperBridge,
            new Vector3(0f, yaw + 18f - index * 2f, 0f),
            Vector3.one * (1.9f + (index % 2) * 0.1f),
            null);
    }

    private static void CreateMountainWallFillSpine(
        Terrain terrain,
        string objectName,
        Vector3 start,
        Vector3 end,
        Vector3 direction,
        Vector3 perpendicular,
        float width,
        float height)
    {
        float length = Vector3.Distance(start, end);
        int fillSegments = Mathf.Max(12, Mathf.CeilToInt(length / (width * 0.26f)));
        float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        for (int i = 0; i < fillSegments; i++)
        {
            float t = (i + 0.5f) / fillSegments;
            Vector3 point = Vector3.Lerp(start, end, t);
            float groundHeight = SampleTerrainHeight(terrain, point);
            float sideSign = i % 2 == 0 ? -1f : 1f;

            Vector3 lowerCore = point + perpendicular * (sideSign * width * 0.06f);
            lowerCore.y = groundHeight - height * 0.03f;
            PlaceRockDecorPrefab(
                $"{objectName}_Fill_{i:00}_Lower",
                StylizedWallRockPrefabPaths[(i + 1) % StylizedWallRockPrefabPaths.Length],
                lowerCore,
                new Vector3(0f, yaw + 14f + i * 5f, 0f),
                Vector3.one * (2.9f + (i % 3) * 0.18f),
                null);

            Vector3 groundSeal = point + perpendicular * (-sideSign * width * 0.03f);
            groundSeal.y = groundHeight - height * 0.04f;
            PlaceRockDecorPrefab(
                $"{objectName}_Fill_{i:00}_GroundSeal",
                StylizedWallRockPrefabPaths[(i + 7) % StylizedWallRockPrefabPaths.Length],
                groundSeal,
                new Vector3(0f, yaw + 6f + i * 2f, 0f),
                Vector3.one * (2.15f + (i % 2) * 0.1f),
                null);

            Vector3 midCore = point - direction * (width * 0.08f) + perpendicular * (-sideSign * width * 0.04f);
            midCore.y = groundHeight + height * 0.22f;
            PlaceRockDecorPrefab(
                $"{objectName}_Fill_{i:00}_Mid",
                StylizedWallRockPrefabPaths[(i + 3) % StylizedWallRockPrefabPaths.Length],
                midCore,
                new Vector3(0f, yaw + 56f - i * 4f, 0f),
                Vector3.one * (2.6f + (i % 2) * 0.2f),
                null);

            Vector3 upperCore = point + direction * (width * 0.06f);
            upperCore.y = groundHeight + height * 0.38f;
            PlaceRockDecorPrefab(
                $"{objectName}_Fill_{i:00}_Upper",
                StylizedWallRockPrefabPaths[(i + 5) % StylizedWallRockPrefabPaths.Length],
                upperCore,
                new Vector3(0f, yaw - 28f + i * 3f, 0f),
                Vector3.one * (2.35f + (i % 3) * 0.14f),
                null);

            Vector3 centerSeal = point + perpendicular * (sideSign * width * 0.02f);
            centerSeal.y = groundHeight + height * 0.3f;
            PlaceRockDecorPrefab(
                $"{objectName}_Fill_{i:00}_CenterSeal",
                StylizedWallRockPrefabPaths[(i + 2) % StylizedWallRockPrefabPaths.Length],
                centerSeal,
                new Vector3(0f, yaw + 22f - i * 3f, 0f),
                Vector3.one * (2.3f + (i % 2) * 0.16f),
                null);

            Vector3 frontSeal = point + direction * (width * 0.16f) + perpendicular * (-sideSign * width * 0.02f);
            frontSeal.y = groundHeight + height * 0.18f;
            PlaceRockDecorPrefab(
                $"{objectName}_Fill_{i:00}_FrontSeal",
                StylizedWallRockPrefabPaths[(i + 4) % StylizedWallRockPrefabPaths.Length],
                frontSeal,
                new Vector3(0f, yaw + 34f + i * 2f, 0f),
                Vector3.one * (2.25f + (i % 3) * 0.12f),
                null);

            Vector3 rearSeal = point - direction * (width * 0.14f) + perpendicular * (sideSign * width * 0.01f);
            rearSeal.y = groundHeight + height * 0.2f;
            PlaceRockDecorPrefab(
                $"{objectName}_Fill_{i:00}_RearSeal",
                StylizedWallRockPrefabPaths[(i + 6) % StylizedWallRockPrefabPaths.Length],
                rearSeal,
                new Vector3(0f, yaw - 36f - i * 2f, 0f),
                Vector3.one * (2.2f + (i % 2) * 0.12f),
                null);
        }
    }

    private static void CreateMountainWallShadowBackers(
        Terrain terrain,
        string objectName,
        Vector3 start,
        Vector3 end,
        Vector3 direction,
        Vector3 perpendicular,
        float width,
        float height,
        Material material)
    {
        float length = Vector3.Distance(start, end);
        int backerSegments = Mathf.Max(8, Mathf.CeilToInt(length / (width * 0.52f)));
        float yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;

        for (int i = 0; i < backerSegments; i++)
        {
            float t = (i + 0.5f) / backerSegments;
            Vector3 point = Vector3.Lerp(start, end, t);
            float groundHeight = SampleTerrainHeight(terrain, point);
            float sideSign = i % 2 == 0 ? -1f : 1f;

            Vector3 lowerShadow = point + perpendicular * (sideSign * width * 0.08f);
            lowerShadow.y = groundHeight - height * 0.02f;
            PlaceRockDecorPrefab(
                $"{objectName}_Shadow_{i:00}_Lower",
                StylizedWallRockPrefabPaths[(i + 1) % StylizedWallRockPrefabPaths.Length],
                lowerShadow,
                new Vector3(0f, yaw + 12f + i * 4f, 0f),
                Vector3.one * (3.1f + (i % 3) * 0.2f),
                material);

            Vector3 toeShadow = point + perpendicular * (sideSign * width * 0.02f);
            toeShadow.y = groundHeight - height * 0.05f;
            PlaceRockDecorPrefab(
                $"{objectName}_Shadow_{i:00}_Toe",
                StylizedWallRockPrefabPaths[(i + 4) % StylizedWallRockPrefabPaths.Length],
                toeShadow,
                new Vector3(0f, yaw + 2f - i * 2f, 0f),
                Vector3.one * (2.35f + (i % 2) * 0.12f),
                material);

            Vector3 midShadow = point - direction * (width * 0.05f) + perpendicular * (-sideSign * width * 0.03f);
            midShadow.y = groundHeight + height * 0.28f;
            PlaceRockDecorPrefab(
                $"{objectName}_Shadow_{i:00}_Mid",
                StylizedWallRockPrefabPaths[(i + 3) % StylizedWallRockPrefabPaths.Length],
                midShadow,
                new Vector3(0f, yaw + 44f - i * 3f, 0f),
                Vector3.one * (2.8f + (i % 2) * 0.18f),
                material);

            Vector3 upperShadow = point + direction * (width * 0.04f);
            upperShadow.y = groundHeight + height * 0.44f;
            PlaceRockDecorPrefab(
                $"{objectName}_Shadow_{i:00}_Upper",
                StylizedWallRockPrefabPaths[(i + 5) % StylizedWallRockPrefabPaths.Length],
                upperShadow,
                new Vector3(0f, yaw - 22f + i * 2f, 0f),
                Vector3.one * (2.45f + (i % 3) * 0.14f),
                material);
        }
    }

    private static float SampleTerrainHeight(Terrain terrain, Vector3 point)
    {
        return terrain != null ? terrain.SampleHeight(point) + terrain.transform.position.y : point.y;
    }

    private static void CreateTrenchLip(string objectName, Vector3 position, Vector3 scale, Material material)
    {
        GameObject lip = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lip.name = objectName;
        lip.transform.position = position;
        lip.transform.localScale = scale;
        lip.GetComponent<MeshRenderer>().sharedMaterial = material;
        RemoveCollider(lip);
        MarkEnvironmentStatic(lip);
    }

    private static float ComputePathInfluence(Vector2 point, Vector2 segmentStart, Vector2 segmentEnd, float radius)
    {
        Vector2 segment = segmentEnd - segmentStart;
        float lengthSquared = segment.sqrMagnitude;
        if (lengthSquared <= Mathf.Epsilon)
        {
            return 0f;
        }

        float t = Mathf.Clamp01(Vector2.Dot(point - segmentStart, segment) / lengthSquared);
        Vector2 closestPoint = segmentStart + segment * t;
        float distance = Vector2.Distance(point, closestPoint);
        if (distance >= radius)
        {
            return 0f;
        }

        float normalized = 1f - distance / radius;
        return Mathf.SmoothStep(0f, 1f, normalized);
    }

    private static Material GetOrCreateLitMaterial(string path, Color baseColor, Color emissionColor, float smoothness)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.shader = shader;
        material.SetColor("_BaseColor", baseColor);
        material.SetColor("_Color", baseColor);
        material.SetFloat("_Smoothness", smoothness);
        material.SetColor("_EmissionColor", emissionColor);
        material.EnableKeyword("_EMISSION");
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material GetOrCreateProceduralSkyboxMaterial(string path, Color skyTint, Color groundColor, float atmosphereThickness, float exposure, float sunSize, float sunSizeConvergence)
    {
        Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
        Shader shader = Shader.Find("Skybox/Procedural");

        if (shader == null)
        {
            return material;
        }

        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, path);
        }

        material.shader = shader;
        material.SetColor("_SkyTint", skyTint);
        material.SetColor("_GroundColor", groundColor);
        material.SetFloat("_AtmosphereThickness", atmosphereThickness);
        material.SetFloat("_Exposure", exposure);
        material.SetFloat("_SunSize", sunSize);
        material.SetFloat("_SunSizeConvergence", sunSizeConvergence);
        material.SetFloat("_SunDisk", 2f);
        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material GetOrCreateGeneratedMaterialCopy(string targetPath, string materialName, Material sourceMaterial)
    {
        if (sourceMaterial == null)
        {
            return null;
        }

        Material material = AssetDatabase.LoadAssetAtPath<Material>(targetPath);
        if (material == null)
        {
            material = new Material(sourceMaterial)
            {
                name = materialName
            };
            AssetDatabase.CreateAsset(material, targetPath);
        }
        else
        {
            material.shader = sourceMaterial.shader;
            material.CopyPropertiesFromMaterial(sourceMaterial);
        }

        EditorUtility.SetDirty(material);
        return material;
    }

    private static Material GetCompatibleImportedLitMaterial(string assetPath)
    {
        Material sourceMaterial = AssetDatabase.LoadAssetAtPath<Material>(assetPath);
        if (sourceMaterial == null)
        {
            return null;
        }

        Shader urpLit = Shader.Find("Universal Render Pipeline/Lit");
        if (urpLit == null || sourceMaterial.shader == urpLit)
        {
            return sourceMaterial;
        }

        EnsureFolder(GeneratedMaterialsFolder, "Imported");
        string guid = AssetDatabase.AssetPathToGUID(assetPath);
        string sourceName = Path.GetFileNameWithoutExtension(assetPath);
        string convertedPath = $"{GeneratedCompatibleMaterialsFolder}/{sourceName}_{guid.Substring(0, 8)}.mat";

        Material convertedMaterial = AssetDatabase.LoadAssetAtPath<Material>(convertedPath);
        if (convertedMaterial == null)
        {
            convertedMaterial = new Material(urpLit)
            {
                name = $"{sourceMaterial.name}_URP"
            };
            AssetDatabase.CreateAsset(convertedMaterial, convertedPath);
        }

        bool isTransparent = sourceMaterial.renderQueue >= 3000 || assetPath.Contains("Glass", StringComparison.OrdinalIgnoreCase);
        ConfigureCompatibleLitMaterial(convertedMaterial, sourceMaterial, urpLit, isTransparent);
        return convertedMaterial;
    }

    private static Material CreateTiledSceneMaterial(Material source, string materialName, Vector2 tiling)
    {
        if (source == null)
        {
            return null;
        }

        EnsureFolder(GeneratedMaterialsFolder, "Tiled");
        string safeName = $"{materialName}_{Mathf.RoundToInt(tiling.x * 100f)}x{Mathf.RoundToInt(tiling.y * 100f)}";
        string materialPath = $"{GeneratedTiledMaterialsFolder}/{safeName}.mat";
        Material materialInstance = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (materialInstance == null)
        {
            materialInstance = new Material(source)
            {
                name = safeName
            };
            AssetDatabase.CreateAsset(materialInstance, materialPath);
        }

        if (materialInstance.HasProperty("_BaseMap"))
        {
            materialInstance.SetTextureScale("_BaseMap", tiling);
        }

        if (materialInstance.HasProperty("_MainTex"))
        {
            materialInstance.SetTextureScale("_MainTex", tiling);
        }

        if (materialInstance.HasProperty("_EmissionMap"))
        {
            materialInstance.SetTextureScale("_EmissionMap", tiling);
        }

        EditorUtility.SetDirty(materialInstance);
        return materialInstance;
    }

    private static void PlaceRockDecorPrefab(string objectName, string prefabPath, Vector3 position, Vector3 rotation, Vector3 scale, Material material)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            return;
        }

        GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.name = objectName;
        instance.transform.position = position;
        instance.transform.rotation = Quaternion.Euler(rotation);
        instance.transform.localScale = scale;
        ApplyCompatibleGeneratedMaterialsRecursive(instance);
        ApplyMaterialRecursive(instance, material);
        AlignPrefabBottomToGround(instance, position.y);
        RemoveCollidersRecursive(instance);
        MarkEnvironmentStatic(instance);
    }

    private static void ConfigureCompatibleLitMaterial(Material targetMaterial, Material sourceMaterial, Shader shader, bool isTransparent)
    {
        Texture mainTex = sourceMaterial.GetTexture("_MainTex");
        Texture bumpMap = sourceMaterial.GetTexture("_BumpMap");
        Texture metallicGlossMap = sourceMaterial.GetTexture("_MetallicGlossMap");
        Texture occlusionMap = sourceMaterial.GetTexture("_OcclusionMap");
        Texture emissionMap = sourceMaterial.GetTexture("_EmissionMap");
        Color baseColor = sourceMaterial.HasProperty("_Color") ? sourceMaterial.GetColor("_Color") : Color.white;
        Color emissionColor = sourceMaterial.HasProperty("_EmissionColor") ? sourceMaterial.GetColor("_EmissionColor") : Color.black;
        float smoothness = sourceMaterial.HasProperty("_Glossiness") ? sourceMaterial.GetFloat("_Glossiness") : 0.5f;
        float metallic = sourceMaterial.HasProperty("_Metallic") ? sourceMaterial.GetFloat("_Metallic") : 0f;
        float bumpScale = sourceMaterial.HasProperty("_BumpScale") ? sourceMaterial.GetFloat("_BumpScale") : 1f;
        float occlusionStrength = sourceMaterial.HasProperty("_OcclusionStrength") ? sourceMaterial.GetFloat("_OcclusionStrength") : 1f;
        Vector2 mainTexScale = sourceMaterial.GetTextureScale("_MainTex");
        Vector2 mainTexOffset = sourceMaterial.GetTextureOffset("_MainTex");

        targetMaterial.shader = shader;
        targetMaterial.SetTexture("_BaseMap", mainTex);
        targetMaterial.SetTextureScale("_BaseMap", mainTexScale);
        targetMaterial.SetTextureOffset("_BaseMap", mainTexOffset);
        targetMaterial.SetColor("_BaseColor", baseColor);
        targetMaterial.SetTexture("_BumpMap", bumpMap);
        targetMaterial.SetFloat("_BumpScale", bumpScale);
        targetMaterial.SetTexture("_MetallicGlossMap", metallicGlossMap);
        targetMaterial.SetFloat("_Smoothness", smoothness);
        targetMaterial.SetFloat("_Metallic", metallic);
        targetMaterial.SetTexture("_OcclusionMap", occlusionMap);
        targetMaterial.SetFloat("_OcclusionStrength", occlusionStrength);
        targetMaterial.SetTexture("_EmissionMap", emissionMap);
        targetMaterial.SetColor("_EmissionColor", emissionColor);

        if (bumpMap != null)
        {
            targetMaterial.EnableKeyword("_NORMALMAP");
        }

        if (metallicGlossMap != null)
        {
            targetMaterial.EnableKeyword("_METALLICSPECGLOSSMAP");
        }

        if (emissionMap != null || emissionColor.maxColorComponent > 0.001f)
        {
            targetMaterial.EnableKeyword("_EMISSION");
        }

        if (isTransparent)
        {
            targetMaterial.SetFloat("_Surface", 1f);
            targetMaterial.SetOverrideTag("RenderType", "Transparent");
            targetMaterial.renderQueue = (int)RenderQueue.Transparent;
        }
        else
        {
            targetMaterial.SetFloat("_Surface", 0f);
            targetMaterial.SetOverrideTag("RenderType", "Opaque");
            targetMaterial.renderQueue = -1;
        }

        EditorUtility.SetDirty(targetMaterial);
    }

    private static void ApplyCompatibleGeneratedMaterialsRecursive(GameObject gameObject)
    {
        if (gameObject == null)
        {
            return;
        }

        foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;
            bool changed = false;
            for (int i = 0; i < materials.Length; i++)
            {
                Material compatible = ResolveCompatibleMaterial(materials[i]);
                if (compatible != null && compatible != materials[i])
                {
                    materials[i] = compatible;
                    changed = true;
                }
            }

            if (changed)
            {
                renderer.sharedMaterials = materials;
            }
        }
    }

    private static Material ResolveCompatibleMaterial(Material material)
    {
        if (material == null)
        {
            return null;
        }

        string assetPath = AssetDatabase.GetAssetPath(material);
        if (string.IsNullOrWhiteSpace(assetPath) || assetPath.StartsWith(GeneratedMaterialsFolder, StringComparison.Ordinal))
        {
            return material;
        }

        return GetCompatibleImportedLitMaterial(assetPath) ?? material;
    }

    private static void AlignPrefabBottomToGround(GameObject gameObject, float groundY)
    {
        if (gameObject == null)
        {
            return;
        }

        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0)
        {
            return;
        }

        Bounds bounds = renderers[0].bounds;
        for (int i = 1; i < renderers.Length; i++)
        {
            bounds.Encapsulate(renderers[i].bounds);
        }

        float offset = groundY - bounds.min.y;
        gameObject.transform.position += Vector3.up * offset;
    }

    private static void ApplyMaterialRecursive(GameObject gameObject, Material material)
    {
        if (gameObject == null || material == null)
        {
            return;
        }

        foreach (Renderer renderer in gameObject.GetComponentsInChildren<Renderer>(true))
        {
            Material[] materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                materials[i] = material;
            }

            renderer.sharedMaterials = materials;
        }
    }

    private static Canvas CreateCanvas(string objectName)
    {
        GameObject canvasObject = new GameObject(objectName, typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        return canvas;
    }

    private static GameObject CreateFullScreenPanel(Transform parent, string objectName, Color color)
    {
        return CreatePanel(parent, objectName, color, Vector2.zero, Vector2.zero, stretch: true);
    }

    private static GameObject CreatePanel(Transform parent, string objectName, Color color, Vector2 size, Vector2 anchoredPosition, bool stretch = false)
    {
        GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(parent, false);

        Image image = panel.GetComponent<Image>();
        image.color = color;

        RectTransform rectTransform = panel.GetComponent<RectTransform>();
        if (stretch)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
        else
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.sizeDelta = size;
            rectTransform.anchoredPosition = anchoredPosition;
        }

        return panel;
    }

    private static Text CreateText(Transform parent, string objectName, string textValue, int fontSize, Vector2 anchoredPosition, Vector2 size, FontStyle style = FontStyle.Normal, TextAnchor alignment = TextAnchor.MiddleCenter, Color? color = null)
    {
        GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(parent, false);

        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;

        Text text = textObject.GetComponent<Text>();
        text.font = GetBuiltinFont();
        text.text = textValue;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = alignment;
        text.color = color ?? TextPrimary;
        AddTextEffects(text, new Color(0f, 0f, 0f, 0.42f), new Color(0.04f, 0.12f, 0.18f, 0.65f));

        return text;
    }

    private static Button CreateButton(Transform parent, string objectName, string label, Vector2 anchoredPosition, Vector2 size)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(parent, false);

        Image image = buttonObject.GetComponent<Image>();
        image.color = SecondaryColor;
        AddGraphicEffects(image, new Color(0.05f, 0.28f, 0.38f, 0.9f), new Color(0f, 0f, 0f, 0.42f));

        RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.sizeDelta = size;
        rectTransform.anchoredPosition = anchoredPosition;

        CreateAccentBar(buttonObject.transform, "AccentLine", new Vector2(0f, size.y * 0.5f - 8f), new Vector2(size.x - 28f, 5f), AccentColor);
        CreateText(buttonObject.transform, "Label", label, 24, new Vector2(0f, -4f), size, FontStyle.Bold, TextAnchor.MiddleCenter, TextPrimary);

        ColorBlock colors = buttonObject.GetComponent<Button>().colors;
        colors.normalColor = SecondaryColor;
        colors.highlightedColor = new Color(0.12f, 0.2f, 0.29f, 1f);
        colors.pressedColor = new Color(0.05f, 0.08f, 0.12f, 1f);
        colors.selectedColor = SecondaryColor;
        buttonObject.GetComponent<Button>().colors = colors;

        return buttonObject.GetComponent<Button>();
    }

    private static GameObject CreateWindowPanel(Transform parent, string objectName, Vector2 size, Vector2 anchoredPosition, Color? color = null)
    {
        GameObject panel = CreatePanel(parent, objectName, color ?? PanelColor, size, anchoredPosition);
        Image image = panel.GetComponent<Image>();
        AddGraphicEffects(image, new Color(0.1f, 0.42f, 0.58f, 0.9f), new Color(0f, 0f, 0f, 0.55f));
        CreateAccentBar(panel.transform, "HeaderAccent", new Vector2(0f, size.y * 0.5f - 24f), new Vector2(size.x - 60f, 4f), AccentColor);
        CreateAccentBar(panel.transform, "FooterAccent", new Vector2(0f, -size.y * 0.5f + 22f), new Vector2(size.x - 60f, 2f), new Color(0.12f, 0.2f, 0.28f, 1f));
        return panel;
    }

    private static GameObject CreateAccentBar(Transform parent, string objectName, Vector2 anchoredPosition, Vector2 size, Color color)
    {
        return CreatePanel(parent, objectName, color, size, anchoredPosition);
    }

    private static void AddTextEffects(Text text, Color shadowColor, Color outlineColor)
    {
        Shadow shadow = text.gameObject.AddComponent<Shadow>();
        shadow.effectColor = shadowColor;
        shadow.effectDistance = new Vector2(1f, -2f);

        Outline outline = text.gameObject.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(1f, -1f);
    }

    private static void AddGraphicEffects(Graphic graphic, Color outlineColor, Color shadowColor)
    {
        Shadow shadow = graphic.gameObject.AddComponent<Shadow>();
        shadow.effectColor = shadowColor;
        shadow.effectDistance = new Vector2(2f, -3f);

        Outline outline = graphic.gameObject.AddComponent<Outline>();
        outline.effectColor = outlineColor;
        outline.effectDistance = new Vector2(1f, -1f);
    }

    private static void CreateEventSystem()
    {
        if (UnityEngine.Object.FindAnyObjectByType<EventSystem>() != null)
        {
            return;
        }

        new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
    }

    private static Camera CreateSimpleCamera(string objectName, Color backgroundColor)
    {
        GameObject cameraObject = new GameObject(objectName);
        cameraObject.tag = "MainCamera";

        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = backgroundColor;

        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<UniversalAdditionalCameraData>();

        return camera;
    }

    private static Light CreateLight(string objectName, LightType lightType, Vector3 position, Color color, float intensity, float range, LightmapBakeType bakeType)
    {
        GameObject lightObject = new GameObject(objectName);
        lightObject.transform.position = position;

        Light light = lightObject.AddComponent<Light>();
        light.type = lightType;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.lightmapBakeType = bakeType;
        light.shadows = LightShadows.None;
        return light;
    }

    private static void CreateInvisibleCollisionBlock(string objectName, Vector3 position, Vector3 scale)
    {
        GameObject block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = objectName;
        block.transform.position = position;
        block.transform.localScale = scale;

        MeshRenderer renderer = block.GetComponent<MeshRenderer>();
        renderer.enabled = false;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        renderer.receiveShadows = false;

        MarkEnvironmentStatic(block);
    }

    private static void MarkEnvironmentStatic(GameObject root)
    {
        if (root == null)
        {
            return;
        }

        foreach (Transform current in root.GetComponentsInChildren<Transform>(true))
        {
            GameObjectUtility.SetStaticEditorFlags(current.gameObject, EnvironmentStaticFlags);
        }
    }

    private static void RemoveCollider(GameObject gameObject)
    {
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }
    }

    private static void RemoveCollidersRecursive(GameObject gameObject)
    {
        foreach (Collider collider in gameObject.GetComponentsInChildren<Collider>(true))
        {
            UnityEngine.Object.DestroyImmediate(collider);
        }
    }

    private static Font GetBuiltinFont()
    {
        Font font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        return font != null ? font : Resources.GetBuiltinResource<Font>("Arial.ttf");
    }

    private static void SaveScene(Scene scene, string path)
    {
        EditorSceneManager.SaveScene(scene, path);
    }

    private static void EnsureFolder(string parent, string child)
    {
        string path = $"{parent}/{child}";
        if (!AssetDatabase.IsValidFolder(path))
        {
            AssetDatabase.CreateFolder(parent, child);
        }
    }
}
