using UnityEngine;

namespace HashtagChampion
{ 
namespace CharacterCreation
{
    [CreateAssetMenu(fileName = "CharacterStaticMesh", menuName = "Character Creation Piece/CharacterStaticMesh")]
    public class CharacterStaticMesh : CharacterMesh
    {
        [SerializeField] private string parentName;
        [SerializeField] private TransformProperties transformOffset;

        public string ParentName => parentName;
        public TransformProperties TransformOffset => transformOffset;

        public void SetFields(string parentName, TransformProperties transformOffset)
        {
            //Remember to set this asset dirty so that changes will be saved to disk;
            this.parentName = parentName;
            this.transformOffset = transformOffset;
        }
    }
}


}


