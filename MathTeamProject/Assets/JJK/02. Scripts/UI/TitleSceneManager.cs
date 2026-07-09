using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using TMPro;

public class TitleSceneManager : MonoBehaviour
{
    // ── Color Palette ──────────────────────────────────────────────────────────
    static readonly Color C_BG      = new Color(0.02f, 0.02f, 0.08f, 1f);
    static readonly Color C_CYAN    = new Color(0.00f, 0.90f, 1.00f, 1f);
    static readonly Color C_PURPLE  = new Color(0.65f, 0.25f, 1.00f, 1f);
    static readonly Color C_GOLD    = new Color(1.00f, 0.82f, 0.00f, 1f);
    static readonly Color C_RED     = new Color(1.00f, 0.28f, 0.28f, 1f);
    static readonly Color C_PANEL   = new Color(0.01f, 0.03f, 0.12f, 0.94f);
    static readonly Color C_DIM     = new Color(0.50f, 0.70f, 0.85f, 0.55f);

    static readonly string[] MathSymbols =
    {
        "π", "Σ", "∞", "√", "∫", "∂", 
        "θ", "α", "β", "γ", "λ", "φ", 
        "ψ", "Ω", "±", "≈"
    };

    // ── Inspector Settings ──────────────────────────────────────────────────────
    [Header("제목 / 텍스트")]
    [SerializeField] string gameTitle        = "MATH\nQUEST";
    [SerializeField] string gameSubtitle     = "수식의 세계를  탐험하라";
    [SerializeField] string versionLabel     = "v1.0.0   |   © 2024 Math Team";
    [SerializeField] string startBtnLabel    = "게임 시작";
    [SerializeField] string settingsBtnLabel = "설     정";
    [SerializeField] string quitBtnLabel     = "게임 종료";

    [Header("폰트  (비워두면 TMP 기본 폰트 사용)")]
    [SerializeField] TMP_FontAsset titleFont;
    [SerializeField] TMP_FontAsset bodyFont;

    [Header("폰트 크기")]
    [SerializeField] float titleFontSize    = 110f;
    [SerializeField] float subtitleFontSize = 22f;
    [SerializeField] float buttonFontSize   = 27f;
    [SerializeField] float versionFontSize  = 14f;

    [Header("씬 이름 (Build Settings 에 추가된 씬)")]
    [SerializeField] string gameSceneName = "JJK_Scene";

    // ── Private references ─────────────────────────────────────────────────────
    Canvas            mainCanvas;
    RectTransform     menuLayer;
    RectTransform     particleLayer;
    Image             fadeImg;
    RawImage          glowImg;
    TextMeshProUGUI   titleText;
    TextMeshProUGUI   subtitleText;

    readonly List<FloatingSymbol> pool = new List<FloatingSymbol>();
    bool transitioning;

    // ══════════════════════════════════════════════════════════════════════════
    //  Bootstrap
    // ══════════════════════════════════════════════════════════════════════════
    void Awake()
    {
        EnsureGameSettings();
        SetupCamera();
        SetupEventSystem();
        BuildCanvas();
        BuildBackground();
        BuildParticleLayer();
        BuildMenuUI();
        BuildFadeOverlay();
    }

