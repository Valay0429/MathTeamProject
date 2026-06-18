using UnityEngine;

public class Player : MonoBehaviour, IBuffReceiver
{
    public float attackPower = 10f;
    public float defense = 10f;
    public float hp = 100f;

    private BuffSO pendingBuff; // 다음 공격에 쓸 버프

    // BuffBox 획득 시 자동 호출
    public void ApplyBuff(BuffSO buff)
    {
        pendingBuff = buff; // 기존 버프는 덮어씌움
        Debug.Log($"버프 획득: {buff.buffType} +{buff.amount}");
    }

    // 공격 실행 시 호출
    public float GetAttackDamage()
    {
        float damage = attackPower;

        if (pendingBuff != null)
        {
            switch (pendingBuff.buffType)
            {
                case BuffType.AttackUp:  damage += pendingBuff.amount; break;
                case BuffType.DefenseUp: defense += pendingBuff.amount; break;
                case BuffType.HealthUp:  hp += pendingBuff.amount; break;
            }
            pendingBuff = null; // 공격 후 소비
        }

        return damage;
    }
}
