using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    [SerializeField] private PlayerUI playerUIPreFab;
    [SerializeField] private Transform playerUICanvasTransform;
    [SerializeField] private Camera camera;

    private static PlayerUIManager instance;

    private void Awake()
    {
        instance = this;
    }

    public static PlayerUI CreatePlayerUI(Transform anchor)
    {
        return instance.CreateNewPlayerUI(anchor);
    }

    private PlayerUI CreateNewPlayerUI(Transform anchor)
    {
        PlayerUI playerUI = Instantiate(playerUIPreFab, playerUICanvasTransform);
        playerUI.Initialise(anchor, camera);
        return playerUI;
    }
}
