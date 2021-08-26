using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HashtagChampion
{
    public class HostUI : MonoBehaviour
    {
        [SerializeField] private GameObject UIElements;
        [SerializeField] private TMPro.TextMeshProUGUI matchAccessibilityButtonText;
        public static HostUI instance;

        private void Start()
        {
            instance = this;
            ShowUI(false);
        }

        public void SwitchMatchAccessibility()
        {
            Player.localPlayer.SwitchMatchAccessibility();
        }

        public void UpdateMatchAccessibilityText(MatchData.StateFlags state)
        {
            matchAccessibilityButtonText.text =
                ((state & MatchData.StateFlags.Public) != 0) ? "Public" : "Private";
        }

        public void ShowUI(bool value)
        {
            UIElements.SetActive(value);
        }
    }
}

