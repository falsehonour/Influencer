using UnityEngine;

namespace CharacterCreation
{
    [CreateAssetMenu(fileName = "CharacterSkinnedMesh", menuName = "Character Creation Piece/CharacterSkinnedMesh")]
    public class CharacterSkinnedMesh : CharacterMesh
    {
        [SerializeField] private SkinnedMeshRenderer skinnedMesh;
        [SerializeField] private string rootName;
        [SerializeField] private string[] bones;

        public SkinnedMeshRenderer SkinnedMesh => skinnedMesh;
        public string RootName => rootName;
        public string[] Bones => bones;

        public void SetSkinnedMesh(SkinnedMeshRenderer skinnedMesh, string rootName, string[] bones)
        {
            //Remember to set this asset dirty so that changes will be saved to disk;
            this.skinnedMesh = skinnedMesh;
            this.rootName = rootName;
            this.bones = bones;
        }
    }
}
