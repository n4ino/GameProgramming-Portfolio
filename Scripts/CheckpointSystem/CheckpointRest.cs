using Unity.VisualScripting;
using UnityEngine;

/*
* CheckpointRest
*
* Handles checkpoint resting functionality.
* Updates the active checkpoint, restores player health,
* respawns enemies, and saves the game state.
*/

public class CheckpointRest : MonoBehaviour
{
    public void Rest(PlayerController player)
    {
        // Set this checkpoint as the current respawn location.
        Checkpoint cp = GetComponent<Checkpoint>();
        if (cp != null && CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.SetLastCheckpoint(cp);
        }

        var hp = player.GetComponent<PlayerHealth>();
        if (hp != null)
        {
            hp.ResetHealth();

            // Restore all enemies when the player rests at a checkpoint.
            if (EnemyRespawnManager.Instance != null)
            {
                EnemyRespawnManager.Instance.RespawnAll();
            }
            else
            {
                Debug.LogWarning("[CheckpointRest] EnemyRespawnManager instance not found in scene.");
            }
        }

        // Save the current game state after resting.
        if (DataPersistenceManager.Instance != null)
        {
            DataPersistenceManager.Instance.SaveGame();
        }
        else
        {
            Debug.LogWarning("[CheckpointRest] DataPersistenceManager instance not found in scene.");
        }
    }
}
