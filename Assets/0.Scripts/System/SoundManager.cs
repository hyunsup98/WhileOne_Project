using UnityEngine;
using UnityEngine.SceneManagement;

public enum SoundType
{
    BGM,            //배경음
    SoundEffect     //효과음
}

//todo
//현재 단계에서 효과음이 얼마나 재생되는지 모르기 때문에 효과음 재생은 PlayOneShot으로 재생
//추후 효과음이 많아져 최적화의 필요성이 보이면 풀링으로 변경할 예정

/// <summary>
/// 사운드 재생을 담당하는 싱글턴 매니저 클래스
/// </summary>
public class SoundManager : Singleton<SoundManager>
{
    [SerializeField] private AudioSource _bgmAudioSource;
    [SerializeField] private AudioSource _sfxAudioSource;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    #region 사운드 재생
    public void PlayBGM(AudioClip clip)
    {
        if (_bgmAudioSource == null || clip == null) return;

        _bgmAudioSource.clip = clip;
        _bgmAudioSource.Play();
    }

    public void PlaySoundEffect(AudioClip clip)
    {
        if (_sfxAudioSource == null || clip == null) return;

        _sfxAudioSource.PlayOneShot(clip);
    }
    #endregion

    private void StopAudio(AudioSource source)
    {
        if(source != null && source.isPlaying)
        {
            source.Stop();
        }
    }

    //씬 이동 시 호출될 메서드
    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //브금이나 효과음이 재생 중이라면 재생 중지
        StopAudio(_bgmAudioSource);
        StopAudio(_sfxAudioSource);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
