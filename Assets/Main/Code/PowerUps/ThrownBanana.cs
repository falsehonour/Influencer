using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

namespace HashtagChampion
{
    public class ThrownBanana : Spawnable
    {
        /*private const int PROJECTILE_LAYER = 8;
        private const int PARTICLES_LAYER = 9;*/
        [SerializeField] private Rigidbody rigidbody;
        [SerializeField] private Collider triggerCollider;

        protected override void OnSpawn(Vector3 position, Quaternion rotation, uint callerNetId)
        {
            base.OnSpawn(position, rotation, callerNetId);

            if (isServer)
            {
                CancelInvoke(nameof(Die));
            }
            myTransform.localScale = Vector3.one;

            rigidbody.isKinematic = false;
            rigidbody.useGravity = true;
            rigidbody.angularVelocity = Vector3.zero;
            rigidbody.velocity = Vector3.zero;
            triggerCollider.enabled = true;

            SoundManager.PlayOneShotSound(SoundNames.BananaThrow, position);

            // PhysicsUtility.SetRootAndDecendentsLayers(gameObject, PROJECTILE_LAYER);

        }

        protected override void OnDeath()
        {
            base.OnDeath();
            rigidbody.isKinematic = true;
            triggerCollider.enabled = false;
            //PhysicsUtility.SetRootAndDecendentsLayers(gameObject, PARTICLES_LAYER);
            //Hide();
            StartCoroutine(Shrink(0.75f));//HARDCODED
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
                    Player player = other.gameObject.GetComponentInParent<Player>();
                    //NOTE: Players who enter a banana while being ammune to slipping will not slip even once they are immune no lomger. should we use OnTriggerStay instead??
                    if (player != null)
                    {
                        //Debug.Log("Player Stepped in za banan");

                        if (player.CanSlip())
                        {//NOTE: Landing on a banana after slipping gives ammunity from the next banana
                            player.Slip();
                            //TODO: These invokes mess up everythin, put some death timer instead
                            CancelInvoke(nameof(Die));
                            Die();
                        }
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

}
