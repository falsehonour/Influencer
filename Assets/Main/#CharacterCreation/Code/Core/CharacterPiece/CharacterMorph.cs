using UnityEngine;

namespace CharacterCreation
{
    [CreateAssetMenu(fileName = "CharacterMorph", menuName = "Character Creation Piece/CharacterMorph")]
    public class CharacterMorph : CharacterMeshModifier
    {
        [System.Serializable]
        public struct MorphBlendShape
        {
            [SerializeField] private string name;
            [SerializeField] private float weight;
            public string Name => name;
            public float Weight => weight;
        }

        [SerializeField] private MorphBlendShape[] morphBlendShapes;
        public MorphBlendShape[] MorphBlendShapes => morphBlendShapes;
    }
}
    