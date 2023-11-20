using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelsManager : MonoBehaviour
{
    [SerializeField] private string[] additionalScenes;
    [SerializeField] private string menuScene;
    public float SceneTransitionDuration = 1;
    public bool IsMenuOpen = true;

    public event System.Action OnLoadBegin;
    public event System.Action<string> OnLoadComplete;

    public static LevelsManager Instance;
    public static string LevelPrefix = "Level";

    [SerializeField] private int currentLevel;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < additionalScenes.Length; i++)
        {
            SceneManager.LoadScene(additionalScenes[i], LoadSceneMode.Additive);
        }

        SceneManager.LoadScene(menuScene, LoadSceneMode.Additive);
        IsMenuOpen = true;
    }

    private IEnumerator UnloadSceneRoutine(string sceneName)
    {
        AsyncOperation op = SceneManager.UnloadSceneAsync(sceneName);
        while (!op.isDone)
        {
            yield return 0;
        }
    }

    private IEnumerator LoadSceneWithTransition(string sceneName, string sceneToUnload = null)
    {
        OnLoadBegin?.Invoke();
        yield return new WaitForSecondsRealtime(SceneTransitionDuration);

        if (sceneToUnload != null)
        {
            yield return StartCoroutine(UnloadSceneRoutine(sceneToUnload));
        }

        GameplayManager.Instance.Resume();
        GameplayManager.Instance.ClearRigidbodiesFromCache();

        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        OnLoadComplete?.Invoke(sceneName);
    }

    public void SwitchSceneToMenu()
    {
        StartCoroutine(LoadSceneWithTransition(menuScene, $"{LevelPrefix} {currentLevel}"));
        IsMenuOpen = true;
    }

    public void ContinueFromSavedLevel()
    {
        IsMenuOpen = false;
        currentLevel = FileWriter.ReadCurrentLevelFromFile();
        StartCoroutine(LoadSceneWithTransition($"{LevelPrefix} {currentLevel}", sceneToUnload: menuScene));
    }

    public void LoadNextLevel()
    {
        if (DoesLevelExist(currentLevel + 1))
        {
            FileWriter.WriteCurrentLevelToFile(currentLevel + 1);
            ChangeLevel(from: currentLevel, to: currentLevel + 1);
        }
        else
        {
            FileWriter.WriteCurrentLevelToFile(1);
            StartCoroutine(LoadSceneWithTransition(menuScene, sceneToUnload: $"{LevelPrefix} {currentLevel}"));
            currentLevel = 1;
        }
    }

    public void RestartLevel()
    {
        ChangeLevel(from: currentLevel, to: currentLevel);
    }

    private void ChangeLevel(int from, int to)
    {
        currentLevel = to;
        StartCoroutine(LoadSceneWithTransition(GetLevelName(to), sceneToUnload: GetLevelName(from)));
    }

    private string GetLevelName(int levelID)
    {
        return $"{LevelPrefix} {levelID}";
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public static bool DoesLevelExist(int levelID)
    {
        if (string.IsNullOrEmpty($"{LevelPrefix} {levelID}"))
            return false;

        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var scenePath = SceneUtility.GetScenePathByBuildIndex(i);
            var lastSlash = scenePath.LastIndexOf("/");
            var sceneName = scenePath.Substring(lastSlash + 1, scenePath.LastIndexOf(".") - lastSlash - 1);
            if (string.Compare($"{LevelPrefix} {levelID}", sceneName, true) == 0)
                return true;
        }
        return false;
    }
}
