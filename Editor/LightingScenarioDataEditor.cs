using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using Unity.EditorCoroutines.Editor;
using System.Collections.Generic;
using System.IO;

[CustomEditor(typeof(LightingScenarioData))]
public class LightingScenarioEditor : Editor
{

    public SerializedProperty geometrySceneName;
    public SerializedProperty lightingSceneName;
    public SerializedProperty storeRendererInfos;
    public SerializedProperty lightmapsMode;
    public SerializedProperty lightmaps;
    public SerializedProperty lightProbes;
    public SerializedProperty rendererInfos;
    public SerializedProperty hasRealtimeLights;

    public void OnEnable()
    {
        geometrySceneName = serializedObject.FindProperty("geometrySceneName");
        lightingSceneName = serializedObject.FindProperty("lightingSceneName");
        storeRendererInfos = serializedObject.FindProperty("storeRendererInfos");
        lightmapsMode = serializedObject.FindProperty("lightmapsMode");
        lightmaps = serializedObject.FindProperty("lightmaps");
        lightProbes = serializedObject.FindProperty("lightProbesAsset");
        rendererInfos = serializedObject.FindProperty("rendererInfos");
        hasRealtimeLights = serializedObject.FindProperty("hasRealtimeLights");
    }

    public override void OnInspectorGUI()
    {
        static IEnumerator EditorLoadLightingScenario(LightingScenarioData scenarioData)
        {
            yield return null;

            LightingScenarioDataFactory.LoadLightingScenarioScenes(scenarioData);
            GameObject.FindObjectOfType<LevelLightmapData>().LoadLightingScenarioData(scenarioData);
        }

        serializedObject.Update();
        EditorGUILayout.PropertyField(geometrySceneName);
        EditorGUILayout.PropertyField(lightingSceneName);
        EditorGUILayout.PropertyField(storeRendererInfos);
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.LabelField("Stored Data", EditorStyles.boldLabel);
        //Begin disabled group as this is a data summary display
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.PropertyField(lightmapsMode);
        EditorGUILayout.TextField("Lightmaps count", lightmaps.arraySize.ToString());
        EditorGUILayout.TextField("Renderer Infos count", rendererInfos.arraySize.ToString());
        EditorGUILayout.ObjectField(lightProbes);
        EditorGUILayout.PropertyField(hasRealtimeLights);

        EditorGUI.EndDisabledGroup();
        EditorGUILayout.EndVertical();

        LightingScenarioData scenarioData = (LightingScenarioData)target;

        if (GUILayout.Button("Generate lighting scenario data"))
        {
            // Pawn off the workload to the Factory.
            LightingScenarioDataFactory f = ObjectFactory.CreateInstance<LightingScenarioDataFactory>();
            f.OnBakingDone += () => { Debug.Log("Baking done."); DestroyImmediate(f); };
            f.GenerateLightingScenarioData(scenarioData, true);
        }

        if (GUILayout.Button("Load Lighting scenario"))
            EditorCoroutineUtility.StartCoroutine(EditorLoadLightingScenario(scenarioData), this);

        serializedObject.ApplyModifiedProperties();
    }
}