using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private Image _introImage;
    [SerializeField] private Sprite[] _introSprites;
    [SerializeField] private string _nextSceneName;     //씬 이름
    [SerializeField] private UI_IntroSkip _introSkip;

    [Header("이미지를 다 넘긴 뒤 바로 다음 씬으로 넘어갈건지")]
    [SerializeField] private bool isSkip;       // 이미지를 다 넘기면 바로 다음 씬으로 넘어갈건지

    private PlayerInput _input;
    private InputAction _clickAction;

    private int currentNum = 0;

    private void Awake()
    {
        //_input = GetComponent<PlayerInput>();

        //_clickAction = _input.actions.FindAction("Click");
        
    }
    void Start()
    {
        InputSystem.actions.FindActionMap("Player").Disable();
        if (_introSprites.Length > 0)
        {
            _introImage.sprite = _introSprites[currentNum];
        }
    }
    private void OnEnable()
    {
        InputSystem.actions["Click"].started += NextImage;

        //_clickAction.performed += NextImage;
    }

    private void NextImage(InputAction.CallbackContext ctx)
    {
        currentNum++;
        if(currentNum < _introSprites.Length)
        {
            _introImage.sprite = _introSprites[currentNum];
        }
        else
        {
            EndSecen();
        }
    }
    public void EndSecen()
    {
        InputSystem.actions.FindActionMap("Player").Enable();
        _introImage.gameObject.SetActive(false);

        if(_introSkip != null)
            _introSkip.gameObject.SetActive(false);

        if (isSkip)
            SceneManager.LoadScene(_nextSceneName);
    }
    private void OnDisable()
    {
        InputSystem.actions["Click"].started -= NextImage;
        //_clickAction.performed -= NextImage;
    }
}
