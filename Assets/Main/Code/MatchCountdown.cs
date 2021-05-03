using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class MatchCountdown : NetworkBehaviour
{
    //TODO: Perhaps seperate into 2 classed, one for server and one for client graphics??? 
    [SerializeField] private TMPro.TextMeshProUGUI text;
    private float timeLeft;
    private bool  isActive;
    private UInt16 previousTimeLeftUInt16;

    public void StartCounting(float time)
    {
        timeLeft = time;
        previousTimeLeftUInt16 = 0;
        isActive = true;
    }

    [Server]
    private void StopCounting()
    {
        isActive = false;
        GameManager.OnCounterStopped();
    }

    [Server]
    void Update()
    {
        if (isActive)
        {
            timeLeft -= Time.deltaTime;
            UInt16 currentTimeLeftUInt16 = (UInt16)timeLeft;
            if (currentTimeLeftUInt16 != previousTimeLeftUInt16)
            {
                Rpc_UpdateText(currentTimeLeftUInt16);
            }
            previousTimeLeftUInt16 = currentTimeLeftUInt16;
            if(timeLeft < 0)
            {
                StopCounting();
            }
        }
    }

    [ClientRpc]
    private void Rpc_UpdateText(UInt16 timeLeft)
    {
        text.text = timeLeft.ToString();
    }
}