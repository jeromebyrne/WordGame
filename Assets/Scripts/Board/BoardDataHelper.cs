using System;
using System.Collections.Generic;
using System.Linq;
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

    public static bool ArePlacedTilesConnectingWithCommittedTile(IReadOnlyBoardState boardState, List<BoardSlotIndex> placedTiles)
    {
        // Ensure there are tiles to check
        if (placedTiles == null || placedTiles.Count == 0)
            return false;

        // Check if any placed tile is adjacent to a committed tile
        foreach (var placedTile in placedTiles)
        {
            // Get the neighboring tiles
            var neighbors = GetNeighboringIndices(placedTile, boardState.Dimensions);

            // Check if any neighbor is a committed tile
            foreach (var neighbor in neighbors)
            {
                BoardSlotState neighborSlotState = boardState.GetSlotState(neighbor);

                if (neighborSlotState.IsOccupied && neighborSlotState.IsTileCommitted)
                {
                    return true; // The tile is connected to a committed tile
                }
            }
        }

        // If no placed tile connects to a committed tile, return false
        return false;
    }

    // Helper function to get the indices of the neighboring slots (up, down, left, right)
    private static List<BoardSlotIndex> GetNeighboringIndices(BoardSlotIndex index, Vector2Int boardDimensions)
    {
        List<BoardSlotIndex> neighbors = new List<BoardSlotIndex>();

        // Up
        if (index.Row > 0)
        {
            neighbors.Add(new BoardSlotIndex { Row = index.Row - 1, Column = index.Column });
        }
        // Down
        if (index.Row < boardDimensions.y - 1)
        {
            neighbors.Add(new BoardSlotIndex { Row = index.Row + 1, Column = index.Column });
        }
        // Left
        if (index.Column > 0)
        {
            neighbors.Add(new BoardSlotIndex { Row = index.Row, Column = index.Column - 1 });
        }
        // Right
        if (index.Column < boardDimensions.x - 1)
        {
            neighbors.Add(new BoardSlotIndex { Row = index.Row, Column = index.Column + 1 });
        }

        return neighbors;
    }

    public static (string word, int score) GetWordAndScoreFromTiles(IReadOnlyBoardState boardState, List<BoardSlotIndex> placedTiles)
    {
        // Ensure there are tiles to process
        if (placedTiles == null || placedTiles.Count == 0)
            return ("", 0);

        // Determine if the tiles are aligned horizontally (same y values) or vertically (same x values)
        bool isHorizontal = true;
        bool isVertical = true;

        int firstX = placedTiles[0].Column;
        int firstY = placedTiles[0].Row;

        foreach (var tile in placedTiles)
        {
            if (tile.Row != firstY)
                isHorizontal = false;
            if (tile.Column != firstX)
                isVertical = false;
        }

        // If neither horizontal nor vertical, return empty (invalid state)
        if (!isHorizontal && !isVertical)
        {
            Debug.LogError("Tiles are neither aligned horizontally nor vertically.");
            return ("", 0);
        }

        // List to store all the connected tiles (placed + connected)
        List<BoardSlotIndex> fullWordTiles = new List<BoardSlotIndex>();

        // Get all connected tiles horizontally
        if (isHorizontal)
        {
            // Sort placed tiles left to right
            placedTiles.Sort((a, b) => a.Column.CompareTo(b.Column));

            // Expand left from the first placed tile
            BoardSlotIndex currentTile = placedTiles[0];
            fullWordTiles.AddRange(ExpandWord(boardState, currentTile, -1, 0)); // Left expansion

            // Add the placed tiles
            fullWordTiles.AddRange(placedTiles);

            // Expand right from the last placed tile
            currentTile = placedTiles[placedTiles.Count - 1];
            fullWordTiles.AddRange(ExpandWord(boardState, currentTile, 1, 0)); // Right expansion
        }
        // Get all connected tiles vertically
        else if (isVertical)
        {
            // Sort placed tiles top to bottom (highest row first)
            placedTiles.Sort((a, b) => b.Row.CompareTo(a.Row));

            // Expand upwards from the first placed tile
            BoardSlotIndex currentTile = placedTiles[0];
            fullWordTiles.AddRange(ExpandWord(boardState, currentTile, 0, -1)); // Up expansion

            // Add the placed tiles
            fullWordTiles.AddRange(placedTiles);

            // Expand downwards from the last placed tile
            currentTile = placedTiles[placedTiles.Count - 1];
            fullWordTiles.AddRange(ExpandWord(boardState, currentTile, 0, 1)); // Down expansion
        }

        // Remove duplicate tiles
        fullWordTiles = fullWordTiles.Distinct().ToList();

        // Sort the full list of word tiles based on the direction
        if (isHorizontal)
        {
            fullWordTiles.Sort((a, b) => a.Column.CompareTo(b.Column));
        }
        else if (isVertical)
        {
            fullWordTiles.Sort((a, b) => b.Row.CompareTo(a.Row));
        }

        // Variables to keep track of the word and its total score
        string word = "";
        int totalScore = 0;
        int wordMultiplier = 1; // Multiplier for the word score (e.g., triple word score)

        // Iterate over the sorted tiles to construct the word and calculate the score
        foreach (var index in fullWordTiles)
        {
            var slotState = boardState.GetSlotState(index);
            var letter = slotState.OccupiedLetter;

            if (letter == null)
                continue;

            word += letter.Character.ToString();
            int tileScore = letter.Score; // Base score of the letter

            // Check if this slot has a bonus, and apply the corresponding bonus
            if (!slotState.IsTileCommitted)
            {
                switch (slotState.BonusType)
                {
                    case TileBonusType.kDoubleLetter:
                        tileScore *= 2;
                        break;
                    case TileBonusType.kTripleLetter:
                        tileScore *= 3;
                        break;
                    case TileBonusType.kDoubleWord:
                        wordMultiplier *= 2;
                        break;
                    case TileBonusType.kTripleWord:
                        wordMultiplier *= 3;
                        break;
                }
            }

            totalScore += tileScore; // Add the tile's score to the total
        }

        // Apply the word multiplier (e.g., double or triple word score)
        totalScore *= wordMultiplier;

        return (word, totalScore); // Return both the word and the total score
    }

    // Helper function to expand word in a specific direction (dx, dy)
    private static List<BoardSlotIndex> ExpandWord(IReadOnlyBoardState boardState, BoardSlotIndex startTile, int dx, int dy)
    {
        List<BoardSlotIndex> connectedTiles = new List<BoardSlotIndex>();

        BoardSlotIndex nextTile = new BoardSlotIndex { Column = startTile.Column + dx, Row = startTile.Row + dy };

        // Traverse in the specified direction until we hit an unoccupied tile or go out of bounds
        while (nextTile.Column >= 0 && nextTile.Column < boardState.Dimensions.x &&
               nextTile.Row >= 0 && nextTile.Row < boardState.Dimensions.y)
        {
            BoardSlotState slotState = boardState.GetSlotState(nextTile);

            // If the slot is occupied, add it to the connected tiles
            if (slotState.IsOccupied)
            {
                connectedTiles.Add(nextTile);
            }
            else
            {
                break; // Stop when we hit an unoccupied tile
            }

            // Move to the next tile in the direction
            nextTile = new BoardSlotIndex { Column = nextTile.Column + dx, Row = nextTile.Row + dy };
        }

        return connectedTiles;
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