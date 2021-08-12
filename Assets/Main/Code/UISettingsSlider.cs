using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class UISettingsSlider : MonoBehaviour
{
    //public event Action<float> OnValueChangedAction;
    //[SerializeField] private Text text;
    [SerializeField] private Slider slider;
    [SerializeField] private InputField inputField;
    public event Action<float> OnValueChangedEvent;
    private float valueNormaliser;

    public void Initialise(Action<float> onValueChangedAction, float initialValue, float minValue, float maxValue,  float valueNormaliser = 1 )
    {

        InitialiseListeners();

        this.valueNormaliser = valueNormaliser;
        slider.minValue = minValue;
        slider.maxValue = maxValue;
        slider.wholeNumbers = false;
        OnValueChangedEvent += onValueChangedAction;
        SetValue(initialValue, false);
        slider.value = initialValue;//TODO: This was done only to avoid stack overflow. find a way to change the slider value without triggering slider.onValueChanged
    }

    private void SetValue(float value, bool invokeAction)
    {
        //NOTE Yeah, we are setting the slider/ input field to value they already have sometimes 
        value = NormaliseValue(value);
        //slider.value = value;//TODO: This was done only to avoid stack overflow. find a way to change the slider value without triggering slider.onValueChanged
        //Debug.Log(gameObject.name + "Value was set");
        inputField.text = value.ToString();
        if (invokeAction)
        {
            OnValueChangedEvent?.Invoke(value);
        }
    }

    private float NormaliseValue(float value)
    {
        float remainder = (value % valueNormaliser);
        if(remainder < (valueNormaliser * 0.5f))
        {
            value -= remainder;
        }
        else
        {
            value += (valueNormaliser-remainder);
        }
        return value;
    }

    private void InitialiseListeners()
    {
        slider.onValueChanged.RemoveAllListeners();
        slider.onValueChanged.AddListener(delegate 
        {
            float value = slider.value;
            SetValue(value,true);
        });

        inputField.onEndEdit.RemoveAllListeners();
        inputField.onEndEdit.AddListener(delegate 
        {
            float value = 0;
            bool workableValue = (float.TryParse(inputField.text, out value));
            if (workableValue)
            {
                value = Mathf.Clamp(value, slider.minValue, slider.maxValue);
                SetValue(value, true);
            }
            else
            {
                inputField.text = slider.value.ToString();
            }
            // text.text = slider.value.ToString(); 
        });
    }

    /* public void OnValueChanged()
     {
         float value = slider.value;
         text.text = value.ToString();
         //OnValueChangedAction.Invoke(value);
     }*/
}
