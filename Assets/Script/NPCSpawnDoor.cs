using UnityEngine;

public class NPCSpawnDoor : MonoBehaviour
{
    [Header("Spawner Settings")]
    public GameObject prefabToSpawn;
    public Transform spawnPoint;

    [Header("Timing Settings")]
    public float minTime = 2.0f;
    public float maxTime = 5.0f;

    public int maxSpawnedNPC = 10;
    private int spawnedNPC = 0;

    private float _timer;

    void Start()
    {
        ResetTimer();
    }

    void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0f && spawnedNPC < maxSpawnedNPC)
        {
            SpawnPrefab();
            ResetTimer();
        }
    }

    void ResetTimer()
    {
        // Pick a new random time for the next spawn
        _timer = Random.Range(minTime, maxTime);
    }

    void SpawnPrefab()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogWarning("DoorSpawner: No Prefab assigned");
            return;
        }

        // Determine where to spawn
        Vector3 finalPosition = (spawnPoint != null) ? spawnPoint.position : transform.position;
        Quaternion finalRotation = (spawnPoint != null) ? spawnPoint.rotation : transform.rotation;

        // Instantiate npc
        Instantiate(prefabToSpawn, finalPosition, finalRotation);
        spawnedNPC += 1;
    }
}