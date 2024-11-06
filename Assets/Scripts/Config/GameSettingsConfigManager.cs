using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class GameSettingsConfigManager
{
    private static GameSettingsConfig _gameSettings;
    public static bool IsInitialized { get; private set; } = false;

    public static GameSettingsConfig GameSettings
    {
        get
        {
            if (!IsInitialized)
            {
                Debug.LogError("SettingsManager not initialized. Ensure Initialize is called.");
            }
            return _gameSettings;
        }
    }

    public static void Reset()
    {
        IsInitialized = false;
        _gameSettings = null;
    }

    public static void Initialize(string address)
    {
        if (IsInitialized) return;

        Addressables.LoadAssetAsync<GameSettingsConfig>(address).Completed += OnGameSettingsLoaded;
    }

    private static void OnGameSettingsLoaded(AsyncOperationHandle<GameSettingsConfig> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _gameSettings = handle.Result;
            IsInitialized = true;

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
        if (GameSettings._defaultBoardDimensions.Column % 2 == 0)
        {
            Debug.LogError("Grid dimensions column has even number");
            GameSettings._defaultBoardDimensions.Column -= 1;
        }

        if (GameSettings._defaultBoardDimensions.Row % 2 == 0)
        {
            Debug.LogError("Grid dimensions row has even number");
            GameSettings._defaultBoardDimensions.Row -= 1;
        }
    }
}