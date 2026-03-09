using UnityEngine;


/*
public class TouchDetection : MonoBehaviour
{
    void Update()
    {
        // Check if there is at least one touch on the screen
        if (Input.touchCount > 0)
        {
            // Get the first touch
            Touch touch = Input.GetTouch(0);

            // Create a ray from the camera to the touch position
            Ray ray = Camera.main.ScreenPointToRay(touch.position);
            RaycastHit hit;

            // Check if the ray hits the collider of this object
            if (Physics.Raycast(ray, out hit))
            {
                // Check if the hit object is this sphere
                if (hit.transform == transform)
                {
                    // Execute your logic here
                    Debug.Log("Sphere touched!");
                }
            }
        }
    }
}
*/

public class ChangeColorOnClick : MonoBehaviour
{
    private void OnMouseDown()
    {
        // Change the color to a random color
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
    }
}
