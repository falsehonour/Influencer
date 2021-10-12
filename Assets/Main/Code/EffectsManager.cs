using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EffectDefinition
{
    public EffectNames name;
    public Effect preFab;
    public int poolSize;
}

public class EffectsManager : MonoBehaviour
{
    [SerializeField] private EffectDefinition[] effectDefinitions;
    private EffectDefinition NullDefinition;
    private Effect[][] pools;
    private Transform allOneShotEffectsParent;
    public static EffectsManager instance;

    private void Start()
    {
        if (Mirror.NetworkServer.active)
        {
            Destroy(this);
        }
        else
        {
            instance = this;
            Initialise();
        }
    }

    private void Initialise()
    {
        InitialisePools();
    }

    private void InitialisePools()
    {
        allOneShotEffectsParent = new GameObject("AllOneShotEffects").transform;
        allOneShotEffectsParent.SetParent(transform.parent);

        int poolsLength = (int)EffectNames.Length;
        pools = new Effect[poolsLength][];

        for (int i = 0; i < poolsLength; i++)
        {
            EffectNames oneShotEffectName = (EffectNames)i;
            ref EffectDefinition definition = ref GetDefinition(oneShotEffectName);
            pools[i] = CreateDeadPool(definition.preFab, definition.poolSize);// new Spawnable[spawnableObjectDefinition.poolSize];
        }
    }

    private ref EffectDefinition GetDefinition(EffectNames effectName)
    {
        for (int i = 0; i < effectDefinitions.Length; i++)
        {
            if(effectDefinitions[i].name == effectName)
            {
                return ref effectDefinitions[i];
            }
        }

        Debug.LogError($"Could not find definition {effectName}!");
        return ref NullDefinition;
    }

    public Effect Spawn(EffectNames effectName, Vector3 spawnPosition, Quaternion spawnRotation)
    {

        Effect validEffect = null;
        int poolIndex = (int)effectName;
        Effect[] pool = pools[poolIndex];

        for (int i = 0; i < pool.Length; i++)
        {
            //TODO: maybe split the pool into living and dead spawnables for quicker lookups
            Effect effect = pool[i];
            if (!effect.gameObject.activeSelf)
            {
                validEffect = effect;
                goto ValidEffectFound;
            }
        }
        //Resize:
        {
            Debug.Log($"Resizein' {effectName}'s pool++");
            ref EffectDefinition definition = ref GetDefinition(effectName);

            //TODO: What's the ideal length..?
            int oldLength = pool.Length;
            int deadPoolLength = (int)(oldLength / 2);
            Effect[] deadPool = CreateDeadPool(definition.preFab, deadPoolLength);
            Effect[] newArray = new Effect[deadPoolLength + oldLength];
            for (int i = 0; i < oldLength; i++)
            {
                newArray[i] = pool[i];
            }
            for (int i = 0; i < deadPoolLength; i++)
            {
                newArray[i + oldLength] = deadPool[i];
            }

            pools[poolIndex] = newArray;
            validEffect = newArray[oldLength];
        }
    ValidEffectFound:

        validEffect.transform.position = spawnPosition;
        validEffect.transform.rotation = spawnRotation;
        validEffect.gameObject.SetActive(true);

        return validEffect;

       // Debug.Log(validEffect.name + " spawned");
       //validEffect.mainParticleSystem.Play();

    }

    private Effect[] CreateDeadPool(Effect preFab, int arrayLength)
    {
        Effect[] deadPool = new Effect[arrayLength];
        for (int i = 0; i < arrayLength; i++)
        {
            Effect oneShotEffect = deadPool[i] = Instantiate(preFab, allOneShotEffectsParent);
            oneShotEffect.gameObject.SetActive(false);
        }
        return deadPool;
    }

}
