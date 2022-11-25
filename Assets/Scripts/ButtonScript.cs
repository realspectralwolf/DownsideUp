using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof( EventTrigger ))]
public class ButtonScript : MonoBehaviour
{
    public static event System.Action hovered;
    public static event System.Action clicked;
    void Start()
    {
        EventTrigger eventTrigger = GetComponent<EventTrigger>( );
        EventTrigger.Entry entry = new EventTrigger.Entry( );
        entry.eventID = EventTriggerType.PointerEnter;
        entry.callback.AddListener( ( data ) => { OnHovered( (PointerEventData)data ); } );
        eventTrigger.triggers.Add( entry );

        EventTrigger.Entry entry2 = new EventTrigger.Entry( );
        entry2.eventID = EventTriggerType.PointerDown;
        entry2.callback.AddListener( ( data ) => { OnClicked( (PointerEventData)data ); } );
        eventTrigger.triggers.Add( entry2 );
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
