using UnityEngine;

public static class StaticData 
{
    public static PlayerSettings playerSettings;
    public static PlayerName playerName;
    private static bool initialised = false;

    public static void Initialise()
    {
        if (initialised)
        {
            Debug.Log("StaticData had already initialised and will not do so more than once.");
        }
        else
        {
            PlayerSettings loadedPlayerSettings = SaveAndLoadManager.TryLoad<PlayerSettings>();
            if(loadedPlayerSettings != null)
            {
                playerSettings = loadedPlayerSettings;
                Debug.Log("PlayerSettings loaded.");
            }
            else
            {
                playerSettings = new PlayerSettings();
                Debug.Log("Failed to load PlayerSettings. a new PlayerSettings will be used.");
            }

            PlayerName loadedPlayerName = SaveAndLoadManager.TryLoad<PlayerName>();
            if (loadedPlayerName != null)
            {
                playerName = loadedPlayerName;
                Debug.Log("playerName loaded.");
            }
            else
            {
                playerName = new PlayerName();
                Debug.Log("Failed to load playerName. a new playerName will be used.");
            }

            initialised = true;
            Debug.Log("StaticData just initialised.");

        }
    }

}
