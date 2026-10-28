using UnityEngine;
using UnityEngine.AI;

public class TurretController : MonoBehaviour
{
    public enum TurretType
    {
        Gun,
        Missile
    }

    [Header("Turret Targeting Settings")]
    public GameObject currentTarget;
    public string[] targetTags = { "Player" };
    public string[] allyTags = { "Enemy" };
    public float targetingRange = 20f;
    
    [Header("Turret Settings")]
    public TurretType turretType = TurretType.Gun;
    public float health = 25f;
    public float maxHealth = 25f;
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint;
    public float fireRate = 1f;
    public float damage = 10f;
    public float projectileSpeed = 20f;
    private float nextFireTime;

    [Header("Turret Structure")]
    public GameObject turretBase;
    public GameObject turretBall;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        currentTarget = FindTargetInRange();

        if (currentTarget != null && turretBall != null)
        {
            Quaternion previousRotation = turretBall.transform.rotation;
            Vector3 direction = currentTarget.transform.position - turretBall.transform.position;
            if (direction.sqrMagnitude > 0f)
            {
                // Rotate the barrel's positive X axis toward the target.
                Quaternion aimRotation = Quaternion.FromToRotation(turretBall.transform.right, direction.normalized);
                turretBall.transform.rotation = aimRotation * turretBall.transform.rotation;
            }

            float zAngle = Mathf.DeltaAngle(0f, turretBall.transform.localEulerAngles.z);
            bool isAimBlocked = zAngle >= -135f && zAngle <= -45f;
            if (isAimBlocked)
            {
                turretBall.transform.rotation = previousRotation;
            }
            else if (Time.time >= nextFireTime && !IsAllyInFiringLine())
            {
                ShootProjectile();
                nextFireTime = Time.time + fireRate;
            }
        }
    }

    bool IsAllyInFiringLine()
    {
        if (projectileSpawnPoint == null || currentTarget == null || allyTags == null)
        {
            return false;
        }

        float distanceToTarget = Vector3.Distance(projectileSpawnPoint.position, currentTarget.transform.position);
        RaycastHit[] hits = Physics.RaycastAll(
            projectileSpawnPoint.position,
            projectileSpawnPoint.forward,
            distanceToTarget,
            Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Collide);

        foreach (RaycastHit hit in hits)
        {
            Transform hitTransform = hit.collider.transform;
            if (hitTransform == transform || hitTransform.IsChildOf(transform))
            {
                continue;
            }

            for (Transform ancestor = hitTransform; ancestor != null && ancestor != transform; ancestor = ancestor.parent)
            {
                foreach (string allyTag in allyTags)
                {
                    if (!string.IsNullOrEmpty(allyTag) && ancestor.CompareTag(allyTag))
                    {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    GameObject FindTargetInRange()
    {
        GameObject closestTarget = null;
        float closestDistance = targetingRange;

        foreach (string targetTag in targetTags)
        {
            GameObject[] objectsWithTag = GameObject.FindGameObjectsWithTag(targetTag);
            foreach (GameObject candidate in objectsWithTag)
            {
                float distance = Vector3.Distance(transform.position, candidate.transform.position);
                if (distance <= closestDistance)
                {
                    closestDistance = distance;
                    closestTarget = candidate;
                }
            }
        }

        return closestTarget;
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        if (health <= 0f)
        {
            // Handle turret destruction or game over logic here
            Debug.Log("Turret destroyed!");
            Destroy(gameObject);
        }
    }

    public void ShootProjectile()
    {
        if (projectilePrefab != null && projectileSpawnPoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, projectileSpawnPoint.rotation);
            Rigidbody projectileRigidbody = projectile.GetComponent<Rigidbody>();
            if (projectileRigidbody != null)
            {
                projectileRigidbody.linearVelocity = projectileSpawnPoint.forward * projectileSpeed;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Projectile"))
        {
            // Handle collision with enemy projectile
            Destroy(other.gameObject);
            TakeDamage(other.GetComponent<BulletController>().bulletDamage);
        }
    }
}
