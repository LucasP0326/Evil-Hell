using UnityEngine;
using UnityEngine.InputSystem;

public class PlaneController : MonoBehaviour
{
    [Header("Camera Settings")]
    public Camera planeCamera;
    public Transform cameraStartPosition;
    public float cameraOrbitSensitivity = 0.2f;
    public float cameraReturnSpeed = 180f;
    public float minCameraPitch = -80f;
    public float maxCameraPitch = 80f;
    private float cameraYaw;
    private float cameraPitch;
    private Vector3 cameraHomeOffset;
    private Quaternion cameraHomeRotation;
    private float cameraReturnBlend = 1f;

    [Header("Plane Settings")]
    public float initialSpeed = 10f;
    public float maxSpeed = 50f;
    public float minSpeed = 5f;
    public float currentSpeed = 10f;
    public float rotationSpeed = 100f;
    public float acceleration = 10f;
    public float mouseSensitivity = 0.1f;

    private Rigidbody planeRigidbody;
    private Quaternion targetRotation;

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
        {
            float throttleInput = (keyboard.wKey.isPressed ? 1f : 0f) - (keyboard.sKey.isPressed ? 1f : 0f);
            currentSpeed = Mathf.Clamp(currentSpeed + throttleInput * acceleration * Time.deltaTime, minSpeed, maxSpeed);

            float rollInput = (keyboard.aKey.isPressed ? 1f : 0f) - (keyboard.dKey.isPressed ? 1f : 0f);
            targetRotation *= Quaternion.AngleAxis(rollInput * rotationSpeed * Time.deltaTime, Vector3.forward);
        }

        Mouse mouse = Mouse.current;
        if (mouse != null && Cursor.lockState == CursorLockMode.Locked)
        {
            Vector2 mouseDelta = mouse.delta.ReadValue();
            if (mouse.leftButton.isPressed)
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

    private static void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
