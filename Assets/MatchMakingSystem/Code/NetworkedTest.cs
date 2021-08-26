using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class NetworkedTest : NetworkBehaviour
{
    [SyncVar] public string testString; 

    void Start()
    {
        if (isServer)
        {
            testString = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                testString += Random.Range(0, 10).ToString();
            }
        }
    }

    [ContextMenu("Guid Test")]
    private void GuidTest()
    {
        NetworkMatch networkMatch = GetComponent<NetworkMatch>();
        Debug.Log("networkMatch.matchId : " + networkMatch.matchId);
        Debug.Log("System.Guid.Empty    : " + System.Guid.Empty.ToString());
        Debug.Log("System.Guid.NewGuid(): " + System.Guid.NewGuid().ToString());
        Debug.Log("string.Empty.ToGuid  : " + string.Empty.ToGuid());
    }

}
