using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSettingsMenuManager : MenuManager
{
    private PlayerSettings modifiedSettings;
    //TODO: Should we generate these in code?
    [SerializeField] private UISettingsController fixedJoystickController;
    [SerializeField] private UISettingsController sfxController;
    [SerializeField] private UISettingsController musicController;
    [SerializeField] private UISettingsController vibrationController;


    private void Start()
    {
        Initialise();
    }

    public override void Activate()
    {
        base.Activate();
        PlayerSettings.Copy(StaticData.playerSettings, modifiedSettings);
        SetSettingsControllersValues();
    }

    private void Initialise()
    {
        modifiedSettings = new PlayerSettings();
        //TODO: There should be a wat to automate this

        fixedJoystickController.Initialise(
            (delegate(sbyte value)
            {
                modifiedSettings.joystickType = (JoystickTypes)value;
            })
            ,typeof(JoystickTypes));

        sfxController.Initialise(
            (delegate (sbyte value)
            {
                modifiedSettings.sfx = (OnOffSwitch)value;
            })
           , typeof(OnOffSwitch));

        musicController.Initialise(
           (delegate (sbyte value)
           {
               modifiedSettings.music = (OnOffSwitch)value;
           })
          , typeof(OnOffSwitch));

        vibrationController.Initialise(
           (delegate (sbyte value)
           {
               modifiedSettings.vibration = (OnOffSwitch)value;
           })
          , typeof(OnOffSwitch));

    }

    /*private System.Action<sbyte> CreateControllerAction(ref sbyte modified)
    {
        return
        (delegate (sbyte value)
        {
            modified = value;
        });
    }*/

    private void SetSettingsControllersValues()
    {
        fixedJoystickController.SetValue((sbyte)modifiedSettings.joystickType, false);
        sfxController.SetValue((sbyte)modifiedSettings.sfx, false);
        musicController.SetValue((sbyte)modifiedSettings.music, false);
        vibrationController.SetValue((sbyte)modifiedSettings.vibration, false);

    }

    public void ApplyAndSaveSettings()
    {
        //TODO: Apply
        PlayerSettings.Copy(modifiedSettings, StaticData.playerSettings);
        //TODO: Should we let StaticData do the saving?
        SaveAndLoadManager.Save<PlayerSettings>(modifiedSettings);

        //TODO: Move this away. also, u might wanna apply setting every time something is changed on the menu
        SoundManager.ConformToPlayerSettings();
    }

    public void ResetToDefault()
    {
        modifiedSettings.SetDefaultValues();
        SetSettingsControllersValues();
    }
}
