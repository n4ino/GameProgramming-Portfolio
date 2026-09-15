using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/*
* ShopUI
*
* Manages the shop user interface, item selection,
* player input, and shop interactions.
* Handles opening, closing, navigation,
* and purchasing upgrades.
*/

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance;

    [Header("UI")]
    public GameObject shopPanel;
    public TextMeshProUGUI[] menuItems;
    public GameObject pressEText;

    [Header("Shopkeeper Feedback")]
    [SerializeField] private ShopkeeperFeedback shopkeeperFeedback;

    private int selectedIndex = 0;
    private bool isOpen = false;
    private bool justOpened = false;

    private PlayerController playerController;

    public bool IsOpen()
    {
        return isOpen;
    }

    private void Awake()
    {
        Instance = this;
        shopPanel.SetActive(false);
        pressEText.SetActive(false);
        playerController = Object.FindFirstObjectByType<PlayerController>();

        if (shopkeeperFeedback == null)
            shopkeeperFeedback = Object.FindFirstObjectByType<ShopkeeperFeedback>();
    }

    public void ShowPressE()
    {
        pressEText.SetActive(true);
    }

    public void HidePressE()
    {
        pressEText.SetActive(false);
    }

    public void ToggleShop()
    {
        if (!isOpen) OpenShop();
        else CloseShop();
    }

    public void OpenShop()
    {
        OpenShop(null);
    }

    public void OpenShop(ShopkeeperFeedback sourceFeedback)
    {
        isOpen = true;
        shopPanel.SetActive(true);
        selectedIndex = 0;

        if (playerController != null)
            playerController.movementLocked = true;

        if (sourceFeedback != null)
            shopkeeperFeedback = sourceFeedback;

        shopkeeperFeedback?.PlayOpenShop();

        if (ShopSystem.Instance.IsMaxHpBought())
            menuItems[0].gameObject.SetActive(false);
        else
            menuItems[0].gameObject.SetActive(true);

        if (ShopSystem.Instance.IsMaxHp2Bought())
            menuItems[1].gameObject.SetActive(false);
        else
            menuItems[1].gameObject.SetActive(true);

        if (ShopSystem.Instance.IsMaxHp3Bought())
            menuItems[2].gameObject.SetActive(false);
        else
            menuItems[2].gameObject.SetActive(true);

        selectedIndex = GetNextActiveIndex(0, 1);
        UpdateSelectionVisual();
        justOpened = true;
    }

    public void CloseShop()
    {
        isOpen = false;
        shopPanel.SetActive(false);

        if (playerController != null)
            playerController.movementLocked = false;
    }

    private void Update()
    {
        if (!isOpen) return;

        if (justOpened)
        {
            // Prevent the same input from instantly selecting an item when opening the shop.
            justOpened = false;
            return;
        }

        if (Keyboard.current == null) return;

        if (Keyboard.current.wKey.wasPressedThisFrame || Keyboard.current.upArrowKey.wasPressedThisFrame)
        {
            selectedIndex = GetNextActiveIndex(selectedIndex - 1, -1);
            UpdateSelectionVisual();
        }

        if (Keyboard.current.sKey.wasPressedThisFrame || Keyboard.current.downArrowKey.wasPressedThisFrame)
        {
            selectedIndex = GetNextActiveIndex(selectedIndex + 1, 1);
            UpdateSelectionVisual();
        }
    }

    public void SubmitSelected()
    {
        if (!isOpen || justOpened) return;
        BuySelected();
    }

    private int GetNextActiveIndex(int startIndex, int direction)
    {
        // Skip purchased (hidden) upgrades when navigating the menu.
        int index = startIndex;
        for (int i = 0; i < menuItems.Length; i++)
        {
            if (index < 0) index = menuItems.Length - 1;
            else if (index >= menuItems.Length) index = 0;
            if (menuItems[index].gameObject.activeSelf) return index;
            index += direction;
        }
        return selectedIndex;
    }

    private void UpdateSelectionVisual()
    {
        for (int i = 0; i < menuItems.Length; i++)
            menuItems[i].color = (i == selectedIndex ? Color.yellow : Color.white);
    }

    private void BuySelected()
    {
        switch (selectedIndex)
        {
            case 0:
                bool success = ShopSystem.Instance.BuyMaxHP();
                if (success)
                {
                    shopkeeperFeedback?.PlayPurchase();
                    menuItems[0].gameObject.SetActive(false);
                    selectedIndex = GetNextActiveIndex(selectedIndex, 1);
                    UpdateSelectionVisual();
                }
                break;

            case 1:
                bool success2 = ShopSystem.Instance.BuyMaxHP2();
                if (success2)
                {
                    shopkeeperFeedback?.PlayPurchase();
                    menuItems[1].gameObject.SetActive(false);
                    selectedIndex = GetNextActiveIndex(selectedIndex, 1);
                    UpdateSelectionVisual();
                }
                break;

            case 2:
                bool success3 = ShopSystem.Instance.BuyMaxHP3();
                if (success3)
                {
                    shopkeeperFeedback?.PlayPurchase();
                    menuItems[2].gameObject.SetActive(false);
                    selectedIndex = GetNextActiveIndex(selectedIndex, 1);
                    UpdateSelectionVisual();
                }
                break;
        }
    }
}
