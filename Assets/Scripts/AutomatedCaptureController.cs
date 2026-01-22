using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public struct ScenarioLocation
{
    public string name; 
    public double latitude;
    public double longitude;
    public double height;
}

public class AutomatedCaptureController : MonoBehaviour
{
    [Header("References")]
    public Movement playerController; 
    public PlayerCam playerCamScript; // DRAG "PlayerCam" SCRIPT HERE
    public YoloAnnotator yoloAnnotator;
    public CesiumTeleport cesiumTeleporter; 
    public ObjectSpawner objectSpawner;
    public TenkokuAutomation weatherAutomation; 

    [Header("Automation Flow")]
    public KeyCode toggleAutomationKey = KeyCode.P;
    public float captureInterval = 2.0f;
    
    [Header("Scenario Settings")]
    public int capturesPerLocation = 200;
    public float cesiumLoadDelay = 5.0f;
    public List<ScenarioLocation> scenarioList;

    // Internal State
    private bool isAutomationRunning = false;
    private bool isWaitingForTiles = false;
    private float timeSinceLastCapture = 0f;
    private float loadTimer = 0f;
    private int currentCaptureCount = 0;
    private int currentLocationIndex = 0;

    // We store the LOCAL position relative to the parent (or world origin if no parent)
    private Vector3 startLocalPosition;
    private Quaternion startRotation;

    void Start()
    {
        if (playerController != null)
        {
            // Save the exact spot where you placed the player
            startLocalPosition = playerController.transform.position;
            startRotation = playerController.transform.rotation;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleAutomationKey))
        {
            if(!isAutomationRunning) StartAutomation();
            else StopAutomation();
        }

        if (!isAutomationRunning) return;

        // 1. WAIT FOR TILES TO LOAD
        if (isWaitingForTiles)
        {
            loadTimer += Time.deltaTime;
            
            // While waiting, FORCE player to stay still at the start position
            if (playerController != null)
            {
                playerController.ForceReset(startLocalPosition, startRotation);
                // Ensure flying is ON so we don't fall
                playerController.SetFlyingState(true); 
            }

            if (loadTimer >= cesiumLoadDelay)
            {
                isWaitingForTiles = false;
                Debug.Log("[Automation] Tiles loaded. Starting Flight.");
            }
            return; 
        }

        // 2. MOVE FORWARD (FLY)
        UpdateStraightLineMovement();

        // 3. CAPTURE LOGIC
        timeSinceLastCapture += Time.deltaTime;
        
        // Update Weather
        if (weatherAutomation != null && capturesPerLocation > 0)
        {
            float progress = (float)currentCaptureCount / (float)capturesPerLocation;
            weatherAutomation.SetWeatherProgress(progress);
        }

        // Take Photo
        if (timeSinceLastCapture >= captureInterval)
        {
            yoloAnnotator.CaptureAndAnnotate();
            timeSinceLastCapture = 0f;
            currentCaptureCount++;

            if(currentCaptureCount % 10 == 0) 
                Debug.Log($"[Automation] Capture {currentCaptureCount}/{capturesPerLocation}");

            if (currentCaptureCount >= capturesPerLocation)
            {
                MoveToNextLocation();
            }
        }
    }

    void StartAutomation()
    {
        if (!playerController || !yoloAnnotator) return;
        
        isAutomationRunning = true;
        playerController.useExternalInput = true;
        
        // 1. DISABLE MOUSE LOOK
        if (playerCamScript != null)
        {
            playerCamScript.ResetCameraRotation(startRotation); 
            playerCamScript.enabled = false;
        }

        // 2. FORCE FLYING MODE (Fixes "Only Walking" bug)
        playerController.SetFlyingState(true);

        // 3. TELEPORT TO FIRST LOCATION
        currentCaptureCount = 0;
        if (scenarioList.Count > 0)
        {
            currentLocationIndex = 0; 
            TeleportToLocation(scenarioList[0]);
        }
        else
        {
            Debug.LogWarning("[Automation] No scenarios! Resetting to start.");
            ResetPlayerState();
            isWaitingForTiles = true; // Wait a bit anyway to let physics settle
            loadTimer = 0f;
        }
        
        Debug.Log("Automation Started.");
    }

    void MoveToNextLocation()
    {
        currentLocationIndex++;

        if (currentLocationIndex >= scenarioList.Count)
        {
            Debug.Log("[Automation] Scenario Complete! Stopping.");
            StopAutomation();
            return;
        }

        TeleportToLocation(scenarioList[currentLocationIndex]);
    }

    void TeleportToLocation(ScenarioLocation loc)
    {
        Debug.Log($"[Automation] Moving to {loc.name} -> Lat: {loc.latitude}, Lon: {loc.longitude}");

        // 1. Move World (Cesium)
        if (cesiumTeleporter != null)
        {
            cesiumTeleporter.JumpToLocation(loc.latitude, loc.longitude, loc.height);
        }

        // 2. Reset Player Position
        // IMPORTANT: We reset to the SAVED position relative to the new world center.
        ResetPlayerState();

        // 3. Reset Spawner & Weather
        if (objectSpawner != null) objectSpawner.ResetSpawner();
        if (weatherAutomation != null) weatherAutomation.GenerateRandomWeatherScenario();

        // 4. Reset Timers
        currentCaptureCount = 0;
        isWaitingForTiles = true;
        loadTimer = 0f;
    }

    void ResetPlayerState()
    {
        if (playerController != null)
        {
            // Snap to start position
            playerController.ForceReset(startLocalPosition, startRotation);
            // Ensure gravity is OFF and flying is ON
            playerController.SetFlyingState(true);
        }

        // Reset Camera Angle
        if (playerCamScript != null)
        {
            playerCamScript.ResetCameraRotation(startRotation);
        }
    }

    void UpdateStraightLineMovement()
    {
        if (!playerController) return;
        
        // Constant forward input
        playerController.externalVerticalInput = 1f; 
        playerController.externalHorizontalInput = 0f; 
        
        // Redundancy: Ensure flying mode stays ON every frame
        if (!playerController.isFlyingMode)
        {
            playerController.SetFlyingState(true);
        }
    }

    void StopAutomation()
    {
        isAutomationRunning = false;
        
        if (playerController) 
        {
            playerController.useExternalInput = false;
            playerController.externalVerticalInput = 0f;
            playerController.SetFlyingState(false); // Turn gravity back on when done?
        }

        if (playerCamScript != null)
        {
            playerCamScript.enabled = true;
        }
        
        Debug.Log("Automation Stopped.");
    }
}