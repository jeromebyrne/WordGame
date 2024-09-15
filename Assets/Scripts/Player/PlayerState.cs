using System.Collections;
using System.Collections.Generic;

public class PlayerState
{
    private PlayerState() { }

    public PlayerState(int index)
    {
        PlayerIndex = index;
    }

    public int PlayerIndex { get; private set; }

    List<SingleLetterInfo> _currentPlayerLetters = new List<SingleLetterInfo>();

    public void AssignLetter(SingleLetterInfo letter)
    {
        _currentPlayerLetters.Add(letter);
    }

    public void ClearLetters()
    {
        _currentPlayerLetters = new List<SingleLetterInfo>();
    }

    public void RemoveLetter(SingleLetterInfo letterInfo)
    {
        int index = -1;
        int count = 0;
        foreach (var l in _currentPlayerLetters)
        {
            if (l._letter == letterInfo._letter)
            {
                index = count;
                break;
            }
            ++count;
        }

        if (index > -1)
        {
            _currentPlayerLetters.RemoveAt(index);
        }
    }
}
