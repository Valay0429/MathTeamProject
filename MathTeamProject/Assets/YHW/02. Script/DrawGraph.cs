using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DrawGraph : MonoBehaviour
{
    [Header("Ball")] 
    [SerializeField] private GameObject ballPrefab; // 풀링할 공 프리팹
    [SerializeField] private float ballSpeed = 10f;

    [Header("Ball Pool")]
    [SerializeField] private int poolSize = 5;
    [SerializeField] private float spawnGapInTimer = 1.5f; // 새 공을 가장 뒤 공보다 얼마나 뒤에 스폰할지 (timer 기준 = x 기준 간격)

    [Header("Slider")]
    public Slider amplitudeSlider;
    public Slider frequencySlider;

    [Header("Slider Limit")]
    [SerializeField] private float hardMax = 1.5f;
    [SerializeField] private float halfMax = 0.7f;

    [Header("Graph Settings")]
    public int resolution = 100; 
    public float graphWidth = 10f;

    [Header("Graph Type")] 
    public bool type;

    private UiInteractable _uiInteractable;
    private LineRenderer lineRenderer;

    // ---- 풀링 & 다중 공 관리 ----
    private class ActiveBall
    {
        public GameObject obj;
        public float timer; // 이 공만의 진행도(=x + graphWidth/2)
    }

    private readonly Queue<GameObject> ballPool = new Queue<GameObject>();
    private readonly List<ActiveBall> activeBalls = new List<ActiveBall>();

    private bool IsBallMoving => activeBalls.Count > 0;

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        _uiInteractable = GetComponent<UiInteractable>();
    }

    private void Start()
    {
        lineRenderer.positionCount = resolution;

        amplitudeSlider.maxValue = hardMax;
        frequencySlider.maxValue = hardMax;

        amplitudeSlider.onValueChanged.AddListener(OnAmplitudeChanged);
        frequencySlider.onValueChanged.AddListener(OnFrequencyChanged);

        OnAmplitudeChanged(amplitudeSlider.value);
        OnFrequencyChanged(frequencySlider.value);

        InitPool();
    }

    private void OnDestroy()
    {
        if (amplitudeSlider != null)
            amplitudeSlider.onValueChanged.RemoveListener(OnAmplitudeChanged);
        if (frequencySlider != null)
            frequencySlider.onValueChanged.RemoveListener(OnFrequencyChanged);
    }

    private void InitPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject obj = Instantiate(ballPrefab, transform);
            obj.SetActive(false);
            ballPool.Enqueue(obj);
        }
    }

    private GameObject GetFromPool()
    {
        GameObject obj = ballPool.Count > 0 ? ballPool.Dequeue() : Instantiate(ballPrefab, transform);
        obj.SetActive(true);
        return obj;
    }

    private void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        ballPool.Enqueue(obj);
    }

    private void Update()
    { 
        float amplitude = amplitudeSlider.value;
        float frequency = frequencySlider.value;
        
        DrawGraphScene(amplitude, frequency);
        UpdateBalls(amplitude, frequency);
    }

    private void DrawGraphScene(float amplitude, float frequency)
    {
        for (int i = 0; i < resolution; i++)
        {
            float x = ((float)i / (resolution - 1)) * graphWidth - (graphWidth / 2f);
            float y = type ? amplitude * Mathf.Sin(frequency * x) : amplitude * Mathf.Cos(frequency * x);
            lineRenderer.SetPosition(i, new Vector3(x, y, 0));
        }
    }

    private void UpdateBalls(float amplitude, float frequency)
    {
        // 뒤에서부터 순회 (제거해도 안전하게)
        for (int i = activeBalls.Count - 1; i >= 0; i--)
        {
            ActiveBall b = activeBalls[i];
            b.timer += Time.deltaTime * ballSpeed;

            float x = b.timer - (graphWidth / 2f);

            if (x >= (graphWidth / 2f))
            {
                ReturnToPool(b.obj);
                activeBalls.RemoveAt(i);
                continue;
            }

            float y = type ? amplitude * Mathf.Sin(frequency * x) : amplitude * Mathf.Cos(frequency * x);
            b.obj.transform.position = new Vector3(x, y, 0);
        }

        // 모든 공이 사라지면 턴 종료
        if (activeBalls.Count == 0 && wasMoving)
        {
            wasMoving = false;
            _uiInteractable.SetUiInteractable(true);
            TurnManager.Instance.EndTurn();
        }
    }

    private bool wasMoving = false;

    // 최초 발사
    public void ShotBall()
    {
        if (IsBallMoving) return; // 이미 진행 중이면 무시 (필요하면 로직 변경 가능)

        SpawnBall(0f);
        wasMoving = true;
        _uiInteractable.SetUiInteractable(false);
    }

    // 아이템 먹었을 때 호출 - 가장 뒤에 있는 공보다 더 뒤에 새 공 생성
    public void SpawnFollowerBall()
    {
        if (activeBalls.Count == 0)
        {
            // 이동 중인 공이 없으면 그냥 새로 쏘는 것과 동일하게 처리
            ShotBall();
            return;
        }

        ActiveBall rearmost = GetRearmostBall();
        float newTimer = rearmost.timer - spawnGapInTimer;
        // 필요하면 최소값 클램프 (음수 허용 시 그래프 밖에서 서서히 들어오는 연출)
        // newTimer = Mathf.Max(newTimer, 0f);

        SpawnBall(newTimer);
    }

    private void SpawnBall(float startTimer)
    {
        GameObject obj = GetFromPool();

        float amplitude = amplitudeSlider.value;
        float frequency = frequencySlider.value;
        float x = startTimer - (graphWidth / 2f);
        float y = type ? amplitude * Mathf.Sin(frequency * x) : amplitude * Mathf.Cos(frequency * x);
        obj.transform.position = new Vector3(x, y, 0);

        activeBalls.Add(new ActiveBall { obj = obj, timer = startTimer });
    }

    private ActiveBall GetRearmostBall()
    {
        ActiveBall rearmost = activeBalls[0];
        for (int i = 1; i < activeBalls.Count; i++)
        {
            if (activeBalls[i].timer < rearmost.timer)
                rearmost = activeBalls[i];
        }
        return rearmost;
    }

    private void OnAmplitudeChanged(float value)
    {
        bool isAtMax = value >= hardMax - 0.001f;
        float newMax = isAtMax ? halfMax : hardMax;
        if (Mathf.Approximately(frequencySlider.maxValue, newMax)) return;
        frequencySlider.maxValue = newMax;
        if (frequencySlider.value > newMax) frequencySlider.value = newMax;
    }

    private void OnFrequencyChanged(float value)
    {
        bool isAtMax = value >= hardMax - 0.001f;
        float newMax = isAtMax ? halfMax : hardMax;
        if (Mathf.Approximately(amplitudeSlider.maxValue, newMax)) return;
        amplitudeSlider.maxValue = newMax;
        if (amplitudeSlider.value > newMax) amplitudeSlider.value = newMax;
    }

    public void ChangeType(bool type)
    {
        this.type = type;
    }
}