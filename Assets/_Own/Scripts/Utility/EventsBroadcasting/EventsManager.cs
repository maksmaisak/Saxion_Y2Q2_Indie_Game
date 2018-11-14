using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;

/// Automatically subscribes objects to broadcastable events.
public class EventsManager : PersistentSingleton<EventsManager>
{
    private readonly Queue<IBroadcastEvent> events = new Queue<IBroadcastEvent>();
    private readonly IEventReceiverRegisterer[] registerers;

    [Tooltip("All event messages will be delivered this way unless specified otherwise when creating the event.")]
    [SerializeField]
    MessageDeliveryType defaultDeliveryType = MessageDeliveryType.FixedUpdate;

    public EventsManager()
    {
        registerers = MakeRegisterers();
    }

    void FixedUpdate()
    {
        IBroadcastEvent[] eventsArray;

        lock (events)
        {
            eventsArray = events.ToArray();
            events.Clear();
        }

        foreach (IBroadcastEvent engineEvent in eventsArray) engineEvent.DeliverEvent();
    }

    public void Add(object obj)
    {
        foreach (IEventReceiverRegisterer registerer in registerers)
        {
            registerer.Add(obj);
        }
    }

    public void Remove(object obj)
    {
        foreach (IEventReceiverRegisterer registerer in registerers)
        {
            registerer.Remove(obj);
        }
    }

    public void Post(IBroadcastEvent broadcastEvent)
    {
        MessageDeliveryType deliveryType = GetDeliveryType(broadcastEvent);
        switch (deliveryType)
        {
            case MessageDeliveryType.Immediate:
                broadcastEvent.DeliverEvent();
                break;
            case MessageDeliveryType.FixedUpdate:
                lock (events) events.Enqueue(broadcastEvent);
                break;
            case MessageDeliveryType.Unspecified:
            default:
                throw new ArgumentOutOfRangeException($"Invalid delivery type: {deliveryType}");
        }
    }

    private MessageDeliveryType GetDeliveryType(IBroadcastEvent broadcastEvent)
    {
        MessageDeliveryType deliveryType = broadcastEvent.deliveryType;
        if (deliveryType == MessageDeliveryType.Unspecified)
            return defaultDeliveryType;

        return deliveryType;
    }

    #region Dark Arts

    private static IEventReceiverRegisterer[] MakeRegisterers()
    {
        return GetEventTypes()
            .Select(MakeRegistererForEventType)
            .ToArray();
    }

    private static IEnumerable<Type> GetEventTypes()
    {
        Type eventBaseType = typeof(IBroadcastEvent);

        return AppDomain.CurrentDomain
            .GetAssemblies()
            .SelectMany(assembly => assembly.GetTypes())
            .Where(type => !type.IsAbstract && !type.IsInterface && eventBaseType.IsAssignableFrom(type));
    }

    private static IEventReceiverRegisterer MakeRegistererForEventType(Type eventType)
    {
        try
        {
            Type registererType = typeof(EventReceiverRegisterer<>).MakeGenericType(eventType);
            return (IEventReceiverRegisterer)Activator.CreateInstance(registererType);
        }
        catch (ArgumentException)
        {
            Debug.LogError(
                $"Invalid generic parameter for {eventType}. It should inherit from NetworkMessage<{eventType.Name}>");
            return null;
        }
    }

    /// Registers event receivers
    private interface IEventReceiverRegisterer
    {
        bool Add(object obj);
        bool Remove(object obj);
    }

    /// Registers event receivers of type TEvent, 
    /// i.e registers objects which implement IEventReceiver<TEvent>.
    private class EventReceiverRegisterer<TEvent> : IEventReceiverRegisterer
        where TEvent : BroadcastEvent<TEvent>
    {
        public bool Add(object obj)
        {
            var receiver = obj as IEventReceiver<TEvent>;
            if (receiver == null) return false;

            BroadcastEvent<TEvent>.handlers += receiver.On;
            return true;
        }

        public bool Remove(object obj)
        {
            var receiver = obj as IEventReceiver<TEvent>;
            if (receiver == null) return false;

            BroadcastEvent<TEvent>.handlers -= receiver.On;
            return true;
        }
    }

    #endregion
}