using System;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public class YHWHealthSystem : MonoBehaviour
{
    [SerializeField] public int maxHp;
    [SerializeField] public int currentHp;
    public event Action OnDead;

    private void Start()
    {
        currentHp = maxHp;
    }

    public void OnDamage(int damage)
    {
        currentHp -= damage;
        if (currentHp <= 0)
            Dead();
    }

    private void Dead()
    {
        OnDead?.Invoke();
    }
    
    public void SetMaxHp()
    {
        currentHp = maxHp;
    }
}
