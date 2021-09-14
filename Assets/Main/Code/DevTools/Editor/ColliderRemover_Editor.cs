using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ColliderRemover))]
public class ColliderRemover_Editor : Editor
{
    private ColliderRemover colliderRemover;

    void OnEnable()
    {
         colliderRemover = (ColliderRemover)target;
    }

    public override void OnInspectorGUI()
    {
         base.OnInspectorGUI();
        if (GUILayout.Button(new GUIContent("Remove All Colliders")))
        {
            RemoveAllColliders(colliderRemover.gameObject);
        }
    }
   /*private void OnGUI()
     {
         GUILayout.Label("Collider Remover", EditorStyles.boldLabel);
         selectedObject = EditorGUILayout.ObjectField("Object", selectedObject, typeof(MonoBehaviour), false) as MonoBehaviour;

         if (GUILayout.Button("Remove All Colliders"))
         {
             RemoveAllColliders();
         }
     }*/

    private void RemoveAllColliders(GameObject gameObject)
    {
        Collider[] colliders = gameObject.GetComponentsInChildren<Collider>();
        int count = colliders.Length;
        for (int i = 0; i < count; i++)
        {
            DestroyImmediate(colliders[i]);
        }
        Debug.Log($"Removed {count} colliders from {gameObject.name}");

    }

}

