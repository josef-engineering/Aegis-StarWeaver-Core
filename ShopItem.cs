// ShopItem.cs
using System;
using UnityEngine;

[Serializable]
public enum ShopItemType { CoinPack, Consumable, PowerUp, Skin, Shape, Character, Bundle, IAP }

[Serializable]
public class ShopItem
{
    [Header("Identity")]
    public string id; // unique id (e.g. "skin_neon")
    public string displayName;
    public ShopItemType itemType = ShopItemType.Skin;

    [Header("Costs")]
    public int priceCoins = 0;
    public int priceKeys = 0;
    public string iapProductId = null; // optional for IAP

    [Header("Rewards / Payload")]
    public int grantCoins = 0;
    public int grantKeys = 0;
    public string grantsSkinId = null;
    public string grantsShapeId = null;

    [Header("Upgrade (optional)")]
    public bool isUpgradeable = false;
    public int level = 0;
    public int maxLevel = 1;
    public int baseUpgradeCost = 100;

    [Header("UI helpers")]
    public Sprite icon;
    public string subtitle;

    [Header("Limited offer")]
    public bool isLimitedTime = false;
    public double limitedStartUnix = 0;
    public double limitedDurationSeconds = 0;

    // Convenience: ownership stored in PlayerPrefs with key "own_<id>"
    public bool IsOwned()
    {
        if (string.IsNullOrEmpty(id)) return false;
        if (itemType == ShopItemType.CoinPack || itemType == ShopItemType.IAP) return false;
        if (PlayerPrefs.GetInt("own_" + id, 0) == 1) return true;
        return false;
    }

    public void MarkOwned()
    {
        if (string.IsNullOrEmpty(id)) return;
        PlayerPrefs.SetInt("own_" + id, 1);
        PlayerPrefs.Save();
    }

    // Upgrade persistence
    public int LoadLevel()
    {
        if (!isUpgradeable || string.IsNullOrEmpty(id)) return level;
        level = PlayerPrefs.GetInt("lvl_" + id, level);
        return level;
    }

    public void SaveLevel(int newLevel)
    {
        if (!isUpgradeable || string.IsNullOrEmpty(id)) return;
        level = Mathf.Clamp(newLevel, 0, maxLevel);
        PlayerPrefs.SetInt("lvl_" + id, level);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// Returns the cost to upgrade to the next level (compat name).
    /// ShopItemButtonController expects GetUpgradeCostForNextLevel(); keep it here.
    /// </summary>
    public int GetUpgradeCostForNextLevel()
    {
        return GetNextUpgradeCost();
    }

    /// <summary>
    /// Older/newer code may call either name — both exist for safety.
    /// </summary>
    public int GetNextUpgradeCost()
    {
        if (!isUpgradeable) return 0;
        return Mathf.Max(0, baseUpgradeCost * (level + 1));
    }

    public bool IsAvailableNow()
    {
        if (!isLimitedTime) return true;
        double now = (DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        return now >= limitedStartUnix && now <= limitedStartUnix + limitedDurationSeconds;
    }
}
