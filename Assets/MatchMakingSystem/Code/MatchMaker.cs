using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HashtagChampion
{
    public class MatchMaker : MonoBehaviour
    {
        private const int MATCH_ID_DIGIT_COUNT = 6;
        public List<MatchData> matches = new List<MatchData>();
        public static MatchMaker instance;
        [SerializeField] private MatchManager matchGameManagerPrefab;
        [SerializeField] private MatchSettings defaultMatchSettings;
        public static MatchSettings GetDefaultMatchSettings => instance.defaultMatchSettings;

        private void Start()
        {
            //TODO: Destroy if this is not on the server?
            instance = this;
        }

        public MatchData HostMatch(Player host, bool publicMatch, MatchSettings settings)
        {
            string matchID = GenerateNewMatchID();
            if (matchID == null || host == null)
            {
                return null;
            }
            MatchManager matchGameManager = Instantiate(matchGameManagerPrefab);
            NetworkServer.Spawn(matchGameManager.gameObject);
            MatchData match = new MatchData(matchID, host, matchGameManager, settings);
            if (publicMatch)
            {
                match.states |= MatchData.StateFlags.Public;
            }
            matchGameManager.SetMatch(match);
            //host.TargetRpc_OnBecomeHost(match.states);
            instance.matches.Add(match);
            return match;
        }


        public MatchData JoinSpecificMatch(string matchID, Player player)
        {
            //bool joined = false;
            MatchData match = FindMatch(matchID);
            bool joined = false;
            JoinGameMessage message = new JoinGameMessage();

            if (match != null)
            {
                if ((match.states & MatchData.StateFlags.Lobby) == 0)
                {
                    message.messageType = JoinGameMessageTypes.GameTakingPlace;
                }
                else if (match.players.Count >= match.settings.maxPlayerCount)
                {
                    //Debug.Log("The match is not open for new players.");
                    message.messageType = JoinGameMessageTypes.MatchIsFull;
                }
                else
                {
                    message.messageType = JoinGameMessageTypes.Joining;
                    match.players.Add(player);
                    match.manager.BroadcastMatchDescriptionToAllPlayers();
                    joined = true;
                }
            }
            else
            {
                //Debug.Log("A match with a corresponding ID was not found on the server.");
                message.messageType = JoinGameMessageTypes.MatchDoesNotExist;
            }

            player.netIdentity.connectionToClient.Send<JoinGameMessage>(message);
            // obselete NetworkServer.SendToClientOfPlayer<JoinGameMessage>(player.netIdentity, message);
            return (joined ? match : null);
        }

        public MatchData JoinSomeMatch(Player player)
        {
            MatchData validMatch = null;
            int matchesCount = matches.Count;
            for (int i = 0; i < matchesCount; i++)
            {
                MatchData match = matches[i];
                //TODO: Add capacity condition
                bool canJoin =
                    ((match.states & MatchData.StateFlags.Public) != 0) &&
                    ((match.states & MatchData.StateFlags.Lobby)  != 0) &&
                    (match.players.Count < match.settings.maxPlayerCount);
                if (canJoin)
                {
                    validMatch = match;
                    break;
                }
#if UNITY_EDITOR
                else
                {
                    if((match.states & MatchData.StateFlags.Public) == 0)
                    {
                        Debug.Log("Match is private");
                    }
                    if ((match.states & MatchData.StateFlags.Lobby) == 0)
                    {
                        Debug.Log("Match is not in lobby mode");
                    }
                    if (!(match.players.Count < match.settings.maxPlayerCount))
                    {
                        Debug.Log("Match is full");
                    }
                }
#endif
            }

            if (validMatch != null)
            {
                validMatch.players.Add(player);
                validMatch.manager.BroadcastMatchDescriptionToAllPlayers();

            }
            else
            {
                validMatch = HostMatch(player, true, defaultMatchSettings);
            }
            return validMatch;
        }

        public void OnPlayerLeftMatch(Player player, string _matchID)
        {
            for (int i = 0; i < matches.Count; i++)
            {
                MatchData match = matches[i];
                if (match.id == _matchID)
                {

                    int playerIndex = match.players.IndexOf(player);
                    match.players.RemoveAt(playerIndex);
                    Debug.Log($"Player disconnected from match {_matchID} | {match.players.Count} players remaining");

                    if (match.players.Count == 0)
                    {
                        Debug.Log($"<color=yellow>No players in Match: {_matchID}. Terminating.</color>");
                        TerminateMatch(i);
                    }
                    else
                    {

                        if (match.host == player)
                        {
                            Player newHost = match.players[0];
                            match.host = newHost;
                           // newHost.TargetRpc_OnBecomeHost(match.states);
                        }
                        match.manager.BroadcastMatchDescriptionToAllPlayers();
                    }
                    break;
                }
            }
        }

        private void TerminateMatch(int matchIndex)
        {
            matches[matchIndex].manager.Terminate();
            matches.RemoveAt(matchIndex);
        }
        /* public static void StartMatch(string matchID)
         {
             GameManager gameManager = Instantiate(instance.gameManagerPrefab);
             gameManager.GetComponent<NetworkMatch>().matchId = matchID.ToGuid();
             NetworkServer.Spawn(gameManager.gameObject);
             Match match = FindMatch(matchID);
             if(match != null)
             {
                 foreach (Player player in match.players)
                 {
                     gameManager.AddPlayer(player);
                     player.StartGame();
                 }

             }
         }*/

        private static string GenerateNewMatchID()
        {
            int maxNumberOfTries = 256;
            int numberOfTries = 0;
            while (numberOfTries < maxNumberOfTries)
            {
                numberOfTries++;
                string id = string.Empty;
                for (int i = 0; i < MATCH_ID_DIGIT_COUNT; i++)
                {
                    int randomInt = Random.Range(0, 36);
                    if (randomInt < 26)
                    {
                        randomInt += 65;
                        id += (char)randomInt;
                    }
                    else
                    {
                        randomInt -= 26;
                        id += randomInt.ToString();
                    }

                }
                bool foundIdenticalID = (instance.FindMatch(id) != null);
                if (!foundIdenticalID)
                {
                    Debug.Log("Mew Match ID = " + id);
                    return id;
                }
            }
            Debug.LogError("numberOfTries was excceeded!");

            return null;

        }

        private MatchData FindMatch(string matchID)
        {
            MatchData match = null;
            int matchesCount = matches.Count;
            for (int i = 0; i < matchesCount; i++)
            {
                if (matches[i].id == matchID)
                {
                    match = matches[i];
                    break;
                }
            }
            return match;
        }
    }
}
