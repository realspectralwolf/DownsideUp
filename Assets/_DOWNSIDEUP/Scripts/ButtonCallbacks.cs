using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class ButtonCallbacks : MonoBehaviour
{
    public void ClickedPlay()
    {
        LevelsManager.Instance.ContinueFromSavedLevel();
    }

    public void ClickedMenu()
    {
        LevelsManager.Instance.SwitchSceneToMenu();
    }

    public void ClickedQuit()
    {
        LevelsManager.Instance.QuitGame();
    }

    public void ClickResume()
    {
        GameplayManager.Instance.Resume();
    }

    #region Audio

    void Start()
    {
        AddUIEventCallback(EventTriggerType.PointerEnter, OnHovered);
        AddUIEventCallback(EventTriggerType.PointerDown, OnClicked);
    }

    public delegate void MyMethodDelegate(PointerEventData data);

    void AddUIEventCallback(EventTriggerType type, MyMethodDelegate methodCallback)
    {
        EventTrigger eventTrigger = GetComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener((data) => { methodCallback((PointerEventData)data); });
        eventTrigger.triggers.Add(entry);
    }

    void OnHovered(PointerEventData data)
    {
        AudioManager.Instance.PlaySound(Sound.uiHover);
    }

    void OnClicked(PointerEventData data)
    {
        AudioManager.Instance.PlaySound(Sound.uiClick);
    }

    #endregion
}
