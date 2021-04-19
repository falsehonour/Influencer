using UnityEngine;
using UnityEngine.UI;

namespace CharacterCreation
{
    public class CharacterCreationButton : MonoBehaviour
    {
        [SerializeField] private Image icon;

        private ButtonBehaviour behaviour;
        private CharacterCreationPanel panel;
        public void Initialise(ButtonBehaviour behaviour, CharacterCreationPanel panel)
        {
            //TODO: Check if this only compiles on editor
#if UNITY_EDITOR
            gameObject.name = behaviour.name;
#endif
            this.behaviour = behaviour;
            this.panel = panel;
            DrawGraphics();
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

        public virtual void OnClick()
        {
            CharacterCreationManager.OnButtonClicked(behaviour, panel);
        }
    }
}
