using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HashtagChampion
{
    public class LobbyUI : MonoBehaviour
    {
        [SerializeField] private GameObject UIElements;
        [SerializeField] private Button matchAccessibilityButton;
        [SerializeField] private Button startGameButton;
        [SerializeField] private TMPro.TextMeshProUGUI matchAccessibilityButtonText;
        [SerializeField] private TMPro.TextMeshProUGUI playerCountText;
        [SerializeField] private TMPro.TextMeshProUGUI matchAccessCodeText;


        public void SwitchMatchAccessibility()
        {
            Player.localPlayer.Client_SwitchMatchAccessibility();
        }

        public void StartGame()
        {
            Player.localPlayer.Client_StartGame();
        }

        public void ShowUI(bool value)
        {
            UIElements.SetActive(value);
        }

        public void UpdateMatchDescription(MatchData.Description description)
        {
            bool isHost = description.hostNetId == Player.localPlayer.netId;
            startGameButton.interactable = isHost && description.playerCount >= MatchSettings.MIN_PLAYER_COUNT;
            matchAccessibilityButton.interactable = isHost;
            matchAccessibilityButtonText.text =
               ((description.states & MatchData.StateFlags.Public) != 0) ? "Public" : "Private";
            playerCountText.text = description.playerCount.ToString() + "/" + description.maxPlayerCount.ToString();
            matchAccessCodeText.text = "Code: " + description.id;
        }
    }
}

