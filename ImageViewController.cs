using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ModelViewController : MonoBehaviour
{
    [Header("Preview Tweaks")]
    public Vector3 shopPreviewOffset = Vector3.zero;
    
    [Header("Grid Adjustments")]
    public bool useAutoScaling = false; 
    public float manualScale = 1.0f;    
    public Vector3 manualRotation = new Vector3(-90, 0, 0); 
    public float targetSize = 5.0f;    
    public float gridScaleMultiplier = 1.0f; 

    [Header("UI Navigation")]
    public GameObject viewPanel;       
    public GameObject mainMenuPanel;   
    public RawImage previewDisplay;    
    public TextMeshProUGUI pageText; 
    public TextMeshProUGUI equipButtonText; 
    
    [Header("Dynamic Instructions")]
    public TextMeshProUGUI instructionText; 
    public int maxLevelForInstruction = 4;  
    
    [Header("The Button Swap System")]
    // 1. Assign your "IAP Buy Button" here (The one with the IAP Component)
    public GameObject buyButtonObj; 
    
    // 2. Assign your "Standard Equip Button" here (The normal button)
    public GameObject equipButtonObj; 
    
    [Header("3D Studio Setup")]
    public Transform studioRoot;       
    public List<Slide> sceneSlides = new List<Slide>();

    [System.Serializable]
    public class Slide {
        public string slideName;
        public GameObject prefabToEquip; 
        public List<GameObject> sceneObjects = new List<GameObject>();
    }

    private int currentSlide = 0;
    private List<GameObject> spawnedModels = new List<GameObject>();

    private void Start() { UpdatePageName(); }

    // --- BUTTON A: THE EQUIP LOGIC ---
    // Link the "Standard Equip Button" to THIS function
    public void OnEquipButtonPressed()
    {
        if (sceneSlides == null || currentSlide >= sceneSlides.Count) return;
        Slide activeSlide = sceneSlides[currentSlide];

        // --- SECURITY CHECK ---
        // If this is the premium slide ("custom shields2") and we DO NOT own it, stop immediately.
        bool isPremium = (activeSlide.slideName == "custom shields2"); 
        bool ownsShields2 = PlayerPrefs.GetInt("PMG_Owns_CustomShields2", 0) == 1;

        if (isPremium && !ownsShields2)
        {
            Debug.LogError("Security Block: You cannot equip this item because you do not own it yet.");
            RefreshDisplay(); // Force the UI to update to the correct "Buy" button
            return;
        }
        // -----------------------
        
        GameObject selectedPrefab = GetCurrentEquipPrefab();
        string currentName = activeSlide.slideName;
        
        if (PatternMemoryGame.I != null)
        {
            if (selectedPrefab != null) // It's a Shape
            {
                PlayerPrefs.SetInt("PMG_WeaponsActive", 0); 
                PlayerPrefs.SetString("PMG_CurrentShape", selectedPrefab.name);
                PlayerPrefs.Save();
            }
            else // It's a Shield Loadout
            {
                PlayerPrefs.SetInt("PMG_WeaponsActive", 1);
                PlayerPrefs.SetString("EquippedLoadoutName", currentName); 
                PlayerPrefs.Save();
            }
        }
        
        Debug.Log($"<color=green>[Shop]</color> Equipped: {currentName}");
    }
    
    // --- BUTTON B: THE IAP SUCCESS LOGIC ---
    // Link the "IAP Buy Button" (On Purchase Complete) to THIS function
    public void UnlockPremiumShieldsSuccess()
    {
        Debug.Log("<color=green>[Shop] Purchase Successful! Unlocking Shields.</color>");
        PlayerPrefs.SetInt("PMG_Owns_CustomShields2", 1);
        PlayerPrefs.Save();
        RefreshDisplay(); 
    }

    public void OpenView() {
        if (viewPanel != null) viewPanel.SetActive(true);
        if (previewDisplay != null) previewDisplay.gameObject.SetActive(true);
        RefreshDisplay();
    }

    public void OnBackToMainMenu() {
        CleanupModels();
        if (viewPanel != null) viewPanel.SetActive(false);
        if (mainMenuPanel != null) {
            mainMenuPanel.SetActive(true); 
            var menuController = mainMenuPanel.GetComponent<MainMenuController>();
            if (menuController != null) menuController.ResetMenu();
        }
    }

    // --- MAIN LOGIC ---
    public void RefreshDisplay()
    {
        CleanupModels();
        if (sceneSlides == null || currentSlide >= sceneSlides.Count) return;

        Slide activeSlide = sceneSlides[currentSlide];
        int currentLevel = PlayerPrefs.GetInt("PMG_SavedActualLevel", 1);
        bool ownsShields2 = PlayerPrefs.GetInt("PMG_Owns_CustomShields2", 0) == 1;

        // --- THE BUTTON SWAPPER ---
        // Logic: If it is "Custom Shields 2" AND we don't own it -> Show BUY button.
        //        Otherwise -> Show EQUIP button.
        
        bool showBuyButton = (activeSlide.slideName == "custom shields2" && !ownsShields2);

        if (buyButtonObj != null) buyButtonObj.SetActive(showBuyButton);
        if (equipButtonObj != null) equipButtonObj.SetActive(!showBuyButton);

        // Update Text
        if (equipButtonText != null)
        {
            if (showBuyButton) equipButtonText.text = "BUY $1.99";
            else equipButtonText.text = "EQUIP";
        }
        
        // Instruction Text Logic
        if (instructionText != null)
        {
            if (currentLevel <= maxLevelForInstruction)
            {
                if (activeSlide.slideName == "custom shields") {
                    instructionText.gameObject.SetActive(true);
                    instructionText.text = "SWIPE LEFT FOR\nMORE SHIELDS >>";
                }
                else if (showBuyButton) {
                    instructionText.gameObject.SetActive(true);
                    instructionText.text = "PRESS ANYWHERE TO\nACQUIRE FOR $1.99";
                }
                else {
                    instructionText.gameObject.SetActive(false);
                }
            }
            else {
                instructionText.gameObject.SetActive(false);
            }
        }

        // 3D Generation
        if (activeSlide.slideName == "custom shields") GenerateLoadoutPreview(PatternMemoryGame.I.nodeLoadouts);
        else if (activeSlide.slideName == "custom shields2") GenerateLoadoutPreview(PatternMemoryGame.I.nodeLoadouts2);
        else {
            foreach (GameObject prefab in activeSlide.sceneObjects) {
                if (prefab == null) continue;
                GameObject instance = Instantiate(prefab, studioRoot);
                instance.transform.localPosition = Vector3.zero + shopPreviewOffset;
                instance.transform.localRotation = Quaternion.identity;
                instance.transform.localScale = Vector3.one; 
                if(useAutoScaling) { CenterInstanceVisuals(instance); ScaleToFit(instance, targetSize); }
                SetLayerRecursively(instance, studioRoot.gameObject.layer);
                spawnedModels.Add(instance);
            }
        }
        UpdatePageName();
    }

    // (Helpers kept same)
    private void GenerateLoadoutPreview(List<PlayerLoadout> loadouts)
    {
        if (loadouts == null || PatternMemoryGame.I == null || PatternMemoryGame.I.nodesParent == null) return;
        
        GameObject gridContainer = new GameObject("GridPreviewContainer");
        gridContainer.transform.SetParent(studioRoot, false);
        spawnedModels.Add(gridContainer);
        
        foreach (var item in loadouts)
        {
            // Skip if both are empty (but allow if one is missing)
            if (item.shieldPrefab == null && item.knifePrefab == null) continue;

            GameObject nodeHolder = new GameObject("NodeHolder_" + item.nodeName);
            nodeHolder.transform.SetParent(gridContainer.transform, false);

            // 1. POSITIONING: Use the Game Board Sockets (The layout you liked)
            if (item.nodeSocket != null) {
                Vector3 relativePos = PatternMemoryGame.I.nodesParent.InverseTransformPoint(item.nodeSocket.position);
                nodeHolder.transform.localPosition = relativePos;
            }

            // 2. SPAWN SHIELD (Primary)
            if (item.shieldPrefab != null)
            {
                GameObject shield = Instantiate(item.shieldPrefab, nodeHolder.transform);
                shield.transform.localPosition = item.gunOffset;
                shield.transform.localEulerAngles = item.gunRotation;
                shield.transform.localScale = item.gunScale; 
            }

            // 3. SPAWN KNIFE / INTERIOR (Secondary) - THIS WAS MISSING!
            if (item.knifePrefab != null)
            {
                GameObject knife = Instantiate(item.knifePrefab, nodeHolder.transform);
                knife.transform.localPosition = item.knifeOffset;
                knife.transform.localEulerAngles = item.knifeRotation;
                knife.transform.localScale = item.knifeScale;
            }

            SetLayerRecursively(nodeHolder, studioRoot.gameObject.layer);
        }

        // Apply Scaling & Centering to the whole group
        if (useAutoScaling) {
            CenterInstanceVisuals(gridContainer); // Centers the board in the view
            ScaleToFit(gridContainer, targetSize * gridScaleMultiplier);
            gridContainer.transform.localEulerAngles += manualRotation; 
        } else {
            gridContainer.transform.localScale = Vector3.one * manualScale;
            gridContainer.transform.localEulerAngles = manualRotation;
        }
        gridContainer.transform.localPosition += shopPreviewOffset;
    }
    private void UpdatePageName() { if (pageText != null && sceneSlides != null && sceneSlides.Count > currentSlide) pageText.text = sceneSlides[currentSlide].slideName; }
    public GameObject GetCurrentEquipPrefab() { if (sceneSlides == null || sceneSlides.Count == 0 || currentSlide >= sceneSlides.Count) return null; return sceneSlides[currentSlide].prefabToEquip; }
    private void CleanupModels() { foreach (var m in spawnedModels) if (m) Destroy(m); spawnedModels.Clear(); }
    public void NextSlide() { if (currentSlide < sceneSlides.Count - 1) { currentSlide++; RefreshDisplay(); } }
    public void PreviousSlide() { if (currentSlide > 0) { currentSlide--; RefreshDisplay(); } }
    private void ScaleToFit(GameObject obj, float target) { Bounds bounds = new Bounds(obj.transform.position, Vector3.zero); foreach (Renderer r in obj.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds); float maxDim = Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z); if (maxDim > 0) obj.transform.localScale *= (target / maxDim); }
    private void CenterInstanceVisuals(GameObject instance) { Bounds bounds = new Bounds(instance.transform.position, Vector3.zero); foreach (Renderer r in instance.GetComponentsInChildren<Renderer>()) bounds.Encapsulate(r.bounds); instance.transform.position -= bounds.center; }
    private void SetLayerRecursively(GameObject obj, int newLayer) { if (null == obj) return; obj.layer = newLayer; foreach (Transform child in obj.transform) { if (null == child) continue; SetLayerRecursively(child.gameObject, newLayer); } }
    
    [ContextMenu("RESET SHOP PURCHASE")]
    public void ResetPurchaseData() { PlayerPrefs.DeleteKey("PMG_Owns_CustomShields2"); PlayerPrefs.Save(); Debug.Log("Shop Purchase Reset!"); RefreshDisplay(); }
}