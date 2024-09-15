using System;
using System.Collections.Generic;

public class GameEventHandler
{
    private static GameEventHandler instance;
    private Dictionary<Type, Action<GameEvent>> eventDictionary;

    public static GameEventHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameEventHandler();
                instance.eventDictionary = new Dictionary<Type, Action<GameEvent>>();
            }
            return instance;
        }
    }

    public void Subscribe<T>(Action<T> listener) where T : GameEvent
    {
        if (eventDictionary.TryGetValue(typeof(T), out var existingAction))
        {
            eventDictionary[typeof(T)] = existingAction + (Action<GameEvent>)(e => listener((T)e));
        }
        else
        {
            eventDictionary[typeof(T)] = (Action<GameEvent>)(e => listener((T)e));
        }
    }

    public void Unsubscribe<T>(Action<T> listener) where T : GameEvent
    {
        if (eventDictionary.TryGetValue(typeof(T), out var existingAction))
        {
            eventDictionary[typeof(T)] = existingAction - (Action<GameEvent>)(e => listener((T)e));
            if (eventDictionary[typeof(T)] == null)
            {
                eventDictionary.Remove(typeof(T));
            }
        }
    }

    public void TriggerEvent<T>(T gameEvent) where T : GameEvent
    {
        if (eventDictionary.TryGetValue(typeof(T), out var action))
        {
            action.Invoke(gameEvent);
            gameEvent.Reset(); // Call Reset before returning to the pool
            GameEvent.Return(gameEvent); // Return event to pool after processing
        }
    }
}