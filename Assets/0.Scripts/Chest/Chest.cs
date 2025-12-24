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

    #region 테스트용, 나중에 지울 변수들
    public Weapon[] weapons;
    #endregion

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

        if (weapon == null)
            SetWeapon();

        if (weapon != null)
        {
            // 아직 무기를 들고가지 않았으므로 OpenedLeft 상태
            chestState = ChestState.OpenedLeft;
            GameManager.Instance.CurrentDungeon.GetWeaponUI.EnableUI(weapon, this);
        }
        else
        {
            // 무기가 없으므로 OpenedTaken 상태
            chestState = ChestState.OpenedTaken;
            GameManager.Instance.CurrentDungeon.WeaponFailUI.SetActive(true);
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
                // 무기를 오브젝트 풀로 집어넣어줌
                break;
        }
    }

    /// <summary>
    /// 상자를 처음으로 열었을 때 무기를 랜덤으로 생성
    /// </summary>
    private void SetWeapon()
    {
        int percentSum = weapons.Length * 10;
        int rand = Random.Range(0, 100);

        if (rand < percentSum)
        {
            int weaponPercent = 0;

            foreach (var w in weapons)
            {
                weaponPercent += 10;
            }

            // 01 23 45 67 89  1011
            // 12 34 56 78 910 1112
            int weaponRand = Random.Range(0, weaponPercent);

            int index = 0;
            foreach (var w in weapons)
            {
                index += 10;

                if (index > weaponRand)
                {
                    weapon = w;
                    break;
                }
            }
        }
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