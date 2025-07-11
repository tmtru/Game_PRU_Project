using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SpawnableObject
{
    public GameObject prefab;
    [Range(0f, 1f)] public float spawnChance = 1f;

    [Header("Max Count Settings")]
    public bool useStaticMaxCount = false;
    [SerializeField] private int staticMaxCount = -1; // -1 = unlimited

    [Header("Day-based Max Count")]
    public bool useDayBasedMaxCount = true;
    public int baseMaxCount = 5; // Số lượng cơ bản ở ngày 0
    public float maxCountMultiplier = 1.2f; // Tăng 20% mỗi ngày
    public int absoluteMaxCount = 100; // Giới hạn tối đa

    [HideInInspector] public int currentCount = 0;

    [Header("Day Scaling")]
    public bool scaleWithDays = true;
    public int minDayToSpawn = 0; // Ngày tối thiểu để spawn object này
    public float dayScalingMultiplier = 2f; // Tăng 10% mỗi ngày

    // Getter cho maxCount dựa trên cài đặt
    public int GetMaxCount(int currentDay)
    {
        if (useStaticMaxCount)
        {
            return staticMaxCount;
        }
        else if (useDayBasedMaxCount)
        {
            if (baseMaxCount == -1) return -1; // Unlimited

            int dayBasedMax = Mathf.RoundToInt(baseMaxCount * Mathf.Pow(maxCountMultiplier, currentDay));
            return Mathf.Min(dayBasedMax, absoluteMaxCount);
        }
        else
        {
            return -1; // Unlimited
        }
    }

    // Getter cho static maxCount (để hiển thị trong Inspector)
    public int maxCount
    {
        get { return staticMaxCount; }
        set { staticMaxCount = value; }
    }
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

[System.Serializable]
public class DayScalingSettings
{
    [Header("Day-based Difficulty")]
    public bool enableDayScaling = true;
    public AnimationCurve difficultyMultiplierCurve = AnimationCurve.Linear(0, 1, 10, 2);

    [Header("Spawn Count Scaling")]
    public bool scaleSpawnCount = true;
    public int baseSpawnCount = 20;
    public int maxSpawnCount = 100;
    public float spawnCountMultiplier = 1.2f; // Tăng 20% mỗi ngày

    [Header("Spawn Speed Scaling")]
    public bool scaleSpawnSpeed = true;
    public float baseSpawnDelay = 0.5f;
    public float minSpawnDelay = 0.1f;
    public float speedMultiplier = 0.9f; // Giảm delay 10% mỗi ngày (spawn nhanh hơn)

    [Header("Enemy Strength Scaling")]
    public bool scaleEnemyStrength = true;
    public float baseHealthMultiplier = 1f;
    public float baseDamageMultiplier = 1f;
    public float strengthIncreasePerDay = 0.15f; // Tăng 15% mỗi ngày
}

public class RandomSpawner : MonoBehaviour
{
    [Header("Spawn Objects")]
    [SerializeField] private SpawnableObject[] spawnableObjects;

    [Header("Spawn Zones")]
    [SerializeField] private SpawnZone[] spawnZones;
    [SerializeField] private bool useLocalZones = true;

    [Header("Base Spawn Settings")]
    [SerializeField] private int totalObjectsToSpawn = 20;
    [SerializeField] private float spawnDelay = 0.1f;
    [SerializeField] private bool spawnOnAwake = true;

    [Header("Day Scaling System")]
    [SerializeField] private DayScalingSettings dayScaling;
    [SerializeField] private bool autoFindDayCounter = true;
    [SerializeField] private ChangeScene dayCounterReference; // Reference đến script ChangeScene

    [Header("Current Day Info (Runtime)")]
    [SerializeField] private int currentDay = 0;
    [SerializeField] private int actualSpawnCount = 0;
    [SerializeField] private float actualSpawnDelay = 0.1f;
    [SerializeField] private float difficultyMultiplier = 1f;

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
    public UnityEvent<int> OnDayChanged; // Event khi ngày thay đổi

    [Header("Debug")]
    [SerializeField] private bool showDebugInfo = true;
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private bool showMaxCountInfo = true; // Hiển thị thông tin maxCount

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private Queue<GameObject> objectPool = new Queue<GameObject>();
    private Coroutine currentSpawnCoroutine;
    private int totalSpawned = 0;
    private int previousDay = -1;

    private void Awake()
    {
        if (spawnParent == null)
            spawnParent = transform;

        // Tự động tìm ChangeScene component nếu cần
        if (autoFindDayCounter && dayCounterReference == null)
        {
            dayCounterReference = FindObjectOfType<ChangeScene>();
        }

        InitializeObjectPool();
    }

    private void Start()
    {
        UpdateDayScaling();

        if (spawnOnAwake)
            StartSpawning();
    }

    private void Update()
    {
        // Cập nhật day scaling nếu ngày thay đổi
        if (dayCounterReference != null)
        {
            int newDay = dayCounterReference.DayCount;
            if (newDay != previousDay)
            {
                previousDay = newDay;
                currentDay = newDay;
                UpdateDayScaling();
                OnDayChanged?.Invoke(currentDay);
                LogDebug($"Day changed to {currentDay}. Difficulty updated.");
                LogMaxCountInfo(); // Log thông tin maxCount khi ngày thay đổi
            }
        }
    }

    private void UpdateDayScaling()
    {
        if (!dayScaling.enableDayScaling)
        {
            actualSpawnCount = totalObjectsToSpawn;
            actualSpawnDelay = spawnDelay;
            difficultyMultiplier = 1f;
            return;
        }

        // Tính toán difficulty multiplier từ curve
        difficultyMultiplier = dayScaling.difficultyMultiplierCurve.Evaluate(currentDay);

        // Cập nhật số lượng spawn
        if (dayScaling.scaleSpawnCount)
        {
            actualSpawnCount = Mathf.RoundToInt(dayScaling.baseSpawnCount *
                Mathf.Pow(dayScaling.spawnCountMultiplier, currentDay));
            actualSpawnCount = Mathf.Min(actualSpawnCount, dayScaling.maxSpawnCount);
        }
        else
        {
            actualSpawnCount = totalObjectsToSpawn;
        }

        // Cập nhật tốc độ spawn
        if (dayScaling.scaleSpawnSpeed)
        {
            actualSpawnDelay = dayScaling.baseSpawnDelay *
                Mathf.Pow(dayScaling.speedMultiplier, currentDay);
            actualSpawnDelay = Mathf.Max(actualSpawnDelay, dayScaling.minSpawnDelay);
        }
        else
        {
            actualSpawnDelay = spawnDelay;
        }

        LogDebug($"Day {currentDay}: Spawn Count = {actualSpawnCount}, Spawn Delay = {actualSpawnDelay:F2}s, Difficulty = {difficultyMultiplier:F2}x");
    }

    // Hàm mới để log thông tin maxCount
    private void LogMaxCountInfo()
    {
        if (!showMaxCountInfo) return;

        LogDebug("=== Max Count Info for Day " + currentDay + " ===");
        foreach (SpawnableObject obj in spawnableObjects)
        {
            if (obj.prefab != null)
            {
                int maxCount = obj.GetMaxCount(currentDay);
                string maxCountStr = maxCount == -1 ? "Unlimited" : maxCount.ToString();
                LogDebug($"{obj.prefab.name}: MaxCount = {maxCountStr}, Current = {obj.currentCount}");
            }
        }
    }

    public void StartSpawning()
    {
        if (currentSpawnCoroutine != null)
        {
            StopCoroutine(currentSpawnCoroutine);
        }

        UpdateDayScaling(); // Cập nhật lại scaling trước khi spawn
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

    // Thêm hàm để manual set ngày (để test)
    public void SetDay(int day)
    {
        currentDay = day;
        UpdateDayScaling();
        LogDebug($"Manually set day to {currentDay}");
        LogMaxCountInfo();
    }

    // Thêm hàm để lấy current day từ dayCounterReference
    public int GetCurrentDay()
    {
        if (dayCounterReference != null)
        {
            return dayCounterReference.DayCount;
        }
        return currentDay;
    }

    private IEnumerator SpawnAllObjects()
    {
        totalSpawned = 0;

        for (int i = 0; i < actualSpawnCount; i++)
        {
            if (SpawnRandomObject())
            {
                totalSpawned++;
                yield return new WaitForSeconds(actualSpawnDelay);
            }
        }

        OnSpawnCompleted?.Invoke();
        LogDebug($"Spawning completed. Total spawned: {totalSpawned}");
    }

    private IEnumerator SpawnInWaves()
    {
        totalSpawned = 0;
        int remainingObjects = actualSpawnCount;

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
                    yield return new WaitForSeconds(actualSpawnDelay);
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

        GameObject spawnedObj = CreateObject(objectToSpawn, spawnPosition);
        if (spawnedObj != null)
        {
            spawnedObjects.Add(spawnedObj);
            objectToSpawn.currentCount++;

            OnObjectSpawned?.Invoke(spawnedObj);
            LogDebug($"Spawned {objectToSpawn.prefab.name} at {spawnPosition} (Day {currentDay}, Count: {objectToSpawn.currentCount}/{objectToSpawn.GetMaxCount(currentDay)})");
            return true;
        }

        return false;
    }

    private SpawnableObject SelectRandomSpawnableObject()
    {
        List<SpawnableObject> availableObjects = new List<SpawnableObject>();
        int currentDayForCheck = GetCurrentDay(); // Lấy ngày hiện tại từ dayCounterReference

        foreach (SpawnableObject obj in spawnableObjects)
        {
            if (obj.prefab != null &&
                currentDayForCheck >= obj.minDayToSpawn && // Kiểm tra ngày tối thiểu
                Random.value <= obj.spawnChance)
            {
                int maxCount = obj.GetMaxCount(currentDayForCheck);
                // Kiểm tra maxCount dựa trên ngày hiện tại
                if (maxCount == -1 || obj.currentCount < maxCount)
                {
                    availableObjects.Add(obj);
                }
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

    private GameObject CreateObject(SpawnableObject spawnableObj, Vector3 position)
    {
        GameObject obj;

        if (useObjectPooling)
        {
            obj = GetFromPool(spawnableObj.prefab);
            obj.transform.position = position;
            obj.SetActive(true);
        }
        else
        {
            obj = Instantiate(spawnableObj.prefab, position, Quaternion.identity, spawnParent);
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
                for (int i = 0; i < 10; i++)
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