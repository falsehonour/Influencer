using UnityEngine;
using UnityEngine.UI;

namespace CharacterCreation
{
    public class CharacterCreationButton : MonoBehaviour
    {
        [SerializeField] private Image icon;

        private ButtonBehaviour behaviour;

        public void Initialise(ButtonBehaviour behaviour)
        {
            gameObject.name = behaviour.name;
            this.behaviour = behaviour;
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
            CharacterCreationManager.OnButtonClicked(behaviour);
        }
    }
}
