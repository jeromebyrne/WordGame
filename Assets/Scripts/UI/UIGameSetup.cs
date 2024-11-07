using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIGameSetup : MonoBehaviour
{
    [SerializeField] TMP_Dropdown _gridSizeDropDown = null;
    [SerializeField] TMP_Dropdown _gameTypeDropDown = null;
    [SerializeField] TMP_InputField _player1NameInput = null;
    [SerializeField] TMP_InputField _player2NameInput = null;

    bool _loadingMainScene = false;

    private void Start()
    {
        PopulatePlayerNames();

        PopulateGameTypeDropdown();

        PopulateGridSizeDropdown();
    }

    void PopulatePlayerNames()
    {
        string player1name = PlayerSettings.GetPlayerName(1);
        string player2name = PlayerSettings.GetPlayerName(2);

        _player1NameInput.text = player1name;
        _player2NameInput.text = player2name;
    }

    void PopulateGameTypeDropdown()
    {
        _gameTypeDropDown.ClearOptions();

        List<string> options = new List<string> { "2 Player (offline - same device)" };

        _gameTypeDropDown.AddOptions(options);

        _gameTypeDropDown.value = 0;
    }

    void PopulateGridSizeDropdown()
    {
        _gridSizeDropDown.ClearOptions();

        List<string> options = new List<string> { "11x11 (Extra small)",
            "13x13 (Small)", "15x15 (Regular)", "17x17 (Large)" };

        _gridSizeDropDown.AddOptions(options);

        var gridDimensions = PlayerSettings.GetBoardDimensions(); // will return defaul dimensions config value if none set

        string defaultGridAsString = gridDimensions.Column.ToString() + "x" + gridDimensions.Row.ToString();

        int defaultIndex = -1;
        int currentIndex = 0;

        foreach (string option in options)
        {
            if (option.Contains(defaultGridAsString))
            {
                defaultIndex = currentIndex;
                break;
            }
            currentIndex++;
        }

        _gridSizeDropDown.value = defaultIndex == -1 ? 1 : defaultIndex;
    }

    public void OnContinueButtonPressed()
    {
        SetPreferences();

        LoadMainScene();
    }

    public void OnDeleteSettingsPressed()
    {
        PlayerSettings.DeleteSettings();
    }

    void SetPreferences()
    {
        string selectedGridDimensionsStr = _gridSizeDropDown.options[_gridSizeDropDown.value].text;

        string[] parts = selectedGridDimensionsStr.Split(new char[] { 'x', ' ' }, System.StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length >= 2 && int.TryParse(parts[0], out int column) && int.TryParse(parts[1], out int row))
        {
            BoardSlotIndex dimensions;
            dimensions.Row = row;
            dimensions.Column = column;

            PlayerSettings.SetBoardDimensions(dimensions);
        }
        else
        {
            Debug.LogError("Failed to parse dimensions from the option text.");
        }

        // Set player name
        string p1name = _player1NameInput.text == "" ? "Player 1" : _player1NameInput.text;
        string p2name = _player2NameInput.text == "" ? "Player 2" : _player2NameInput.text;
        PlayerSettings.SetPlayerName(1, p1name);
        PlayerSettings.SetPlayerName(2, p2name);
    }

    public void LoadMainScene()
    {
        if (_loadingMainScene)
        {
            return;
        }

        _loadingMainScene = true;
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
        _loadingMainScene = false;
    }
}
