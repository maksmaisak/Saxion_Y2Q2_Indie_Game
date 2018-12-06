using System;
using UnityEngine;

public class FSMState<TAgent> : MyBehaviour
{
    protected TAgent agent;

    public void SetAgent(TAgent agent)
    {
        Debug.Assert(this.agent == null);
        this.agent = agent;
    }

    public virtual void Enter()
    {
        Print("entered");
        enabled = true;
    }

    public virtual void Exit()
    {
        Print("exited");
        enabled = false;
    }

    protected void Print(string message)
    {
        Debug.Log($"{this}: {message} ");
    }
}


