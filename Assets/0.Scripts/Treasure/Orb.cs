using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class Orb : Treasure, IInteractable
{
    public Vector3 Pos => transform.position;

    [field: SerializeField] public float YOffset { get; set; } = 1.5f;
    [field: SerializeField] public string InteractText { get; set; } = "열기";

    [Header("공중에 떠있는 효과를 위한 변수")]
    [SerializeField] private float _floatingYOffset = 0.25f;
    [SerializeField] private float _floatingSpeed = 5f;
    private Vector3 prevPos;

    [SerializeField] private Transform _maskRenderer;       // 마스크 렌더러
    [SerializeField] private float _fadeTime = 5f;          // 페이드 효과 시간
    private Vector3 startScale;                             // 마스크의 시작 스케일
    private Coroutine _fadeCoroutine;

    protected override void Awake()
    {
        base.Awake();

        prevPos = transform.position;
        startScale = _maskRenderer.localScale;
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

        if (_fadeCoroutine != null) return;

        _fadeCoroutine = StartCoroutine(FadeIn());
    }

    // 페이드 효과
    private IEnumerator FadeIn()
    {
        float timer = 0f;

        while(timer <= _fadeTime)
        {
            _maskRenderer.localScale = Vector3.Lerp(startScale, Vector3.zero, timer / _fadeTime);
            timer += Time.deltaTime;
            yield return null;
        }

        SceneManager.LoadScene("Ending");
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

    private void OnDisable()
    {
        if (_fadeCoroutine != null)
            StopCoroutine(_fadeCoroutine);
    }
}
