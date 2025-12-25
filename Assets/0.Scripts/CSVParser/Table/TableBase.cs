using UnityEngine;

/// <summary>
/// 데이터 SO가 상속받을 부모 클래스
/// </summary>
/// <typeparam name="TKey"> 아이디 컬럼 타입 </typeparam>
public abstract class TableBase<TKey> : ScriptableObject
{
    // 자식이 자신의 ID 변수를 반환하도록 강제
    public abstract TKey GetID();
}
