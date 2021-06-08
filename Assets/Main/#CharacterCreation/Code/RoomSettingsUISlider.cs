using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
public class RoomSettingsUISlider : MonoBehaviour
{
    //public event Action<float> OnValueChangedAction;
    //[SerializeField] private Text text;
    [SerializeField] private Slider slider;
    [SerializeField] private InputField inputField;
    public event Action<float> OnValueChangedAction;

    public void SetValue(float value, bool invokeAction)
    {
        //NOTE Yeah, we are setting the slider/ input field to value they already have sometimes 
        slider.value = value;
        inputField.text = value.ToString();
        if (invokeAction)
        {
            OnValueChangedAction?.Invoke(value);
        }
    }

    private void Awake()
    {
           slider.onValueChanged.AddListener(delegate 
           {
               float value = slider.value;
               SetValue(value,true);
           });

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
    //[SerializeField] private Slider playerCountSlider;

}
