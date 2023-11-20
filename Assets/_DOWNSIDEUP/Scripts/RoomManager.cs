using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Physics")]
    [SerializeField] private float waitTime = 3;
    [SerializeField] private bool freezePlayerAtStart = true;

    [Header("Start Animation")]
    [SerializeField] private float animTime = 1;
    [SerializeField] private Vector3 targetRot = new Vector3(0, 0, 0);
    [SerializeField] private Vector3 targetPos = new Vector3(0, 0, 0);
    [SerializeField] private MeshRenderer ceilingMesh;
    [SerializeField] private float targetCeilingOpacity = 0.54f;
    
    private Vector3 initialRot, initialPos;
    private float initialCeilingOpacity;
    private float animTimer = 0;
    private bool isRotActive, isCompleted = false;

    // Start is called before the first frame update
    private void Start()
    {
        initialRot = transform.rotation.eulerAngles;
        initialPos = transform.position;
        initialCeilingOpacity = ceilingMesh.material.color.a;

        GameplayManager.Instance.EnableInterpolateAllRigibody();
        GameplayManager.Instance.SetFreezeAllRigidbody(true, freezePlayerAtStart);
        
        LeanTween.delayedCall(waitTime, () =>
        {
            if (animTime == 0)
            {
                HandleCompletion();
                return;
            }

            GameplayManager.Instance.SetFreezeAllRigidbody(true);
            isRotActive = true;
        });
    }

    private void FixedUpdate()
    {
        if (!isRotActive) return;

        if (animTimer < animTime)
        {
            animTimer += Time.deltaTime;

            float t = animTimer / animTime;

            transform.rotation = Quaternion.Lerp(Quaternion.Euler(initialRot), Quaternion.Euler(targetRot), t);
            transform.position = Vector3.Lerp(initialPos, targetPos, t);

            Color col = ceilingMesh.material.color;
            col.a = Mathf.Lerp(initialCeilingOpacity, targetCeilingOpacity, t);
            ceilingMesh.material.color = col;
        }
        else if (!isCompleted)
        {
            HandleCompletion();
        }
    }

    private void HandleCompletion()
    {
        isCompleted = true;
        GameplayManager.Instance.SetFreezeAllRigidbody(false);
        GameplayManager.Instance.ClearRigidbodiesFromCache();
        GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>().MoveGroundCheckToHead();
    }
}
