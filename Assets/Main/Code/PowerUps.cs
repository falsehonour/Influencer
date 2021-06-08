[System.Serializable]
public struct PowerUp
{
    public enum Type : byte
    {
        //TODO:maybe get rid of None and equate it with any powerup with an amount of zero
        None = 0, Nerf = 1, Football = 2, Banana = 3, Length = 4
    }
    public sbyte count;
    public Type type;


}