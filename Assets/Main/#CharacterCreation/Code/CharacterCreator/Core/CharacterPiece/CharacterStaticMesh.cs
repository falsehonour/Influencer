using UnityEngine;

namespace CharacterCreation
{
    [CreateAssetMenu(fileName = "CharacterStaticMesh", menuName = "Character Creation Piece/CharacterStaticMesh")]
    public class CharacterStaticMesh : CharacterMesh
    {
        [SerializeField] private MeshRenderer meshRenderer;
        [SerializeField] private string parentName;
        [SerializeField] private TransformProperties transformOffset;

        public MeshRenderer MeshRenderer => meshRenderer;
        public string ParentName => parentName;
        public TransformProperties TransformOffset => transformOffset;

        public void SetFields(MeshRenderer meshRenderer, string parentName, TransformProperties transformOffset)
        {
            //Remember to set this asset dirty so that changes will be saved to disk;
            this.meshRenderer = meshRenderer;
            this.parentName = parentName;
            this.transformOffset = transformOffset;
        }
    }
}


