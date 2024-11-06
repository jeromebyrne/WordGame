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
            LoadSetupScene();
            _hasStartedLoadingMainScene = true;
        }
    }

    public void LoadSetupScene()
    {
        SceneManager.LoadSceneAsync("Assets/Scenes/GameSetupScene.unity", LoadSceneMode.Single).completed += OnSetupSceneLoaded;
    }

    private void OnSetupSceneLoaded(AsyncOperation obj)
    {
        if (obj.isDone)
        {
            Debug.Log("Setup Scene loaded successfully!");
        }
        else
        {
            Debug.LogError("Failed to load scene.");
        }
    }
}
