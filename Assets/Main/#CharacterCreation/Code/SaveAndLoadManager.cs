using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using CharacterCreation;
public static class SaveAndLoadManager 
{
    private static string BuildSaveFilePath(string fileName)
    {
        string persistentDataPath = Application.persistentDataPath + "/";
        //string fileName = "player_skin";//TODO: Expand;
        string path = (persistentDataPath + fileName + ".dat");
        return path;
    }

    public static ISavable Load<T>(T data) where T : ISavable
    {      
        string path = BuildSaveFilePath(data.GetSaveFileName());
        if (File.Exists(path))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Open(path, FileMode.Open);
            //T savedData = new ;
            data = (T)binaryFormatter.Deserialize(fileStream);
            fileStream.Close();
            Debug.Log("Loading from " + path);
            return data;
        }
        return null;
    }

    public static void Save<T>(T data) where T : ISavable
    {
        string path = BuildSaveFilePath(data.GetSaveFileName());
        if (!File.Exists(path))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
        }
        BinaryFormatter binaryFormatter = new BinaryFormatter();
        FileStream fileStream = File.Open(path, FileMode.OpenOrCreate);
        T savedData = data;// new PlayerStats.PlayerData(name, lives, powerUps);
        binaryFormatter.Serialize(fileStream, savedData);
        Debug.Log("Saving to " + path);
        fileStream.Close();
    }
}