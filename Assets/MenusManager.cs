using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenusManager : MonoBehaviour
{
    [SerializeField] private GameObject roomSettingsMenu;
    [SerializeField] private GameObject playerSettingsMenu;

    private void Start()
    {
        roomSettingsMenu.SetActive(false);
        playerSettingsMenu.SetActive(false);
    }

    public void SwitchRoomSettingsMenu()
    {
        bool switchOn = !roomSettingsMenu.activeSelf;
        roomSettingsMenu.SetActive(switchOn);
        playerSettingsMenu.SetActive(false);

    }

    public void SwitchPlayerSettingsMenu()
    {
        bool switchOn = !playerSettingsMenu.activeSelf;
        playerSettingsMenu.SetActive(switchOn);
        roomSettingsMenu.SetActive(false);

    }
}
