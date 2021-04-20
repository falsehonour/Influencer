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
        private CharacterCreationManager characterCreationManager;
        private ButtonBehaviour[] buttonBehaviours;
        private ButtonBehaviour backButtonBehaviour;
        private CharacterCreationPanel linkedPanel;
        private int pageCount;
        private int pageIndex;

        public void SwitchPage()
        {
            pageIndex++;
            if(pageIndex >= pageCount)
            {
                pageIndex = 0;
            }

            UpdateButtons();
        }

        public void Initialise(ButtonBehaviour[] buttonBehaviours, CharacterCreationManager characterCreationManager, CharacterCreationPanel linkedPanel)
        {
             this.characterCreationManager = characterCreationManager;
             this.linkedPanel = linkedPanel;
             SetNewButtons(buttonBehaviours, null);
             UpdateButtons();
        }

        public void OnButtonClicked(ButtonBehaviour buttonBehaviour, ButtonBehaviour backButtonBehaviour)
        {
            characterCreationManager.OnButtonClicked(buttonBehaviour);

            ButtonBehaviour[] linkedButtonBehaviours = buttonBehaviour.LinkedButtonBehaviours;
            if(linkedButtonBehaviours != null && linkedButtonBehaviours.Length > 0)
            {
                SetNewButtons(linkedButtonBehaviours, backButtonBehaviour);
                UpdateButtons();
            }

        }

        private void SetNewButtons(ButtonBehaviour[] buttonBehaviours, ButtonBehaviour backButtonBehaviour)
        {
            this.buttonBehaviours = buttonBehaviours;
            pageCount =
                (buttonBehaviours != null && buttonBehaviours.Length > 0) ?
                (buttonBehaviours.Length / buttons.Length) + 1 : 0;
            pageTurner.SetActive(pageCount > 1);
            pageIndex = 0;

            this.backButtonBehaviour = backButtonBehaviour;
        }

        private void UpdateButtons()
        {

            bool hasButtonBehaviours = buttonBehaviours != null && buttonBehaviours.Length > 0;
            for (int buttonIndex = 0; buttonIndex < buttons.Length; buttonIndex++)
            {
                CharacterCreationButton button = buttons[buttonIndex];
                bool showButton = false;
                if (hasButtonBehaviours)
                {
                    int buttonBehaviourIndex = buttonIndex + (pageIndex * buttons.Length);

                    if (buttonBehaviourIndex < buttonBehaviours.Length)
                    {
                        showButton = true;
                        button.Initialise(buttonBehaviours[buttonBehaviourIndex], linkedPanel, true);
                    }
                }
                button.gameObject.SetActive(showButton);

            }
            if (backButtonBehaviour != null)
            {
                backButton.gameObject.SetActive(true);
                backButton.Initialise(backButtonBehaviour, linkedPanel, false);
            }
            else
            {
                backButton.gameObject.SetActive(false);
            }
        }

    }
}

