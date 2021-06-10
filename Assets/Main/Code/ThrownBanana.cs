using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ThrownBanana : Spawnable
{
    /*private const int PROJECTILE_LAYER = 8;
    private const int PARTICLES_LAYER = 9;*/
    [SerializeField] private Rigidbody rigidbody;
    [SerializeField] private Collider triggerCollider;

    protected override void OnSpawn(Vector3 position, Quaternion rotation)
    {
        base.OnSpawn(position, rotation);

        if (isServer)
        {
            CancelInvoke("Die");
        }
        myTransform.localScale = Vector3.one;

        rigidbody.isKinematic = false;
        rigidbody.useGravity = true;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.velocity = Vector3.zero;
        triggerCollider.enabled = true;

        //rigidbody.AddForce(myTransform.forward * SPEED, ForceMode.VelocityChange);

        // PhysicsUtility.SetRootAndDecendentsLayers(gameObject, PROJECTILE_LAYER);

    }

    protected override void OnDeath()
    {
        base.OnDeath();
        rigidbody.isKinematic = true;
        triggerCollider.enabled = false;
        //PhysicsUtility.SetRootAndDecendentsLayers(gameObject, PARTICLES_LAYER);

        //Hide();
        StartCoroutine(Shrink(0.5f));
    }
  
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log("Banana: OnCollisionEnter");

        if (IsAlive)
        {
            if (isServer)
            {
                //TODO: These invokes mess up everythin, put some death timer instead
                PlayerController playerController = 
                    collision.gameObject.GetComponentInParent<PlayerController>();
                if(playerController != null)
                {
                    Debug.Log("Banana: playerController != null");
                    playerController.OnFootballHit();
                    CancelInvoke("Die");
                    Die();
                }
            }
        }
    }


    private void OnTriggerEnter(Collider other)
    {
        //NOTE: We have both a normal and a trigger collider on this banana because CharacterController 
        //is not very good at detecting normal collisions for some reason..

       // Debug.Log("Banana: OnTriggerEnter");
        if (IsAlive)
        {
            if (isServer)
            {
                //TODO: These invokes mess up everythin, put some death timer instead
                PlayerController playerController =
                    other.gameObject.GetComponentInParent<PlayerController>();
                if (playerController != null && playerController.CanSlip())
                {//NOTE: Landing on a banana after slipping gives ammunity from the next banana
                    playerController.Slip();
                    CancelInvoke("Die");
                    Die();
                }
            }
        }
    }
    /*private void SwitchKinematicState(bool value)
    {
        Debug.Log("SwitchKinematicState: " + value);
        rigidbody.isKinematic = value;
        //TODO: cache an array if there's more than one collider
        Collider collider = GetComponentInChildren<Collider>();
        collider.isTrigger = value;
    }*/
}
