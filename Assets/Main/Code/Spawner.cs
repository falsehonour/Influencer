using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;



public class Spawner : MonoBehaviour
{

    [SerializeField] private SpawnableObjectDefinitionsHolder spawnableObjectDefinitionsHolder;
    private Spawnable[][] spawnablesPools;
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
        allSpawnablesParent = new GameObject("AllSpawnables").transform;
        allSpawnablesParent.position = Vector3.zero;

        int spawnableObjectsLength = (int)Spawnables.Length;
        spawnablesPools = new Spawnable[spawnableObjectsLength][];

        for (int i = 0; i < spawnableObjectsLength; i++)
        {
            Spawnables spawnableObjectName = (Spawnables)i;
            ref SpawnableObjectDefinition spawnableDefinition = ref spawnableObjectDefinitionsHolder.GetSpawnableDefinition(spawnableObjectName);
            spawnablesPools[i] = CreateDeadPool(spawnableDefinition.preFab,spawnableDefinition.poolSize);// new Spawnable[spawnableObjectDefinition.poolSize];
        }
    }

    [Server]
    public Spawnable Spawn(Spawnables spawnableName, Vector3 spawnPosition, Quaternion spawnRotation, uint? callerNetID)
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
            ref SpawnableObjectDefinition spawnableDefinition = ref spawnableObjectDefinitionsHolder.GetSpawnableDefinition(spawnableName);
            //TODO: What's the ideal length..?
            int oldLength = spawnableArray.Length;
            int deadPoolLength = (int)(oldLength / 2);
            Spawnable[] deadPool = CreateDeadPool(spawnableDefinition.preFab, deadPoolLength);
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

        //TODO: This might be dumb, are we sure that this is the null equivilent of netID? 
        //can we not make an optional parameter instead of carrying useless info through the network?
        uint realCallerNetID = (callerNetID == null ? uint.MaxValue : (uint)callerNetID);

        validSpawnable.Spawn(spawnPosition, spawnRotation, realCallerNetID);
        return validSpawnable;

    }

    private Spawnable[] CreateDeadPool(Spawnable preFab, int arrayLength)
    {       
        Spawnable[] spawnableArray = new Spawnable[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            //TODO: Destroy when match is over
            Spawnable spawnable = spawnableArray[i] = Instantiate(preFab, allSpawnablesParent);
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
            Spawnable[] spawnablesPool = spawnablesPools[i];
            for (int j = 0; j < spawnablesPool.Length; j++)
            {
                Spawnable spawnable = spawnablesPool[j];
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
            Spawnable[] spawnablesPool = spawnablesPools[i];
            for (int j = 0; j < spawnablesPool.Length; j++)
            {
                Spawnable spawnable = spawnablesPool[j];
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
