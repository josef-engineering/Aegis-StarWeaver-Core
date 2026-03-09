using UnityEngine;

public class BlackHoleUnstable : MonoBehaviour
{
    [Header("Instability Settings")]
    public float idleSpeed = 20f;
    public float surgeSpeed = 1200f; // Faster, sharper spin
    public float surgeDuration = 0.2f; // Much shorter bursts (like a glitch)
    
    [Header("Distortion (The Glitch)")]
    public float shakeAmount = 0.05f; // How violently it shakes position
    public float scaleJitter = 0.2f;  // How violently the size flickers

    [Header("Timing")]
    public float minTime = 2f;
    public float maxTime = 6f;

    private Vector3 baseScale;
    private Vector3 basePosition;
    private float currentSpeed;
    private float targetSpeed;
    private float surgeTimer;
    private bool isSurging = false;

    void Start()
    {
        baseScale = transform.localScale;
        basePosition = transform.localPosition;
        currentSpeed = idleSpeed;
        targetSpeed = idleSpeed;
        surgeTimer = Random.Range(minTime, maxTime);
    }

    void Update()
    {
        // 1. Random Timer Logic
        surgeTimer -= Time.deltaTime;
        if (surgeTimer <= 0)
        {
            StartCoroutine(DoGlitchSurge());
            surgeTimer = Random.Range(minTime, maxTime);
        }

        // 2. Spin Logic (Sharp snapping instead of smooth Lerp)
        // If surging, snap instantly to fast speed. If not, slowly decay.
        if (isSurging)
        {
            currentSpeed = surgeSpeed; 
        }
        else
        {
            currentSpeed = Mathf.Lerp(currentSpeed, idleSpeed, 2f * Time.deltaTime);
        }
        
        transform.Rotate(0, currentSpeed * Time.deltaTime, 0);

        // 3. The "Horror" Distortion
        if (isSurging)
        {
            // Vibrate Position (Like an earthquake)
            transform.localPosition = basePosition + (Random.insideUnitSphere * shakeAmount);

            // Flicker Scale (Like a failing lightbulb)
            float flicker = 1f + Random.Range(-scaleJitter, scaleJitter);
            transform.localScale = baseScale * flicker;
        }
        else
        {
            // Return to normal smoothly
            transform.localPosition = Vector3.Lerp(transform.localPosition, basePosition, 10f * Time.deltaTime);
            transform.localScale = Vector3.Lerp(transform.localScale, baseScale, 5f * Time.deltaTime);
        }
    }

    System.Collections.IEnumerator DoGlitchSurge()
    {
        isSurging = true;
        // The surge is short and violent
        yield return new WaitForSeconds(surgeDuration);
        isSurging = false;
    }
}