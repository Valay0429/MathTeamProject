using UnityEngine;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

/// <summary>
/// 첫 씬에 하나만 놓으면 DontDestroyOnLoad로 모든 씬에서 유지되며,
/// ESC 키로 설정창을 열고 닫습니다.
/// </summary>
public class SettingsPanelOpener : MonoBehaviour
{
    public static SettingsPanelOpener Instance { get; private set; }

    [Header("설정창 프리팹")]
    [SerializeField] SettingsPanel settingsPanelPrefab;

    SettingsPanel panel;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        if (settingsPanelPrefab != null) SpawnPanel();
    }

    /// <summary>코드로 생성 후 프리팹을 주입할 때 호출합니다.</summary>
    public void Initialize(SettingsPanel prefab)
    {
        settingsPanelPrefab = prefab;
        if (panel == null && settingsPanelPrefab != null) SpawnPanel();
    }

    void SpawnPanel()
    {
        if (settingsPanelPrefab == null)
        {
            Debug.LogWarning("[SettingsPanelOpener] settingsPanelPrefab이 비어 있습니다. Inspector에서 SettingsPanel 프리팹을 할당하세요.");
            return;
        }

        // 영구 캔버스 — sortingOrder 100으로 모든 씬 UI 위에 표시
        var canvasGO = new GameObject("SettingsOverlayCanvas");
        canvasGO.transform.SetParent(transform);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode   = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        var cs = canvasGO.AddComponent<CanvasScaler>();
        cs.uiScaleMode         = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        cs.referenceResolution = new Vector2(1920, 1080);
        cs.screenMatchMode     = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        cs.matchWidthOrHeight  = 0.5f;

        canvasGO.AddComponent<GraphicRaycaster>();

        panel = Instantiate(settingsPanelPrefab, canvasGO.transform);
        panel.gameObject.SetActive(false);
    }

    void Update()
    {
        if (EscapeDown()) Toggle();
    }

    static bool EscapeDown()
    {
#if ENABLE_INPUT_SYSTEM
        return Keyboard.current?.escapeKey.wasPressedThisFrame == true;
#else
        return Input.GetKeyDown(KeyCode.Escape);
#endif
    }

    public void Toggle()
    {
        if (panel == null) return;
        if (panel.gameObject.activeSelf) panel.Hide();
        else                             panel.Show();
    }

    public void OpenPanel()  => panel?.Show();
    public void ClosePanel() => panel?.Hide();
}
