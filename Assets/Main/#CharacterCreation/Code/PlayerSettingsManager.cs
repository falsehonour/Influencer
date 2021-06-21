using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettingsManager : MonoBehaviour
{
    //TODO: Why are we doing things so differently compared to room settings? 
    //NOTE: settings can be set wully-nully at the moment
    public PlayerSettings settings;
    [SerializeField] private GameObject fixedJoystick;
    [SerializeField] private GameObject dynamicJoystick;
    [SerializeField] private GameObject fpsCounter;

    [SerializeField] private Toggle fpsToggle;
    [SerializeField] private Toggle joystickToggle;

    public void ApplySettings()
    {
        fixedJoystick.SetActive(settings.fixedJoystick);
        dynamicJoystick.SetActive(!settings.fixedJoystick);
        fpsCounter.SetActive(settings.showFPS);
    }

    private void Awake()
    {
        fpsToggle.isOn = settings.showFPS;
        fpsToggle.onValueChanged.AddListener(delegate
        {
            bool value = fpsToggle.isOn;
            settings.showFPS = value;
        });

        joystickToggle.isOn = settings.fixedJoystick;
        joystickToggle.onValueChanged.AddListener(delegate
        {
            bool value = joystickToggle.isOn;
            settings.fixedJoystick = value;
        });
    }
}
