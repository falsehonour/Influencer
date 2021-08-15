using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UISettingsController : MonoBehaviour
{
    [SerializeField]private Button forwardButton;
    [SerializeField] private Button backwardButton;
    [SerializeField] private TMPro.TextMeshProUGUI text;

    private Type enumType;
    public event Action<sbyte> OnValueChangedEvent;
    private sbyte value;
    private sbyte minValue;
    private sbyte maxValue;

    public void Initialise(Action<sbyte> onValueChangedAction, Type enumType)
    {
        this.enumType = enumType;// = typeof(T);
        OnValueChangedEvent += onValueChangedAction;

        minValue = (sbyte)Enum.Parse(enumType, "Min");
        maxValue = (sbyte)Enum.Parse(enumType, "Max");
        /* Debug.Log("minValue: " + minValue);
         Debug.Log("maxValue: " + maxValue);*/
        InitialiseListeners();
    }

    private void InitialiseListeners()
    {
        forwardButton.onClick.RemoveAllListeners();
        forwardButton.onClick.AddListener(delegate
        {
            SetValue((sbyte)(value+1), true);
        });

        backwardButton.onClick.RemoveAllListeners();
        backwardButton.onClick.AddListener(delegate
        {
            SetValue((sbyte)(value - 1), true);
        });
    }

    public void SetValue(sbyte newValue, bool invokeAction)
    {

        if (newValue < minValue)
        {
            newValue = maxValue;
        }
        else if (newValue > maxValue)
        {
            newValue = minValue;
        }
        value = newValue;
        text.text = Enum.ToObject(enumType, value).ToString();
        if (invokeAction)
        {
            OnValueChangedEvent?.Invoke(value);
        }
    }
}
