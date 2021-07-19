using UnityEngine;

namespace HashtagChampion
{ 
  namespace CharacterCreation
  {
      [CreateAssetMenu(fileName = "CharacterSkinnedMesh", menuName = "Character Creation Piece/CharacterSkinnedMesh")]
      public class CharacterSkinnedMesh : CharacterMesh
      {
          [SerializeField] private string rootName;
          [SerializeField] private string[] bones;
     
          public string RootName => rootName;
          public string[] Bones => bones;
     
          public void SetSkinnedMesh( string rootName, string[] bones)
          {
              //Remember to set this asset dirty so that changes will be saved to disk;
              this.rootName = rootName;
              this.bones = bones;
          }
      }
      }
}


