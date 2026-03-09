
/*


// DraggableSphere.cs
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Collider))]
public class DraggableSphere : MonoBehaviour
{
    [Tooltip("How long (seconds) it takes to return to the original position. Set to 0 to snap back.")]
    public float returnDuration = 0.5f;

    [Tooltip("Curve to control the easing of the return motion.")]
    public AnimationCurve returnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Vector3 originalPosition;
    Camera cam;
    bool dragging = false;
    Plane dragPlane;
    Vector3 grabOffset;
    Coroutine returnCoroutine;

    void Start()
    {
        originalPosition = transform.position;
        cam = Camera.main;
        if (cam == null)
        {
            cam = FindObjectOfType<Camera>();
            if (cam == null)
                Debug.LogError("No Camera found in scene. DraggableSphere requires a Camera.");
        }
    }

    void OnMouseDown()
    {
        // Cancel any running return
        if (returnCoroutine != null)
        {
            StopCoroutine(returnCoroutine);
            returnCoroutine = null;
        }

        dragging = true;

        // Create a plane that faces the camera and passes through the object's current position.
        // This keeps dragging at the object's depth relative to the camera.
        dragPlane = new Plane(cam.transform.forward, transform.position);

        // Find the point on the plane where the click ray hit to compute offset
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (dragPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            grabOffset = transform.position - hitPoint;
        }
        else
        {
            grabOffset = Vector3.zero;
        }
    }

    void Update()
    {
        if (dragging)
        {
            // Move with the mouse projected onto the dragPlane
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (dragPlane.Raycast(ray, out float enter))
            {
                Vector3 hitPoint = ray.GetPoint(enter);
                transform.position = hitPoint + grabOffset;
            }

            // If mouse released (robust even if mouse up happens off the collider)
            if (!Input.GetMouseButton(0))
            {
                EndDragAndReturn();
            }
        }
    }

    void OnMouseUp()
    {
        // OnMouseUp might not always be called if mouse is released off-camera/etc.
        // The Update check above covers that, but keep OnMouseUp to be safe.
        if (dragging)
            EndDragAndReturn();
    }

    void EndDragAndReturn()
    {
        dragging = false;

        if (returnDuration <= 0f)
        {
            transform.position = originalPosition;
            return;
        }

        if (returnCoroutine != null)
            StopCoroutine(returnCoroutine);
        returnCoroutine = StartCoroutine(ReturnToOriginal());
    }

    IEnumerator ReturnToOriginal()
    {
        Vector3 startPos = transform.position;
        float t = 0f;
        while (t < returnDuration)
        {
            t += Time.deltaTime;
            float eval = returnCurve.Evaluate(Mathf.Clamp01(t / returnDuration));
            transform.position = Vector3.Lerp(startPos, originalPosition, eval);
            yield return null;
        }
        transform.position = originalPosition;
        returnCoroutine = null;
    }

    // optional: reset original position in editor or during runtime if you want
    public void SetCurrentAsOriginal()
    {
        originalPosition = transform.position;
    }
}
*/