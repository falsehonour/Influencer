using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;


namespace HashtagChampion
{
    public class MatchCountdown : NetworkBehaviour
    {
        private float timeLeft;
        private Coroutine countRoutine;
        //private UInt16 previousTimeLeftUInt16;
        //[SerializeField] private GameObject UIObject;
        private MatchCountdownDisplay display;

        [Client]
        public void Client_Initialise()
        {
            display = GameSceneManager.GetReferences().countdownDisplay;
        }

        public void Client_ConformToInitialState()
        {
            display.Show(false);
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
            bool isServer = this.isServer;
            if (!isServer)
            {
                display.Show(true);
            }

            UInt16 previousTimeLeftUInt16 = 0;
            timeLeft = time;
            do //while (timeLeft > 0)
            {
                //TODO: will it be more efficient to wait for seconds..?
                timeLeft -= Time.deltaTime;
                if (!isServer)
                {
                    UInt16 currentTimeLeftUInt16 = (UInt16)Mathf.CeilToInt(timeLeft);
                    if (currentTimeLeftUInt16 != previousTimeLeftUInt16)
                    {
                        display.UpdateDigits(currentTimeLeftUInt16);
                        if(currentTimeLeftUInt16 <= 10)
                        {
                            SoundNames sound = SoundNames.CountdownCritical;

                            if (currentTimeLeftUInt16 == 0)
                            {
                                sound = SoundNames.CountdownTimeIsUp;
                            }

                            SoundManager.PlayOneShotSound(sound, null);
                        }
                    }
                    previousTimeLeftUInt16 = currentTimeLeftUInt16;
                }

                yield return null;

            } while (timeLeft > 0);

            if (isServer)
            {
                MatchGameManager gameManager = FindObjectOfType<MatchGameManager>();
                if (gameManager)
                {
                    gameManager.OnCountdownStopped();
                }
                else
                {
                    Debug.LogError("Cannot find a game manager, Cannot stop time.");
                }
            }

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
    }
}
