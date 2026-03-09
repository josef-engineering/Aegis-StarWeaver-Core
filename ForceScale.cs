using UnityEngine;
using System.Collections; 

public class ForceScale : MonoBehaviour
{
    // Adjust these values until the model looks correct in your ModelViewCamera
    
    // Scale: Set this much larger than 14.4 (e.g., 50.0) to ensure the model fills the preview.
    private readonly Vector3 TargetScale = new Vector3(50.0f, 50.0f, 50.0f); 
    
    // Position: Move the object to the center of the world (0, 0) and slightly forward (Z=15) 
    // to place it in front of the camera, preventing clipping.
    private readonly Vector3 TargetPosition = new Vector3(0f, 0f, 15f); 

    void Start()
    {
        // Start a coroutine to ensure this code runs AFTER the ModelViewController's initialization attempts, 
        // effectively overriding any scaling or positioning failures.
        StartCoroutine(ApplyFinalTransform());
    }

    private IEnumerator ApplyFinalTransform()
    {
        // Wait until the end of the frame. This ensures all other scripts (like ModelViewController's Awake/Start) 
        // have executed their logic for this frame.
        yield return new WaitForEndOfFrame(); 

        // 1. FORCE SCALE: Overrides any tiny scale set by the ModelViewController's failed normalization logic.
        transform.localScale = TargetScale;
        
        // 2. FORCE POSITION: Clears the parent and sets the position relative to the world origin.
        // This ensures the model is placed correctly, regardless of the failed ModelViewController positioning.
        transform.SetParent(null); 
        transform.position = TargetPosition; 
        
        // 3. FORCE ROTATION: Clears any rotation that might be making the spherical model appear stretched or flat.
        transform.localRotation = Quaternion.identity; // Sets rotation to (0, 0, 0)
        
        // Optional: If the ModelViewController requires the object to be parented to 'ModelWorldRoot',
        // you can re-parent it here, but only after applying scale/position.
        // transform.SetParent(GameObject.Find("ModelWorldRoot")?.transform); 

        Debug.Log($"[ForceScale] Applied final scale ({TargetScale.x}), position, and rotation.");
    }
}