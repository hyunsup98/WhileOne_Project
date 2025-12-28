using UnityEngine;

public sealed class Orb : Treasure, IInteractable
{
    public Vector3 Pos => transform.position;

    [field: SerializeField] public float YOffset { get; set; } = 1.5f;

    [SerializeField] private float _floatingYOffset = 0.25f;
    [SerializeField] private float _floatingSpeed = 5f;
    private Vector3 prevPos;

    protected override void Awake()
    {
        base.Awake();

        prevPos = transform.position;
    }

    private void Update()
    {
        float yPos = Mathf.Sin(Time.time * _floatingSpeed) * _floatingYOffset;
        transform.position = prevPos + new Vector3(0f, yPos, 0f);

    }

    public void OnInteract()
    {
        // todo
        // 페이드 아웃 후 엔딩씬으로 이동
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if(other.CompareTag("Player"))
        {
            GameManager.Instance.InteractObj = this;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            GameManager.Instance.InteractObj = null;
        }
    }
}
