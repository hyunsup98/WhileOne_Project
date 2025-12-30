using UnityEngine;

[CreateAssetMenu(fileName = "SFXDataSO", menuName = "Scriptable Objects/Data/SFXDataSO")]
public class SFXDataSO : TableBase<string>
{
    // SFX ID
    [field: SerializeField] public string sfxID { get; private set; }

    // SFX 객체
    [field: SerializeField] public string sfxObject { get; private set; }

    // SFX사용처
    [field: SerializeField] public string sfxUse { get; private set; }

    // SFX경로
    [field: SerializeField] public AudioClip sfxPath_AudioClip { get; private set; }

    // 부모 클래스의 ID 반환 추상 메서드
    public override string GetID() => sfxID;
}
