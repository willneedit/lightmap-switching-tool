﻿using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;
using UnityEngine.Rendering;
using System.Collections;
#if UNITY_EDITOR
using UnityEditor;
using System.IO;
#endif

[ExecuteInEditMode]
public class LevelLightmapData : MonoBehaviour
{

    [System.Serializable]
    public class RendererInfo
    {
        public int transformHash;
        public int meshHash;
        public string name;
        public int lightmapIndex;
        public Vector4 lightmapScaleOffset;
    }

    public bool latestBuildHasReltimeLights;
    [Tooltip("Enable this if you want to allow the script to load a lighting scene additively. This is useful when the scene contains a light set to realtime or mixed mode or reflection probes. If you're managing the scenes loading yourself you should disable it.")]
    public bool allowLoadingLightingScenes = true;
    [Tooltip("Enable this if you want to use different lightmap resolutions in your different lighting scenarios. In that case you'll have to disable Static Batching in the Player Settings. When disabled, Static Batching can be used but all your lighting scenarios need to use the same lightmap resolution.")]
    public bool applyLightmapScaleAndOffset = true;

    [SerializeField]
    List<LightingScenarioData> lightingScenariosData;

    public int currentLightingScenario = -1;
    public int previousLightingScenario = -1;

    //TODO : enable logs only when verbose enabled
    public bool verbose = false;

    public void LoadLightingScenarioData(LightingScenarioData data)
    {
        LightmapSettings.lightmapsMode = data.lightmapsMode;

        if (data.storeRendererInfos)
        {
            ApplyDataRendererInfo(data.rendererInfos);
        }

        LightmapSettings.lightmaps = LoadLightmaps(data);

        LoadLightProbes(data);
    }



#if UNITY_EDITOR

    // In editor only we cache the baked probe data when entering playmode, and reset it on exit
    // This negates runtime changes that the LevelLightmapData library creates in the lighting asset loaded into the starting scene 

    UnityEngine.Rendering.SphericalHarmonicsL2[] cachedBakedProbeData = null;

    public void OnEnteredPlayMode_EditorOnly()
    {
        cachedBakedProbeData = LightmapSettings.lightProbes != null ? LightmapSettings.lightProbes.bakedProbes : null;
        Debug.Log("Lightmap swtching tool - Caching editor lightProbes");
    }

    public void OnExitingPlayMode_EditorOnly()
    {
        // Only do this cache restore if we have probe data of matching length
        if (cachedBakedProbeData != null && LightmapSettings.lightProbes.bakedProbes.Length == cachedBakedProbeData.Length)
        {
            LightmapSettings.lightProbes.bakedProbes = cachedBakedProbeData;
            Debug.Log("Lightmap swtching tool - Restoring editor lightProbes");
        }
    }

#endif

    LightmapData[] LoadLightmaps(LightingScenarioData data)
    {
        if (data.lightmaps == null
                || data.lightmaps.Length == 0)
        {
            Debug.LogWarning("No lightmaps stored in scenario " + data.name);
            return null;
        }

        LightmapData[] newLightmaps = new LightmapData[data.lightmaps.Length];

        for (int i = 0; i < newLightmaps.Length; i++)
        {
            newLightmaps[i] = new LightmapData
            {
                lightmapColor = data.lightmaps[i]
            };

            if (data.lightmapsMode != LightmapsMode.NonDirectional)
            {
                newLightmaps[i].lightmapDir = data.lightmapsDir[i];
            }
            if (data.shadowMasks.Length > 0)
            {
                newLightmaps[i].shadowMask = data.shadowMasks[i];
            }
        }

        return newLightmaps;
    }

    public void ApplyDataRendererInfo(RendererInfo[] infos)
    {
        try
        {
            //TODO : find better way to handle terrain. This doesn't support multiple terrains.
            Terrain terrain = FindObjectOfType<Terrain>();
            int i = 0;
            if (terrain != null)
            {
                terrain.lightmapIndex = infos[i].lightmapIndex;
                terrain.lightmapScaleOffset = infos[i].lightmapScaleOffset;
                i++;
            }

            Dictionary<int, RendererInfo> hashRendererPairs = new();

            //Fill with lighting scenario to load renderer infos
            foreach (RendererInfo info in infos)
            {
                hashRendererPairs.Add(info.transformHash, info);
            }

            //Find all renderers
            Renderer[] renderers = FindObjectsOfType<Renderer>();

            //Apply stored scale and offset if transform and mesh hashes match
            foreach (Renderer render in renderers)
            {
                RendererInfo infoToApply = new();

                //int transformHash = render.gameObject.transform.position

                if(hashRendererPairs.TryGetValue(GetStableHash(render.gameObject.transform), out infoToApply))
                {
                    if(render.gameObject.name == infoToApply.name)
                    {
                        render.lightmapIndex = infoToApply.lightmapIndex;
                        render.lightmapScaleOffset = infoToApply.lightmapScaleOffset;
                    }
                }
            }

        }
        catch (Exception e)
        {
            if (verbose || Application.isEditor)
                Debug.LogError("Error in ApplyDataRendererInfo:" + e.GetType().ToString());
        }
    }

    public void LoadLightProbes(LightingScenarioData data)
    {
        if(data.lightProbesAsset.lightProbes.Length > 0)
        {
            try
            {
                LightmapSettings.lightProbes.bakedProbes = data.lightProbesAsset.lightProbes;
            }
            catch { Debug.LogWarning("Warning, error when trying to load lightprobes for scenario " + data.name); }
        }
    }
    public static int GetStableHash(Transform transform)
    {
        Vector3 stablePos = new(LimitDecimals(transform.position.x, 2), LimitDecimals(transform.position.y, 2), LimitDecimals(transform.position.z, 2));
        Vector3 stableRot = new(LimitDecimals(transform.rotation.x, 1), LimitDecimals(transform.rotation.y, 1), LimitDecimals(transform.rotation.z, 1));

        return stablePos.GetHashCode() + stableRot.GetHashCode();
    }
    static float LimitDecimals(float input, int decimalcount)
    {
        float multiplier = Mathf.Pow(10, decimalcount);
        return Mathf.Floor(input * multiplier) / multiplier;
    }

}
