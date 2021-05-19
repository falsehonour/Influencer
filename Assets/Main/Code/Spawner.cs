using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public enum Spawnables : byte
{
    Trap = 0, Bullet = 1, HealthPickup = 2,
    Length = 3
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
    private static Spawnable[][] spawnablesPools;
    private static int[] spawnableIndices;

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
        Transform allSpawnablesParent = new GameObject("AllSpawnables").transform;
        allSpawnablesParent.position = new Vector3();

        int spawnableObjectsLength = (int)Spawnables.Length;
        spawnablesPools = new Spawnable[spawnableObjectsLength][];
        spawnableIndices = new int[spawnableObjectsLength];

        for (int i = 0; i < spawnableObjectsLength; i++)
        {
            Spawnables spawnableObjectName = (Spawnables)i;
            SpawnableObjectDefinition spawnableObjectDefinition = new SpawnableObjectDefinition();
            //Look for definition:
            for (int j = 0; j < spawnableObjectDefinitions.Length; j++)
            {
                if (spawnableObjectDefinitions[j].name == spawnableObjectName)
                {
                    spawnableObjectDefinition = spawnableObjectDefinitions[j];
                    goto DefinitionFound;
                }
            }

            Debug.LogError($"There is a missing spawnable definition for '{ spawnableObjectName.ToString()}'! Fix that.");
            continue;

            DefinitionFound:
            {
                /* Transform parent = new GameObject(spawnableObjectName.ToString() + "s").transform;
                 parent.position = new Vector3();
                 parent.SetParent(allSpawnablesParent);*/

                Spawnable[] spawnableArray = spawnablesPools[i] = new Spawnable[spawnableObjectDefinition.poolSize];
                for (int j = 0; j < spawnableArray.Length; j++)
                {
                    Spawnable spawnable = spawnableArray[j] = Instantiate(spawnableObjectDefinition.preFab);
                    /*spawnable.transform.SetParent(parent);
                    spawnable.gameObject.SetActive(false);*/
                    NetworkServer.Spawn(spawnable.gameObject);
                    spawnable.Die();
                }
            }
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
    }

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
