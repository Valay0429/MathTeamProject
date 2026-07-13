using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YHW._02._Script
{
    public class HPUI : MonoBehaviour
    {
        [SerializeField] private YHWHealthSystem _healthSystem;
        [SerializeField] private Image hpBar;
        [SerializeField] private TextMeshProUGUI hpText;

        private void Update()
        {
            hpText.text = _healthSystem.currentHp + "/" +  _healthSystem.maxHp;
            hpBar.fillAmount = (float)_healthSystem.currentHp / _healthSystem.maxHp;
        }
    }
}