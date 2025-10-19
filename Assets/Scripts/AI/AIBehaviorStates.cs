using System;

public enum AIBehaviorState
{
    Idle,
    Patrolling,
    Defending,
    Attacking,
    Retreat,
    Wander,
    Dodge,
    Dead,
    Chase
}

public enum AIState
{
    Patrol,
    Chase,
    Attack,
    Dead
}

[System.Serializable]
public class AIStateData
{
    public AIBehaviorState currentState = AIBehaviorState.Idle;
    public AIBehaviorState previousState = AIBehaviorState.Idle;
    public float stateTimer = 0f;
    public bool canChangeState = true;
    
    public void ChangeState(AIBehaviorState newState)
    {
        if (canChangeState && newState != currentState)
        {
            previousState = currentState;
            currentState = newState;
            stateTimer = 0f;
        }
    }
    
    public bool IsInState(AIBehaviorState state)
    {
        return currentState == state;
    }
    
    public bool WasInState(AIBehaviorState state)
    {
        return previousState == state;
    }
}
