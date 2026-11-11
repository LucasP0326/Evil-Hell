using UnityEngine;

public class AirshipPatrol : State
{
    public bool active;

    [Header("State References")]
    public AirshipController airshipController;
    public AirshipIdle idleState;
    public AirshipPursue pursueState;

    [Header("Patrol State Settings")]
    public bool canSeePlayer;
    public Transform[] patrolPoints;
    public Transform currentPatrolPoint;

    private int currentPatrolPointIndex;

    public override State RunCurrentState()
    {
        BeginPatrol();
        return this;
    }

    public void BeginPatrol()
    {
        if (airshipController == null)
        {
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            currentPatrolPoint = null;
            currentPatrolPointIndex = 0;
            airshipController.MoveToPoint(null);
            return;
        }

        if (currentPatrolPoint != null &&
            (airshipController.transform.position - currentPatrolPoint.position).sqrMagnitude <= airshipController.targetMarginofError * airshipController.targetMarginofError)
        {
            currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
            currentPatrolPoint = null;
        }

        if (currentPatrolPoint == null)
        {
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                int pointIndex = (currentPatrolPointIndex + i) % patrolPoints.Length;
                if (patrolPoints[pointIndex] != null)
                {
                    currentPatrolPointIndex = pointIndex;
                    currentPatrolPoint = patrolPoints[pointIndex];
                    break;
                }
            }
        }

        airshipController.MoveToPoint(currentPatrolPoint);
    }
}
