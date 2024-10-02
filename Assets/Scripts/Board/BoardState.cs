using System.Collections.Generic;
using UnityEngine;

public struct BoardSlotState
{
    public BoardSlotState(bool occupied, Vector2Int boardIndex, bool isTileCommitted)
    {
        IsOccupied = occupied;
        IsTileCommitted = isTileCommitted;
        OccupiedLetter = new SingleLetterInfo();
        BoardIndex = boardIndex;
    }

    public bool IsOccupied { get; set; }
    public bool IsTileCommitted { get; set; }
    public SingleLetterInfo OccupiedLetter { get; set; }
    public Vector2Int BoardIndex { get; private set; }
}

public interface IReadOnlyBoardState
{
    Vector2Int Dimensions { get; }
    BoardSlotState GetSlotState(int row, int column);
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

    public BoardSlotState GetSlotState(int row, int column)
    {
        return _slots[row][column];
    }

    public void UpdateSlotState(int row, int column, BoardSlotState slotState)
    {
        if (row >= _slots.Count)
        {
            Debug.Log("UpdateSlotState: row is out of bounds");
            return;
        }

        if (column >= _slots[row].Count)
        {
            Debug.Log("UpdateSlotState: column is out of bounds");
            return;
        }

        _slots[row][column] = slotState;
    }
}
