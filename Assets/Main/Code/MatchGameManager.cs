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

    public class MatchGameManager : NetworkBehaviour
    {
        [SyncVar] private GameStates state;
        public  GameStates State
        {
            get { return state; }
        }

        [SerializeField] private Kevin kevinPrefab;//TODO: Move refference to some singletom
        public Kevin kevin;
        [SerializeField] private MatchCountdown countdown;

        [SerializeField] private float playerCircleRadius = 2f;
       // private List<PlayerController> relevantPlayersCache = new List<PlayerController>();
        private MatchData match;
        public MatchData Match => match;

        public PlayerController[] relevantPlayerControllers;

        private void Start()
        {
            StartCoroutine(InitialisationRoutine());
        }

        private IEnumerator InitialisationRoutine()
        {
            //Wait for GameScene
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.15f);
            while (!GameSceneManager.initialised)
            {
                yield return waitForSeconds;
                Debug.Log("Waiting for game scene...");
            }

           // PlayerController.Initialise();//TODO: Should not be here any longer!

            if (isServer)
            {
                //TODO: Apply room settings
                /*RoomManager roomManager = GetRoomManager();
                PlayerController.ServerData.UpdateTaggerSpeed(roomManager.settings.taggerSpeedBoostInKilometresPerHour);
                PlayerController.ServerData.UpdateRotationSpeed(roomManager.settings.playerRotationSpeed);*/

                Transform kevinSpawnPoint = GameSceneManager.GetReferences().kevinSpawnPoint;

                kevin = Instantiate(kevinPrefab, kevinSpawnPoint.position, kevinSpawnPoint.rotation);
                kevin.GetComponent<NetworkMatch>().matchId = match.id.ToGuid();
                NetworkServer.Spawn(kevin.gameObject);
                kevin.Initialise();

                StartCoroutine(WaitForPlayers());

            }
            else
            {
                SoundManager.PlayMusicTrack(SoundNames.LobbyMusic);
            }
        }

        public void SetMatch(MatchData match)
        {
            this.match = match;
            GetComponent<NetworkMatch>().matchId = match.id.ToGuid();
        }

        [Server]
        private IEnumerator WaitForPlayers()
        {
            //HARDCODED values, can we store these in a file seperate to the game so that we don't have to rebuild the game every time we change these
            state = GameStates.Waiting;
            WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);
            while (relevantPlayerControllers.Length < match.settings.playerCount)
            {
                yield return new WaitForSeconds(1f) ;
            }
            yield return waitForSeconds;

            StartCoroutine(ChooseTagger());

        }

        [Server]
        private IEnumerator ChooseTagger()
        {
            Rpc_ChooseTagger();

            state = GameStates.ChoosingTagger;

            int playerCount = relevantPlayerControllers.Length;
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
                PlayerController playerController = relevantPlayerControllers[i];
                if(playerController == null)
                {
                    Debug.LogError("playerController is null!");
                }
                playerController.TargetRpc_Teleport(circleSpawnPoints[i].position, circleSpawnPoints[i].rotation);
            }
            kevin.SpawnInitialPickups(match.settings.initialPickups);

            yield return new WaitForSeconds(2f);//Hardcoded

            //Spinning sequence
            yield return kevin.SpinCoroutine(circleSpawnPoints[taggerIndex].position);
            yield return new WaitForSeconds(0.2f);//Hardcoded

            for (int i = 0; i < playerCount; i++)
            {
                PlayerController player = relevantPlayerControllers[i];
                player.SetTagger(i == taggerIndex);
            }

            yield return new WaitForSeconds(0.2f);//Hardcoded

            StartMatch();

        }

        [ClientRpc]
        private void Rpc_ChooseTagger()
        {
            SoundManager.FadeOutMusic();
        }

        [Server]
        private void StartMatch()
        {
            state = GameStates.TagGame;
            kevin.StartDropRoutine();
            countdown.Server_StartCounting(match.settings.countdown);
            Rpc_StartMatch();
        }

        [ClientRpc]
        private void Rpc_StartMatch()
        {
            SoundManager.PlayMusicTrack(SoundNames.GameMusic);
        }

        [Server]
        public void UpdatePlayersState()
        {
            GenerateRelevantPlayerControllersArray();
            if (State == GameStates.TagGame)
            {
                int relevantPlayersCount = relevantPlayerControllers.Length;
                if (relevantPlayersCount == 0)
                {
                    Debug.LogError("relevantPlayersCount == 0");
                }
                else if (relevantPlayersCount == 1)
                {
                    countdown.Server_StopCounting();
                    EndGame(relevantPlayerControllers[0]);
                }
                else
                {
                    //Let's check wheather a tagger exists in the game. if not, promote the player with the highest HP.

                    PlayerController nextTagger = relevantPlayerControllers[0];
                    for (int i = 0; i < relevantPlayersCount; i++)
                    {
                        PlayerController player = relevantPlayerControllers[i];

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
        private void GenerateRelevantPlayerControllersArray()
        {
            int count = match.players.Count;
            List<PlayerController> playerControllersList = new List<PlayerController>(count);
            for (int i = 0; i < count; i++)
            {
                PlayerController playerController = match.players[i].playerController;
                if(playerController == null)
                {
                    Debug.LogError("layerController == null");
                }
                else if (/*playerController.Initialised && */playerController.IsAlive())
                {
                    playerControllersList.Add(playerController);
                }
                
            }

            relevantPlayerControllers = playerControllersList.ToArray();
        }

        [Server]
        public void OnCountdownStopped()
        {
            int relevantPlayersCount = relevantPlayerControllers.Length;
            if (relevantPlayersCount == 0)
            {
                Debug.LogError("relevantPlayersCount == 0");
            }
            else
            {
                //Let's check wheather a tagger exists in the game. if not, promote the player with the highest HP.

                PlayerController winner = relevantPlayerControllers[0];
                for (int i = 0; i < relevantPlayersCount; i++)
                {
                    PlayerController player = relevantPlayerControllers[i];
                    //TODO: But what about the rare occasion where the highest health value is shared among more than a single player 
                    if (player.Health > winner.Health)
                    {
                        winner = player;
                    }
                }
                EndGame(winner);
            }
        }

        [Server]
        private void EndGame(PlayerController winner)
        {
            Debug.Log(winner.name + "WON THE MATCH!");
            winner.Win();

            int relevantPlayersCount = relevantPlayerControllers.Length;
            for (int i = 0; i < relevantPlayersCount; i++)
            {
                PlayerController player = relevantPlayerControllers[i];
                if (player != winner)
                {
                    player.Lose();
                }
            }

            state = GameStates.PostGame;
            Invoke(nameof(StopServer),4f);
            Rpc_EndGame();
        }

        [ClientRpc]
        private void Rpc_EndGame()
        {
            SoundManager.FadeOutMusic();
        }

        private void StopServer()
        {
            Debug.Log("<color=yellow>StopServer</color>");
            if (NetworkServer.active)
            {
                if (NetworkClient.isConnected)
                {
                    NetworkManager.singleton.StopHost();
                }
                // stop server if server-only
                else
                {
                    NetworkManager.singleton.StopServer();
                }
            }
            else
            {
                Debug.LogError("StopServer: !NetworkServer.active");
            }

        }

        [Server]
        public void Terminate()
        {
            NetworkServer.Destroy(kevin.gameObject);
            NetworkServer.Destroy(this.gameObject);
        }
    }
}
