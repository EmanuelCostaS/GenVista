using UnityEngine;
using System.Reflection;

public class TenkokuAutomation : MonoBehaviour
{
    [Header("Tenkoku Reference")]
    [Tooltip("Drag the 'Tenkoku Dynamic Sky' object here manually if it is not found automatically.")]
    public MonoBehaviour manualTenkokuModule; 

    [Header("Debug")]
    public bool showDebugLogs = true;

    [Header("Time Settings")]
    public float minStartHour = 6f;
    public float maxEndHour = 19f;

    [Header("Cloud Settings")]
    public float minCloudAmount = 0.0f;
    public float maxCloudAmount = 0.9f;

    // --- Internal State ---
    private float startHour, endHour;
    private float startCumulus, endCumulus;
    private float startStratus, endStratus;
    private float startCirrus, endCirrus;
    private float startOvercast, endOvercast;

    private MonoBehaviour tenkokuModule;
    private bool hasInitialized = false;

    void Start()
    {
        InitializeTenkoku();
        if (hasInitialized)
        {
            // CRITICAL: Disable Tenkoku's built-in auto time so it doesn't fight us
            SetTenkokuBool("autoTime", false);
            SetTenkokuBool("useAutoTime", false); // Try both common naming conventions
            
            GenerateRandomWeatherScenario();
        }
    }

    void InitializeTenkoku()
    {
        if (manualTenkokuModule != null)
        {
            tenkokuModule = manualTenkokuModule;
            hasInitialized = true;
            return;
        }

        // Try to find the Tenkoku Module dynamically
        GameObject tenkokuObj = GameObject.Find("Tenkoku Dynamic Sky");
        if (tenkokuObj != null)
        {
            tenkokuModule = tenkokuObj.GetComponent("TenkokuModule") as MonoBehaviour;
        }

        if (tenkokuModule == null)
        {
            // Fallback: Find by type name
            MonoBehaviour[] allScripts = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            foreach (var s in allScripts)
            {
                if (s.GetType().Name == "TenkokuModule")
                {
                    tenkokuModule = s;
                    break;
                }
            }
        }

        if (tenkokuModule != null)
        {
            if(showDebugLogs) Debug.Log($"[TenkokuAutomation] Successfully connected to: {tenkokuModule.name}");
            hasInitialized = true;
        }
        else
        {
            Debug.LogError("[TenkokuAutomation] CRITICAL: Could not find 'TenkokuModule'. Please drag the Tenkoku object into the 'Manual Tenkoku Module' slot in the Inspector.");
        }
    }

    public void GenerateRandomWeatherScenario()
    {
        if (!hasInitialized) InitializeTenkoku();

        startHour = Random.Range(minStartHour, maxEndHour - 2f); 
        endHour = Random.Range(startHour + 1f, maxEndHour);

        startCumulus = Random.Range(minCloudAmount, maxCloudAmount);
        endCumulus = Random.Range(minCloudAmount, maxCloudAmount);

        startStratus = Random.Range(minCloudAmount, maxCloudAmount);
        endStratus = Random.Range(minCloudAmount, maxCloudAmount);

        startCirrus = Random.Range(minCloudAmount, maxCloudAmount);
        endCirrus = Random.Range(minCloudAmount, maxCloudAmount);

        startOvercast = Random.Range(0f, 0.5f);
        endOvercast = Random.Range(0f, 0.5f);
        
        if(showDebugLogs) Debug.Log($"[TenkokuAutomation] New Scenario | Time: {startHour:F1}-{endHour:F1} | Clouds: {startCumulus:F1}-{endCumulus:F1}");
    }

    public void SetWeatherProgress(float t)
    {
        if (!hasInitialized) return;

        t = Mathf.Clamp01(t);

        // --- Interpolate Time ---
        float currentH = Mathf.Lerp(startHour, endHour, t);
        
        // Tenkoku usually uses 'currentHour' (int) and 'currentMinute' (int) or (float)
        // We set both just in case.
        SetTenkokuField("currentHour", Mathf.FloorToInt(currentH)); // Try int
        SetTenkokuField("currentMinute", (int)((currentH % 1.0f) * 60f)); // Try int

        // --- Interpolate Weather ---
        SetTenkokuField("weather_cloudCumulusAmt", Mathf.Lerp(startCumulus, endCumulus, t));
        SetTenkokuField("weather_cloudAltoStratusAmt", Mathf.Lerp(startStratus, endStratus, t));
        SetTenkokuField("weather_cloudCirrusAmt", Mathf.Lerp(startCirrus, endCirrus, t));
        SetTenkokuField("weather_OvercastAmt", Mathf.Lerp(startOvercast, endOvercast, t));
    }

    // Helper to set values using Reflection safely
    private void SetTenkokuField(string fieldName, object value)
    {
        if (tenkokuModule == null) return;
        
        var type = tenkokuModule.GetType();
        var field = type.GetField(fieldName);

        if (field != null)
        {
            try {
                // Convert value type if necessary (float to int, etc)
                if (field.FieldType == typeof(int) && value is float fVal)
                {
                    field.SetValue(tenkokuModule, (int)fVal);
                }
                else
                {
                    field.SetValue(tenkokuModule, value);
                }
            }
            catch (System.Exception e) {
                if(showDebugLogs) Debug.LogWarning($"[TenkokuAutomation] Error setting {fieldName}: {e.Message}");
            }
        }
        else
        {
            // Only warn once per field to avoid spam, or check specific fields
            if(showDebugLogs && Random.value < 0.01f) 
                Debug.LogWarning($"[TenkokuAutomation] Field '{fieldName}' not found on Tenkoku script. Check variable names.");
        }
    }

    private void SetTenkokuBool(string fieldName, bool value)
    {
        if (tenkokuModule == null) return;
        var field = tenkokuModule.GetType().GetField(fieldName);
        if (field != null) field.SetValue(tenkokuModule, value);
    }
}