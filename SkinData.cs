// SkinData.cs
using System;
using UnityEngine;

[Serializable]
public class SkinData
{
    public string id;
    public string displayName;
    public Material material;
    public int priceCoins = 0;
    public bool unlockByDefault = false;
    public Color trailColor = Color.white;
    public string subtitle;
    // You can extend this with more fields like icons, iapProductId, etc.

    // convenience
    public bool IsUnlocked()
    {
        if (unlockByDefault) return true;
        return PlayerPrefs.GetInt("skin_unlocked_" + id, 0) == 1 || PlayerPrefs.GetInt("own_" + id, 0) == 1;
    }

    public void MarkUnlocked()
    {
        PlayerPrefs.SetInt("skin_unlocked_" + id, 1);
        PlayerPrefs.SetInt("own_" + id, 1);
        PlayerPrefs.Save();
    }
}
