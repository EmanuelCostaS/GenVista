using UnityEngine;
// We wrap this in a define check just in case, but you likely have Cesium installed.
// If your IDE complains, ensure you have the CesiumForUnity package.
#if CESIUM_FOR_UNITY_AVAILABLE || true 
using CesiumForUnity;
#endif

public class CesiumTeleport : MonoBehaviour
{
    [Header("Cesium Reference")]
    [Tooltip("Drag your CesiumGeoreference object here.")]
    public CesiumGeoreference georeference;

    [Header("Debug")]
    public bool printDebug = true;

    /// <summary>
    /// Teleports the world origin to the specified Longitude, Latitude, and Height.
    /// </summary>
    public void JumpToLocation(double latitude, double longitude, double height)
    {
#if CESIUM_FOR_UNITY_AVAILABLE || true
        if (georeference != null)
        {
            if (printDebug) Debug.Log($"[CesiumTeleport] Teleporting to Lat: {latitude}, Lon: {longitude}, Height: {height}");
            
            // Note: Cesium uses (Longitude, Latitude, Height) order usually
            georeference.SetOriginLongitudeLatitudeHeight(longitude, latitude, height);
        }
        else
        {
            Debug.LogError("[CesiumTeleport] CesiumGeoreference is not assigned!");
        }
#else
        Debug.LogWarning("[CesiumTeleport] Cesium for Unity not found or not compiled.");
#endif
    }
}