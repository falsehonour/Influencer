using UnityEngine;

namespace HashtagChampion
{
    namespace CharacterCreation
    {
        [CreateAssetMenu(fileName = "CharacterMeshNameRequirements", menuName = "Character Creation/CharacterMeshNameRequirements")]
        public class CharacterMeshNameRequirements : ScriptableObject
        {
            [System.Serializable]
            private struct CharacterMeshNameRequirementUnit
            {
                public MeshCategories category;
                public string name;
            }

            [SerializeField] private CharacterMeshNameRequirementUnit[] characterPieceNameRequirementUnits;
            public string GetRequiredMeshName(MeshCategories categories)
            {
                string requiredName = null;

                for (int i = 0; i < characterPieceNameRequirementUnits.Length; i++)
                {
                    //TODO: this does not cover meshes that belong to multiple categories yet
                    if (categories == characterPieceNameRequirementUnits[i].category)
                    {
                        requiredName = characterPieceNameRequirementUnits[i].name;
                        break;
                    }
                }

                return requiredName;
            }
        }
    }
}
