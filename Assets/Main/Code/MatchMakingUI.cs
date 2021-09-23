using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HashtagChampion
{
    public class MatchMakingUI : MonoBehaviour
    {

        public static MatchMakingUI instance;

        private void Start()
        {
            instance = this;
        }

        public void JoinSomeMatch()
        {
            Player.localPlayer.JoinSomeMatch();
        }
    }
}


