using UnityEngine;

public class AnimationScript : MonoBehaviour
{
    public void OnAnimationEnd()
    {
        gameObject.SetActive(false); //이펙트 끝나면 비활성화
    }
    
}
