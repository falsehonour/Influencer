using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace HashtagChampion
{
    public class PlayerInputButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private Image image;
        [SerializeField] private GameObject pressedOverlayGraphics;
        private MaskableGraphic[] maskableGraphics;

        [SerializeField] private float disabledAlpha;
        [SerializeField] private UnityEvent onClickEvent;
        private bool isEnabled = true;
        /*[SerializeField] private Sprite unpressedSprite;
        [SerializeField] private Sprite pressedSprite;*/

        private void Start()
        {
            maskableGraphics = GetComponentsInChildren<MaskableGraphic>();
            OnUnpressed();
        }

        public virtual void SimulatePressFor(float duration)
        {
            OnPressed();
            Invoke("OnUnpressed", duration);
        }

        protected virtual void OnPressed()
        {
            pressedOverlayGraphics.SetActive(true);
            // image.sprite = pressedSprite;
        }

        protected virtual void OnUnpressed()
        {
            pressedOverlayGraphics.SetActive(false);
            // image.sprite = unpressedSprite;
        }

        public void SetIsEnabled(bool value)
        {
            //NOTE: We cache the current state in order to avoid unnesssrily 
            //call ManipulateImagesAlpha wchich might be costly.
            if (value != isEnabled)
            {
                ManipulateImagesAlpha(value ? 1 : disabledAlpha);

                isEnabled = value;
            }
        }

        private void Disable()
        {
            SetIsEnabled(false);
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
            OnPressed();
            onClickEvent.Invoke();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            OnUnpressed();
        }

        //TODO: Make sure these are not clickable before the player is created..
        public void TryTag()
        {
            Player.localPlayerController.TryTag();
        }

        public void TryUsePowerUp()
        {
            Player.localPlayerController.TryUsePowerUp();
        }


    }
}

