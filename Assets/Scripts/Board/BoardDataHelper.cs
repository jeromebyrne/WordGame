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

    public static void DisplayWordScoresForPlacedTiles(IReadOnlyBoardState boardState, List<WorldLetterTileVisual> placedTileObjects)
    {
        var placedTiles = GetUncommittedTiles(boardState);
        var wordsAndScores = GetWordsAndScoresFromTiles(boardState, placedTiles);

        // Hide all score badges initially
        foreach (var tile in placedTileObjects)
        {
            tile.HideScoreBadge();
        }

        // Display score only on the last tile of each word
        foreach (var (word, score, wordTiles) in wordsAndScores)
        {
            if (wordTiles.Count > 0)
            {
                var lastTileIndex = wordTiles[wordTiles.Count - 1];
                var lastTileObject = placedTileObjects.Find(tile => tile.GridIndex.Equals(lastTileIndex));

                if (lastTileObject != null)
                {
                    lastTileObject.DisplayScoreBadge(score);
                }
            }
        }
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

    public static Tuple<bool, string> AreTilesContiguous(List<BoardSlotIndex> uncommittedTiles,
                                                        IReadOnlyBoardState boardState,
                                                        out List<BoardSlotIndex> contiguousTiles)
    {
        contiguousTiles = new List<BoardSlotIndex>();

        if (uncommittedTiles.Count == 0)
        {
            return new Tuple<bool, string>(false, "No tiles provided.");
        }

        bool isFirstTurn = boardState.GetCommittedTileCount() == 0;

        // Check if this is the first turn where only placed tiles need to be contiguous
        if (isFirstTurn)
        {
            if (AreTilesConnectedWithin(uncommittedTiles, boardState, out contiguousTiles))
                return new Tuple<bool, string>(true, string.Empty);
            else
                return new Tuple<bool, string>(false, "Tiles are not connected for the first turn.");
        }

        // Step 1: Ensure all placed tiles form a single interconnected group (horizontal or vertical)
        if (!AreTilesInSingleLineOrConnected(uncommittedTiles, boardState))
        {
            return new Tuple<bool, string>(false, "Tiles must form a single line or connect through a shared tile.");
        }

        // Step 2: Ensure connection to any existing committed tile
        bool connectsToCommittedTile = uncommittedTiles.Any(tile =>
        {
            var neighbors = GetNeighboringIndices(tile, boardState.Dimensions);
            return neighbors.Any(neighbor => boardState.GetSlotState(neighbor).IsTileCommitted);
        });

        if (!connectsToCommittedTile)
        {
            return new Tuple<bool, string>(false, "Newly placed tiles must connect with at least one previously placed tile.");
        }

        // Step 3: Verify that all uncommitted tiles are connected to form a contiguous group.
        if (AreTilesConnectedWithin(uncommittedTiles, boardState, out contiguousTiles))
        {
            return new Tuple<bool, string>(true, string.Empty);
        }
        else
        {
            return new Tuple<bool, string>(false, "Tiles are not contiguous.");
        }
    }

    // Check if tiles form a single line or are connected through shared letters
    private static bool AreTilesInSingleLineOrConnected(List<BoardSlotIndex> placedTiles, IReadOnlyBoardState boardState)
    {
        bool sameRow = placedTiles.All(tile => tile.Row == placedTiles[0].Row);
        bool sameColumn = placedTiles.All(tile => tile.Column == placedTiles[0].Column);

        // If they are in a single line, return true
        if (sameRow || sameColumn)
            return true;

        // Otherwise, check if all placed tiles are indirectly connected via shared tiles
        var toVisit = new Queue<BoardSlotIndex>();
        var visited = new HashSet<BoardSlotIndex>();

        toVisit.Enqueue(placedTiles[0]);
        visited.Add(placedTiles[0]);

        while (toVisit.Count > 0)
        {
            var currentTile = toVisit.Dequeue();
            var neighbors = GetNeighboringIndices(currentTile, boardState.Dimensions);

            foreach (var neighbor in neighbors)
            {
                if (placedTiles.Contains(neighbor) && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    toVisit.Enqueue(neighbor);
                }
            }
        }

        // All placed tiles must be visited if they are connected
        return visited.Count == placedTiles.Count;
    }

    private static bool AreTilesConnectedWithin(List<BoardSlotIndex> tiles, IReadOnlyBoardState boardState, out List<BoardSlotIndex> contiguousTiles)
    {
        contiguousTiles = new List<BoardSlotIndex>();
        if (tiles.Count == 0) return false;
        if (tiles.Count == 1) // Only one tile, trivially contiguous
        {
            contiguousTiles.Add(tiles[0]);
            return true;
        }

        var toVisit = new Queue<BoardSlotIndex>();
        var visited = new HashSet<BoardSlotIndex>();

        // Start flood fill from the first tile in the list
        toVisit.Enqueue(tiles[0]);
        visited.Add(tiles[0]);

        while (toVisit.Count > 0)
        {
            var currentTile = toVisit.Dequeue();
            contiguousTiles.Add(currentTile);

            // Get horizontal and vertical neighbors
            var neighbors = GetNeighboringIndices(currentTile, boardState.Dimensions);
            foreach (var neighbor in neighbors)
            {
                // Only add unvisited tiles that are part of the placed tiles list or committed tiles that form intersections
                var neighborState = boardState.GetSlotState(neighbor);
                bool isPartOfPlacedOrCommittedWord = tiles.Contains(neighbor) || (neighborState.IsOccupied && neighborState.IsTileCommitted);

                if (isPartOfPlacedOrCommittedWord && !visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    toVisit.Enqueue(neighbor);
                }
            }
        }

        // Ensure contiguousTiles is populated with all visited tiles
        contiguousTiles = visited.ToList();

        // Check if every tile in the input list was visited (indicating full contiguity)
        return visited.Intersect(tiles).Count() == tiles.Count;
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