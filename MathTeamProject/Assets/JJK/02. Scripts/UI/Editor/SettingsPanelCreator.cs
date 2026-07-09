#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Menu: Tools > Math Quest > 설정창 프리팹 생성
/// Assets/JJK/03. Prefabs/SettingsPanel.prefab 을 생성 또는 덮어씁니다.
/// 생성 후 TitleSceneManager의 Settings Panel Prefab 슬롯에 드래그하세요.
/// </summary>
public static class SettingsPanelCreator
{
    const string PrefabPath = "Assets/JJK/03. Prefabs/SettingsPanel.prefab";

    // ── Color Palette ──────────────────────────────────────────────────────────
    static readonly Color C_PANEL  = new Color(0.01f, 0.03f, 0.13f, 0.96f);
    static readonly Color C_BORDER = new Color(0.00f, 0.88f, 1.00f, 0.70f);
    static readonly Color C_HEAD   = new Color(0.00f, 0.88f, 1.00f, 1.00f);
    static readonly Color C_TEXT   = new Color(0.80f, 0.92f, 1.00f, 1.00f);
    static readonly Color C_DIM    = new Color(0.45f, 0.60f, 0.75f, 0.85f);
    static readonly Color C_SEP    = new Color(0.15f, 0.40f, 0.65f, 0.50f);
    static readonly Color C_APPLY  = new Color(0.00f, 0.88f, 1.00f, 1.00f);
    static readonly Color C_CANCEL = new Color(0.85f, 0.30f, 0.30f, 1.00f);
    static readonly Color C_TRACK  = new Color(0.08f, 0.12f, 0.25f, 1.00f);

    // ── Menu Entry ─────────────────────────────────────────────────────────────
    [MenuItem("Tools/Math Quest/⚙ 설정창 프리팹 생성 (덮어쓰기)")]
    public static void CreatePrefab()
    {
        Directory.CreateDirectory("Assets/JJK/03. Prefabs");

        var root = BuildUI(out var refs);

        // ★ 참조 연결을 SaveAsPrefabAsset 전에 씬 오브젝트에서 직접 처리
        //   (저장 후에 하면 DestroyImmediate로 참조가 끊겨 null이 됨)
        var sp = root.GetComponent<SettingsPanel>();
        sp.EditorSetReferences(
            refs.masterSlider, refs.bgmSlider,   refs.sfxSlider,
            refs.masterVal,    refs.bgmVal,       refs.sfxVal,
            refs.resDropdown,  refs.qualDropdown,
            refs.fullToggle,   refs.fullToggleLabel,
            refs.panelRT,      refs.panelCG,
            refs.applyBtn,     refs.cancelBtn,    refs.closeBtn);

        bool ok;
        var prefab = PrefabUtility.SaveAsPrefabAsset(root, PrefabPath, out ok);
        Object.DestroyImmediate(root);

        if (!ok) { Debug.LogError($"[SettingsPanelCreator] 프리팹 저장 실패 ({PrefabPath})"); return; }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeObject = prefab;
        EditorGUIUtility.PingObject(prefab);
        Debug.Log($"[SettingsPanelCreator] ✅ 완료: {PrefabPath}");
    }

    // ── UI 계층 구조 생성 ──────────────────────────────────────────────────────
    struct Refs
    {
        public Slider          masterSlider, bgmSlider, sfxSlider;
        public TextMeshProUGUI masterVal,    bgmVal,    sfxVal;
        public TMP_Dropdown    resDropdown,  qualDropdown;
        public Toggle          fullToggle;
        public TextMeshProUGUI fullToggleLabel;
        public RectTransform   panelRT;
        public CanvasGroup     panelCG;
        public Button          applyBtn, cancelBtn, closeBtn;
    }

