using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 개별 데미지 텍스트 오브젝트.
/// 풀에서 꺼내져 위치에 표시되고, 위로 이동하며 서서히 사라진 뒤 다시 풀로 반환된다.
/// </summary>
[RequireComponent(typeof(TMP_Text))]
public class DamageText : MonoBehaviour
{
    [Header("애니메이션 설정")]
    [SerializeField] private float moveDistance = 1.2f;   // 위로 이동할 총 거리
    [SerializeField] private float duration = 0.9f;        // 표시~소멸까지 걸리는 시간

    // 시간(0~1)에 따른 이동 비율 곡선 (처음엔 빠르게, 끝으로 갈수록 느려지게)
    [SerializeField]
    private AnimationCurve moveCurve = new AnimationCurve(
        new Keyframe(0f, 0f, 0f, 2f),
        new Keyframe(1f, 1f, 0f, 0f)
    );

    // 시간(0~1)에 따른 알파값 곡선 (초반엔 선명하게 유지하다가 후반에 자연스럽게 사라짐)
    [SerializeField]
    private AnimationCurve alphaCurve = new AnimationCurve(
        new Keyframe(0f, 1f),
        new Keyframe(0.6f, 1f),
        new Keyframe(1f, 0f)
    );

    private TMP_Text text;
    private Coroutine playingRoutine;
    private DamageTextPool ownerPool;

    private void Awake()
    {
        text = GetComponent<TMP_Text>();
    }

    /// <summary>
    /// 풀에서 생성될 때 한 번 호출되어 자신을 반환할 풀을 등록해둔다.
    /// </summary>
    public void Init(DamageTextPool pool)
    {
        ownerPool = pool;
    }

    /// <summary>
    /// 데미지 텍스트를 특정 위치에 표시하고 애니메이션을 시작한다.
    /// </summary>
    public void Show(int damage, Vector3 worldPosition, Color? color = null)
    {
        transform.position = worldPosition;
        text.text = damage.ToString();

        Color baseColor = color ?? Color.white;
        text.color = baseColor;

        gameObject.SetActive(true);

        if (playingRoutine != null)
            StopCoroutine(playingRoutine);

        playingRoutine = StartCoroutine(PlayRoutine(worldPosition, baseColor));
    }

    private IEnumerator PlayRoutine(Vector3 startPos, Color baseColor)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float ratio = Mathf.Clamp01(elapsed / duration);

            // 위치: 곡선 값만큼 위로 이동 (감속 곡선이라 뚝 멈추지 않고 자연스럽게 느려짐)
            float yOffset = moveCurve.Evaluate(ratio) * moveDistance;
            transform.position = startPos + new Vector3(0f, yOffset, 0f);

            // 알파: 곡선에 따라 서서히 감소 (마지막에 갑자기 뚝 끊기지 않음)
            float alpha = alphaCurve.Evaluate(ratio);
            text.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);

            yield return null;
        }

        // 애니메이션 종료 -> 비활성화 후 풀로 반환
        gameObject.SetActive(false);
        playingRoutine = null;
        ownerPool.Return(this);
    }
}