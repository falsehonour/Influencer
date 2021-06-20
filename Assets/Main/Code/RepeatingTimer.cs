
using System;

public struct RepeatingTimer
{
    private float timeLeft;
    private readonly float interval;

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

    public void AddToCurrentIteration(float addition)
    {
        if(addition < 0)
        {
            UnityEngine.Debug.LogWarning("AddToCurrentIteration is meant for positive additions only. Bye!");
            return;
        }
        timeLeft += addition;
    }
}

public struct SingleCycleTimer
{
    private float timeLeft;
    public float TimeLeft => timeLeft;
    //private bool isActive;
    public bool IsActive
    {
       get { return timeLeft > 0; }
    }

    public void Start(float time)
    {
        timeLeft = time;
        //isActive = true;
    }

    public bool Update(float timePassed)
    {
        //TODO: Can we trust users to pass the time that passed?
        timeLeft -= timePassed;
        if (timeLeft < 0)
        {
            //isActive = false;
            return true;
        }
        else
        {
            return false;
        }
    }
}