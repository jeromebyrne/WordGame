using UnityEngine;

[CreateAssetMenu(fileName = "GameSettingsConfig", menuName = "Settings/GameSettingsConfig")]
public class GameSettingsConfig : ScriptableObject
{
    public Vector2Int _baseScreenResolution;
    public BoardSlotIndex _defaultBoardDimensions;
    public int _maxPlayerLetters = 7;
}