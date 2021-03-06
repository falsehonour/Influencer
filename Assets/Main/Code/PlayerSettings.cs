
[System.Serializable]
public class PlayerSettings : ISavable
{
    public JoystickTypes joystickType;
    public OnOffSwitch sfx;
    public OnOffSwitch music;
    public OnOffSwitch vibration;

    public PlayerSettings()
    {
        SetDefaultValues();
    }

    public void SetDefaultValues()
    {
        //Default values:
        joystickType = JoystickTypes.Fixed;
        sfx = OnOffSwitch.On;
        music = OnOffSwitch.On;
        vibration = OnOffSwitch.On;
    }

    public static void Copy(PlayerSettings from, PlayerSettings to)
    {
        to.joystickType = from.joystickType;
        to.sfx = from.sfx;
        to.music = from.music;
        to.vibration = from.vibration;
    }

    /* public string GetSaveFileName()
     {
         return "player_settings";
     }*/
}

public enum OnOffSwitch : sbyte
{
   Off = 0, On = 1,
}

public enum JoystickTypes : sbyte
{
    Fixed = 0, Dynamic = 1,
}

