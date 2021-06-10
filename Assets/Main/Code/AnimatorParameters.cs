using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class AnimatorParameters
{
    public static readonly int Speed = Animator.StringToHash(nameof(Speed));
    public static readonly int Push = Animator.StringToHash(nameof(Push));
    public static readonly int FlipForward = Animator.StringToHash(nameof(FlipForward));
    public static readonly int Slap = Animator.StringToHash(nameof(Slap));
    public static readonly int Recover = Animator.StringToHash(nameof(Recover));
    public static readonly int Lose = Animator.StringToHash(nameof(Lose));
    public static readonly int Win = Animator.StringToHash(nameof(Win));
    public static readonly int ThrowForward = Animator.StringToHash(nameof(ThrowForward));
    public static readonly int ThrowBackward = Animator.StringToHash(nameof(ThrowBackward));
    public static readonly int SpeedState = Animator.StringToHash(nameof(SpeedState));
    public static readonly int State = Animator.StringToHash(nameof(State));



    /* static AnimatorParameters()
     {
         //TODO: Is there a way to throw the fields in a collection and itterate over them..?
        // InitialiseParameter(ref Speed);
         InitialiseParameter(ref Push);
         InitialiseParameter(ref FlipForward);
         InitialiseParameter(ref Slap);

     }*/

    /*private static void InitialiseParameter(ref int param)
    {
        string paramName = nameof(param);
        Debug.Log(paramName);
        param = Animator.StringToHash(paramName);
    }*/

    /* public AnimatorParameters()
     {
         //TODO: Is there a way to throw the fields in a collection and itterate over them..?
         InitialiseParameter(ref Speed);
         InitialiseParameter(ref Push);
         InitialiseParameter(ref FlipForward);
     }*/

    /* private static void InitialiseParameter(ref int param)
     {
         param = Animator.StringToHash(nameof(param));
    */
}
