using UnityEngine;

public enum BuffType
{
    AttackUp,
    DefenseUp,
    HealthUp
}

[CreateAssetMenu(fileName = "NewBuff", menuName = "JJK/Buff")]
public class BuffSO : ScriptableObject
{
    [Header("버프 정보")]
    public BuffType buffType;
    public float amount = 10f;

    [Header("박스 비주얼")]
    public Color boxColor = Color.white;
    public Sprite boxSprite;

    [Header("획득 이펙트")]
    public GameObject pickupEffectPrefab;

    [Header("스폰 가중치 (클수록 자주 등장)")]
    [Range(0f, 10f)]
    public float weight = 1f;
}
