using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class GameInit : MonoBehaviour
{
    [SerializeField] private string _gameSettingsAddress;

    private void Awake()
    {
        GameSettingsManager.Initialize(_gameSettingsAddress);
    }

    private void Start()
    {
        LoadMainScene();
    }

    public void LoadMainScene()
    {
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
