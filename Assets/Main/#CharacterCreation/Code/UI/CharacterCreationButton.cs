using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace CharacterCreation
{
    public class CharacterCreationButton : MonoBehaviour
    {
        private static ButtonBehaviour lastClickedButtonBehaviour;
        private static List<ButtonBehaviour> buttonHistory = new List<ButtonBehaviour>();

        [SerializeField] private Image icon;
        private ButtonBehaviour behaviour;
        private CharacterCreationPanel linkedPanel;


        public void Initialise(ButtonBehaviour behaviour, CharacterCreationPanel panel, bool UpdateGraphics)
        {
            //TODO: Check if this only compiles on editor
#if UNITY_EDITOR
            gameObject.name = behaviour.name;
#endif
            this.behaviour = behaviour;
            this.linkedPanel = panel;
            if (UpdateGraphics)
            {
                DrawGraphics();
            }
        }

        private void DrawGraphics()
        {
            //TODO: Get rid of this mess
            bool hasIcon = behaviour.Icon != null;
            Sprite iconSprite = (hasIcon ? behaviour.Icon : null);
            string text = (hasIcon ? "" : behaviour.name);
            icon.sprite = iconSprite;
            GetComponentInChildren<Text>().text = text;
        }

        public void OnClick()
        {
            if (this.behaviour == lastClickedButtonBehaviour)
            {
                Debug.LogWarning("Tried to click the same object more than once in a row. Aborting");
                return;
            }

            lastClickedButtonBehaviour = this.behaviour;

            if(HasLinkedBehaviours())
            {
                int buttonHistoryCount = buttonHistory.Count;
                if (buttonHistoryCount > 0)
                {
                    for (int i = buttonHistoryCount - 1; i >= 0; i--)
                    {
                        var buttons = buttonHistory[i].LinkedButtonBehaviours;
                        if (buttons != null)
                        {
                            for (int j = 0; j < buttons.Length; j++)
                            {
                                if (buttons[j] == this.behaviour)
                                {
                                    Debug.Log("Button was found in buttonHistory");
                                    goto Continue;
                                }
                            }
                        }
                        buttonHistory.RemoveAt(i);
                    }
                }
                Continue:
                buttonHistory.Add(this.behaviour);
            }
            ButtonBehaviour backButtonBehaviour =
               (buttonHistory.Count > 1 ? buttonHistory[buttonHistory.Count - 2] : null);
            linkedPanel.OnButtonClicked(behaviour, backButtonBehaviour);

            string debug = "Button History: ";
            for (int i = 0; i < buttonHistory.Count; i++)
            {
                debug += buttonHistory[i].name + ", ";
            }
            Debug.Log(debug);


        }

        public bool HasLinkedBehaviours()
        {
            return (this.behaviour.LinkedButtonBehaviours != null && this.behaviour.LinkedButtonBehaviours.Length > 0);
        }
    }
}
