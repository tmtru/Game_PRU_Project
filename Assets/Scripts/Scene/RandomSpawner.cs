using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SpawnableObject
{
    public GameObject prefab;
    [Range(0f, 1f)] public float spawnChance = 1f;
    public int maxCount = -1; // -1 = unlimited
    [HideInInspector] public int currentCount = 0;
}

[System.Serializable]
public class SpawnZone
{
    public string zoneName = "Zone";
    public Vector3 center = Vector3.zero;
    public Vector3 size = Vector3.one * 10f;
    public bool isActive = true;
    public Color gizmoColor = Color.green;
}

public class RandomSpawner : MonoBehaviour
{
    [Header("Spawn Objects")]
    [SerializeField] private SpawnableObject[] spawnableObjects;

    [Header("Spawn Zones")]
    [SerializeField] private SpawnZone[] spawnZones;
    [SerializeField] private bool useLocalZones = true; // Zones relative to spawner position

    [Header("Spawn Settings")]
    [SerializeField] private int totalObjectsToSpawn = 20;
    [SerializeField] private float spawnDelay = 0.1f;
    [SerializeField] private bool spawnOnAwake = true;

    [Header("Positioning")]
    [SerializeField] private bool useRaycastForHeight = true;
    [SerializeField] private LayerMask groundLayerMask = -1;
    [SerializeField] private float heightOffset = 0.5f;
    [SerializeField] private float raycastDistance = 100f;

    [Header("Collision Detection")]
    [SerializeField] private bool checkCollisions = true;
    [SerializeField] private float collisionCheckRadius = 1f;
    [SerializeField] private LayerMask collisionLayerMask = -1;

    [Header("Randomization")]
    [SerializeField] private bool randomizeRotation = true;
    [SerializeField] private bool randomizeScale = false;
    [SerializeField] private Vector2 scaleRange = new Vector2(0.8f, 1.2f);
    [SerializeField] private bool uniformScale = true;

    [Header("Wave Spawning")]
    [SerializeField] private bool enableWaveSpawning = false;
    [SerializeField] private int objectsPerWave = 5;
    [SerializeField] private float timeBetweenWaves = 2f;

    [Header("Performance")]
    [SerializeField] private int maxAttemptsPerObject = 50;
    [SerializeField] private bool useObjectPooling = false;
    [SerializeField] private Transform spawnParent;

    [Header("Events")]
    public UnityEvent OnSpawnStarted;
    public UnityEvent OnSpawnCompleted;
    public UnityEvent<GameObject> OnObjectSpawned;

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showGizmos = true;

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private Queue<GameObject> objectPool = new Queue<GameObject>();
    private Coroutine currentSpawnCoroutine;
    private int totalSpawned = 0;

    private void Awake()
    {
        if (spawnParent == null)
            spawnParent = transform;

        InitializeObjectPool();
    }

    private void Start()
    {
        if (spawnOnAwake)
            StartSpawning();
    }

    public void StartSpawning()
    {
        if (currentSpawnCoroutine != null)
        {
            StopCoroutine(currentSpawnCoroutine);
        }

        ResetSpawnCounts();
        OnSpawnStarted?.Invoke();

        if (enableWaveSpawning)
        {
            currentSpawnCoroutine = StartCoroutine(SpawnInWaves());
        }
        else
        {
            currentSpawnCoroutine = StartCoroutine(SpawnAllObjects());
        }
    }

    public void StopSpawning()
    {
        if (currentSpawnCoroutine != null)
        {
            StopCoroutine(currentSpawnCoroutine);
            currentSpawnCoroutine = null;
        }
    }

    public void ClearAllSpawned()
    {
        foreach (GameObject obj in spawnedObjects)
        {
            if (obj != null)
            {
                if (useObjectPooling)
                {
                    ReturnToPool(obj);
                }
                else
                {
                    if (Application.isPlaying)
                        Destroy(obj);
                    else
                        DestroyImmediate(obj);
                }
            }
        }

        spawnedObjects.Clear();
        totalSpawned = 0;
        ResetSpawnCounts();
    }

