using UnityEngine;
using System.Collections.Generic;

public class AndroidPatternLock : MonoBehaviour
{
    [Header("Pattern Settings")]
    public float nodeRadius = 0.5f;
    public float lineWidth = 0.1f;
    public Material lineMaterial;
    public Color activeColor = Color.blue;
    public Color normalColor = Color.white;

    [Header("Pattern Nodes")]
    public List<Transform> patternNodes = new List<Transform>();
    
    private List<Transform> selectedNodes = new List<Transform>();
    private LineRenderer lineRenderer;
    private bool isDrawing = false;
    private Vector3 currentMousePosition;

    void Start()
    {
        // Create line renderer for drawing the pattern
        GameObject lineObj = new GameObject("PatternLine");
        lineRenderer = lineObj.AddComponent<LineRenderer>();
        lineRenderer.material = lineMaterial;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.positionCount = 0;
        lineRenderer.useWorldSpace = true;
        
        // Set up node materials
        foreach (Transform node in patternNodes)
        {
            Renderer rend = node.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = normalColor;
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            StartDrawing();
        }
        else if (Input.GetMouseButton(0) && isDrawing)
        {
            ContinueDrawing();
        }
        else if (Input.GetMouseButtonUp(0) && isDrawing)
        {
            FinishDrawing();
        }
    }

    void StartDrawing()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            // Check if we hit a pattern node
            if (patternNodes.Contains(hit.transform))
            {
                isDrawing = true;
                selectedNodes.Clear();
                AddNodeToPattern(hit.transform);
                
                // Update line renderer
                UpdateLineRenderer();
            }
        }
    }

    void ContinueDrawing()
    {
        currentMousePosition = GetMouseWorldPosition();
        
        // Check if we're over a new node
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit))
        {
            if (patternNodes.Contains(hit.transform) && !selectedNodes.Contains(hit.transform))
            {
                AddNodeToPattern(hit.transform);
            }
        }
        
        // Update line renderer
        UpdateLineRenderer();
    }

    void FinishDrawing()
    {
        isDrawing = false;
        
        // Process the pattern
        Debug.Log("Pattern drawn: " + GetPatternString());
        
        // Here you would typically validate the pattern
        // For example: Check if it matches a stored pattern
        
        // Clear after a delay
        Invoke("ClearPattern", 1.0f);
    }

    void AddNodeToPattern(Transform node)
    {
        selectedNodes.Add(node);
        
        // Visual feedback
        Renderer rend = node.GetComponent<Renderer>();
        if (rend != null)
        {
            rend.material.color = activeColor;
        }
    }

    void UpdateLineRenderer()
    {
        if (selectedNodes.Count == 0) return;
        
        lineRenderer.positionCount = selectedNodes.Count + (isDrawing ? 1 : 0);
        
        // Set positions for selected nodes
        for (int i = 0; i < selectedNodes.Count; i++)
        {
            lineRenderer.SetPosition(i, selectedNodes[i].position);
        }
        
        // Add current mouse position if still drawing
        if (isDrawing)
        {
            lineRenderer.SetPosition(selectedNodes.Count, currentMousePosition);
        }
    }

    void ClearPattern()
    {
        // Reset node colors
        foreach (Transform node in patternNodes)
        {
            Renderer rend = node.GetComponent<Renderer>();
            if (rend != null)
            {
                rend.material.color = normalColor;
            }
        }
        
        // Clear line
        lineRenderer.positionCount = 0;
        selectedNodes.Clear();
    }

    Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        // Create a plane at the same height as your nodes
        Plane plane = new Plane(Vector3.up, patternNodes[0].position.y);
        float distance;
        
        if (plane.Raycast(ray, out distance))
        {
            return ray.GetPoint(distance);
        }
        
        return Vector3.zero;
    }

    string GetPatternString()
    {
        string pattern = "";
        foreach (Transform node in selectedNodes)
        {
            // Find the index of the node
            int index = patternNodes.IndexOf(node);
            if (index >= 0)
            {
                pattern += index.ToString();
            }
        }
        return pattern;
    }

    // For validating the pattern
    public bool CheckPattern(string correctPattern)
    {
        return GetPatternString() == correctPattern;
    }
}