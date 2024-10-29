using System.Collections.Generic;
using UnityEngine;

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

public class ReturnTileToHolderEvent : GameEvent
{
    public int PlayerIndex { get; private set; }
    public uint LetterId { get; private set; }

    public static ReturnTileToHolderEvent Get(int playerIndex, uint LetterId)
    {
        var evt = Get<ReturnTileToHolderEvent>();
        evt.PlayerIndex = playerIndex;
        evt.LetterId = LetterId;
        return evt;
    }

    public override void Reset()
    {
        PlayerIndex = -1;
        LetterId = 0;
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

public class TilesCommittedEvent : GameEvent
{
    public List<BoardSlotIndex> CommittedTileIndices { get; private set; }
    public List<uint> CommittedTileLetterIds { get; private set; }
    public int PlayerIndex { get; private set; }

    public static TilesCommittedEvent Get(int playerIndex, List<BoardSlotIndex> committedTileIndices, List<uint> committedTileLetterIds)
    {
        var evt = Get<TilesCommittedEvent>();
        evt.CommittedTileIndices = committedTileIndices;
        evt.CommittedTileLetterIds = committedTileLetterIds;
        evt.PlayerIndex = playerIndex;
        return evt;
    }

    public override void Reset()
    {
        CommittedTileIndices = null;
        CommittedTileLetterIds = null;
        PlayerIndex = -1;
    }
}

public class ConfirmSwitchPlayerEvent : GameEvent
{
    public int EndPlayerIndex { get; private set; }
    public int NextPlayerIndex { get; private set; }

    public static ConfirmSwitchPlayerEvent Get(int endPlayerIndex, int nextPlayerIndex)
    {
        var evt = Get<ConfirmSwitchPlayerEvent>();
        evt.EndPlayerIndex = endPlayerIndex;
        evt.NextPlayerIndex = nextPlayerIndex;
        return evt;
    }

    public override void Reset()
    {
        EndPlayerIndex = -1;
    }
}

public class PlayerStateUpdatedEvent : GameEvent
{
    public PlayerState PlayerState { get; private set; }

    public static PlayerStateUpdatedEvent Get(PlayerState playerState)
    {
        var evt = Get<PlayerStateUpdatedEvent>();
        evt.PlayerState = playerState;
        return evt;
    }

    public override void Reset()
    {
        PlayerState = null;
    }
}

public class PlayAudioEvent : GameEvent
{
    public string AudioClipPath { get; private set; }
    public float Volume { get; private set; } = 1.0f;
    public bool Loop { get; private set; }
    public bool IsMusic { get; private set; }

    public static PlayAudioEvent Get(string path, float volume, bool loop, bool isMusic)
    {
        var evt = Get<PlayAudioEvent>();
        evt.AudioClipPath = path;
        evt.Volume = Mathf.Clamp(volume, 0f, 1f);
        evt.Loop = loop;
        evt.IsMusic = isMusic;
        return evt;
    }

    public override void Reset()
    {
        AudioClipPath = "";
        Volume = 1.0f;
        Loop = false;
        IsMusic = false;
    }
}

public class StopAudioEvent : GameEvent
{
    public string AudioClipPath { get; private set; }

    public static StopAudioEvent Get(string path)
    {
        var evt = Get<StopAudioEvent>();
        evt.AudioClipPath = path;
        return evt;
    }

    public override void Reset()
    {
        AudioClipPath = "";
    }
}

public class StartTurnCountdownTimer : GameEvent
{
    public float CountdownTime { get; private set; }

    public static StartTurnCountdownTimer Get(float countdownTime)
    {
        var evt = Get<StartTurnCountdownTimer>();
        evt.CountdownTime = countdownTime;
        return evt;
    }

    public override void Reset()
    {
        CountdownTime = 0.0f;
    }
}

public class StopTurnCountdownTimer : GameEvent
{
    public static StopTurnCountdownTimer Get()
    {
        var evt = Get<StopTurnCountdownTimer>();
        return evt;
    }

    public override void Reset()
    {
    }
}

public class ReturnAllUncommittedTilesToHolder : GameEvent
{
    public int PlayerIndex { get; private set; }

    public static ReturnAllUncommittedTilesToHolder Get(int playerIndex)
    {
        var evt = Get<ReturnAllUncommittedTilesToHolder>();
        evt.PlayerIndex = playerIndex;
        return evt;
    }

    public override void Reset()
    {
        PlayerIndex = -1;
    }
}
