using System.Collections;
using System.Collections.Generic;
using HashtagChampion;
using UnityEngine;

public class GameSceneManager : MonoBehaviour
{
    [System.Serializable]
    public class References
    {
        public Transform playerSpawnPoint;
        public Transform kevinSpawnPoint;
        public MatchCountdownDisplay countdownDisplay;
        public Transform kevinDropPointsParent;
        public PlayerCamera playerCamera;
        public LobbyUI lobbyUI;

        public void DestroyServerOnlyObjects()
        {
            //TODO: Find a way to not create this in the first place...
            Destroy(kevinDropPointsParent.gameObject);
        }

        public void DestroyClientOnlyObjects()
        {
            //TODO: Find a way to not create this in the first place...
            Destroy(playerCamera.gameObject);
            Destroy(lobbyUI.gameObject);

        }

    }
    [SerializeField] private References references;
    public static bool initialised;
    private static GameSceneManager instance;

    public static References GetReferences() => instance.references;

    void Start()
    {
        if (Mirror.NetworkServer.active)
        {
            references.DestroyClientOnlyObjects();
        }
        else
        {
            references.DestroyServerOnlyObjects();
        }
        initialised = true;
        instance = this;
        Debug.Log("<color=yellow>GameSceneManager initialised</color>");
    }

    private void OnDestroy()
    {
        initialised = false;
        Debug.Log("<color=yellow>GameSceneManager uninitialised</color>");
    }
}
