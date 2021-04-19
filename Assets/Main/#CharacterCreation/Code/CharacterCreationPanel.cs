using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CharacterCreation
{
    public class CharacterCreationPanel : MonoBehaviour
    {
        [SerializeField] private CharacterCreationButton[] buttons;
        [SerializeField] private GameObject pageTurner;
        [SerializeField] private CharacterCreationButton backButton;
        private ButtonBehaviour[] buttonBehaviours;
        private int pageCount;
        private int pageIndex;

        public void Initialise(ButtonBehaviour[] buttonBehaviours, bool addBackButton)
        {
            this.buttonBehaviours = buttonBehaviours;
            pageCount = (buttonBehaviours.Length / buttons.Length) + 1;
            pageTurner.SetActive(pageCount > 1);
            pageIndex = 0;
            ShowButtons();
            if (addBackButton)
            {

            }
            else
            {

            }
        }

        public void SwitchPage()
        {
            pageIndex++;
            if(pageIndex >= pageCount)
            {
                pageIndex = 0;
            }

            ShowButtons();
        }

        private void ShowButtons()
        {
            for (int buttonIndex = 0; buttonIndex < buttons.Length; buttonIndex++)
            {
                CharacterCreationButton button = buttons[buttonIndex];
                int buttonBehaviourIndex = buttonIndex + (pageIndex * buttons.Length);

                if (buttonBehaviourIndex < buttonBehaviours.Length)
                {
                    button.gameObject.SetActive(true);
                    button.Initialise(buttonBehaviours[buttonBehaviourIndex], this);
                }
                else
                {
                    button.gameObject.SetActive(false);
                }
            }
            /*if (addBackButton)
            {
                return; //TODO: Add functionality
                /* CharacterCreationButton backButton = buttons[buttonBehaviours.Length];
                 backButton.gameObject.SetActive(true);
                 backButton.Initialise(TextReverser.Reverse("חזור"), piece.Category);
            }*/
        }
    }
}

