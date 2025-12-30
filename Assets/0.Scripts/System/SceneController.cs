using UnityEngine;

/// <summary>
/// 각 씬에서 
/// </summary>
public class SceneController : MonoBehaviour
{
    [SerializeField] private string _bgmID;      // BGM 데이터베이스에 접근하여 해당 오디오 클립을 가져오기 위한 ID

    private void Start()
    {
        // 브금 아이디를 이용해 데이터매니저에 접근해 오디오 클립 가져오기
        if (!string.IsNullOrEmpty(_bgmID))
            SoundManager.Instance.PlayBGM(_bgmID);
    }
}
