using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct BoardSlotState
{
    public BoardSlotState(bool occupied, BoardSlotIndex boardIndex, bool isTileCommitted, TileBonusType bonusType)
    {
        IsOccupied = occupied;
        IsTileCommitted = isTileCommitted;
        OccupiedLetter = null;
        BoardIndex = boardIndex;
        BonusType = bonusType;
    }

    public bool IsOccupied { get; set; }
    public bool IsTileCommitted { get; set; }
    public LetterDataObj OccupiedLetter { get; set; }
    public BoardSlotIndex BoardIndex { get; private set; }
    public TileBonusType BonusType { get; private set; }
}

[System.Serializable]
public struct BoardSlotIndex
{
    public int Column;
    public int Row;

    public Vector2Int ToVector2Int()
    {
        return new Vector2Int(Column, Row);
    }

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

public enum TileBonusType
{
    kNone,
    kDoubleWord,
    kTripleWord,
    kDoubleLetter,
    kTripleLetter,
    kCenterTile
}

public interface IReadOnlyBoardState
{
    BoardSlotIndex Dimensions { get; }
    BoardSlotState GetSlotState(BoardSlotIndex index);
    int GetCommittedTileCount();
    bool IsCenterTileOccupied(); // on the first turn, the center tile should be occupied
    List<BoardSlotState> GetAllSlotStatesFlattened();
}

public class BoardState : IReadOnlyBoardState
{
    private readonly List<List<BoardSlotState>> _slots = new List<List<BoardSlotState>>();

    private BoardState() { }

    public BoardSlotIndex Dimensions { get; private set; }

    public BoardState(BoardSlotIndex dimensions)
    {
        Dimensions = dimensions;

        for (int column = 0; column < dimensions.Column; column++)
        {
            _slots.Add(new List<BoardSlotState>());
            for (int row = 0; row < dimensions.Row; row++)
            {
                BoardSlotState slotState = new BoardSlotState(false, new BoardSlotIndex { Row = row, Column = column }, false, GetTileBonusForIndex(row, column));
                _slots[column].Add(slotState);
            }
        }
    }

    public List<BoardSlotState> GetAllSlotStatesFlattened()
    {
        List<BoardSlotState> flatList = _slots.SelectMany(subList => subList).ToList();
        return flatList;
    }

    private TileBonusType GetTileBonusForIndex(int row, int column)
    {
        int maxRow = Dimensions.Row - 1;
        int maxColumn = Dimensions.Column - 1;

        if (row == maxRow / 2 && column == maxColumn / 2)
        {
            return TileBonusType.kCenterTile;
        }

        if (column == (maxColumn / 2) - 1 ||
            column == (maxColumn / 2) + 1 ||
            row == (maxRow / 2) - 1 ||
            row == (maxRow / 2) + 1)
        {
            // exlude tiles around the center
            return TileBonusType.kNone;
        }

        // Triple Word Score: Corner and mid-column/row positions
        if ((row == 0 && column == 0) || (row == 0 && column == maxColumn) ||
            (row == maxRow && column == 0) || (row == maxRow && column == maxColumn) ||
            (row == maxRow / 2 && column == 0) || (row == maxRow / 2 && column == maxColumn) ||
            (row == 0 && column == maxColumn / 2) || (row == maxRow && column == maxColumn / 2))
        {
            return TileBonusType.kTripleWord;
        }

        // Double Letter Score: Along the diagonals
        if (row == column || row + column == maxRow)
        {
            return TileBonusType.kDoubleLetter;
        }

        // Triple Letter Score: Spread near the edges
        if ((row == 2 && column == maxColumn / 2) || (row == maxRow - 2 && column == maxColumn / 2) ||
            (row == maxRow / 2 && column == 2) || (row == maxRow / 2 && column == maxColumn - 2))
        {
            return TileBonusType.kTripleLetter;
        }

        // Double Word Score: Spread near the center
        if ((row == 2 && (column == maxColumn / 3 || column == maxColumn - maxColumn / 3)) ||
            (row == maxRow - 2 && (column == maxColumn / 3 || column == maxColumn - maxColumn / 3)) ||
            (row == maxRow / 3 && (column == 2 || column == maxColumn - 2)) ||
            (row == maxRow - maxRow / 3 && (column == 2 || column == maxColumn - 2)))
        {
            return TileBonusType.kDoubleWord;
        }

        return TileBonusType.kNone;
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

    public List<uint> GetUncommittedTileIds()
    {
        List<uint> tileIds = new List<uint>();

        foreach (var column in _slots)
        {
            foreach (var slot in column)
            {
                if (slot.IsOccupied && !slot.IsTileCommitted)
                {
                    tileIds.Add(slot.OccupiedLetter.UniqueId);
                }
            }
        }

        return tileIds;
    }

    public List<uint> GetAllBoardTileIds()
    {
        // include committed tiles
        List<uint> tileIds = new List<uint>();

        foreach (var column in _slots)
        {
            foreach (var slot in column)
            {
                if (slot.IsOccupied)
                {
                    tileIds.Add(slot.OccupiedLetter.UniqueId);
                }
            }
        }

        return tileIds;
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

    public bool IsCenterTileOccupied()
    {
        int x = _slots.Count / 2;
        int y = _slots[0].Count / 2;

        return _slots[x][y].IsOccupied;
    }
}
