using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class YoloAnnotator : MonoBehaviour
{
    [Header("Capture Settings")]
    public Camera captureCamera;
    public Vector2Int imageResolution = new Vector2Int(640, 640);
    public KeyCode captureKey = KeyCode.F12;

    [Header("Save Path")]
    public string savePath = "YOLO_Dataset";

    private List<YoloObject> trackedObjects = new List<YoloObject>();
    private string imageSavePath;
    private string labelSavePath;

    void Start()
    {
        if (captureCamera == null)
        {
            captureCamera = Camera.main;
        }

        trackedObjects.AddRange(FindObjectsByType<YoloObject>(FindObjectsSortMode.None));
        if (trackedObjects.Count == 0)
        {
            Debug.LogWarning("YoloAnnotator: No objects with the 'YoloObject' component found in the scene. No annotations will be generated.");
        }
        else
        {
            Debug.Log($"YoloAnnotator: Found {trackedObjects.Count} trackable objects.");
        }

        string projectRootPath = Path.Combine(Application.dataPath, "..");
        imageSavePath = Path.Combine(projectRootPath, savePath, "images");
        labelSavePath = Path.Combine(projectRootPath, savePath, "labels");
        Directory.CreateDirectory(imageSavePath);
        Directory.CreateDirectory(labelSavePath);
        Debug.Log($"Dataset will be saved in: {Path.Combine(projectRootPath, savePath)}");
    }

    void Update()
    {
        if (Input.GetKeyDown(captureKey))
        {
            Debug.Log($"Manual capture triggered with key '{captureKey}'.");
            CaptureAndAnnotate();
        }
    }

    public void CaptureAndAnnotate()
    {
        if (captureCamera == null)
        {
            Debug.LogError("YoloAnnotator: Capture Camera is not set!");
            return;
        }

        StringBuilder annotationBuilder = new StringBuilder();
        bool objectsToAnnotateFound = false;

        foreach (YoloObject obj in trackedObjects)
        {
            if (obj == null || !obj.gameObject.activeInHierarchy) continue;

            // --- NEW DEBUG LOG ---
            // This will tell us the name of the root object being processed.
            Debug.Log($"[DEBUG] Processing object named: '{obj.gameObject.name}'");

            Rect? boundingBox = GetObjectScreenBoundingBox(obj.gameObject);

            if (boundingBox.HasValue)
            {
                objectsToAnnotateFound = true;
                string yoloLine = ToYoloFormat(boundingBox.Value, obj.classId);
                annotationBuilder.AppendLine(yoloLine);
            }
        }

        if (objectsToAnnotateFound)
        {
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
            SaveScreenshot(timestamp);
            File.WriteAllText(Path.Combine(labelSavePath, $"image_{timestamp}.txt"), annotationBuilder.ToString());
        }
    }

    private Rect? GetObjectScreenBoundingBox(GameObject obj)
    {
        var meshFilters = obj.GetComponentsInChildren<MeshFilter>();
        var renderers = obj.GetComponentsInChildren<Renderer>();

        // --- NEW DEBUG LOG ---
        // This will tell us exactly which meshes the script finds inside the root object.
        Debug.Log($"[DEBUG] -- The object '{obj.name}' contains {meshFilters.Length} mesh(es).");
        foreach (var mf in meshFilters)
        {
            Debug.Log($"[DEBUG] ---- Found mesh on child GameObject: '{mf.gameObject.name}'");
        }


        bool isAnyPartVisible = false;
        foreach (var r in renderers)
        {
            if (r.isVisible)
            {
                isAnyPartVisible = true;
                break;
            }
        }

        if (meshFilters.Length == 0 || !isAnyPartVisible)
        {
            return null;
        }

        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        bool objectIsInView = false;

        foreach (var meshFilter in meshFilters)
        {
            Vector3[] vertices = meshFilter.mesh.vertices;
            if (vertices.Length == 0) continue;

            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 worldPoint = meshFilter.transform.TransformPoint(vertices[i]);
                Vector3 screenPoint = captureCamera.WorldToScreenPoint(worldPoint);

                if (screenPoint.z > 0)
                {
                    objectIsInView = true;
                    minX = Mathf.Min(minX, screenPoint.x);
                    minY = Mathf.Min(minY, screenPoint.y);
                    maxX = Mathf.Max(maxX, screenPoint.x);
                    maxY = Mathf.Max(maxY, screenPoint.y);
                }
            }
        }

        if (!objectIsInView)
        {
            return null;
        }
        
        minX = Mathf.Clamp(minX, 0, imageResolution.x);
        maxX = Mathf.Clamp(maxX, 0, imageResolution.x);
        minY = Mathf.Clamp(minY, 0, imageResolution.y);
        maxY = Mathf.Clamp(maxY, 0, imageResolution.y);

        if (maxX <= minX || maxY <= minY)
        {
            return null;
        }

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private string ToYoloFormat(Rect rect, int classId)
    {
        float normXCenter = (rect.x + rect.width / 2f) / imageResolution.x;
        float normYCenter = 1f - ((rect.y + rect.height / 2f) / imageResolution.y);
        float normWidth = rect.width / imageResolution.x;
        float normHeight = rect.height / imageResolution.y;

        return string.Format(System.Globalization.CultureInfo.InvariantCulture,
                             "{0} {1:F6} {2:F6} {3:F6} {4:F6}",
                             classId, normXCenter, normYCenter, normWidth, normHeight);
    }

    private void SaveScreenshot(string timestamp)
    {
        RenderTexture rt = new RenderTexture(imageResolution.x, imageResolution.y, 24);
        captureCamera.targetTexture = rt;
        Texture2D screenShot = new Texture2D(imageResolution.x, imageResolution.y, TextureFormat.RGB24, false);
        captureCamera.Render();
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, imageResolution.x, imageResolution.y), 0, 0);
        screenShot.Apply();
        
        byte[] bytes = screenShot.EncodeToPNG();
        string fileName = $"image_{timestamp}.png";
        File.WriteAllBytes(Path.Combine(imageSavePath, fileName), bytes);

        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);
        Destroy(screenShot);
    }
}