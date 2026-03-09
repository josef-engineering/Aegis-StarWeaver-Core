using UnityEngine;
using System.Collections;

public class SingleSideGrowth : MonoBehaviour
{
    public enum GrowthDirection
    {
        PositiveX,
        NegativeX,
        PositiveY,
        NegativeY,
        PositiveZ,
        NegativeZ
    }

    [Header("Growth Settings")]
    public GrowthDirection growthDirection = GrowthDirection.PositiveX;
    public float growthSpeed = 0.5f;
    public float maxSize = 5f;
    public string targetSphereName = "Sphere9";
    
    [Header("Debug")]
    public bool showDebugRays = true;
    public Color debugRayColor = Color.green;

    private Vector3 initialScale;
    private Vector3 initialPosition;
    private bool isGrowing = false;
    private float currentGrowth = 0f;
    private Vector3 pivotPoint;
    private Vector3 growthAxis;

    void Start()
    {
        initialScale = transform.localScale;
        initialPosition = transform.position;
        
        // Calculate the pivot point based on the selected growth direction
        CalculatePivotPoint();
        
        Debug.Log($"{gameObject.name} initialized. Pivot at: {pivotPoint}");
    }

    void CalculatePivotPoint()
    {
        // Determine the growth axis
        switch (growthDirection)
        {
            case GrowthDirection.PositiveX:
                growthAxis = Vector3.right;
                pivotPoint = transform.position - transform.right * (initialScale.x / 2f);
                break;
            case GrowthDirection.NegativeX:
                growthAxis = Vector3.left;
                pivotPoint = transform.position + transform.right * (initialScale.x / 2f);
                break;
            case GrowthDirection.PositiveY:
                growthAxis = Vector3.up;
                pivotPoint = transform.position - transform.up * (initialScale.y / 2f);
                break;
            case GrowthDirection.NegativeY:
                growthAxis = Vector3.down;
                pivotPoint = transform.position + transform.up * (initialScale.y / 2f);
                break;
            case GrowthDirection.PositiveZ:
                growthAxis = Vector3.forward;
                pivotPoint = transform.position - transform.forward * (initialScale.z / 2f);
                break;
            case GrowthDirection.NegativeZ:
                growthAxis = Vector3.back;
                pivotPoint = transform.position + transform.forward * (initialScale.z / 2f);
                break;
        }
    }

    void Update()
    {
        if (showDebugRays)
        {
            // Draw debug ray showing growth direction
            Debug.DrawRay(pivotPoint, transform.TransformDirection(growthAxis) * 2f, debugRayColor);
            
            // Draw debug sphere at pivot point
            Debug.DrawLine(pivotPoint - Vector3.up * 0.1f, pivotPoint + Vector3.up * 0.1f, debugRayColor);
            Debug.DrawLine(pivotPoint - Vector3.right * 0.1f, pivotPoint + Vector3.right * 0.1f, debugRayColor);
            Debug.DrawLine(pivotPoint - Vector3.forward * 0.1f, pivotPoint + Vector3.forward * 0.1f, debugRayColor);
        }

        if (isGrowing)
        {
            GrowFromPivot();
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.name == targetSphereName && !isGrowing)
        {
            Debug.Log($"Collision with {other.name}. Starting growth.");
            isGrowing = true;
        }
    }

    void GrowFromPivot()
    {
        // Calculate growth amount for this frame
        float growthThisFrame = growthSpeed * Time.deltaTime;
        currentGrowth += growthThisFrame;
        
        // Limit growth to max size
        if (currentGrowth > maxSize)
        {
            currentGrowth = maxSize;
            isGrowing = false;
        }
        
        // Determine which axis to scale
        Vector3 newScale = initialScale;
        if (growthDirection == GrowthDirection.PositiveX || growthDirection == GrowthDirection.NegativeX)
        {
            newScale.x = initialScale.x + currentGrowth;
        }
        else if (growthDirection == GrowthDirection.PositiveY || growthDirection == GrowthDirection.NegativeY)
        {
            newScale.y = initialScale.y + currentGrowth;
        }
        else
        {
            newScale.z = initialScale.z + currentGrowth;
        }
        
        // Apply new scale
        transform.localScale = newScale;
        
        // Calculate new position to keep the pivot point fixed
        Vector3 growthVector = transform.TransformDirection(growthAxis) * (currentGrowth / 2f);
        transform.position = pivotPoint + growthVector;
    }

    [ContextMenu("Reset Growth")]
    public void ResetGrowth()
    {
        isGrowing = false;
        currentGrowth = 0f;
        transform.localScale = initialScale;
        transform.position = initialPosition;
        CalculatePivotPoint();
    }

    [ContextMenu("Start Growth Manually")]
    public void StartGrowthManual()
    {
        if (!isGrowing)
        {
            isGrowing = true;
        }
    }
}