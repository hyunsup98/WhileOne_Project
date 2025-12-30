using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingManager : MonoBehaviour
{
    static public string nextSceneName = ""; 
    static public int nextSceneIndex = -1; 
    [SerializeField] private Image _loadingBar;
    [SerializeField] private TMP_Text _loadingText;

    private float _loadingTime = 2f; //최소 시간동안은 로딩 안됨


    private void Start()
    {
        _loadingBar.fillAmount = 0;
        Debug.Log(nextSceneName);
        if(nextSceneIndex != -1)
        {
            StartCoroutine(LoadingSecen(nextSceneIndex));
        }
        else if (!string.IsNullOrEmpty(nextSceneName))
        {
            StartCoroutine(LoadingSecen(nextSceneName));
        }
        else
        {
            Debug.LogError("다음 씬이 정해지지 않았습니다.");
        }
    }

    //인덱스와 이름을 둘 다 사용할수 있게 코루틴 오버로딩

    //이름
    IEnumerator LoadingSecen(string SceneName)
    {
        AsyncOperation oper = SceneManager.LoadSceneAsync(SceneName);
        oper.allowSceneActivation = false;
        
        //페이크 로딩을 위한 현재 시간 계산
        float currentTime = 0f;

        while(!oper.isDone)
        {
            yield return null;
            currentTime += Time.deltaTime;
            // Debug.Log(oper.progress*100);
            if (oper.progress < 0.9f) //로딩하기 전에 해야할 것들
            {
                //근데 여기서 뭐해야함?
            }
            float progressTime = Mathf.Min(oper.progress / 0.9f, currentTime / _loadingTime); //로딩의 최소치

            _loadingBar.fillAmount = progressTime;
            _loadingText.text = $"{(_loadingBar.fillAmount * 100f):F0}%";
            if(oper.progress>=0.9f && currentTime >= _loadingTime) //페이크 로딩시간 끝나고 실제 로딩도 끝났을 때
            {
                _loadingBar.fillAmount = 1f;
                _loadingText.text = $"100%";
                yield return new WaitForSeconds(0.5f);
                
                //혹시 모를 코드 초기화
                nextSceneIndex = -1; 
                nextSceneName = "";

                oper.allowSceneActivation = true;
                yield break;
            }
        }
    }
    //인덱스
    IEnumerator LoadingSecen(int SceneIndex)
    {
        AsyncOperation oper = SceneManager.LoadSceneAsync(SceneIndex);
        oper.allowSceneActivation = false;
        
        //페이크 로딩을 위한 현재 시간 계산
        float currentTime = 0f;

        while(!oper.isDone)
        {
            yield return null;
            currentTime += Time.deltaTime;
            // Debug.Log(oper.progress*100);
            if (oper.progress < 0.9f) //로딩하기 전에 해야할 것들
            {
                //근데 여기서 뭐해야함?
            }
            float progressTime = Mathf.Min(oper.progress / 0.9f, currentTime / _loadingTime); //로딩의 최소치

            _loadingBar.fillAmount = progressTime;
            _loadingText.text = $"{(_loadingBar.fillAmount * 100f):F0}%";
            if(oper.progress>=0.9f && currentTime >= _loadingTime) //페이크 로딩시간 끝나고 실제 로딩도 끝났을 때
            {
                _loadingBar.fillAmount = 1f;
                _loadingText.text = $"100%";
                yield return new WaitForSeconds(0.5f);
                oper.allowSceneActivation = true;

                yield break;
            }
        }
    }
}
