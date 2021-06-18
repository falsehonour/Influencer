using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSettingsUI : MonoBehaviour
{

    [SerializeField] private RoomManager roomManager;
    [SerializeField] private RoomSettingsUISlider playerCountSlider;
    [SerializeField] private RoomSettingsUISlider countdownSlider;

    private void Awake()
    {
        playerCountSlider.SetValue(roomManager.settings.playerCount,false);
        playerCountSlider.OnValueChangedAction += SetPlayerCount;
        //(delegate { roomManager.settings.playerCount = (byte)playerCountSlider.slider.value; });
        countdownSlider.SetValue(roomManager.settings.countdown, false);
        countdownSlider.OnValueChangedAction += SetCountdown;
        // (delegate { roomManager.settings.countdown =  countdownSlider.slider.value; });
        // { roomManager.settings.playerCount = (byte)playerCountSlider.slider.value; };
    }

    private void SetPlayerCount(float value)
    {
        roomManager.settings.playerCount = (byte)value;
    }
    private void SetCountdown(float value)
    {
        roomManager.settings.countdown = value;
    }
}
