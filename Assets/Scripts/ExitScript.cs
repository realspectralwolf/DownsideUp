using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitScript : MonoBehaviour
{
    [SerializeField] AudioClip audioLevelCompleted;
    bool isGame = false;
    public static event System.Action PlayerExited;

    private void OnEnable()
    {
        RestartMgr.gameStarted += GameStarted;
    }

    private void OnDisable()
    {
        RestartMgr.gameStarted -= GameStarted;
    }

    void GameStarted()
    {
        isGame = true;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && isGame)
        {
            Debug.Log("exited");

            AudioSource.PlayClipAtPoint(audioLevelCompleted, transform.position);
            PlayerExited?.Invoke();

            string name = SceneManager.GetActiveScene().name;
            string newName = "";
            
            switch (name)
            {
                case "Level 1":
                    newName = "Level 2";
                    break;
                case "Level 2":
                    newName = "Level 3";
                    break;
                case "Level 3":
                    newName = "Level 4";
                    break;
                case "Level 4":
                    newName = "Level 5";
                    break;
                case "Level 5":
                    newName = "End";
                    break;
            }

            StartCoroutine(LoadSceneAfter(0.75F, newName));
        }
    }

    IEnumerator LoadSceneAfter(float t, string name)
    {
        yield return new WaitForSeconds(t);
        SceneManager.LoadScene(name);
    }
}
