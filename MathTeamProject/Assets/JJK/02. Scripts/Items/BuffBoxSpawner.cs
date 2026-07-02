using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;

public class BuffBoxSpawner : MonoBehaviour
{
    [Header("프리팹")]
    [SerializeField] private GameObject buffBoxPrefab;

    [Header("스폰 개수")]
    [SerializeField] private int minCount = 3;
    [SerializeField] private int maxCount = 7;

    [Header("스폰 위치 — 스폰 포인트가 없으면 영역 랜덤 사용")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private Vector2 areaMin = new Vector2(-10f, -10f);
    [SerializeField] private Vector2 areaMax = new Vector2(10f, 10f);

    [Header("버프 풀 (BuffSO 에셋을 여기에 등록)")]
    [SerializeField] private BuffSO[] buffPool;

    [Header("겹침 방지")]
    [SerializeField] private float minDistance = 1.5f;
    [SerializeField] private int maxPlacementAttempts = 30; // 영역 랜덤 배치 시 최대 시도 횟수

    private readonly List<GameObject> activeBoxes = new();
    private readonly List<Vector3> placedPositions = new();

    private void Update()
    {
        if (Keyboard.current.tKey.wasPressedThisFrame)
            SpawnBuffBoxes();
    }
    
    public void SpawnBuffBoxes()
    {
        ClearBuffBoxes();

        if (buffPool == null || buffPool.Length == 0)
        {
            Debug.LogWarning("[BuffBoxSpawner] buffPool이 비어 있습니다. BuffSO 에셋을 등록해주세요.");
            return;
        }

        int count = Random.Range(minCount, maxCount + 1);

        if (spawnPoints != null && spawnPoints.Length > 0)
            SpawnAtPoints(count);
        else
            SpawnInArea(count);
    }
    
    public void ClearBuffBoxes()
    {
        foreach (GameObject box in activeBoxes)
            if (box != null)
                Destroy(box);

        activeBoxes.Clear();
        placedPositions.Clear();
    }

    private void SpawnAtPoints(int count)
    {
        List<int> shuffled = ShuffledIndices(spawnPoints.Length);

        foreach (int idx in shuffled)
        {
            if (activeBoxes.Count >= count) break;

            Vector3 pos = spawnPoints[idx].position;
            if (IsTooClose(pos)) continue;

            CreateBox(pos);
        }
    }

    private void SpawnInArea(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = Vector3.zero;
            bool found = false;

            for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
            {
                pos = new Vector3(
                    Random.Range(areaMin.x, areaMax.x),
                    Random.Range(areaMin.y, areaMax.y),
                    0f);

                if (!IsTooClose(pos)) { found = true; break; }
            }

            if (!found)
            {
                Debug.LogWarning($"[BuffBoxSpawner] {i + 1}번째 박스 배치 실패 — 여유 공간 부족");
                continue;
            }

            CreateBox(pos);
        }
    }

    private void CreateBox(Vector3 position)
    {
        GameObject go = Instantiate(buffBoxPrefab, position, Quaternion.identity, transform);

        if (go.TryGetComponent(out BuffBox box))
            box.SetBuff(PickRandomBuff());

        activeBoxes.Add(go);
        placedPositions.Add(position);
    }

    private bool IsTooClose(Vector3 pos)
    {
        foreach (Vector3 placed in placedPositions)
            if (Vector3.Distance(pos, placed) < minDistance)
                return true;
        return false;
    }

    // 가중치 기반 랜덤 선택
    private BuffSO PickRandomBuff()
    {
        float total = 0f;
        foreach (BuffSO b in buffPool)
            total += b.weight;

        float roll = Random.Range(0f, total);

        foreach (BuffSO b in buffPool)
        {
            if (roll < b.weight) return b;
            roll -= b.weight;
        }

        return buffPool[^1];
    }

    // Fisher-Yates 셔플
    private static List<int> ShuffledIndices(int length)
    {
        List<int> list = new();
        for (int i = 0; i < length; i++) list.Add(i);

        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }

        return list;
    }
}
