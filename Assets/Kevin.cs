using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

public class Kevin : MonoBehaviour
{
    private enum KevinStates : byte
    {
        ChoosingTagger, DroppingItems
    }

    private KevinStates state;
    #region First Tagger Picking:
    [SerializeField] private AnimationCurve rotationCurve;
    [SerializeField] private int extraCompleteRotations;
    #endregion
    [SerializeField] private Transform[] dropPoints;
    private int nextDropPointIndex;
    private const float REQUIRED_DROP_POINT_DISTANCE = (1f * 1f);
    [SerializeField] private NavMeshAgent navAgent;
    private Transform myTransform;
    private Spawnables[] allowedDroppedItems;

    private void Start()
    {
        //Removing unnesssry components from Kevin
        if(NetworkServer.active)
        {
            myTransform = transform;
            navAgent.enabled = false;
            state = KevinStates.ChoosingTagger;
            allowedDroppedItems = new Spawnables[] { Spawnables.HealthPickup, Spawnables.Trap };
        }
        else
        {
            Destroy(navAgent);
            Destroy(this);
        }
    }

    private void FixedUpdate()
    {
        if (/*isServer && */state == KevinStates.DroppingItems)
        {
            DropRoutine();
        }

       /* Debug.Log(navAgent.hasPath);
        Debug.Log(navAgent.remainingDistance);*/

    }

    [Server]
    public void StartDropRoutine()
    {
        state = KevinStates.DroppingItems;
        navAgent.enabled = true;
        ChangeDestination();
    }

    [Server]
    private void DropRoutine()
    {
        float squaredDistanceFromDestination =
              Vector3.SqrMagnitude(myTransform.position - dropPoints[nextDropPointIndex].position);
        if (squaredDistanceFromDestination < REQUIRED_DROP_POINT_DISTANCE)//HARDCODED
        {
            DropItem();
            ChangeDestination();
        }
    }

    [Server]
    private void DropItem()
    {
        int droppedItemIndex = Random.Range(0, allowedDroppedItems.Length);
        Vector3 itemPosition = dropPoints[nextDropPointIndex].position; //myTransform.position;// + (myTransform.forward * -1.1f);//HARDCODED

        Spawner.Spawn(allowedDroppedItems[droppedItemIndex], itemPosition, Quaternion.identity);
    }

    [Server]
    private void ChangeDestination()
    {
        Debug.Log("ChangeDestination");
        nextDropPointIndex = Random.Range(0, dropPoints.Length);
        navAgent.SetDestination(dropPoints[nextDropPointIndex].position);
        /*Debug.Log(navAgent.hasPath);
        Debug.Log(navAgent.remainingDistance);*/
    }

    [Server]
    public void Spin(Vector3 lookAtPoint)
    {
        StartCoroutine(SpinCoroutine(lookAtPoint));
    }

    [Server]
    private IEnumerator SpinCoroutine(Vector3 lookAtPoint)
    {
        Keyframe lastKeyFrame = rotationCurve.keys[rotationCurve.keys.Length - 1];
        if (lastKeyFrame.value != 1)
        {
            Debug.LogWarning("lastKeyFrame.value != 1, spin animation will not play properly.");
        }
        float startY = transform.eulerAngles.y;

        Vector3 endDirection = (lookAtPoint - transform.position).normalized;
        Quaternion endRotation = Quaternion.LookRotation(endDirection);
        float endY = endRotation.eulerAngles.y + ((float)extraCompleteRotations * 360f);

        float endTime = lastKeyFrame.time;
        float currentTime = 0;
        float y;
        while (currentTime < endTime)
        {
            y = Mathf.Lerp(startY, endY, rotationCurve.Evaluate(currentTime));
            transform.rotation = Quaternion.Euler(0, y, 0);
            currentTime += Time.deltaTime;
            yield return null;
        }
    }
}
