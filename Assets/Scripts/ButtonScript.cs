using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(EventTrigger))]
public class ButtonScript : MonoBehaviour
{
    public static event System.Action hovered;
    public static event System.Action clicked;

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
        hovered?.Invoke();
    }

    void OnClicked(PointerEventData data)
    {
        clicked?.Invoke();
    }
}
