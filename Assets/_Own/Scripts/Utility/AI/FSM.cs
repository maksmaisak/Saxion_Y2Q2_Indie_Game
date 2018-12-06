using System.Collections.Generic;
using System;
using UnityEngine;

public class FSM<AgentT> where AgentT : Component
{
    // Maps the class name of a state to a specific instance of that state
    private Dictionary<Type, FSMState<AgentT>> stateCache;

    // The current state we are in
    private FSMState<AgentT> currentState;

    // Reference to our target so we can pass into our new states.
    private AgentT agent;

    public FSM(AgentT agent)
    {

        this.agent = agent;

        stateCache = new Dictionary<Type, FSMState<AgentT>>();
        DetectExistingStates();
    }
    
    public FSMState<AgentT> GetCurrentState()
    {
        return currentState;
    }

    public void Disable()
    {
        if (!currentState) return;

        currentState.enabled = false;
        currentState = null;
    }

    public void Reset()
    {
        if (currentState != null)
        {
            currentState.Exit();
            currentState.Enter();
        }
    }

    /**
	 * Tells the FSM to enter a state which is a subclass of AbstractState<T>.
	 * So for example for FSM<Bob> the state entered must be a subclass of AbstractState<Bob>
	 */
    public void ChangeState<StateT>() where StateT : FSMState<AgentT>
    {
        // Check if a state like this was already in our cache
        FSMState<AgentT> newState;
        if (!stateCache.TryGetValue(typeof(StateT), out newState))
        {
            // If not, create it, passing in the target
            newState = agent.gameObject.AddComponent<StateT>();
            newState.SetAgent(agent);
            stateCache.Add(typeof(StateT), newState);
        }

        ChangeState(newState);
    }

    private void ChangeState(FSMState<AgentT> newState)
    {
        if (currentState == newState) return;

        if (currentState != null) currentState.Exit();
        currentState = newState;
        if (currentState != null) currentState.Enter();
    }

    private void DetectExistingStates()
    {
        FSMState<AgentT>[] states = agent.GetComponentsInChildren<FSMState<AgentT>>();
        foreach (FSMState<AgentT> state in states)
        {
            state.enabled = false;
            state.SetAgent(agent);
            stateCache.Add(state.GetType(), state);
        }
    }
}
