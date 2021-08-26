using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace HashtagChampion
{
    public abstract class PhysicalProjectile : Spawnable
    {
        private const int PROJECTILE_LAYER = 8;
        private const int PARTICLES_LAYER = 9;
        //private static readonly float FIXED_UPDATE_SPEED = SPEED * Time.fixedDeltaTime;
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private Collider myCollider;
        private Collider ignoredCollider;
        //private Vector3 cachedFixedUpdateMovement;
        private bool isFlying;


        protected virtual float Speed { get; }


        protected override void OnSpawn(Vector3 position, Quaternion rotation, uint callerNetId)
        {
            base.OnSpawn(position, rotation, callerNetId);
            //TODO: Stop invoking.. use our custom timer perhaps;
            if (isServer)
            {
                CancelInvoke("Die");
            }
            Invoke("Die", 8f);
            //cachedFixedUpdateMovement = myTransform.forward * SPEED * Time.fixedDeltaTime;
            myTransform.localScale = Vector3.one;
            //TODO: Go back to the previous system where callerNetId is sent seperately from OnSpawn. 
            //Most spawnables don't care about their senders and the delay you thought you saw might not exist
            NetworkIdentity networkIdentity = NetworkIdentity.spawned[callerNetId];
            if (networkIdentity == null)
            {
                Debug.LogError($"THE NETWORK IDENTITY {callerNetId} LOOKING FOR DOES NOT EXIST!");
            }
            else
            {
                Collider collider = networkIdentity.GetComponent<Collider>();
                if (collider == null)
                {
                    Debug.LogError($"This networkIdentity {networkIdentity.name} has no collider to speak of...!");
                }
                else
                {
                    IgnoreCollider(collider);
                }
            }


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
                    if (player != null /*&& player != originator*/)
                    {
                        Debug.Log("Player HIT");
                        HitPlayer(player);
                        //playerController.OnFootballHit();
                    }
                    // Server_OnTriggerEnter(other);
                }
                Stop();
            }
        }

        /* private void OnTriggerEnter(Collider other)
         {
             if (IsAlive && isFlying)
             {
                 if (isServer)
                 {
                     //TODO: These invokes mess up everythin, put some death timer instead
                     CancelInvoke("Die");
                     Invoke("Die", 2f);//HARDCODED
                     PlayerController player = other.gameObject.GetComponent<PlayerController>();
                     if (player != null)
                     {
                         Debug.Log("Player HIT");
                         Hit(player);
                     }
                 }
                 Stop();
             }
         }*/


        protected virtual void HitPlayer(PlayerController player) { }

        protected virtual void Stop()
        {
            //SwitchKinematicState(false);
            rigidbody.useGravity = true;
            rigidbody.AddForce((myTransform.forward * -1) * 2f, ForceMode.Impulse);//HARDCODED
            isFlying = false;
            PhysicsUtility.SetRootAndDecendentsLayers(gameObject, PARTICLES_LAYER);
        }

        private void IgnoreCollider(Collider collider)
        {
            if (ignoredCollider != null)
            {
                Physics.IgnoreCollision(myCollider, ignoredCollider, false);
            }


            ignoredCollider = collider;
            Physics.IgnoreCollision(myCollider, ignoredCollider, true);
        }
        /*public void SetIgnoredCollider(Collider collider)
        {
            if (ignoredCollider != null)
            {
                Physics.IgnoreCollision(myCollider, ignoredCollider, false);
            }

            ignoredCollider = collider;
            Physics.IgnoreCollision(myCollider, ignoredCollider, true);
        }*/


    }

}
