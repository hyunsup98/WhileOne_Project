using UnityEngine;

public static class ActionFactory
{
    // 몬스터 액션 매칭시, 호출
    public static MonsterPattern Create(MonsterActionSO actionData, MonsterPresenter monster)
    {
        switch (actionData)
        {
            case Pattern01SO pattern01:
                return new MonsterPattern01(pattern01, monster);

            case Pattern04SO pattern04:
                return new MonsterPattern04(pattern04, monster);

            case Pattern05SO pattern05:
                return new MonsterPattern05(pattern05, monster);

            default:
                Debug.LogError("할당되지 않은 행동을 생성하려 합니다.");
                return null;
        }
    }

}
