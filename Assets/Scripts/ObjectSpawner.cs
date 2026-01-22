using UnityEngine;
using System.Collections.Generic;

public class ObjectSpawner : MonoBehaviour
{
    [Header("References")]
    public Transform playerTransform;
    [Tooltip("The direction the player is actually moving.")]
    public Transform orientationTransform; 
    public List<GameObject> objectPrefabs;

    [Header("Spawn Settings")]
    public float spawnDistanceForward = 40f; 
    public float spawnWidth = 20f;           
    public float destroyDistanceBehind = 10f; 

    [Header("Population Control")]
    public int minObjects = 3;
    public int maxObjects = 10;
    public float countChangeInterval = 5f; 

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private int currentTargetCount;
    private float timer;

    void Start()
    {
        if (playerTransform == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) playerTransform = p.transform;
        }

        if (orientationTransform == null && playerTransform != null)
        {
            var mov = playerTransform.GetComponent<Movement>();
            if (mov != null && mov.orientation != null) orientationTransform = mov.orientation;
            else orientationTransform = playerTransform;
        }

        currentTargetCount = (minObjects + maxObjects) / 2;
    }

    // --- NEW METHOD ---
    // Called by the Automation Controller when switching cities
    public void ResetSpawner()
    {
        // Destroy all currently tracked objects
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            if (spawnedObjects[i] != null) Destroy(spawnedObjects[i]);
        }
        spawnedObjects.Clear();
        
        // Reset population target
        currentTargetCount = minObjects; 
    }

    void Update()
    {
        if (playerTransform == null) return;

        Vector3 forwardDir = (orientationTransform != null) ? orientationTransform.forward : playerTransform.forward;

        // 1. Vary the target count
        timer += Time.deltaTime;
        if (timer > countChangeInterval)
        {
            currentTargetCount = Random.Range(minObjects, maxObjects + 1);
            timer = 0f;
        }

        // 2. Clean up old objects
        for (int i = spawnedObjects.Count - 1; i >= 0; i--)
        {
            GameObject obj = spawnedObjects[i];
            
            if (obj == null) 
            {
                spawnedObjects.RemoveAt(i);
                continue;
            }

            Vector3 toObj = obj.transform.position - playerTransform.position;
            float distAlongPath = Vector3.Dot(toObj, forwardDir);

            if (distAlongPath < -destroyDistanceBehind)
            {
                spawnedObjects.RemoveAt(i);
                Destroy(obj);
            }
        }

        // 3. Spawn new objects
        if (spawnedObjects.Count < currentTargetCount)
        {
            SpawnObjectAhead();
        }
    }

    void SpawnObjectAhead()
    {
        if (objectPrefabs == null || objectPrefabs.Count == 0) return;
        if (playerTransform == null) return;

        Vector3 forwardDir = (orientationTransform != null) ? orientationTransform.forward : playerTransform.forward;
        Vector3 rightDir = (orientationTransform != null) ? orientationTransform.right : playerTransform.right;

        Vector3 centerPos = playerTransform.position + (forwardDir * spawnDistanceForward);
        
        float randomX = Random.Range(-spawnWidth / 2f, spawnWidth / 2f);
        Vector3 spawnPos = centerPos + (rightDir * randomX);
        spawnPos.y = playerTransform.position.y; 

        GameObject prefab = objectPrefabs[Random.Range(0, objectPrefabs.Count)];
        GameObject newObj = Instantiate(prefab, spawnPos, Quaternion.identity);
        
        spawnedObjects.Add(newObj);
    }
}