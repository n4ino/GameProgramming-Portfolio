using UnityEngine;

/*
* Echo
*
* Stores lost player currency after death and allows it
* to be recovered when the player returns to the Echo's location.
* Handles player interaction prompts within the Echo area.
*/

public class Echo : MonoBehaviour
{
    public static Echo Current { get; private set; }

    private int storedAmount = 0;

    public void StoreCurrency(int amount)
    {
        // Store the amount of currency that can be recovered later.
        storedAmount = amount;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Current = this;
        EchoPromptUI.Instance?.Show();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (Current == this)
            Current = null;

        EchoPromptUI.Instance?.Hide();
    }

    private void OnDestroy()
    {
        if (Current != this) return;

        Current = null;
        EchoPromptUI.Instance?.Hide();
    }

    public void Recover()
    {
        // Return the stored currency to the player.
        PlayerCurrency.Instance?.Restore(storedAmount);
    }
}
