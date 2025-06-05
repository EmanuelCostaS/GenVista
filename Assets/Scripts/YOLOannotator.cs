using UnityEngine;
using System.IO; // Required for file operations
using System.Collections.Generic; // If you have multiple objects to detect in the future

public class YoloAnnotator : MonoBehaviour
{
    public GameObject targetObject; // Assign your target GameObject in the Inspector
    public Camera captureCamera;    // Assign the camera that will be capturing the images
    public string savePath = "YOLO_Dataset"; // Folder to save images and annotations
    public int objectClassId = 0; // Define the class ID for your target object
    public KeyCode captureKey = KeyCode.Space; // Key to trigger capture
    public Vector2Int imageResolution = new Vector2Int(1920, 1200); // Desired image resolution

    private void Start()
    {
        if (captureCamera == null)
        {
            captureCamera = Camera.main; // Default to the main camera if not assigned
        }

        if (targetObject == null)
        {
            Debug.LogError("Target Object not assigned!");
            enabled = false; // Disable script if no target
            return;
        }

        // Create the save directory if it doesn't exist
        if (!Directory.Exists(Path.Combine(Application.dataPath, "..", savePath))) // Save outside Assets
        {
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", savePath));
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", savePath, "images"));
            Directory.CreateDirectory(Path.Combine(Application.dataPath, "..", savePath, "labels"));
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(captureKey))
        {
            CaptureAndAnnotate();
        }
    }

