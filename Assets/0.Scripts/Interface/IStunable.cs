

public interface IStunable
{
    bool IsStun { get; }

    void OnStun();

    void SetStun(bool isStun);
}
