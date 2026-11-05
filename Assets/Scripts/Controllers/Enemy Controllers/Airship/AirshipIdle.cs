using UnityEngine;

public class AirshipIdle : State
{
    public bool active;

    [Header("State References")]
    public AirshipController airshipController;
    public AirshipPatrol patrolState;
    public AirshipPursue pursueState;

    [Header("Idle State Settings")]
    public bool canSeePlayer;

    public override State RunCurrentState()
    {
        if (canSeePlayer)
        {
            return pursueState; // Transition to Pursue state if the player is seen
        }
        else
        {
            return this; // Stay in Idle state if the player is not seen
        }
    }
}
