using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    //public float jumpForce = 5f;

    [Header("Mouse Look")]
   // public float mouseSensitivity = 2f;
  //  public float maxLookAngle = 85f;

    private Rigidbody rb;
    private float xRotation;
    //private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        rb.freezeRotation = true;

      //  Cursor.lockState = CursorLockMode.Locked;
      //  Cursor.visible = false;
    }

    void Update()
    {
        // Mouse look
       // float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
       // float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

      //  xRotation -= mouseY;
      //  xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

       // transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Rotate the body horizontally
       // rb.MoveRotation(
       //     Quaternion.Euler(0f, transform.eulerAngles.y + mouseX, 0f)
       // );

        // Jump
       // if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
       // {
       //     rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
      //      isGrounded = false;
      //  }

        // Unlock cursor with Escape
      //  if (Input.GetKeyDown(KeyCode.Escape))
      //  {
      //      Cursor.lockState = CursorLockMode.None;
      //      Cursor.visible = true;
      //  }
    }

    void FixedUpdate()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 move = (transform.right * x + transform.forward * z) * moveSpeed;
        Vector3 newVelocity = new Vector3(move.x, rb.linearVelocity.y, move.z);
        rb.linearVelocity = newVelocity;
        move.Normalize();

        Vector3 velocity = rb.linearVelocity;
        velocity.x = move.x * moveSpeed;
        velocity.z = move.z * moveSpeed;

        rb.linearVelocity = velocity;
    }

   // void OnCollisionStay(Collision collision)
   // {
  //      foreach (ContactPoint contact in collision.contacts)
   //     {
   //         if (contact.normal.y > 0.5f)
   //         {
    //            isGrounded = true;
    //            return;
            }
    //    }
   // }

   // void OnCollisionExit(Collision collision)
  //  {
   //     isGrounded = false;
   // }
