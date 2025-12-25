using UnityEngine;

public class Weapon : MonoBehaviour
{
    [SerializeField] private int _keyId;                     // 무기 데이터베이스 SO에서 데이터를 가져오기 위해 필요한 ID
    [SerializeField] private SpriteRenderer _renderer;
    public WeaponDataSO WeaponData { get; set; }    // 실제 무기 데이터

    private void Awake()
    {
        if (_renderer == null)
            TryGetComponent(out _renderer);
    }

    private void Start()
    {
        if(_keyId != default)
        {
            // DataManager의 WeaponDatabase에 인덱서를 통해 접근해 값을 가져옴
            InitData(DataManager.Instance.WeaponData.WeaponDatabase[_keyId]);
        }
    }

    /// <summary>
    /// 무기 데이터 수치 세팅
    /// </summary>
    /// <param name="data"> 적용할 무기 데이터 </param>
    public void InitData(WeaponDataSO data)
    {
        if (data == null) return;

        _keyId = data.weaponID;
        WeaponData = data;
        _renderer.sprite = WeaponData.weaponResourcePath_Sprite;
    }
}
