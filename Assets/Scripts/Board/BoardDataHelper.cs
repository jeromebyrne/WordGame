using System;
using System.Collections.Generic;
using UnityEngine;

public static class BoardDataHelper
{
    public static List<Vector2Int> GetUncommittedTiles(IReadOnlyBoardState boardState)
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

    public static string GetWordFromTiles(IReadOnlyBoardState boardState, List<Vector2Int> contiguousTiles)
    {
        // Ensure there are tiles to process
        if (contiguousTiles == null || contiguousTiles.Count == 0)
            return "";

        // Determine if the tiles are aligned horizontally (same y values) or vertically (same x values)
        bool isHorizontal = true;
        bool isVertical = true;

        int firstX = contiguousTiles[0].x;
        int firstY = contiguousTiles[0].y;

        foreach (var tile in contiguousTiles)
        {
            if (tile.y != firstY)
                isHorizontal = false;
            if (tile.x != firstX)
                isVertical = false;
        }

        // If neither horizontal nor vertical, return empty (invalid state)
        if (!isHorizontal && !isVertical)
        {
            Debug.LogError("Tiles are neither aligned horizontally nor vertically.");
            return "";
        }

        // Sort the tiles based on the orientation
        if (isHorizontal)
        {
            // Sort left to right (by x-coordinate)
            contiguousTiles.Sort((a, b) => a.x.CompareTo(b.x));
        }
        else if (isVertical)
        {
            // Sort top to bottom (by y-coordinate) - highest y first, so reverse the order
            contiguousTiles.Sort((a, b) => b.y.CompareTo(a.y));
        }

        // Construct the word by iterating over the sorted tiles
        string word = "";
        foreach (var index in contiguousTiles)
        {
            var s = boardState.GetSlotState(index.x, index.y);
            word += s.OccupiedLetter.Character.ToString();
        }

        return word;
    }

    public static int GetScoreFromTiles(IReadOnlyBoardState boardState, List<Vector2Int> contiguousTiles)
    {
        // TODO: future update: check tile multipliers
        int score = 0;

        foreach (var index in contiguousTiles)
        {
            var s = boardState.GetSlotState(index.x, index.y);

            score += s.OccupiedLetter.Score;
        }

        return score;
    }

    public static bool AreTilesContiguous(List<Vector2Int> uncommittedTiles, IReadOnlyBoardState boardState, out List<Vector2Int> contiguousTiles)
    {
        contiguousTiles = new List<Vector2Int>();

        if (uncommittedTiles.Count < 1)
        {
            return false;
        }

        if (uncommittedTiles.Count < 2)
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

    private static bool CheckContiguityWithGaps(List<Vector2Int> tiles, bool isRowCheck, IReadOnlyBoardState boardState, out List<Vector2Int> contiguousTiles)
    {
        contiguousTiles = new List<Vector2Int>();

        // Add the first tile to the contiguous list
        if (tiles.Count > 0)
        {
            contiguousTiles.Add(tiles[0]);
        }

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

    public static Tuple<bool, Vector2Int> FindNextNearestUnoccupiedSlot(Vector2Int currentSlot, BoardState boardState, Vector2 worldPos, BoardVisual boardVisual)
    {
        BoardSlotState currentSlotState = boardState.GetSlotState(currentSlot.x, currentSlot.y);

        if (!currentSlotState.IsOccupied)
        {
            return new Tuple<bool, Vector2Int>(true, currentSlot);
        }

        // Variables to track the nearest slot
        Vector2Int nearestSlot = new Vector2Int(-1, -1);
        float nearestDistance = float.MaxValue;

        // current slot is occupied, so search neighbors
        int totalRows = boardState.Dimensions.x;
        int totalColumns = boardState.Dimensions.y;

        for (int searchRadius = 1; searchRadius < Mathf.Max(totalRows, totalColumns); searchRadius++)
        {
            for (int offsetX = -searchRadius; offsetX <= searchRadius; offsetX++)
            {
                for (int offsetY = -searchRadius; offsetY <= searchRadius; offsetY++)
                {
                    // Skip the current slot being checked
                    if (offsetX == 0 && offsetY == 0) continue;

                    int candidateX = currentSlot.x + offsetX;
                    int candidateY = currentSlot.y + offsetY;

                    // Check if the candidate slot is within the board boundaries
                    if (candidateX >= 0 && candidateX < totalRows && candidateY >= 0 && candidateY < totalColumns)
                    {
                        BoardSlotState candidateSlotState = boardState.GetSlotState(candidateX, candidateY);

                        // If the slot is unoccupied, calculate the distance to worldPos
                        if (!candidateSlotState.IsOccupied)
                        {
                            // Get the world position of the candidate slot
                            Vector2 candidateWorldPos = boardVisual.GetWorldPositionForGridIndex(new Vector2Int(candidateX, candidateY));

                            // Calculate the distance to the provided worldPos
                            float distance = Vector2.Distance(worldPos, candidateWorldPos);

                            // Check if this is the closest unoccupied slot found
                            if (distance < nearestDistance)
                            {
                                nearestDistance = distance;
                                nearestSlot = new Vector2Int(candidateX, candidateY);
                            }
                        }
                    }
                }
            }
        }

        // If a nearest slot was found, return it
        if (nearestSlot != new Vector2Int(-1, -1))
        {
            return new Tuple<bool, Vector2Int>(true, nearestSlot);
        }

        // If no unoccupied slots were found, return false
        return new Tuple<bool, Vector2Int>(false, currentSlot);
    }
}