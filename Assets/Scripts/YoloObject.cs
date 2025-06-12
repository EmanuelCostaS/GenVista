using UnityEngine;

/// <summary>
/// Attach this component to any GameObject you want to be detected by the YoloAnnotator.
/// </summary>
public class YoloObject : MonoBehaviour
{
    [Tooltip("The class ID for this object, as defined in your YOLO model's configuration (e.g., 0 for player, 1 for car, etc.).")]
    public int classId = 0;
}