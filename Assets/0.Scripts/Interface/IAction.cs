
public interface IAction
{

    // 몬스터 행동을 수행상태 결정
    bool IsAction { get; }

    // 몬스터 행동 시작시, 1번 호출
    void StartAction();

    // 몬스터 행동 진행 중
    void OnAction();

    // 몬스터 행동 종료시, 1번 호출
    void EndAction();
}
