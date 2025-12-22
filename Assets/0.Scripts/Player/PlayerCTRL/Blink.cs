using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Blink : MonoBehaviour
{
    Player _player;
    SpriteRenderer[] allRender;

    float alpha = 1f;
    private void Awake()
    {
        _player = transform.parent.GetComponent<Player>();
        allRender = GetComponentsInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        StartCoroutine(AP());
    }


    IEnumerator AP()
    {
        alpha -= 2f * Time.deltaTime;
        foreach(var sr in allRender)
        {
            Color c = sr.color;
            c.a = alpha;
            sr.color = c;
        }
         yield return new WaitForSeconds(0.5f);
    }

}
