// ShopSystem.cs
using System;
using UnityEngine;

public class ShopSystem : MonoBehaviour
{
    public static ShopSystem Instance { get; private set; }

    // persistent currencies (saved to PlayerPrefs)
    public int coins = 1000;
    public int keys = 0;

    // events other systems can subscribe to
    public event Action<int> OnCoinsChanged;
    public event Action<int> OnKeysChanged;

    void Awake()
    {
        // singleton pattern: ensure one instance
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadCurrency();
    }

    void LoadCurrency()
    {
        coins = PlayerPrefs.GetInt("Coins", coins);
        keys = PlayerPrefs.GetInt("Keys", keys);

        // Notify listeners of initial values
        OnCoinsChanged?.Invoke(coins);
        OnKeysChanged?.Invoke(keys);
    }

    void SaveCurrency()
    {
        PlayerPrefs.SetInt("Coins", coins);
        PlayerPrefs.SetInt("Keys", keys);
        PlayerPrefs.Save();
    }

    // Public API
    public void GrantCoins(int amount)
    {
        if (amount <= 0) return;
        coins += amount;
        SaveCurrency();
        OnCoinsChanged?.Invoke(coins);
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return true;
        if (coins >= amount)
        {
            coins -= amount;
            SaveCurrency();
            OnCoinsChanged?.Invoke(coins);
            return true;
        }
        return false;
    }

    public void GrantKeys(int amount)
    {
        if (amount <= 0) return;
        keys += amount;
        SaveCurrency();
        OnKeysChanged?.Invoke(keys);
    }

    public bool SpendKeys(int amount)
    {
        if (amount <= 0) return true;
        if (keys >= amount)
        {
            keys -= amount;
            SaveCurrency();
            OnKeysChanged?.Invoke(keys);
            return true;
        }
        return false;
    }

    // Utility to force UI to refresh externally
    public void NotifyCurrencyChanged()
    {
        OnCoinsChanged?.Invoke(coins);
        OnKeysChanged?.Invoke(keys);
    }
}
