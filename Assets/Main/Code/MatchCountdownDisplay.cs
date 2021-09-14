using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MatchCountdownDisplay : MonoBehaviour
{
    [SerializeField] private Image[] digits;
    [SerializeField] private Sprite[] digitsSprites;

    public void UpdateDigits(UInt16 timeLeft)
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

    public void Show(bool value)
    {
        gameObject.SetActive(value);
    }
}
