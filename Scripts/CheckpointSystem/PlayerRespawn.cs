using System.Collections;
using UnityEngine;

/*
* PlayerRespawn
*
* Handles player respawning after death.
* Restores the player at the latest checkpoint,
* resets health, and supports save/load integration.
*/

public class PlayerRespawn : MonoBehaviour, IDataPersistence
{
    public static PlayerRespawn Instance { get; private set; }
    public Vector3 defaultSpawnPoint;
    public PlayerHealth playerHealth;
    [SerializeField] private float deathRespawnDelay = 1f;

    private Rigidbody2D rb;
    private Coroutine respawnRoutine;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnDied += QueueRespawn;
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnDied -= QueueRespawn;

        if (respawnRoutine != null)
        {
            StopCoroutine(respawnRoutine);
            respawnRoutine = null;
        }
    }

    public void SetRespawnPoint(Vector3 newPoint)
    {
        defaultSpawnPoint = newPoint;
    }

    private void QueueRespawn()
    {
        if (respawnRoutine != null)
            StopCoroutine(respawnRoutine);

        // Start delayed respawn after the player dies.
        respawnRoutine = StartCoroutine(RespawnAfterDelay());
    }

    private IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(deathRespawnDelay);
        respawnRoutine = null;
        RespawnNow();
    }

    public void RespawnNow()
    {
        if (respawnRoutine != null)
        {
            StopCoroutine(respawnRoutine);
            respawnRoutine = null;
        }

        // Use the most recently activated checkpoint if one exists.
        Vector3 targetPoint = defaultSpawnPoint;
        if (CheckpointManager.Instance != null)
            targetPoint = CheckpointManager.Instance.GetLastCheckpointPosition(defaultSpawnPoint);

        transform.position = targetPoint;
        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        if (playerHealth != null)
            playerHealth.ResetHealth();
    }

    public void LoadData(GameData data)
    {
        // CheckpointManager triggers respawn after checkpoint data has been loaded.
    }

    public void SaveData(ref GameData data)
    {
    }
}
