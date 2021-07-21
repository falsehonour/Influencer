using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSettingsUI : MonoBehaviour
{

    private RoomManager roomManager;
    //TODO: Should we generate these in code?
    [SerializeField] private RoomSettingsUISlider playerCountSlider;
    [SerializeField] private RoomSettingsUISlider countdownSlider;
    [SerializeField] private RoomSettingsUISlider initialPickupsSlider;
    [SerializeField] private RoomSettingsUISlider taggerSpeedBoostSlider;
    [SerializeField] private RoomSettingsUISlider playerRotationSpeedSlider;

    private void Start()
    {
        roomManager = HashtagChampion.TagNetworkManager.instance.roomManager;
//TODO: Looks like repeating buisness to me
//TODO: This class should not dictate min and max values for room settings.  
playerCountSlider.Initialise(SetPlayerCount, roomManager.settings.playerCount, 1, 8, 1);
        initialPickupsSlider.Initialise(SetInitialPickups, roomManager.settings.initialPickups, 0, 32, 1);
        //(delegate { roomManager.settings.playerCount = (byte)playerCountSlider.slider.value; });
        countdownSlider.Initialise(SetCountdown, roomManager.settings.countdown, 30, 420, 1);
        // (delegate { roomManager.settings.countdown =  countdownSlider.slider.value; });
        // { roomManager.settings.playerCount = (byte)playerCountSlider.slider.value; };
        taggerSpeedBoostSlider.Initialise(SetTaggerSpeedBoost, roomManager.settings.taggerSpeedBoostInKilometresPerHour, 0, 2, 0.1f);
        playerRotationSpeedSlider.Initialise(SetPlayerRotationSpeed, roomManager.settings.playerRotationSpeed, 90, (90 * 24), 90);
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

    private void SetTaggerSpeedBoost(float value)
    {
        roomManager.settings.taggerSpeedBoostInKilometresPerHour = value;
    }

    private void SetPlayerRotationSpeed(float value)
    {
        roomManager.settings.playerRotationSpeed = value;
    }
}
