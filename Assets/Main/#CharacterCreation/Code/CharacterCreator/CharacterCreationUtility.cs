using UnityEngine;

namespace CharacterCreation
{
    [CreateAssetMenu(fileName = "CharacterCreationUtility", menuName = "Character Creation/CharacterCreationUtility")]
    public class CharacterCreationUtility : ScriptableObject
    {

        [System.Serializable]
        public struct CharacterMeshNameRequirement
        {
            public MeshCategories category;
            public string name;
        }

        [SerializeField] private CharacterMeshNameRequirement[] characterPieceNameRequirements;
        public CharacterMeshNameRequirement[] CharacterPieceNameRequirements => characterPieceNameRequirements;
    }
}

