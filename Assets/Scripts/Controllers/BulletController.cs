using UnityEngine;
using System.Collections;

public class BulletController : MonoBehaviour
{
    public enum BulletType
    {
        Default,
        ArmorPiercing,
        Explosive
    }

    [Header("References")]
    public PlaneController planeController; // Reference to the PlaneController script

    [Header("Bullet Properties")]
    public float bulletDamage = 10f;
    public float bulletSpeed = 10f;
    public float bulletLifetime = 5f;

    private Rigidbody bulletRigidbody;
    private Collider bulletCollider;
    private bool hitResolved;

    void Awake()
    {
        bulletRigidbody = GetComponent<Rigidbody>();
        bulletCollider = GetComponent<Collider>();
        planeController = FindObjectOfType<PlaneController>();

        if (planeController != null)
        {
            InitializeBullet(planeController.projectileSpeed, planeController.projectileLifetime, planeController.projectileDamage);
        }
        else
        {
            StartCoroutine(DestroyAfterLifetime());
        }
    }

    public void InitializeBullet(float speed, float lifetime, float damage)
    {
        bulletSpeed = speed;
        bulletLifetime = lifetime;
        bulletDamage = damage;
        StartCoroutine(DestroyAfterLifetime());
    }

    void FixedUpdate()
    {
        if (hitResolved || bulletRigidbody == null)
        {
            return;
        }

        Vector3 velocity = bulletRigidbody.linearVelocity;
        float distance = velocity.magnitude * Time.fixedDeltaTime;
        if (distance <= 0f)
        {
            return;
        }

        RaycastHit[] hits = bulletRigidbody.SweepTestAll(
            velocity.normalized,
            distance,
            QueryTriggerInteraction.Ignore);

        TurretController hitTurret = null;
        PlaneController hitPlane = null;
        float closestDistance = float.MaxValue;

        foreach (RaycastHit hit in hits)
        {
            TurretController turret = hit.collider.GetComponentInParent<TurretController>();
            PlaneController plane = hit.collider.GetComponentInParent<PlaneController>();
            if ((turret != null || plane != null) && hit.distance < closestDistance)
            {
                hitTurret = turret;
                hitPlane = plane;
                closestDistance = hit.distance;
            }
        }

        if (hitTurret == null && hitPlane == null)
        {
            return;
        }

        hitResolved = true;
        if (bulletCollider != null)
        {
            bulletCollider.enabled = false;
        }

        if (hitTurret != null)
        {
            hitTurret.TakeDamage(bulletDamage);
        }
        else if (hitPlane != null)
        {
            hitPlane.TakeDamage(bulletDamage);
        }

        Destroy(gameObject);
    }

    IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(bulletLifetime);
        Destroy(gameObject);
    }
}
