using UnityEngine;
using System.Collections;

public class AutomatedCaptureController : MonoBehaviour
{
    [Header("Target References")]
    public Movement playerController; // Assign your Player GameObject (that has Player.cs)
    public YoloAnnotator yoloAnnotator; // Assign your GameObject that has YoloAnnotator.cs

    [Header("Automation Settings")]
    public KeyCode toggleAutomationKey = KeyCode.P; // Key to toggle automation
    public float totalAutomationDuration = 120f; // How long the automation runs in seconds
    public float captureInterval = 2.0f;   // Time between captures in seconds
    public float movementChangeInterval = 3.0f; // How often to change movement direction/action

    [Header("Movement Behavior")]
    [Range(0f, 1f)] public float chanceToMoveForward = 0.7f;
    [Range(0f, 1f)] public float chanceToMoveSideways = 0.5f;
    [Range(0f, 1f)] public float chanceToAttemptJumpOrFlyUp = 0.1f; // Chance per movement change
    [Range(0f, 1f)] public float chanceToAttemptDescend = 0.05f; // Chance if flying

    private float timeSinceLastCapture = 0f;
    private float timeSinceLastMovementChange = 0f;
    private float currentSessionElapsedTime = 0f; // Tracks time for the current automation session
    private bool isAutomationRunning = false;

    void Start()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController not assigned to AutomatedCaptureController!", this);
            this.enabled = false; // Disable this script if critical references are missing
            return;
        }
        if (yoloAnnotator == null)
        {
            Debug.LogError("YoloAnnotator not assigned to AutomatedCaptureController!", this);
            this.enabled = false; // Disable this script if critical references are missing
            return;
        }

        // Initial setup for YoloAnnotator's targetObject (if not manually set)
        if (yoloAnnotator.targetObject == null)
        {
            Debug.LogWarning("YoloAnnotator's Target Object is not set. Attempting to set it to the PlayerController's GameObject.", this);
            yoloAnnotator.targetObject = playerController.gameObject;
        }
        else if (yoloAnnotator.targetObject != playerController.gameObject)
        {
            Debug.LogWarning("YoloAnnotator's Target Object is set, but it's not the PlayerController. Ensure this is intended for annotation.", this);
        }
        // Automation does not start automatically anymore.
    }

    void Update()
    {
        // Toggle Automation
        if (Input.GetKeyDown(toggleAutomationKey))
        {
            if (isAutomationRunning)
            {
                StopAutomation();
            }
            else
            {
                StartAutomation();
            }
        }

        if (!isAutomationRunning)
            return; // Do nothing further if automation is not active

        // --- Update session timer ---
        currentSessionElapsedTime += Time.deltaTime;
        if (currentSessionElapsedTime > totalAutomationDuration)
        {
            Debug.Log("Total automation duration reached.");
            StopAutomation();
            return;
        }

        // --- Handle Movement ---
        timeSinceLastMovementChange += Time.deltaTime;
        if (timeSinceLastMovementChange >= movementChangeInterval)
        {
            UpdateAutomaticMovement();
            timeSinceLastMovementChange = 0f;
        }

        // --- Handle Capture ---
        timeSinceLastCapture += Time.deltaTime;
        if (timeSinceLastCapture >= captureInterval)
        {
            if (playerController.gameObject.activeInHierarchy && yoloAnnotator.enabled)
            {
                // Debug.Log("Automated capture triggered."); // Can be noisy, enable if needed
                yoloAnnotator.CaptureAndAnnotate();
            }
            timeSinceLastCapture = 0f;
        }
    }

    void StartAutomation()
    {
        if (playerController == null || yoloAnnotator == null)
        {
            Debug.LogError("Cannot start automation: PlayerController or YoloAnnotator is missing.", this);
            return;
        }

        Debug.Log("Starting automated capture session. Press '" + toggleAutomationKey.ToString() + "' to stop.");
        isAutomationRunning = true;
        playerController.useExternalInput = true;

        // Reset timers for the new session
        currentSessionElapsedTime = 0f;
        timeSinceLastCapture = captureInterval; // Set to capture on the first relevant frame after a short delay or immediately
        timeSinceLastMovementChange = movementChangeInterval; // Force movement change at start
    }

    void UpdateAutomaticMovement()
    {
        if (!playerController) return;

        // Horizontal/Vertical Movement
        playerController.externalVerticalInput = (Random.value < chanceToMoveForward) ? Random.Range(0.5f, 1f) : 0f;
        if (Random.value < chanceToMoveSideways)
        {
            playerController.externalHorizontalInput = Random.Range(-1f, 1f);
        }
        else
        {
            playerController.externalHorizontalInput = 0f;
        }

        // Reset continuous actions for this frame before deciding
        playerController.externalJumpKeyActive = false;
        playerController.externalDescendKeyActive = false;

        // Jumping / Flying Up
        if (Random.value < chanceToAttemptJumpOrFlyUp)
        {
            if (playerController.grounded)
            {
                playerController.TriggerExternalJump(); // Request a single jump
            }
            else // In air
            {
                playerController.externalJumpKeyActive = true; // Hold "jump" to fly up
            }
        }

        // Descending (only if flying and not trying to ascend)
        if (playerController.isFlyingMode && !playerController.externalJumpKeyActive && Random.value < chanceToAttemptDescend)
        {
            playerController.externalDescendKeyActive = true;
        }
    }

    void StopAutomation()
    {
        Debug.Log("Automated capture session stopped. Press '" + toggleAutomationKey.ToString() + "' to start again.");
        isAutomationRunning = false;
        if (playerController != null)
        {
            playerController.useExternalInput = false;
            // Reset external inputs to ensure player stops automated actions
            playerController.externalHorizontalInput = 0f;
            playerController.externalVerticalInput = 0f;
            playerController.externalJumpKeyActive = false;
            playerController.externalDescendKeyActive = false;
            // playerController.externalJumpTriggered is handled internally by Player.cs
        }
        // Note: We don't disable this script (this.enabled = false;) anymore
        // so it can listen for the toggle key to restart.
    }

    void OnDisable()
    {
        // This is a failsafe: if this script component is disabled for any reason
        // (e.g., manually in editor, or by another script), ensure automation stops cleanly.
        if (isAutomationRunning)
        {
            Debug.Log("AutomatedCaptureController component disabled, stopping automation.");
            StopAutomation(); // This will also set isAutomationRunning to false.
        }
    }
}