using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameOver : MonoBehaviour
{
    [SerializeField] GameObject _winObject;
    [SerializeField] GameObject _drawObject;
    [SerializeField] TMP_Text _winnerPlayerNameLabel;
    [SerializeField] TMP_Text _winnerScoreLabel;
    [SerializeField] GameObject _bestWordObject;
    [SerializeField] TMP_Text _bestWordLabel;
    [SerializeField] TMP_Text _bestWordPlayerLabel;

    public void Populate(List<PlayerState> playerStates, List<Color> playerColors)
    {
        bool isDraw = true;

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
                highestScore = playerScore;
            }

            lastScore = playerScore;
        }

        _winObject.SetActive(!isDraw);
        _drawObject.SetActive(isDraw);

        if (!isDraw)
        {
            _winnerPlayerNameLabel.text = "Player " + highestScorePlayerIndex;
            _winnerPlayerNameLabel.color = playerColors[highestScorePlayerIndex - 1];
            _winnerScoreLabel.text = highestScore.ToString();
            _winnerScoreLabel.color = playerColors[highestScorePlayerIndex - 1];
        }

        int bestWordPlayerIndex = -1;
        int bestWordScore = 0;
        string bestScoringWord = "";

        foreach (PlayerState playerState in playerStates)
        {
            if (playerState.HighestWordScore > bestWordScore)
            {
                bestWordScore = playerState.HighestWordScore;
                bestWordPlayerIndex = playerState.PlayerIndex;
                bestScoringWord = playerState.HighestScoringWord;
            }
        }

        _bestWordObject.SetActive(bestWordPlayerIndex != -1);

        if (bestWordPlayerIndex != -1)
        {
            _bestWordLabel.text = bestScoringWord.ToUpper();
            _bestWordLabel.color = playerColors[bestWordPlayerIndex - 1];
            _bestWordPlayerLabel.text = "Player " + bestWordPlayerIndex;
            _bestWordPlayerLabel.color = playerColors[bestWordPlayerIndex - 1];
        }
    }

    public void OnNewGameButtonPressed()
    {
        GameEventHandler.Instance.TriggerEvent(RestartGameEvent.Get());
    }
}