    static GameObject BuildUI(out Refs r)
    {
        r = new Refs();

        // ── 루트 (풀스크린 오버레이) ──
        var root   = new GameObject("SettingsPanel");
        var rootRT = root.AddComponent<RectTransform>();
        rootRT.anchorMin = Vector2.zero; rootRT.anchorMax = Vector2.one;
        rootRT.offsetMin = rootRT.offsetMax = Vector2.zero;
        var rootImg = root.AddComponent<Image>();
        rootImg.color = new Color(0f, 0f, 0f, 0.72f);
        var overlayBtn = root.AddComponent<Button>();
        overlayBtn.transition = Selectable.Transition.None;

        // ── 테두리 (border) ──
        var borderGO = new GameObject("Border");
        borderGO.transform.SetParent(root.transform, false);
        var borderRT = borderGO.AddComponent<RectTransform>();
        SetCentred(borderRT, 614f, 730f);
        var borderImg2 = borderGO.AddComponent<Image>();
        borderImg2.color = C_BORDER;
        borderImg2.raycastTarget = false;

        // ── 패널 내부 ──
        var panelGO = new GameObject("Panel");
        panelGO.transform.SetParent(borderGO.transform, false);
        r.panelRT          = panelGO.AddComponent<RectTransform>();
        r.panelRT.anchorMin = Vector2.zero; r.panelRT.anchorMax = Vector2.one;
        r.panelRT.offsetMin = new Vector2(2f, 2f); r.panelRT.offsetMax = new Vector2(-2f, -2f);
        panelGO.AddComponent<Image>().color = C_PANEL;
        var eatBtn = panelGO.AddComponent<Button>();
        eatBtn.transition  = Selectable.Transition.None;
        r.panelCG          = panelGO.AddComponent<CanvasGroup>();
        r.panelCG.alpha    = 1f;

        Transform p = panelGO.transform;

        // ── 제목 + 닫기 버튼 ──
        MakeLabel(p, "⚙  설  정", 32f, FontStyles.Bold, C_HEAD, 0f, 290f, 560f, 50f, TextAlignmentOptions.Center);
        r.closeBtn = MakeCloseButton(p, 250f, 295f);
        MakeSep(p, 0f, 255f, 520f);

        // ── AUDIO ──
        MakeSectionHeader(p, "AUDIO", 0f, 228f);
        r.masterSlider = MakeSliderRow(p, "전체 음량", 0f, 182f, out r.masterVal);
        r.bgmSlider    = MakeSliderRow(p, "BGM  음량", 0f, 130f, out r.bgmVal);
        r.sfxSlider    = MakeSliderRow(p, "효과음   ", 0f,  78f, out r.sfxVal);

        // ── 화면 설정 ──
        MakeSep(p, 0f, 45f, 520f);
        MakeSectionHeader(p, "화면 설정", 0f, 18f);
        r.resDropdown  = MakeDropdownRow(p, "해상도",      0f, -30f);
        r.fullToggle   = MakeToggleRow(p,  "전체화면",    0f, -80f, out r.fullToggleLabel);
        r.qualDropdown = MakeDropdownRow(p, "그래픽 품질", 0f, -130f);

        // ── 하단 버튼 ──
        MakeSep(p, 0f, -165f, 520f);
        MakeActionButtons(p, 0f, -210f, out r.applyBtn, out r.cancelBtn);

        // SettingsPanel 컴포넌트는 root에 추가 (WireReferences에서 SerializedObject로 연결)
        root.AddComponent<SettingsPanel>();

        return root;
    }

    // ══════════════════════════════════════════════════════════════════════════
    //  UI 빌더 헬퍼
    // ══════════════════════════════════════════════════════════════════════════
    static TextMeshProUGUI MakeLabel(Transform parent, string text, float size,
        FontStyles style, Color c, float x, float y, float w, float h,
        TextAlignmentOptions align = TextAlignmentOptions.Left)
    {
        var go = new GameObject(text.Length > 12 ? text[..12].Trim() : text.Trim());
        go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(w, h); rt.anchoredPosition = new Vector2(x, y);
        var tmp = go.AddComponent<TextMeshProUGUI>();
        tmp.text = text; tmp.fontSize = size; tmp.fontStyle = style;
        tmp.color = c;   tmp.alignment = align;
        return tmp;
    }

    static void MakeSectionHeader(Transform parent, string text, float x, float y)
    {
        MakeLabel(parent, text, 15f, FontStyles.Bold, C_HEAD, x - 220f, y, 200f, 26f);
        var bar = new GameObject("Bar"); bar.transform.SetParent(parent, false);
        var rt = bar.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(200f, 2f); rt.anchoredPosition = new Vector2(x - 220f, y - 14f);
        var img = bar.AddComponent<Image>(); img.color = new Color(C_HEAD.r, C_HEAD.g, C_HEAD.b, 0.4f);
        img.raycastTarget = false;
    }

