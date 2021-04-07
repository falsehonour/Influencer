
using UnityEngine;
using Mirror;

public class PickUp : Spawnable
{
    [Server]
    public void Collect()
    {
        Disappear();
    }
}
