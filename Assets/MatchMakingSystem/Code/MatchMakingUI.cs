using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HashtagChampion
{
    public class MatchMakingUI : MonoBehaviour
    {
        [SerializeField] private GameObject UIElements;
        [SerializeField] private InputField joinGameInputField;
        public static MatchMakingUI instance;

        private void Start()
        {
            instance = this;
        }

        public void Host()
        {
            //MatchMaker.instance.HostMatch();
            Player.localPlayer.HostMatch();
        }

        public void JoinSpecificMatch()
        {
            Player.localPlayer.JoinSpecificMatch(joinGameInputField.text.ToUpper());
        }

        public void JoinSomeMatch()
        {
            Player.localPlayer.JoinSomeMatch();
        }

        public void ShowUI(bool value)
        {
            UIElements.SetActive(value);
        }
    }
}


