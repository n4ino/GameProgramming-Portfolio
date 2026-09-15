using UnityEngine;
using UnityEngine.Video;

/*
* CutscenePickup
*
* Handles collectible items that trigger a cutscene.
* Can optionally reward currency and remove the pickup
* after the cutscene has finished playing.
*/

[RequireComponent(typeof(Collider2D))]
public class CutscenePickup : MonoBehaviour, IItem
{
    [Header("Cutscene")]
    [SerializeField] private VideoClip cutsceneClip;
    [SerializeField] private bool playOnlyOnce = true;

    [Header("Pickup")]
    [SerializeField] private int currencyAmount;
    [SerializeField] private bool destroyAfterCutscene = true;

    private bool collected;

    private void Reset()
    {
        Collider2D pickupCollider = GetComponent<Collider2D>();
        pickupCollider.isTrigger = true;
    }

    public void Collect()
    {
        // Prevent the pickup from being collected multiple times.
        if (collected && playOnlyOnce)
            return;

        collected = true;

        // Award currency before playing the cutscene.
        if (currencyAmount > 0)
        {
            if (PlayerCurrency.Instance != null)
                PlayerCurrency.Instance.Add(currencyAmount);
            else
                Debug.LogWarning("[CutscenePickup] PlayerCurrency.Instance is missing from the scene.", this);
        }

        if (cutsceneClip == null)
        {
            Debug.LogWarning("[CutscenePickup] Cutscene clip is missing.", this);
            FinishPickup();
            return;
        }

        if (CutsceneManager.Instance == null)
        {
            Debug.LogWarning("[CutscenePickup] CutsceneManager.Instance is missing from the scene.", this);
            FinishPickup();
            return;
        }

        // Play the cutscene and finish the pickup once it ends.
        CutsceneManager.Instance.PlayCutscene(cutsceneClip, FinishPickup);
    }

    // Remove the pickup after it has been consumed.
    private void FinishPickup()
    {
        if (destroyAfterCutscene)
            Destroy(gameObject);
    }
}
