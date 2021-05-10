using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MatchCountdown : NetworkBehaviour
{
    //TODO: Perhaps seperate into 2 classed, one for server and one for client graphics??? 
    [SerializeField] private TMPro.TextMeshProUGUI text;
    //private bool isActive;
    //private UInt16 previousTimeLeftUInt16;

    [Server]
    public void StartCounting(float time)
    {
       /* timeLeft = time;
        previousTimeLeftUInt16 = 0;*/
        StartCoroutine(CountdownRoutine(time));
       // isActive = true;
    }

    private IEnumerator CountdownRoutine(float time)
    {
        UInt16 previousTimeLeftUInt16 = 0;
        float timeLeft = time;

        while (timeLeft > 0)
        {
            //TODO: will it be more efficient to wait for seconds..?
            timeLeft -= Time.deltaTime;
            UInt16 currentTimeLeftUInt16 = (UInt16)timeLeft;
            if (currentTimeLeftUInt16 != previousTimeLeftUInt16)
            {
                Rpc_UpdateText(currentTimeLeftUInt16);
            }
            previousTimeLeftUInt16 = currentTimeLeftUInt16;
            yield return null;
        }

        GameManager.OnCountdownStopped();

    }

    [ClientRpc]
    private void Rpc_UpdateText(UInt16 timeLeft)
    {
        text.text = timeLeft.ToString();
    }

    /*[Server]
private void StopCounting()
{
    isActive = false;
    GameManager.OnCounterStopped();
}*/
}