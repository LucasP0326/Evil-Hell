using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{
    public enum InputMode
    {
        Mouse,
        Directional
    }

    public enum CameraMode
    {
        ThirdPerson,
        FirstPerson,
        FirstPersonCockpit
    }

    [Header("Input Settings")]
    public InputMode inputMode = InputMode.Directional;
    public bool invertVerticalFlight = false;

    [Header("Camera Settings")]
    public CameraMode cameraMode = CameraMode.ThirdPerson;
    public Camera planeCamera;
        public Camera firstPersonCamera;
    public Camera firstPersonCockpitCamera;
    public Transform cameraStartPosition;
    public float cameraOrbitSensitivity = 0.2f;
    public float cameraReturnSpeed = 180f;
    public float minCameraPitch = -80f;
    public float maxCameraPitch = 80f;
    private float cameraYaw;
    private float cameraPitch;
    private Vector3 cameraHomeOffset;
    private Quaternion cameraHomeRotation;
    private float cameraReturnBlend = 0f;

    [Header("Movement Settings")]
    public float initialSpeed = 10f;
    public float maxSpeed = 50f;
    public float minSpeed = 5f;
    public float currentSpeed = 10f;
    public float rotationSpeed = 100f;
    public float acceleration = 10f;
    public float mouseSensitivity = 0.1f;
    private Rigidbody planeRigidbody;
    private Quaternion targetRotation;

    [Header("Plane Combat Settings")]
    public GameObject projectilePrefab;
    public Transform projectileSpawnPoint1;
    public Transform projectileSpawnPoint2;
    public float projectileSpeed = 20f;
    public float fireRate = 0.5f;
    public float projectileDamage = 10f;
    public float projectileLifetime = 5f;
    private bool useSecondProjectileSpawnPoint;
    private float nextFireTime;

    [Header("Plane Stats")]
    public float health = 100f;
    public float maxHealth = 100f;

    [Header("Cursor Settings")]
    private CursorLockMode previousCursorLockState;
    private bool previousCursorVisibility;

    void Awake()
    {
        planeRigidbody = GetComponent<Rigidbody>();
        if (planeRigidbody == null)
        {
            planeRigidbody = gameObject.AddComponent<Rigidbody>();
        }

        currentSpeed = Mathf.Clamp(initialSpeed, minSpeed, maxSpeed);
        targetRotation = planeRigidbody.rotation;

        if (planeCamera == null)
        {
            planeCamera = Camera.main;
        }

        if (planeCamera != null)
        {
            if (cameraStartPosition == null)
            {
                cameraStartPosition = planeCamera.transform;
            }

            cameraHomeOffset = transform.InverseTransformPoint(cameraStartPosition.position);
            cameraHomeRotation = Quaternion.Inverse(transform.rotation) * cameraStartPosition.rotation;
        }
    }

    void Start()
    {
        planeRigidbody.linearVelocity = transform.forward * currentSpeed;
    }

    void OnEnable()
    {
        previousCursorLockState = Cursor.lockState;
        previousCursorVisibility = Cursor.visible;
        LockCursor();
    }

    void OnDisable()
    {
        Cursor.lockState = previousCursorLockState;
        Cursor.visible = previousCursorVisibility;
    }

    void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
            if (inputMode == InputMode.Mouse)
            {
                //W and S for Accelerate and Decelerate
                float throttleInput = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
                currentSpeed = Mathf.Clamp(currentSpeed + throttleInput * acceleration * Time.deltaTime, minSpeed, maxSpeed);

                //A and D for Roll
                float rollInput = (keyboard.aKey.isPressed ? 1f : 0f) - (keyboard.dKey.isPressed ? 1f : 0f);
                targetRotation *= Quaternion.AngleAxis(rollInput * rotationSpeed * Time.deltaTime, Vector3.forward);

                //E and Q for Yaw
                float yawInput = (keyboard.eKey.isPressed ? 1f : 0f) - (keyboard.qKey.isPressed ? 1f : 0f);
                targetRotation *= Quaternion.AngleAxis(yawInput * rotationSpeed * Time.deltaTime, Vector3.up);
            }
            if (inputMode == InputMode.Directional)
            {
                //Space and Ctrl for Accelerate and Decelerate
                if (keyboard.spaceKey.isPressed)
                {
                    currentSpeed = Mathf.Clamp(currentSpeed + acceleration * Time.deltaTime, minSpeed, maxSpeed);
                }
                else if (keyboard.leftCtrlKey.isPressed)
                {
                    currentSpeed = Mathf.Clamp(currentSpeed - acceleration * Time.deltaTime, minSpeed, maxSpeed);
                }
                //float throttleInput = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
                //currentSpeed = Mathf.Clamp(currentSpeed + throttleInput * acceleration * Time.deltaTime, minSpeed, maxSpeed);

                //A and D for Roll
                float rollInput = (keyboard.aKey.isPressed ? 1f : 0f) - (keyboard.dKey.isPressed ? 1f : 0f);
                targetRotation *= Quaternion.AngleAxis(rollInput * rotationSpeed * Time.deltaTime, Vector3.forward);

                //E and Q for Yaw
                float yawInput = (keyboard.eKey.isPressed ? 1f : 0f) - (keyboard.qKey.isPressed ? 1f : 0f);
                targetRotation *= Quaternion.AngleAxis(yawInput * rotationSpeed * Time.deltaTime, Vector3.up);

                //W and S for Pitch (Change with invertVerticalFlight)
                if (invertVerticalFlight)
                {
                    float pitchInput = (keyboard.sKey.isPressed ? 1f : 0f) - (keyboard.wKey.isPressed ? 1f : 0f);
                    targetRotation *= Quaternion.AngleAxis(pitchInput * rotationSpeed * Time.deltaTime, Vector3.right);
                }
                else
                {
                    float pitchInput = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
                    targetRotation *= Quaternion.AngleAxis(pitchInput * rotationSpeed * Time.deltaTime, Vector3.right);
                }
            }

        Mouse mouse = Mouse.current;
        if (inputMode == InputMode.Mouse)
        {
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                Vector2 mouseDelta = mouse.delta.ReadValue();
                if (keyboard.ctrlKey.isPressed)
                {
                    cameraReturnBlend = 0f;
                    cameraYaw += mouseDelta.x * cameraOrbitSensitivity;
                    cameraPitch = Mathf.Clamp(cameraPitch - mouseDelta.y * cameraOrbitSensitivity, minCameraPitch, maxCameraPitch);
                }
                else
                {
                    cameraYaw = Mathf.MoveTowardsAngle(cameraYaw, 0f, cameraReturnSpeed * Time.deltaTime);
                    cameraPitch = Mathf.MoveTowards(cameraPitch, 0f, cameraReturnSpeed * Time.deltaTime);
                    cameraReturnBlend = Mathf.MoveTowards(cameraReturnBlend, 1f, cameraReturnSpeed / 90f * Time.deltaTime);
                    mouseDelta *= mouseSensitivity;
                    targetRotation *= Quaternion.Euler(-mouseDelta.y, mouseDelta.x, 0f);
                }
            }
        }

        if (inputMode == InputMode.Directional)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
            {
                cameraReturnBlend = 0f;
                cameraYaw += mouseDelta.x * cameraOrbitSensitivity;
                cameraPitch = Mathf.Clamp(cameraPitch - mouseDelta.y * cameraOrbitSensitivity, minCameraPitch, maxCameraPitch);
            }
        }

        // Shoot continuously while the left mouse button is held.
        if (mouse != null && mouse.leftButton.isPressed && Time.time >= nextFireTime)
        {
            ShootProjectile();
            nextFireTime = Time.time + fireRate;
        }

        if (keyboard != null && keyboard.vKey.wasPressedThisFrame)
        {
            SwitchPerspective();
        }
    }

    void LateUpdate()
    {
        if (planeCamera == null || !planeCamera.isActiveAndEnabled)
        {
            return;
        }

        Vector3 cameraOffset = Quaternion.Euler(cameraPitch, cameraYaw, 0f) * cameraHomeOffset;
        planeCamera.transform.position = transform.TransformPoint(cameraOffset);
        Quaternion lookRotation = Quaternion.LookRotation(transform.position - planeCamera.transform.position, transform.up);
        Quaternion homeRotation = transform.rotation * cameraHomeRotation;
        planeCamera.transform.rotation = Quaternion.Slerp(lookRotation, homeRotation, cameraReturnBlend);
    }

    void FixedUpdate()
    {
        planeRigidbody.MoveRotation(targetRotation);
        planeRigidbody.linearVelocity = targetRotation * Vector3.forward * currentSpeed;
    }

    public void SwitchPerspective()
    {
        if (cameraMode == CameraMode.ThirdPerson)
        {
            cameraMode = CameraMode.FirstPerson;
            if (firstPersonCamera != null)
            {
                planeCamera.enabled = false;
                firstPersonCamera.enabled = true;
            }
        }
        else if (cameraMode == CameraMode.FirstPerson)
        {
            cameraMode = CameraMode.FirstPersonCockpit;
            if (firstPersonCockpitCamera != null)
            {
                firstPersonCamera.enabled = false;
                firstPersonCockpitCamera.enabled = true;
            }
        }
        else if (cameraMode == CameraMode.FirstPersonCockpit)
        {
            cameraMode = CameraMode.ThirdPerson;
            if (planeCamera != null)
            {
                firstPersonCockpitCamera.enabled = false;
                planeCamera.enabled = true;
            }
        }
    }

    public void ShootProjectile()
    {
        if (projectilePrefab != null)
        {
            Transform spawnPoint = useSecondProjectileSpawnPoint ? projectileSpawnPoint2 : projectileSpawnPoint1;
            if (spawnPoint == null)
            {
                spawnPoint = useSecondProjectileSpawnPoint ? projectileSpawnPoint1 : projectileSpawnPoint2;
            }

            if (spawnPoint == null)
            {
                return;
            }

            useSecondProjectileSpawnPoint = !useSecondProjectileSpawnPoint;
            GameObject projectile = Instantiate(projectilePrefab, spawnPoint.position, spawnPoint.rotation);
            Rigidbody projectileRigidbody = projectile.GetComponent<Rigidbody>();
            if (projectileRigidbody != null)
            {
                float bulletSpeed = Mathf.Max(projectileSpeed * currentSpeed, currentSpeed);
                projectileRigidbody.linearVelocity = transform.forward * bulletSpeed;
            }
        }
    }

    public void TakeDamage(float damageAmount)
    {
        health -= damageAmount;
        health = Mathf.Clamp(health, 0f, maxHealth);

        if (health <= 0f)
        {
            // Handle plane destruction or game over logic here
            Debug.Log("Plane destroyed!");
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

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
