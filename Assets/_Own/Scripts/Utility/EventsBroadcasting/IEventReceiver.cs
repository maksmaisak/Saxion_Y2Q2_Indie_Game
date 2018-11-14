using System;

public interface IEventReceiver<in T> where T : IBroadcastEvent
{
    void On(T message);
}