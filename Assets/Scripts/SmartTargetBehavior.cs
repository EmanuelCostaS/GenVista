using UnityEngine;
using System.Collections;

public class SmartTargetBehavior : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;

    [Header("Positioning Settings")]
    [Tooltip("How much random forward/backward distance to add upon spawning.")]
    public float distanceVariance = 10f; // <--- The variance is now handled here

    [Header("Visibility (Blinking)")]
    public bool enableBlinking = true;
    public float minVisibleTime = 1f;
    public float maxVisibleTime = 3f;
    public float minInvisibleTime = 0.5f;
    public float maxInvisibleTime = 1.5f;

    [Header("Wandering (Local Movement)")]
    public bool enableWandering = true;
    public float wanderSpeed = 2f;
    public float wanderRadius = 4f;
    public float changeDirectionInterval = 2f;

    private Renderer[] renderers;
    private Vector3 initialOffsetFromPlayer;
    
    // Wandering variables
    private Vector3 targetWanderOffset;
    private Vector3 currentWanderOffset;
    private float wanderTimer;

    void Start()
    {
        renderers = GetComponentsInChildren<Renderer>();
        
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (playerTransform != null)
        {
            // 1. Calculate the base offset (where the Spawner put us initially)
            Vector3 baseOffset = transform.position - playerTransform.position;

            // 2. Add random variance along the Player's Forward direction
            // We generate a random number between -variance and +variance
            float randomZ = Random.Range(-distanceVariance, distanceVariance);
            
            // We project this variance onto the player's forward vector so it moves closer/further
            Vector3 varianceVector = playerTransform.forward * randomZ;

            // 3. Set the final anchored offset
            initialOffsetFromPlayer = baseOffset + varianceVector;
        }

        // Initialize Wandering
        targetWanderOffset = Vector3.zero;
        currentWanderOffset = Vector3.zero;

        if (enableBlinking)
        {
            StartCoroutine(BlinkRoutine());
        }
    }

    void LateUpdate()
    {
        if (playerTransform == null) return;

        // --- Handle Wandering ---
        if (enableWandering)
        {
            wanderTimer += Time.deltaTime;
            if (wanderTimer > changeDirectionInterval)
            {
                targetWanderOffset = Random.insideUnitSphere * wanderRadius;
                wanderTimer = 0f;
            }
            currentWanderOffset = Vector3.Lerp(currentWanderOffset, targetWanderOffset, Time.deltaTime * wanderSpeed);
        }

        // --- Apply Position ---
        // The object is locked to: Player + (Initial Position + Random Variance) + Wander Wiggle
        transform.position = playerTransform.position + initialOffsetFromPlayer + currentWanderOffset;
    }

    IEnumerator BlinkRoutine()
    {
        while (true)
        {
            SetVisibility(true);
            yield return new WaitForSeconds(Random.Range(minVisibleTime, maxVisibleTime));

            SetVisibility(false);
            yield return new WaitForSeconds(Random.Range(minInvisibleTime, maxInvisibleTime));
        }
    }

    void SetVisibility(bool isVisible)
    {
        foreach (var r in renderers) r.enabled = isVisible;
    }
}