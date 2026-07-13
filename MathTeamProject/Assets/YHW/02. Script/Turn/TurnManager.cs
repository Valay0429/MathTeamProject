using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using YHW;
using YHW._02._Script.FT;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private FTBuffBoxSpawner buffBoxSpawner;
    [SerializeField] private Enemy enemy;
    [SerializeField] private YHWHealthSystem playerHealth;
    [SerializeField] private PlayerAttack _playerAttack;
    [SerializeField] private DamageTextPool _damageTextPool;
    public static TurnManager Instance { get; private set; }

    private int currentTurn = 1;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Start()
    {
        turnText.text = currentTurn + "Turn";
        buffBoxSpawner.SpawnBuffBoxes();
    }

    public void EndTurn()
    {
        currentTurn++;
        turnText.text = currentTurn + " Turn";
        _playerAttack.currentattackPower = _playerAttack.attackBase;
        EnemyAttack();
        buffBoxSpawner.SpawnBuffBoxes();
    }

    void EnemyAttack()
    {
        Sequence s =  DOTween.Sequence();
        if (!enemy.isDead)
        {
            s.Append(enemy.gameObject.transform.DOMoveX(-1, 0.5f).SetRelative());
            s.AppendCallback(EnemyAttackPlayer);
            s.Append(enemy.gameObject.transform.DOMoveX(1, 0.5f).SetRelative());
        }
        else
        {
            enemy.isDead = false;
        }
    }

    void EnemyAttackPlayer()
    {
        _damageTextPool.ShowDamage(enemy.attackPower, _damageTextPool.transform.position, Color.red);
        playerHealth.OnDamage(enemy.attackPower);
    }
}
