using Name = System.ComponentModel.DescriptionAttribute;

[System.Serializable]
public struct MatchSettings 
{
    public const byte MIN_PLAYER_COUNT = 2;

    ///<summary>The number of players that compel the server to start a match</summary>
    public MaxPlayerCount maxPlayerCount;
    ///<summary>The time a match will last in seconds</summary>
    public CountdownValues countdown;
    public InitialPickups initialPickups;
    public TaggerSpeedValues taggerSpeed;

   /* public MatchSettings()
    {
        SetDefaultValues();
    }*/

    public void SetDefaultValues()
    {
        //Default values:
        maxPlayerCount = MaxPlayerCount._3;
        countdown = CountdownValues._45;
        initialPickups = InitialPickups.Normal;
        taggerSpeed = TaggerSpeedValues.Normal;
    }

    public byte GetMaxPlayerCount()
    {
        return (byte.Parse(maxPlayerCount.GetDescription()));
    }

    public int GetInitialPickupsCount()
    {
        //TODO: Initial pickups is to be expanded!!
        return ((int)initialPickups * 3);
    }

    public float GetTaggerSpeedMultiplier()
    {
        return (((float)taggerSpeed / 6f) + 1f);
    }

    public int GetCountdown()
    {
        //TODO: Initial pickups is to be expanded!!
        return (int.Parse(countdown.GetDescription()));
    }
}

public enum MaxPlayerCount : sbyte
{
    [Name("3")] _3 = 3, [Name("4")] _4 = 4, [Name("5")] _5 = 5,
    [Name("6")] _6 = 6, [Name("7")] _7 = 7, [Name("8")] _8 = 8
}

public enum CountdownValues : sbyte
{
    [Name("45")] _45 = 0, [Name("60")] _60 = 1,
    [Name("100")] _100 = 2, [Name("120")] _120 = 3, [Name("150")] _150 = 4,
    [Name("180")] _180 = 5
}

public enum InitialPickups : sbyte
{
    None = 0, Few = 1, Normal = 2, Plenty = 3,
}

public enum TaggerSpeedValues : sbyte
{
    Normal = 0, Fast = 1, [Name("Very Fast")] Max = 2
}