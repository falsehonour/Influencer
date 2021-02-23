using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UVManipulator : MonoBehaviour
{
    [SerializeField] private float uvMultiplier;
    [SerializeField] private Mesh[] meshes;
    

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            MeshFilter[] meshFilters = FindObjectsOfType<MeshFilter>();
            meshes = new Mesh[meshFilters.Length];
            for (int i = 0; i < meshFilters.Length; i++)
            {
                meshes[i] = meshFilters[i].sharedMesh;
            }
            ManipulateUVs();
        }
    }

    private void ManipulateUVs()
    {
        for (int i = 0; i < meshes.Length; i++)
        {
            Mesh mesh = meshes[i];
            if (meshes[i].isReadable)
            {
                Vector2[] uvs = mesh.uv;
                //Vector2[] newUvs = new Vector2[oldUvs.Length];

                for (int j = 0; j < uvs.Length; j++)
                {
                    uvs[j] = (uvs[j] * uvMultiplier);
                }
                mesh.SetUVs(0, uvs);

            }
            else
            {
                Debug.LogError
                    ("The mesh you're trying to manipulate ain't readable");
            }
        }
       /* UnityEditor.AssetDatabase.SaveAssets();
        ModelImporter modelImporter;
       // modelImporter.PA*/
    }
}
