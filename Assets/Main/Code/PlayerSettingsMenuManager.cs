using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettingsMenuManager : MenuManager
{
    private PlayerSettings modifiedSettings;
    //TODO: Should we generate these in code?
    [SerializeField] private UISettingsToggle fixedJoystickToggle;

    public override void Activate()
    {
        base.Activate();
        PlayerSettings currentSettings = StaticData.playerSettings;
        modifiedSettings = new PlayerSettings(currentSettings);
        Initialise();
    }

    private void Initialise()
    {


        fixedJoystickToggle.Initialise(SetFixedJoystick, modifiedSettings.fixedJoystick);

    }

    private void SetFixedJoystick(bool value)
    {
        modifiedSettings.fixedJoystick = value;
    }

    public void ApplyAndSaveSettings()
    {
        StaticData.playerSettings = modifiedSettings;
        SaveAndLoadManager.Save<PlayerSettings>(modifiedSettings);
    }

    public void ResetToDefault()
    {
        modifiedSettings = new PlayerSettings();
        Initialise();
    }
}
