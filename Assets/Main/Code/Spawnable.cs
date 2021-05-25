using UnityEngine;
using Mirror;

public class Spawnable : NetworkBehaviour
{
    private static readonly Vector3 HIDDEN_LOCATION = new Vector3(0, -10, 0);
    //private NetworkTransform networkTransform;
    protected Transform myTransform;
    private bool isAlive = false;
    public bool IsAlive => isAlive;
    private void Awake()
    {
        myTransform = transform;
       // networkTransform = GetComponent<NetworkTransform>();
    }

    [Server]
    public void Spawn(Vector3 position, Quaternion rotation)
    {
        OnSpawn(position, rotation);
        Rpc_Spawn(position, rotation);
    }

    [ClientRpc]
    private void Rpc_Spawn(Vector3 position, Quaternion rotation)
    {
        if (isClientOnly)
        {
            OnSpawn(position, rotation);
        }
    }

    protected virtual void OnSpawn(Vector3 position, Quaternion rotation)
    {
        // gameObject.SetActive(true);
        myTransform.position = position;
        myTransform.rotation = rotation;
        isAlive = true;
    }

    [Server]
    public void Die()
    {
        OnDeath();
        Rpc_Die();
    }

    [ClientRpc]
    private void Rpc_Die()
    {
        if (isClientOnly)
        {
            OnDeath();
        }
    }

    protected virtual void OnDeath() 
    {
        isAlive = false;
    }

    protected void Hide()
    {
        // TODO: For some reason Mirror doesnt spawn inactive objects on clients that connect to the server. 
        // This solution might not be optimal cause it does not stop behaviours executing
        // gameObject.SetActive(false);

        myTransform.position = HIDDEN_LOCATION;
    }
}
