using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 설정창 로직 전담 스크립트.
/// UI 계층 구조는 Editor/SettingsPanelCreator 메뉴로 생성된 프리팹에 정의됩니다.
/// </summary>
public class SettingsPanel : MonoBehaviour
{
    // ── 오디오 슬라이더 ───────────────────────────────────────────────────────
    [Header("오디오 슬라이더")]
    [SerializeField] Slider          masterSlider;
    [SerializeField] Slider          bgmSlider;
    [SerializeField] Slider          sfxSlider;
    [SerializeField] TextMeshProUGUI masterVal;
    [SerializeField] TextMeshProUGUI bgmVal;
    [SerializeField] TextMeshProUGUI sfxVal;

    // ── 화면 설정 ─────────────────────────────────────────────────────────────
    [Header("화면 설정")]
    [SerializeField] TMP_Dropdown    resDropdown;
    [SerializeField] TMP_Dropdown    qualDropdown;
    [SerializeField] Toggle          fullToggle;
    [SerializeField] TextMeshProUGUI fullToggleLabel;

    // ── 패널 애니메이션 ───────────────────────────────────────────────────────
    [Header("패널 루트 (애니메이션용)")]
    [SerializeField] RectTransform panelRT;
    [SerializeField] CanvasGroup   panelCG;

    // ── 버튼 ──────────────────────────────────────────────────────────────────
    [Header("버튼")]
    [SerializeField] Button applyBtn;
    [SerializeField] Button cancelBtn;
    [SerializeField] Button closeBtn;

    static readonly Color C_APPLY = new Color(0.00f, 0.88f, 1.00f, 1f);
    static readonly Color C_DIM   = new Color(0.45f, 0.60f, 0.75f, 0.85f);

    static readonly string[] QualityNames =
        { "매우 낮음", "낮음", "보통", "높음", "매우 높음", "Ultra" };

    bool animating;
    bool populated;

    // ──────────────────────────────────────────────────────────────────────────
    void Awake()
    {
        AutoBind();
        WireEvents();
    }

    // ── 계층 구조에서 자동으로 참조 수집 (프리팹 refs가 null일 때 폴백) ────────
    void AutoBind()
    {
        var panel = transform.Find("Border/Panel");
        if (panel == null) return;

        if (!panelRT)  panelRT  = panel.GetComponent<RectTransform>();
        if (!panelCG)  panelCG  = panel.GetComponent<CanvasGroup>();
        if (!closeBtn) closeBtn = panel.Find("CloseBtn")?.GetComponent<Button>();

        if (!masterSlider) masterSlider = panel.Find("Slider_전체 음량")?.GetComponent<Slider>();
        if (!bgmSlider)    bgmSlider    = panel.Find("Slider_BGM  음량")?.GetComponent<Slider>();
        if (!sfxSlider)    sfxSlider    = panel.Find("Slider_효과음")?.GetComponent<Slider>();

        if (!resDropdown)  resDropdown  = panel.Find("DD_해상도")?.GetComponent<TMP_Dropdown>();
        if (!qualDropdown) qualDropdown = panel.Find("DD_그래픽 품질")?.GetComponent<TMP_Dropdown>();

        if (!fullToggle)      fullToggle      = panel.Find("Toggle_전체화면")?.GetComponent<Toggle>();
        if (!fullToggleLabel) fullToggleLabel = panel.Find("OFF")?.GetComponent<TextMeshProUGUI>();

        if (!applyBtn)  applyBtn  = panel.Find("Btn_적   용")?.GetComponent<Button>();
        if (!cancelBtn) cancelBtn = panel.Find("Btn_취   소")?.GetComponent<Button>();

        if (!masterVal || !bgmVal || !sfxVal)
        {
            int idx = 0;
            foreach (Transform child in panel)
            {
                if (child.name != "80%") continue;
                var tmp = child.GetComponent<TextMeshProUGUI>();
                if (idx == 0 && !masterVal) masterVal = tmp;
                else if (idx == 1 && !bgmVal) bgmVal = tmp;
                else if (idx == 2 && !sfxVal) sfxVal = tmp;
                idx++;
            }
        }
    }

    void Start()
    {
        Populate();
    }

    // ── 이벤트 연결 ───────────────────────────────────────────────────────────
    public void WireEvents()
    {
        if (masterSlider) masterSlider.onValueChanged.AddListener(v => SetPct(masterVal, v));
        if (bgmSlider)    bgmSlider.onValueChanged.AddListener(v    => SetPct(bgmVal,    v));
        if (sfxSlider)    sfxSlider.onValueChanged.AddListener(v    => SetPct(sfxVal,    v));

        if (fullToggle && fullToggleLabel)
            fullToggle.onValueChanged.AddListener(on =>
            {
                fullToggleLabel.text  = on ? "ON" : "OFF";
                fullToggleLabel.color = on ? C_APPLY : C_DIM;
            });

        if (applyBtn)  applyBtn.onClick.AddListener(ApplyAndClose);
        if (cancelBtn) cancelBtn.onClick.AddListener(Hide);
        if (closeBtn)  closeBtn.onClick.AddListener(Hide);

        // 오버레이(배경) 클릭 → 닫기
        var overlay = GetComponent<Button>();
        if (overlay) { overlay.onClick.RemoveAllListeners(); overlay.onClick.AddListener(Hide); }
    }

