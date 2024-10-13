public class LetterDataObj 
{
    private static uint s_uniqueIdCounter = 0;
    public uint UniqueId { get; private set; }
    private SingleLetterType _letterType;

    public LetterDataObj(SingleLetterType singleLetterType)
    {
        _letterType = singleLetterType;
        UniqueId = s_uniqueIdCounter;
        s_uniqueIdCounter = s_uniqueIdCounter + 1;
    }

    public char Character
    {
        get { return _letterType._letter; }
    }

    public int Score
    {
        get { return _letterType._points; }
    }

    private LetterDataObj() { }
}
