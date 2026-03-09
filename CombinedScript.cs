using UnityEngine;

public class SmartCylinder : MonoBehaviour
{
    [Header("Connection Settings")]
    public Transform sphere1;
    public Transform sphere2;
    
    [Header("Growth Settings")]
    public float growthSpeed = 5.0f;
    public float maxSize = 5f;
    public string targetSphereName = "Sphere9";
    public Transform pivotPoint;
    public enum GrowthAxis { X, Y, Z }
    public GrowthAxis growthAxis = GrowthAxis.Y;
    public bool growPositiveDirection = true;
    
    [Header("Debug")]
    public bool showDebugInfo = true;
    
    // Private variables
    private Vector3 initialScale;
    private Vector3 initialPosition;
    private bool isGrowing = false;
    private float currentGrowth = 0f;
    private Vector3 initialPivotPosition;
    private bool isConnected = true;
    private Vector3 growthDirection;

    void Start()
    {
        initialScale = transform.localScale;
        initialPosition = transform.position;
        
        if (pivotPoint == null)
        {
            pivotPoint = transform.parent;
        }
        
        if (pivotPoint != null)
        {
            initialPivotPosition = pivotPoint.position;
        }
        
        // Pre-calculate the growth direction
        CalculateGrowthDirection();
    }

    void Update()
    {
        if (isConnected)
        {
            UpdateConnection();
        }
        
        if (isGrowing)
        {
            UpdateGrowth();
        }
    }

    void UpdateConnection()
    {
        if (sphere2 == null && sphere1 != null)
        {
            transform.position = sphere1.position;
        }
        else if (sphere1 != null && sphere2 != null)
        {
            Vector3 dir = sphere2.position - sphere1.position;
            float distance = dir.magnitude;
            
            transform.position = sphere1.position + dir * 0.5f;
            transform.rotation = Quaternion.LookRotation(dir.normalized) * Quaternion.Euler(90, 0, 0);
            transform.localScale = new Vector3(
                transform.localScale.x, 
                distance / 2,
                transform.localScale.z
            );
        }
    }

    void CalculateGrowthDirection()
    {
        switch (growthAxis)
        {
            case GrowthAxis.X:
                growthDirection = growPositiveDirection ? Vector3.right : Vector3.left;
                break;
            case GrowthAxis.Y:
                growthDirection = growPositiveDirection ? Vector3.up : Vector3.down;
                break;
            case GrowthAxis.Z:
                growthDirection = growPositiveDirection ? Vector3.forward : Vector3.back;
                break;
        }
        
        // Convert to world space if we have a pivot
        if (pivotPoint != null)
        {
            growthDirection = pivotPoint.TransformDirection(growthDirection);
        }
    }

    void UpdateGrowth()
    {
        if (pivotPoint == null) return;

        float growthThisFrame = growthSpeed * Time.deltaTime;
        currentGrowth += growthThisFrame;
        
        // Disconnect from spheres during growth
        if (isConnected && currentGrowth > 0.1f)
        {
            isConnected = false;
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
        
        // Calculate and apply position offset - FIXED to prevent Y transfer
        // Only move along the intended growth axis
        Vector3 growthOffset = growthDirection * (currentGrowth / 2f);
        
        // Preserve the Y position (or other axes) that shouldn't change
        Vector3 newPosition = initialPivotPosition + growthOffset;
        
        // If you want to prevent Y movement completely, uncomment this line:
         newPosition.y = initialPosition.y;
        
        transform.position = newPosition;
        
        // Check if we've reached max size
        if (currentGrowth >= maxSize)
        {
            isGrowing = false;
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name == targetSphereName && !isGrowing)
        {
            Debug.Log($"Collision with {other.name}. Starting growth.");
            isGrowing = true;
            if (pivotPoint != null)
            {
                initialPivotPosition = pivotPoint.position;
            }
            // Recalculate growth direction in case something changed
            CalculateGrowthDirection();
        }
    }

    [ContextMenu("Reset Growth")]
    public void ResetGrowth()
    {
        isGrowing = false;
        isConnected = true;
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
            if (pivotPoint != null)
            {
                initialPivotPosition = pivotPoint.position;
            }
            CalculateGrowthDirection();
        }
    }
}