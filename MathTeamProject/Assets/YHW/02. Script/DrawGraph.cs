using System;
using UnityEngine;
using UnityEngine.UI;

public class DrawGraph : MonoBehaviour
{
    [Header("Ball")] 
    [SerializeField] private GameObject ball;
    [SerializeField] private float ballSpeed = 10f;
    [Header("Slider")]
    public Slider amplitudeSlider;
    public Slider frequencySlider;
    [Header("Graph Settings")]
    public int resolution = 100; 
    public float graphWidth = 10f;
    
    private LineRenderer lineRenderer;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    private void Start()
    {
        lineRenderer.positionCount = resolution;
    }

    private void Update()
    { 
        float amplitude = amplitudeSlider.value;
        float frequency = frequencySlider.value;
        
        DrawGraphScene(amplitude, frequency);
        
        MoveBall(amplitude, frequency);
    }

    private void DrawGraphScene(float amplitude, float frequency)
    {
        for (int i = 0; i < resolution; i++)
        {
            float x = ((float)i / (resolution - 1)) * graphWidth - (graphWidth / 2f);
            
            float y = amplitude * Mathf.Sin(frequency * x);
            
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }
    
    private void MoveBall(float amplitude, float frequency)
    {
        if (ball != null)
        {
            float x = Mathf.Repeat(Time.time * ballSpeed, graphWidth) - (graphWidth / 2f);
            
            float y = amplitude * Mathf.Sin(frequency * x);
            
            ball.transform.position = new Vector3(x, y, 0);
        }
    }
}
