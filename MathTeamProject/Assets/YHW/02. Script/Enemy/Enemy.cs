using System;
using UnityEngine;
using YHW;

public class Enemy : MonoBehaviour
{
    [SerializeField] private int[] powers;
    [SerializeField] private int[] hp;
    [SerializeField] private Sprite[] enemySprite;
    [SerializeField] private PlayerAttack player;
    public int attackPower;
    public bool isDead = false;
    private YHWHealthSystem _healthSystem;
    private DamageTextPool _damageTextPool;
    private SpriteRenderer _renderer;
    private int index = 0;

    private void Awake()
    {
        _healthSystem = GetComponent<YHWHealthSystem>();
        _damageTextPool = GetComponentInChildren<DamageTextPool>();
        _renderer = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _healthSystem.OnDead += ChangeEnemy;
    }

    private void OnDisable()
    {
        _healthSystem.OnDead -= ChangeEnemy;
    }

    public void DamageHandler(int damage)
    {
        _healthSystem.OnDamage(damage);
        _damageTextPool.ShowDamage(damage, transform.position, Color.red);
    }

    void ChangeEnemy()
    {
        index++;
        attackPower = powers[index];
        _healthSystem.maxHp = hp[index];
        _renderer.sprite = enemySprite[index];
        SetPlusAmount();
        _healthSystem.SetMaxHp();
        if (index == 4)
        {
            transform.localScale = new Vector3(12.05f, 12.05f, 1);
            transform.position = new Vector3(6.07f, 0, 0);
        }
        isDead = true;
    }

    void SetPlusAmount()
    {
        switch (index)
        {
            case 1:
                player.attacPlusAmount = 14;
                player.hpPlusAmount = 15;
                player.attackDownAmount = 13;
                break;
            case 2:
                player.attacPlusAmount = 18;
                player.hpPlusAmount = 20;
                player.attackDownAmount = 16;
                break;
            case 3:
                player.attacPlusAmount = 23;
                player.hpPlusAmount = 25;
                player.attackDownAmount = 20;
                break;
            case 4:
                player.attacPlusAmount = 29;
                player.hpPlusAmount = 30;
                player.attackDownAmount = 25;
                break;
        }
    }
}
