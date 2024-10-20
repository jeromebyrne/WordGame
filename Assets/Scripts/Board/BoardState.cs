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

    public static bool operator ==(BoardSlotIndex lhs, BoardSlotIndex rhs)
    {
        return lhs.Column == rhs.Column && lhs.Row == rhs.Row;
    }

    public static bool operator !=(BoardSlotIndex lhs, BoardSlotIndex rhs)
    {
        return !(lhs == rhs);
    }

    public override bool Equals(object obj)
    {
        if (obj is BoardSlotIndex)
        {
            var other = (BoardSlotIndex)obj;
            return this == other;
        }
        return false;
    }

    // Override GetHashCode for proper dictionary or hash-based collection behavior
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + Column.GetHashCode();
            hash = hash * 31 + Row.GetHashCode();
            return hash;
        }
    }
}

public interface IReadOnlyBoardState
{
    Vector2Int Dimensions { get; }
    BoardSlotState GetSlotState(BoardSlotIndex index);
    int GetCommittedTileCount();
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

    public int GetCommittedTileCount()
    {
        int count = 0;
        foreach (var column in _slots)
        {
            foreach (var slot in column)
            {
                if (slot.IsTileCommitted) { count += 1; }
            }
        }

        return count;
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
