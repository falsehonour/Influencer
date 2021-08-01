using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HashtagChampion.CharacterCreation;

public class MenusManager : MonoBehaviour
{
    [SerializeField] private GameObject roomSettingsMenu;
    [SerializeField] private GameObject playerSettingsMenu;
    [SerializeField] private CharacterCreationManager characterCreationManager;
    [SerializeField] private MainMenuManager mainMenuManager;
    [SerializeField] private PlayerSettingsMenuManager playerSettingsMenuManager;

    private MenuManager activeMenu;

    private void Start()
    {
        roomSettingsMenu.SetActive(false);
        playerSettingsMenu.SetActive(false);

        mainMenuManager.Activate();
        activeMenu = mainMenuManager;
        characterCreationManager.Deactivate();
        playerSettingsMenuManager.Deactivate();

    }

    public void SwitchRoomSettingsMenu()
    {
        bool switchOn = !roomSettingsMenu.activeSelf;
        roomSettingsMenu.SetActive(switchOn);
        playerSettingsMenu.SetActive(false);

    }

    private void GoToMenu(MenuManager newMenu)
    {
        activeMenu.Deactivate();
        newMenu.Activate();
        activeMenu = newMenu;
    }

    //TODO: Repeating buisness:

    public void GoToMainMenu()
    {
        GoToMenu( mainMenuManager);
    }

    public void GoToCharacterCreation()
    {
        GoToMenu(characterCreationManager);
    }

    public void GoToPlayerSettingsMenu()
    {
        GoToMenu(playerSettingsMenuManager);
    }
}
