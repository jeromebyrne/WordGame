using UnityEngine;

public class PlayerSettings : MonoBehaviour
{
    public static BoardSlotIndex GetBoardDimensions()
    {
        int rows = PlayerPrefs.GetInt("board_dimensions_rows", -1);
        int columns = PlayerPrefs.GetInt("board_dimensions_columns", -1);

        if (rows < 0 ||
            columns < 0)
        {
            return GameSettingsConfigManager.GameSettings._defaultBoardDimensions;
        }

        BoardSlotIndex retVal;
        retVal.Row = rows;
        retVal.Column = columns;

        return retVal;
    }

    public static void SetBoardDimensions(BoardSlotIndex dimensions)
    {
        PlayerPrefs.SetInt("board_dimensions_rows", dimensions.Row);
        PlayerPrefs.SetInt("board_dimensions_columns", dimensions.Column);
    }

    public static void DeleteSettings()
    {
        PlayerPrefs.DeleteAll();
    }
}
