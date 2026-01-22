using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Text;

public class YoloAnnotator : MonoBehaviour
{
    public static YoloAnnotator Instance;

    [Header("Capture Settings")]
    public Camera captureCamera;
    public Vector2Int imageResolution = new Vector2Int(640, 640);
    public KeyCode captureKey = KeyCode.F12;

    [Header("Save Path")]
    public string savePath = "Dataset";

    private static List<YoloObject> trackedObjects = new List<YoloObject>();
    
    private string imageSavePath;
    private string labelSavePath;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (captureCamera == null) captureCamera = Camera.main;

        string projectRootPath = Path.Combine(Application.dataPath, "..");
        imageSavePath = Path.Combine(projectRootPath, savePath, "images");
        labelSavePath = Path.Combine(projectRootPath, savePath, "labels");
        Directory.CreateDirectory(imageSavePath);
        Directory.CreateDirectory(labelSavePath);
    }

    public static void RegisterObject(YoloObject obj)
    {
        if (!trackedObjects.Contains(obj)) trackedObjects.Add(obj);
    }

    public static void UnregisterObject(YoloObject obj)
    {
        if (trackedObjects.Contains(obj)) trackedObjects.Remove(obj);
    }

    void Update()
    {
        if (Input.GetKeyDown(captureKey))
        {
            CaptureAndAnnotate();
        }
    }

    public void CaptureAndAnnotate()
    {
        if (captureCamera == null) return;

        // 1. Setup RenderTexture
        RenderTexture rt = new RenderTexture(imageResolution.x, imageResolution.y, 24);
        captureCamera.targetTexture = rt;
        captureCamera.Render();
        RenderTexture.active = rt;

        StringBuilder annotationBuilder = new StringBuilder();
        bool objectsFound = false;

        // 2. Iterate through registered objects
        var currentObjects = new List<YoloObject>(trackedObjects);

        foreach (YoloObject obj in currentObjects)
        {
            if (obj == null || !obj.gameObject.activeInHierarchy) continue;

            Rect? box = GetBoundingBox(obj.gameObject);
            if (box.HasValue)
            {
                objectsFound = true;
                annotationBuilder.AppendLine(ToYoloFormat(box.Value, obj.classId));
            }
        }

        // --- CHANGE START: Save logic moved outside the conditional check ---
        
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmssfff");
        
        // Always save the image
        SaveImage(rt, timestamp);

        // Always save the text file (it will just be empty if no objects are found)
        File.WriteAllText(Path.Combine(labelSavePath, $"image_{timestamp}.txt"), annotationBuilder.ToString());
        
        string logMsg = objectsFound ? "Captured [Objects Detected]" : "Captured [Background Only]";
        Debug.Log($"{logMsg}: image_{timestamp}");

        // --- CHANGE END ---

        captureCamera.targetTexture = null;
        RenderTexture.active = null;
        rt.Release();
        Destroy(rt);
    }

    private Rect? GetBoundingBox(GameObject obj)
    {
        // 1. Get all Renderers (MeshRenderer, SkinnedMeshRenderer)
        var renderers = obj.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return null;

        float minX = float.MaxValue, minY = float.MaxValue, maxX = float.MinValue, maxY = float.MinValue;
        bool visible = false;

        foreach (var r in renderers)
        {
            if (!r.enabled) continue; 
            
            Mesh mesh = null;
            if (r is MeshRenderer)
            {
                var mf = r.GetComponent<MeshFilter>();
                if (mf != null) mesh = mf.sharedMesh;
            }
            else if (r is SkinnedMeshRenderer smr)
            {
                mesh = smr.sharedMesh;
            }

            if (mesh == null) continue;

            Vector3[] verts = mesh.vertices;
            for (int i = 0; i < verts.Length; i++)
            {
                Vector3 worldPos = r.transform.TransformPoint(verts[i]);
                Vector3 screenPos = captureCamera.WorldToScreenPoint(worldPos);

                if (screenPos.z > 0)
                {
                    visible = true;
                    minX = Mathf.Min(minX, screenPos.x);
                    minY = Mathf.Min(minY, screenPos.y);
                    maxX = Mathf.Max(maxX, screenPos.x);
                    maxY = Mathf.Max(maxY, screenPos.y);
                }
            }
        }

        if (!visible) return null;

        minX = Mathf.Clamp(minX, 0, imageResolution.x);
        maxX = Mathf.Clamp(maxX, 0, imageResolution.x);
        minY = Mathf.Clamp(minY, 0, imageResolution.y);
        maxY = Mathf.Clamp(maxY, 0, imageResolution.y);

        if (maxX <= minX || maxY <= minY) return null;

        return new Rect(minX, minY, maxX - minX, maxY - minY);
    }

    private string ToYoloFormat(Rect rect, int classId)
    {
        float x = (rect.x + rect.width / 2f) / imageResolution.x;
        float y = 1f - ((rect.y + rect.height / 2f) / imageResolution.y);
        float w = rect.width / imageResolution.x;
        float h = rect.height / imageResolution.y;
        return $"{classId} {x:F6} {y:F6} {w:F6} {h:F6}";
    }

    private void SaveImage(RenderTexture rt, string timestamp)
    {
        Texture2D tex = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);
        tex.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        tex.Apply();
        File.WriteAllBytes(Path.Combine(imageSavePath, $"image_{timestamp}.png"), tex.EncodeToPNG());
        Destroy(tex);
    }
}