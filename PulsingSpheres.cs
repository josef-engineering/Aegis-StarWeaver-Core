using UnityEngine;

public class PulseEffect : MonoBehaviour
{
    public float pulseSpeed = 2f;      // Speed of the pulsing (how fast it grows/shrinks)
    public float pulseAmount = 1.5f;   // Amount of scaling (how large it gets, increased)
    public Vector3 originalScale;     // The original scale of the sphere
    public GameObject particleEffectPrefab;  // Reference to your particle effect prefab
    private GameObject particleEffect;  // The instantiated particle effect

    void Start()
    {
        // Save the original scale to ensure it pulses around its original size
        originalScale = transform.localScale;
    }

    void Update()
    {
        // Calculate the new scale based on a sine wave (more drastic change)
        float scaleFactor = Mathf.Sin(Time.time * pulseSpeed) * pulseAmount + 1;

        // Apply the pulsing scale factor (drastic scaling)
        transform.localScale = originalScale * scaleFactor;
    }

    void OnMouseDown()
    {
        // When the sphere is clicked, instantiate the particle effect
        if (particleEffectPrefab != null)
        {
            particleEffect = Instantiate(particleEffectPrefab, transform.position, Quaternion.identity);
            particleEffect.transform.SetParent(transform);  // Parent it to the sphere for better control

            // Destroy the particle effect after 2 seconds to clean up memory
            Destroy(particleEffect, 2f);
        }
    }
}
