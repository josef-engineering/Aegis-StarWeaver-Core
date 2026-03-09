using UnityEngine;

public class AutoRotate : MonoBehaviour
{
    [Header("Spin Settings")]
    public Vector3 rotationAxis = Vector3.up; // Spins around Y (Height)
    public float rotationSpeed = 100f;        // How fast it spins

    void Update()
    {
        // Rotates the object constantly every frame
        transform.Rotate(rotationAxis * rotationSpeed * Time.deltaTime);
    }
}