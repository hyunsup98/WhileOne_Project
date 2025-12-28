using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialMap : MonoBehaviour
{
    [SerializeField] private string _sceneName = "Dungeon_Floor1";
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            LoadingManager.nextSceneName = _sceneName;

            SceneManager.LoadScene("Loading");
        }
    }
}
