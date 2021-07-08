using UnityEngine;

namespace CharacterCreation
{
    [CreateAssetMenu(fileName = "CharacterReferences", menuName = "Character Creation/CharacterReferences")]
    public class CharacterReferences : ScriptableObject
    {
        [SerializeField] private Character[] allCharacterPreFabs;
        [SerializeField] private CharacterMesh[] allCharacterMeshes;
        [SerializeField] private CharacterMeshMod[] allCharacterMeshModifiers;

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

        public byte? GetCharacterMeshModifierIndex(CharacterMeshMod modifier)
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

        public byte? GetCharacterPreFabIndex(Character character)
        {
            for (byte i = 0; i < allCharacterPreFabs.Length; i++)
            {
                if (allCharacterPreFabs[i] == character)
                {
                    return i;
                }
            }
            Debug.LogError("Could not find " + character.name);
            return null;
        }

        public Character GetCharacterPreFab(byte index)
        {
            return allCharacterPreFabs[index];
        }

        public CharacterMesh GetCharacterMesh(byte index)
        {
            //Debug.Log("index" + index);
            return allCharacterMeshes[index];
        }

        public CharacterMeshMod GetCharacterMeshModifier(byte index)
        {
            return allCharacterMeshModifiers[index];
        }

        public void UpdateRefferences()
        {
            allCharacterMeshes = Resources.LoadAll<CharacterMesh>("");
            allCharacterMeshModifiers = Resources.LoadAll<CharacterMeshMod>("");

            if (allCharacterMeshes.Length >= byte.MaxValue || allCharacterMeshModifiers.Length >= byte.MaxValue)
            {
                Debug.LogError("one of your collections has a lenghth greater/equal to byte.MaxValue!");
            }
#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}