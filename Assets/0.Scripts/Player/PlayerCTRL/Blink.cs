using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Blink : MonoBehaviour
{
    Player _player;
    SpriteRenderer[] allRender;

    float alpha = 100f;
    private void Awake()
    {
        _player = transform.parent.GetComponent<Player>();
        
        Debug.Log(allRender);
    }

    private void OnEnable()
    {
        alpha = 1f;
        StartCoroutine(AP());
    }
    void Update()
    {
        alpha -= 0.2f * Time.deltaTime;
    }

    IEnumerator AP()
    {
       
        yield return new WaitForSeconds(0.2f);
        foreach(var sr in allRender)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
    }

}
