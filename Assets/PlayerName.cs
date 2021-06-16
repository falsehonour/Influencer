using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerName : MonoBehaviour
{
    //TODO: This is a temporary class, get rid of it.
    private static readonly string fallbackName = "Player";
    public static string name = fallbackName;
    private static PlayerName instance;
    [SerializeField] private InputField nameInputField;

    private void Awake()
    {
        instance = this;
    }

    public void OnInputFieldUpdated()
    {
        SetName(nameInputField.text);
    }

    private void SetName(string newName)
    {
        if(newName != string.Empty)
        {
            name = newName;
        }
        else
        {
            name = fallbackName;
        }
    }
}

