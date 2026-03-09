// ShopReferenceValidator.cs
// Place this in Assets/Editor/
using UnityEngine;
using UnityEditor;

public class ShopReferenceValidator : MonoBehaviour
{
    // Assign these inspector fields to match your scene's components (optional)
    public GameObject mainMenuControllerObj;
    public GameObject patternMemoryGameObj;
    public GameObject introLoadingControllerObj;
    public GameObject shopContentControllerObj;

    [ContextMenu("Validate Shop References (log)")]
    public void ValidateAll()
    {
        ValidateField(mainMenuControllerObj, "MainMenuController", "shopPanel");
        ValidateField(patternMemoryGameObj, "PatternMemoryGame", "shopPanel");
        ValidateField(introLoadingControllerObj, "IntroLoadingController", "shopPanel");
        ValidateField(shopContentControllerObj, "ShopContentController", "itemsContainer (child)");
    }

    void ValidateField(GameObject go, string compName, string field)
    {
        if (go == null) { Debug.LogWarning($"[ShopValidator] {compName} gameObject not assigned on validator."); return; }
        Debug.Log($"[ShopValidator] {compName} assigned GameObject: {go.name}. Check Inspector -> {field} on its component(s).");

        // Try to find a likely root: Menu_Shop_Panel
        var root = GameObject.Find("Menu_Shop_Panel");
        if (root != null) Debug.Log($"[ShopValidator] Found root candidate: {root.name}. If your {compName}.{field} is not pointing to it, you probably assigned the wrong child.");
        else Debug.Log($"[ShopValidator] Could not find a GameObject named 'Menu_Shop_Panel' in scene. Make sure the root shop object is named consistently or update this validator.");
    }
}
