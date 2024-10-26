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

    public static bool DoWordsShareCommonTile(List<(string word, int score, List<BoardSlotIndex> wordTiles)> wordsAndScores)
    {
        // Use a HashSet to keep track of the tiles in the first word
        var commonTiles = new HashSet<BoardSlotIndex>(wordsAndScores[0].wordTiles);

        // Iterate over the remaining words
        for (int i = 1; i < wordsAndScores.Count; i++)
        {
            var currentWordTiles = wordsAndScores[i].wordTiles;

            // Check if there are any common tiles with the current word's tiles
            bool hasCommonTile = currentWordTiles.Any(tile => commonTiles.Contains(tile));

            // If no common tile is found, the words do not share a common tile
            if (!hasCommonTile)
            {
                return false;
            }

            // Add current word's tiles to commonTiles for further checking
            foreach (var tile in currentWordTiles)
            {
                commonTiles.Add(tile);
            }
        }

        // If we reach here, all words share at least one common tile
        return true;
    }

    public static bool ArePlacedTilesConnectingWithCommittedTile(IReadOnlyBoardState boardState, List<BoardSlotIndex> placedTiles)
    {
        if (placedTiles == null || placedTiles.Count == 0)
            return false;

        foreach (var placedTile in placedTiles)
        {
            var neighbors = GetNeighboringIndices(placedTile, boardState.Dimensions);
            foreach (var neighbor in neighbors)
            {
                if (boardState.GetSlotState(neighbor).IsTileCommitted)
                {
                    return true;
                }
            }
        }
        return false;
    }


    public static List<(string word, int score, List<BoardSlotIndex> wordTiles)> GetWordsAndScoresFromTiles(
           IReadOnlyBoardState boardState, List<BoardSlotIndex> placedTiles)
    {
        var wordsAndScores = new List<(string word, int score, List<BoardSlotIndex>)>();
        var checkedTiles = new HashSet<BoardSlotIndex>();

        bool singleLine = AreTilesInSingleLine(placedTiles);

        foreach (var tile in placedTiles)
        {
            if (!checkedTiles.Contains(tile))
            {
                var mainWord = singleLine ? GetNewWordInDirection(boardState, tile, 1, 0, placedTiles, checkedTiles) :
                                             GetNewWordInDirection(boardState, tile, 0, 1, placedTiles, checkedTiles);

                if (!string.IsNullOrEmpty(mainWord.word) && mainWord.word.Length > 1)
                    wordsAndScores.Add(mainWord);

                // For each placed tile, also check the perpendicular word (cross-word)
                var crossWord = singleLine ? GetNewWordInDirection(boardState, tile, 0, 1, placedTiles, checkedTiles) :
                                             GetNewWordInDirection(boardState, tile, 1, 0, placedTiles, checkedTiles);

                if (!string.IsNullOrEmpty(crossWord.word) && crossWord.word.Length > 1)
                    wordsAndScores.Add(crossWord);
            }
        }

        return wordsAndScores;
    }

    private static (string word, int score, List<BoardSlotIndex> wordTiles) GetNewWordInDirection(
        IReadOnlyBoardState boardState, BoardSlotIndex startTile, int dx, int dy,
        List<BoardSlotIndex> placedTiles, HashSet<BoardSlotIndex> checkedTiles)
    {
        var wordTiles = new List<BoardSlotIndex>();

        wordTiles.AddRange(ExpandWord(boardState, startTile, -dx, -dy));
        wordTiles.Add(startTile);
        wordTiles.AddRange(ExpandWord(boardState, startTile, dx, dy));

        if (dx != 0)
            wordTiles = wordTiles.OrderBy(t => t.Column).ToList();
        else if (dy != 0)
            wordTiles = wordTiles.OrderByDescending(t => t.Row).ToList();

        bool containsNewTile = wordTiles.Any(t => placedTiles.Contains(t) && !boardState.GetSlotState(t).IsTileCommitted);

        if (!containsNewTile)
            return ("", 0, new List<BoardSlotIndex>());

        string word = "";
        int totalScore = 0;
        int wordMultiplier = 1;

        foreach (var index in wordTiles)
        {
            var slotState = boardState.GetSlotState(index);
            var letter = slotState.OccupiedLetter;
            if (letter == null) continue;

            word += letter.Character.ToString();
            int tileScore = letter.Score;

            if (!slotState.IsTileCommitted && placedTiles.Contains(index))
            {
                switch (slotState.BonusType)
                {
                    case TileBonusType.kDoubleLetter: tileScore *= 2; break;
                    case TileBonusType.kTripleLetter: tileScore *= 3; break;
                    case TileBonusType.kDoubleWord: wordMultiplier *= 2; break;
                    case TileBonusType.kTripleWord: wordMultiplier *= 3; break;
                }
            }

            totalScore += tileScore;
            checkedTiles.Add(index);
        }

        totalScore *= wordMultiplier;
        return word.Length > 1 ? (word, totalScore, wordTiles) : ("", 0, new List<BoardSlotIndex>());
    }

    private static List<BoardSlotIndex> ExpandWord(IReadOnlyBoardState boardState, BoardSlotIndex startTile, int dx, int dy)
    {
        var connectedTiles = new List<BoardSlotIndex>();
        var nextTile = new BoardSlotIndex { Column = startTile.Column + dx, Row = startTile.Row + dy };

        while (nextTile.Column >= 0 && nextTile.Column < boardState.Dimensions.x &&
               nextTile.Row >= 0 && nextTile.Row < boardState.Dimensions.y)
        {
            var slotState = boardState.GetSlotState(nextTile);
            if (!slotState.IsOccupied) break;

            connectedTiles.Add(nextTile);
            nextTile = new BoardSlotIndex { Column = nextTile.Column + dx, Row = nextTile.Row + dy };
        }

        return connectedTiles;
    }

    private static List<BoardSlotIndex> GetNeighboringIndices(BoardSlotIndex index, Vector2Int boardDimensions)
    {
        var neighbors = new List<BoardSlotIndex>();

        if (index.Row > 0) neighbors.Add(new BoardSlotIndex { Row = index.Row - 1, Column = index.Column });
        if (index.Row < boardDimensions.y - 1) neighbors.Add(new BoardSlotIndex { Row = index.Row + 1, Column = index.Column });
        if (index.Column > 0) neighbors.Add(new BoardSlotIndex { Row = index.Row, Column = index.Column - 1 });
        if (index.Column < boardDimensions.x - 1) neighbors.Add(new BoardSlotIndex { Row = index.Row, Column = index.Column + 1 });

        return neighbors;
    }

    public static bool AreTilesContiguous(List<BoardSlotIndex> uncommittedTiles, IReadOnlyBoardState boardState, out List<BoardSlotIndex> contiguousTiles)
    {
        contiguousTiles = new List<BoardSlotIndex>();

        if (uncommittedTiles.Count == 0)
            return false;

        bool isFirstTurn = boardState.GetCommittedTileCount() == 0;

        // Ensure that all placed tiles are in a single line (either row or column)
        if (!AreTilesInSingleLine(uncommittedTiles))
        {
            Debug.Log("Tiles must be in a single row or column.");
            return false;
        }

        // Check for connectivity among uncommitted tiles
        var toVisit = new Queue<BoardSlotIndex>();
        var visited = new HashSet<BoardSlotIndex>();

        toVisit.Enqueue(uncommittedTiles[0]);
        visited.Add(uncommittedTiles[0]);

        while (toVisit.Count > 0)
        {
            var currentTile = toVisit.Dequeue();
            contiguousTiles.Add(currentTile);

            var neighbors = GetNeighboringIndices(currentTile, boardState.Dimensions);
            foreach (var neighbor in neighbors)
            {
                var neighborSlot = boardState.GetSlotState(neighbor);
                bool isUncommittedTile = uncommittedTiles.Contains(neighbor);
                bool isCommittedTile = neighborSlot.IsOccupied && neighborSlot.IsTileCommitted;

                if ((isUncommittedTile || (!isFirstTurn && isCommittedTile)) && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    toVisit.Enqueue(neighbor);
                }
            }
        }

        // Check that all uncommitted tiles were visited
        bool allTilesConnected = visited.Intersect(uncommittedTiles).Count() == uncommittedTiles.Count;
        if (!allTilesConnected)
        {
            Debug.Log("All tiles must be connected.");
            return false;
        }

        // Ensure connection with committed tiles for non-first turns
        if (!isFirstTurn && !ArePlacedTilesConnectingWithCommittedTile(boardState, uncommittedTiles))
        {
            Debug.Log("Placed tiles must connect with at least one committed tile.");
            return false;
        }

        return true;
    }

    private static bool AreTilesInSingleLine(List<BoardSlotIndex> placedTiles)
    {
        bool sameRow = placedTiles.All(tile => tile.Row == placedTiles[0].Row);
        bool sameColumn = placedTiles.All(tile => tile.Column == placedTiles[0].Column);
        return sameRow || sameColumn;
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