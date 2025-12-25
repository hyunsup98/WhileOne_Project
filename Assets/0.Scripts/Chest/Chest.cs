using UnityEngine;

public enum ChestState
{
    Closed,             // 상자를 한 번도 열지 않는 상태
    OpenedLeft,         // 상자를 열었지만 무기는 가져가지 않은 상태
    OpenedTaken         // 상자를 열고 무기를 가져간 상태
}

public class Chest : MonoBehaviour, IInteractable
{
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Animator _animator;

    private Weapon weapon;

    private ChestState chestState;

    [field: SerializeField] public float YOffset { get; set; } = 1.5f;      //인터페이스 필드 → 상호작용 이미지 위치에 더할 오프셋

    public Vector3 Pos => transform.position;           // 인터페이스 필드 → 상호작용 이미지 위치 세팅을 위한 오브젝트 위치 좌표

    private void Awake()
    {
        // 컴포넌트 초기화
        if (_renderer == null)
            TryGetComponent(out _renderer);

        if (_animator == null)
            TryGetComponent(out _animator);

        chestState = ChestState.Closed;
    }

    // 상호 작용 메서드
    public void OnInteract()
    {
        GameManager.Instance.InteractObj = null;
        _animator.SetBool("isOpen", true);

        if(chestState == ChestState.Closed)
        {
            // 무기 랜덤 소환
            SetWeapon();
        }

        // 무기가 랜덤으로 소환됐으면, 이미 상자에 무기가 있을 때
        if (weapon != null)
        {
            // 아직 무기를 들고가지 않았으므로 OpenedLeft 상태
            chestState = ChestState.OpenedLeft;
            GameManager.Instance.CurrentDungeon.WeaponUI.EnableGainUI(weapon, this);
        }
        // 꽝이 떠서 무기가 안뽑혔으면 or 
        else
        {
            // 무기가 없으므로 OpenedTaken 상태
            chestState = ChestState.OpenedTaken;
            GameManager.Instance.CurrentDungeon.WeaponUI.EnableFailUI();
        }
    }

    // 플레이어가 상자를 확인한 뒤 닫을 때 행동
    // 무기를 가져가고 닫거나, 가져가지 않고 닫을 수 있음
    public void ChestClose(ChestState state)
    {
        chestState = state;

        switch (state)
        {
            case ChestState.OpenedLeft:
                _animator.SetBool("isOpen", false);
                break;

            case ChestState.OpenedTaken:
                weapon = null;
                break;
        }
    }

    /// <summary>
    /// 상자를 처음으로 열었을 때 무기를 랜덤으로 생성
    /// </summary>
    private void SetWeapon()
    {
        int rand = Random.Range(0, 100);

        if (rand < 10)
            weapon = null;
        else
            weapon = DataManager.Instance.WeaponData.GetRandomWeapon();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 상자에서 이미 무기를 가져간 뒤면 상호작용 불가능
        if (collision.CompareTag("Player") && chestState != ChestState.OpenedTaken)
        {
            GameManager.Instance.InteractObj = this;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GameManager.Instance.InteractObj = null;
        }
    }
}