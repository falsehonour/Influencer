using System;
using UnityEngine;

[Serializable]
public struct FloatRange
{
    //TODO: Implement a restriction in the editor so that one cannot make min larger than max or make max smaller than min.
    [SerializeField] public float min;
    [SerializeField] public float max;    
}
