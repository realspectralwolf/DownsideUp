using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [SerializeField] Canvas gameplayUI;
    [SerializeField] TextMeshProUGUI levelNumber;
    [SerializeField] Canvas pauseMenuUI;
    [SerializeField] Image transitionImage;

    private void OnEnable()
    {
        GameplayManager.Instance.OnGameResume += ClosePauseMenu;
        GameplayManager.Instance.OnGamePause += OpenPauseMenu;
        LevelsManager.Instance.OnLoadBegin += StartTransition;
        LevelsManager.Instance.OnLoadComplete += EndTransition;
    }

    private void OnDisable()
    {
        GameplayManager.Instance.OnGameResume -= ClosePauseMenu;
        GameplayManager.Instance.OnGamePause -= OpenPauseMenu;
        LevelsManager.Instance.OnLoadBegin -= StartTransition;
        LevelsManager.Instance.OnLoadComplete -= EndTransition;
    }

    private void OpenPauseMenu()
    {
        Cursor.lockState = CursorLockMode.None;
        pauseMenuUI.gameObject.SetActive(true);
    }

    private void ClosePauseMenu()
    {
        Cursor.lockState = CursorLockMode.Locked;
        pauseMenuUI.gameObject.SetActive(false);
    }

    private void StartTransition()
    {
        transitionImage.gameObject.SetActive(true);
        float animTime = LevelsManager.Instance.SceneTransitionDuration;
        LeanTween.alpha(transitionImage.rectTransform, 1f, animTime).setIgnoreTimeScale(true);
    }

    private void EndTransition(string newSceneName)
    {
        if (newSceneName.Split(" ")[0] == LevelsManager.LevelPrefix)
        {
            int num = int.Parse(newSceneName.Split(" ")[1]);
            levelNumber.text = $"#{num}";
            gameplayUI.gameObject.SetActive(true);
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            gameplayUI.gameObject.SetActive(false);
            pauseMenuUI.gameObject.SetActive(false);
            Cursor.lockState = CursorLockMode.None;
        }

        float animTime = LevelsManager.Instance.SceneTransitionDuration;
        LeanTween.alpha(transitionImage.rectTransform, 0f, animTime)
            .setOnComplete(() => { 
                transitionImage.gameObject.SetActive(false);
            }).setIgnoreTimeScale(true);
    }
}
