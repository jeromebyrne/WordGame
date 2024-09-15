using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    [SerializeField] private Camera _camera;

    private void Awake()
    {
        AdjustView();
    }

    void AdjustView()
    {
        Vector2 baseResolution = GameSettingsConfigManager.GameSettings._baseScreenResolution;

        float targetAspect = baseResolution.x / baseResolution.y;
        float windowAspect = (float)Screen.width / (float)Screen.height;

        // Calculate the orthographic size to ensure it always fits the height
        float scaleHeight = windowAspect / targetAspect;

        float scaleOverride = 0.0105f;

        // TODO: I should actually take the board grid dimensions into consideration

        if (scaleHeight >= 1.0f)
        {
            _camera.orthographicSize = baseResolution.y / 2 * scaleOverride;
        }
        else
        {
            _camera.orthographicSize = (baseResolution.y / 2 * scaleOverride) / scaleHeight;
        }
    }
}