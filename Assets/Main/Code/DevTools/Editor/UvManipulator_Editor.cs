using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UvManipulator_Editor : EditorWindow
{
    public MeshFilter[] meshFilters;// =new MeshFilter[];
    private float uvMultiplier =1;
    private string newMeshesPath = "Assets/";

    [MenuItem("Custom Tools/UV Manipulator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(UvManipulator_Editor));
    }

    void OnGUI()
    {
        uvMultiplier = EditorGUILayout.FloatField("UV Multiplier",uvMultiplier);
        newMeshesPath = EditorGUILayout.TextField("New Meshes Path", newMeshesPath);

        //mesh filters array field:
        {
            // "target" can be any class derrived from ScriptableObject 
            // (could be EditorWindow, MonoBehaviour, etc)
            ScriptableObject target = this;
            SerializedObject serializedObject = new SerializedObject(target);
            SerializedProperty property = serializedObject.FindProperty(nameof(meshFilters));

            EditorGUILayout.PropertyField(property, true); // True means show children
            serializedObject.ApplyModifiedProperties(); // Remember to apply modified properties
        }

        EditorGUILayout.Space();

        bool pathIsLegal = newMeshesPath[newMeshesPath.Length - 1] == '/';
        bool meshFiltersIsEmpty = (meshFilters == null || meshFilters.Length == 0);
        bool disableButton = (!pathIsLegal || meshFiltersIsEmpty);
        EditorGUI.BeginDisabledGroup(disableButton);
        if (GUILayout.Button("Manipulate!"))
        {
            ManipulateUVs();
        }
        EditorGUI.EndDisabledGroup();

        //Errors and warnings:
        if (!pathIsLegal)
        {
            EditorGUILayout.HelpBox("Path name must end with '/'!", MessageType.Warning);
        }
        if (meshFiltersIsEmpty)
        {
            EditorGUILayout.HelpBox("There are no mesh filters in the mesh filters array...", MessageType.Warning);
        }


    }

    private void ManipulateUVs()
    {
        for (int i = 0; i < meshFilters.Length; i++)
        {
           /* if (!oldMesh.isReadable)
            {
                Debug.LogError
                   ("The mesh you're trying to manipulate ain't readable");
                continue;
            }*/
            MeshFilter meshFilter = meshFilters[i];
            Mesh oldMesh = meshFilter.sharedMesh;
            if (!oldMesh.isReadable)
            {
                Debug.LogError
                   ("The mesh you're trying to manipulate ain't readable");
                continue;
            }
           
            Mesh newMesh = Instantiate(oldMesh); 
            Vector2[] uvs = newMesh.uv;
            for (int j = 0; j < uvs.Length; j++)
            {
                uvs[j] = (uvs[j] * uvMultiplier);
            }
            newMesh.SetUVs(0, uvs);

            AssetDatabase.CreateAsset(newMesh, newMeshesPath + oldMesh.name + ".mesh");
            meshFilter.sharedMesh = newMesh;

        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