    void Start()
    {
        StartCoroutine(IntroSequence());
        StartCoroutine(SpawnLoop());
        StartCoroutine(TitleGlowLoop());
        StartCoroutine(PulseGlowLoop());
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Initialisation helpers
    // ══════════════════════════════════════════════════════════════════════════
    void EnsureGameSettings()
    {
        if (GameSettings.Instance == null)
            new GameObject("GameSettings").AddComponent<GameSettings>();
    }

    void SetupCamera()
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            var go = new GameObject("Main Camera"); go.tag = "MainCamera";
            cam = go.AddComponent<Camera>();
        }
        cam.clearFlags       = CameraClearFlags.SolidColor;
        cam.backgroundColor  = C_BG;
        cam.orthographic     = true;
    }

    void SetupEventSystem()
    {
        var existing = FindObjectOfType<EventSystem>();
        if (existing != null)
        {
            if (existing.GetComponent<InputSystemUIInputModule>() != null) return;
            var old = existing.GetComponent<StandaloneInputModule>();
            if (old != null) DestroyImmediate(old);   // 즉시 제거 → 같은 프레임에 두 모듈 공존 방지
            existing.gameObject.AddComponent<InputSystemUIInputModule>();
            return;
        }
        var es = new GameObject("EventSystem");
        es.AddComponent<EventSystem>();
        es.AddComponent<InputSystemUIInputModule>();
    }

    void BuildCanvas()
    {
        var go = new GameObject("TitleCanvas");
        mainCanvas = go.AddComponent<Canvas>();
        mainCanvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        mainCanvas.sortingOrder = 0;

        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight  = 0.5f;

        go.AddComponent<GraphicRaycaster>();
    }

    // ── Background ─────────────────────────────────────────────────────────────
    void BuildBackground()
    {
        var layer = MakeFullRect("Background", mainCanvas.transform);
        layer.gameObject.AddComponent<Image>().color = C_BG;

        // vertical gradient
        var grad = MakeFullRect("Gradient", layer);
        var ri = grad.gameObject.AddComponent<RawImage>();
        ri.texture    = CreateGradientTex(2, 512,
            new Color(0.00f, 0.02f, 0.12f), new Color(0.02f, 0.01f, 0.06f));
        ri.raycastTarget = false;

        // grid
        var grid = MakeFullRect("Grid", layer);
        var gi = grid.gameObject.AddComponent<RawImage>();
        gi.texture       = CreateGridTex(80, 80, new Color(1f, 1f, 1f, 0.03f));
        gi.raycastTarget = false;

        // centre glow (pulsed later)
        var gGO = new GameObject("CentreGlow");
        gGO.transform.SetParent(layer, false);
        var gRT = gGO.AddComponent<RectTransform>();
        gRT.anchorMin = new Vector2(0.1f, 0.15f);
        gRT.anchorMax = new Vector2(0.9f, 0.90f);
        gRT.offsetMin = gRT.offsetMax = Vector2.zero;
        glowImg = gGO.AddComponent<RawImage>();
        glowImg.texture = CreateRadialTex(256, 256,
            new Color(0.00f, 0.25f, 0.60f, 0.18f), new Color(0f, 0f, 0f, 0f));
        glowImg.raycastTarget = false;

        // vignette
        var vig = MakeFullRect("Vignette", layer);
        var vi = vig.gameObject.AddComponent<RawImage>();
        vi.texture       = CreateVignetteTex(256, 256);
        vi.raycastTarget = false;
    }

    // ── Particle Layer ─────────────────────────────────────────────────────────
    void BuildParticleLayer()
    {
        particleLayer = MakeFullRect("Particles", mainCanvas.transform);
        var cg = particleLayer.gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable   = false;

        for (int i = 0; i < 20; i++)
        {
            var go  = new GameObject($"Sym{i}");
            go.transform.SetParent(particleLayer, false);
            var rt  = go.AddComponent<RectTransform>();
            rt.sizeDelta = new Vector2(70, 70);
            var txt = go.AddComponent<TextMeshProUGUI>();
            txt.text      = MathSymbols[i % MathSymbols.Length];
            txt.fontSize  = 28;
            txt.alignment = TextAlignmentOptions.Center;
            txt.color     = new Color(1f, 1f, 1f, 0f);
            var fs = go.AddComponent<FloatingSymbol>();
            fs.Initialize(rt, txt, MathSymbols);
            pool.Add(fs);
            go.SetActive(false);
        }
    }

    // ── Menu UI ────────────────────────────────────────────────────────────────
    void BuildMenuUI()
    {
        menuLayer = MakeFullRect("MenuUI", mainCanvas.transform);
        var cg = menuLayer.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        // ── Title ──
        var tGO = new GameObject("TitleText");
        tGO.transform.SetParent(menuLayer, false);
        var tRT = tGO.AddComponent<RectTransform>();
        tRT.anchorMin = new Vector2(0.08f, 0.62f);
        tRT.anchorMax = new Vector2(0.92f, 0.90f);
        tRT.offsetMin = tRT.offsetMax = Vector2.zero;
        titleText              = tGO.AddComponent<TextMeshProUGUI>();
        titleText.text         = gameTitle;
        titleText.fontSize     = titleFontSize;
        if (titleFont != null) titleText.font = titleFont;
        titleText.fontStyle    = FontStyles.Bold;
        titleText.alignment    = TextAlignmentOptions.Center;
        titleText.color        = Color.white;
        titleText.outlineWidth = 0.18f;
        titleText.outlineColor = C_CYAN;
        // depth shadow
        var sh = tGO.AddComponent<Shadow>();
        sh.effectColor    = new Color(0f, 0.8f, 1f, 0.45f);
        sh.effectDistance = new Vector2(4f, -4f);

        // ── Subtitle ──
        var sGO = new GameObject("Subtitle");
        sGO.transform.SetParent(menuLayer, false);
        var sRT = sGO.AddComponent<RectTransform>();
        sRT.anchorMin = new Vector2(0.12f, 0.57f);
        sRT.anchorMax = new Vector2(0.88f, 0.635f);
        sRT.offsetMin = sRT.offsetMax = Vector2.zero;
        subtitleText           = sGO.AddComponent<TextMeshProUGUI>();
        subtitleText.text      = gameSubtitle;
        subtitleText.fontSize  = subtitleFontSize;
        if (bodyFont != null) subtitleText.font = bodyFont;
        subtitleText.alignment = TextAlignmentOptions.Center;
        subtitleText.color     = new Color(0.68f, 0.88f, 1.00f, 0.90f);

        // decorative separator lines
        MakeSeparator(menuLayer, new Vector2(0.20f, 0.565f), new Vector2(0.80f, 0.568f), C_CYAN);

        // ── Buttons ──
        (string label, Color color, System.Action action)[] btns =
        {
            (startBtnLabel,    C_CYAN,   StartGame),
            (settingsBtnLabel, C_PURPLE, OpenSettings),
            (quitBtnLabel,     C_RED,    QuitGame),
        };

        for (int i = 0; i < btns.Length; i++)
        {
            float yMax = 0.50f - i * 0.125f;
            float yMin = yMax - 0.095f;
            var btn = CreateButton(menuLayer, btns[i].label, btns[i].color,
                new Vector2(0.33f, yMin), new Vector2(0.67f, yMax));
            var action = btns[i].action;
            btn.onClick.AddListener(() => action());
        }

        // ── Version ──
        var vGO = new GameObject("Version");
        vGO.transform.SetParent(menuLayer, false);
        var vRT = vGO.AddComponent<RectTransform>();
        vRT.anchorMin = new Vector2(0.60f, 0.02f);
        vRT.anchorMax = new Vector2(0.99f, 0.065f);
        vRT.offsetMin = vRT.offsetMax = Vector2.zero;
        var vTxt = vGO.AddComponent<TextMeshProUGUI>();
        vTxt.text      = versionLabel;
        vTxt.fontSize  = versionFontSize;
        vTxt.alignment = TextAlignmentOptions.Right;
        vTxt.color     = C_DIM;
        if (bodyFont != null) vTxt.font = bodyFont;
    }

    Button CreateButton(RectTransform parent, string label, Color accent,
                        Vector2 ancMin, Vector2 ancMax)
    {
        // Outer (border)
        var outer = new GameObject($"Btn_{label}");
        outer.transform.SetParent(parent, false);
        var outerRT = outer.AddComponent<RectTransform>();
        outerRT.anchorMin = ancMin;
        outerRT.anchorMax = ancMax;
        outerRT.offsetMin = outerRT.offsetMax = Vector2.zero;
        var borderImg = outer.AddComponent<Image>();
        borderImg.color = new Color(accent.r, accent.g, accent.b, 0.65f);

        // Inner fill
        var inner = new GameObject("Fill");
        inner.transform.SetParent(outer.transform, false);
        var innerRT = inner.AddComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero;
        innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = new Vector2(2f, 2f);
        innerRT.offsetMax = new Vector2(-2f, -2f);
        var fillImg = inner.AddComponent<Image>();
        fillImg.color = new Color(accent.r * 0.07f, accent.g * 0.07f, accent.b * 0.14f, 0.92f);

        // Label
        var lGO = new GameObject("Label");
        lGO.transform.SetParent(inner.transform, false);
        var lRT = lGO.AddComponent<RectTransform>();
        lRT.anchorMin = Vector2.zero;
        lRT.anchorMax = Vector2.one;
        lRT.offsetMin = lRT.offsetMax = Vector2.zero;
        var lTxt = lGO.AddComponent<TextMeshProUGUI>();
        lTxt.text      = label;
        lTxt.fontSize  = buttonFontSize;
        lTxt.fontStyle = FontStyles.Bold;
        if (bodyFont != null) lTxt.font = bodyFont;
        lTxt.alignment = TextAlignmentOptions.Center;
        lTxt.color     = accent;

        // Button component + nav off
        var btn = outer.AddComponent<Button>();
        btn.targetGraphic = fillImg;
        var nav = Navigation.defaultNavigation;
        nav.mode = Navigation.Mode.None;
        btn.navigation = nav;

        // Hover behaviour
        var hover = outer.AddComponent<ButtonHoverEffect>();
        hover.Setup(outerRT, borderImg, fillImg, lTxt, accent);

        return btn;
    }

    void BuildFadeOverlay()
    {
        var go = new GameObject("FadeOverlay");
        go.transform.SetParent(mainCanvas.transform, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        fadeImg = go.AddComponent<Image>();
        fadeImg.color        = Color.black;
        fadeImg.raycastTarget = false;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Coroutines
    // ══════════════════════════════════════════════════════════════════════════
    IEnumerator IntroSequence()
    {
        fadeImg.color = Color.black;
        yield return new WaitForSeconds(0.2f);
        yield return StartCoroutine(FadeImage(fadeImg, 1f, 0f, 1.6f));

        var cg = menuLayer.GetComponent<CanvasGroup>();
        yield return StartCoroutine(FadeGroup(cg, 0f, 1f, 1.0f));
    }

    IEnumerator TitleGlowLoop()
    {
        while (true)
        {
            float t = Time.time;
            if (titleText != null)
            {
                float bob = Mathf.Sin(t * 1.4f) * 7f;
                titleText.rectTransform.anchoredPosition = new Vector2(0f, bob);

                float hue = Mathf.Repeat(t * 0.12f, 1f);
                titleText.outlineColor = Color.HSVToRGB(hue, 0.85f, 1f);
            }
            if (subtitleText != null)
            {
                float alpha = 0.70f + Mathf.Sin(t * 1.1f) * 0.18f;
                subtitleText.color = new Color(0.68f, 0.88f, 1f, alpha);
            }
            yield return null;
        }
    }

    IEnumerator PulseGlowLoop()
    {
        while (true)
        {
            float pulse = Mathf.Sin(Time.time * 0.65f) * 0.5f + 0.5f;
            if (glowImg != null)
                glowImg.color = new Color(1f, 1f, 1f, 0.55f + pulse * 0.30f);
            yield return null;
        }
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(0.4f, 1.2f));
            var sym = pool.Find(s => !s.gameObject.activeSelf);
            sym?.Activate();
        }
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Button actions
    // ══════════════════════════════════════════════════════════════════════════
    void StartGame()
    {
        if (transitioning) return;
        transitioning = true;
        StartCoroutine(LoadScene(gameSceneName));
    }

    void OpenSettings()
    {
        SettingsPanelOpener.Instance?.Toggle();
    }

    void QuitGame()
    {
        if (transitioning) return;
        transitioning = true;
        StartCoroutine(DoQuit());
    }

    IEnumerator LoadScene(string sceneName)
    {
        yield return StartCoroutine(FadeImage(fadeImg, 0f, 1f, 0.7f));
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator DoQuit()
    {
        yield return StartCoroutine(FadeImage(fadeImg, 0f, 1f, 0.7f));
        Application.Quit();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  Utilities
    // ══════════════════════════════════════════════════════════════════════════
    static RectTransform MakeFullRect(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        return rt;
    }

    static void MakeSeparator(RectTransform parent, Vector2 aMin, Vector2 aMax, Color c)
    {
        var go = new GameObject("Sep");
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = aMin; rt.anchorMax = aMax;
        rt.offsetMin = rt.offsetMax = Vector2.zero;
        var img = go.AddComponent<Image>();
        img.color        = new Color(c.r, c.g, c.b, 0.45f);
        img.raycastTarget = false;
    }

    static IEnumerator FadeImage(Image img, float from, float to, float dur)
    {
        Color c = img.color;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            c.a = Mathf.Lerp(from, to, t / dur);
            img.color = c; yield return null;
        }
        c.a = to; img.color = c;
    }

    static IEnumerator FadeGroup(CanvasGroup cg, float from, float to, float dur)
    {
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            cg.alpha = Mathf.Lerp(from, to, t / dur); yield return null;
        }
        cg.alpha = to;
    }

    // ── Procedural textures ────────────────────────────────────────────────────
    static Texture2D CreateGradientTex(int w, int h, Color top, Color bottom)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        for (int y = 0; y < h; y++)
        {
            Color c = Color.Lerp(bottom, top, y / (float)(h - 1));
            for (int x = 0; x < w; x++) tex.SetPixel(x, y, c);
        }
        tex.Apply(); return tex;
    }

    static Texture2D CreateGridTex(int w, int h, Color line)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        Color clear = new Color(0, 0, 0, 0);
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
                tex.SetPixel(x, y, (x == 0 || y == 0) ? line : clear);
        tex.Apply(); return tex;
    }

    static Texture2D CreateVignetteTex(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var ctr = new Vector2(w * 0.5f, h * 0.5f);
        float maxD = ctr.magnitude;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), ctr) / maxD;
                tex.SetPixel(x, y, new Color(0f, 0f, 0f, d * d * 0.85f));
            }
        tex.Apply(); return tex;
    }

    static Texture2D CreateRadialTex(int w, int h, Color center, Color edge)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        var ctr = new Vector2(w * 0.5f, h * 0.5f);
        float maxD = ctr.magnitude;
        for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float t = Vector2.Distance(new Vector2(x, y), ctr) / maxD;
                tex.SetPixel(x, y, Color.Lerp(center, edge, t));
            }
        tex.Apply(); return tex;
    }
}