    static void SetPct(TextMeshProUGUI lbl, float v)
    {
        if (lbl) lbl.text = Mathf.RoundToInt(v * 100) + "%";
    }

    // ── Show / Hide ───────────────────────────────────────────────────────────
    public void Show()
    {
        LoadCurrentValues();
        if (animating) return;
        gameObject.SetActive(true);
        StartCoroutine(AnimateIn());
    }

    public void Hide()
    {
        if (animating) return;
        StartCoroutine(AnimateOut());
    }

    IEnumerator AnimateIn()
    {
        animating = true;
        if (panelCG) panelCG.alpha      = 0f;
        if (panelRT) panelRT.localScale = Vector3.one * 0.82f;
        float dur = 0.22f;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float k = t / dur;
            if (panelCG) panelCG.alpha      = k;
            if (panelRT) panelRT.localScale = Vector3.Lerp(Vector3.one * 0.82f, Vector3.one, k);
            yield return null;
        }
        if (panelCG) panelCG.alpha      = 1f;
        if (panelRT) panelRT.localScale = Vector3.one;
        animating = false;
    }

    IEnumerator AnimateOut()
    {
        animating = true;
        float a0  = panelCG ? panelCG.alpha : 1f;
        float dur = 0.16f;
        for (float t = 0; t < dur; t += Time.deltaTime)
        {
            float k = t / dur;
            if (panelCG) panelCG.alpha      = Mathf.Lerp(a0, 0f, k);
            if (panelRT) panelRT.localScale = Vector3.Lerp(Vector3.one, Vector3.one * 0.82f, k);
            yield return null;
        }
        if (panelCG) panelCG.alpha = 0f;
        animating = false;
        gameObject.SetActive(false);
    }

    // ── 데이터 ────────────────────────────────────────────────────────────────
    void Populate()
    {
        if (populated) return;
        populated = true;
        PopulateResolutions();
        PopulateQualities();
    }

    void LoadCurrentValues()
    {
        Populate();
        var gs = GameSettings.Instance;
        if (gs == null) return;
        if (masterSlider) { masterSlider.value = gs.MasterVolume; SetPct(masterVal, gs.MasterVolume); }
        if (bgmSlider)    { bgmSlider.value    = gs.BGMVolume;    SetPct(bgmVal,    gs.BGMVolume); }
        if (sfxSlider)    { sfxSlider.value    = gs.SFXVolume;    SetPct(sfxVal,    gs.SFXVolume); }
        if (fullToggle)   fullToggle.isOn = gs.Fullscreen;
        if (qualDropdown) qualDropdown.value = Mathf.Clamp(gs.QualityLevel, 0, qualDropdown.options.Count - 1);
        int ri = gs.ResolutionIndex;
        if (resDropdown && ri >= 0 && ri < resDropdown.options.Count) resDropdown.value = ri;
    }

    void PopulateResolutions()
    {
        if (!resDropdown) return;
        resDropdown.ClearOptions();
        var opts = new List<string>();
        var seen = new HashSet<string>();
        foreach (var r in Screen.resolutions)
        {
            string s = $"{r.width} × {r.height}";
            if (seen.Add(s)) opts.Add(s);
        }
        if (opts.Count == 0) opts.Add("1920 × 1080");
        resDropdown.AddOptions(opts);
        resDropdown.value = resDropdown.options.Count - 1;
    }

    void PopulateQualities()
    {
        if (!qualDropdown) return;
        qualDropdown.ClearOptions();
        qualDropdown.AddOptions(new List<string>(QualityNames));
    }

    void ApplyAndClose()
    {
        GameSettings.Instance?.Save(
            masterSlider  ? masterSlider.value  : 1f,
            bgmSlider     ? bgmSlider.value     : 0.7f,
            sfxSlider     ? sfxSlider.value     : 0.8f,
            fullToggle    ? fullToggle.isOn      : false,
            qualDropdown  ? qualDropdown.value  : 2,
            resDropdown   ? resDropdown.value   : -1);
        Hide();
    }

#if UNITY_EDITOR
    /// <summary>프리팹 생성 스크립트(SettingsPanelCreator)가 SaveAsPrefabAsset 전에 호출합니다.</summary>
    public void EditorSetReferences(
        Slider master,           Slider bgm,           Slider sfx,
        TextMeshProUGUI masterV, TextMeshProUGUI bgmV, TextMeshProUGUI sfxV,
        TMP_Dropdown res,        TMP_Dropdown qual,
        Toggle fullTog,          TextMeshProUGUI fullTogLbl,
        RectTransform pRT,       CanvasGroup pCG,
        Button apply,            Button cancel,        Button close)
    {
        masterSlider    = master;
        bgmSlider       = bgm;
        sfxSlider       = sfx;
        masterVal       = masterV;
        bgmVal          = bgmV;
        sfxVal          = sfxV;
        resDropdown     = res;
        qualDropdown    = qual;
        fullToggle      = fullTog;
        fullToggleLabel = fullTogLbl;
        panelRT         = pRT;
        panelCG         = pCG;
        applyBtn        = apply;
        cancelBtn       = cancel;
        closeBtn        = close;
    }
#endif
}
