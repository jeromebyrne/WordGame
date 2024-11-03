using System;
using System.Collections.Generic;

public class GameEventHandler
{
    private static GameEventHandler instance;
    private Dictionary<Type, Action<GameEvent>> eventDictionary;
    private Dictionary<Delegate, Action<GameEvent>> delegateLookup; // Store the original delegate mappings

    public static GameEventHandler Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new GameEventHandler();
                instance.eventDictionary = new Dictionary<Type, Action<GameEvent>>();
                instance.delegateLookup = new Dictionary<Delegate, Action<GameEvent>>();
            }
            return instance;
        }
    }

    public void Subscribe<T>(Action<T> listener) where T : GameEvent
    {
        if (delegateLookup.TryGetValue(listener, out var mappedDelegate))
        {
            // Already subscribed with this listener
            return;
        }

        // Create a new delegate mapping for this listener
        mappedDelegate = (Action<GameEvent>)(e => listener((T)e));
        delegateLookup[listener] = mappedDelegate;

        if (eventDictionary.TryGetValue(typeof(T), out var existingAction))
        {
            eventDictionary[typeof(T)] = existingAction + mappedDelegate;
        }
        else
        {
            eventDictionary[typeof(T)] = mappedDelegate;
        }
    }

    public void Unsubscribe<T>(Action<T> listener) where T : GameEvent
    {
        if (delegateLookup.TryGetValue(listener, out var mappedDelegate))
        {
            if (eventDictionary.TryGetValue(typeof(T), out var existingAction))
            {
                eventDictionary[typeof(T)] = existingAction - mappedDelegate;

                // Clean up if no more listeners are attached
                if (eventDictionary[typeof(T)] == null)
                {
                    eventDictionary.Remove(typeof(T));
                }
            }

            // Remove from delegate lookup
            delegateLookup.Remove(listener);
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