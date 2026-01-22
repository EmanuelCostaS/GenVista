using UnityEngine;

public class PlayerCam : MonoBehaviour
{
    public float sensX;
    public float sensY;

    public Transform orientation;

    float xRotation;
    float yRotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false; // Usually cleaner to hide it
    }

    private void Update()
    {
        // Get mouse input
        float mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * sensX;
        float mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * sensY;

        yRotation += mouseX;
        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Rotate cam and orientation
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }

    // --- ADDED THIS METHOD ---
    // Call this to forcefully reset where the camera thinks it is looking
    public void ResetCameraRotation(Quaternion targetRotation)
    {
        // Extract the target angles
        Vector3 euler = targetRotation.eulerAngles;
        
        // Update internal variables so they don't snap back later
        yRotation = euler.y;
        xRotation = euler.x;
        
        // Handle wrapping (Unity Euler can be 0..360, we want -90..90 for X usually)
        if (xRotation > 180) xRotation -= 360;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Apply immediately
        transform.rotation = Quaternion.Euler(xRotation, yRotation, 0);
        orientation.rotation = Quaternion.Euler(0, yRotation, 0);
    }
}