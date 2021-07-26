[System.Serializable]
public class PlayerSettings : ISavable
{
    public bool fixedJoystick;
    public bool playSounds;
    public bool vibrate;

    public PlayerSettings()
    {
        //Default values:
        fixedJoystick = true;
        playSounds = true;
        vibrate = true;
    }

    public PlayerSettings(PlayerSettings copy)
    {
        //Default values:
        fixedJoystick = copy.fixedJoystick;
        playSounds = copy.playSounds;
        vibrate = copy.vibrate;
    }

    public string GetSaveFileName()
    {
        return "player_settings";
    }
}
