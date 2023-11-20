using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] float _timeBetweenImpacts;
    [SerializeField] AudioClip _audioImpact;
    [SerializeField] AudioAttribute[] audioList;

    [Header("Footsteps")]
    [SerializeField] AudioSource footstepSource;
    [SerializeField] float footstepStopDelay = 0.25f;
    [SerializeField] public float MinImpactVelocity = 2;

    float _timer = 0;
    bool _canPlayImpact = true;

    [HideInInspector] public bool PlayFootsteps = false;
    [HideInInspector] public static AudioManager Instance { get; private set; }

    void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
        
        if (Instance != null && Instance != this) 
        { 
            Destroy(gameObject); 
        } 
        else 
        { 
            Instance = this; 
        } 
    }

    void Update()
    {
        if (_timer < _timeBetweenImpacts && !_canPlayImpact)
        {
            _timer += Time.deltaTime;
        }
        else
        {
            _canPlayImpact = true;
        }

        if (PlayFootsteps)
        {
            footstepSource.enabled = true;
            _footstepsTimer = 0;
        }
        else if (_footstepsTimer < footstepStopDelay)
        {
            _footstepsTimer += Time.deltaTime;
            footstepSource.enabled = false;
        }
    }

    public void PlayImpact(Vector3 pos)
    {
        if (_canPlayImpact)
            AudioSource.PlayClipAtPoint(_audioImpact, pos);
            _canPlayImpact = false;
            _timer = 0;
    }

    public void PlaySound(Sound soundToPlay)
    {
        for (int i = 0; i < audioList.Length; i++)
        {
            if (soundToPlay != audioList[i].audioID) continue;

            AudioSource.PlayClipAtPoint(audioList[i].audioClip, Camera.main.transform.position);
            return;
        }
    }

    float _footstepsTimer = 0;
}

public enum Sound
{
    levelCompleted,
    itemGrabbed,
    itemDropped,
    uiClick,
    uiHover,
    jump
}

[System.Serializable]
struct AudioAttribute
{
    public Sound audioID;
    public AudioClip audioClip;
}