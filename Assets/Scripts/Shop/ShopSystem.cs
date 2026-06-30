using UnityEngine;
using System.Collections.Generic;

/// <summary>Loja com itens, skins, habilidades e consumíveis.</summary>
public class ShopSystem : MonoBehaviour
{
    public static ShopSystem Instance { get; private set; }

    public enum ItemCategory { Character, Ability, Consumable, Cosmetic, SeasonPass }

    [System.Serializable]
    public class ShopItem
    {
        public string id;
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public ItemCategory category;
        public int coinCost;
        public int gemCost;
        public bool isPermanent;
        public bool isLimited;
    }

    [SerializeField] private List<ShopItem> catalog;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public bool Purchase(string itemId)
    {
        var item = catalog?.Find(x => x.id == itemId);
        if (item == null) return false;

        var data = SaveSystem.Instance?.Data;
        if (data == null) return false;
        if (data.purchasedItems.Contains(itemId)) return false;

        bool paid = false;
        if (item.gemCost > 0)
            paid = SaveSystem.Instance.SpendGems(item.gemCost);
        else if (item.coinCost > 0)
            paid = SaveSystem.Instance.SpendCoins(item.coinCost);

        if (!paid) { UIManager.Instance?.ShowMessage("Moedas/Gemas insuficientes!"); return false; }

        data.purchasedItems.Add(itemId);
        SaveSystem.Instance.Save();
        AudioManager.Instance?.Play("purchase");
        UIManager.Instance?.ShowMessage($"Comprado: {item.displayName}!");
        return true;
    }

    public bool IsPurchased(string itemId) =>
        SaveSystem.Instance?.Data?.purchasedItems?.Contains(itemId) ?? false;

    public List<ShopItem> GetByCategory(ItemCategory cat) =>
        catalog?.FindAll(x => x.category == cat) ?? new List<ShopItem>();
}
