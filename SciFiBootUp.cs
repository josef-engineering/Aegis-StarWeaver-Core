using UnityEngine;
using UnityEngine.UI; 
using System.Collections;
using TMPro; 

public class SciFiBootUp : MonoBehaviour
{
    [Header("Target Objects")]
    [Tooltip("The object that spins (Ring/Graphic).")]
    public Transform rotatorObject;
    
    [Tooltip("The Image or Text to color shift. If empty, tries to find one on this object.")]
    public Graphic targetGraphic; 

    [Header("Timing")]
    public float startDelay = 1.0f;     
    public float glitchDuration = 0.8f; 
    public float stayVisibleDuration = 10.0f; // NEW: How long it stays before vanishing

    [Header("Glitch Settings")]
    public Vector3 finalScale = Vector3.one;
    public Color glitchColor = Color.white; 
    [Range(0f, 1f)] public float minAlpha = 0.2f; 

    [Header("Sniper Rotation")]
    public float calibrationAngle = 60f;
    public float lockSpeed = 0.15f;      

    private Color originalColor;
    private CanvasGroup canvasGroup;

    private void OnEnable()
    {
        // 1. Setup Rotator
        if (rotatorObject == null) rotatorObject = transform;

        // 2. Setup Color Target
        if (targetGraphic == null) targetGraphic = GetComponent<Graphic>();
        if (targetGraphic != null) originalColor = targetGraphic.color;

        // 3. Setup Alpha (We need a CanvasGroup to fade everything out)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = gameObject.AddComponent<CanvasGroup>();

        // Ensure we are visible at start
        canvasGroup.alpha = 1f;

        // Restart sequence
        StopAllCoroutines();
        StartCoroutine(RebootSequence());
    }

    IEnumerator RebootSequence()
    {
        // --- STEP 1: SYSTEM OFFLINE ---
        transform.localScale = Vector3.zero;
        canvasGroup.alpha = 0f; 
        rotatorObject.localRotation = Quaternion.Euler(0, 0, Random.Range(-180f, 180f));
        
        yield return new WaitForSeconds(startDelay);

        // --- STEP 2: CALIBRATING (The Glitch) ---
        float timer = 0f;
        while (timer < glitchDuration)
        {
            // A. Chaotic Scale
            float randomX = Random.Range(0.8f, 1.1f) * finalScale.x;
            float randomY = Random.Range(0.1f, 1.2f) * finalScale.y;
            transform.localScale = new Vector3(randomX, randomY, 1f);

            // B. Chaotic Rotation
            float randomZ = Random.Range(-calibrationAngle, calibrationAngle);
            rotatorObject.localRotation = Quaternion.Euler(0, 0, randomZ);

            // C. Alpha Glitch
            canvasGroup.alpha = Random.Range(minAlpha, 1f);

            // D. Color Glitch
            if (targetGraphic != null)
            {
                bool flash = Random.value > 0.7f; 
                targetGraphic.color = flash ? glitchColor : originalColor;
            }

            float step = Random.Range(0.05f, 0.1f);
            timer += step;
            yield return new WaitForSeconds(step);
        }

        // --- STEP 3: SNIPER LOCK-ON ---
        canvasGroup.alpha = 1f;
        if (targetGraphic != null) targetGraphic.color = originalColor;
        transform.localScale = finalScale;

        // Rotate LEFT
        yield return StartCoroutine(SmoothRotate(calibrationAngle * 0.5f, lockSpeed));

        // Rotate RIGHT
        yield return StartCoroutine(SmoothRotate(-calibrationAngle * 0.25f, lockSpeed));

        // LOCK CENTER
        yield return StartCoroutine(SmoothRotate(0f, lockSpeed * 0.8f));

        // Final Assurance
        transform.localScale = finalScale;
        rotatorObject.localRotation = Quaternion.identity;

        // --- STEP 4: WAIT AND VANISH (NEW) ---
        yield return new WaitForSeconds(stayVisibleDuration);

        // Fade out smoothly over 1 second
        float fadeDuration = 1.0f;
        float fadeTimer = 0f;
        while (fadeTimer < fadeDuration)
        {
            fadeTimer += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, fadeTimer / fadeDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f; // Make it completely invisible
    }

    IEnumerator SmoothRotate(float targetZ, float duration)
    {
        Quaternion startRot = rotatorObject.localRotation;
        Quaternion endRot = Quaternion.Euler(0, 0, targetZ);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            t = Mathf.SmoothStep(0f, 1f, t); 
            rotatorObject.localRotation = Quaternion.Lerp(startRot, endRot, t);
            yield return null;
        }
        rotatorObject.localRotation = endRot;
    }
}