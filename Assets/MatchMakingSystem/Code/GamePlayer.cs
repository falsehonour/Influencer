using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class GamePlayer : NetworkBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private float speed = 1;

    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            Vector3 movement = Vector3.zero;
            {
                //Input
                float horizontalMovement = Input.GetAxisRaw("Horizontal");
                movement.x = horizontalMovement;
                float verticalMovement = Input.GetAxisRaw("Vertical");
                movement.z = verticalMovement;
            }

            movement *= speed * Time.fixedDeltaTime;
            characterController.Move(movement);
        }

    }

    private void Update()
    {
        if (isLocalPlayer)
        {
          /*  if (Input.GetKeyDown(KeyCode.Escape))
            {
                Player.localPlayer.DisconnectGame();
            }*/
        }
    }
}
