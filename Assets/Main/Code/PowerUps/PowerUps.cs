[System.Serializable]
public struct PowerUp
{
    public enum Type : byte
    {
        //TODO:maybe get rid of None and equate it with any powerup with an amount of zero
        //TODO: Maybe add all collectibles to avoid the health boost seperation from the rest
        None = 0, Gun = 1, Football = 2, Banana = 3, Sprint = 4, Length = 5
    }

    public sbyte count;
    public Type type;

}