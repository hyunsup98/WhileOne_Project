using UnityEngine;

[CreateAssetMenu(fileName = "BGMDataSO", menuName = "Scriptable Objects/Data/BGMDataSO")]
public class BGMDataSO : TableBase<string>
{
    // BGM ID
    [field: SerializeField] public string bgmID { get; private set; }

    // BGM사용처
    [field: SerializeField] public string bgmUse { get; private set; }

    // BGM경로
    [field: SerializeField] public AudioClip bgmPath_AudioClip { get; private set; }

    // 부모 클래스의 ID 반환 추상 메서드
    public override string GetID() => bgmID;
}
