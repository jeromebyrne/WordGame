using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class GameSettingsManager
{
    private static GameSettings _gameSettings;
    private static bool _isInitialized;

    public static GameSettings GameSettings
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

        Addressables.LoadAssetAsync<GameSettings>(address).Completed += OnGameSettingsLoaded;
    }

    private static void OnGameSettingsLoaded(AsyncOperationHandle<GameSettings> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _gameSettings = handle.Result;
            _isInitialized = true;
        }
        else
        {
            Debug.LogError("Failed to load GameSettings.");
        }
    }
}