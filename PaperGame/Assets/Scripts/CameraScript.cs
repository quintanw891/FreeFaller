using UnityEngine;

public class CameraScript : MonoBehaviour
{
    void Start() {
        AdjustCameraFOV();
    }

    void AdjustCameraFOV() {
        // Target aspect ratio (for example, 16:9)
        float targetAspect = 16f / 9f;
        float windowAspect = (float)Screen.width / (float)Screen.height;

        Camera camera = Camera.main;

        if (windowAspect > targetAspect) {
            // Wider screen, adjust FOV to show less area
            camera.fieldOfView = Mathf.Lerp(60f, 75f, (windowAspect - targetAspect) / targetAspect);
        } else {
            // Taller screen, adjust FOV to show more area
            camera.fieldOfView = Mathf.Lerp(60f, 75f, (targetAspect - windowAspect) / targetAspect);
        }
    }
}
