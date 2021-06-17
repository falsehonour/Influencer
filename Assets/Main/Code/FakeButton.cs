using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class FakeButton : MonoBehaviour, IPointerDownHandler
{
    [SerializeField] private Image image;
    [SerializeField] private GameObject pressedOverlayGraphics;
    private MaskableGraphic[] maskableGraphics;

    [SerializeField] private float disabledAlpha;
    [SerializeField] private UnityEvent onClickEvent;
    /*[SerializeField] private Sprite unpressedSprite;
    [SerializeField] private Sprite pressedSprite;*/

    private void Start()
    {
        maskableGraphics = GetComponentsInChildren<MaskableGraphic>();
        Unpress();
    }

    [ContextMenu(nameof(Press))]
    public virtual void Press()
    {
        pressedOverlayGraphics.SetActive(true);
       // image.sprite = pressedSprite;
        Invoke("Unpress", 0.5f);

    }

    protected virtual void Unpress()
    {
        pressedOverlayGraphics.SetActive(false);
       // image.sprite = unpressedSprite;
    }

    [ContextMenu(nameof(Enable))]
    public void Enable()
    {
        ManipulateImagesAlpha(1);
    }

    [ContextMenu(nameof(Disable))]
    public void Disable()
    {
        ManipulateImagesAlpha(disabledAlpha);
    }

    private void ManipulateImagesAlpha(float value)
    {
        for (int i = 0; i < maskableGraphics.Length; i++)
        {
            MaskableGraphic maskableGraphic = maskableGraphics[i];
            Color color = maskableGraphic.color;
            color.a = value;
            maskableGraphic.color = color;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        //TODO: Is there a way to modify the UI element "collider"? We could use alternative methods to achieve this 
        onClickEvent.Invoke();
    }

    //TODO: Make sure these are not clickable before the player is created..
    public void TryTag()
    {
        PlayerController.localPlayerController.TryTag();
    }

    public void TryUsePowerUp()
    {
        PlayerController.localPlayerController.TryUsePowerUp();
    }
}
