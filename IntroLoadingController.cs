// IntroLoadingController.cs
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video; // optional: if you use VideoPlayer

public class IntroLoadingController : MonoBehaviour
{
    [Header("References")]
    public GameObject introPanel;      // root panel for the loading screen
    public Image loadingImage;         // UI Image to show frames (sprite method)
    public VideoPlayer loadingVideo;   // optional: if using VideoPlayer instead of frames
    public GameObject shopPanel;       // panel to show after loading (set inactive initially)
    public PatternMemoryGame patternGame; // reference to your main game script

    [Header("Sprite-frame animation settings (preferred)")]
    public List<Sprite> loadingFrames = new List<Sprite>();
    public float framesPerSecond = 12f;
    public float minShowTime = 1.2f;   // show loading screen at least this long (seconds)

    [Header("Fallback / options")]
    public bool useVideoInsteadOfFrames = false;
    public bool autoShowShopWhenDone = true;

    private Coroutine animCoroutine;

    void OnEnable()
    {
        // Ensure panels in expected state
        if (introPanel != null) introPanel.SetActive(true);
        if (shopPanel != null) shopPanel.SetActive(false);

        // start animation
        if (useVideoInsteadOfFrames && loadingVideo != null)
        {
            animCoroutine = StartCoroutine(PlayVideoThenShowShop());
        }
        else
        {
            animCoroutine = StartCoroutine(PlayFramesThenShowShop());
        }
    }

    IEnumerator PlayFramesThenShowShop()
    {
        float elapsed = 0f;
        int frameCount = loadingFrames != null ? loadingFrames.Count : 0;
        if (frameCount == 0)
        {
            // no frames - just wait minShowTime
            yield return new WaitForSeconds(minShowTime);
            ShowShop();
            yield break;
        }

        int index = 0;
        float frameTime = 1f / Mathf.Max(0.001f, framesPerSecond);
        while (elapsed < minShowTime)
        {
            // set image sprite
            if (loadingImage != null)
            {
                loadingImage.sprite = loadingFrames[index % frameCount];
                // optional: loadingImage.SetNativeSize();
            }

            yield return new WaitForSeconds(frameTime);
            elapsed += frameTime;
            index++;
        }

        // small extra loop to give it a smooth end (optional)
        yield return new WaitForSeconds(0.15f);

        ShowShop();
    }

    IEnumerator PlayVideoThenShowShop()
    {
        if (loadingVideo == null)
        {
            ShowShop();
            yield break;
        }

        loadingVideo.Play();
        float start = Time.unscaledTime;
        // ensure video plays at least minShowTime
        while (loadingVideo.isPlaying == false || Time.unscaledTime - start < minShowTime)
        {
            yield return null;
        }

        // optionally stop the video
        loadingVideo.Stop();
        ShowShop();
    }

    // ShowShop: safely activates the shop root and ensures MainContent/ShopContent children are in known states
   void ShowShop()
{
    if (introPanel != null) 
        introPanel.SetActive(false);
    
    if (shopPanel != null)
    {
        shopPanel.SetActive(true);
        
        // Ensure only MainContent is active, ShopContent is inactive
        var mainContent = shopPanel.transform.Find("MainContent");
        var shopContent = shopPanel.transform.Find("ShopContent");
        
        if (mainContent != null) 
            mainContent.gameObject.SetActive(true);
        if (shopContent != null) 
            shopContent.gameObject.SetActive(false);
        
        // Refresh main menu UI
        var mainMenuController = mainContent?.GetComponent<MainMenuController>();
        if (mainMenuController != null)
        {
            mainMenuController.UpdateCurrencyUI();
            mainMenuController.UpdateLevelUI();
        }
    }
}

    // optional: call to cancel (if you want a skip)
    public void SkipIntroAndShowShop()
    {
        if (animCoroutine != null) StopCoroutine(animCoroutine);
        ShowShop();
    }
}
