using EscapeFacility;
using UnityEditor;
using UnityEngine;

public static partial class EscapeFacilitySceneBuilder
{
    private static void SetupGameplaySceneContext(
        string objectName,
        int requiredCollectibles,
        string nextSceneName,
        MusicTheme musicTheme,
        SceneObjectiveMode objectiveMode,
        int objectiveProgressTarget,
        string progressObjectiveTemplate,
        string postProgressObjectiveText,
        string postActivationObjectiveText,
        string completedObjectiveText)
    {
        SceneContext context = new GameObject(objectName).AddComponent<SceneContext>();
        SerializedObject sceneContext = new SerializedObject(context);
        sceneContext.FindProperty("requiredCollectibles").intValue = requiredCollectibles;
        sceneContext.FindProperty("nextSceneName").stringValue = nextSceneName;
        sceneContext.FindProperty("musicTheme").enumValueIndex = (int)musicTheme;
        sceneContext.FindProperty("objectiveMode").enumValueIndex = (int)objectiveMode;
        sceneContext.FindProperty("objectiveProgressTarget").intValue = objectiveProgressTarget;
        sceneContext.FindProperty("progressObjectiveTemplate").stringValue = progressObjectiveTemplate;
        sceneContext.FindProperty("postProgressObjectiveText").stringValue = postProgressObjectiveText;
        sceneContext.FindProperty("postActivationObjectiveText").stringValue = postActivationObjectiveText;
        sceneContext.FindProperty("completedObjectiveText").stringValue = completedObjectiveText;
        sceneContext.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void CreateSceneVersionMarker(string sceneKey)
    {
        GameObject markerObject = new GameObject($"SceneVersion_{sceneKey}_{_generationStamp.Replace(':', '-').Replace(' ', '_')}");
        SceneVersionMarker marker = markerObject.AddComponent<SceneVersionMarker>();
        SerializedObject markerObjectData = new SerializedObject(marker);
        markerObjectData.FindProperty("sceneKey").stringValue = sceneKey;
        markerObjectData.FindProperty("generationStamp").stringValue = _generationStamp;
        markerObjectData.ApplyModifiedPropertiesWithoutUndo();
    }
}
