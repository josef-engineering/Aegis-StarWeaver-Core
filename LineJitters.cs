using UnityEngine;

public class LaserFlow : MonoBehaviour
{
    public LineRenderer lineRenderer;
    public float scrollSpeed = 10.0f; // High speed for electricity!
    public float pulseSpeed = 5.0f;

    void Update()
    {
        // 1. Scroll the texture to make it flow
        float offset = Time.time * scrollSpeed;
        lineRenderer.material.mainTextureOffset = new Vector2(offset, 0);

        // 2. (Optional) Jitter the width slightly to make it "crackle"
        float noise = Mathf.PerlinNoise(Time.time * pulseSpeed, 0);
        lineRenderer.widthMultiplier = 0.4f + (noise * 0.2f); 
    }
}