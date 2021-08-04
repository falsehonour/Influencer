[System.Serializable]
public class PlayerSettings : ISavable
{
    public JoystickTypes joystickType;
    public OnOffSwitch sfx;
    public OnOffSwitch music;
    public OnOffSwitch vibration;

    public PlayerSettings()
    {
        //Default values:
        joystickType = JoystickTypes.Fixed;
        sfx = OnOffSwitch.On;
        music = OnOffSwitch.On;
        vibration = OnOffSwitch.On;
    }

    public PlayerSettings(PlayerSettings copy)
    {
        //Default values:
        joystickType = copy.joystickType;
        sfx = copy.sfx;
        music = copy.music;
        vibration = copy.vibration;
    }

    public string GetSaveFileName()
    {
        return "player_settings";
    }
}

public enum OnOffSwitch : sbyte
{
    Min = 0, Off = 0, On = 1, Max = 1
}

public enum JoystickTypes : sbyte
{
    Min = 0, Fixed = 0, Dynamic = 1, Max = 1
}

