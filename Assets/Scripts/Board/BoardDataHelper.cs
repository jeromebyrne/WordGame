using System;
using System.Collections.Generic;
using UnityEngine;

public static class BoardDataHelper
{
    public static List<BoardSlotIndex> GetUncommittedTiles(IReadOnlyBoardState boardState)
    {
        List<BoardSlotIndex> uncommittedTiles = new List<BoardSlotIndex>();

        for (int row = 0; row < boardState.Dimensions.y; row++)
        {
            for (int column = 0; column < boardState.Dimensions.x; column++)
            {
                BoardSlotIndex index;
                index.Row = row;
                index.Column = column;
                BoardSlotState slot = boardState.GetSlotState(index);
                if (slot.IsOccupied && !slot.IsTileCommitted)
                {
                    uncommittedTiles.Add(index);
                }
            }
        }

        return uncommittedTiles;
    }

    public static string GetWordFromTiles(IReadOnlyBoardState boardState, List<BoardSlotIndex> contiguousTiles)
    {
        // Ensure there are tiles to process
        if (contiguousTiles == null || contiguousTiles.Count == 0)
            return "";

        // Determine if the tiles are aligned horizontally (same y values) or vertically (same x values)
        bool isHorizontal = true;
        bool isVertical = true;

        int firstX = contiguousTiles[0].Column;
        int firstY = contiguousTiles[0].Row;

        foreach (var tile in contiguousTiles)
        {
            if (tile.Row != firstY)
                isHorizontal = false;
            if (tile.Column != firstX)
                isVertical = false;
        }

        // If neither horizontal nor vertical, return empty (invalid state)
        if (!isHorizontal && !isVertical)
        {
            // This shouldn't happen as we should only hit this function
            // if the tiles are in a contiguous line horizontally or vertically
            Debug.LogError("Tiles are neither aligned horizontally nor vertically.");
            return "";
        }

        // Sort the tiles based on the orientation
        if (isHorizontal)
        {
            // Sort left to right (by x-coordinate)
            contiguousTiles.Sort((a, b) => a.Column.CompareTo(b.Column));
        }
        else if (isVertical)
        {
            // Sort top to bottom (by y-coordinate) - highest y first, so reverse the order
            contiguousTiles.Sort((a, b) => b.Row.CompareTo(a.Row));
        }

        // Construct the word by iterating over the sorted tiles
        string word = "";
        foreach (var index in contiguousTiles)
        {
            var s = boardState.GetSlotState(index);
            word += s.OccupiedLetter.Character.ToString();
        }

        return word;
    }

    public static int GetScoreFromTiles(IReadOnlyBoardState boardState, List<BoardSlotIndex> contiguousTiles)
    {
        // TODO: future update: check tile multipliers
        int score = 0;

        foreach (var index in contiguousTiles)
        {
            var s = boardState.GetSlotState(index);

            score += s.OccupiedLetter.Score;
        }

        return score;
    }

    public static bool AreTilesContiguous(List<BoardSlotIndex> uncommittedTiles, IReadOnlyBoardState boardState, out List<BoardSlotIndex> contiguousTiles)
    {
        contiguousTiles = new List<BoardSlotIndex>();

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
            if (uncommittedTiles[i].Column != uncommittedTiles[0].Column)
            {
                allInSameRow = false;
            }
            if (uncommittedTiles[i].Row != uncommittedTiles[0].Row)
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

    private static void SortByColumn(List<BoardSlotIndex> tiles)
    {
        for (int i = 0; i < tiles.Count - 1; i++)
        {
            for (int j = i + 1; j < tiles.Count; j++)
            {
                if (tiles[i].Row > tiles[j].Row)
                {
                    BoardSlotIndex temp = tiles[i];
                    tiles[i] = tiles[j];
                    tiles[j] = temp;
                }
            }
        }
    }

    private static void SortByRow(List<BoardSlotIndex> tiles)
    {
        for (int i = 0; i < tiles.Count - 1; i++)
        {
            for (int j = i + 1; j < tiles.Count; j++)
            {
                if (tiles[i].Column > tiles[j].Column)
                {
                    BoardSlotIndex temp = tiles[i];
                    tiles[i] = tiles[j];
                    tiles[j] = temp;
                }
            }
        }
    }

    private static bool CheckContiguityWithGaps(List<BoardSlotIndex> tiles, bool isRowCheck, IReadOnlyBoardState boardState, out List<BoardSlotIndex> contiguousTiles)
    {
        contiguousTiles = new List<BoardSlotIndex>();

        // Add the first tile to the contiguous list
        if (tiles.Count > 0)
        {
            contiguousTiles.Add(tiles[0]);
        }

        for (int i = 1; i < tiles.Count; i++)
        {
            BoardSlotIndex currentTile = tiles[i];
            BoardSlotIndex previousTile = tiles[i - 1];

            int distance = isRowCheck ? currentTile.Row - previousTile.Row : currentTile.Column - previousTile.Column;

            if (distance > 1)
            {
                // Check for gaps: Ensure that any gaps are filled by committed tiles
                for (int j = 1; j < distance; j++)
                {
                    BoardSlotIndex intermediateTile;

                    if (isRowCheck)
                    {
                        intermediateTile.Row = previousTile.Row + j;
                        intermediateTile.Column = previousTile.Column;
                    }
                    else
                    {
                        intermediateTile.Row = previousTile.Row;
                        intermediateTile.Column = previousTile.Column + j;
                    }

                    BoardSlotState intermediateSlot = boardState.GetSlotState(intermediateTile);
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

    public static Tuple<bool, BoardSlotIndex> FindNextNearestUnoccupiedSlot(BoardSlotIndex currentSlot, IReadOnlyBoardState boardState, Vector2 worldPos, BoardVisual boardVisual)
    {
        BoardSlotState currentSlotState = boardState.GetSlotState(currentSlot);

        if (!currentSlotState.IsOccupied)
        {
            return new Tuple<bool, BoardSlotIndex>(true, currentSlot);
        }

        // Variables to track the nearest slot
        BoardSlotIndex nearestSlot;
        nearestSlot.Row = -1;
        nearestSlot.Column = -1;

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

                    BoardSlotIndex candidate;
                    candidate.Column = currentSlot.Column + offsetX;
                    candidate.Row = currentSlot.Row + offsetY;

                    // Check if the candidate slot is within the board boundaries
                    if (candidate.Column >= 0 && candidate.Column < totalRows && candidate.Row >= 0 && candidate.Row < totalColumns)
                    {
                        BoardSlotState candidateSlotState = boardState.GetSlotState(candidate);

                        // If the slot is unoccupied, calculate the distance to worldPos
                        if (!candidateSlotState.IsOccupied)
                        {
                            // Get the world position of the candidate slot
                            Vector2 candidateWorldPos = boardVisual.GetWorldPositionForGridIndex(candidate);

                            // Calculate the distance to the provided worldPos
                            float distance = Vector2.Distance(worldPos, candidateWorldPos);

                            // Check if this is the closest unoccupied slot found
                            if (distance < nearestDistance)
                            {
                                nearestDistance = distance;
                                nearestSlot = candidate;
                            }
                        }
                    }
                }
            }
        }

        // If a nearest slot was found, return it
        if (nearestSlot.Row != -1 && nearestSlot.Column != -1)
        {
            return new Tuple<bool, BoardSlotIndex>(true, nearestSlot);
        }

        // If no unoccupied slots were found, return false
        return new Tuple<bool, BoardSlotIndex>(false, currentSlot);
    }
}