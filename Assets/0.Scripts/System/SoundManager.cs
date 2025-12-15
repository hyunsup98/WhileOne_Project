using UnityEngine;
using UnityEngine.SceneManagement;

public enum SoundType
{
    BGM,            //배경음
    SFX,            //효과음
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

    #region 오디오 볼륨 조절
    //실질적인 오디오소스 볼륨 설정
    public void SetSoundVolume(SoundType type, float volume)
    {
        switch(type)
        {
            case SoundType.BGM:
                SetVolume(_bgmAudioSource, volume);
                break;
            case SoundType.SFX:
                SetVolume(_sfxAudioSource, volume);
                break;
        }
    }

    //오디오소스 볼륨 설정 뼈대
    private void SetVolume(AudioSource source, float volume)
    {
        if (source == null) return;

        source.volume = volume;
    }
    #endregion

    //오디오소스 재생 정지
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

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
