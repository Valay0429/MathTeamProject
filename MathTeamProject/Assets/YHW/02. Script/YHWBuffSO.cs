using UnityEngine;

public enum YHWBuffType
{
    AttackUp,
    HealthUp,
    BallUp,
    AttackDown,
}

[CreateAssetMenu(fileName = "NewBuff", menuName = "YHW/Buff")]
public class YHWBuffSO : ScriptableObject
{
    [Header("버프 정보")]
    public YHWBuffType buffType;
    public int amount = 10;

    [Header("박스 비주얼")]
    public Color boxColor = Color.white;
    public Sprite boxSprite;

    [Header("획득 이펙트")]
    public GameObject pickupEffectPrefab;

    [Header("스폰 가중치 (클수록 자주 등장)")]
    [Range(0f, 10f)]
    public float weight = 1f;
}