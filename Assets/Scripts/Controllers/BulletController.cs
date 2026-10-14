using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class BulletController : MonoBehaviour
{
    public PlaneController planeController; // Reference to the PlaneController script
    public float bulletSpeed = 10f;
    public float bulletLifetime = 5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        planeController = FindObjectOfType<PlaneController>();
        InitializeBullet(planeController.projectileSpeed, planeController.projectileLifetime);
    }
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitializeBullet(float speed, float lifetime)
    {
        bulletSpeed = speed;
        bulletLifetime = lifetime;
        StartCoroutine(DestroyAfterLifetime());
    }

    IEnumerator DestroyAfterLifetime()
    {
        yield return new WaitForSeconds(bulletLifetime);
        Destroy(gameObject);
    }
}
