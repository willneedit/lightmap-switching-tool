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
        IEnumerator EditorGenerateLightingScenarioData(LightingScenarioData scenarioData)
        {
            yield return null;

            LoadLightingScenarioScenes(scenarioData);
            //Check if the lighting scene needs requires dynamic lighting ( if not, never try to load the lighting scene ).
            scenarioData.hasRealtimeLights = SearchLightsNeededRealtime();
            Debug.Log("Lightmap switcher - Start baking");
            //Remove reference to LightingDataAsset so that Unity doesn't delete the previous bake
            Lightmapping.lightingDataAsset = null;
            EditorCoroutineUtility.StartCoroutine(BuildLightingAsync(scenarioData), this);
        }

        IEnumerator EditorLoadLightingScenario(LightingScenarioData scenarioData)
        {
            yield return null;

            LoadLightingScenarioScenes(scenarioData);
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
            EditorCoroutineUtility.StartCoroutine(EditorGenerateLightingScenarioData(scenarioData), this);

        if (GUILayout.Button("Load Lighting scenario"))
            EditorCoroutineUtility.StartCoroutine(EditorLoadLightingScenario(scenarioData), this);

        serializedObject.ApplyModifiedProperties();
    }

    public void LoadLightingScenarioScenes(LightingScenarioData scenarioData)
    {
        if (UnityEditor.Lightmapping.giWorkflowMode != UnityEditor.Lightmapping.GIWorkflowMode.OnDemand)
        {
            Debug.LogError("Lightmap switcher requires Auto Generate lighting mode disabled.");
            return;
        }

        Debug.Log("Loading scenario " + scenarioData.name);

        if(EditorApplication.isPlaying)
        {
            Debug.LogWarning("Skipping scene loading for the play mode.");
            return;
        }

        if(scenarioData.geometrySceneName == "" )
        {
            Debug.LogError("Geometry scene name cannot be null. Stopping generation.");
            return;
        }
        if (scenarioData.lightingSceneName == "")
        {
            Debug.LogError("Lighting scene name cannot be null. Stopping generation.");
            return;
        }
        string lightingSceneGUID = AssetDatabase.FindAssets(scenarioData.lightingSceneName)[0];
        string lightingScenePath = AssetDatabase.GUIDToAssetPath(lightingSceneGUID);
        if (!lightingScenePath.EndsWith(".unity"))
            lightingScenePath += ".unity";

        string geometrySceneGUID = AssetDatabase.FindAssets(scenarioData.geometrySceneName)[0];
        string geometryScenePath = AssetDatabase.GUIDToAssetPath(geometrySceneGUID);
        if (!geometryScenePath.EndsWith(".unity"))
            geometryScenePath += ".unity";

        EditorSceneManager.OpenScene(geometryScenePath);
        Lightmapping.lightingDataAsset = null;
        EditorSceneManager.OpenScene(lightingScenePath, OpenSceneMode.Additive);
        Scene lightingScene = SceneManager.GetSceneByName(scenarioData.lightingSceneName);
        EditorSceneManager.SetActiveScene(lightingScene);
    }

    private IEnumerator BuildLightingAsync(LightingScenarioData scenarioData)
    {
        Scene lightingScene = SceneManager.GetSceneByName(scenarioData.lightingSceneName);
        Scene geometryScene = SceneManager.GetSceneByName(scenarioData.geometrySceneName);

        Lightmapping.BakeAsync();
        while (Lightmapping.isRunning) { yield return null; }
        EditorSceneManager.SaveScene(geometryScene);
        EditorSceneManager.SaveScene(lightingScene);
        StoreLightingData();
        EditorSceneManager.CloseScene(lightingScene, true);
        AssetDatabase.SaveAssets();
    }

    public bool SearchLightsNeededRealtime()
    {
        bool latestBuildHasRealtimeLights = false;

        Light[] lights = FindObjectsOfType<Light>();
        ReflectionProbe[] reflectionProbes = FindObjectsOfType<ReflectionProbe>();

        foreach (Light light in lights)
        {
            if (light.lightmapBakeType == LightmapBakeType.Mixed || light.lightmapBakeType == LightmapBakeType.Realtime)
                latestBuildHasRealtimeLights = true;
        }
        if (reflectionProbes.Length > 0)
            latestBuildHasRealtimeLights = true;

        return latestBuildHasRealtimeLights;
    }

    public void StoreLightingData()
    {
        LightingScenarioData scenarioData = (LightingScenarioData)target;

        GenerateLightingData(scenarioData);
        if (scenarioData.lightProbesAsset == null)
        {
            LightProbesAsset probes = ScriptableObject.CreateInstance<LightProbesAsset>();
            string path = AssetDatabase.GetAssetPath(scenarioData);
            if (path == "")
            {
                path = "Assets";
            }
            else if (Path.GetExtension(path) != "")
            {
                path = path.Replace(Path.GetFileName(AssetDatabase.GetAssetPath(scenarioData)), "");
            }

            string assetPathAndName = AssetDatabase.GenerateUniqueAssetPath(path + scenarioData.name + "_LightProbes" + ".asset");
            AssetDatabase.CreateAsset(probes, assetPathAndName);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            scenarioData.lightProbesAsset = probes;
        }
        scenarioData.lightProbesAsset.lightProbes = LightmapSettings.lightProbes.bakedProbes;

        EditorUtility.SetDirty(scenarioData.lightProbesAsset);
        EditorUtility.SetDirty(scenarioData);
        AssetDatabase.SaveAssets();
    }

    static void GenerateLightingData(LightingScenarioData data)
    {
        List<LevelLightmapData.RendererInfo> newRendererInfos = new();
        List<Texture2D> newLightmapsLight = new();
        List<Texture2D> newLightmapsDir = new();
        List<Texture2D> newLightmapsShadow = new();

        data.lightmapsMode = LightmapSettings.lightmapsMode;

        //TODO : Fin better solution for terrain. This is not compatible with several terrains.
        Terrain terrain = FindObjectOfType<Terrain>();
        if (terrain != null && terrain.lightmapIndex != -1 && terrain.lightmapIndex != 65534)
        {
            LevelLightmapData.RendererInfo terrainRendererInfo = new()
            {
                lightmapScaleOffset = terrain.lightmapScaleOffset
            };

            Texture2D lightmaplight = LightmapSettings.lightmaps[terrain.lightmapIndex].lightmapColor;
            terrainRendererInfo.lightmapIndex = newLightmapsLight.IndexOf(lightmaplight);
            if (terrainRendererInfo.lightmapIndex == -1)
            {
                terrainRendererInfo.lightmapIndex = newLightmapsLight.Count;
                newLightmapsLight.Add(lightmaplight);
            }

            if (data.lightmapsMode != LightmapsMode.NonDirectional)
            {
                Texture2D lightmapdir = LightmapSettings.lightmaps[terrain.lightmapIndex].lightmapDir;
                terrainRendererInfo.lightmapIndex = newLightmapsDir.IndexOf(lightmapdir);
                if (terrainRendererInfo.lightmapIndex == -1)
                {
                    terrainRendererInfo.lightmapIndex = newLightmapsDir.Count;
                    newLightmapsDir.Add(lightmapdir);
                }
            }
            if (LightmapSettings.lightmaps[terrain.lightmapIndex].shadowMask != null)
            {
                Texture2D lightmapShadow = LightmapSettings.lightmaps[terrain.lightmapIndex].shadowMask;
                terrainRendererInfo.lightmapIndex = newLightmapsShadow.IndexOf(lightmapShadow);
                if (terrainRendererInfo.lightmapIndex == -1)
                {
                    terrainRendererInfo.lightmapIndex = newLightmapsShadow.Count;
                    newLightmapsShadow.Add(lightmapShadow);
                }
            }
            if (data.storeRendererInfos)
            {
                newRendererInfos.Add(terrainRendererInfo);
                if (Application.isEditor)
                    Debug.Log("Terrain lightmap stored in" + terrainRendererInfo.lightmapIndex.ToString());
            }

        }

        Renderer[] renderers = FindObjectsOfType<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer.lightmapIndex != -1 && renderer.lightmapIndex != 65534)
            {
                LevelLightmapData.RendererInfo info = new()
                {
                    transformHash = LevelLightmapData.GetStableHash(renderer.gameObject.transform),
                    meshHash = renderer.gameObject.GetComponent<MeshFilter>().sharedMesh.vertexCount,
                    name = renderer.gameObject.name,
                    lightmapScaleOffset = renderer.lightmapScaleOffset
                };

                Texture2D lightmaplight = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapColor;
                info.lightmapIndex = newLightmapsLight.IndexOf(lightmaplight);
                if (info.lightmapIndex == -1)
                {
                    info.lightmapIndex = newLightmapsLight.Count;
                    newLightmapsLight.Add(lightmaplight);
                }

                if (data.lightmapsMode != LightmapsMode.NonDirectional)
                {
                    Texture2D lightmapdir = LightmapSettings.lightmaps[renderer.lightmapIndex].lightmapDir;
                    info.lightmapIndex = newLightmapsDir.IndexOf(lightmapdir);
                    if (info.lightmapIndex == -1)
                    {
                        info.lightmapIndex = newLightmapsDir.Count;
                        newLightmapsDir.Add(lightmapdir);
                    }
                }
                if (LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask != null)
                {
                    Texture2D lightmapShadow = LightmapSettings.lightmaps[renderer.lightmapIndex].shadowMask;
                    info.lightmapIndex = newLightmapsShadow.IndexOf(lightmapShadow);
                    if (info.lightmapIndex == -1)
                    {
                        info.lightmapIndex = newLightmapsShadow.Count;
                        newLightmapsShadow.Add(lightmapShadow);
                    }
                }

                if(data.storeRendererInfos) newRendererInfos.Add(info);
            }
        }

        if(data.storeRendererInfos && Application.isEditor)
            Debug.Log("stored info for " + renderers.Length + " meshrenderers");

        data.lightmaps = newLightmapsLight.ToArray();
        data.lightmapsDir = newLightmapsDir.ToArray();
        data.shadowMasks = newLightmapsShadow.ToArray();
        data.rendererInfos = newRendererInfos.ToArray();
    }
}