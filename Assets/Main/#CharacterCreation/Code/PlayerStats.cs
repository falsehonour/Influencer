using System;
using UnityEngine;
using CharacterCreation;
using System.Collections.Generic;

namespace CharacterCreation
{
    [Serializable]
    public class PlayerSkinData : ISavable
    {
        public byte characterPrefabIndex;
        public byte[] meshIndexes;
        public byte[] meshModifierIndexes;

        public PlayerSkinData() { }

        public PlayerSkinData(byte characterPrefabIndex, byte[] meshIndexes, byte[] meshModifierIndexes)
        {
            this.characterPrefabIndex = characterPrefabIndex;
            this.meshIndexes = meshIndexes;
            this.meshModifierIndexes = meshModifierIndexes;
        }

        public string GetSaveFileName()
        {
            return "player_skin";
        }

        public static PlayerSkinData CreatePlayerSkinData(Character characterPreFab, CharacterMesh[] meshes, CharacterMeshModifier[] modifiers)
        {
            CharacterReferences references = CharacterCreationReferencer.References;
            byte characterPrefabIndex = (byte)references.GetCharacterPreFabIndex(characterPreFab);
            List<byte> meshIndexes = new List<byte>();
            for (int i = 0; i < meshes.Length; i++)
            {
                if(meshes[i] != null)
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

            return new PlayerSkinData(characterPrefabIndex, meshIndexes.ToArray(), modifierIndexes.ToArray());
        }

    }
}

public static class LocalPlayerData
{
    private static PlayerSkinData skinData;

    public static void Initialise()
    {
        skinData = (PlayerSkinData)(SaveAndLoadManager.Load<PlayerSkinData>(new PlayerSkinData()));
        if (skinData == null)
        {
            Debug.LogError("skinData == null");
        }
    }


   /* public static void SaveChanges()
    {
        SaveAndLoadManager.SavePlayerData(data);
    }

    public static LocalPlayerData LoadSavedData()
    {
       return SaveAndLoadManager.LoadPlayerSavedData();
    }*/
}
