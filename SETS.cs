using UnityEngine;
using TMPro;
using UnityEngine.Purchasing;

public class RestoreFeedback : MonoBehaviour
{
    public TextMeshProUGUI feedbackText; // Drag your 'version text' or a new text here

    // Link this to the IAP Button's "On Transactions Restored" event
    public void OnRestoreComplete(bool success, string error)
    {
        if (success)
        {
            feedbackText.text = "Purchases Restored!";
            Debug.Log("Restore Successful");
        }
        else
        {
            feedbackText.text = "Restore Failed: " + error;
        }
        
        // Clear the message after 3 seconds
        Invoke(nameof(ResetText), 3f);
    }

    void ResetText()
    {
        feedbackText.text = ""; // Or reset to original text
    }
}