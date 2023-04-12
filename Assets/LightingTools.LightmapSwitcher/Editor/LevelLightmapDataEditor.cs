using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;
using System.Collections;
using Unity.EditorCoroutines.Editor;

[CustomEditor(typeof(LevelLightmapData))]
public class LevelLightmapDataEditor : Editor
{
    public SerializedProperty lightingScenariosScenes;
    public SerializedProperty lightingScenariosData;
    public SerializedProperty allowLoadingLightingScenes;
    public SerializedProperty applyLightmapScaleAndOffset;

    private readonly GUIContent allowLoading = new(
        "Allow loading Lighting Scenes",
        "Allow the Level Lightmap Data script to load a lighting scene additively at runtime if the lighting scenario contains realtime lights.");

    public void OnEnable()
    {
        lightingScenariosData = serializedObject.FindProperty("lightingScenariosData");
        allowLoadingLightingScenes = serializedObject.FindProperty("allowLoadingLightingScenes");
        applyLightmapScaleAndOffset = serializedObject.FindProperty("applyLightmapScaleAndOffset");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(allowLoadingLightingScenes, allowLoading);    
        EditorGUILayout.PropertyField(lightingScenariosData, new GUIContent("Lighting Scenarios"), includeChildren: true);

        if (EditorGUI.EndChangeCheck())
            serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        for (int i = 0; i < lightingScenariosData.arraySize; i++)
        {
            Object data = lightingScenariosData.GetArrayElementAtIndex(i).objectReferenceValue;
            if (data != null)
            {
                Editor subEditor = null;
                CreateCachedEditor(data, null, ref subEditor);
                EditorGUILayout.LabelField(data.name, EditorStyles.boldLabel);
                EditorGUI.indentLevel++;
                subEditor.OnInspectorGUI();
                EditorGUI.indentLevel--;
            }
        }

        serializedObject.ApplyModifiedProperties();

    }
}