using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomSettingsUI : MonoBehaviour
{

    private MatchSettings matchSettings;
    //TODO: Should we generate these in code?
    [SerializeField] private UISettingsSlider playerCountSlider;
    [SerializeField] private UISettingsSlider countdownSlider;
    [SerializeField] private UISettingsSlider initialPickupsSlider;
    [SerializeField] private UISettingsSlider taggerSpeedBoostSlider;

    private void Start()
    {
        Debug.LogWarning("RoomSettingsUI will not work properly.");
        //roomManager = HashtagChampion.TagNetworkManager.RoomManager;
        //TODO: Looks like repeating buisness to me
        //TODO: This class should not dictate min and max values for room settings.  
        playerCountSlider.Initialise(SetPlayerCount, matchSettings.playerCount, 1, 8, 1);
        initialPickupsSlider.Initialise(SetInitialPickups, matchSettings.initialPickups, 0, 32, 1);
        //(delegate { roomManager.settings.playerCount = (byte)playerCountSlider.slider.value; });
        countdownSlider.Initialise(SetCountdown, matchSettings.countdown, 30, 420, 1);
        // (delegate { roomManager.settings.countdown =  countdownSlider.slider.value; });
        // { roomManager.settings.playerCount = (byte)playerCountSlider.slider.value; };
        taggerSpeedBoostSlider.Initialise(SetTaggerSpeedBoost, matchSettings.taggerSpeedBoostInKilometresPerHour, 0, 2, 0.1f);
    }

    //TODO: Check for value legitimacy (maybe in RoomManager instead of here)
    private void SetPlayerCount(float value)
    {
        matchSettings.playerCount = (byte)value;
    }

    private void SetCountdown(float value)
    {
        matchSettings.countdown = value;
    }

    private void SetInitialPickups(float value)
    {
        matchSettings.initialPickups = (byte)value;
    }

    private void SetTaggerSpeedBoost(float value)
    {
        matchSettings.taggerSpeedBoostInKilometresPerHour = value;
    }
}
