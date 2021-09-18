using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class MenuCharacterCollider : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private UnityEvent onClickEvent;

    public void OnPointerClick(PointerEventData eventData)
    {
        onClickEvent.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData){}

    public void OnPointerUp(PointerEventData eventData)
    {
        //TODO: Is there a way to modify the UI element "collider"? We could use alternative methods to achieve this 
        onClickEvent.Invoke();
    }
}
