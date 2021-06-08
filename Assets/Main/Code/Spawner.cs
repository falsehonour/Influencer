using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum Spawnables : byte
{
    Null = 0, Bullet = 1, HealthPickup = 2, ThrownFootball = 3, 
    FootballPickup = 4, ThrownBanana = 5, BananaPickup = 6,
    Length = 7
}

public class Spawner : MonoBehaviour//NetworkBehaviour
{
    [System.Serializable]
    private struct SpawnableObjectDefinition
    {
        public Spawnables name;
        public Spawnable preFab;
        public int poolSize;
    }

    public static Spawner instance;
    [SerializeField] private SpawnableObjectDefinition[] spawnableObjectDefinitions;
    private  static SpawnableObjectDefinition nullDefinition = new SpawnableObjectDefinition();
    private static Spawnable[][] spawnablesPools;
    private Transform allSpawnablesParent;
    //private static int[] spawnableIndices;

    private void Awake()
    {
        instance = this;
    }

    [Server]
    public static void Initialise()
    {
        instance.InitialisePools();
    }

    [Server]
    private void InitialisePools()
    {
        allSpawnablesParent = new GameObject("AllSpawnables").transform;
        allSpawnablesParent.position = Vector3.zero;

        int spawnableObjectsLength = (int)Spawnables.Length;
        spawnablesPools = new Spawnable[spawnableObjectsLength][];
        //spawnableIndices = new int[spawnableObjectsLength];

        for (int i = 0; i < spawnableObjectsLength; i++)
        {
            Spawnables spawnableObjectName = (Spawnables)i;
            ref SpawnableObjectDefinition spawnableDefinition = ref GetSpawnableDefinition(spawnableObjectName);
            spawnablesPools[i] = CreateDeadPool(spawnableDefinition.preFab,spawnableDefinition.poolSize);// new Spawnable[spawnableObjectDefinition.poolSize];
        }
    }

    /* [Server]
     public static Spawnable Spawn(Spawnables spawnableName, Vector3 spawnPosition)
     {
         return instance.Spawn(spawnableName, spawnPosition);
     }*/

    [Server]
    public static Spawnable Spawn(Spawnables spawnableName, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        Spawnable validSpawnable = null;
        int spawnableArrayIndex = (int)spawnableName;
        Spawnable[] spawnableArray = spawnablesPools[spawnableArrayIndex];

        //ref int spawnableIndex = ref spawnableIndices[spawnableArrayIndex];
        for (int i = 0; i < spawnableArray.Length; i++)
        {
            //TODO: maybe split the pool into living and dead spawnables for quicker lookups
            Spawnable spawnable = spawnableArray[i];
            if (!spawnable.IsAlive)
            {
                validSpawnable = spawnable;
                goto ValidSpawnableFound;
            }
        }
        //Resize:
        {
            Debug.Log($"Resizein' {spawnableName}'s pool++");
            ref SpawnableObjectDefinition spawnableDefinition = ref instance.GetSpawnableDefinition(spawnableName);
            //TODO: What's the ideal length..?
            int oldLength = spawnableArray.Length;
            int deadPoolLength = (int)(oldLength / 2);
            Spawnable[] deadPool = instance.CreateDeadPool(spawnableDefinition.preFab, deadPoolLength);
            Spawnable[] newSpawnableArray =  new Spawnable[deadPoolLength + oldLength];
            for (int i = 0; i < oldLength; i++)
            {
                newSpawnableArray[i] = spawnableArray[i];
            }
            for (int i = 0; i < deadPoolLength; i++)
            {
                newSpawnableArray[i + oldLength] = deadPool[i];
            }

            spawnablesPools[spawnableArrayIndex] = newSpawnableArray;
            validSpawnable = newSpawnableArray[oldLength];    
        }
        ValidSpawnableFound:
        if(validSpawnable == null)
        {
            Debug.LogWarning("No dead spawnables were found... ");
        }
        validSpawnable.Spawn(spawnPosition, spawnRotation);
        return validSpawnable;

        /* Debug.LogWarning("No dead spawnables were found... ");
         return null;*/
    }

    private ref SpawnableObjectDefinition GetSpawnableDefinition(Spawnables spawnableName)
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

    private Spawnable[] CreateDeadPool(Spawnable preFab, int arrayLength)
    {       
        Spawnable[] spawnableArray = new Spawnable[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            Spawnable spawnable = spawnableArray[i] = 
                Instantiate(preFab, allSpawnablesParent);
            NetworkServer.Spawn(spawnable.gameObject);       
            spawnable.Die();

        }
        return spawnableArray;
    }

    public static GameObject[] GetAllSpawnablePrefabs()
    {
        SpawnableObjectDefinition[] definitions = instance.spawnableObjectDefinitions;
        int length = definitions.Length;
        GameObject[] prefabs = new GameObject[length];
        for (int i = 0; i < length; i++)
        {
            prefabs[i] = definitions[i].preFab.gameObject;
        }
        return prefabs;
    }
    /*[Server]
    public static Spawnable Spawn(Spawnables spawnableName, Vector3 spawnPosition, Quaternion spawnRotation)
    {
        int spawnableArrayIndex = (int)spawnableName;
        ref int spawnableIndex = ref spawnableIndices[spawnableArrayIndex];

        //TODO: Make sure the spawnable is not alive..?
        Spawnable spawnedObject = spawnablesPools[spawnableArrayIndex][spawnableIndex];
        spawnedObject.Spawn(spawnPosition, spawnRotation);

        spawnableIndex++;
        if (spawnableIndex >= spawnablesPools[spawnableArrayIndex].Length)
        {         
            spawnableIndex = 0;
        }

        return spawnedObject;
    }*/

    /*private void Update()
    {
        //For testing purposes:
        if (Input.GetKeyDown(KeyCode.S))
        {
            float offset = 2f;
            Vector3 spawnPosition = new Vector3(Random.Range(-offset, offset), 0, Random.Range(-offset, offset));
            Spawn(Spawnables.Trap, spawnPosition);
        }
    }*/
}
