using System;
using UnityEngine;
using CharacterCreation;

namespace CharacterCreation
{
    [Serializable]
    public class PlayerSkinData : ISavable
    {
        public byte[] meshIndexes;
        public byte[] meshModifierIndexes;

        public PlayerSkinData() { }

        public PlayerSkinData(byte[] meshIndexes, byte[] meshModifierIndexes)
        {
            this.meshIndexes = meshIndexes;
            this.meshModifierIndexes = meshModifierIndexes;
        }

        public string GetSaveFileName()
        {
            return "player_skin";
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
