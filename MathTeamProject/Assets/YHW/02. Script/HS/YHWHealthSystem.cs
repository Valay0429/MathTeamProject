using System;
using UnityEngine;
using UnityEngine.InputSystem.Processors;

public class YHWHealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHp;
    private int currentHp;

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
        gameObject.SetActive(false);
    }
}
