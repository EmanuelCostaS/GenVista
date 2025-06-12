using UnityEngine;
using System.Collections;

public class AutomatedCaptureController : MonoBehaviour
{
    [Header("Target References")]
    // Note: Ensure this class name matches your movement script. We've used 'Player' previously.
    public Movement playerController; 
    public YoloAnnotator yoloAnnotator;

    [Header("Automation Settings")]
    public KeyCode toggleAutomationKey = KeyCode.P;
    public float totalAutomationDuration = 120f;
    public float captureInterval = 2.0f;
    public float movementChangeInterval = 3.0f;

    [Header("Movement Behavior")]
    [Range(0f, 1f)] public float chanceToMoveForward = 0.7f;
    [Range(0f, 1f)] public float chanceToMoveSideways = 0.5f;
    [Range(0f, 1f)] public float chanceToAttemptJumpOrFlyUp = 0.1f;
    [Range(0f, 1f)] public float chanceToAttemptDescend = 0.05f;

    private float timeSinceLastCapture = 0f;
    private float timeSinceLastMovementChange = 0f;
    private float currentSessionElapsedTime = 0f;
    private bool isAutomationRunning = false;

    void Start()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController not assigned to AutomatedCaptureController!", this);
            this.enabled = false;
            return;
        }
        if (yoloAnnotator == null)
        {
            Debug.LogError("YoloAnnotator not assigned to AutomatedCaptureController!", this);
            this.enabled = false;
            return;
        }

        // --------------------------------------------------------------------
        // ---- THIS ENTIRE BLOCK OF CODE HAS BEEN REMOVED ----
        //
        // The new YoloAnnotator automatically finds all objects with the
        // YoloObject component, so this logic is no longer needed.
        //
        // if (yoloAnnotator.targetObject == null) { ... }
        //
        // --------------------------------------------------------------------
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
            return;

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

        currentSessionElapsedTime = 0f;
        timeSinceLastCapture = captureInterval;
        timeSinceLastMovementChange = movementChangeInterval;
    }

    void UpdateAutomaticMovement()
    {
        if (!playerController) return;

        playerController.externalVerticalInput = (Random.value < chanceToMoveForward) ? Random.Range(0.5f, 1f) : 0f;
        if (Random.value < chanceToMoveSideways)
        {
            playerController.externalHorizontalInput = Random.Range(-1f, 1f);
        }
        else
        {
            playerController.externalHorizontalInput = 0f;
        }

        playerController.externalJumpKeyActive = false;
        playerController.externalDescendKeyActive = false;

        if (Random.value < chanceToAttemptJumpOrFlyUp)
        {
            if (playerController.grounded)
            {
                playerController.TriggerExternalJump();
            }
            else
            {
                playerController.externalJumpKeyActive = true;
            }
        }

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
            playerController.externalHorizontalInput = 0f;
            playerController.externalVerticalInput = 0f;
            playerController.externalJumpKeyActive = false;
            playerController.externalDescendKeyActive = false;
        }
    }

    void OnDisable()
    {
        if (isAutomationRunning)
        {
            Debug.Log("AutomatedCaptureController component disabled, stopping automation.");
            StopAutomation();
        }
    }
}