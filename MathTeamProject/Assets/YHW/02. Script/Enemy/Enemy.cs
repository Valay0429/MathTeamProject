using System;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    private YHWHealthSystem _healthSystem;

    private void Awake()
    {
        _healthSystem = GetComponent<YHWHealthSystem>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Player player))
        {
            _healthSystem.OnDamage(1);
        }
    }
}
