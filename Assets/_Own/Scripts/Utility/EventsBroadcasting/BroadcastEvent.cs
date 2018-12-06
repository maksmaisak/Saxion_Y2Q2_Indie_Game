using System;
using UnityEngine.Assertions;

public interface IBroadcastEvent
{
    void PostEvent();
    void DeliverEvent();
    MessageDeliveryType deliveryType { get; }
}

/// A message that can be broadcast to all objects which subscribe to this type of event.
/// Use PostEvent to send,
/// Use SetDeliveryType to set when the message will be delivered.
public abstract class BroadcastEvent<T> : IBroadcastEvent where T : BroadcastEvent<T>
{
    public delegate void Handler(T eventData);
    public static event Handler handlers;

    private bool wasDeliveryTypeSet;
    public MessageDeliveryType deliveryType { get; private set; }
    
    private bool isPosted;
    private bool isDelivered;

    public void PostEvent()
    {
        Assert.IsFalse(isPosted, $"{this} has already been posted!");
        EventsManager.instance.Post(this);
        isPosted = true;
    }
    
    void IBroadcastEvent.DeliverEvent()
    {
        Assert.IsFalse(isDelivered, $"{this} has already been delivered!");
        handlers?.Invoke((T)this);
        isDelivered = true;
    }
    
    public BroadcastEvent<T> SetDeliveryType(MessageDeliveryType newDeliveryType)
    {
        Assert.IsFalse(isDelivered, "Can't set the delivery type of a broadcast event after it has been delivered.");
        Assert.IsFalse(isPosted, "Can't set the delivery type of a broadcast event after it has been posted.");
        Assert.IsFalse(wasDeliveryTypeSet, "You can only set the delivery type of a broadcast event once.");
        
        deliveryType = newDeliveryType;
        wasDeliveryTypeSet = true;

        return this;
    }
}
