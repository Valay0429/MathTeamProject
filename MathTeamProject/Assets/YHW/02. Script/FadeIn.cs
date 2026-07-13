using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    [SerializeField] private Image _image;

    private void Start()
    {
        _image.DOFade(0, 1);
    }
}
