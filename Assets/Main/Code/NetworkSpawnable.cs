using System.Collections;
using UnityEngine;
using Mirror;

public abstract class NetworkSpawnable : NetworkBehaviour
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

    private void Start()
    {
        OnDeath();
    }

    [Server]
    public void Spawn(Vector3 position, Quaternion rotation, uint callerNetID)
    {
        OnSpawn(position, rotation, callerNetID);
        Rpc_Spawn(position, rotation, callerNetID);
    }

    [ClientRpc]
    private void Rpc_Spawn(Vector3 position, Quaternion rotation, uint callerNetID)
    {
        if (isClientOnly)
        {
            OnSpawn(position, rotation, callerNetID);
        }
    }

    protected virtual void OnSpawn(Vector3 position, Quaternion rotation, uint callerNetID)
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
        // TODO/NOTE: For some reason Mirror doesnt spawn inactive objects on clients that connect to the server. 
        // This solution might not be optimal cause it does not stop behaviours executing
        // gameObject.SetActive(false);

        myTransform.position = HIDDEN_LOCATION;
    }

    //TODO: This was hastly writ for presentin'
    protected IEnumerator Shrink(float speed)
    {
        float size = 1;

        while (true)
        {
            if (IsAlive)
            {
                goto StopCoroutine;
            }
            size -= speed * Time.deltaTime;
            if(size < 0)
            {
                break;
            }
            myTransform.localScale = size * Vector3.one;
            yield return null;
        }
        myTransform.localScale = Vector3.zero;
        StopCoroutine: { }
    }
}
