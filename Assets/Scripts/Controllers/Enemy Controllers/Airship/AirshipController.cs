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

    [Header("Movement Settings")]
    public Transform moveTarget;
    public float targetMarginofError = 5f;
    private Rigidbody airshipRigidbody;
    public float speed = 10f;
    public float turnTime = 1f;
    public float rotationSpeed = 5f;
    public float maxTurnAngle = 45f;

    [Header("Targeting Settings")]
    public Transform frontPivot;
    public GameObject target;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        airshipRigidbody = GetComponent<Rigidbody>();
        if (airshipRigidbody != null)
        {
            //airshipRigidbody.isKinematic = true;
            airshipRigidbody.useGravity = false;
        }
    }
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        MoveTowardsTarget();
    }

    void FixedUpdate()
    {
        Vector3 directionToTarget = Vector3.zero;

        if (moveTarget != null)
        {
            directionToTarget = moveTarget.position - transform.position;
        }

        if (moveTarget != null)
        {
            Vector3 currentVelocity = airshipRigidbody.linearVelocity;
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
            airshipRigidbody.linearVelocity = direction * speed;
            if (direction.sqrMagnitude > 0f)
            {
                Vector3 forward = frontPivot != null
                    ? frontPivot.forward
                    : transform.forward;
                Quaternion rotationDelta = Quaternion.FromToRotation(forward, direction);
                airshipRigidbody.MoveRotation(rotationDelta * airshipRigidbody.rotation);
            }
        }
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

    public void MoveToPoint(Transform point)
    {
        moveTarget = point;
    }

    private void MoveTowardsTarget()
    {
        if (moveTarget == null)
        {
            return;
        }

        Vector3 directionToTarget = moveTarget.position - transform.position;
        if (directionToTarget.sqrMagnitude <= 0.0001f)
        {
            return;
        }

        float deltaTime = Time.deltaTime;
        Transform forwardPivot = frontPivot != null ? frontPivot : transform;
        float turnStep = Mathf.Min(
            Mathf.Max(0f, rotationSpeed) * deltaTime,
            Mathf.Max(0f, maxTurnAngle));

        if (turnStep > 0f)
        {
            Vector3 turnedForward = Vector3.RotateTowards(
                forwardPivot.forward,
                directionToTarget.normalized,
                turnStep * Mathf.Deg2Rad,
                0f);
            Quaternion rotationDelta = Quaternion.FromToRotation(forwardPivot.forward, turnedForward);
            transform.rotation = rotationDelta * transform.rotation;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            moveTarget.position,
            Mathf.Max(0f, speed) * deltaTime);
    }
}
