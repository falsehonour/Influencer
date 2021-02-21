using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Liftable : Interactable
{
    [SerializeField] private Transform graphics;
    public Transform Graphics
    {
        get
        {
            return graphics;
        }
    }
   /* public override void Interact(PlayerController player)
    {
        base.Interact(player);
        player.Lift(this);
    }*/

}
