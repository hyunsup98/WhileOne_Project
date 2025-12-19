using UnityEngine;

public class MonsterView : MonoBehaviour
{
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void OnHurtAni() => _animator.SetTrigger("Hurt");

    public void OnDeathAni() => _animator.SetBool("Death", true);
    public void OnAttackAni() => _animator.SetTrigger("Attack");

    public void OnIdleAni() => _animator.SetBool("Idle", true);

    public void OnDisIdleAni() => _animator.SetBool("Idle", false);
}