    static Slider MakeSliderRow(Transform parent, string label, float x, float y,
                                out TextMeshProUGUI valLbl)
    {
        MakeLabel(parent, label, 16f, FontStyles.Normal, C_TEXT, x - 248f, y, 150f, 36f);

        var sGO = new GameObject($"Slider_{label.Trim()}");
        sGO.transform.SetParent(parent, false);
        var sRT = sGO.AddComponent<RectTransform>();
        sRT.sizeDelta = new Vector2(270f, 18f); sRT.anchoredPosition = new Vector2(x + 20f, y);

        // BG
        var bg = Child(sGO.transform, "BG", Vector2.zero, Vector2.one);
        bg.AddComponent<Image>().color = C_TRACK;

        // FillArea
        var fillArea = Child(sGO.transform, "Fill Area", Vector2.zero, Vector2.one, new Vector2(5,0), new Vector2(-5,0));
        var fillChild = Child(fillArea.transform, "Fill", Vector2.zero, Vector2.one);
        var fillChildImg = fillChild.AddComponent<Image>(); fillChildImg.color = C_APPLY;

        // Handle
        var handleGO = new GameObject("Handle");
        handleGO.transform.SetParent(sGO.transform, false);
        var handleRT = handleGO.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20f, 20f);
        var handleImg = handleGO.AddComponent<Image>(); handleImg.color = Color.white;

        var slider = sGO.AddComponent<Slider>();
        slider.fillRect      = fillChild.GetComponent<RectTransform>();
        slider.handleRect    = handleRT;
        slider.targetGraphic = handleImg;
        slider.direction     = Slider.Direction.LeftToRight;
        slider.minValue = 0f; slider.maxValue = 1f; slider.value = 0.8f;

