// CurrencyManager.cs
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Lightweight local currency UI helper that forwards to ShopSystem if present.
/// Keeps backwards compatibility with other scripts referencing CurrencyManager.Instance.
/// </summary>
public class CurrencyManager : MonoBehaviour
{
    public static CurrencyManager Instance { get; private set; }

    [Header("Starting values")]
    public int startingCoins = 1000;

    [Header("UI")]
    public Text coinText; // assign a UnityEngine.UI.Text in the inspector

    private int currentCoins;

    void Awake()
    {
        // singleton pattern
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // load saved or default
        currentCoins = PlayerPrefs.GetInt("Coins", startingCoins);
    }

    void Start()
    {
        RefreshUI();
    }

    public int Coins
    {
        get
        {
            if (ShopSystem.Instance != null) return ShopSystem.Instance.coins;
            return currentCoins;
        }
    }

    public bool SpendCoins(int amount)
    {
        if (amount <= 0) return true;
        if (ShopSystem.Instance != null)
        {
            return ShopSystem.Instance.SpendCoins(amount);
        }

        if (currentCoins >= amount)
        {
            currentCoins -= amount;
            Save();
            RefreshUI();
            return true;
        }
        return false;
    }

    public void GrantCoins(int amount)
    {
        if (amount <= 0) return;
        if (ShopSystem.Instance != null)
        {
            ShopSystem.Instance.GrantCoins(amount);
            return;
        }

        currentCoins += amount;
        Save();
        RefreshUI();
    }

    void Save()
    {
        PlayerPrefs.SetInt("Coins", currentCoins);
        PlayerPrefs.Save();
    }

    void RefreshUI()
    {
        int display = (ShopSystem.Instance != null) ? ShopSystem.Instance.coins : currentCoins;
        if (coinText != null) coinText.text = display.ToString("N0");
    }
}
