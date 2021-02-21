using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class LightSwitch : NetworkBehaviour
{
    [SerializeField] private Light light;

    public void Switch()
    {
        bool switchedState = !light.enabled;
        RpcTurn(switchedState);
        //CmdSwitch();
    }

   /* [Command]
    private void CmdSwitch()
    {
        RpcSwitch();
    }*/

    [ClientRpc]
    private void RpcTurn(bool value)
    {
        light.enabled = value;
    }
}
