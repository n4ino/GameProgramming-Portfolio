using UnityEngine;

/*
* ShopTrigger
*
* Manages player interaction with shop zones.
* Handles trigger detection, interaction prompts,
* and shop UI opening/closing.
*/

public class ShopTrigger : MonoBehaviour
{
    public static ShopTrigger Current { get; private set; }

    [SerializeField] private ShopkeeperFeedback shopkeeperFeedback;

    private void Awake()
    {
        if (shopkeeperFeedback == null)
            shopkeeperFeedback = GetComponentInChildren<ShopkeeperFeedback>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        // Register this shop as the currently available interaction.
        Current = this;
        ShopUI.Instance?.ShowPressE();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (Current == this)
            Current = null;

        ShopUI.Instance?.HidePressE();
        ShopUI.Instance?.CloseShop();
    }

    public void Interact()
    {
        if (ShopUI.Instance != null && !ShopUI.Instance.IsOpen())
            ShopUI.Instance.OpenShop(shopkeeperFeedback);
    }
}
