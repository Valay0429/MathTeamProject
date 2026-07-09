using System.Collections;
using UnityEngine;
using TMPro;

public class FloatingSymbol : MonoBehaviour
{
    RectTransform rt;
    TextMeshProUGUI txt;
    string[] symbols;

    static readonly Color[] Palette =
    {
        new Color(0.00f, 0.90f, 1.00f, 1f), // cyan
        new Color(0.70f, 0.30f, 1.00f, 1f), // purple
        new Color(1.00f, 0.75f, 0.00f, 1f), // gold
        new Color(0.30f, 1.00f, 0.55f, 1f), // green
        new Color(1.00f, 0.40f, 0.70f, 1f), // pink
    };

    public void Initialize(RectTransform rectTransform, TextMeshProUGUI text, string[] symbolSet)
    {
        rt = rectTransform;
        txt = text;
        symbols = symbolSet;
    }

    public void Activate()
    {
        gameObject.SetActive(true);
        StopAllCoroutines();
        StartCoroutine(FloatUp());
    }

    IEnumerator FloatUp()
    {
        float startX   = Random.Range(0.03f, 0.97f);
        float startY   = Random.Range(-0.08f, 0.06f);
        float duration = Random.Range(7f, 14f);
        float fontSize  = Random.Range(16f, 44f);
        float driftAmp  = Random.Range(0.02f, 0.06f);
        float driftFreq = Random.Range(0.3f, 0.8f);
        Color c = Palette[Random.Range(0, Palette.Length)];

        txt.text     = symbols[Random.Range(0, symbols.Length)];
        txt.fontSize = fontSize;
        txt.color    = new Color(c.r, c.g, c.b, 0f);
        rt.anchorMin = rt.anchorMax = new Vector2(startX, startY);
        rt.anchoredPosition = Vector2.zero;
        rt.localEulerAngles = Vector3.zero;

        float fadeIn  = Mathf.Min(1.2f, duration * 0.15f);
        float fadeOut = Mathf.Min(2.0f, duration * 0.25f);
        float maxAlpha = Random.Range(0.12f, 0.28f);

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float progress = t / duration;
            float x = startX + Mathf.Sin(t * driftFreq) * driftAmp;
            float y = startY + progress * 0.65f;

            rt.anchorMin = rt.anchorMax = new Vector2(Mathf.Clamp01(x), y);

            float alpha;
            if (t < fadeIn)
                alpha = (t / fadeIn) * maxAlpha;
            else if (t > duration - fadeOut)
                alpha = ((duration - t) / fadeOut) * maxAlpha;
            else
                alpha = maxAlpha;

            txt.color = new Color(c.r, c.g, c.b, alpha);
            rt.localEulerAngles = new Vector3(0f, 0f, Mathf.Sin(t * 0.4f) * 12f);
            yield return null;
        }

        txt.color = new Color(c.r, c.g, c.b, 0f);
        gameObject.SetActive(false);
    }
}