    private IEnumerator SpawnAllObjects()
    {
        totalSpawned = 0;

        for (int i = 0; i < totalObjectsToSpawn; i++)
        {
            if (SpawnRandomObject())
            {
                totalSpawned++;
                yield return new WaitForSeconds(spawnDelay);
            }
        }

        OnSpawnCompleted?.Invoke();
        LogDebug($"Spawning completed. Total spawned: {totalSpawned}");
    }

    private IEnumerator SpawnInWaves()
    {
        totalSpawned = 0;
        int remainingObjects = totalObjectsToSpawn;

        while (remainingObjects > 0)
        {
            int objectsThisWave = Mathf.Min(objectsPerWave, remainingObjects);
            LogDebug($"Starting wave: {objectsThisWave} objects");

            for (int i = 0; i < objectsThisWave; i++)
            {
                if (SpawnRandomObject())
                {
                    totalSpawned++;
                    remainingObjects--;
                    yield return new WaitForSeconds(spawnDelay);
                }
            }

            if (remainingObjects > 0)
            {
                yield return new WaitForSeconds(timeBetweenWaves);
            }
        }

        OnSpawnCompleted?.Invoke();
        LogDebug($"Wave spawning completed. Total spawned: {totalSpawned}");
    }

    private bool SpawnRandomObject()
    {
        SpawnableObject objectToSpawn = SelectRandomSpawnableObject();
        if (objectToSpawn == null || objectToSpawn.prefab == null)
        {
            return false;
        }

        Vector3 spawnPosition = GetValidSpawnPosition();
        if (spawnPosition == Vector3.zero)
        {
            LogDebug("Failed to find valid spawn position");
            return false;
        }

        GameObject spawnedObj = CreateObject(objectToSpawn.prefab, spawnPosition);
        if (spawnedObj != null)
        {
            spawnedObjects.Add(spawnedObj);
            objectToSpawn.currentCount++;
            OnObjectSpawned?.Invoke(spawnedObj);
            LogDebug($"Spawned {objectToSpawn.prefab.name} at {spawnPosition}");
            return true;
        }

        return false;
    }

    private SpawnableObject SelectRandomSpawnableObject()
    {
        List<SpawnableObject> availableObjects = new List<SpawnableObject>();

        foreach (SpawnableObject obj in spawnableObjects)
        {
            if (obj.prefab != null &&
                (obj.maxCount == -1 || obj.currentCount < obj.maxCount) &&
                Random.value <= obj.spawnChance)
            {
                availableObjects.Add(obj);
            }
        }

        if (availableObjects.Count == 0)
            return null;

        return availableObjects[Random.Range(0, availableObjects.Count)];
    }

    private Vector3 GetValidSpawnPosition()
    {
        for (int attempt = 0; attempt < maxAttemptsPerObject; attempt++)
        {
            SpawnZone selectedZone = SelectRandomActiveZone();
            if (selectedZone == null) continue;

            Vector3 randomPos = GetRandomPositionInZone(selectedZone);

            if (useRaycastForHeight)
            {
                randomPos = AdjustHeightWithRaycast(randomPos);
                if (randomPos == Vector3.zero) continue;
            }

            if (checkCollisions && IsPositionBlocked(randomPos))
            {
                continue;
            }

            return randomPos;
        }

        return Vector3.zero;
    }

    private SpawnZone SelectRandomActiveZone()
    {
        List<SpawnZone> activeZones = new List<SpawnZone>();

        foreach (SpawnZone zone in spawnZones)
        {
            if (zone.isActive)
                activeZones.Add(zone);
        }

        if (activeZones.Count == 0)
            return null;

        return activeZones[Random.Range(0, activeZones.Count)];
    }

