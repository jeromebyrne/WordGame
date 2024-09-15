using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public static class WordConfigManager
{
    private static LetterConfig _letterConfig;
    private static bool _isInitialized;
    private static HashSet<string> _wordSet;

    public static LetterConfig LetterConfig
    {
        get
        {
            if (!_isInitialized)
            {
                Debug.LogError("LetterConfig not initialized. Ensure Initialize is called.");
            }
            return _letterConfig;
        }
    }

    public static void Initialize(string letterDataAddress, string wordFileAddress)
    {
        if (_isInitialized) return;

        Addressables.LoadAssetAsync<LetterConfig>(letterDataAddress).Completed += OnLetterConfigLoaded;
        Addressables.LoadAssetAsync<TextAsset>(wordFileAddress).Completed += OnWordFileLoaded;
    }

    private static void OnWordFileLoaded(AsyncOperationHandle<TextAsset> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _wordSet = new HashSet<string>();

            TextAsset wordFile = handle.Result;

            string[] words = wordFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            foreach (string word in words)
            {
                _wordSet.Add(word.ToLower());
            }
        }
        else
        {
            Debug.LogError("Failed to load words file.");
        }
    }

    private static void OnLetterConfigLoaded(AsyncOperationHandle<LetterConfig> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            _letterConfig = handle.Result;
            _isInitialized = true;
        }
        else
        {
            Debug.LogError("Failed to load LetterConfig.");
        }
    }

    public static bool IsValidWord(string word)
    {
        return _wordSet.Contains(word.ToLower());
    }
}