using UnityEngine;

public class PivotBasedGrowth : MonoBehaviour
{
    [Header("Growth Settings")]
    public float growthSpeed = 5.0f; // Much faster for testing
    public float maxSize = 5f;
    public string targetSphereName = "Sphere9";
    
    [Header("Pivot Configuration")]
    public Transform pivotPoint; // Assign this in Inspector!
    public enum GrowthAxis { X, Y, Z }
    public GrowthAxis growthAxis = GrowthAxis.Y;
    public bool growPositiveDirection = true;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    public Color debugRayColor = Color.green;

    private Vector3 initialScale;
    private Vector3 initialPosition;
    private bool isGrowing = false;
    private float currentGrowth = 0f;

    void Start()
    {
        // Store initial values
        initialScale = transform.localScale;
        initialPosition = transform.position;
        
        Debug.Log($"{gameObject.name} growth system initialized.");
    }

    void Update()
    {
        if (isGrowing)
        {
            GrowFromPivot();
        }
        
        // Debug visualization
        if (showDebugInfo && pivotPoint != null)
        {
            Debug.DrawRay(pivotPoint.position, GetGrowthDirection() * 2f, debugRayColor);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name == targetSphereName && !isGrowing)
        {
            Debug.Log($"Collision with {other.name}. Starting growth.");
            isGrowing = true;
            currentGrowth = 0f; // Reset growth
        }
    }

    Vector3 GetGrowthDirection()
    {
        if (pivotPoint == null) return Vector3.zero;
        
        switch (growthAxis)
        {
            case GrowthAxis.X:
                return growPositiveDirection ? Vector3.right : Vector3.left;
            case GrowthAxis.Y:
                return growPositiveDirection ? Vector3.up : Vector3.down;
            case GrowthAxis.Z:
                return growPositiveDirection ? Vector3.forward : Vector3.back;
            default:
                return Vector3.zero;
        }
    }

    void GrowFromPivot()
    {
        if (pivotPoint == null)
        {
            Debug.LogError("Pivot point is null!");
            return;
        }

        float growthThisFrame = growthSpeed * Time.deltaTime;
        currentGrowth += growthThisFrame;
        
        if (showDebugInfo)
        {
            Debug.Log($"Growing: {currentGrowth}/{maxSize}");
        }
        
        // Calculate and apply new scale
        Vector3 newScale = initialScale;
        switch (growthAxis)
        {
            case GrowthAxis.X:
                newScale.x = initialScale.x + currentGrowth;
                break;
            case GrowthAxis.Y:
                newScale.y = initialScale.y + currentGrowth;
                break;
            case GrowthAxis.Z:
                newScale.z = initialScale.z + currentGrowth;
                break;
        }
        transform.localScale = newScale;
        
        // Calculate and apply position offset
        Vector3 growthDirection = GetGrowthDirection();
        Vector3 growthOffset = growthDirection * (currentGrowth / 2f);
        transform.position = initialPosition + growthOffset;
        
        // Debug visualization
        if (showDebugInfo)
        {
            Debug.DrawLine(initialPosition, transform.position, Color.red, 0.1f);
            Debug.DrawRay(transform.position, growthDirection * 1f, Color.blue, 0.1f);
        }
        
        // Check if we've reached max size
        if (currentGrowth >= maxSize)
        {
            isGrowing = false;
            if (showDebugInfo) Debug.Log("Growth completed");
        }
    }

    [ContextMenu("Reset Growth")]
    public void ResetGrowth()
    {
        isGrowing = false;
        currentGrowth = 0f;
        transform.localScale = initialScale;
        transform.position = initialPosition;
    }

    [ContextMenu("Start Growth Manually")]
    public void StartGrowthManual()
    {
        if (!isGrowing)
        {
            isGrowing = true;
            currentGrowth = 0f;
            Debug.Log("Manual growth started");
        }
    }

    void OnDrawGizmos()
    {
        if (!showDebugInfo) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, 0.1f);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(transform.position, transform.lossyScale);
    }
}