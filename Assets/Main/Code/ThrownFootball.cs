using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class ThrownFootball : Spawnable
{
    private const float SPEED = 11f;
    private const int PROJECTILE_LAYER = 8;
    private const int PARTICLES_LAYER = 9;
    //private static readonly float FIXED_UPDATE_SPEED = SPEED * Time.fixedDeltaTime;
    [SerializeField] private Rigidbody rigidbody;
    //private Vector3 cachedFixedUpdateMovement;
    private bool isFlying;

    protected override void OnSpawn(Vector3 position, Quaternion rotation)
    {
        base.OnSpawn(position, rotation);

        if (isServer)
        {
            CancelInvoke("Die");
        }
        Invoke("Die", 8f);
        //cachedFixedUpdateMovement = myTransform.forward * SPEED * Time.fixedDeltaTime;
        myTransform.localScale = Vector3.one;

        rigidbody.isKinematic = false;
        rigidbody.useGravity = false;
        rigidbody.angularVelocity = Vector3.zero;
        rigidbody.velocity = Vector3.zero;
        rigidbody.AddForce(myTransform.forward * SPEED, ForceMode.VelocityChange);

        //rigidbody.isKinematic = true;
        isFlying = true;

        PhysicsUtility.SetRootAndDecendentsLayers(gameObject, PROJECTILE_LAYER);

    }

    protected override void OnDeath()
    {
        base.OnDeath();
        rigidbody.isKinematic = true;
        //Hide();
        StartCoroutine(Shrink(3f));
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (IsAlive && isFlying)
        {
            if (isServer)
            {
                //TODO: These invokes mess up everythin, put some death timer instead
                CancelInvoke("Die");
                Invoke("Die", 2f);
                PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();
                if(playerController != null)
                {
                   //Debug.Log("")
                    playerController.OnFootballHit();
                }
                // Server_OnTriggerEnter(other);
            }
            Stop();
        }
    }

    /*[Server]
    private void Server_OnTriggerEnter(Collider other)
    {
        //Die();
    }
    */
    private void Stop()
    {
        //SwitchKinematicState(false);
        rigidbody.useGravity = true;
        rigidbody.AddForce((myTransform.forward * -1) * 2f, ForceMode.Impulse);//HARDCODED
        isFlying = false;
        PhysicsUtility.SetRootAndDecendentsLayers(gameObject, PARTICLES_LAYER);
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