        valLbl = MakeLabel(parent, "80%", 15f, FontStyles.Normal, C_DIM,
            x + 170f, y, 60f, 36f, TextAlignmentOptions.Center);
        return slider;
    }

    static TMP_Dropdown MakeDropdownRow(Transform parent, string label, float x, float y)
    {
        MakeLabel(parent, label, 16f, FontStyles.Normal, C_TEXT, x - 248f, y, 150f, 36f);

        var dGO = new GameObject($"DD_{label.Trim()}");
        dGO.transform.SetParent(parent, false);
        var dRT = dGO.AddComponent<RectTransform>();
        dRT.sizeDelta = new Vector2(270f, 36f); dRT.anchoredPosition = new Vector2(x + 60f, y);
        var dImg = dGO.AddComponent<Image>(); dImg.color = C_TRACK;

        // Caption label
        var capGO = new GameObject("Label");
        capGO.transform.SetParent(dGO.transform, false);
        var capRT = capGO.AddComponent<RectTransform>();
        capRT.anchorMin = Vector2.zero; capRT.anchorMax = Vector2.one;
        capRT.offsetMin = new Vector2(8f, 0f); capRT.offsetMax = new Vector2(-30f, 0f);
        var capTxt = capGO.AddComponent<TextMeshProUGUI>();
        capTxt.fontSize = 14f; capTxt.color = C_TEXT; capTxt.alignment = TextAlignmentOptions.Left;

        // Arrow
        var arrGO = new GameObject("Arrow"); arrGO.transform.SetParent(dGO.transform, false);
        var arrRT = arrGO.AddComponent<RectTransform>();
        arrRT.anchorMin = new Vector2(1f,0.5f); arrRT.anchorMax = new Vector2(1f,0.5f);
        arrRT.pivot = new Vector2(1f,0.5f); arrRT.sizeDelta = new Vector2(24f,24f);
        arrRT.anchoredPosition = new Vector2(-4f, 0f);
        var arrTxt = arrGO.AddComponent<TextMeshProUGUI>();
        arrTxt.text = "▼"; arrTxt.fontSize = 12f; arrTxt.color = C_DIM;
        arrTxt.alignment = TextAlignmentOptions.Center;

        // Template
        var tplGO = new GameObject("Template"); tplGO.transform.SetParent(dGO.transform, false);
        var tplRT = tplGO.AddComponent<RectTransform>();
        tplRT.anchorMin = new Vector2(0f,0f); tplRT.anchorMax = new Vector2(1f,0f);
        tplRT.pivot = new Vector2(0.5f,1f); tplRT.sizeDelta = new Vector2(0f,150f);
        tplGO.AddComponent<Image>().color = new Color(0.05f,0.08f,0.20f,0.98f);
        var tplScroll = tplGO.AddComponent<ScrollRect>(); tplScroll.horizontal = false;

        var vp = Child(tplGO.transform, "Viewport", Vector2.zero, Vector2.one);
        vp.GetComponent<RectTransform>().offsetMin = vp.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        vp.AddComponent<Image>();
        vp.AddComponent<Mask>().showMaskGraphic = false;

        var content = new GameObject("Content"); content.transform.SetParent(vp.transform, false);
        var contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0f,1f); contentRT.anchorMax = new Vector2(1f,1f);
        contentRT.pivot = new Vector2(0.5f,1f); contentRT.sizeDelta = new Vector2(0f,28f);

        // Item template
        var item = new GameObject("Item"); item.transform.SetParent(content.transform, false);
        var itemRT = item.AddComponent<RectTransform>();
        itemRT.anchorMin = new Vector2(0f,0.5f); itemRT.anchorMax = new Vector2(1f,0.5f);
        itemRT.sizeDelta = new Vector2(0f,28f);
        var iToggle = item.AddComponent<Toggle>();

        var iBG = Child(item.transform, "Item Background", Vector2.zero, Vector2.one);
        var iBGImg = iBG.AddComponent<Image>(); iBGImg.color = new Color(0f,0.3f,0.5f,0.4f);

        var iCheckGO = new GameObject("Item Checkmark"); iCheckGO.transform.SetParent(item.transform, false);
        var iCheckRT = iCheckGO.AddComponent<RectTransform>();
        iCheckRT.anchorMin = Vector2.zero; iCheckRT.anchorMax = new Vector2(0f,1f);
        iCheckRT.offsetMin = new Vector2(4f,4f); iCheckRT.offsetMax = new Vector2(28f,-4f);
        var iCheckImg = iCheckGO.AddComponent<Image>(); iCheckImg.color = C_APPLY;

        var iLblGO = new GameObject("Item Label"); iLblGO.transform.SetParent(item.transform, false);
        var iLblRT = iLblGO.AddComponent<RectTransform>();
        iLblRT.anchorMin = Vector2.zero; iLblRT.anchorMax = Vector2.one;
        iLblRT.offsetMin = new Vector2(32f,0f); iLblRT.offsetMax = Vector2.zero;
        var iLblTxt = iLblGO.AddComponent<TextMeshProUGUI>();
        iLblTxt.fontSize = 14f; iLblTxt.color = C_TEXT; iLblTxt.alignment = TextAlignmentOptions.Left;

        iToggle.targetGraphic = iBGImg;
        iToggle.graphic       = iCheckImg;
        tplScroll.content     = contentRT;
        tplScroll.viewport    = vp.GetComponent<RectTransform>();
        tplGO.SetActive(false);

        var dd = dGO.AddComponent<TMP_Dropdown>();
        dd.template      = tplRT;
        dd.captionText   = capTxt;
        dd.itemText      = iLblTxt;
        dd.targetGraphic = dImg;

        return dd;
    }

    static Toggle MakeToggleRow(Transform parent, string label, float x, float y,
                                out TextMeshProUGUI stateLbl)
    {
        MakeLabel(parent, label, 16f, FontStyles.Normal, C_TEXT, x - 248f, y, 150f, 36f);

        var tGO = new GameObject($"Toggle_{label.Trim()}");
        tGO.transform.SetParent(parent, false);
        var tRT = tGO.AddComponent<RectTransform>();
        tRT.sizeDelta = new Vector2(52f,28f); tRT.anchoredPosition = new Vector2(x + 40f, y);
        var bgImg = tGO.AddComponent<Image>(); bgImg.color = C_TRACK;

        var ck = Child(tGO.transform, "Checkmark", Vector2.zero, Vector2.one, new Vector2(2,2), new Vector2(-2,-2));
        var ckImg = ck.AddComponent<Image>(); ckImg.color = C_APPLY;

        var toggle = tGO.AddComponent<Toggle>();
        toggle.targetGraphic = bgImg; toggle.graphic = ckImg; toggle.isOn = false;

        stateLbl = MakeLabel(parent, "OFF", 14f, FontStyles.Normal, C_DIM,
            x + 80f, y, 60f, 36f, TextAlignmentOptions.Left);
        return toggle;
    }

    static void MakeActionButtons(Transform parent, float x, float y,
                                  out Button applyBtn, out Button cancelBtn)
    {
        applyBtn  = MakeActionButton(parent, "적   용", C_APPLY,  x - 80f, y);
        cancelBtn = MakeActionButton(parent, "취   소", C_CANCEL, x + 80f, y);
    }

    static Button MakeActionButton(Transform parent, string label, Color accent, float x, float y)
    {
        var outer = new GameObject($"Btn_{label.Trim()}");
        outer.transform.SetParent(parent, false);
        var oRT = outer.AddComponent<RectTransform>();
        oRT.sizeDelta = new Vector2(140f,44f); oRT.anchoredPosition = new Vector2(x, y);
        var borderImg = outer.AddComponent<Image>(); borderImg.color = new Color(accent.r,accent.g,accent.b,0.70f);

        var inner = Child(outer.transform, "Fill", Vector2.zero, Vector2.one, new Vector2(2,2), new Vector2(-2,-2));
        var fillImg = inner.AddComponent<Image>();
        fillImg.color = new Color(accent.r*0.08f, accent.g*0.08f, accent.b*0.15f, 0.95f);

        var lGO = Child(inner.transform, "Label", Vector2.zero, Vector2.one);
        var lTxt = lGO.AddComponent<TextMeshProUGUI>();
        lTxt.text = label; lTxt.fontSize = 20f; lTxt.fontStyle = FontStyles.Bold;
        lTxt.alignment = TextAlignmentOptions.Center; lTxt.color = accent;

        var btn = outer.AddComponent<Button>();
        btn.targetGraphic = fillImg;
        var nav = Navigation.defaultNavigation; nav.mode = Navigation.Mode.None; btn.navigation = nav;

        var hover = outer.AddComponent<ButtonHoverEffect>();
        hover.Setup(oRT, borderImg, fillImg, lTxt, accent);

        return btn;
    }

    static Button MakeCloseButton(Transform parent, float x, float y)
    {
        var go = new GameObject("CloseBtn"); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(38f,38f); rt.anchoredPosition = new Vector2(x, y);
        var img = go.AddComponent<Image>(); img.color = new Color(0.8f,0.2f,0.2f,0.6f);

        var lGO = Child(go.transform, "X", Vector2.zero, Vector2.one);
        var lTxt = lGO.AddComponent<TextMeshProUGUI>();
        lTxt.text = "✕"; lTxt.fontSize = 20f;
        lTxt.alignment = TextAlignmentOptions.Center; lTxt.color = Color.white;

        var btn = go.AddComponent<Button>(); btn.targetGraphic = img;
        return btn;
    }

    static void MakeSep(Transform parent, float x, float y, float width)
    {
        var go = new GameObject("Sep"); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.sizeDelta = new Vector2(width, 1f); rt.anchoredPosition = new Vector2(x, y);
        var img = go.AddComponent<Image>(); img.color = C_SEP; img.raycastTarget = false;
    }

    // ── 공통 유틸 ──────────────────────────────────────────────────────────────
    static void SetCentred(RectTransform rt, float w, float h)
    {
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = new Vector2(w, h); rt.anchoredPosition = Vector2.zero;
    }

    static GameObject Child(Transform parent, string name,
        Vector2 ancMin, Vector2 ancMax,
        Vector2 offsetMin = default, Vector2 offsetMax = default)
    {
        var go = new GameObject(name); go.transform.SetParent(parent, false);
        var rt = go.AddComponent<RectTransform>();
        rt.anchorMin = ancMin; rt.anchorMax = ancMax;
        rt.offsetMin = offsetMin; rt.offsetMax = offsetMax;
        return go;
    }
}
#endif
