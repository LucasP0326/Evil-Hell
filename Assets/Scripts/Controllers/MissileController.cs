using System.Collections.Generic;
using System.Collections;
using UnityEngine;

public class MissileController : MonoBehaviour
{
    public enum MissileType
    {
        Missile,
        Rocket
    }

    [Header("References")]
    public PlaneController planeController;

    [Header("Missile Properties")]
    public GameObject target;
    public bool isDeployed = false;
    public MissileType missileType;
    public bool isHoming = true;
    public float proximityRadius = 5f;
    public float speed = 5f;
    [Tooltip("Time in seconds to turn up to 180 degrees toward the target.")]
    public float turnTime = 1f;
    public int damage = 50;
    public float explosionRadius = 10f;
    public float lifetime = 10f;
    public float deploymentDelay = 0.15f;

    [Header("Movement Settings")]
    public Transform frontPivot; // Reference to the front pivot of the missile for orientation
    public float rotationSpeed = 5f; // Speed at which the missile rotates to face the target
    public float maxTurnAngle = 45f; // Maximum angle the missile can turn per second

    private Rigidbody missileRigidbody;
    private Collider missileCollider;
    private bool isPursuing;
    private bool hasCollided;

    void Awake()
    {
        missileRigidbody = GetComponent<Rigidbody>();
        missileCollider = GetComponent<Collider>();
        planeController = FindObjectOfType<PlaneController>();

        if (missileCollider != null)
        {
            missileCollider.enabled = false;
        }

        if (missileRigidbody != null)
        {
            missileRigidbody.isKinematic = true;
            missileRigidbody.useGravity = false;
        }
    }

    void FixedUpdate()
    {
        if (!isDeployed || !isPursuing || missileRigidbody == null)
        {
            return;
        }

        Vector3 directionToTarget = Vector3.zero;
        bool isWithinProximity = false;
        if (target != null)
        {
            directionToTarget = target.transform.position - transform.position;
            isWithinProximity = directionToTarget.sqrMagnitude <= proximityRadius * proximityRadius;
        }

        if (isWithinProximity)
        {
            Explode();
            return;
        }
        else if (hasCollided)
        {
            Explode();
            return;
        }
        else if (isHoming && target != null)
        {
            Vector3 currentVelocity = missileRigidbody.linearVelocity;
            Vector3 currentDirection = currentVelocity.sqrMagnitude > 0f
                ? currentVelocity.normalized
                : transform.forward;
            Vector3 targetDirection = directionToTarget.normalized;
            float maxTurnRadians = turnTime <= 0f
                ? Mathf.PI
                : Mathf.PI * Time.fixedDeltaTime / turnTime;
            Vector3 direction = Vector3.RotateTowards(
                currentDirection,
                targetDirection,
                maxTurnRadians,
                0f);
            missileRigidbody.linearVelocity = direction * speed;
            if (direction.sqrMagnitude > 0f)
            {
                Vector3 forward = frontPivot != null
                    ? frontPivot.forward
                    : transform.forward;
                Quaternion rotationDelta = Quaternion.FromToRotation(forward, direction);
                missileRigidbody.MoveRotation(rotationDelta * missileRigidbody.rotation);
            }
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        hasCollided = true;
    }

    public void Deploy()
    {
        if (isDeployed)
        {
            return;
        }

        StartCoroutine(TimeDeployed());
    }

    public void PursueTarget()
    {
        isPursuing = true;
        if (missileRigidbody != null)
        {
            missileRigidbody.linearVelocity = transform.forward * speed;
        }
    }

    private void Explode()
    {
        isPursuing = false;
        if (missileCollider != null)
        {
            missileCollider.enabled = false;
        }

        Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
        HashSet<GameObject> damagedEnemies = new HashSet<GameObject>();

        foreach (Collider hit in hits)
        {
            TurretController turret = hit.GetComponentInParent<TurretController>();
            PlaneController plane = hit.GetComponentInParent<PlaneController>();
            AirshipController airship = hit.GetComponentInParent<AirshipController>();
            GameObject enemy = turret != null
                ? turret.gameObject
                : plane != null
                    ? plane.gameObject
                    : airship != null
                        ? airship.gameObject
                        : null;

            if (enemy == null ||
                (!hit.CompareTag("Enemy") && !enemy.CompareTag("Enemy")) ||
                !damagedEnemies.Add(enemy))
            {
                continue;
            }

            if (turret != null)
            {
                turret.TakeDamage(damage);
            }
            else if (airship != null)
            {
                airship.TakeDamage(damage);
            }
            else if (plane != null)
            {
                plane.TakeDamage(damage);
            }
        }

        Destroy(gameObject);
    }

    private IEnumerator TimeDeployed()
    {
        isDeployed = true;
        if (missileCollider != null)
        {
            missileCollider.enabled = true;
        }

        if (missileRigidbody != null)
        {
            missileRigidbody.isKinematic = false;
            missileRigidbody.useGravity = true;
        }

        yield return new WaitForSeconds(deploymentDelay);
        transform.SetParent(null);
        if (missileRigidbody != null)
        {
            missileRigidbody.isKinematic = false;
            missileRigidbody.useGravity = false;
        }

        PursueTarget();
    }
}
