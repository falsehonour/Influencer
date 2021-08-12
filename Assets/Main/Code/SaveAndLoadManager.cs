using UnityEngine;
using System.IO;
//using System.Runtime.Serialization.Formatters.Binary;

public static class SaveAndLoadManager 
{
    //Note: Deserialize might be dangerous: https://docs.microsoft.com/he-il/dotnet/standard/serialization/binaryformatter-security-guide

    private static string BuildSaveFilePath(System.Type type )
    {    
        string dataPath = Application.persistentDataPath + "/";
        string path = (dataPath + type.ToString() + ".txt");
        return path;
    }

    public static T TryLoad<T>() where T : class,  ISavable
    {
        //TODO: Handle situations where a ISavable  changed.
        
        string path = BuildSaveFilePath(typeof(T));
        if (File.Exists(path))
        {
            //TODO: Should we cache this BinaryFormatter?
            string json = File.ReadAllText(path);
            try
            {
                T loadedObject = JsonUtility.FromJson<T>(json);
                Debug.Log("Loaded from " + path);
                return loadedObject;
            }
            catch
            {
                Debug.LogError("JsonUtility.FromJson failed!");
            }
        }
        return null;
    }

    public static void Save<T>(T data) where T : class, ISavable
    {
        string path = BuildSaveFilePath(typeof(T));
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(path, json);
        Debug.Log("Saving to " + path);
    }
}