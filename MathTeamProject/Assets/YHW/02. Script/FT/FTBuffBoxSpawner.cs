using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace YHW._02._Script.FT
{
    public class FTBuffBoxSpawner : MonoBehaviour
    {
        [Header("프리팹")]
        [SerializeField] private GameObject buffBoxPrefab;
        [SerializeField] private GameObject debuffBoxPrefab;

        [Header("버프 스폰 개수")]
        [SerializeField] private int minCount = 3;
        [SerializeField] private int maxCount = 7;

        [Header("디버프 스폰 개수")]
        [SerializeField] private int minDebuffCount = 2;
        [SerializeField] private int maxDebuffCount = 5;

        [Header("스폰 위치 — 스폰 포인트가 없으면 콜라이더 영역 랜덤 사용")]
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private Collider2D[] spawnArea; // 이 콜라이더 내부에서만 랜덤 스폰

        [Header("버프 풀 (BuffSO 에셋을 여기에 등록)")]
        [SerializeField] private YHWBuffSO[] buffPool;

        [Header("디버프 풀 (BuffSO 에셋을 여기에 등록)")]
        [SerializeField] private YHWBuffSO[] debuffPool;

        [Header("겹침 방지")]
        [SerializeField] private float minDistance = 1.5f;
        [SerializeField] private int maxPlacementAttempts = 30; // 영역 랜덤 배치 시 최대 시도 횟수

        private readonly List<GameObject> activeBoxes = new();
        private readonly List<Vector3> placedPositions = new();

        public void SpawnBuffBoxes()
        {
            ClearBuffBoxes();

            bool hasBuffPool = buffPool != null && buffPool.Length > 0;
            bool hasDebuffPool = debuffPool != null && debuffPool.Length > 0;

            if (!hasBuffPool && !hasDebuffPool)
            {
                Debug.LogWarning("[BuffBoxSpawner] buffPool과 debuffPool이 모두 비어 있습니다. BuffSO 에셋을 등록해주세요.");
                return;
            }

            if ((spawnPoints == null || spawnPoints.Length == 0) && spawnArea == null)
            {
                Debug.LogWarning("[BuffBoxSpawner] spawnPoints도 spawnArea(Collider2D)도 설정되지 않았습니다.");
                return;
            }

            int buffCount = hasBuffPool ? Random.Range(minCount, maxCount + 1) : 0;
            int debuffCount = hasDebuffPool ? Random.Range(minDebuffCount, maxDebuffCount + 1) : 0;

            if (spawnPoints != null && spawnPoints.Length > 0)
            {
                // 버프/디버프가 스폰 포인트를 나눠 쓰도록 한 번만 섞어서 순서대로 소비
                List<int> shuffled = ShuffledIndices(spawnPoints.Length);
                int cursor = 0;

                if (hasBuffPool)
                    cursor = SpawnAtPoints(shuffled, cursor, buffCount, buffBoxPrefab, buffPool);

                if (hasDebuffPool)
                    SpawnAtPoints(shuffled, cursor, debuffCount, debuffBoxPrefab, debuffPool);
            }
            else
            {
                if (hasBuffPool)
                    SpawnInArea(buffCount, buffBoxPrefab, buffPool);

                if (hasDebuffPool)
                    SpawnInArea(debuffCount, debuffBoxPrefab, debuffPool);
            }
        }

        public void ClearBuffBoxes()
        {
            foreach (GameObject box in activeBoxes)
                if (box != null)
                    Destroy(box);

            activeBoxes.Clear();
            placedPositions.Clear();
        }

        // startIndex부터 shuffled 리스트를 소비하며 count개 배치, 다 쓴 다음 인덱스를 반환 (다음 그룹이 이어서 사용)
        private int SpawnAtPoints(List<int> shuffled, int startIndex, int count, GameObject prefab, YHWBuffSO[] pool)
        {
            int placed = 0;
            int i = startIndex;

            for (; i < shuffled.Count && placed < count; i++)
            {
                Vector3 pos = spawnPoints[shuffled[i]].position;
                if (IsTooClose(pos)) continue;

                CreateBox(pos, prefab, pool);
                placed++;
            }

            return i;
        }

        private void SpawnInArea(int count, GameObject prefab, YHWBuffSO[] pool)
        {
            if (spawnArea == null)
            {
                Debug.LogWarning("[BuffBoxSpawner] spawnArea(Collider2D)가 설정되지 않았습니다.");
                return;
            }

            for (int i = 0; i < count; i++)
            {
                Vector3 pos = Vector3.zero;
                bool found = false;

                for (int attempt = 0; attempt < maxPlacementAttempts; attempt++)
                {
                    int randomIndex = Random.Range(0, spawnArea.Length);
                    Collider2D area = spawnArea[randomIndex];
                    Bounds bounds = area.bounds;

                    Vector3 candidate = new Vector3(
                        Random.Range(bounds.min.x, bounds.max.x),
                        Random.Range(bounds.min.y, bounds.max.y),
                        0f);

                    // 콜라이더 형태 내부인지 확인 (사각형이 아닌 콜라이더도 정확히 걸러짐)
                    if (!area.OverlapPoint(candidate)) continue;

                    if (IsTooClose(candidate)) continue;

                    pos = candidate;
                    found = true;
                    break;
                }

                if (!found)
                {
                    Debug.LogWarning($"[BuffBoxSpawner] {i + 1}번째 박스 배치 실패 — 여유 공간 부족");
                    continue;
                }

                CreateBox(pos, prefab, pool);
            }
        }

        private void CreateBox(Vector3 position, GameObject prefab, YHWBuffSO[] pool)
        {
            GameObject go = Instantiate(prefab, position, Quaternion.identity, transform);

            if (go.TryGetComponent(out YHWBuffBox box))
                box.SetBuff(PickRandomBuff(pool));

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
        private YHWBuffSO PickRandomBuff(YHWBuffSO[] pool)
        {
            float total = 0f;
            foreach (YHWBuffSO b in pool)
                total += b.weight;

            float roll = Random.Range(0f, total);

            foreach (YHWBuffSO b in pool)
            {
                if (roll < b.weight) return b;
                roll -= b.weight;
            }

            return pool[^1];
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
}