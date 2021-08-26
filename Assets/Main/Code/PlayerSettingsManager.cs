using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Change name
public class PlayerSettingsManager : MonoBehaviour
{
    [SerializeField] private Joystick fixedJoystick;
    [SerializeField] private Joystick dynamicJoystick;

    public void SetActiveJoystick(bool fixedJoystick)
    {
        Joystick activeJoystick = fixedJoystick ? this.fixedJoystick : dynamicJoystick;
        Joystick nonactiveJoystick = fixedJoystick ? dynamicJoystick : this.fixedJoystick;

        activeJoystick.gameObject.SetActive(true);
        nonactiveJoystick.gameObject.SetActive(false);
        HashtagChampion.PlayerController.SetActiveJoystick(activeJoystick);

    }
}
