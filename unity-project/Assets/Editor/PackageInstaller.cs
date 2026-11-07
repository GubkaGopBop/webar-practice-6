using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class PackageInstaller
{
    static AddAndRemoveRequest request;
    static double deadline;
    public static void Install()
    {
        request = Client.AddAndRemove(new[] {
            "file:../Dependencies/webxr", "file:../Dependencies/webxr-interactions",
            "com.unity.xr.management@4.5.3"
        }, new[] {"com.unity.collab-proxy", "com.unity.feature.development", "com.unity.multiplayer.center", "com.unity.visualscripting", "com.unity.timeline"});
        deadline = EditorApplication.timeSinceStartup + 600;
        EditorApplication.update += Poll;
    }
    static void Poll()
    {
        if (!request.IsCompleted && EditorApplication.timeSinceStartup < deadline) return;
        EditorApplication.update -= Poll;
        bool ok = request.IsCompleted && request.Status == StatusCode.Success;
        if (ok) Debug.Log("[Practice6] WebXR packages installed.");
        else Debug.LogError(request.Error?.message ?? "UPM timeout");
        EditorApplication.Exit(ok ? 0 : 1);
    }
}
