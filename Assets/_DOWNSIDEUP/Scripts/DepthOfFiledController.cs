using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class DepthOfFiledController : MonoBehaviour
{
    Transform _camera;
    Rigidbody _target;
    [SerializeField] UnityEngine.Rendering.Volume _volume;
    UnityEngine.Rendering.Universal.DepthOfField _dofSetting;

    // Start is called before the first frame update
    void Start()
    {
        _camera = Camera.main.transform;
        _target = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody>();

        _volume.profile.TryGet<UnityEngine.Rendering.Universal.DepthOfField>(out _dofSetting);
    }

    // Update is called once per frame
    void Update()
    {
        _dofSetting.focusDistance.value = Vector3.Distance(_camera.position, _target.transform.position);
    }
}
