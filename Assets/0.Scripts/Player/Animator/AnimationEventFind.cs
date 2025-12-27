using UnityEngine;

public class AnimationEventFind : MonoBehaviour
{
    private WeaponChange _weaponChange;
    void Awake()
    {
        _weaponChange = GetComponentInParent<WeaponChange>();
    }

    // Update is called once per frame
    public void FindEvent()
    {
        if (_weaponChange != null)
        {
            _weaponChange.ResetAttack();
        }
    }
}
