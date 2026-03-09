using UnityEngine;

public class StartupDisabler : MonoBehaviour
{
    [Tooltip("Drag components (MonoBehaviour) here to disable at Awake for fast bisecting.")]
    public MonoBehaviour[] toDisable;

    void Awake()
    {
        foreach (var m in toDisable)
        {
            if (m == null) continue;
            m.enabled = false;
            Debug.Log("[StartupDisabler] Disabled: " + m.GetType().Name + " on " + m.gameObject.name);
        }
    }
}
