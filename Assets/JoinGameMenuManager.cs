using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JoinGameMenuManager : MenuManager
{
    [SerializeField] private TMPro.TMP_InputField codeInputField;
    public override void Activate()
    {
        base.Activate();
        codeInputField.text = "";
    }
}
