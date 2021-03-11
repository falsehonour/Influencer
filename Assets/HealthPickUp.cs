using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System;

public class HealthPickUp : NetworkBehaviour
{
    [Server]
    public void Collect()
    {
        Rpc_Collect();
    }

    [ClientRpc]
    private void Rpc_Collect()
    {
        gameObject.SetActive(false);
    }
}
