using UnityEngine;
using System.Collections;

/*
* ShopSystem
*
* Handles shop purchases and permanent health upgrades.
* Uses berries as currency, updates the player's health,
* and saves purchased upgrades
*/

public class ShopSystem : MonoBehaviour, IDataPersistence
{
    public static ShopSystem Instance;

    [Header("Prices")]
    public int maxHpUpgradeCost = 10;
    public int maxHpUpgrade2Cost = 20;
    public int maxHpUpgrade3Cost = 30;

    private bool maxHpBought;
    private bool maxHp2Bought;
    private bool maxHp3Bought;
    private bool loadedUpgradesApplied;

    public bool IsMaxHpBought() { return maxHpBought; }
    public bool IsMaxHp2Bought() { return maxHp2Bought; }
    public bool IsMaxHp3Bought() { return maxHp3Bought; }

    private void Awake()
    {
        Instance = this;
    }

    public void LoadData(GameData data)
    {
        maxHpBought = data.maxHpBought;
        maxHp2Bought = data.maxHp2Bought;
        maxHp3Bought = data.maxHp3Bought;
        loadedUpgradesApplied = false;

        StartCoroutine(ApplyUpgradesAfterLoad());
    }

    private IEnumerator ApplyUpgradesAfterLoad()
    {
        // Wait one frame to ensure PlayerHealth and UI are initialized.
        yield return null;

        if (loadedUpgradesApplied)
            yield break;

        PlayerHealth player = Object.FindFirstObjectByType<PlayerHealth>();
        if (player == null)
            yield break;

        loadedUpgradesApplied = true;
        PlayerHealthUI healthUI = ResolveHealthUI(player);

        if (maxHpBought)
        {
            player.maxHealth += 1;
            healthUI?.AddHeart();
        }

        if (maxHp2Bought)
        {
            player.maxHealth += 1;
            healthUI?.AddHeart();
        }

        if (maxHp3Bought)
        {
            player.maxHealth += 1;
            healthUI?.AddHeart();
        }

        player.currentHealth = player.maxHealth;
        healthUI?.UpdateHearts();
    }

    public void SaveData(ref GameData data)
    {
        data.maxHpBought = maxHpBought;
        data.maxHp2Bought = maxHp2Bought;
        data.maxHp3Bought = maxHp3Bought;
    }

    public bool BuyMaxHP()
    {
        if (maxHpBought)
        {
            return false;
        }

        if (!TryBuyUpgrade(maxHpUpgradeCost))
            return false;

        maxHpBought = true;
        DataPersistenceManager.Instance?.SaveGame();
        return true;
    }

    public bool BuyMaxHP2()
    {
        if (maxHp2Bought)
        {
            return false;
        }

        if (!TryBuyUpgrade(maxHpUpgrade2Cost))
            return false;

        maxHp2Bought = true;
        DataPersistenceManager.Instance?.SaveGame();
        return true;
    }

    public bool BuyMaxHP3()
    {
        if (maxHp3Bought)
        {
            return false;
        }

        if (!TryBuyUpgrade(maxHpUpgrade3Cost))
            return false;

        maxHp3Bought = true;
        DataPersistenceManager.Instance?.SaveGame();
        return true;
    }

    private bool TryBuyUpgrade(int cost)
    {
        PlayerHealth player = Object.FindFirstObjectByType<PlayerHealth>();
        if (player == null)
        {
            Debug.LogWarning("[ShopSystem] PlayerHealth missing from scene.");
            return false;
        }

        if (PlayerCurrency.Instance == null)
        {
            Debug.LogWarning("[ShopSystem] PlayerCurrency.Instance missing from scene.");
            return false;
        }

        if (PlayerCurrency.Instance.CurrentAmount < cost)
        {
            return false;
        }

        ApplyMaxHealthUpgrade(player);
        PlayerCurrency.Instance.TrySpend(cost);
        return true;
    }

    private void ApplyMaxHealthUpgrade(PlayerHealth player)
    {
        PlayerHealthUI healthUI = ResolveHealthUI(player);

        player.maxHealth += 1;
        player.currentHealth = player.maxHealth;
        healthUI?.AddHeart();
        healthUI?.UpdateHearts();
    }

    private PlayerHealthUI ResolveHealthUI(PlayerHealth player)
    {
        if (player == null)
            return Object.FindFirstObjectByType<PlayerHealthUI>();

        if (player.healthUI == null)
            player.healthUI = Object.FindFirstObjectByType<PlayerHealthUI>();

        return player.healthUI;
    }
}
