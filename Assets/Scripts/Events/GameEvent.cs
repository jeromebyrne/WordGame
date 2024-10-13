using System.Collections.Generic;

public abstract class GameEvent
{
    private static readonly Dictionary<System.Type, Queue<GameEvent>> eventPools = new Dictionary<System.Type, Queue<GameEvent>>();

    public static T Get<T>() where T : GameEvent, new()
    {
        var type = typeof(T);
        if (!eventPools.TryGetValue(type, out var pool))
        {
            pool = new Queue<GameEvent>();
            eventPools[type] = pool;
        }

        return pool.Count > 0 ? (T)pool.Dequeue() : new T();
    }

    public static void Return(GameEvent gameEvent)
    {
        var type = gameEvent.GetType();
        if (!eventPools.TryGetValue(type, out var pool))
        {
            pool = new Queue<GameEvent>();
            eventPools[type] = pool;
        }

        pool.Enqueue(gameEvent);
    }

    public virtual void Reset() { }
}

public class UILetterTileStartDragEvent : GameEvent
{
    public UILetterTile LetterTile { get; private set; }

    public static UILetterTileStartDragEvent Get(UILetterTile letterTile)
    {
        var evt = Get<UILetterTileStartDragEvent>();
        evt.LetterTile = letterTile;
        return evt;
    }

    public override void Reset()
    {
        LetterTile = null;
    }
}

public class UILetterTileEndDragEvent : GameEvent
{
    public UILetterTile LetterTile { get; private set; }

    public static UILetterTileEndDragEvent Get(UILetterTile letterTile)
    {
        var evt = Get<UILetterTileEndDragEvent>();
        evt.LetterTile = letterTile;
        return evt;
    }

    public override void Reset()
    {
        LetterTile = null;
    }
}

public class PlayerLetterAssignedEvent : GameEvent
{
    public PlayerState PlayerState { get; private set; }
    public LetterDataObj LetterInfo { get; private set; }

    public static PlayerLetterAssignedEvent Get(PlayerState playerState, LetterDataObj letterInfo)
    {
        var evt = Get<PlayerLetterAssignedEvent>();
        evt.PlayerState = playerState;
        evt.LetterInfo = letterInfo;
        return evt;
    }

    public override void Reset()
    {
        PlayerState = null;
    }
}

public class SendTileToHolderEvent : GameEvent
{
    public int PlayerIndex { get; private set; }
    public UILetterTile Tile { get; private set; }

    public static SendTileToHolderEvent Get(int playerIndex, UILetterTile tile)
    {
        var evt = Get<SendTileToHolderEvent>();
        evt.PlayerIndex = playerIndex;
        evt.Tile = tile;
        return evt;
    }

    public override void Reset()
    {
        PlayerIndex = -1;
        Tile = null;
    }
}

public class UITilePlacedonBoardEvent : GameEvent
{
    public int PlayerIndex { get; private set; }
    public UILetterTile Tile { get; private set; }

    public static UITilePlacedonBoardEvent Get(int playerIndex, UILetterTile tile)
    {
        var evt = Get<UITilePlacedonBoardEvent>();
        evt.PlayerIndex = playerIndex;
        evt.Tile = tile;
        return evt;
    }

    public override void Reset()
    {
        PlayerIndex = -1;
        Tile = null;
    }
}

public class PlayerTurnFinishedEvent : GameEvent
{
    public int PlayerIndex { get; private set; }

    public static PlayerTurnFinishedEvent Get(int playerIndex)
    {
        var evt = Get<PlayerTurnFinishedEvent>();
        evt.PlayerIndex = playerIndex;
        return evt;
    }

    public override void Reset()
    {
        PlayerIndex = -1;
    }
}

public class UIPlayButtonPressedEvent : GameEvent
{
    public static UIPlayButtonPressedEvent Get()
    {
        var evt = Get<UIPlayButtonPressedEvent>();
        return evt;
    }

    public override void Reset()
    {
    }
}

public class WorldTileStartDragEvent : GameEvent
{
    public WorldLetterTileVisual LetterTile { get; private set; }

    public static WorldTileStartDragEvent Get(WorldLetterTileVisual letterTile)
    {
        var evt = Get<WorldTileStartDragEvent>();
        evt.LetterTile = letterTile;
        return evt;
    }

    public override void Reset()
    {
        LetterTile = null;
    }
}

public class WorldTileEndDragEvent : GameEvent
{
    public WorldLetterTileVisual LetterTile { get; private set; }

    public static WorldTileEndDragEvent Get(WorldLetterTileVisual letterTile)
    {
        var evt = Get<WorldTileEndDragEvent>();
        evt.LetterTile = letterTile;
        return evt;
    }

    public override void Reset()
    {
        LetterTile = null;
    }
}