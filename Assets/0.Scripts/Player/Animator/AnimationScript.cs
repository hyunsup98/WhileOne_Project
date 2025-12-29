using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    public void OnAnimationStart()
    {

    }
    public void OnAnimationEnd()
    {
         //이펙트 끝나면 비활성화
        gameObject.SetActive(false);
    }
    
}
