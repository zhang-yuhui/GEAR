using Unity.Mathematics;
using UnityEngine;

public class CameraRotation : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float mouseSensitivity = 1f;
    public float moveSpeed = 1f;
    private float rotationX = 0.0f;
    private float rotationY = 0.0f;
    private bool isCursorLocked;
    void Start()
    {
        rotationX = transform.eulerAngles.x;
        rotationY = transform.eulerAngles.y;
        isCursorLocked = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetMouseButtonDown(0) && !isCursorLocked){
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isCursorLocked = true;
        }
        if (Input.GetKeyDown(KeyCode.LeftAlt) || Input.GetKeyDown(KeyCode.RightAlt))
        {
            // Unlock the cursor and make it visible when Alt is pressed
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            isCursorLocked = false;
        }
        else if (Input.GetKeyUp(KeyCode.LeftAlt) || Input.GetKeyUp(KeyCode.RightAlt))
        {
            // Lock the cursor and hide it when Alt is released
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            isCursorLocked = true;
        }
        if(! isCursorLocked)
            return;
        Vector3 moveDirection = Vector3.zero;

        Vector3 fixedForward = Vector3.Normalize(new Vector3(transform.forward.x, 0f, transform.forward.z));
        Vector3 fixedRight = Vector3.Normalize(new Vector3(transform.right.x, 0f, transform.right.z));


        if (Input.GetKey(KeyCode.W))
        {
            moveDirection += fixedForward;
            //// Debug.Log($"forward: {transform.forward}");
        }
        if (Input.GetKey(KeyCode.S))
        {
            moveDirection -= fixedForward;
        }
        if (Input.GetKey(KeyCode.A))
        {
            //// Debug.Log($"forward: {transform.right}");
            moveDirection -= fixedRight;
        }
        if (Input.GetKey(KeyCode.D))
        {
            moveDirection += fixedRight;
        }
        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveDirection -= Vector3.up;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            moveDirection += Vector3.up;
        }

        // Normalize direction and move the camera
        moveDirection.Normalize();
        transform.position += moveDirection * moveSpeed * Time.deltaTime;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * 0.01f;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * 0.01f;
        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -89f, 89f);

        rotationY += mouseX;
        transform.eulerAngles = new Vector3(rotationX, rotationY, 0.0f);
    }
}
