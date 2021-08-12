[System.Serializable]
public struct RoomSettings 
{
    ///<summary>The number of players that compel the server to start a match</summary>
    public byte playerCount;
    ///<summary>The time a match will last in seconds</summary>
    public float countdown;
    public byte initialPickups;
    public float taggerSpeedBoostInKilometresPerHour;
    public float playerRotationSpeed;
}
