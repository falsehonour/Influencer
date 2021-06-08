using UnityEngine;
public struct TransformStruct
{
    public Vector3 position;
    public Quaternion rotation;
    public TransformStruct(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }

    public TransformStruct(Vector3 position)
    {
        this.position = position;
        this.rotation = Quaternion.identity;
    }
}
