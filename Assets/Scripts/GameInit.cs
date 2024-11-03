using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class GameInit : MonoBehaviour
{
    [SerializeField] private string _gameSettingsAddress;
    [SerializeField] private string _letterConfigAddress;
    [SerializeField] private string _wordFileAddress;

    private bool _hasStartedLoadingMainScene = false;

    private void Awake()
    {
        GameSettingsConfigManager.Reset();
        WordConfigManager.Reset();

        GameSettingsConfigManager.Initialize(_gameSettingsAddress);
        WordConfigManager.Initialize(_letterConfigAddress, _wordFileAddress);
    }

    private void Update()
    {
        if (_hasStartedLoadingMainScene)
        {
            return;
        }

        if (GameSettingsConfigManager.IsInitialized &&
            WordConfigManager.IsInitialized)
        {
            Debug.Log("GameSettingsConfigManager and WordConfigManager should both be initialized");
            LoadMainScene();
            _hasStartedLoadingMainScene = true;
        }
    }

    public void LoadMainScene()
    {
        SceneManager.LoadSceneAsync("Assets/Scenes/MainScene.unity", LoadSceneMode.Single).completed += OnMainSceneLoaded;
    }

    private void OnMainSceneLoaded(AsyncOperation obj)
    {
        if (obj.isDone)
        {
            Debug.Log("Main Scene loaded successfully!");
        }
        else
        {
            Debug.LogError("Failed to load scene.");
        }
    }
}
