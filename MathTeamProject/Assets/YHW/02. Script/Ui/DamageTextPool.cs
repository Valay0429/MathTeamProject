using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 데미지 텍스트 오브젝트 풀.
/// 씬에 하나만 두고, 외부(전투/데미지 처리 스크립트)에서
/// DamageTextPool.Instance.ShowDamage(데미지량, 위치) 형태로 호출해서 사용한다.
/// </summary>
public class DamageTextPool : MonoBehaviour
{
    public static DamageTextPool Instance { get; private set; }

    [Header("풀 설정")]
    [SerializeField] private DamageText damageTextPrefab;
    [SerializeField] private int initialSize = 20;
    [SerializeField] private Transform poolParent;

    private readonly Queue<DamageText> pool = new Queue<DamageText>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (poolParent == null)
            poolParent = transform;

        for (int i = 0; i < initialSize; i++)
        {
            DamageText instance = CreateInstance();
            instance.gameObject.SetActive(false);
            pool.Enqueue(instance);
        }
    }

    private DamageText CreateInstance()
    {
        DamageText instance = Instantiate(damageTextPrefab, poolParent, true);
        instance.Init(this);
        return instance;
    }

    /// <summary>
    /// 외부에서 데미지가 발생했을 때 호출하는 메서드.
    /// 예) DamageTextPool.Instance.ShowDamage(25, enemy.transform.position);
    /// </summary>
    /// <param name="damage">표시할 데미지량</param>
    /// <param name="worldPosition">텍스트가 나타날 월드 좌표</param>
    /// <param name="color">글자 색상 (생략 시 흰색, 크리티컬 등은 빨간색 등으로 넘기면 됨)</param>
    public void ShowDamage(int damage, Vector3 worldPosition, Color? color = null)
    {
        DamageText dt = pool.Count > 0 ? pool.Dequeue() : CreateInstance();
        dt.Show(damage, worldPosition, color);
    }

    /// <summary>
    /// 애니메이션이 끝난 DamageText가 스스로 호출해서 풀로 돌아온다.
    /// </summary>
    public void Return(DamageText dt)
    {
        pool.Enqueue(dt);
    }
}