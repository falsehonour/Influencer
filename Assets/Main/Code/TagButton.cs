using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagButton : MonoBehaviour
{
    public void OnClick()
    {
        PlayerController.localPlayerController.TryTag();
    }
}
