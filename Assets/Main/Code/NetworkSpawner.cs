using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;


public class NetworkSpawner : MonoBehaviour
{

    [SerializeField] private SpawnableObjectDefinitionsHolder spawnableObjectDefinitionsHolder;
    private NetworkSpawnable[][] spawnablesPools;
    private Transform allSpawnablesParent;
    private Guid matchID;

    [Server]
    public void Initialise(Guid matchID)
    {
        this.matchID = matchID;
        InitialisePools();
    }

    [Server]
    private void InitialisePools()
    {
        allSpawnablesParent = new GameObject("NetworkSpawnables").transform;
        allSpawnablesParent.position = Vector3.zero;

        int spawnableObjectsLength = (int)NetworkSpawnables.Length;
        spawnablesPools = new NetworkSpawnable[spawnableObjectsLength][];

        for (int i = 0; i < spawnableObjectsLength; i++)
        {
            NetworkSpawnables spawnableObjectName = (NetworkSpawnables)i;
            ref SpawnableObjectDefinition spawnableDefinition = ref spawnableObjectDefinitionsHolder.GetSpawnableDefinition(spawnableObjectName);
            spawnablesPools[i] = CreateDeadPool(spawnableDefinition.preFab,spawnableDefinition.poolSize);// new Spawnable[spawnableObjectDefinition.poolSize];
        }
    }

    [Server]
    public NetworkSpawnable Spawn(NetworkSpawnables spawnableName, Vector3 spawnPosition, Quaternion spawnRotation, uint? callerNetID)
    {
        
        NetworkSpawnable validSpawnable = null;
        int spawnableArrayIndex = (int)spawnableName;
        NetworkSpawnable[] spawnableArray = spawnablesPools[spawnableArrayIndex];

        //ref int spawnableIndex = ref spawnableIndices[spawnableArrayIndex];
        for (int i = 0; i < spawnableArray.Length; i++)
        {
            //TODO: maybe split the pool into living and dead spawnables for quicker lookups
            NetworkSpawnable spawnable = spawnableArray[i];
            if (!spawnable.IsAlive)
            {
                validSpawnable = spawnable;
                goto ValidSpawnableFound;
            }
        }
        //Resize:
        {
            Debug.Log($"Resizein' {spawnableName}'s pool++");
            ref SpawnableObjectDefinition spawnableDefinition = ref spawnableObjectDefinitionsHolder.GetSpawnableDefinition(spawnableName);
            //TODO: What's the ideal length..?
            int oldLength = spawnableArray.Length;
            int deadPoolLength = (int)(oldLength / 2);
            NetworkSpawnable[] deadPool = CreateDeadPool(spawnableDefinition.preFab, deadPoolLength);
            NetworkSpawnable[] newSpawnableArray =  new NetworkSpawnable[deadPoolLength + oldLength];
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

        //TODO: This might be dumb, are we sure that this is the null equivilent of netID? 
        //can we not make an optional parameter instead of carrying useless info through the network?
        uint realCallerNetID = (callerNetID == null ? uint.MaxValue : (uint)callerNetID);

        validSpawnable.Spawn(spawnPosition, spawnRotation, realCallerNetID);
        return validSpawnable;

    }

    private NetworkSpawnable[] CreateDeadPool(NetworkSpawnable preFab, int arrayLength)
    {       
        NetworkSpawnable[] spawnableArray = new NetworkSpawnable[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            //TODO: Destroy when match is over
            NetworkSpawnable spawnable = spawnableArray[i] = Instantiate(preFab, allSpawnablesParent);
            spawnable.GetComponent<NetworkMatch>().matchId = matchID;
            NetworkServer.Spawn(spawnable.gameObject);
           // spawnable.Die();
        }
        return spawnableArray;
    }

    public void KillAll()
    {
        for (int i = 0; i < spawnablesPools.Length; i++)
        {
            NetworkSpawnable[] spawnablesPool = spawnablesPools[i];
            for (int j = 0; j < spawnablesPool.Length; j++)
            {
                NetworkSpawnable spawnable = spawnablesPool[j];
                if (spawnable)
                {
                    if (spawnable.IsAlive)
                    {
                        //TODO: might cause a lot of calls..? maybe tell the client to do this...?
                        spawnable.Die();
                    }
                }
                else
                {
                    Debug.LogError("A null Spawnable was found in a pool while terminating!");
                }
            }
        }
    }

    public void Terminate()
    {
        for (int i = 0; i < spawnablesPools.Length; i++)
        {
            NetworkSpawnable[] spawnablesPool = spawnablesPools[i];
            for (int j = 0; j < spawnablesPool.Length; j++)
            {
                NetworkSpawnable spawnable = spawnablesPool[j];
                if (spawnable)
                {
                    NetworkServer.Destroy(spawnable.gameObject);
                }
                else
                {
                    Debug.LogError("A null Spawnable was found in a pool while terminating!");
                }
            }
        }
        //(This is not a networked object so no need to destroy via NetworkServer.Destroy)
        Destroy(this.gameObject);
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
}
