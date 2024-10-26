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

    public static List<(string word, int score)> GetWordsAndScoresFromTiles(IReadOnlyBoardState boardState, List<BoardSlotIndex> placedTiles)
    {
        // Ensure there are tiles to process
        if (placedTiles == null || placedTiles.Count == 0)
            return new List<(string, int)>();

        var wordsAndScores = new List<(string, int)>();
        var checkedTiles = new HashSet<BoardSlotIndex>();

        foreach (var tile in placedTiles)
        {
            if (!checkedTiles.Contains(tile))
            {
                // Check horizontally
                var horizontalWord = GetNewWordInDirection(boardState, tile, 1, 0, placedTiles, checkedTiles);
                if (!string.IsNullOrEmpty(horizontalWord.word) && horizontalWord.word.Length > 1) // Ensure word has more than one letter
                    wordsAndScores.Add(horizontalWord);

                // Check vertically
                var verticalWord = GetNewWordInDirection(boardState, tile, 0, 1, placedTiles, checkedTiles);
                if (!string.IsNullOrEmpty(verticalWord.word) && verticalWord.word.Length > 1) // Ensure word has more than one letter
                    wordsAndScores.Add(verticalWord);
            }
        }

        return wordsAndScores;
    }

    private static (string word, int score) GetNewWordInDirection(
    IReadOnlyBoardState boardState, BoardSlotIndex startTile, int dx, int dy,
    List<BoardSlotIndex> placedTiles, HashSet<BoardSlotIndex> checkedTiles)
    {
        var wordTiles = new List<BoardSlotIndex>();

        // Expand in the negative direction
        wordTiles.AddRange(ExpandWord(boardState, startTile, -dx, -dy));

        // Add the start tile itself
        wordTiles.Add(startTile);

        // Expand in the positive direction
        wordTiles.AddRange(ExpandWord(boardState, startTile, dx, dy));

        // Sort the tiles to maintain correct word order
        if (dx != 0) // Horizontal sort by column, left to right
            wordTiles = wordTiles.OrderBy(t => t.Column).ToList();
        else if (dy != 0) // Vertical sort by row, descending order for top-to-bottom orientation
            wordTiles = wordTiles.OrderByDescending(t => t.Row).ToList();

        // Check if the word includes at least one of the newly placed, uncommitted tiles
        bool containsNewTile = wordTiles.Any(t => placedTiles.Contains(t) && !boardState.GetSlotState(t).IsTileCommitted);

        // Skip if the word only contains committed tiles
        if (!containsNewTile)
            return ("", 0);

        // Construct the word and calculate the score
        string word = "";
        int totalScore = 0;
        int wordMultiplier = 1;

        foreach (var index in wordTiles)
        {
            var slotState = boardState.GetSlotState(index);
            var letter = slotState.OccupiedLetter;

            if (letter == null)
                continue;

            word += letter.Character.ToString();
            int tileScore = letter.Score;

            if (!slotState.IsTileCommitted && placedTiles.Contains(index))
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

            totalScore += tileScore;
            checkedTiles.Add(index); // Mark this tile as processed
        }

        // Apply the word multiplier (e.g., double or triple word score)
        totalScore *= wordMultiplier;

        // Return the new word and its score if it has more than one letter
        return word.Length > 1 ? (word, totalScore) : ("", 0);
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

        if (uncommittedTiles.Count == 0)
            return false;

        // Use a queue for flood-fill and a hash set to track visited tiles
        var toVisit = new Queue<BoardSlotIndex>();
        var visited = new HashSet<BoardSlotIndex>();

        // Start with the first uncommitted tile
        toVisit.Enqueue(uncommittedTiles[0]);
        visited.Add(uncommittedTiles[0]);

        while (toVisit.Count > 0)
        {
            var currentTile = toVisit.Dequeue();
            contiguousTiles.Add(currentTile);

            // Get all neighboring tiles
            var neighbors = GetNeighboringIndices(currentTile, boardState.Dimensions);

            foreach (var neighbor in neighbors)
            {
                // Check if neighbor is either uncommitted or a committed, occupied tile
                var neighborSlot = boardState.GetSlotState(neighbor);
                bool isUncommittedTile = uncommittedTiles.Contains(neighbor);
                bool isCommittedTile = neighborSlot.IsOccupied && neighborSlot.IsTileCommitted;

                if ((isUncommittedTile || isCommittedTile) && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    toVisit.Enqueue(neighbor);
                }
            }
        }

        // All uncommitted tiles must be visited for them to be contiguous
        return visited.Intersect(uncommittedTiles).Count() == uncommittedTiles.Count;
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

            int distance = isRowCheck ? currentTile.Column - previousTile.Column : currentTile.Row - previousTile.Row;

            if (distance > 1)
            {
                // Check for gaps: Ensure that any gaps are filled by committed tiles
                for (int j = 1; j < distance; j++)
                {
                    BoardSlotIndex intermediateTile;

                    if (isRowCheck)
                    {
                        intermediateTile.Row = previousTile.Row;
                        intermediateTile.Column = previousTile.Column + j;
                    }
                    else
                    {
                        intermediateTile.Row = previousTile.Row + j;
                        intermediateTile.Column = previousTile.Column;
                    }

                    BoardSlotState intermediateSlot = boardState.GetSlotState(intermediateTile);

                    // Check if the intermediate slot is occupied and the tile is committed
                    if (!intermediateSlot.IsOccupied || !intermediateSlot.IsTileCommitted)
                    {
                        contiguousTiles.Clear(); // Clear contiguous tiles if a gap can't be filled
                        return false; // If the gap is not filled by a committed tile, it's not valid
                    }

                    // Add the intermediate committed tile to the contiguous list
                    contiguousTiles.Add(intermediateTile);
                }
            }

            contiguousTiles.Add(currentTile); // Add the current tile to the contiguous list
        }

        if (contiguousTiles.Count < 2)
        {
            return false;
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