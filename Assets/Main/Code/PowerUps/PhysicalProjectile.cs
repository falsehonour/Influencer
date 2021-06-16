﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PhysicalProjectile : Spawnable
{

    private const int PROJECTILE_LAYER = 8;
    private const int PARTICLES_LAYER = 9;
    //private static readonly float FIXED_UPDATE_SPEED = SPEED * Time.fixedDeltaTime;
    [SerializeField] private Rigidbody rigidbody;
    //private Vector3 cachedFixedUpdateMovement;
    private bool isFlying;
    protected virtual float Speed { get; }


    protected override void OnSpawn(Vector3 position, Quaternion rotation)
    {
        base.OnSpawn(position, rotation);
        //TODO: Stop invoking.. use our custom timer perhaps;
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
        rigidbody.AddForce(myTransform.forward * Speed, ForceMode.VelocityChange);
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
                Invoke("Die", 2f);//HARDCODED
                PlayerController player = collision.gameObject.GetComponent<PlayerController>();
                if(player != null)
                {
                    Debug.Log("Player HIT");
                    Hit(player);
                    //playerController.OnFootballHit();
                }
                // Server_OnTriggerEnter(other);
            }
            Stop();
        }
    }

    protected virtual void Hit(PlayerController player){}

    private void Stop()
    {
        //SwitchKinematicState(false);
        rigidbody.useGravity = true;
        rigidbody.AddForce((myTransform.forward * -1) * 2f, ForceMode.Impulse);//HARDCODED
        isFlying = false;
        PhysicsUtility.SetRootAndDecendentsLayers(gameObject, PARTICLES_LAYER);
    }
}