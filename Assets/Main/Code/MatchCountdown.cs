using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

namespace HashtagChampion
{
    public class MatchCountdown : NetworkBehaviour
    {
        //TODO: Perhaps seperate into 2 classed, one for server and one for client graphics??? 
        [SerializeField] private GameObject UIObject;
        [SerializeField] private Image[] digits;
        [SerializeField] private Sprite[] digitsSprites;

        private float timeLeft;
        private Coroutine countRoutine;
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
            timeLeft = time;
            bool dedicatedServer = isServerOnly;
            do //while (timeLeft > 0)
            {
                //TODO: will it be more efficient to wait for seconds..?
                timeLeft -= Time.deltaTime;
                if (!dedicatedServer)
                {
                    UInt16 currentTimeLeftUInt16 = (UInt16)Mathf.CeilToInt(timeLeft);
                    if (currentTimeLeftUInt16 != previousTimeLeftUInt16)
                    {
                        UpdateDigits(currentTimeLeftUInt16);
                    }
                    previousTimeLeftUInt16 = currentTimeLeftUInt16;
                }

                yield return null;

            } while (timeLeft > 0);

            if (isServer)
            {
                GameManager gameManager = FindObjectOfType<GameManager>();
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

        private void ShowGraphics(bool value)
        {
            UIObject.SetActive(value);
        }

        private void UpdateDigits(UInt16 timeLeft)
        {
            string timeLeftString = timeLeft.ToString();
            int digitsDifference = digits.Length - timeLeftString.Length;
            if (digitsDifference > 0)
            {

                for (int i = digits.Length - 1; i > digits.Length - 1 - digitsDifference; i--)
                {
                    digits[i].sprite = digitsSprites[0];
                }
            }
            //int length = timeLeftString.Length < digits.Length ? timeLeftString.Length : digits.Length;
            for (int i = 0; i < timeLeftString.Length; i++)
            {
                char digitIndexChar = timeLeftString[i];
                int digitIndex = int.Parse(digitIndexChar.ToString());
                Sprite sprite = digitsSprites[digitIndex];
                digits[timeLeftString.Length - 1 - i].sprite = sprite;
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
