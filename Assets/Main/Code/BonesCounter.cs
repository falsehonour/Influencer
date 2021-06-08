using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonesCounter : MonoBehaviour
{
    [SerializeField] private SkinnedMeshRenderer[] skinnedMeshRenderers;
    [SerializeField] private int[] numberOfBones;

    private void Start()
    {
        numberOfBones = new int[skinnedMeshRenderers.Length];
    }

    void Update()
    {
        for (int i = 0; i < numberOfBones.Length; i++)
        {
            numberOfBones[i] = skinnedMeshRenderers[i].bones.Length;
        }
    }
}
