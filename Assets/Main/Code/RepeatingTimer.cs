
public struct RepeatingTimer
{
    public float timeLeft;
    public readonly float interval;

    public RepeatingTimer(float interval)
    {
        this.interval = interval;
        timeLeft = interval;
    }

    public void Reset()
    {
        timeLeft = interval;
    }

    public bool Update(float timePassed)
    {
        //TODO: Can we trust users to pass the time that passed?
        timeLeft -= timePassed;
        if (timeLeft < 0)
        {
            Reset();
            return true;
        }
        else
        {
            return false;
        }
    }
}
