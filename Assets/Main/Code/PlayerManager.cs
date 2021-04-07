using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class PlayerManager : NetworkBehaviour
{
    public static List<PlayerController> allPlayers = new List<PlayerController>();

    [Server]
    public static void AddPlayer(PlayerController player)
    {
        allPlayers.Add(player);
    }
}
