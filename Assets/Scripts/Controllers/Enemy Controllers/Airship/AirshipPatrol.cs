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

    private int currentPatrolPointIndex;

    public override State RunCurrentState()
    {
        BeginPatrol();
        return this;
    }

    public void BeginPatrol()
    {
        if (airshipController == null || patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        Transform targetPoint = null;
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            int pointIndex = (currentPatrolPointIndex + i) % patrolPoints.Length;
            if (patrolPoints[pointIndex] != null)
            {
                currentPatrolPointIndex = pointIndex;
                targetPoint = patrolPoints[pointIndex];
                break;
            }
        }

        if (targetPoint == null)
        {
            return;
        }

        Transform airship = airshipController.transform;
        Vector3 direction = targetPoint.position - airship.position;
        if (airshipController.frontPivot != null && direction.sqrMagnitude > 0.0001f)
        {
            airship.rotation = Quaternion.FromToRotation(
                airshipController.frontPivot.forward,
                direction.normalized) * airship.rotation;
        }

        airship.position = Vector3.MoveTowards(
            airship.position,
            targetPoint.position,
            Mathf.Max(0f, airshipController.speed) * Time.deltaTime);

        if (Vector3.SqrMagnitude(airship.position - targetPoint.position) <= 0.0001f)
        {
            currentPatrolPointIndex = (currentPatrolPointIndex + 1) % patrolPoints.Length;
        }
    }
}
