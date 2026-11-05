using System.Xml;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orient;
    float xRotate;
    float yRotate;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotate += mouseX;
        xRotate -= mouseY;
        xRotate = Mathf.Clamp(xRotate, -90f, 90f);
        transform.rotation = Quaternion.Euler(xRotate, yRotate, 0);
        orient.rotation = Quaternion.Euler(0, yRotate, 0);
    }
}
