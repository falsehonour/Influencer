using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableAccessPoint : MonoBehaviour
{
    [SerializeField]  private Interactable interactable;
    public Interactable GetInterractable()
    {
        return interactable;
    }
    [HideInInspector]public Transform myTransform;
    private void Start()
    {
        myTransform = this.transform;
    }
    /*public void Interact(PlayerController player)
    {
        interactable.Interact(player);
    }*/


}
