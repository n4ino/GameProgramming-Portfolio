using UnityEngine;

/*
* ShopkeeperFeedback
*
* Handles shopkeeper animations and audio feedback
* when the shop is opened or a purchase is made.
*/

public class ShopkeeperFeedback : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

    [Header("Open Shop")]
    [SerializeField] private string openTrigger = "OpenShop";
    [SerializeField] private AudioClip openClip;

    [Header("Purchase")]
    [SerializeField] private string purchaseTrigger = "Purchase";
    [SerializeField] private AudioClip purchaseClip;

    private void Reset()
    {
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();
    }

    public void PlayOpenShop()
    {
        PlayFeedback(openTrigger, openClip);
    }

    public void PlayPurchase()
    {
        PlayFeedback(purchaseTrigger, purchaseClip);
    }

    private void PlayFeedback(string triggerName, AudioClip clip)
    {
        // Play animation and sound effect associated with the shop event.
        if (animator != null && !string.IsNullOrWhiteSpace(triggerName))
            animator.SetTrigger(triggerName);

        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip);
    }
}