    void CaptureAndAnnotate()
    {
        if (targetObject == null || !targetObject.activeInHierarchy)
        {
            Debug.LogWarning("Target object is null or inactive. Skipping capture.");
            return;
        }

        Renderer rend = targetObject.GetComponent<Renderer>();
        if (rend == null || !rend.isVisible)
        {
            Debug.LogWarning("Target object's renderer is not available or not visible. Skipping capture.");
            return; // Object is not visible, no need to annotate
        }

        Bounds bounds = rend.bounds;
        Vector3[] corners = new Vector3[8];
        GetBoundsCorners(bounds, corners);

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        bool objectIsInView = false;

        for (int i = 0; i < 8; i++)
        {
            Vector3 screenPoint = captureCamera.WorldToScreenPoint(corners[i]);

            // Check if the corner is in front of the camera and within screen bounds
            if (screenPoint.z > 0) // In front of the camera
            {
                objectIsInView = true;
                minX = Mathf.Min(minX, screenPoint.x);
                minY = Mathf.Min(minY, screenPoint.y);
                maxX = Mathf.Max(maxX, screenPoint.x);
                maxY = Mathf.Max(maxY, screenPoint.y);
            }
        }

        // If no part of the object is in front of the camera, don't proceed
        if (!objectIsInView)
        {
            Debug.Log("Target object is not in camera view. Skipping capture.");
            return;
        }

        // Clamp values to be within screen boundaries (0 to screen width/height)
        // Important if parts of the object are off-screen
        minX = Mathf.Clamp(minX, 0, captureCamera.pixelWidth);
        maxX = Mathf.Clamp(maxX, 0, captureCamera.pixelWidth);
        minY = Mathf.Clamp(minY, 0, captureCamera.pixelHeight);
        maxY = Mathf.Clamp(maxY, 0, captureCamera.pixelHeight);

        // Check if the bounding box has a valid area
        if (maxX <= minX || maxY <= minY)
        {
            Debug.LogWarning("Bounding box is invalid (e.g., object is behind other objects or completely off-screen after clamping). Skipping capture.");
            return;
        }

        // --- Prepare for screenshot using RenderTexture ---
        RenderTexture rt = new RenderTexture(imageResolution.x, imageResolution.y, 24);
        captureCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(imageResolution.x, imageResolution.y, TextureFormat.RGB24, false);
        captureCamera.Render(); // Render the camera's view to the RenderTexture

        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, imageResolution.x, imageResolution.y), 0, 0);
        screenShot.Apply(); // Apply changes to the texture

        captureCamera.targetTexture = null; // Reset camera's target texture
        RenderTexture.active = null; // Reset active RenderTexture
        Destroy(rt); // Clean up RenderTexture

        // --- Save Image ---
        byte[] bytes = screenShot.EncodeToPNG(); // Or EncodeToJPG()
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
        string imageFileName = $"image_{timestamp}.png";
        string imagePath = Path.Combine(Application.dataPath, "..", savePath, "images", imageFileName);
        File.WriteAllBytes(imagePath, bytes);
        Debug.Log($"Saved Image: {imagePath}");

        // --- Calculate YOLO Annotation ---
        // Note: Screen coordinates (0,0) are bottom-left. YOLO needs top-left.
        // And coordinates need to be normalized by the CAPTURED image dimensions (imageResolution)

        float boxWidth = maxX - minX;
        float boxHeight = maxY - minY;

        // Adjust min/max X and Y based on how much the actual camera view (captureCamera.pixelWidth/Height)
        // maps to the target imageResolution. This is important if the camera's aspect ratio
        // doesn't match the imageResolution aspect ratio, or if you're rendering a sub-region.
        // For this script, we assume the RenderTexture matches the desired output, so we normalize against imageResolution.

        float normXCenter = (minX + boxWidth / 2f) / imageResolution.x;
        // YOLO Y is from top, Unity screen Y is from bottom.
        float normYCenter = ((imageResolution.y - maxY) + boxHeight / 2f) / imageResolution.y;
        float normWidth = boxWidth / imageResolution.x;
        float normHeight = boxHeight / imageResolution.y;

        // Clamp normalized values to be between 0 and 1, just in case
        normXCenter = Mathf.Clamp01(normXCenter);
        normYCenter = Mathf.Clamp01(normYCenter);
        normWidth = Mathf.Clamp01(normWidth);
        normHeight = Mathf.Clamp01(normHeight);


        //string annotation = $"{objectClassId} {normXCenter} {normYCenter} {normWidth} {normHeight}";
        string annotation = string.Format(System.Globalization.CultureInfo.InvariantCulture,
                                  "{0} {1:F6} {2:F6} {3:F6} {4:F6}",
                                  objectClassId,
                                  normXCenter,
                                  normYCenter,
                                  normWidth,
                                  normHeight);

        // --- Save Annotation ---
        string annotationFileName = $"image_{timestamp}.txt";
        string annotationPath = Path.Combine(Application.dataPath, "..", savePath, "labels", annotationFileName);
        File.WriteAllText(annotationPath, annotation);
        Debug.Log($"Saved Annotation: {annotationPath}");

        Destroy(screenShot); // Clean up Texture2D
    }

    // Helper to get the 8 corners of a bounds
    void GetBoundsCorners(Bounds bounds, Vector3[] corners)
    {
        Vector3 center = bounds.center;
        Vector3 extents = bounds.extents;

        corners[0] = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
        corners[1] = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);
        corners[2] = new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z);
        corners[3] = new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z);
        corners[4] = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
        corners[5] = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
        corners[6] = new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z);
        corners[7] = new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z);
    }

    // Optional: Gizmos to visualize the bounding box in the editor
    void OnDrawGizmos()
    {
        if (targetObject != null && captureCamera != null)
        {
            Renderer rend = targetObject.GetComponent<Renderer>();
            if (rend == null || !rend.isVisible) return;

            Bounds bounds = rend.bounds;
            Vector3[] corners = new Vector3[8];
            GetBoundsCorners(bounds, corners);

            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            bool objectIsInView = false;

            for (int i = 0; i < 8; i++)
            {
                Vector3 screenPoint = captureCamera.WorldToScreenPoint(corners[i]);
                if (screenPoint.z > 0)
                {
                    objectIsInView = true;
                    minX = Mathf.Min(minX, screenPoint.x);
                    minY = Mathf.Min(minY, screenPoint.y);
                    maxX = Mathf.Max(maxX, screenPoint.x);
                    maxY = Mathf.Max(maxY, screenPoint.y);
                }
            }

            if (!objectIsInView) return;

            minX = Mathf.Clamp(minX, 0, captureCamera.pixelWidth);
            maxX = Mathf.Clamp(maxX, 0, captureCamera.pixelWidth);
            minY = Mathf.Clamp(minY, 0, captureCamera.pixelHeight);
            maxY = Mathf.Clamp(maxY, 0, captureCamera.pixelHeight);

            if (maxX <= minX || maxY <= minY) return;


            // Draw Gizmo rectangle on screen (only works in Game View with Gizmos enabled)
            // This is a bit tricky as Gizmos draw in world space.
            // A more robust visualization would be to use an OnGUI call or a UI Panel.

            // For simplicity, let's draw the world space bounds
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(bounds.center, bounds.size);

            // And lines from camera to screen box corners (approximate)
            // This part is more complex to draw accurately in screen space with Gizmos.
            // Consider drawing a UI rectangle instead for precise screen space visualization.
            // Rect screenRect = new Rect(minX, minY, maxX - minX, maxY - minY);
            // Debug.Log("Screen BBox (approx): " + screenRect); // Log for checking
        }
    }
}