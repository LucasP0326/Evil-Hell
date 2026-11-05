using UnityEngine;
using UnityEngine.AI;

public class AirshipController : MonoBehaviour
{
    public enum AirshipType
    {
        Transport,
        Bomber,
        GunPlatform
    }
   

    [Header("Airship Settings")]
    public AirshipType airshipType;
    public float health = 5000f;
    public float maxHealth = 5000f;
    public float visionRange = 1000f;
    public float speed = 10f;

    [Header("Targeting Settings")]
    public Transform frontPivot;
    public GameObject target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        if (health <= 0f)
        {
            // Handle airship destruction or game over logic here
            Debug.Log("Airship destroyed!");
            Destroy(gameObject);
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
