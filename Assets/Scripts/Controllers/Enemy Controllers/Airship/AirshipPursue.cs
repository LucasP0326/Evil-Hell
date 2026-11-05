using UnityEngine;

public class AirshipPursue : State
{
    public bool active;

    [Header("State References")]
    public AirshipController airshipController;
    public AirshipIdle idleState;
    public AirshipPatrol patrolState;

    [Header("Pursue State Settings")]
    public bool canSeePlayer;

    public override State RunCurrentState()
    {
        return this;
    }
}
