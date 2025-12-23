using UnityEngine;

public static class ActionFactory
{
    public static IAction Create(MonsterActionSO actionData, MonsterPresenter monster)
    {
        switch (actionData)
        {
            case Pattern01SO pattern01:
                return new MonsterPattern01(pattern01, monster);

            default:
                Debug.LogError("할당되지 않은 행동을 생성하려 합니다.");
                return null;
        }
    }

}
