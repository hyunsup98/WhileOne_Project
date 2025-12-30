using TMPro;
using UnityEngine;

public enum ChestState
{
    Closed,             // ���ڸ� �� ���� ���� �ʴ� ����
    OpenedLeft,         // ���ڸ� �������� ����� �������� ���� ����
    OpenedTaken         // ���ڸ� ���� ���⸦ ������ ����
}

public class Chest : MonoBehaviour, IInteractable
{
    [Header("������Ʈ")]
    [SerializeField] private SpriteRenderer _renderer;
    [SerializeField] private Animator _animator;

    [Header("���Ⱑ ����Ǵ� �ۼ�Ʈ")]
    [Range(0, 100)]
    [SerializeField] private int dropWeaponPer;

    protected Weapon Weapon { get; private set; }

    private ChestState chestState;

    [field: SerializeField] public float YOffset { get; set; } = 1.5f;      //�������̽� �ʵ� �� ��ȣ�ۿ� Ű �̹��� ��ġ�� ���� ������

    public Vector3 Pos => transform.position;           // �������̽� �ʵ� �� ��ȣ�ۿ� �̹��� ��ġ ������ ���� ������Ʈ ��ġ ��ǥ

    [field: SerializeField] public string InteractText { get; set; } = "열기";

    private void Awake()
    {
        // ������Ʈ �ʱ�ȭ
        if (_renderer == null)
            TryGetComponent(out _renderer);

        if (_animator == null)
            TryGetComponent(out _animator);

        chestState = ChestState.Closed;
    }

    // ��ȣ �ۿ� �޼���
    public virtual void OnInteract()
    {
        SoundManager.Instance.PlaySoundEffect("WeaponBox_On_FX_001");
        GameManager.Instance.InteractObj = null;
        _animator.SetBool("isOpen", true);

        if(chestState == ChestState.Closed)
        {
            // ���� ���� ��ȯ
            SetWeapon();
        }

        // ���Ⱑ �������� ��ȯ������, �̹� ���ڿ� ���Ⱑ ���� ��
        if (Weapon != null)
        {
            // ���� ���⸦ ������� �ʾ����Ƿ� OpenedLeft ����
            chestState = ChestState.OpenedLeft;
            GameManager.Instance.CurrentDungeon.WeaponUI.EnableGainUI(Weapon, this);
        }
        // ���� ���� ���Ⱑ �Ȼ������� or 
        else
        {
            // ���Ⱑ �����Ƿ� OpenedTaken ����
            chestState = ChestState.OpenedTaken;
            GameManager.Instance.CurrentDungeon.WeaponUI.EnableFailUI();
        }
    }

    // �÷��̾ ���ڸ� Ȯ���� �� ���� �� �ൿ
    // ���⸦ �������� �ݰų�, �������� �ʰ� ���� �� ����
    public void ChestClose(ChestState state)
    {
        chestState = state;

        switch (state)
        {
            case ChestState.OpenedLeft:
                _animator.SetBool("isOpen", false);
                break;

            case ChestState.OpenedTaken:
                Weapon = null;
                break;
        }
    }

    /// <summary>
    /// ���ڸ� ó������ ������ �� ���⸦ �������� ����
    /// </summary>
    private void SetWeapon()
    {
        int rand = Random.Range(0, 100);

        if (rand < dropWeaponPer)
            Weapon = DataManager.Instance.WeaponData.GetRandomWeapon();
        else
            Weapon = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // ���ڿ��� �̹� ���⸦ ������ �ڸ� ��ȣ�ۿ� �Ұ���
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