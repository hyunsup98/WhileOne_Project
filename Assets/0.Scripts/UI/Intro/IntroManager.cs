using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    [SerializeField] private Image _introImage;
    [SerializeField] private Sprite[] _introSprites;
    [SerializeField] private string _nextSceneName;     //æ¿ ¿Ã∏ß

    private PlayerInput _input;
    private InputAction _clickAction;

    private int currentNum = 0;

    private static bool _isIntroSeen = false;

    private void Awake()
    {
        //_input = GetComponent<PlayerInput>();

        //_clickAction = _input.actions.FindAction("Click");
        
    }
    void Start()
    {
        if (_isIntroSeen)
        {
            SceneManager.LoadScene(_nextSceneName);
            return;
        }
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
    private void EndSecen()
    {
        _isIntroSeen = true;
        _introImage.gameObject.SetActive(false);
        SceneManager.LoadScene(_nextSceneName);
    }
    private void OnDisable()
    {
        InputSystem.actions["Click"].started -= NextImage;
        //_clickAction.performed -= NextImage;
    }
}
