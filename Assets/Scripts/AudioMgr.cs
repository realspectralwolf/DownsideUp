using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioMgr : MonoBehaviour
{
    [SerializeField] float _timeBetweenImpacts;
    [SerializeField] AudioClip _audioImpact;
    [SerializeField] AudioClip _audioHoverUI;
    [SerializeField] AudioSource _audioSourceClickUI;
    float _timer = 0;
    bool _canPlayImpact = true;

    public static AudioMgr instance {get; private set;}

    void Awake() 
    { 
        // If there is an instance, and it's not me, delete myself.
        
        if (instance != null && instance != this) 
        { 
            Destroy(gameObject); 
        } 
        else 
        { 
            instance = this; 
        } 

        DontDestroyOnLoad(this.gameObject);
    }

    void OnEnable()
    {
        DraggableObject.Touched += PlayImpact;
        ButtonScript.hovered += PlayHoverUI;
        ButtonScript.clicked += PlayClickUI;
    }

    void OnDisable()
    {
        DraggableObject.Touched -= PlayImpact;
        ButtonScript.hovered -= PlayHoverUI;
        ButtonScript.clicked -= PlayClickUI;
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
    }

    void PlayImpact(Vector3 pos)
    {
        if (_canPlayImpact)
            AudioSource.PlayClipAtPoint(_audioImpact, pos);
            _canPlayImpact = false;
            _timer = 0;
    }

    void PlayHoverUI()
    {
        AudioSource.PlayClipAtPoint(_audioHoverUI, Vector3.zero);
    }

    void PlayClickUI()
    {
        _audioSourceClickUI.Play(0);
    }
}
