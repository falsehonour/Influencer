using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public static class SaveAndLoadManager 
{
    private static string BuildSaveFilePath(string fileName)
    {
        string persistentDataPath = Application.persistentDataPath + "/";
        //string fileName = "player_skin";//TODO: Expand;
        string path = (persistentDataPath + fileName + ".dat");
        return path;
    }

    public static T Load<T>(T data) where T : class, ISavable
    {      
        string path = BuildSaveFilePath(data.GetSaveFileName());
        if (File.Exists(path))
        {
            BinaryFormatter binaryFormatter = new BinaryFormatter();
            FileStream fileStream = File.Open(path, FileMode.Open);
            //T savedData = new ;
            Debug.Log("Loading from " + path);

            object obj = binaryFormatter.Deserialize(fileStream);
            if(obj is T)
            {
                data = (T)obj;
                fileStream.Close();
                return data;
            }
            else
            {
                Debug.LogError("Load Failed! The file was found but did not to the type of "+ typeof(T).ToString());
                Debug.LogError(path);
            }
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