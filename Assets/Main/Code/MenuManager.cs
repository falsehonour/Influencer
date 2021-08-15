using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private GameObject[] exclusiveObjects;

    public virtual void Activate()
    {
        for (int i = 0; i < exclusiveObjects.Length; i++)
        {
            exclusiveObjects[i].SetActive(true);
        }

    }

    public virtual void Deactivate()
    {
        for (int i = 0; i < exclusiveObjects.Length; i++)
        {
            exclusiveObjects[i].SetActive(false);
        }
    }
}
