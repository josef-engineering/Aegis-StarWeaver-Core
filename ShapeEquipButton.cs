using UnityEngine;
using UnityEngine.UI;

public class ShapeEquipButton : MonoBehaviour
{
    // Reference to the View Controller so we know which slide is active
    public ModelViewController modelViewer; 

    void Start()
    {
        Button btn = GetComponent<Button>();
        if (btn != null)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(EquipCurrentSlideShape);
        }
    }

    public void EquipCurrentSlideShape()
    {
        if (modelViewer == null)
        {
            Debug.LogError("[ShapeEquipButton] ModelViewController is NOT assigned in Inspector!");
            return;
        }

        // 1. Ask the viewer for the current prefab
        GameObject prefabToEquip = modelViewer.GetCurrentEquipPrefab();

        if (prefabToEquip == null)
        {
            Debug.LogError("[ShapeEquipButton] The current slide has no 'Prefab To Equip' assigned!");
            return;
        }

        if (PatternMemoryGame.I == null)
        {
            Debug.LogError("PatternMemoryGame.I is null! Is the game running?");
            return;
        }

        // 2. Send it to the game
        PatternMemoryGame.I.ApplyShape(prefabToEquip);

        Debug.Log($"[ShapeEquipButton] Equipped: {prefabToEquip.name}");

        // Optional: Save preference
        PlayerPrefs.SetString("PMG_CurrentShape", prefabToEquip.name);
        PlayerPrefs.Save();
    }
}