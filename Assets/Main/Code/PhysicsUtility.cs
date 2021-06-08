using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicsUtility 
{
    public static void SetRootAndDecendentsLayers(GameObject go, int layer)
    {
        go.layer = layer;
        Transform goTransform = go.transform;
        int childCount = goTransform.childCount;
        //TODO: consider caching them childersss
        if (childCount > 0)
        {
            for (int i = 0; i < childCount; i++)
            {
                SetRootAndDecendentsLayers(goTransform.GetChild(i).gameObject, layer);
            }
        }
    }
}
