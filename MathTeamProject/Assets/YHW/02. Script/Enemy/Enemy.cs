using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private YHWHealthSystem _healthSystem;
    private DamageTextPool _damageTextPool;

    private void Awake()
    {
        _healthSystem = GetComponent<YHWHealthSystem>();
        _damageTextPool = GetComponentInChildren<DamageTextPool>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Player player))
        {
            _healthSystem.OnDamage((int)player.attackPower);
            _damageTextPool.ShowDamage((int)player.attackPower, transform.position, Color.red);
        }
    }
}
