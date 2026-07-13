using System;
using UnityEngine;
using YHW;

public class YHWPlayer : MonoBehaviour, IYHWBuffReceiver
{
    [SerializeField] private YHWHealthSystem _healthSystem;
    [SerializeField] private PlayerAttack _playerAttack;
    [SerializeField] private DamageTextPool _damageTextPool;
    private DrawGraph _drawGraph;
    
    private void Awake()
    {
        _playerAttack.currentattackPower = _playerAttack.attackBase;
        _drawGraph = GetComponentInParent<DrawGraph>();
    }

    // BuffBox 획득 시 자동 호출
    public void ApplyBuff(YHWBuffSO buff)
    {
        switch (buff.buffType)
        {
            case YHWBuffType.AttackUp:
            {
                _playerAttack.currentattackPower += _playerAttack.attacPlusAmount;
                _playerAttack.currentattackPower = Mathf.Clamp(_playerAttack.currentattackPower, 0, 999);
                break;
            }
            case YHWBuffType.HealthUp:
            {
                _healthSystem.currentHp += _playerAttack.hpPlusAmount;
                _damageTextPool.ShowDamage(_playerAttack.hpPlusAmount, _damageTextPool.transform.position, Color.lightGreen);
                _healthSystem.currentHp = Mathf.Clamp(_healthSystem.currentHp, 0, _healthSystem.maxHp);
                break;
            }
            case YHWBuffType.BallUp:
            {
                _drawGraph.SpawnFollowerBall();
                break;
            }
            case YHWBuffType.AttackDown:
            {
                _playerAttack.currentattackPower -= _playerAttack.attackDownAmount;
                _playerAttack.currentattackPower = Mathf.Clamp(_playerAttack.currentattackPower, 0, 999);
                break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.TryGetComponent(out Enemy enemy))
        {
            enemy.DamageHandler( _playerAttack.currentattackPower);
        }
    }
}