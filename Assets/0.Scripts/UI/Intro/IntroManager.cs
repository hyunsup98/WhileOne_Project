using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class IntroManager : MonoBehaviour
{
    private Image _introImage;
    private Sprite[] _introSprites;
    private string nextSceneName; //æ¿ ¿Ã∏ß

    private PlayerInput _input;
    private InputAction _clickAction;

    private int currentNum = 0;

    private static bool _isIntroSeen = false;

    private void Awake()
    {
        _input = GetComponent<PlayerInput>();

        _clickAction = _input.actions.FindAction("Click");
        
    }
    void Start()
    {

        if (_isIntroSeen)
        {
            SceneManager.LoadScene(nextSceneName);
            return;
        }
        if (_introSprites.Length > 0)
        {
            _introImage.sprite = _introSprites[currentNum];
        }
    }
    private void OnEnable()
    {

        _clickAction.performed += NextImage;
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

        SceneManager.LoadScene(nextSceneName);
    }
    private void OnDisable()
    {

        _clickAction.performed -= NextImage;
    }


}
