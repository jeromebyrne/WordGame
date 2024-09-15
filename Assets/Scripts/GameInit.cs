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

    private void Awake()
    {
        GameSettingsConfigManager.Initialize(_gameSettingsAddress);
        WordConfigManager.Initialize(_letterConfigAddress, _wordFileAddress);
    }

    private void Start()
    {
        LoadMainScene();
    }

    public void LoadMainScene()
    {
        // TODO: we should be waiting for the configs to load before switching scenes
        Addressables.LoadSceneAsync("Assets/Scenes/MainScene.unity", LoadSceneMode.Single).Completed += OnMainSceneLoaded;
    }

    private void OnMainSceneLoaded(AsyncOperationHandle<SceneInstance> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Main Scene loaded successfully!");
        }
        else
        {
            Debug.LogError("Failed to load scene.");
        }
    }
}
