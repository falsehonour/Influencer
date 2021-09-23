using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject[] exclusiveObjects;
    [SerializeField] private MonoBehaviour[] exclusiveComponents;

    public virtual void Activate()
    {
        for (int i = 0; i < exclusiveObjects.Length; i++)
        {
            exclusiveObjects[i].SetActive(true);
        }

        for (int i = 0; i < exclusiveComponents.Length; i++)
        {
            exclusiveComponents[i].enabled = true;
        }

    }

    public virtual void Deactivate()
    {
        for (int i = 0; i < exclusiveObjects.Length; i++)
        {
            exclusiveObjects[i].SetActive(false);
        }

        for (int i = 0; i < exclusiveComponents.Length; i++)
        {
            exclusiveComponents[i].enabled = false;
        }
    }
}
