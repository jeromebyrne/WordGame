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

public class PlayerLettersAssigned : GameEvent
{
    public PlayerState PlayerState { get; private set; }

    public static PlayerLettersAssigned Get(PlayerState playerState)
    {
        var evt = Get<PlayerLettersAssigned>();
        evt.PlayerState = playerState;
        return evt;
    }

    public override void Reset()
    {
        PlayerState = null;
    }
}