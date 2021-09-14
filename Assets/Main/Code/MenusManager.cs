using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HashtagChampion.CharacterCreation;

public class MenusManager : MonoBehaviour
{
    [SerializeField] private CharacterCreationManager characterCreationManager;
    [SerializeField] private MenuManager mainMenuManager;
    [SerializeField] private MenuManager playerSettingsMenuManager;
    [SerializeField] private MenuManager matchSettingsMenuManager;
    [SerializeField] private MenuManager joinGameMenuManager;

    private MenuManager activeMenu;

    private void Start()
    {
        mainMenuManager.Activate();
        activeMenu = mainMenuManager;
        characterCreationManager.Deactivate();
        playerSettingsMenuManager.Deactivate();
        matchSettingsMenuManager.Deactivate();
        joinGameMenuManager.Deactivate();
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

    public void GoToMatchSettingsMenu()
    {
        GoToMenu(matchSettingsMenuManager);
    }

    public void GoToJoinGameMenu()
    {
        GoToMenu(joinGameMenuManager);
    }
}
