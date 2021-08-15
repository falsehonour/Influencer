using UnityEngine;
using System;


namespace HashtagChampion
{
    namespace CharacterCreation
    {

        ///<summary>Character Materials Modifier</summary>
        [CreateAssetMenu(fileName = "MatsMod", menuName = "Character Creation Piece/CharacterMatsMod")]
        public class CharacterMatsMod : CharacterMeshMod
        {
            public enum MaterialParameters : byte
            {
                _MainTex, _Color
            }

            [Serializable]
            public struct ColourMod
            {
                public byte matIndex;
                public MaterialParameters colourName;
                public Color32 colour;
            }

            [Serializable]
            public struct TextureMod
            {
                public byte matIndex;
                public MaterialParameters textureName;//TODO:Find a way use index instead...
                public Texture2D texture;
            }

            [SerializeField] private ColourMod[] colourMods;
            public ColourMod[] ColourMods => colourMods;
            [SerializeField] private TextureMod[] textureMods;
            public TextureMod[] TextureMods => textureMods;

            /*[System.Serializable]
            public struct MaterialTexture
            {
                public Texture2D texture2D;
                public Color32 colour;
                public byte materialIndex;
            }*/

        }
    }

}
