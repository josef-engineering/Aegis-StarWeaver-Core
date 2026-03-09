using UnityEngine;
using UnityEngine.UI;

public class PurchaseHintsButton : MonoBehaviour
{
    private Button myButton;

    void Start()
    {
        myButton = GetComponent<Button>();
        myButton.onClick.AddListener(OnBuyClicked);
    }

    void OnBuyClicked()
    {
        Debug.Log("<color=green>[Payment] User purchased 5 Hints!</color>");
        
        // 1. Add the hints to the save file
        if (PatternMemoryGame.I != null)
        {
            PatternMemoryGame.I.AddPurchasedHints(5);
        }

        // 2. The PatternMemoryGame will handle closing the panel and unpausing
    }
}