// LifecycleLogger.cs
using UnityEngine;
using System.Diagnostics;

[DisallowMultipleComponent]
public class LifecycleLogger : MonoBehaviour
{
    public bool logDisable = true;
    public bool logDestroy = true;

    void OnDisable()
    {
        if (!logDisable) return;
        UnityEngine.Debug.Log($"[LifecycleLogger] OnDisable -> {name} (activeInHierarchy: {gameObject.activeInHierarchy})");
    }

    void OnDestroy()
    {
        if (!logDestroy) return;
        // Use System.Diagnostics to capture the stack trace of the destroy caller
        var st = new StackTrace(true);
        UnityEngine.Debug.LogWarning($"[LifecycleLogger] OnDestroy -> {name}\nStackTrace:\n{st}");
    }
}
