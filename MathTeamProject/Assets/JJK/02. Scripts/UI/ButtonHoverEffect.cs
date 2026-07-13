using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class ButtonHoverEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerDownHandler,  IPointerUpHandler
{
    // [SerializeField] 로 선언해야 프리팹 저장 시 직렬화됩니다.
    [SerializeField] RectTransform   rt;
    [SerializeField] Image           borderImg;
    [SerializeField] Image           fillImg;
    [SerializeField] TextMeshProUGUI label;
    [SerializeField] Color           accent;
    [SerializeField] Color           normalFill;

    bool      hovered;
    Coroutine scaleCo;

    /// <summary>
    /// 런타임에서 코드로 버튼을 만들 때 호출합니다.
    /// 프리팹 방식에서는 SettingsPanelCreator 가 에디터에서 호출하여 값을 직렬화합니다.
    /// </summary>
    public void Setup(RectTransform rect, Image border, Image fill,
                      TextMeshProUGUI lbl, Color accentColor)
    {
        rt         = rect;
        borderImg  = border;
        fillImg    = fill;
        label      = lbl;
        accent     = accentColor;
        normalFill = fill != null ? fill.color : Color.clear;
    }

    public void OnPointerEnter(PointerEventData e)
    {
        hovered = true;
        if (fillImg)   fillImg.color   = new Color(accent.r * 0.22f, accent.g * 0.22f, accent.b * 0.32f, 0.92f);
        if (borderImg) borderImg.color = new Color(accent.r, accent.g, accent.b, 1f);
        if (label)     label.color     = Color.white;
        Rescale(1.05f, 0.12f);
    }

    public void OnPointerExit(PointerEventData e)
    {
        hovered = false;
        if (fillImg)   fillImg.color   = normalFill;
        if (borderImg) borderImg.color = new Color(accent.r, accent.g, accent.b, 0.65f);
        if (label)     label.color     = accent;
        Rescale(1f, 0.12f);
    }

    public void OnPointerDown(PointerEventData e) => Rescale(0.96f, 0.07f);

    public void OnPointerUp(PointerEventData e)   => Rescale(hovered ? 1.05f : 1f, 0.07f);

    void Rescale(float target, float dur)
    {
        if (rt == null) return;           // 참조가 없으면 스케일 시도 안 함
        if (scaleCo != null) StopCoroutine(scaleCo);
        scaleCo = StartCoroutine(ScaleTo(target, dur));
    }

    IEnumerator ScaleTo(float target, float dur)
    {
        if (rt == null) yield break;      // 오브젝트가 파괴된 경우 안전 종료
        Vector3 from = rt.localScale;
        Vector3 to   = Vector3.one * target;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            if (rt == null) yield break;
            rt.localScale = Vector3.Lerp(from, to, t / dur);
            yield return null;
        }
        if (rt != null) rt.localScale = to;
    }
}
