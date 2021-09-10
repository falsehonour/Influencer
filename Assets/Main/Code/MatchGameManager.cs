using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HashtagChampion
{
    public enum GameStates
    {
        Lobby, ChoosingTagger, TagGame, PostGame
    }

    public class MatchGameManager : NetworkBehaviour
    {
        [SyncVar] private GameStates state;
        public GameStates State
        {
            get { return state; }
        }

        [SerializeField] private Kevin kevinPrefab;//TODO: Move refference to some singletom
        public Kevin kevin;
        [SerializeField] private MatchCountdown countdown;
        [SerializeField] private LobbyUI lobbyUI;

        [SerializeField] private float playerCircleRadius = 2f;
       // private List<PlayerController> relevantPlayersCache = new List<PlayerController>();
        private MatchData match;
        public MatchData Match => match;

        public PlayerController[] relevantPlayerControllers;
        private bool initialised;

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

                Server_StartLobby();
            }
            else
            {
                countdown.Client_Initialise();
                lobbyUI = GameSceneManager.GetReferences().lobbyUI;
                Client_StartLobby();
            }

            initialised = true;
        }

        public void SetMatch(MatchData match)
        {
            this.match = match;
            GetComponent<NetworkMatch>().matchId = match.id.ToGuid();
        }


        public void SwitchMatchAccessibility(Player player)
        {
            if (match.host == player)
            {
                //NOTE: there must be a way to do this without conditions;
                if ((match.states & MatchData.StateFlags.Public) != 0)
                {
                    match.states &= ~MatchData.StateFlags.Public;
                }
                else
                {
                    match.states |= MatchData.StateFlags.Public;
                }

                BroadcastMatchDescription();
            }
        }

        public void BroadcastMatchDescription()
        {
            MatchData.Description description = match.GetDescription();
            Rpc_UpdateMatchDescription(description);
           /* int playerCount = match.players.Count;
            for (int i = 0; i < playerCount; i++)
            {
                Player player = match.players[i];
                //player.TargetRpc_UpdateMatchDescription(description, (player == match.host));
            }*/
        }

        [ClientRpc]
        public void Rpc_UpdateMatchDescription(MatchData.Description description)
        {
            lobbyUI.UpdateMatchDescription(description);
        }

        [Server]
        private void Server_StartLobby()
        {
            state = GameStates.Lobby;
            match.states |= MatchData.StateFlags.Lobby;

            for (int i = 0; i < match.players.Count; i++)
            {
                PlayerController playerController = match.players[i].playerController;
                if (playerController == null)
                {
                    Debug.LogWarning("playerController == null");
                }
                else
                {
                    playerController.Server_ConformToInitialState();
                }
            }

            Rpc_StartLobby();
        }

        [ClientRpc]
        private void Rpc_StartLobby()
        {
            if (initialised)
            {
                Client_StartLobby();
            }
        }

        [Client]
        private void Client_StartLobby()
        {
            SoundManager.PlayMusicTrack(SoundNames.LobbyMusic);
            countdown.Client_ConformToInitialState();
            lobbyUI.ShowUI(true);
        }
        /* [Server]
         private IEnumerator WaitForPlayers()
         {
             //HARDCODED values, can we store these in a file seperate to the game so that we don't have to rebuild the game every time we change these

             WaitForSeconds waitForSeconds = new WaitForSeconds(0.2f);
             while (relevantPlayerControllers.Length < match.settings.playerCount)
             {
                 yield return waitForSeconds;
             }
             yield return new WaitForSeconds(2f);

             StartCoroutine(ChooseTagger());

         }*/

        [Server]
        public void StartGame(Player caller)
        {
            if(caller == match.host && state == GameStates.Lobby)
            {
                StartCoroutine(ChooseTagger());
            }
            else
            {
                Debug.Log("Could not start game!");
            }
        }

        [Server]
        private IEnumerator ChooseTagger()
        {
            Rpc_ChooseTagger();

            state = GameStates.ChoosingTagger;
            match.states &= ~MatchData.StateFlags.Lobby;

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

            StartTagGame();

        }

        [ClientRpc]
        private void Rpc_ChooseTagger()
        {
            SoundManager.FadeOutMusic();
            lobbyUI.ShowUI(false);

        }

        [Server]
        private void StartTagGame()
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
                    Debug.LogError("playerController == null");
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
            Invoke(nameof(Server_StartLobby),4f);
            Rpc_EndGame();
        }

        [ClientRpc]
        private void Rpc_EndGame()
        {
            SoundManager.FadeOutMusic();
        }


        [Server]
        public void Terminate()
        {
            NetworkServer.Destroy(kevin.gameObject);
            NetworkServer.Destroy(this.gameObject);
        }
    }
}
