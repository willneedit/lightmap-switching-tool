using System;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "LightingScenario", menuName = "Lighting/Lighting Scenario Data")]
public class LightingScenarioData : ScriptableObject
{
    [FormerlySerializedAs("sceneName")]
    public string lightingSceneName;
    public string geometrySceneName;
    public bool storeRendererInfos;
    public LevelLightmapData.RendererInfo[] rendererInfos;
    public Texture2D[] lightmaps;
    public Texture2D[] lightmapsDir;
    public Texture2D[] shadowMasks;
    public LightmapsMode lightmapsMode;
    public LightProbesAsset lightProbesAsset;
    public bool hasRealtimeLights;

    public SerializedRenderSettings renderSettings;

    public bool HasRenderSettings { get => renderSettings.hasRenderSettings; }
    public void SnapshotRenderSettings() => renderSettings.SnapshotRenderSettings();
    public void RestoreRenderSettings() => renderSettings.RestoreRenderSettings();
}

[Serializable]
public class SerializedRenderSettings
{
    public bool hasRenderSettings = false;

    public Color ambientSkyColor;
    public Color ambientEquatorColor;
    public Color ambientGroundColor;
    public float ambientIntensity;
    public Color ambientLight;
    public UnityEngine.Rendering.AmbientMode ambientMode;
    // public Rendering.SphericalHarmonicsL2 ambientProbe;
    public UnityEngine.Rendering.DefaultReflectionMode defaultReflectionMode;
    public int defaultReflectionResolution;
    public float flareFadeSpeed;
    public float flareStrength;
    public bool fog;
    public Color fogColor;
    public float fogDensity;
    public float fogEndDistance;
    public float fogStartDistance;
    public FogMode fogMode;
    public float haloStrength;
    public int reflectionBounces;
    public float reflectionIntensity;
    public Color subtractiveShadowColor;

    public Light sun;
    public Texture customReflection;
    public Material skybox;

    public void SnapshotRenderSettings()
    {
        ambientSkyColor = RenderSettings.ambientSkyColor;
        ambientEquatorColor = RenderSettings.ambientEquatorColor;
        ambientGroundColor = RenderSettings.ambientGroundColor;
        ambientIntensity = RenderSettings.ambientIntensity;
        ambientLight = RenderSettings.ambientLight;
        ambientMode = RenderSettings.ambientMode;
        //ambientProbe = RenderSettings.ambientProbe;
        defaultReflectionMode = RenderSettings.defaultReflectionMode;
        defaultReflectionResolution = RenderSettings.defaultReflectionResolution;
        flareFadeSpeed = RenderSettings.flareFadeSpeed;
        flareStrength = RenderSettings.flareStrength;
        fog = RenderSettings.fog;
        fogColor = RenderSettings.fogColor;
        fogDensity = RenderSettings.fogDensity;
        fogEndDistance = RenderSettings.fogEndDistance;
        fogStartDistance = RenderSettings.fogStartDistance;
        fogMode = RenderSettings.fogMode;
        haloStrength = RenderSettings.haloStrength;
        reflectionBounces = RenderSettings.reflectionBounces;
        reflectionIntensity = RenderSettings.reflectionIntensity;
        subtractiveShadowColor = RenderSettings.subtractiveShadowColor;

        sun = RenderSettings.sun;
        customReflection = RenderSettings.customReflection;
        skybox = RenderSettings.skybox;

        hasRenderSettings = true;
    }

    public void RestoreRenderSettings()
    {
        RenderSettings.ambientMode = ambientMode;
        switch(ambientMode)
        {
            case UnityEngine.Rendering.AmbientMode.Skybox:
                RenderSettings.ambientIntensity = ambientIntensity;
                break;
            case UnityEngine.Rendering.AmbientMode.Trilight:
                RenderSettings.ambientSkyColor = ambientSkyColor;
                RenderSettings.ambientEquatorColor = ambientEquatorColor;
                RenderSettings.ambientGroundColor = ambientGroundColor;
                break;
            case UnityEngine.Rendering.AmbientMode.Flat:
                RenderSettings.ambientSkyColor = ambientSkyColor;
                break;
            case UnityEngine.Rendering.AmbientMode.Custom:
                RenderSettings.ambientLight = ambientLight;
                break;
        }

        //RenderSettings.ambientProbe = ambientProbe;
        RenderSettings.defaultReflectionMode = defaultReflectionMode;
        RenderSettings.defaultReflectionResolution = defaultReflectionResolution;
        RenderSettings.flareFadeSpeed = flareFadeSpeed;
        RenderSettings.flareStrength = flareStrength;
        RenderSettings.fog = fog;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        RenderSettings.fogEndDistance = fogEndDistance;
        RenderSettings.fogStartDistance = fogStartDistance;
        RenderSettings.fogMode = fogMode;
        RenderSettings.haloStrength = haloStrength;
        RenderSettings.reflectionBounces = reflectionBounces;
        RenderSettings.reflectionIntensity = reflectionIntensity;
        RenderSettings.subtractiveShadowColor = subtractiveShadowColor;

        RenderSettings.sun = sun;
        RenderSettings.customReflection = customReflection;
        RenderSettings.skybox = skybox;
    }
}