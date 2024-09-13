using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    private void Awake()
    {
        Vector2 baseResolution = GameSettingsManager.GameSettings._baseScreenResolution;

        float targetAspect = baseResolution.x / baseResolution.y;
        float windowAspect = (float)Screen.width / (float)Screen.height;
        float scaleWidth = targetAspect / windowAspect;
        float halfWidth = baseResolution.x * 0.5f;

        float totalScale = 0.07f;

        // Ensure the camera adjusts to fit width and expands height
        if (scaleWidth >= 1.0f)
        {
            _camera.orthographicSize = halfWidth * scaleWidth * totalScale;
        }
        else
        {
            _camera.orthographicSize = halfWidth * totalScale;
        }
    }
}