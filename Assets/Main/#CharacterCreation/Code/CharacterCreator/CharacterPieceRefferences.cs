using UnityEngine;

namespace CharacterCreation
{
    [CreateAssetMenu(fileName = "CharacterPieceRefferences", menuName = "Character Creation/CharacterPieceRefferences")]
    public class CharacterPieceRefferences : ScriptableObject
    {
        [SerializeField] private CharacterMesh[] allCharacterMeshes;
        [SerializeField] private CharacterMeshModifier[] allCharacterMeshModifiers;
        private static CharacterPieceRefferences instance;
        public void UpdateRefferences()
        {
            instance = this;
            allCharacterMeshes = Resources.LoadAll<CharacterMesh>("");
            if (allCharacterMeshes.Length >= byte.MaxValue)
            {
                Debug.LogError("allCharacterMeshes indexes can be greater than byte.MaxValue!");
            }
            allCharacterMeshModifiers = Resources.LoadAll<CharacterMeshModifier>("");
            if (allCharacterMeshModifiers.Length >= byte.MaxValue)
            {
                Debug.LogError("allCharacterMeshes indexes can be greater than byte.MaxValue!");
            }
        }

        public static byte? GetCharacterMeshIndex(CharacterMesh mesh)
        {
            CharacterMesh[] allCharacterMeshes = instance.allCharacterMeshes;
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

        public static byte? GetCharacterMeshModifierIndex(CharacterMeshModifier modifier)
        {
            CharacterMeshModifier[] allCharacterMeshModifiers = instance.allCharacterMeshModifiers;
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

        public static CharacterMesh GetCharacterMesh(byte index)
        {
            return instance.allCharacterMeshes[index];
        }
        public static CharacterMeshModifier GetCharacterMeshModifier(byte index)
        {
            return instance.allCharacterMeshModifiers[index];
        }
    }
}