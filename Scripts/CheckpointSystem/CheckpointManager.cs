using System.Collections.Generic;
using UnityEngine;

/*
* CheckpointManager
*
* Tracks registered checkpoints, manages the player's
* active checkpoint, and handles checkpoint save/load data.
*/

public class CheckpointManager : MonoBehaviour, IDataPersistence
{
    public static CheckpointManager Instance { get; private set; }

    private Checkpoint lastCheckpoint;

    // Stores checkpoints by ID so they can be restored during save/load.
    private Dictionary<string, Checkpoint> checkpoints = new Dictionary<string, Checkpoint>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

    }

    public void SetLastCheckpoint(Checkpoint cp)
    {
        lastCheckpoint = cp;
        Debug.Log($"[CheckpointManager] Last checkpoint set to {cp.name}");
    }

    public Vector3 GetLastCheckpointPosition(Vector3 fallbackSpawn)
    {
        if (lastCheckpoint != null)
            return lastCheckpoint.transform.position;

        // Use the default spawn position if no checkpoint has been activated.
        return fallbackSpawn;

    }

    public void RegisterCheckpoint(Checkpoint checkpoint)
    {
        if (checkpoint == null) return;

        if (string.IsNullOrWhiteSpace(checkpoint.CheckpointId))
        {
            Debug.LogWarning($"Checkpoint {checkpoint.name} is missing checkpointId!");
            return;
        }

        if (checkpoints.ContainsKey(checkpoint.CheckpointId))
        {
            Debug.LogWarning($"Duplicate checkpointId found: {checkpoint.CheckpointId}");
            return;
        }

        checkpoints.Add(checkpoint.CheckpointId, checkpoint);
    }

    public string GetCurrentCheckpointId()
    {
        return lastCheckpoint != null ? lastCheckpoint.CheckpointId : "";
    }

    public Checkpoint GetCheckpointById(string checkpointId)
    {
        if (string.IsNullOrWhiteSpace(checkpointId))
            return null;

        checkpoints.TryGetValue(checkpointId, out Checkpoint checkpoint);
        return checkpoint;
    }

    public void LoadData(GameData data)
    {
        if (data == null) return;
        string checkpointId = data.currentCheckpointID;
        if (!string.IsNullOrEmpty(checkpointId))
        {
            var checkpoint = GetCheckpointById(checkpointId);
            if (checkpoint != null)
            {
                lastCheckpoint = checkpoint;
                Debug.Log($"[CheckpointManager] Loaded last checkpoint: {checkpoint.name}");

                // Move the player to the restored checkpoint immediately after loading.
                if (PlayerRespawn.Instance != null)
                {
                    PlayerRespawn.Instance.RespawnNow();
                }
            }
            else
            {
                Debug.LogWarning($"[CheckpointManager] Checkpoint with ID '{checkpointId}' not found during load.");
            }
        }
    }

    public void SaveData(ref GameData data)
    {
        if (data == null) return;
        data.currentCheckpointID = GetCurrentCheckpointId();
    }

}
