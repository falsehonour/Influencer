using UnityEngine;
using System;

namespace HashtagChampion
{
    namespace CharacterCreation
    {
        public abstract class CharacterPiece : ScriptableObject { }

        #region Mesh:
        [Flags]
        public enum MeshCategories : byte
        {
            None = 0b_0000_0000,
            Body = 0b_0000_0001,
            HeadWear = 0b_0000_0010,
            Hair = 0b_0000_0100,
            Torso = 0b_0001_0000,
            Eyebrows = 0b_0000_1000,
            Legs = 0b_0010_0000,
            Feet = 0b_0100_0000,
            FullBody = Torso | Legs,
            /*Last = 0b1000000,*/
            Length = 7
        }

        public abstract class CharacterMesh : CharacterPiece
        {
            [SerializeField] private MeshCategories categories;
            public MeshCategories Categories => categories;
            [SerializeField] protected Renderer renderer;
            public Renderer Renderer => renderer;

        }
        #endregion

        #region Mesh Modifier:
        [Flags]
        public enum MeshModifierCategories : UInt16
        {
            None = 0b_0000_0000_0000_0000,
            FaceMorph = 0b_0000_0000_0000_0001,
            NoseMorph = 0b_0000_0000_0000_0010,
            MouthMorph = 0b_0000_0000_0000_0100,
            HairColour = 0b_0000_0000_0000_1000,
            EyebrowsMorph = 0b_0000_0000_0001_0000,
            EyebrowsColour = 0b_0000_0000_0010_0000,
            SkinColourTextures = 0b_0000_0000_0100_0000,
            FeetTextures = 0b_0000_0000_1000_0000,
            LegsTextures = 0b_0000_0001_0000_0000,
            TorsoTextures = 0b_0000_0010_0000_0000,
            EyeColourTextures = 0b_0000_0100_0000_0000,
            FullBodyTextures = LegsTextures | TorsoTextures,

            /*Last = 0b10000000,*/
            Length = 12
        }

        public abstract class CharacterMeshMod : CharacterPiece
        {
            [SerializeField] private MeshModifierCategories categories;
            public MeshModifierCategories Categories => categories;
            [SerializeField] private CharacterMesh[] compatibleMeshes;
            public CharacterMesh[] CompatibleMeshes => compatibleMeshes;
        }
        #endregion
    }
}
