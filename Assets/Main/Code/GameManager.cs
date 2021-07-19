using System.Collections.Generic;
using UnityEngine;
using Mirror;
using System.Collections;

namespace HashtagChampion
{
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
        [SerializeField] private PlayerSettingsManager playerSettingsManager;
        [SerializeField] private GameObject roomManagementCanvas;
        [SerializeField] private GameObject playerSettingsCanvas;

        [SerializeField] private float playerCircleRadius = 2f;
        private static List<Player> relevantPlayersCache = new List<Player>();

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
            //TODO: not the most fitting place for this
            if (roomManagementCanvas)
            {
                roomManagementCanvas.SetActive(false);
            }
            playerSettingsManager.ApplySettings();
            playerSettingsCanvas.gameObject.SetActive(false);
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

            while (Player.allPlayers.Count < roomManager.settings.playerCount)
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

            int playerCount = Player.allPlayers.Count;
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
                    Vector3 position = circleCentre.position + (circleCentre.forward * playerCircleRadius);
                    Quaternion rotation = Quaternion.LookRotation(circleForward * -1);

                    circleSpawnPoints[i] = new TransformStruct(position, rotation);
                }
            }

            for (int i = 0; i < playerCount; i++)
            {
                Player player = Player.allPlayers[i];
                player.TargetRpc_Teleport(circleSpawnPoints[i].position, circleSpawnPoints[i].rotation);
            }

            yield return new WaitForSeconds(2f);//Hardcoded

            //Spinning sequence
            /*kevin.Spin(circleSpawnSpots[taggerIndex].transform.position);
            IEnumerator spinCoroutine = kevin.SpinCoroutine(circleSpawnSpots[taggerIndex].transform.position);
            yield return new wait(spinCoroutine != null);*/
            yield return kevin.SpinCoroutine(circleSpawnPoints[taggerIndex].position);
            yield return new WaitForSeconds(0.2f);//Hardcoded

            for (int i = 0; i < playerCount; i++)
            {
                Player player = Player.allPlayers[i];
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

        private static List<Player> GetRelevantPlayers()
        {
            relevantPlayersCache.Clear();
            int playerCount = Player.allPlayers.Count;
            for (int i = 0; i < playerCount; i++)
            {
                Player player = Player.allPlayers[i];
                if (player.IsAlive())
                {
                    relevantPlayersCache.Add(player);
                }
            }
            return relevantPlayersCache;
        }

        [Server]
        public static void UpdatePlayersState()
        {
            if (State == GameStates.TagGame)
            {
                List<Player> relevantPlayers = GetRelevantPlayers();

                int relevantPlayersCount = relevantPlayers.Count;
                if (relevantPlayersCount == 0)
                {
                    Debug.LogError("relevantPlayersCount == 0");
                }
                else if (relevantPlayersCount == 1)
                {
                    instance.countdown.Server_StopCounting();
                    instance.EndGame(relevantPlayers[0]);
                }
                else
                {
                    //Let's check wheather a tagger exists in the game. if not, promote the player with the highest HP.

                    Player nextTagger = relevantPlayers[0];
                    for (int i = 0; i < relevantPlayersCount; i++)
                    {
                        Player player = relevantPlayers[i];

                        if (player.Tagger)
                        {
                            return;
                        }
                        //TODO: But what about the rare occasion where the highest health value is shared among more than a single player 
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
            List<Player> relevantPlayers = GetRelevantPlayers();
            int relevantPlayersCount = relevantPlayers.Count;
            if (relevantPlayersCount == 0)
            {
                Debug.LogError("relevantPlayersCount == 0");
            }
            else
            {
                //Let's check wheather a tagger exists in the game. if not, promote the player with the highest HP.

                Player winner = relevantPlayers[0];
                for (int i = 0; i < relevantPlayersCount; i++)
                {
                    Player player = relevantPlayers[i];
                    //TODO: But what about the rare occasion where the highest health value is shared among more than a single player 
                    if (player.Health > winner.Health)
                    {
                        winner = player;
                    }
                }
                instance.EndGame(winner);
            }
        }

        [Server]
        private void EndGame(Player winner)
        {
            Debug.Log(winner.name + "WON THE MATCH!");
            winner.Win();

            List<Player> relevantPlayers = GetRelevantPlayers();
            int relevantPlayersCount = relevantPlayers.Count;
            for (int i = 0; i < relevantPlayersCount; i++)
            {
                Player player = relevantPlayers[i];
                if (player != winner)
                {
                    player.Lose();
                }
            }

            state = GameStates.PostGame;
        }
    }
}
