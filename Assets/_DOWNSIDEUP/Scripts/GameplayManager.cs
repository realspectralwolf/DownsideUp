using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameplayManager : MonoBehaviour
{
    public event System.Action OnGameResume;
    public event System.Action OnGamePause;

    public static GameplayManager Instance;

    bool isPaused = false;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1;
    }

    private Rigidbody[] cachedRbs;

    private Rigidbody[] GetAllRigidbodies()
    {
        if (cachedRbs == null)
        {
            cachedRbs = FindObjectsOfType<Rigidbody>();
        }
        return cachedRbs;
    }

    public void ClearRigidbodiesFromCache()
    {
        cachedRbs = null;
    }

    public void EnableInterpolateAllRigibody()
    {
        Rigidbody[] rbs = GetAllRigidbodies();
        for (int i = 0; i < rbs.Length; i++)
        {
            rbs[i].interpolation = RigidbodyInterpolation.Interpolate;
        }
    }

    public void SetFreezeAllRigidbody(bool kinematicState, bool affectPlayer = true)
    {
        Rigidbody[] rbs = GetAllRigidbodies();
        for (int i = 0; i < rbs.Length; i++)
        {
            if (rbs[i].CompareTag("Player"))
            {
                if (affectPlayer)
                    rbs[i].isKinematic = kinematicState;

                continue;
            }

            rbs[i].isKinematic = kinematicState;
        }
    }

    public void Pause()
    {
        if (!isPaused && !LevelsManager.Instance.IsMenuOpen)
        {
            Time.timeScale = 0;
            isPaused = true;
            OnGamePause?.Invoke();
        }
    }

    public void Resume()
    {
        if (isPaused)
        {
            Time.timeScale = 1;
            isPaused = false;
            OnGameResume?.Invoke();
        }
    }
}
