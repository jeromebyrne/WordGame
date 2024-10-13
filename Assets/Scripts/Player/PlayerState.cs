using System.Collections.Generic;

public class PlayerState
{
    private PlayerState() { }

    public PlayerState(int index)
    {
        PlayerIndex = index;
    }

    public int PlayerIndex { get; private set; }

    List<LetterDataObj> _currentPlayerLetters = new List<LetterDataObj>();

    public void AssignLetter(LetterDataObj letter)
    {
        _currentPlayerLetters.Add(letter);
    }

    public void ClearLetters()
    {
        _currentPlayerLetters = new List<LetterDataObj>();
    }

    public void RemoveLetter(LetterDataObj letterInfo)
    {
        int index = -1;
        int count = 0;
        foreach (var l in _currentPlayerLetters)
        {
            if (l.Character == letterInfo.Character)
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
