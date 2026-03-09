using UnityEngine;
using UnityEngine.UI;

public class DebugUIWizard : MonoBehaviour
{
    [ContextMenu("🎨 Create Debug UI Structure")]
    public void CreateDebugUIStructure()
    {
        // Clear existing children
        foreach (Transform child in transform)
        {
            DestroyImmediate(child.gameObject);
        }

        // Create background panel
        GameObject background = CreateUIObject("DebugBackground", transform);
        Image bgImage = background.AddComponent<Image>();
        bgImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform bgRect = background.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 1);
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // Create debug log area
        GameObject scrollView = CreateUIObject("DebugScrollView", background.transform);
        ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
        scrollView.AddComponent<Image>().color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
        
        // Viewport
        GameObject viewport = CreateUIObject("Viewport", scrollView.transform);
        viewport.AddComponent<RectMask2D>();
        Image viewportImage = viewport.AddComponent<Image>();
        viewportImage.color = new Color(0.05f, 0.05f, 0.05f, 1f);
        
        // Content
        GameObject content = CreateUIObject("Content", viewport.transform);
        
        // Debug log text
        GameObject logText = CreateUIObject("DebugLogText", content.transform);
        Text textComponent = logText.AddComponent<Text>();
        textComponent.color = Color.white;
        textComponent.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        textComponent.fontSize = 12;
        textComponent.alignment = TextAnchor.UpperLeft;
        
        // Setup scroll rect
        scrollRect.content = content.GetComponent<RectTransform>();
        scrollRect.viewport = viewport.GetComponent<RectTransform>();
        scrollRect.horizontal = false;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        
        // Position and size the scroll view
        RectTransform scrollRectTransform = scrollView.GetComponent<RectTransform>();
        scrollRectTransform.anchorMin = new Vector2(0, 0.3f);
        scrollRectTransform.anchorMax = new Vector2(1, 0.9f);
        scrollRectTransform.offsetMin = new Vector2(10, 10);
        scrollRectTransform.offsetMax = new Vector2(-10, -10);
        
        // Size the content and text
        RectTransform contentRect = content.GetComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0, 1);
        contentRect.anchorMax = new Vector2(1, 1);
        contentRect.pivot = new Vector2(0, 1);
        contentRect.sizeDelta = new Vector2(0, 500);
        
        RectTransform textRect = logText.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(5, 5);
        textRect.offsetMax = new Vector2(-5, -5);
        
        // Create debug buttons
        string[] buttonNames = new string[] {
            "ToggleDebugBtn", "ClearLogBtn", "TestPurchaseBtn", 
            "TestCurrencyBtn", "TestVisibilityBtn"
        };
        
        for (int i = 0; i < buttonNames.Length; i++)
        {
            GameObject button = CreateButton(buttonNames[i], background.transform);
            RectTransform btnRect = button.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(i * 0.2f, 0.1f);
            btnRect.anchorMax = new Vector2((i + 1) * 0.2f, 0.25f);
            btnRect.offsetMin = new Vector2(5, 5);
            btnRect.offsetMax = new Vector2(-5, -5);
            
            Text btnText = button.GetComponentInChildren<Text>();
            btnText.text = buttonNames[i].Replace("Btn", "");
        }
        
        // Create quick test panel
        GameObject quickTestPanel = CreateUIObject("QuickTestPanel", transform);
        quickTestPanel.AddComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f, 0.9f);
        RectTransform qtRect = quickTestPanel.GetComponent<RectTransform>();
        qtRect.anchorMin = new Vector2(0.7f, 0);
        qtRect.anchorMax = new Vector2(1, 0.3f);
        qtRect.offsetMin = new Vector2(5, 5);
        qtRect.offsetMax = new Vector2(-5, -5);
        
        // Create quick test buttons
        string[] quickTestNames = new string[] {
            "GrantCoinsBtn", "GrantKeysBtn", "ResetProgressBtn", 
            "UnlockAllSkinsBtn", "TestAllPurchasesBtn"
        };
        
        for (int i = 0; i < quickTestNames.Length; i++)
        {
            GameObject button = CreateButton(quickTestNames[i], quickTestPanel.transform);
            RectTransform btnRect = button.GetComponent<RectTransform>();
            btnRect.anchorMin = new Vector2(0, i * 0.2f);
            btnRect.anchorMax = new Vector2(1, (i + 1) * 0.2f);
            btnRect.offsetMin = new Vector2(5, 5);
            btnRect.offsetMax = new Vector2(-5, -5);
            
            Text btnText = button.GetComponentInChildren<Text>();
            btnText.text = quickTestNames[i].Replace("Btn", "\n");
        }
        
        Debug.Log("✅ Debug UI structure created! Now wire up the references in the Inspector.");
    }
    
    GameObject CreateUIObject(string name, Transform parent)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent);
        obj.AddComponent<RectTransform>();
        return obj;
    }
    
    GameObject CreateButton(string name, Transform parent)
    {
        GameObject button = new GameObject(name);
        button.transform.SetParent(parent);
        
        // Add button component
        Button btn = button.AddComponent<Button>();
        Image btnImage = button.AddComponent<Image>();
        btnImage.color = new Color(0.3f, 0.3f, 0.5f, 1f);
        
        // Create text child
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(button.transform);
        Text text = textObj.AddComponent<Text>();
        text.text = name.Replace("Btn", "");
        text.color = Color.white;
        text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 10;
        text.alignment = TextAnchor.MiddleCenter;
        
        // Set up text rect transform
        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        return button;
    }
}