using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettingsUI : MonoBehaviour
{
    private PlayerSettings settings;
    //TODO: Should we generate these in code?
    [SerializeField] private UISettingsToggle fixedJoystickToggle;


    private void OnEnable()
    {
        Initialise();
    }

    private void Initialise()
    {
        Debug.Log("PlayerSettingsUI: Initialise()");
        PlayerSettings currentSettings = StaticData.playerSettings;
        settings = new PlayerSettings(currentSettings);

        fixedJoystickToggle.Initialise(SetFixedJoystick, settings.fixedJoystick);

    }

    private void SetFixedJoystick(bool value)
    {
        settings.fixedJoystick = value;
    }

    public void ApplyAndSaveSettings()
    {
        StaticData.playerSettings = settings;
        SaveAndLoadManager.Save<PlayerSettings>(settings);
    }
}
