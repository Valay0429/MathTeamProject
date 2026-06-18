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
    [Header("Graph Type")] 
    public bool type;
    
    private LineRenderer lineRenderer;
    private bool  isBallMoving = false;
    private float timer = 0f;

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
        
        if (isBallMoving)
        {
            MoveBall(amplitude, frequency);
        }
    }

    private void DrawGraphScene(float amplitude, float frequency)
    {
        for (int i = 0; i < resolution; i++)
        {
            float x = ((float)i / (resolution - 1)) * graphWidth - (graphWidth / 2f);
            float y;
            if (type == true)
                 y = amplitude * Mathf.Sin(frequency * x);
            else
                y = amplitude * Mathf.Cos(frequency * x);
            
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    private void MoveBall(float amplitude, float frequency)
    {
        if (ball != null)
        {
            timer += Time.deltaTime * ballSpeed;

            float x = timer - (graphWidth / 2f);

            if (x >= (graphWidth / 2f))
            {
                isBallMoving = false;
                ball.SetActive(false);
                return;
            }

            float y = type ? amplitude * Mathf.Sin(frequency * x) : amplitude * Mathf.Cos(frequency * x);

            ball.transform.position = new Vector3(x, y, 0);
        }
    }

    public void ShotBall()
    {
        if (!isBallMoving)
        {
            isBallMoving = true;
            timer = 0f;
            ball.SetActive(true);
        }
    }

    public void ChangeType(int index)
    {
        if (index == 0)
            type = true;
        else if (index == 1)
            type = false;
    }
}
