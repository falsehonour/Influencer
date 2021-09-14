using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Spawnables : byte
{
    Null = 0, HealthPickup = 2, ThrownFootball = 3, FootballPickup = 4,
    ThrownBanana = 5, BananaPickup = 6, GunPickup = 7, Bullet = 1, SprintPickup = 8,
    Length = 9
}

[System.Serializable]
public struct SpawnableObjectDefinition
{
    public Spawnables name;
    public Spawnable preFab;
    public int poolSize;
}

[CreateAssetMenu(fileName = "SpawnableObjectDefinitionsHolder", menuName = "SpawnableObjectDefinitionsHolder", order = 1)]
public class SpawnableObjectDefinitionsHolder : ScriptableObject
{
    [SerializeField] private SpawnableObjectDefinition[] spawnableObjectDefinitions;
    private static SpawnableObjectDefinition nullDefinition = new SpawnableObjectDefinition();

    public GameObject[] GetAllSpawnablePrefabs()
    {   
        int length = spawnableObjectDefinitions.Length;
        List<GameObject> prefabs = new List<GameObject>();
        for (int i = 0; i < length; i++)
        {
            if (spawnableObjectDefinitions[i].preFab != null)
            {
                prefabs.Add(spawnableObjectDefinitions[i].preFab.gameObject);
            }
        }
        return prefabs.ToArray();
    }


    public ref SpawnableObjectDefinition GetSpawnableDefinition(Spawnables spawnableName)
    {
        //Look for definition:
        for (int j = 0; j < spawnableObjectDefinitions.Length; j++)
        {
            if (spawnableObjectDefinitions[j].name == spawnableName)
            {
                return ref spawnableObjectDefinitions[j];
            }
        }

        Debug.LogError($"There is a missing spawnable definition for '{ spawnableName.ToString()}'! Fix that.");
        return ref nullDefinition;
    }
}
