using System;
using UnityEngine;
using System.Collections.Generic;

namespace HashtagChampion
{
    namespace CharacterCreation
    {
        [Serializable]
        public class PlayerSkinDataHolder : ISavable
        {
            [Serializable]
            public struct Data
            {
                public byte characterPrefabIndex;
                public byte[] meshIndexes;
                public byte[] meshModifierIndexes;
                public Data(byte characterPrefabIndex, byte[] meshIndexes, byte[] meshModifierIndexes)
                {
                    this.characterPrefabIndex = characterPrefabIndex;
                    this.meshIndexes = meshIndexes;
                    this.meshModifierIndexes = meshModifierIndexes;
                }
            }
            public Data data;

            public PlayerSkinDataHolder() { }

            public PlayerSkinDataHolder(byte characterPrefabIndex, byte[] meshIndexes, byte[] meshModifierIndexes)
            {
                data = new Data(characterPrefabIndex, meshIndexes, meshModifierIndexes);
            }

            /*public string GetSaveFileName()
            {
                return "player_skin";
            }
            */
            public static PlayerSkinDataHolder CreatePlayerSkinData(Character characterPreFab, CharacterMesh[] meshes, CharacterMeshMod[] modifiers)
            {
                CharacterReferences references = CharacterCreationReferencer.References;
                byte characterPrefabIndex = (byte)references.GetCharacterPreFabIndex(characterPreFab);
                List<byte> meshIndexes = new List<byte>();
                for (int i = 0; i < meshes.Length; i++)
                {
                    if (meshes[i] != null)
                    {
                        byte meshIndex = (byte)references.GetCharacterMeshIndex(meshes[i]);
                        if (!meshIndexes.Contains(meshIndex))
                        {
                            meshIndexes.Add(meshIndex);
                        }
                    }
                }
                List<byte> modifierIndexes = new List<byte>();
                for (int i = 0; i < modifiers.Length; i++)
                {
                    if (modifiers[i] != null)
                    {
                        byte modifierIndex = (byte)references.GetCharacterMeshModifierIndex(modifiers[i]);
                        if (!modifierIndexes.Contains(modifierIndex))
                        {
                            modifierIndexes.Add(modifierIndex);
                        }
                    }
                }

                return new PlayerSkinDataHolder(characterPrefabIndex, meshIndexes.ToArray(), modifierIndexes.ToArray());
            }

        }
    }

}

/*public static class LocalPlayerData
{
    private static PlayerSkinData skinData;

    public static void Initialise()
    {
        skinData = (PlayerSkinData)(SaveAndLoadManager.Load<PlayerSkinData>(new PlayerSkinData()));
        if (skinData == null)
        {
            Debug.LogError("skinData == null");
        }
    }*/


/* public static void SaveChanges()
 {
     SaveAndLoadManager.SavePlayerData(data);
 }

 public static LocalPlayerData LoadSavedData()
 {
    return SaveAndLoadManager.LoadPlayerSavedData();
 }
}*/
