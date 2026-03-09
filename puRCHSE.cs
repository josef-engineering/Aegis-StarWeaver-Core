using UnityEngine;
using UnityEngine.Purchasing;

// This script forces the buy button to work and listens for the result directly.
public class SimplePurchaser : MonoBehaviour
{
    public void ClickBuy5Hints()
    {
        // 1. Force the purchase request
        CodelessIAPStoreListener.Instance.InitiatePurchase("hints_05");
    }

    // 2. This magic function is called automatically by Unity IAP when ANY purchase succeeds
    public void OnPurchaseComplete(Product product)
    {
        if (product.definition.id == "hints_05")
        {
            Debug.Log("Purchase Verified: Giving 5 Hints");
            if (PatternMemoryGame.I != null)
            {
                PatternMemoryGame.I.Buy5HintsSuccess();
            }
        }
    }
    
    // 3. This catches failures so the button doesn't freeze
    public void OnPurchaseFailed(Product product, PurchaseFailureReason reason)
    {
        Debug.Log("Purchase Failed: " + reason);
    }
}