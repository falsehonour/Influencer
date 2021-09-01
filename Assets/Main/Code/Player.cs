using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace HashtagChampion
{
    public class Player : NetworkBehaviour
    {
        private const string NO_MATCH_ID = "NO_MATCH_ID";
        public static Player localPlayer;
        //[SyncVar] public string matchID;
        private NetworkMatch networkMatch;
        private MatchData currentMatchData = null;
        public MatchData CurrentMatchData => currentMatchData;
        public PlayerController playerController;

        private void Start()
        {
            if (isLocalPlayer)
            {
                localPlayer = this;
            }
            networkMatch = GetComponent<NetworkMatch>();
            if (isServer)
            {
                networkMatch.matchId = NO_MATCH_ID.ToGuid();
            }
        }

        #region Host Match:

        public void HostMatch()
        {
            Cmd_HostMatch();
        }

        [Command]
        private void Cmd_HostMatch()
        {
            MatchData match = MatchMaker.instance.HostMatch(this, false,MatchMaker.GetDefaultMatchSettings);
            if (match != null)
            {
                GoToMatch(match);
                /* networkMatch.matchId = match.id.ToGuid();
                 //matchID = match.id;
                // TargetRpc_HostMatch(true, match.id);
                 TargetRpc_GoToGameScene();*/
            }
            else
            {
                Debug.LogError("Failed to create match!");
                //  TargetRpc_HostMatch(false, null);

            }

        }

        /* [TargetRpc]
         private void TargetRpc_HostMatch(bool success, string matchID)
         {
             Debug.Log($"matchID: {this.matchID} ==  + { matchID}");
         }*/
        #endregion
        #region Join Specific Match:

        public void JoinSpecificMatch(string matchID)
        {
            Cmd_JoinSpecificMatch(matchID);
        }

        [Command]
        private void Cmd_JoinSpecificMatch(string matchID)
        {
            MatchData match = MatchMaker.instance.JoinSpecificMatch(matchID, this);
            if (match != null)
            {
                // this.matchID = matchID;
                GoToMatch(match);
                //TargetRpc_JoinMatch(true, matchID);
                // TargetRpc_GoToGameScene();
                Debug.Log("joined match: " + matchID);

            }
            else
            {
                Debug.LogError("Failed to join match: " + matchID);
                // TargetRpc_JoinMatch(false, matchID);

            }

        }

        /*  [TargetRpc]
          private void TargetRpc_JoinMatch(bool success, string matchID)
          {
              Debug.Log($"matchID: {this.matchID} ==  + { matchID}");
          }*/
        #endregion

        #region Join Some Match:

        public void JoinSomeMatch()
        {
            Cmd_JoinSomeMatch();
        }

        [Command]
        private void Cmd_JoinSomeMatch()
        {
            MatchData match = MatchMaker.instance.JoinSomeMatch(this);
            if (match != null)
            {
                // this.matchID = matchID;
                GoToMatch(match);
                //TargetRpc_JoinMatch(true, matchID);
                // TargetRpc_GoToGameScene();
                Debug.Log("joined some match: " + match.id);

            }
            else
            {
                Debug.LogError("Failed to join some match");
                // TargetRpc_JoinMatch(false, matchID);
            }
        }

        /*  [TargetRpc]
          private void TargetRpc_JoinMatch(bool success, string matchID)
          {
              Debug.Log($"matchID: {this.matchID} ==  + { matchID}");
          }*/
        #endregion

        /*#region Start Match:

        public void StartMatch()
        {
            Cmd_StartMatch();
        }

        [Command]
        private void Cmd_StartMatch()
        {
            MatchMaker.StartMatch(matchID );
            Debug.Log("Cmd_StartMatch");

        }

        public void StartGame()
        {
            TargetRpc_StartMatch();
        }

        [TargetRpc]
        private void TargetRpc_StartMatch()
        {
            TargetRpc_GoToGameScene();
        }
        #endregion*/

        [Server]
        private void GoToMatch(MatchData match)
        {
            currentMatchData = match;
            networkMatch.matchId = match.id.ToGuid();
            TargetRpc_GoToMatch();

            //NetworkIdentity networkIdentity = other.gameObject.GetComponent<NetworkIdentity>();
            //TODO: Add unload on disconnect
            SceneMessage message = new SceneMessage 
               { sceneName = TagNetworkManager.Instance.gameScene, sceneOperation = SceneOperation.LoadAdditive };
            netIdentity.connectionToClient.Send(message);

            playerController = (TagNetworkManager.Instance.CreatePlayerController(gameObject));
            playerController.GetComponent<NetworkMatch>().matchId = this.networkMatch.matchId;
            playerController.SetPlayer(this);
            //NetworkServer.Spawn(playerController.gameObject,this.gameObject);
            
        }

        [TargetRpc]
        private void TargetRpc_GoToMatch()
        {
            /*SceneManager.LoadScene(2, LoadSceneMode.Additive);
            MatchMakingUI.instance.ShowUI(false);*/
            SceneSwitcher.instance.GoToGame();
        }

        #region Host Privilige:

        public void SwitchMatchAccessibility()
        {
            Cmd_SwitchMatchAccessibility();
        }

        [Command]
        private void Cmd_SwitchMatchAccessibility()
        {
            if (currentMatchData != null)
            {
                TargetRpc_OnMatchAccessibilitySet(MatchMaker.instance.SwitchMatchAccessibility(currentMatchData, this));
            }
            else
            {
                Debug.LogError("Failed to create match!");
                //  TargetRpc_HostMatch(false, null);

            }
        }

        [TargetRpc]
        private void TargetRpc_OnMatchAccessibilitySet(MatchData.StateFlags matchStateFlags)
        {
            HostUI.instance.UpdateMatchAccessibilityText(matchStateFlags);
        }

        [TargetRpc]
        public void TargetRpc_OnBecomeHost(MatchData.StateFlags matchStateFlags)
        {
            return;
            HostUI.instance.ShowUI(true);
            HostUI.instance.UpdateMatchAccessibilityText(matchStateFlags);
        }
        #endregion

        #region Disconnect:
        public void LeaveMatch()
        {
            Cmd_LeaveMatch();
        }

        [Command]
        private void Cmd_LeaveMatch()
        {
            Server_LeaveMatch();
        }

        [Server]
        public void Server_LeaveMatch()
        {
            if (currentMatchData != null)
            {
                MatchMaker.instance.OnPlayerLeftMatch(this, currentMatchData.id);
            }

            currentMatchData = null;
            networkMatch.matchId = NO_MATCH_ID.ToGuid();

            SceneMessage message = new SceneMessage 
               { sceneName = TagNetworkManager.Instance.gameScene, sceneOperation = SceneOperation.UnloadAdditive };
            netIdentity.connectionToClient.Send(message);

            Rpc_OnLeaveMatch();
            TargetRpc_OnLeaveMatch();

            //NOTE: Mirror seems to delete our player controller automattically..
            if(playerController != null)
            {
                //TODO: Mirror gives an error whenm we do it once the app is closed. 
                NetworkServer.Destroy(playerController.gameObject);
            }
        }

        [ClientRpc]
        private void Rpc_OnLeaveMatch()
        {
            //ClientLeaveMatch();
        }

        [TargetRpc]
        private void TargetRpc_OnLeaveMatch()
        {
            SceneSwitcher.instance.GoToMainMenu();
        }

        private void OnDestroy()
        {
            if (isServer)
            {
                Server_LeaveMatch();
            }
        }
        #endregion
    }

}
