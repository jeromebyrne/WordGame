using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    [SerializeField] GameObject _winObject;
    [SerializeField] GameObject _drawObject;

    public void OnNewGameButtonPressed()
    {

    }

    public void Populate(List<PlayerState> playerStates, Dictionary<int, Color> playerColors)
    {
        bool isDraw = false;

        int lastScore = playerStates[0].Score;
        int highestScore = lastScore;
        int highestScorePlayerIndex = 1;

        for (int i = 1; i < playerStates.Count; i++)
        {
            int playerScore = playerStates[i].Score;
            if (playerScore != lastScore)
            {
                isDraw = false;
            }

            if (playerScore > highestScore)
            {
                highestScorePlayerIndex = i + 1;
            }

            lastScore = playerScore;
        }

        _winObject.SetActive(!isDraw);
        _drawObject.SetActive(isDraw);
    }
}
