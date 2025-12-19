
public interface IAttack
{
    bool IsAttack { get; }
    void StartAttack();

    void OnAttack();

    void EndAttack();
}
