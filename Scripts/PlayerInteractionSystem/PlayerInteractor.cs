using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/*
* PlayerInteractor
*
* Handles player interactions with world objects,
* including checkpoints, shops, and Echo recovery.
* Displays context-sensitive interaction prompts.
*/

public class PlayerInteractor : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI interactText;

    private CheckpointRest currentRestPoint;
    private PlayerController controller;

    private void Awake()
    {
        controller = GetComponent<PlayerController>();

        if (interactText != null)
            interactText.gameObject.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed) return;

        // Prioritize interactions based on the current context.
        if (ShopUI.Instance != null && ShopUI.Instance.IsOpen())
        {
            ShopUI.Instance.SubmitSelected();
            return;
        }

        if (Echo.Current != null)
        {
            ForestEchoManager.Instance?.RecoverEcho();
            return;
        }

        if (ShopTrigger.Current != null)
        {
            ShopTrigger.Current.Interact();
            return;
        }

        if (currentRestPoint != null && controller != null)
            currentRestPoint.Rest(controller);
    }

    // Display the checkpoint interaction prompt.
    private void ShowPrompt()
    {
        if (interactText == null) return;
        interactText.text = "Press E to Rest";
        interactText.gameObject.SetActive(true);
    }

    // Hide the checkpoint interaction prompt.
    private void HidePrompt()
    {
        if (interactText == null) return;
        interactText.gameObject.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CheckpointRest rest = other.GetComponent<CheckpointRest>();
        if (rest == null) return;

        // Register the checkpoint resting area currently occupied by the player.
        currentRestPoint = rest;
        ShowPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.GetComponent<CheckpointRest>() == null) return;

        // Clear the active resting area when the player leaves it.
        currentRestPoint = null;
        HidePrompt();
    }
}
