using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TransitionMgr : MonoBehaviour
{
    [SerializeField] Animator _animator;
    public static System.Action<string> LoadScene;

    void OnEnable()
    {
        ExitScript.PlayerExited += OnExitLevel;
        LoadScene += LoadSceneWithTransition;
    }

    void OnDisable()
    {
        ExitScript.PlayerExited -= OnExitLevel;
        LoadScene -= LoadSceneWithTransition;
    }

    void OnExitLevel() 
    {
        _animator.Play("UI_CloseLevel", 0, 0);
    }

    public void StartGame()
    {
        string level = PlayerPrefs.GetString("savedLevel", "Level 1");
        LoadSceneWithTransition(level);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    void LoadSceneWithTransition(string level)
    {
        _animator.Play("UI_CloseLevel", 0, 0);
        StartCoroutine(LoadSceneAfterAnim(level));
    }

    IEnumerator LoadSceneAfterAnim(string name)
    {
        yield return new WaitForSeconds(_animator.GetCurrentAnimatorClipInfo(0)[0].clip.length);
        SceneManager.LoadScene(name);
    }
}
