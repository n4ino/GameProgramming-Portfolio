using UnityEngine;

/*
* Checkpoint
*
* Represents an individual checkpoint in the level.
* Registers itself with the CheckpointManager and
* activates when the player enters its trigger area.
*/

public class Checkpoint : MonoBehaviour
{
    [SerializeField] private string checkpointId;

    public string CheckpointId => checkpointId;

    private void Awake()
    {
        if (CheckpointManager.Instance != null)
        {
            CheckpointManager.Instance.RegisterCheckpoint(this);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Support both direct colliders and colliders attached to a Rigidbody2D.
        var player = other.attachedRigidbody ?
        other.attachedRigidbody.gameObject :
        other.gameObject;

        if (!player.CompareTag("Player"))
            return;

        // Update the current respawn checkpoint.
        CheckpointManager.Instance.SetLastCheckpoint(this);
        Debug.Log($"Checkpoint {name} activated.");
    }
}
