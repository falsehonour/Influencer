using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

namespace HashtagChampion
{
    public class JoinGameMenuManager : MenuManager
    {

        [SerializeField] private TMP_InputField joinGameInputField;
        [SerializeField] private TextMeshProUGUI joinGameMessageText;

        public void JoinSpecificMatch()
        {
            Player.localPlayer.JoinSpecificMatch(joinGameInputField.text.ToUpper());
        }

        private void Start()
        {
            NetworkClient.RegisterHandler<JoinGameMessage>(RecieveMessage);
        }

        public override void Activate()
        {
            base.Activate();
            joinGameInputField.text = "";
            joinGameMessageText.text = ""; 
        }

        public void RecieveMessage(JoinGameMessage message)
        {
            string messageText = string.Empty;
            switch (message.messageType)
            {
                case JoinGameMessageTypes.MatchDoesNotExist:
                    { messageText = "Room does not exist."; }
                    break;
                case JoinGameMessageTypes.MatchIsFull:
                    { messageText = "Room full."; }
                    break;
                case JoinGameMessageTypes.GameTakingPlace:
                    { messageText = "Cannot join while a game is taking place."; }
                    break;
                case JoinGameMessageTypes.Joining:
                    { messageText = "Joining!"; }
                    break;
            }
            joinGameMessageText.text = messageText;
        }
    }

    public struct JoinGameMessage : NetworkMessage
    {
        public JoinGameMessageTypes messageType;
    }

    public enum JoinGameMessageTypes : byte
    {
        MatchDoesNotExist, MatchIsFull, GameTakingPlace,
        Joining
    }

}
