using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerButton : MonoBehaviour
{
    public void TryTag()
    {
        PlayerController.localPlayerController.TryTag();
    }

    public void TryPlaceTrap()
    {
        PlayerController.localPlayerController.TryPlaceTrap();
    }
}
