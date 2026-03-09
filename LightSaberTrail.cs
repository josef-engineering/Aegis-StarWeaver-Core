using UnityEngine;

public class LightSaberTrail : MonoBehaviour
{
    [Header("Trail Prefab (has a TrailRenderer)")]
    public GameObject trailPrefab;
    public float followSmoothing = 0.0f; // 0 = immediate, >0 = interpolation
    public Color tint = Color.white;

    private GameObject activeTrail;
    private TrailRenderer trailRenderer;
    private Vector3 lastPos;

    // Start the trail at a world position
    public void StartTrail(Vector3 worldPos)
    {
        if (trailPrefab == null) return;
        if (activeTrail != null) Destroy(activeTrail);

        activeTrail = Instantiate(trailPrefab);
        activeTrail.name = "LS_ActiveTrail";
        trailRenderer = activeTrail.GetComponent<TrailRenderer>();
        if (trailRenderer != null)
        {
            // Set color if material supports _Color or _BaseColor
            Material mat = trailRenderer.material;
            if (mat.HasProperty("_Color"))
                mat.SetColor("_Color", tint);
            else if (mat.HasProperty("_BaseColor"))
                mat.SetColor("_BaseColor", tint);
            else if (mat.HasProperty("_EmissionColor"))
            {
                mat.SetColor("_EmissionColor", tint * 1f);
                mat.EnableKeyword("_EMISSION");
            }
        }
        activeTrail.transform.position = worldPos;
        lastPos = worldPos;
    }

    // Update trail cursor world position (call each frame while pointer down)
    public void SetPosition(Vector3 worldPos)
    {
        if (activeTrail == null) return;
        if (followSmoothing <= 0f) activeTrail.transform.position = worldPos;
        else activeTrail.transform.position = Vector3.Lerp(activeTrail.transform.position, worldPos, Time.unscaledDeltaTime * (1f / Mathf.Max(0.0001f, followSmoothing)));
        lastPos = worldPos;
    }

    // Stop and let the trail fade out (TrailRenderer.time controls fade)
    public void StopTrail(float destroyDelay = 0.35f)
    {
        if (activeTrail == null) return;
        // detach so it can fade out naturally
        if (activeTrail.transform.parent != null) activeTrail.transform.parent = null;
        // optionally stop emission, etc.
        var tr = activeTrail.GetComponent<TrailRenderer>();
        if (tr != null) tr.emitting = false;
        // schedule destroy after a short delay so trail has time to vanish
        Destroy(activeTrail, Mathf.Max(0.1f, destroyDelay));
        activeTrail = null;
        trailRenderer = null;
    }

    // Convenience: set tint color at runtime
    public void SetTint(Color c)
    {
        tint = c;
        if (trailRenderer != null)
        {
            var mat = trailRenderer.material;
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", c);
            else if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", c);
            else if (mat.HasProperty("_EmissionColor")) mat.SetColor("_EmissionColor", c);
        }
    }
}
