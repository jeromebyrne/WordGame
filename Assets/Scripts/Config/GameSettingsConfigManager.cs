using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class GameSettingsConfigManager
{
    private static GameSettingsConfig _gameSettings;
    private static bool _isInitialized;

    public static GameSettingsConfig GameSettings
    {
        get
        {
            if (!_isInitialized)
            {
                Debug.LogError("SettingsManager not initialized. Ensure Initialize is called.");
            }
            return _gameSettings;
        }
    }

    public static void Initialize(string address)
    {
        if (_isInitialized) return;

        Addressables.LoadAssetAsync<GameSettingsConfig>(address).Completed += OnGameSettingsLoaded;
    }

    private static void OnGameSettingsLoaded(AsyncOperationHandle<GameSettingsConfig> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _gameSettings = handle.Result;
            _isInitialized = true;

            ValidateBoardDimensions();
        }
        else
        {
            Debug.LogError("Failed to load GameSettings.");
        }
    }

    private static void ValidateBoardDimensions()
    {
        // the dimensions must be odd numbers so that there is a center tile to start from
        if (GameSettings._boardDimensions.x % 2 == 0)
        {
            Debug.LogError("Grid dimensions width has even number");
            GameSettings._boardDimensions.x -= 1;
        }

        if (GameSettings._boardDimensions.y % 2 == 0)
        {
            Debug.LogError("Grid dimensions height has even number");
            GameSettings._boardDimensions.y -= 1;
        }
    }
}