// UnityFindHelpers.cs
// Cross-version helpers to replace deprecated FindObject(s) API calls
using UnityEngine;

public static class UnityFindHelpers
{
    // Find any single object of type T (prefer faster new API when available)
    public static T FindAny<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        // returns any instance or null
        return Object.FindAnyObjectByType<T>();
#elif UNITY_2021_2_OR_NEWER
        // newer versions may have FindFirstObjectByType
        try { return Object.FindFirstObjectByType<T>(); } catch { }
        return Object.FindObjectOfType<T>();
#else
        return Object.FindObjectOfType<T>();
#endif
    }

    // Find first object (semantic alias)
    public static T FindFirst<T>() where T : Object => FindAny<T>();

    // Find all objects of type T. includeInactive controls whether inactive objects are included.
    public static T[] FindAll<T>(bool includeInactive = false) where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        var include = includeInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude;
        // Use sort mode None for performance unless you specifically need InstanceID ordering
        return Object.FindObjectsByType<T>(include, FindObjectsSortMode.None);
#elif UNITY_2021_2_OR_NEWER
        // Older API: use overload if available, otherwise fall back
        try
        {
            return Object.FindObjectsOfType<T>(includeInactive);
        }
        catch
        {
            return Object.FindObjectsOfType<T>();
        }
#else
        return Object.FindObjectsOfType<T>();
#endif
    }
}
