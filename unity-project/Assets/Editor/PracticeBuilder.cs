using System;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine.XR.Management;
using WebXR;

public static class PracticeBuilder
{
    const string ScenePath = "Assets/Scenes/ViewerAR.unity";

    [MenuItem("Practice 6/Create scene and configure WebXR")]
    public static void Setup()
    {
        Directory.CreateDirectory("Assets/Scenes");
        Directory.CreateDirectory("Assets/Settings");
        Directory.CreateDirectory("Assets/Prefabs");
        CopyDirectory("Dependencies/webxr/Hidden~/WebGLTemplates", "Assets/WebGLTemplates");
        CopyDirectory("../WebXRTemplate", "Assets/WebGLTemplates/PracticeWebXR");
        PlayerSettings.companyName = "Student";
        PlayerSettings.productName = "Practice 6 - Viewer AR";
        PlayerSettings.colorSpace = ColorSpace.Linear;
        PlayerSettings.runInBackground = true;
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
        PlayerSettings.WebGL.decompressionFallback = false;
        PlayerSettings.WebGL.threadsSupport = false;
        PlayerSettings.WebGL.template = "PROJECT:PracticeWebXR";
        PlayerSettings.WebGL.initialMemorySize = 128;
        PlayerSettings.WebGL.maximumMemorySize = 512;
        PlayerSettings.SetManagedStrippingLevel(UnityEditor.Build.NamedBuildTarget.WebGL, ManagedStrippingLevel.Low);
        PlayerSettings.SetApiCompatibilityLevel(UnityEditor.Build.NamedBuildTarget.WebGL, ApiCompatibilityLevel.NET_Standard);
        var settings = AssetDatabase.LoadAssetAtPath<WebXRSettings>("Assets/Settings/WebXRSettings.asset");
        if (settings == null) {
            settings = ScriptableObject.CreateInstance<WebXRSettings>();
            AssetDatabase.CreateAsset(settings, "Assets/Settings/WebXRSettings.asset");
        }
        settings.DisableXRDisplaySubsystem = true; // Official BiRP camera path, supported since WebXR 0.22.
        settings.AutoLoadWebXRManager = false;
        settings.AutoLoadWebXRInputSystem = false;
        settings.ARRequiredReferenceSpace = WebXRSettings.ReferenceSpaceTypes.local;
        settings.AROptionalFeatures = 0;
        settings.VROptionalFeatures = 0;
        EditorBuildSettings.AddConfigObject("WebXR.Settings", settings, true);
        EditorUtility.SetDirty(settings);

        var perTarget = AssetDatabase.LoadAssetAtPath<XRGeneralSettingsPerBuildTarget>("Assets/Settings/XRGeneralSettings.asset");
        if (perTarget == null) {
            perTarget = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
            AssetDatabase.CreateAsset(perTarget, "Assets/Settings/XRGeneralSettings.asset");
        }
        EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, perTarget, true);
        if (!perTarget.HasSettingsForBuildTarget(BuildTargetGroup.WebGL))
            perTarget.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.WebGL);
        var general = perTarget.SettingsForBuildTarget(BuildTargetGroup.WebGL);
        if (general.Manager == null) {
            var manager = ScriptableObject.CreateInstance<XRManagerSettings>();
            AssetDatabase.CreateAsset(manager, "Assets/Settings/XRManager.asset");
            general.Manager = manager;
        }
        if (!general.Manager.activeLoaders.Any(l => l is WebXRLoader) &&
            !XRPackageMetadataStore.AssignLoader(general.Manager, typeof(WebXRLoader).FullName, BuildTargetGroup.WebGL))
            throw new Exception("Could not assign WebXRLoader");
        general.InitManagerOnStart = true;
        EditorUtility.SetDirty(general);
        EditorUtility.SetDirty(perTarget);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("WebXRManager");
        root.AddComponent<WebXRManager>();
        var rig = new GameObject("WebXRCameraSet");
        rig.transform.SetParent(root.transform, false);
        var poseDriver = rig.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
        poseDriver.SetPoseSource(UnityEngine.SpatialTracking.TrackedPoseDriver.DeviceType.GenericXRDevice,
            UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.Center);
        poseDriver.trackingType = UnityEngine.SpatialTracking.TrackedPoseDriver.TrackingType.RotationAndPosition;
        poseDriver.updateType = UnityEngine.SpatialTracking.TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
        var xrCamera = rig.AddComponent<WebXRCamera>();
        var serialized = new SerializedObject(xrCamera);
        string[] properties = {"cameraMain", "cameraL", "cameraR", "cameraARL", "cameraARR"};
        string[] names = {"Main Camera", "Left VR", "Right VR", "Left AR", "Right AR"};
        for (int i=0; i<properties.Length; i++) {
            var go = new GameObject(names[i]);
            go.transform.SetParent(rig.transform, false);
            var cam = go.AddComponent<Camera>();
            cam.nearClipPlane = 0.05f;
            cam.farClipPlane = 30;
            cam.fieldOfView = 55;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = i >= 3 ? Color.clear : new Color(0.08f,0.12f,0.15f,1);
            cam.enabled = i == 0;
            if (i == 0) { go.tag = "MainCamera"; go.AddComponent<AudioListener>(); }
            serialized.FindProperty(properties[i]).objectReferenceValue = cam;
        }
        serialized.ApplyModifiedPropertiesWithoutUndo();
        var left = new GameObject("Left Controller").AddComponent<WebXRController>();
        left.transform.SetParent(root.transform, false); left.hand=WebXRControllerHand.LEFT;
        var right = new GameObject("Right Controller").AddComponent<WebXRController>();
        right.transform.SetParent(root.transform, false); right.hand=WebXRControllerHand.RIGHT;
        var anchor = new GameObject("Viewer-space Object");
        anchor.transform.SetParent(root.transform, false);
        var follower = anchor.AddComponent<ViewerObject>();
        follower.leftController=left; follower.rightController=right;
        var cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.name = "Cube";
        cube.transform.SetParent(anchor.transform, false);
        cube.transform.localPosition = Vector3.zero;
        cube.transform.localScale = Vector3.one * 0.23f;
        cube.transform.localRotation = Quaternion.Euler(15,30,0);
        var material = AssetDatabase.LoadAssetAtPath<Material>("Assets/Settings/Cube.mat");
        if (material == null) {
            material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.16f,0.64f,0.47f);
            material.SetFloat("_Metallic", 0.25f);
            material.SetFloat("_Glossiness", 0.65f);
            AssetDatabase.CreateAsset(material, "Assets/Settings/Cube.mat");
        }
        cube.GetComponent<Renderer>().sharedMaterial = material;
        PrefabUtility.SaveAsPrefabAsset(cube, "Assets/Prefabs/ViewerCube.prefab");
        // The follower positions the parent; the visual remains at its local origin.
        anchor.transform.position = new Vector3(0,0,1.2f);
        cube.transform.localPosition = Vector3.zero;
        follower.visual = cube.transform;
        var sun = new GameObject("Soft key light").AddComponent<Light>();
        sun.type = LightType.Directional; sun.intensity=1.4f;
        sun.transform.rotation=Quaternion.Euler(35,-30,0);
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight=new Color(0.55f,0.58f,0.65f);
        QualitySettings.shadows=ShadowQuality.Disable;
        QualitySettings.antiAliasing=2;
        EditorSceneManager.SaveScene(scene, ScenePath);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(ScenePath,true)};
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[Practice6] Scene and WebXR configuration ready.");
    }

    [MenuItem("Practice 6/Build WebGL")]
    public static void Build()
    {
        if (!File.Exists(ScenePath)) Setup();
        CopyDirectory("../WebXRTemplate", "Assets/WebGLTemplates/PracticeWebXR");
        PlayerSettings.WebGL.template = "PROJECT:PracticeWebXR";
        AssetDatabase.Refresh();
        string output = Path.GetFullPath("../docs");
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions {
            scenes=new[]{ScenePath}, locationPathName=output, target=BuildTarget.WebGL, options=BuildOptions.None
        });
        Directory.CreateDirectory("../Verification");
        var summary = new {
            result=report.summary.result.ToString(), unity=Application.unityVersion,
            sizeBytes=report.summary.totalSize, seconds=report.summary.totalTime.TotalSeconds,
            errors=report.summary.totalErrors, warnings=report.summary.totalWarnings,
            compression=PlayerSettings.WebGL.compressionFormat.ToString(),
            decompressionFallback=PlayerSettings.WebGL.decompressionFallback,
            multithread=PlayerSettings.WebGL.threadsSupport
        };
        File.WriteAllText("../Verification/build-summary.txt", summary.ToString());
        if (report.summary.result != BuildResult.Succeeded) throw new Exception("WebGL build failed");
        Debug.Log("[Practice6] Build succeeded: " + summary);
    }

    static void CopyDirectory(string source, string destination)
    {
        Directory.CreateDirectory(destination);
        foreach(var file in Directory.GetFiles(source)) {
            if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase)) continue;
            File.Copy(file,Path.Combine(destination,Path.GetFileName(file)),true);
        }
        foreach(var dir in Directory.GetDirectories(source)) CopyDirectory(dir,Path.Combine(destination,Path.GetFileName(dir)));
    }
}
