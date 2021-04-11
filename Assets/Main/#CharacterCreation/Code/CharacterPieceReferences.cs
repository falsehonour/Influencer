using UnityEngine;

namespace CharacterCreation
{
    [CreateAssetMenu(fileName = "CharacterPieceReferences", menuName = "Character Creation/CharacterPieceReferences")]
    public class CharacterPieceReferences : ScriptableObject
    {

        [SerializeField] private CharacterMesh[] allCharacterMeshes;
        [SerializeField] private CharacterMeshModifier[] allCharacterMeshModifiers;

        public byte? GetCharacterMeshIndex(CharacterMesh mesh)
        {
            for (byte i = 0; i < allCharacterMeshes.Length; i++)
            {
                if(allCharacterMeshes[i] == mesh)
                {
                    return i;
                }
            }
            Debug.LogError("Could not find " + mesh.name);
            return null;
        }

        public byte? GetCharacterMeshModifierIndex(CharacterMeshModifier modifier)
        {
            for (byte i = 0; i < allCharacterMeshModifiers.Length; i++)
            {
                if (allCharacterMeshModifiers[i] == modifier)
                {
                    return i;
                }
            }
            Debug.LogError("Could not find " + modifier.name);
            return null;
        }

        public CharacterMesh GetCharacterMesh(byte index)
        {
            return allCharacterMeshes[index];
        }

        public CharacterMeshModifier GetCharacterMeshModifier(byte index)
        {
            return allCharacterMeshModifiers[index];
        }

        public void UpdateRefferences()
        {
            allCharacterMeshes = Resources.LoadAll<CharacterMesh>("");
            allCharacterMeshModifiers = Resources.LoadAll<CharacterMeshModifier>("");

            if (allCharacterMeshes.Length >= byte.MaxValue || allCharacterMeshModifiers.Length >= byte.MaxValue)
            {
                Debug.LogError("one of your collections has a lenghth greater/equal to byte.MaxValue!");
            }
        }
    }
}