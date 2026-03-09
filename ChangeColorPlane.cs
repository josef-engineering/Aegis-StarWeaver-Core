using System.Collections;
using UnityEngine;

public class DiscoPlane : MonoBehaviour
{
    // Neon colors array
    private Color[] neonColors = new Color[]
    {
        new Color(0.8f, 0.1f, 0.1f), // Neon Red
        new Color(0.1f, 0.8f, 0.1f), // Neon Green
        new Color(0.1f, 0.1f, 0.8f), // Neon Blue
        new Color(0.9f, 0.6f, 0.1f), // Neon Yellow
        new Color(0.6f, 0.1f, 0.9f), // Neon Purple
        new Color(0.1f, 0.8f, 0.9f), // Neon Cyan
        new Color(0.8f, 0.4f, 0.9f)  // Neon Pink
    };

    // Time interval range in seconds (min, max)
    public float minInterval = 0.5f;
    public float maxInterval = 2.0f;

    // Renderer to change the material color
    private Renderer planeRenderer;

    void Start()
    {
        planeRenderer = GetComponent<Renderer>();
        // Start the color change routine
        StartCoroutine(ChangeColorRoutine());
    }

    // Coroutine to change color at random intervals
    IEnumerator ChangeColorRoutine()
    {
        while (true)
        {
            // Randomly choose a neon color
            Color randomColor = neonColors[Random.Range(0, neonColors.Length)];
            // Change the color of the plane
            planeRenderer.material.color = randomColor;

            // Wait for a random time interval before changing the color again
            float randomTime = Random.Range(minInterval, maxInterval);
            yield return new WaitForSeconds(randomTime);
        }
    }
}
