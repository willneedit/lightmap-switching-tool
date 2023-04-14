using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class SetupLLDMenuItem : ScriptableObject
{
    public static void BootstrapLLD(out LevelLightmapData lld, out LightingScenarioData lsd)
    {
        GameObject go;
        go = new("Level Lightmap Data");
        lld = go.AddComponent<LevelLightmapData>();

        lsd = ObjectFactory.CreateInstance<LightingScenarioData>();
        lsd.name = "Embedded Scenario Data";
        lld.lightingScenariosData.Add(lsd);
    }

    public static void SaveLLDAssets(LevelLightmapData lld, LightingScenarioData lsd, string pathRoot = "Assets/")
    {
        AssetDatabase.CreateAsset(lsd, pathRoot + lsd.name + ".asset");
        PrefabUtility.SaveAsPrefabAsset(lld.gameObject, pathRoot + lld.gameObject.name + ".prefab");
    }

    [MenuItem("Tools/Setup Level Lightmap Data...")]
    public static void DoSetupMenuItem()
    {
        LevelLightmapData lld = FindObjectOfType<LevelLightmapData>();
        if(lld != null)
        {
            Debug.Log("There already is a level lightmap data switcher gameobject in this scene.");
            EditorGUIUtility.PingObject(lld.gameObject);
            return;
        }

        BootstrapLLD(out lld, out LightingScenarioData lsd);

        SaveLLDAssets(lld, lsd);
    }
}
