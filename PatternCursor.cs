using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Renderer))]
public class PatternCursor : MonoBehaviour
{
    public float idleScale = 0.18f;
    public float pulseScale = 0.36f;
    public float pulseTime = 0.28f;
    public Color pulseEmission = default;
    public bool useMaterialPropertyBlock = true;

    Renderer rend;
    MaterialPropertyBlock mpb;

    void Awake()
    {
        rend = GetComponent<Renderer>();
        if (useMaterialPropertyBlock) mpb = new MaterialPropertyBlock();
        transform.localScale = Vector3.one * idleScale;
        if (pulseEmission == default && rend != null && rend.sharedMaterial != null)
            pulseEmission = rend.sharedMaterial.GetColor("_EmissionColor");
    }

    // Simple pulse: scale up then back down while briefly boosting emission
    public void Pulse()
    {
        StopAllCoroutines();
        StartCoroutine(PulseCoroutine());
    }

    IEnumerator PulseCoroutine()
    {
        float half = pulseTime * 0.5f;
        // Grow
        float t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(0f, 1f, t / half);
            transform.localScale = Vector3.one * Mathf.Lerp(idleScale, pulseScale, p);
            if (useMaterialPropertyBlock && rend != null)
            {
                rend.GetPropertyBlock(mpb);
                mpb.SetColor("_EmissionColor", pulseEmission * (0.6f + 0.4f * p));
                rend.SetPropertyBlock(mpb);
            }
            yield return null;
        }
        // Shrink
        t = 0f;
        while (t < half)
        {
            t += Time.deltaTime;
            float p = Mathf.SmoothStep(1f, 0f, t / half);
            transform.localScale = Vector3.one * Mathf.Lerp(idleScale, pulseScale, p);
            if (useMaterialPropertyBlock && rend != null)
            {
                rend.GetPropertyBlock(mpb);
                mpb.SetColor("_EmissionColor", pulseEmission * (0.6f + 0.4f * p));
                rend.SetPropertyBlock(mpb);
            }
            yield return null;
        }

        transform.localScale = Vector3.one * idleScale;
        if (useMaterialPropertyBlock && rend != null)
        {
            rend.GetPropertyBlock(mpb);
            mpb.SetColor("_EmissionColor", Color.black);
            rend.SetPropertyBlock(mpb);
        }
    }

    // Utility: set emission color at runtime
    public void SetEmission(Color c)
    {
        if (rend == null) return;
        if (useMaterialPropertyBlock)
        {
            rend.GetPropertyBlock(mpb);
            mpb.SetColor("_EmissionColor", c);
            rend.SetPropertyBlock(mpb);
        }
        else
        {
            rend.material.SetColor("_EmissionColor", c);
        }
    }
}
