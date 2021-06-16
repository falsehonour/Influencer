using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;

public enum GameStates
{
    Waiting, ChoosingTagger, TagGame, PostGame
}

public class GameManager : NetworkBehaviour
{
    [SyncVar] private GameStates state;
    public static GameStates State
    {
        get { return instance.state; }
    }

    //private RoomSettings roomSettings;
    //[SerializeField] private Transform[] circleSpawnSpots;

    [SerializeField] private Kevin kevin;
    [SerializeField] private MatchCountdown countdown;
    [SerializeField] private RoomManager roomManager;
    [SerializeField] private GameObject  roomManagementCanvas;
    [SerializeField] private float playerCircleRadius = 2f;

    private static GameManager instance;

    private void Awake()
    {
        instance = this;
        //roomSettings = 
    }

    [Server]
    public void OnServerStarted()
    {
        Rpc_OnServerStarted();
        StartCoroutine(WaitForPlayers());
    }

    private void Start()
    {
        roomManagementCanvas.SetActive(false);
    }

    [ClientRpc]
    private void Rpc_OnServerStarted()
    {
        if (isClientOnly)
        {
            roomManagementCanvas.SetActive(false);
        }
    }

 
    [Server]
    private IEnumerator WaitForPlayers()
    {
        //HARDCODED values, can we store these in a file seperate to the game so that we don't have to rebuild the game every time we change these
        Debug.Log("WaitForPlayers()");

        state = GameStates.Waiting;

        while (PlayerController.allPlayers.Count < roomManager.settings.playerCount)
        {
            yield return new WaitForSeconds(0.25f);
        }
        StartCoroutine(ChooseTagger());

    }

    [Server]
    private IEnumerator ChooseTagger()
    {
        Debug.Log("ChooseTagger()");

        state = GameStates.ChoosingTagger;

        int playerCount = PlayerController.allPlayers.Count;
        int taggerIndex = Random.Range(0, playerCount);

        //NOTE: David, please convert my solution to a mathematically elegant one
        TransformStruct[] circleSpawnPoints = new TransformStruct[playerCount];
        {

            Transform circleCentre = new GameObject().transform;
            circleCentre.position = kevin.transform.position;
            float anglePortion = 360f / (float)playerCount;
            //Vector3 circleCentrePosition = kevin.transform.position;
            for (int i = 0; i < playerCount; i++)
            {
                float angle = i * anglePortion;
                // circleCentre.Rotate(new Vector3(0, angle, 0));
                circleCentre.rotation = Quaternion.Euler(0, angle, 0);
                Vector3 circleForward = circleCentre.forward;
                Vector3 position = circleCentre.position +   (circleCentre.forward * playerCircleRadius);
                Quaternion rotation = Quaternion.LookRotation(circleForward * -1);

                circleSpawnPoints[i] = new TransformStruct(position, rotation);
            }
        }

        for (int i = 0; i < playerCount; i++)
        {
            PlayerController player = PlayerController.allPlayers[i];
            player.TargetRpc_Teleport(circleSpawnPoints[i].position, circleSpawnPoints[i].rotation);
        }

        yield return new WaitForSeconds(2f);//Hardcoded

        //Spinning sequence
        /*kevin.Spin(circleSpawnSpots[taggerIndex].transform.position);
        IEnumerator spinCoroutine = kevin.SpinCoroutine(circleSpawnSpots[taggerIndex].transform.position);
        yield return new wait(spinCoroutine != null);*/
        yield return kevin.SpinCoroutine(circleSpawnPoints[taggerIndex].position);
        //yield return new WaitForSeconds(3f);//Hardcoded
        yield return new WaitForSeconds(0.2f);//Hardcoded

        for (int i = 0; i < playerCount; i++)
        {
            PlayerController player = PlayerController.allPlayers[i];
            player.SetTagger(i == taggerIndex);
        }

        yield return new WaitForSeconds(0.2f);//Hardcoded

        StartMatch();

    }

    [Server]
    private void StartMatch()
    {
        state = GameStates.TagGame;
        kevin.StartDropRoutine();
        countdown.Server_StartCounting(roomManager.settings.countdown);
    }


    private static List<PlayerController> GetRelevantPlayers()
    {
        List<PlayerController> relevantPlayers = new List<PlayerController>();

        int playerCount = PlayerController.allPlayers.Count;
        for (int i = 0; i < playerCount; i++)
        {
            PlayerController player = PlayerController.allPlayers[i];
            if (player.IsAlive)
            {
                relevantPlayers.Add(player);
            }
        }
        return relevantPlayers;
    }

    [Server]
    public static void UpdatePlayersState()
    {
        if(State == GameStates.TagGame)
        {
            List<PlayerController> relevantPlayers = GetRelevantPlayers();

            int relevantPlayersCount = relevantPlayers.Count;
            if (relevantPlayersCount == 0)
            {
                Debug.LogError("relevantPlayersCount == 0");
            }
            else if (relevantPlayersCount == 1)
            {
                instance.countdown.Server_StopCounting();
                instance.DeclareWinner(relevantPlayers[0]);
            }
            else
            {
                //Let's check wheather a tagger exists in the game. if not, promote the player with the highest HP.

                PlayerController nextTagger = relevantPlayers[0];
                for (int i = 0; i < relevantPlayersCount; i++)
                {
                    PlayerController player = relevantPlayers[i];

                    if (player.Tagger)
                    {
                        return;
                    }
                    else if (player.Health > nextTagger.Health)
                    {
                        nextTagger = player;
                    }
                }
                nextTagger.SetTagger(true);
            }
        }
       
    }

    [Server]
    public static void OnCountdownStopped()
    {
        List<PlayerController> relevantPlayers = GetRelevantPlayers();
        int relevantPlayersCount = relevantPlayers.Count;
        if (relevantPlayersCount == 0)
        {
            Debug.LogError("relevantPlayersCount == 0");
        }
        else
        {
            //Let's check wheather a tagger exists in the game. if not, promote the player with the highest HP.

            PlayerController winner = relevantPlayers[0];
            for (int i = 0; i <  relevantPlayersCount; i++)
            {
                PlayerController player = relevantPlayers[i];
                if (player.Health > winner.Health)
                {
                    winner = player;
                }
            }
            instance.DeclareWinner(winner);
        }
    }

    [Server]
    private void DeclareWinner(PlayerController winner)
    {
        Debug.Log(winner.name + "WON THE MATCH!");
        winner.Rpc_Win();
        state = GameStates.PostGame;
    }
}