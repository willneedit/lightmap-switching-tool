
Tool intended for **switching pre-baked lightmaps**, light probes and realtime lighting on a static scene at runtime.

Depending on the platform or depending on the content, the switch might not be instant but take some seconds, this script just allows you avoid duplicating your scene if you just want to change the lighting.

This version is compatible with **unity 2021.3** and above, check previous releases for unity 5.5 - 5.6 version.

If you want to use lightmaps of different resolutions in your different lighting scenarios you will probably need to **disable static batching** in the PlayerSettings (if you use the same lightmap resolution on all your lighting scenarios and the object packing in the lightmap atlas doesn't change accross lighting scenarios it's ok to keep static batching enabled).

The system relies on this component :

**LevelLightmapData**
It references the different lighting scenes, builds the lighting, and stores the dependencies to the lightmaps.

If your lighting scene contains Realtime/Mixed lights or Reflection probes, the script will consider it's necessary to load the lighting scene at runtime to replicate the full lighting. The lighting scene will thus need to be part of the "Scenes in Build" list in the Build settings (File/Build Settings).

### How it works :

- Make a scene with your static geometry only. Disable Auto baking (important). If you want to use lightprobes, also add a lightprobe group to the geometry scene.
- Use the menu **"Tools/Setup Level Lightmap Data..."** at the top to bootstrap the setting. This will create a gameobject with the setting manager as with an empty scenario data and a prefab in the **Assets/** folder.
- Move the two created items where you wish, and **save the scene**.
- Make several lighting scenes in your project. These scenes should not contain static geometry. The Lighting scene settings must not use auto baking.
- In the Create Asset Menu, use **"Lighting/Lighting Scenario Data"** to create additional lighting scenario data files for each lighting scenes - you can use the already created lighting scenario data asset, too.
- Enter the lighting scene names in the respective lighting scenario data assets.
- Add (or modify) the list in the **Level Lightmap Data** (the GameObject in your geometry scene...) to gather the created Lighting Scenario Data files. *This is optional, Lighting Scenario Data assets can be used directly to switch the lighing scenarios at runtime.*
- Using the **Level Lightmap Data**, or using the Lighting Scenario Data assets separately, use "Generate Lighting Scenario Data" in the Inspector, one by one.
- In a similar vein, use "Load lighting scenario" to test the created lighting scenarios.

### Runtime :

- In runtime, Call the public method `LoadLightingScenario` in the single `LevelLightmapData` gameobject (`FindObjectByType<>()` ... ) using an integer argument that represents the index of the lighting scenario in the list of scenarios. Or, use `Resources.Load()` to directly use a Lighting Scenario Data.
- The UI buttons in this sample project do this through the use of the button's **UnityEvent**.
- Start playing -> In the sample project, click the different buttons on the screen to switch to a different lighting. In your own project, use script or UnityEvents to call the LoadLightingScenario method as described previously

### Tutorial :
- Video tutorial? Nope.

### Supports :

- Lightmaps
- Light Probes
- Mixed lighting mode (tested only "baked indirect" and "shadowmask")
- Reflection probes, they need to be placed in the lighting scenes.

### Contributors :

- Originally created from [Laurent](https://github.com/laurenth-personal)
