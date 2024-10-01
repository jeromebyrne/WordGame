using System.Collections.Generic;
using UnityEngine;

public static class BoardDataHelper
{
    public static List<Vector2Int> GetUncommittedTiles(BoardState boardState)
    {
        List<Vector2Int> uncommittedTiles = new List<Vector2Int>();

        for (int row = 0; row < boardState.Dimensions.y; row++)
        {
            for (int column = 0; column < boardState.Dimensions.x; column++)
            {
                BoardSlotState slot = boardState.GetSlotState(row, column);
                if (slot.IsOccupied && !slot.IsTileCommitted)
                {
                    uncommittedTiles.Add(new Vector2Int(row, column));
                }
            }
        }

        return uncommittedTiles;
    }

    public static bool AreTilesContiguous(List<Vector2Int> uncommittedTiles, BoardState boardState, out List<Vector2Int> contiguousTiles)
    {
        contiguousTiles = new List<Vector2Int>();

        if (uncommittedTiles.Count <= 1)
        {
            contiguousTiles.AddRange(uncommittedTiles);
            return true; // One or zero tiles are trivially contiguous
        }

        // Check if all tiles are in the same row or column
        bool allInSameRow = true;
        bool allInSameColumn = true;

        for (int i = 1; i < uncommittedTiles.Count; i++)
        {
            if (uncommittedTiles[i].x != uncommittedTiles[0].x)
            {
                allInSameRow = false;
            }
            if (uncommittedTiles[i].y != uncommittedTiles[0].y)
            {
                allInSameColumn = false;
            }
        }

        if (!allInSameRow && !allInSameColumn)
        {
            return false; // If they're not in the same row or column, they're not contiguous
        }

        // Sort by row or column depending on alignment
        if (allInSameRow)
        {
            SortByColumn(uncommittedTiles);
            return CheckContiguityWithGaps(uncommittedTiles, true, boardState, out contiguousTiles);
        }
        else if (allInSameColumn)
        {
            SortByRow(uncommittedTiles);
            return CheckContiguityWithGaps(uncommittedTiles, false, boardState, out contiguousTiles);
        }

        return false;
    }

    // Helper method to sort uncommitted tiles by column (y-axis) for rows
    private static void SortByColumn(List<Vector2Int> tiles)
    {
        for (int i = 0; i < tiles.Count - 1; i++)
        {
            for (int j = i + 1; j < tiles.Count; j++)
            {
                if (tiles[i].y > tiles[j].y)
                {
                    Vector2Int temp = tiles[i];
                    tiles[i] = tiles[j];
                    tiles[j] = temp;
                }
            }
        }
    }

    // Helper method to sort uncommitted tiles by row (x-axis) for columns
    private static void SortByRow(List<Vector2Int> tiles)
    {
        for (int i = 0; i < tiles.Count - 1; i++)
        {
            for (int j = i + 1; j < tiles.Count; j++)
            {
                if (tiles[i].x > tiles[j].x)
                {
                    Vector2Int temp = tiles[i];
                    tiles[i] = tiles[j];
                    tiles[j] = temp;
                }
            }
        }
    }

    private static bool CheckContiguityWithGaps(List<Vector2Int> tiles, bool isRowCheck, BoardState boardState, out List<Vector2Int> contiguousTiles)
    {
        contiguousTiles = new List<Vector2Int>();

        for (int i = 1; i < tiles.Count; i++)
        {
            Vector2Int currentTile = tiles[i];
            Vector2Int previousTile = tiles[i - 1];

            int distance = isRowCheck ? currentTile.y - previousTile.y : currentTile.x - previousTile.x;

            if (distance > 1)
            {
                // Check for gaps: Ensure that any gaps are filled by committed tiles
                for (int j = 1; j < distance; j++)
                {
                    Vector2Int intermediateTile = isRowCheck
                        ? new Vector2Int(previousTile.x, previousTile.y + j)
                        : new Vector2Int(previousTile.x + j, previousTile.y);

                    BoardSlotState intermediateSlot = boardState.GetSlotState(intermediateTile.x, intermediateTile.y);
                    if (!intermediateSlot.IsOccupied || !intermediateSlot.IsTileCommitted)
                    {
                        contiguousTiles.Clear(); // Clear contiguous tiles if a gap can't be filled
                        return false; // If the gap is not filled by a committed tile, it's not valid
                    }
                }
            }

            contiguousTiles.Add(currentTile); // Add to the contiguous list
        }

        return true; // All gaps are filled with committed tiles, so the tiles are contiguous
    }
}