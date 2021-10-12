using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: Change name
namespace HashtagChampion
{
    public class PlayerSettingsManager : MonoBehaviour
    {
        [SerializeField] private Joystick fixedJoystick;
        [SerializeField] private Joystick dynamicJoystick;

        private void Start()
        {
            SetActiveJoystick(StaticData.playerSettings.joystickType == JoystickTypes.Fixed);
        }

        private void SetActiveJoystick(bool fixedJoystick)
        {
            Joystick activeJoystick = fixedJoystick ? this.fixedJoystick : dynamicJoystick;
            Joystick nonactiveJoystick = fixedJoystick ? dynamicJoystick : this.fixedJoystick;

            activeJoystick.gameObject.SetActive(true);
            nonactiveJoystick.gameObject.SetActive(false);
            HashtagChampion.PlayerController.SetActiveJoystick(activeJoystick);

        }

        public void LeaveMatch()
        {
            PlayerController.localPlayerController.Cmd_LeaveMatch();
        }
    }
}