    private Vector3 GetRandomPositionInZone(SpawnZone zone)
    {
        Vector3 zoneCenter = useLocalZones ? transform.position + zone.center : zone.center;

        Vector3 randomOffset = new Vector3(
            Random.Range(-zone.size.x / 2f, zone.size.x / 2f),
            Random.Range(-zone.size.y / 2f, zone.size.y / 2f),
            Random.Range(-zone.size.z / 2f, zone.size.z / 2f)
        );

        return zoneCenter + randomOffset;
    }

    private Vector3 AdjustHeightWithRaycast(Vector3 position)
    {
        Vector3 rayStart = new Vector3(position.x, position.y + raycastDistance, position.z);

        if (Physics.Raycast(rayStart, Vector3.down, out RaycastHit hit, raycastDistance * 2f, groundLayerMask))
        {
            return hit.point + Vector3.up * heightOffset;
        }

        return Vector3.zero;
    }

    private bool IsPositionBlocked(Vector3 position)
    {
        return Physics.CheckSphere(position, collisionCheckRadius, collisionLayerMask);
    }

    private GameObject CreateObject(GameObject prefab, Vector3 position)
    {
        GameObject obj;

        if (useObjectPooling)
        {
            obj = GetFromPool(prefab);
            obj.transform.position = position;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(prefab, position, Quaternion.identity, spawnParent);
        }

        if (randomizeRotation)
        {
            obj.transform.rotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        }

        if (randomizeScale)
        {
            ApplyRandomScale(obj);
        }

        return obj;
    }

    private void ApplyRandomScale(GameObject obj)
    {
        if (uniformScale)
        {
            float scale = Random.Range(scaleRange.x, scaleRange.y);
            obj.transform.localScale = Vector3.one * scale;
        }
        else
        {
            Vector3 scale = new Vector3(
                Random.Range(scaleRange.x, scaleRange.y),
                Random.Range(scaleRange.x, scaleRange.y),
                Random.Range(scaleRange.x, scaleRange.y)
            );
            obj.transform.localScale = scale;
        }
    }

    private void InitializeObjectPool()
    {
        if (!useObjectPooling) return;

        foreach (SpawnableObject spawnableObj in spawnableObjects)
        {
            if (spawnableObj.prefab != null)
            {
                for (int i = 0; i < 10; i++) // Pre-create 10 of each
                {
                    GameObject pooledObj = Instantiate(spawnableObj.prefab, spawnParent);
                    pooledObj.SetActive(false);
                    objectPool.Enqueue(pooledObj);
                }
            }
        }
    }

    private GameObject GetFromPool(GameObject prefab)
    {
        if (objectPool.Count > 0)
        {
            GameObject pooledObj = objectPool.Dequeue();
            return pooledObj;
        }
        else
        {
            return Instantiate(prefab, spawnParent);
        }
    }

    private void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        objectPool.Enqueue(obj);
    }

    private void ResetSpawnCounts()
    {
        foreach (SpawnableObject obj in spawnableObjects)
        {
            obj.currentCount = 0;
        }
    }

    private void LogDebug(string message)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[AdvancedSpawner] {message}");
        }
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;

        foreach (SpawnZone zone in spawnZones)
        {
            if (!zone.isActive) continue;

            Gizmos.color = zone.gizmoColor;
            Vector3 center = useLocalZones ? transform.position + zone.center : zone.center;
            Gizmos.DrawWireCube(center, zone.size);
        }

        // Draw collision check spheres for spawned objects
        if (checkCollisions && Application.isPlaying)
        {
            Gizmos.color = Color.red;
            foreach (GameObject obj in spawnedObjects)
            {
                if (obj != null)
                {
                    Gizmos.DrawWireSphere(obj.transform.position, collisionCheckRadius);
                }
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos) return;

        foreach (SpawnZone zone in spawnZones)
        {
            Gizmos.color = zone.isActive ? Color.green : Color.gray;
            Vector3 center = useLocalZones ? transform.position + zone.center : zone.center;
            Gizmos.DrawCube(center, zone.size);
        }
    }
}