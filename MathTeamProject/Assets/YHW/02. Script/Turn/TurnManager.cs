using System;
using TMPro;
using UnityEngine;
using YHW._02._Script.FT;

public class TurnManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI turnText;
    [SerializeField] private FTBuffBoxSpawner buffBoxSpawner;
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
        turnText.text = currentTurn + "Turn";
        buffBoxSpawner.SpawnBuffBoxes();
    }
}
