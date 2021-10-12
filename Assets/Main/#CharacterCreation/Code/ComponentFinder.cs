using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.ProBuilder;
public class ComponentFinder : MonoBehaviour
{
    [ContextMenu("Find ProBuilderMeshes in children")]
    private void FindProBuilderMeshesInChildren()
    {
        FindComponentsInChildren<ProBuilderMesh>();
    }

    private void FindComponentsInChildren<T>() where T : Component
    {
         Debug.Log("Objects contaning " + typeof(T).ToString());

         T[] objects = gameObject.GetComponentsInChildren<T>();
         int count = objects.Length;
         for (int i = 0; i < count; i++)
         {
            Debug.Log(objects[i].name);
         }
    }
}
