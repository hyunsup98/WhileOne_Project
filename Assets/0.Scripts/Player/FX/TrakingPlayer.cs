using UnityEngine;

public class TrakingPlayer : MonoBehaviour
{
    [SerializeField] private Transform _standard;
    [SerializeField] private Transform _trans;
    private SpriteRenderer _rend;
    private void Awake()
    {
        _rend = GetComponent<SpriteRenderer>();
    }
    private void OnEnable()
    {
        if (_standard.localScale.x > 0)
        {
            gameObject.transform.position = _trans.position+new Vector3 (-0.2f,0,0);
            _rend.flipX = true;
        }
        else
        {
            gameObject.transform.position = _trans.position + new Vector3(0.2f, 0, 0);
            _rend.flipX = false;
        }

    }

    private void OnDisable()
    {
        gameObject.transform.position = Vector3.zero;
    }
}
