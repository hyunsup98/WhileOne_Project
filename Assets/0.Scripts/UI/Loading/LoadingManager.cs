using System.Collections;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    [SerializeField] private string nextScene;
    [SerializeField] private Image _loadingBar;
    [SerializeField] private TMP_Text _loadingText;

    private float _loadingTime = 3f; //최소 시간동안은 로딩 안됨


    private void Start()
    {
        _loadingBar.fillAmount = 0;
        StartCoroutine(LoadingSecen());
    }

    IEnumerator LoadingSecen()
    {
        AsyncOperation oper = SceneManager.LoadSceneAsync(nextScene);
        oper.allowSceneActivation = false;
        
        //페이크 로딩을 위한 현재 시간 계산
        float currentTime = 0f;

        while(!oper.isDone)
        {
            yield return null;
            currentTime += Time.deltaTime;
            Debug.Log(oper.progress*100);
            if (oper.progress < 0.9f) //로딩하기 전에 해야할 것들
            {
                //근데 여기서 뭐해야함?
            }
            float progressTime = Mathf.Min(oper.progress / 90, currentTime / _loadingTime); //로딩의 최소치

            _loadingBar.fillAmount = progressTime;
            _loadingText.text = $"{_loadingBar.fillAmount * 100f}%";
            if(oper.progress>=0.9f && currentTime >= _loadingTime) //페이크 로딩시간 끝나고 실제 로딩도 끝났을 때
            {
                _loadingBar.fillAmount = 1f;
                _loadingText.text = $"100%";
            }
            yield return new WaitForSeconds(0.5f);
            oper.allowSceneActivation = true;

            yield break;
        }
    }
}
