using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class MatchCountdown : NetworkBehaviour
{
    //TODO: Perhaps seperate into 2 classed, one for server and one for client graphics??? 
    [SerializeField] private GameObject UIObject;
    [SerializeField] private TMPro.TextMeshProUGUI text;
    [SerializeField] private Image fillImage;

    private float initialTime;
    private float timeLeft;
    private Coroutine countRoutine;
    //private bool isActive;
    //private UInt16 previousTimeLeftUInt16;

    private void Start()
    {
        ShowGraphics(false);
    }

    [Server]
    public void Server_StartCounting(float time)
    {
        /* timeLeft = time;
         previousTimeLeftUInt16 = 0;*/
        countRoutine = StartCoroutine(CountdownRoutine(time));
        Rpc_StartCounting(time);
       // isActive = true;
    }

    [ClientRpc]
    private void Rpc_StartCounting(float time)
    {
        if (isClientOnly)
        {
            countRoutine = StartCoroutine(CountdownRoutine(time));
        }
    }

    private IEnumerator CountdownRoutine(float time)
    {
        ShowGraphics(true);

        UInt16 previousTimeLeftUInt16 = 0;
        initialTime = time;
        timeLeft = time;
        bool dedicatedServer = isServerOnly;
        while (timeLeft > 0)
        {
            //TODO: will it be more efficient to wait for seconds..?
            timeLeft -= Time.deltaTime;
            if (!dedicatedServer)
            {
                UInt16 currentTimeLeftUInt16 = (UInt16)timeLeft;
                if (currentTimeLeftUInt16 != previousTimeLeftUInt16)
                {
                    UpdateText(currentTimeLeftUInt16);
                }
                UpdateBackgroundImage();
                previousTimeLeftUInt16 = currentTimeLeftUInt16;
            }

            yield return null;
        }

        //TODO: cache his bool so we don't call a getter..?
        if (isServer)
        {
            GameManager.OnCountdownStopped();
        }

    }

    private void ShowGraphics(bool value)
    {
        UIObject.SetActive(value);
    }

    private void UpdateText(UInt16 timeLeft)
    {
        text.text = timeLeft.ToString();
    }

    private void UpdateBackgroundImage()
    {
        fillImage.fillAmount = (timeLeft / initialTime);
    }

    [Server]
    public void Server_StopCounting()
    {
        StopCoroutine(countRoutine);
        Rpc_StopCounting();
    }

    [ClientRpc]
    public void Rpc_StopCounting()
    {
        if (isClientOnly)
        {
            StopCoroutine(countRoutine);
        }
    }
    /* [ClientRpc]
private void Rpc_UpdateText(UInt16 timeLeft)
{
    text.text = timeLeft.ToString();
}*/

    /*[Server]
private void StopCounting()
{
    isActive = false;
    GameManager.OnCounterStopped();
}*/
}