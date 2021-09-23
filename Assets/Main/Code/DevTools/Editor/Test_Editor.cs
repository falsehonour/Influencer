using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Test_Editor : EditorWindow
{
    private Mesh selectedMesh;
    private GameObject selectedPreFab;
    private MeshFilter selectedMeshFilter;

    [MenuItem("Custom Tools/Test UV Manipulator")]
    public static void ShowWindow()
    {
        GetWindow(typeof(Test_Editor));
    }

    private void OnGUI()
    {
        GUILayout.Label("Test UV Manipulator", EditorStyles.boldLabel);
        selectedMesh = EditorGUILayout.ObjectField("Mesh", selectedMesh, typeof(Mesh), false) as Mesh;
        selectedPreFab = EditorGUILayout.ObjectField("Pre-Fab", selectedPreFab, typeof(GameObject), false) as GameObject;
        selectedMeshFilter = EditorGUILayout.ObjectField("Mesh Filter", selectedMeshFilter, typeof(MeshFilter), false) as MeshFilter;

        if (GUILayout.Button("Manipulate Mesh"))
        {
            //Manipulate();
            //absolutePath = EditorUtility.SaveFilePanel("Save Image", relativePath, "NewMesh.asset", "asset");
            // string path = AssetDatabase.GetAssetPath(mesh);
            /*AssetDatabase.MoveAsset(path, "Assets/" + "Old.fbx");
            Debug.Log("Mesh path: " + path);*/

            /* Mesh newMesh = CreateManipulatedMesh(mesh);
             AssetDatabase.CreateAsset(newMesh, "Assets/" + "New.fbx");*/
            Debug.Log("selectedMesh IsReadable " + selectedMesh.isReadable);

            Mesh copy = Mesh.Instantiate(selectedMesh);
            //Manipulate(copy);
            //Debug.Log("copy IsReadable " + copy.isReadable); 

            AssetDatabase.CreateAsset(copy, "Assets/" + "New.asset");

            AssetDatabase.SaveAssets();

            /*  AssetDatabase.Refresh();
              AssetDatabase.SaveAssets();
              AssetDatabase.Refresh();*/

        }

        if (GUILayout.Button("Manipulate Pre-Fab"))
        {

            selectedPreFab.GetComponent<MeshRenderer>().enabled = false;

            AssetDatabase.SaveAssets();

             AssetDatabase.Refresh(); 

        }

        if (GUILayout.Button("Manipulate MeshFilter"))
        {

            Mesh newMesh = Instantiate(selectedMeshFilter.sharedMesh);
            AssetDatabase.CreateAsset(newMesh, "Assets/" + "New.mesh");
            selectedMeshFilter.sharedMesh = newMesh;

            AssetDatabase.SaveAssets();

            AssetDatabase.Refresh();

        }
    }

    private void Manipulate(Mesh mesh)
    {   
        mesh.SetVertices(new Vector3[mesh.vertices.Length]);
    }

    private Mesh CreateManipulatedMesh(Mesh originalMesh)
    {
        Mesh newMesh = new Mesh();
        newMesh.SetVertices(originalMesh.vertices);
        //newMesh.SetVertices(new Vector3[originalMesh.vertices.Length]);
        newMesh.SetUVs(0,originalMesh.uv);
        newMesh.SetTriangles(originalMesh.GetTriangles(0),0);
        newMesh.SetNormals(originalMesh.normals);
        
        return newMesh;
    }
}
