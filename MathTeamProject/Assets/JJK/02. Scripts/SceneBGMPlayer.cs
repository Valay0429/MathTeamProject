using UnityEngine;

public class SceneBGMPlayer : MonoBehaviour
{
    [SerializeField] private BGM bgm;

    private void Start()
    {
        SoundManager.Instance?.PlayBGM(bgm);
    }
}
