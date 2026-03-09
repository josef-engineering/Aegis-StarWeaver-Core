using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class MainMenuController : MonoBehaviour
{
    [Header("UI References")]
    public Button startButton;          
    public GameObject mainContentObj;   
    public Button shopButton;
    public Button skinsButton; 
    
    [Header("Dynamic Text")]
    [Tooltip("Assign 'Text_PlayLevel' here. This ONE will disappear.")]
    public TextMeshProUGUI gameStartText;   
    
    [Tooltip("Assign 'Text_BlackHole' here. This ONE will stay.")]
    public TextMeshProUGUI blackHoleNameText; 
    
    public float indicatorDuration = 10.0f; 

    [Header("Settings Menu")]
    public Button settingsButton;       
    public GameObject settingsPanel;    
    public Button closeSettingsButton;  

    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI levelText; 

    [Header("Intro Animation")]
    public List<Transform> blackHoleParts;   
    public GameObject startTextObject; 
    public float growthSpeed = 3.5f;
    public Vector3 targetScale = new Vector3(11, 11, 11);

    [Header("Panel References")]
    public GameObject shopPanel; 
    public GameObject backgroundsPanel; 
    
    private CanvasGroup menuCanvasGroup;
    private Coroutine hideTextCoroutine; 
    
    private string[] blackHoleNames = new string[]
    {
        "GAIA BH1", "GAIA BH2", "A0620-00", "CYGNUS X-1", "V404 CYGNI", 
        "GRO J1655-40", "SAGITTARIUS A*", "M87*", "OJ 287", "TON 618"
    };

    void Start()
    {
        Application.targetFrameRate = 60; 

        // --- SAFETY CHECK ---
        if (gameStartText == blackHoleNameText && gameStartText != null)
        {
            Debug.LogError("<color=red><b>SETUP ERROR:</b> You assigned the SAME text object to both slots!</color>");
        }

        menuCanvasGroup = GetComponent<CanvasGroup>();
        if (menuCanvasGroup == null) menuCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        SetupButtons();
        UpdateCurrencyUI();
        UpdateLevelUI(); 
        if (ShopSystem.Instance != null) ShopSystem.Instance.OnCoinsChanged += UpdateCurrencyUI;
        ResetMenu(); 
    }

    public void ResetMenu()
    {
        UpdateLevelUI(); 

        if (menuCanvasGroup != null) { menuCanvasGroup.alpha = 1f; menuCanvasGroup.blocksRaycasts = true; menuCanvasGroup.interactable = true; }

        var bgImage = GetComponent<Image>();
        if (bgImage != null) bgImage.enabled = true;

        if (startButton != null) { startButton.interactable = true; startButton.gameObject.SetActive(true); }
        if (mainContentObj != null) mainContentObj.SetActive(true);
        this.gameObject.SetActive(true);

        if (gameStartText != null)
        {
            gameStartText.gameObject.SetActive(true); 
            if (hideTextCoroutine != null) StopCoroutine(hideTextCoroutine); 
            hideTextCoroutine = StartCoroutine(AutoHideIndicator()); 
        }

        if (blackHoleNameText != null)
        {
            blackHoleNameText.gameObject.SetActive(true); 
        }

        if (blackHoleParts != null && blackHoleParts.Count > 0)
        {
            StopAllCoroutines(); 
            StartCoroutine(PlayIntroSequence());
            if (gameStartText != null) hideTextCoroutine = StartCoroutine(AutoHideIndicator());
        }
    }

    IEnumerator AutoHideIndicator()
    {
        yield return new WaitForSeconds(indicatorDuration);
        if (gameStartText != null) gameStartText.gameObject.SetActive(false);
        if (blackHoleNameText != null) blackHoleNameText.gameObject.SetActive(true);
    }

    public void UpdateLevelUI() 
    { 
        int realLevel = PlayerPrefs.GetInt("PMG_SavedActualLevel", 1);
        int levelsPerBoss = 4;

        if (gameStartText != null) gameStartText.text = $"PLAY LEVEL {realLevel}";

        if (blackHoleNameText != null)
        {
            int index = (realLevel - 1) / levelsPerBoss;
            if (index >= blackHoleNames.Length) index = blackHoleNames.Length - 1;
            blackHoleNameText.text = $"BLACKHOLE:\n{blackHoleNames[index]}\n<size=60%>SPIN ME</size>";
            blackHoleNameText.gameObject.SetActive(true); 
        }

        if (levelText != null) levelText.text = "Level " + realLevel; 
    }

    public void OnStartGame() 
    { 
        if (menuCanvasGroup != null) { menuCanvasGroup.alpha = 0f; menuCanvasGroup.blocksRaycasts = false; menuCanvasGroup.interactable = false; } 
        if (PatternMemoryGame.I != null) PatternMemoryGame.I.StartGame(); 
    }

    IEnumerator PlayIntroSequence() 
    { 
        if (startTextObject != null) startTextObject.SetActive(false); 
        Vector3 startScale = Vector3.zero; 
        foreach (Transform part in blackHoleParts) if (part != null) part.localScale = startScale; 
        float timer = 0f; 
        while (timer < growthSpeed) { 
            float progress = timer / growthSpeed; 
            float curve = Mathf.SmoothStep(0, 1, progress); 
            Vector3 currentScale = Vector3.Lerp(startScale, targetScale, curve); 
            foreach (Transform part in blackHoleParts) if (part != null) part.localScale = currentScale; 
            timer += Time.deltaTime; 
            yield return null; 
        } 
        foreach (Transform part in blackHoleParts) if (part != null) part.localScale = targetScale; 
        if (startTextObject != null) startTextObject.SetActive(true); 
    }

    void SetupButtons() 
    { 
        if (startButton != null) { startButton.onClick.RemoveAllListeners(); startButton.onClick.AddListener(OnStartGame); } 
        if (shopButton != null) { shopButton.onClick.RemoveAllListeners(); shopButton.onClick.AddListener(OnOpenShop); } 
        if (skinsButton != null) { skinsButton.onClick.RemoveAllListeners(); skinsButton.onClick.AddListener(OnOpenSkins); } 
        if (settingsButton != null) { settingsButton.onClick.RemoveAllListeners(); settingsButton.onClick.AddListener(OnOpenSettings); } 
        if (closeSettingsButton != null) { closeSettingsButton.onClick.RemoveAllListeners(); closeSettingsButton.onClick.AddListener(OnCloseSettings); } 
    }

    // --- SHOP NAVIGATION ---
    public void OnOpenShop() 
    { 
        if (shopPanel == null) return; 
        if (mainContentObj != null) mainContentObj.SetActive(false); 
        
        var shopContent = shopPanel.transform.Find("ShopContent"); 
        if (shopContent != null) shopContent.gameObject.SetActive(true);  
    }

    // *** ADD THIS FUNCTION TO YOUR SHOP CLOSE BUTTON ***
    public void OnCloseShop()
    {
        if (shopPanel != null)
        {
            var shopContent = shopPanel.transform.Find("ShopContent");
            if (shopContent != null) shopContent.gameObject.SetActive(false);
        }
        if (mainContentObj != null) mainContentObj.SetActive(true);
    }
    // -----------------------

    public void OnOpenSkins() 
    { 
        if (shopPanel != null) shopPanel.SetActive(true); 
        if (mainContentObj != null) mainContentObj.SetActive(false); 
        shopPanel.transform.Find("ShopContent")?.gameObject.SetActive(false); 
        if (backgroundsPanel != null) { 
            backgroundsPanel.SetActive(true); 
            backgroundsPanel.transform.SetAsLastSibling(); 
            backgroundsPanel.GetComponent<ModelViewController>()?.OpenView(); 
        } 
    }

    public void OnOpenSettings() { if (mainContentObj != null) mainContentObj.SetActive(false); if (settingsPanel != null) settingsPanel.SetActive(true); }
    public void OnCloseSettings() { if (settingsPanel != null) settingsPanel.SetActive(false); if (mainContentObj != null) mainContentObj.SetActive(true); }
    public void UpdateCurrencyUI(int coins = 0) { if (coinsText != null && ShopSystem.Instance != null) coinsText.text = ShopSystem.Instance.coins.ToString("N0"); }
    void OnDestroy() { if (ShopSystem.Instance != null) ShopSystem.Instance.OnCoinsChanged -= UpdateCurrencyUI; }
    public void OpenPrivacyPolicy() { Application.OpenURL("https://docs.google.com/document/d/1AH4rWt3uZ6fQzrhgUzrOkNaFEut6u6O3cYoKdytFuyo/edit?usp=sharing"); }
    public void OpenTermsOfService() { Application.OpenURL("https://your-website.com/terms"); }
}