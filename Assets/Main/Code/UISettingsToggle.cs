using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UISettingsToggle : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    public event Action<bool> OnValueChangedEvent;

    public void Initialise(Action<bool> onValueChangedAction, bool initialValue)
    {
        InitialiseListeners();
        OnValueChangedEvent += onValueChangedAction;
        toggle.isOn = initialValue;
    }

    private void SetValue(bool value)
    {
         OnValueChangedEvent?.Invoke(value);
    }

    private void InitialiseListeners()
    {
        toggle.onValueChanged.RemoveAllListeners();
        toggle.onValueChanged.AddListener(delegate
        {
            bool value = toggle.isOn;
            SetValue(value);
        });
    }
}
