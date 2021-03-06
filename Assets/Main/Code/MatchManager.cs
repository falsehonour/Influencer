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

    public class MatchManager : NetworkBehaviour
    {
        [SyncVar] private GameStates state;
        public GameStates State
        {
            get { return state; }
        }

        [SerializeField] private NetworkSpawner spawnerPrefab;//TODO: Move refference to some singletom
        [HideInInspector] public NetworkSpawner spawner;
        [SerializeField] private Kevin kevinPrefab;//TODO: Move refference to some singletom
        [HideInInspector] public Kevin kevin;
        [SerializeField] private MatchCountdown countdown;
        [SerializeField] private LobbyUI lobbyUI;

        [SerializeField] private float playerCircleRadius = 2f;
       // private List<PlayerController> relevantPlayersCache = new List<PlayerController>();
        private MatchData match;
        public MatchData Match => match;
        [SerializeField] private NetworkMatch networkMatch;
        public PlayerController[] playerControllers;
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

                spawner = Instantiate(spawnerPrefab);
                spawner.Initialise(networkMatch.matchId);

                Transform kevinSpawnPoint = GameSceneManager.GetReferences().kevinSpawnPoint;

                kevin = Instantiate(kevinPrefab, kevinSpawnPoint.position, kevinSpawnPoint.rotation);
                kevin.GetComponent<NetworkMatch>().matchId = networkMatch.matchId;
                NetworkServer.Spawn(kevin.gameObject);
                kevin.Initialise(this);

                Server_StartLobby();
            }
            else
            {
                //NOTE: We assume that players can only join during lobby phase
                countdown.Client_Initialise();
                lobbyUI = GameSceneManager.GetReferences().lobbyUI;
                Client_StartLobby();
                Cmd_SendMatchDescription();
            }

            initialised = true;
        }

        public void SetMatch(MatchData match)
        {
            this.match = match;
            networkMatch.matchId = match.id.ToGuid();
        }

        public void RegisterPlayer(Player player)
        {
            match.players.Add(player);

            PlayerController playerController = (TagNetworkManager.Instance.CreatePlayerController(player.gameObject));
            playerController.GetComponent<NetworkMatch>().matchId = this.GetComponent<NetworkMatch>().matchId;
            playerController.SetPlayer(player);
            player.playerController = playerController;
            RebuildPlayerControllersArray();
            BroadcastMatchDescriptionToAllPlayers();
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

                BroadcastMatchDescriptionToAllPlayers();
            }
        }

        private void BroadcastMatchDescriptionToAllPlayers()
        {
            if(state != GameStates.Lobby)
            {
                return;
            }
            MatchData.Description description = match.GetDescription();
            Rpc_UpdateMatchDescription(description);
           /* int playerCount = match.players.Count;
            for (int i = 0; i < playerCount; i++)
            {
                Player player = match.players[i];
                //player.TargetRpc_UpdateMatchDescription(description, (player == match.host));
            }*/
        }

        [Command(requiresAuthority = false)]
        private void Cmd_SendMatchDescription(NetworkConnectionToClient conn = null)
        {
            MatchData.Description description = match.GetDescription();
            TargetRpc_UpdateMatchDescription(conn,description);
        }

        [ClientRpc]
        public void Rpc_UpdateMatchDescription(MatchData.Description description)
        {
            if (initialised) 
            {
                lobbyUI.UpdateMatchDescription(description);
            }
        }

        [TargetRpc]
        public void TargetRpc_UpdateMatchDescription(NetworkConnection target, MatchData.Description description)
        {
            lobbyUI.UpdateMatchDescription(description);
        }

        [Server]
        private void Server_StartLobby()
        {
            state = GameStates.Lobby;
            match.states |= MatchData.StateFlags.Lobby;

            spawner.KillAll();
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
            BroadcastMatchDescriptionToAllPlayers();

            kevin.StartRoutine(Kevin.KevinStates.Idle);

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
            bool canStart =
                (caller == match.host && state == GameStates.Lobby && match.players.Count >= MatchSettings.MIN_PLAYER_COUNT);
            if (canStart)
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

            int playerCount = playerControllers.Length;
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
                PlayerController playerController = playerControllers[i];
                if(playerController == null)
                {
                    Debug.LogError("playerController is null!");
                }
                playerController.TargetRpc_Teleport(circleSpawnPoints[i].position, circleSpawnPoints[i].rotation);
            }
            //TODO: Initial pickups is to be expanded!!
            kevin.SpawnInitialPickups(match.settings.GetInitialPickupsCount());

            yield return new WaitForSeconds(2f);//Hardcoded

            //Spinning sequence
            yield return kevin.SpinCoroutine(circleSpawnPoints[taggerIndex].position);
            yield return new WaitForSeconds(0.2f);//Hardcoded

            for (int i = 0; i < playerCount; i++)
            {
                PlayerController player = playerControllers[i];
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
            kevin.StartRoutine(Kevin.KevinStates.DroppingItems);
            countdown.Server_StartCounting(match.settings.GetCountdown());
            Rpc_StartTagGame();
        }

        [ClientRpc]
        private void Rpc_StartTagGame()
        {
            SoundManager.PlayMusicTrack(SoundNames.GameMusic);
        }

        [Server]
        public void OnPlayerDied()
        {
            OnPlayerOutOfTheGame();
        }

        [Server]
        private void OnPlayerOutOfTheGame()
        {
            if (State == GameStates.TagGame)
            {
                //Filter players for those who are alive:
                List<PlayerController> livingPlayers = new List<PlayerController>();
                for (int i = 0; i < playerControllers.Length; i++)
                {
                    PlayerController playerController = playerControllers[i];
                    if (playerController.IsAlive())
                    {
                        livingPlayers.Add(playerController);
                    }
                }

                int livingPlayersCount = livingPlayers.Count;
                if (livingPlayersCount == 0)
                {
                    Debug.LogError("livingPlayersCount == 0");
                }
                else if (livingPlayersCount == 1)
                {
                    countdown.Server_StopCounting();
                    EndGame(livingPlayers[0]);
                }
                else
                {
                    //Let's check wheather a tagger exists in the game. if not, promote the player with the highest HP.

                    PlayerController nextTagger = livingPlayers[0];
                    for (int i = 0; i < livingPlayersCount; i++)
                    {
                        PlayerController player = livingPlayers[i];

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
        private void RebuildPlayerControllersArray()
        {
            int count = match.players.Count;
            playerControllers = new PlayerController[count];

            for (int i = 0; i < count; i++)
            {
                PlayerController playerController = match.players[i].playerController;
                if(playerController == null)
                {
                    Debug.LogError("playerController == null!");
                }
                else //if (/*playerController.Initialised && */playerController.IsAlive())
                {
                    playerControllers[i] = playerController;
                }            
            }
        }

        [Server]
        public void OnCountdownStopped()
        {      
            PlayerController winner = playerControllers[0];
            for (int i = 0; i < playerControllers.Length; i++)
            {
                PlayerController player = playerControllers[i];
                //TODO: But what about the rare occasion where the highest health value is shared among more than a single player 
                if (player.Health > winner.Health)
                {
                    winner = player;
                }
            }
            EndGame(winner);
        }

        [Server]
        private void EndGame(PlayerController winner)
        {
            Debug.Log(winner.name + "WON THE MATCH!");
            winner.Win();

            int playerCount = playerControllers.Length;
            for (int i = 0; i < playerCount; i++)
            {
                PlayerController player = playerControllers[i];
                if (player != winner && player.IsAlive())
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

        public void OnPlayerLeftMatch(Player player)
        {
            match.players.Remove(player);
            //NOTE: Mirror seems to delete our player controller automattically..
            //TODO: Mirror gives an error whenm we do it once the app is closed. 
            NetworkServer.Destroy(player.playerController.gameObject);

            RebuildPlayerControllersArray();
            Debug.Log($"Player disconnected from match {match.id} | {match.players.Count} players remaining");

            if (match.players.Count == 0)
            {
                Debug.Log($"<color=yellow>No players in Match: {match.id}. Terminating.</color>");
                MatchMaker.instance.TerminateMatch(match);
            }
            else
            {
                if (match.host == player)
                {
                    Player newHost = match.players[0];
                    match.host = newHost;
                }
                BroadcastMatchDescriptionToAllPlayers();
                OnPlayerOutOfTheGame();
            }
        }

        [Server]
        public void Terminate()
        {
            NetworkServer.Destroy(kevin.gameObject);
            spawner.Terminate();
            NetworkServer.Destroy(this.gameObject);
        }
    }
}
