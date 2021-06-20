using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ObjectRemover))]
public class ObjectRemover_Editor : Editor
{
    private string removedObjectsName = "";
    private ObjectRemover objectRemover;

    void OnEnable()
    {
        objectRemover = (ObjectRemover)target;
    }

    public override void OnInspectorGUI()
    {
         base.OnInspectorGUI();
        removedObjectsName = GUILayout.TextField(removedObjectsName);
        if (GUILayout.Button(new GUIContent("Remove Objects Named " + removedObjectsName)))
        {
            RemoveObjects(objectRemover.gameObject);
        }
    }

    private void RemoveObjects(GameObject gameObject)
    {
        Transform[] objects = gameObject.GetComponentsInChildren<Transform>();
        int count = objects.Length;
        int removedCount = 0;
        for (int i = 0; i < count; i++)
        {
            if(objects[i].name == removedObjectsName)
            {
                DestroyImmediate(objects[i].gameObject);
                removedCount++;
            }
        }
        Debug.Log($"Removed {removedCount} objects from {gameObject.name}");

    }

}

