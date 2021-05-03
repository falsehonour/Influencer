using UnityEngine;

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
        public Sprite Icon => icon;
    }
}