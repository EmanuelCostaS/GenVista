using UnityEngine;

public class YoloObject : MonoBehaviour
{
    [Tooltip("The class ID for this object (e.g., 0=player, 1=car).")]
    public int classId = 0;

    void OnEnable()
    {
        YoloAnnotator.RegisterObject(this);
    }

    void OnDisable()
    {
        YoloAnnotator.UnregisterObject(this);
    }
}