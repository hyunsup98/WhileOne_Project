using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class TextBlink : MonoBehaviour
{
    TMP_Text _text;
    WaitForSeconds seconds1;
    private void Start()
    {
        _text = GetComponent<TMP_Text>();
        seconds1 = new WaitForSeconds(1f);
        StartCoroutine(HideText());
    }
    IEnumerator HideText()
    {

        while (true)
        {
            _text.alpha = 1f;
            yield return seconds1;
            _text.alpha = 0f;
            yield return seconds1;
        }

    }
    void OnDisable()
    {
        StopCoroutine(HideText());
    }
}
