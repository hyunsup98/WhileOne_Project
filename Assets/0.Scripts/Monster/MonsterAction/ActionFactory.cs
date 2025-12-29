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

            case Pattern02SO pattern02:
                return new MonsterPattern02(pattern02, monster);

            case Pattern03SO pattern03:
                return new MonsterPattern03(pattern03, monster);

            case Pattern04SO pattern04:
                return new MonsterPattern04(pattern04, monster);

            case Pattern05SO pattern05:
                return new MonsterPattern05(pattern05, monster);

            case Pattern06SO pattern06:
                return new MonsterPattern06(pattern06, monster);

            default:
                Debug.LogError(actionData.Name + "할당되지 않은 행동을 생성하려 합니다.");
                return null;
        }
    }

}
