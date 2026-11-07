using UnityEngine;
using WebXR;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

// A viewer-space object follows the active camera rather than a world anchor.
[DefaultExecutionOrder(1000)]
public sealed class ViewerObject : MonoBehaviour
{
    [Min(0.3f)] public float distance = 1.2f;
    public float rotationSpeed = 22f;
    public WebXRController leftController;
    public WebXRController rightController;
    public Transform visual;
    int frames;
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")] static extern void Practice6Report(int frame, float x, float y, float distance, int ar);
#endif

    void LateUpdate()
    {
        Camera view = Camera.main;
        if (view == null || !view.isActiveAndEnabled) return;
        transform.position = view.transform.position + view.transform.forward * distance;
        transform.rotation = view.transform.rotation;
        if (visual != null) visual.Rotate(Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);
        if ((leftController != null && leftController.GetButtonDown(WebXRController.ButtonTypes.Trigger)) ||
            (rightController != null && rightController.GetButtonDown(WebXRController.ButtonTypes.Trigger)))
            ResetRotation();
        if (++frames == 120)
            Debug.Log("[Practice6] Viewer-space active; frame=" + Time.frameCount + "; camera=" + view.name);
#if UNITY_WEBGL && !UNITY_EDITOR
        if (frames % 120 == 0) {
            Vector3 screen = view.WorldToViewportPoint(transform.position);
            Practice6Report(Time.frameCount, screen.x, screen.y, screen.z,
                WebXRManager.Instance != null && WebXRManager.Instance.XRState == WebXRState.AR ? 1 : 0);
        }
#endif
    }

    public void ResetRotation()
    {
        if (visual != null) visual.localRotation = Quaternion.Euler(15, 30, 0);
    }
}
