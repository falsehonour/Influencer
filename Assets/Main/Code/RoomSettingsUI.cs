using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSettingsUI : MonoBehaviour
{

    [SerializeField] private RoomManager roomManager;
    //TODO: Should we generate these in code?
    [SerializeField] private RoomSettingsUISlider playerCountSlider;
    [SerializeField] private RoomSettingsUISlider countdownSlider;
    [SerializeField] private RoomSettingsUISlider initialPickupsSlider;

    private void Awake()
    {
        //TODO: Looks like repeating buisness to me
        playerCountSlider.SetValue(roomManager.settings.playerCount,false);
        playerCountSlider.OnValueChangedAction += SetPlayerCount;

        initialPickupsSlider.SetValue(roomManager.settings.initialPickups, false);
        initialPickupsSlider.OnValueChangedAction += SetInitialPickups;

        //(delegate { roomManager.settings.playerCount = (byte)playerCountSlider.slider.value; });
        countdownSlider.SetValue(roomManager.settings.countdown, false);
        countdownSlider.OnValueChangedAction += SetCountdown;
        // (delegate { roomManager.settings.countdown =  countdownSlider.slider.value; });
        // { roomManager.settings.playerCount = (byte)playerCountSlider.slider.value; };
    }

    //TODO: Check for value legitimacy (maybe in RoomManager instead of here)
    private void SetPlayerCount(float value)
    {
        roomManager.settings.playerCount = (byte)value;
    }

    private void SetCountdown(float value)
    {
        roomManager.settings.countdown = value;
    }

    private void SetInitialPickups(float value)
    {
        roomManager.settings.initialPickups = (byte)value;
    }
}
