using UnityEngine;

public class CylinderConnector : MonoBehaviour
{
    public Transform sphere1; // Drag the sphere to connect to in Inspector
    public Transform sphere2; // Optional: Second sphere if connecting two points
    
    void Update()
    {
        // If attached to a single sphere, simply follow its position
        if (sphere2 == null)
        {
            transform.position = sphere1.position;
            // Add offset if needed, e.g., transform.position = sphere1.position + Vector3.up * 2;
        }
        // If connecting two spheres, position/scale/rotate between them
        else
        {
            Vector3 dir = sphere2.position - sphere1.position;
            float distance = dir.magnitude;
            
            // Position cylinder midpoint between spheres
            transform.position = sphere1.position + dir * 0.5f;
            
            // Rotate cylinder to align with the direction between spheres
            transform.rotation = Quaternion.LookRotation(dir.normalized) * Quaternion.Euler(90, 0, 0);
            
            // Scale cylinder length to match distance between spheres (default cylinder height=2 units)
            transform.localScale = new Vector3(
                transform.localScale.x, 
                distance / 2, // Compensate for default height
                transform.localScale.z
            );
        }
    }
}