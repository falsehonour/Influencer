using UnityEngine;

namespace CharacterCreation
{
    public class CharacterCreationReferencer : MonoBehaviour
    {
        private static CharacterCreationReferencer instance;
        [SerializeField] private CharacterMeshNameRequirements nameRequirements;
        public static CharacterMeshNameRequirements NameRequirements => instance.nameRequirements;
        [SerializeField] private CharacterPieceReferences pieceRefferences;
        public static CharacterPieceReferences PieceRefferences => instance.pieceRefferences;

        private void Awake()
        {
            instance = this;
        }
    }
}