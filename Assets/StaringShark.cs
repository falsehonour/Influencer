using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HashtagChampion
{
    public class StaringShark : MonoBehaviour
    {
        private Transform myTransform;
        [SerializeField] private float rotationSpeed;
        private Transform lookAtTransform;

        void Start()
        {
            if (Mirror.NetworkServer.active)
            {
                Destroy(gameObject);
            }
            else
            {
                myTransform = transform;
                StartCoroutine(FindClosestPlayer());
            }

        }

        private IEnumerator FindClosestPlayer()
        {
            WaitForSeconds waitInterval = new WaitForSeconds(0.25f);
            List<Transform> allPlayerTransforms = PlayerController.allPlayerTransforms;
            while (true)
            {
                int playerCount = allPlayerTransforms.Count;
                if(playerCount > 0)
                {
                    Vector3 myPosition = myTransform.position;
                    float smallestDistance = float.MaxValue;
                    int smallestDistanceIndex = 0;
                    for (int i = 0; i < playerCount; i++)
                    {
                        float squaredDistance = Vector3.SqrMagnitude(allPlayerTransforms[i].position - myPosition);
                        if (squaredDistance < smallestDistance)
                        {
                            smallestDistance = squaredDistance;
                            smallestDistanceIndex = i;
                        }
                    }
                    lookAtTransform = allPlayerTransforms[smallestDistanceIndex];
                }
                
                yield return waitInterval;

            }
        }

        private void FixedUpdate()
        {
            if (lookAtTransform)
            {
                Vector3 lookAtForward = (lookAtTransform.position - myTransform.position);
                lookAtForward.y = 0;
                Quaternion targetRotation = Quaternion.LookRotation(lookAtForward);
                Quaternion newRotation = Quaternion.RotateTowards(myTransform.rotation, targetRotation,
                    rotationSpeed * Time.fixedDeltaTime);
                myTransform.rotation = newRotation;
            }

        }
    }
}

