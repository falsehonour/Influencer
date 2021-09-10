[System.Serializable]
public struct MatchSettings 
{
    public const byte MIN_PLAYER_COUNT = 2;

    ///<summary>The number of players that compel the server to start a match</summary>
    public byte maxPlayerCount;
    ///<summary>The time a match will last in seconds</summary>
    public float countdown;
    public byte initialPickups;
    public float taggerSpeedBoostInKilometresPerHour;
}
