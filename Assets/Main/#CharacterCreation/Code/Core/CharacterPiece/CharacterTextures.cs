using UnityEngine;

namespace CharacterCreation
{
    [CreateAssetMenu(fileName = "CharacterTextures", menuName = "Character Creation Piece/CharacterTextures")]
    public class CharacterTextures : CharacterMeshModifier
    {
        [System.Serializable]
        public struct MaterialTexture
        {
            public Texture2D texture2D;
            public Color32 colour;
            public byte materialIndex;
        }
        [SerializeField] private MaterialTexture[] materialTextures;
        public MaterialTexture[] MaterialTextures => materialTextures;
    }
}
