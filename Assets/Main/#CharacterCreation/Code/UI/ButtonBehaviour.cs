using UnityEngine;

namespace HashtagChampion
{
    namespace CharacterCreation
    {
        [CreateAssetMenu(fileName = "ButtonBehaviour", menuName = "Character Creation/ButtonBehaviour")]
        public class ButtonBehaviour : ScriptableObject
        {

            [SerializeField] private CharacterPiece[] characterPieces;
            public CharacterPiece[] CharacterPieces => characterPieces;
            [SerializeField] private Character characterPreFab;
            public Character CharacterPreFab => characterPreFab;
            [SerializeField] private ButtonBehaviour[] linkedButtonBehaviours;
            public ButtonBehaviour[] LinkedButtonBehaviours => linkedButtonBehaviours;

            [SerializeField] private Sprite icon;
            [SerializeField] private Color iconTint = Color.white;

            public Sprite IconSprite => icon;
            public Color IconTint => iconTint;

            public void CopyBehaviours(ButtonBehaviour from)
            {
                characterPieces = from.characterPieces;
                characterPreFab = from.characterPreFab;
                linkedButtonBehaviours = from.linkedButtonBehaviours;
                //icon = from.icon;
            }

            [ContextMenu("Overide tint by first colour mod found")]
            private void OverideTint()
            {
                for (int i = 0; i < characterPieces.Length; i++)
                {
                    if (characterPieces[i] is CharacterMatsMod)
                    {
                        CharacterMatsMod mod = (CharacterMatsMod)characterPieces[i];
                        if(mod.ColourMods != null && mod.ColourMods.Length > 0)
                        {
                            iconTint = mod.ColourMods[0].colour;
                            return;
                        }
                    }
                }
                Debug.LogWarning("No ColourMods found..");
            }
            /* public void SetIcon(Sprite icon)
             {
                 this.icon = icon;
             }*/
        }
    }
}
