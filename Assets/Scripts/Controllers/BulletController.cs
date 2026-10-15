using UnityEngine;
using UnityEngine.InputSystem;
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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        planeController = FindObjectOfType<PlaneController>();
        InitializeBullet(planeController.projectileSpeed, planeController.projectileLifetime, planeController.projectileDamage);
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeBullet(float speed, float lifetime, float damage)
    {
        bulletSpeed = speed;
        bulletLifetime = lifetime;
        bulletDamage = damage;
        StartCoroutine(DestroyAfterLifetime());
    }

    IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(bulletLifetime);
        Destroy(gameObject);
    }
}
