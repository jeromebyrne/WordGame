using System.Collections.Generic;
using UnityEngine;

public struct BoardSlotState
{
    public BoardSlotState(bool occupied, Vector2Int boardIndex, bool isTileCommitted)
    {
        IsOccupied = occupied;
        IsTileCommitted = isTileCommitted;
        OccupiedLetter = null;
        BoardIndex = boardIndex;
    }

    public bool IsOccupied { get; set; }
    public bool IsTileCommitted { get; set; }
    public LetterDataObj OccupiedLetter { get; set; }
    public Vector2Int BoardIndex { get; private set; }
}

public struct BoardSlotIndex
{
    public int Column;
    public int Row;
}

public interface IReadOnlyBoardState
{
    Vector2Int Dimensions { get; }
    BoardSlotState GetSlotState(BoardSlotIndex index);
}

public class BoardState : IReadOnlyBoardState
{
    private readonly List<List<BoardSlotState>> _slots = new List<List<BoardSlotState>>();

    private BoardState() { }

    public Vector2Int Dimensions { get; private set; }

    public BoardState(Vector2Int dimensions)
    {
        Dimensions = dimensions;

        for (int column = 0; column < dimensions.x; column++)
        {
            _slots.Add(new List<BoardSlotState>());
            for (int row = 0; row < dimensions.y; row++)
            {
                BoardSlotState slotState = new BoardSlotState(false, new Vector2Int(row, column), false);
                _slots[column].Add(slotState);
            }
        }
    }

    public BoardSlotState GetSlotState(BoardSlotIndex index)
    {
        return _slots[index.Row][index.Column];
    }

    public void UpdateSlotState(BoardSlotIndex index, BoardSlotState slotState)
    {
        if (index.Row >= _slots.Count)
        {
            Debug.Log("UpdateSlotState: row is out of bounds");
            return;
        }

        if (index.Column >= _slots[index.Column].Count)
        {
            Debug.Log("UpdateSlotState: column is out of bounds");
            return;
        }

        _slots[index.Row][index.Column] = slotState;
    }
}
