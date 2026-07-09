using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    public float MasterVolume { get; private set; }
    public float BGMVolume    { get; private set; }
    public float SFXVolume    { get; private set; }
    public bool  Fullscreen   { get; private set; }
    public int   QualityLevel { get; private set; }
    public int   ResolutionIndex { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        Load();
    }

    public void Load()
    {
        MasterVolume    = PlayerPrefs.GetFloat("MasterVolume", 1f);
        BGMVolume       = PlayerPrefs.GetFloat("BGMVolume", 0.7f);
        SFXVolume       = PlayerPrefs.GetFloat("SFXVolume", 0.8f);
        Fullscreen      = PlayerPrefs.GetInt("Fullscreen", 0) == 1;
        QualityLevel    = PlayerPrefs.GetInt("Quality", 2);
        ResolutionIndex = PlayerPrefs.GetInt("Resolution", -1);
        Apply();
    }

    public void Save(float master, float bgm, float sfx, bool fullscreen, int quality, int resIndex)
    {
        MasterVolume = master; BGMVolume = bgm; SFXVolume = sfx;
        Fullscreen   = fullscreen; QualityLevel = quality; ResolutionIndex = resIndex;

        PlayerPrefs.SetFloat("MasterVolume", master);
        PlayerPrefs.SetFloat("BGMVolume",    bgm);
        PlayerPrefs.SetFloat("SFXVolume",    sfx);
        PlayerPrefs.SetInt("Fullscreen",     fullscreen ? 1 : 0);
        PlayerPrefs.SetInt("Quality",        quality);
        PlayerPrefs.SetInt("Resolution",     resIndex);
        PlayerPrefs.Save();
        Apply();
    }

    void Apply()
    {
        Screen.fullScreen = Fullscreen;
        QualitySettings.SetQualityLevel(QualityLevel, true);
        Resolution[] res = Screen.resolutions;
        if (ResolutionIndex >= 0 && ResolutionIndex < res.Length)
            Screen.SetResolution(res[ResolutionIndex].width, res[ResolutionIndex].height, Fullscreen);

        // SoundManager가 씬에 존재하면 볼륨 즉시 반영
        var sm = FindFirstObjectByType<SoundManager>();
        if (sm != null)
        {
            sm.SetMasterVolume(MasterVolume);
            sm.SetBgmVolume(BGMVolume);
            sm.SetSfxVolume(SFXVolume);
        }
    }
}
