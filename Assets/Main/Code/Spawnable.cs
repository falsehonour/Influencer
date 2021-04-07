using UnityEngine;
using Mirror;

public class Spawnable : NetworkBehaviour
{
    static Vector3 HIDDEN_LOCATION = new Vector3(0, -10, 0);

    [Server]
    public void Spawn(Vector3 spawnPosition)
    {
       // gameObject.SetActive(true);
        gameObject.transform.position = spawnPosition;
        Rpc_Spawn(spawnPosition);
    }

    [ClientRpc]
    private void Rpc_Spawn(Vector3 spawnPosition)
    {
        //gameObject.SetActive(true);
        gameObject.transform.position = spawnPosition;
    }

    [Server]
    public void Disappear()
    {
        Hide();
        Rpc_Disappear();
    }

    [ClientRpc]
    private void Rpc_Disappear()
    {
        Hide();
    }

    private void Hide()
    {
        // TODO: For some reason Mirror doesnt spawn inactive objects on clients that connect to the server. 
        // This solution might not be optimal cause it does not stop behaviours executing
        // gameObject.SetActive(false);

        gameObject.transform.position = HIDDEN_LOCATION;
    }
}
