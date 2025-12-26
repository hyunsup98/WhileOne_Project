using UnityEngine;

[CreateAssetMenu(fileName = "UIStringDataSO", menuName = "Scriptable Objects/Data/UIStringDataSO")]
public class UIStringDataSO : TableBase<string>
{
    // 스트링자료이름
    [field: SerializeField] public string stringResourceID { get; private set; }

    // 국문
    [field: SerializeField] public string ko { get; private set; }

    // 부모 클래스의 ID 반환 추상 메서드
    public override string GetID() => stringResourceID;
}
