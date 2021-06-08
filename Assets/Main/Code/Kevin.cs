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
    private List<int> availableDropPointIndexes = new List<int>();
    private float availableDropPointCheckRadius = 1.5f;
    private Collider[] availableDropPointCheckColliders = new Collider[16];
    private int nextDropPointIndex;
    private const float REQUIRED_DROP_POINT_DISTANCE = (1f * 1f);
    [SerializeField] private NavMeshAgent navAgent;
    private Transform myTransform;
    //private Spawnables[] allowedDroppedItems;
    [SerializeField]private DroppableItem[] droppableItems;
    private int droppableItemsOverallChancePoints;
    [SerializeField] private Animator animator;
    [SerializeField] private NetworkAnimator networkAnimator;
    
    [System.Serializable]
    private struct DroppableItem
    {
        public Spawnables spawnable;
        public int dropChancePoints;
        [HideInInspector] public int chanceThreshold;
    }

    private void Start()
    {
        //Removing unnesssry components from Kevin
        //TODO: Find a way to remove these things from the build, they shouldn't be there in the first place
        if(NetworkServer.active)
        {
            myTransform = transform;
            navAgent.enabled = false;
            state = KevinStates.ChoosingTagger;
            InitialiseDroppableItems();
           // allowedDroppedItems = new Spawnables[] { Spawnables.HealthPickup, Spawnables.FootballPickup/*, Spawnables.Trap*/ };
        }
        else
        {
            Destroy(navAgent);
            Destroy(this);
        }
    }

    private void InitialiseDroppableItems()
    {
        //Debug.Log("InitialiseDroppableItems");
        droppableItemsOverallChancePoints = 0;
        for (int i = 0; i < droppableItems.Length; i++)
        {
            ref DroppableItem droppable = ref droppableItems[i];
            if(droppable.dropChancePoints <= 0)
            {
                Debug.LogError("droppable has an invalid value for dropChancePoints, correcting...");
                droppable.dropChancePoints = 1;
            }
            droppableItemsOverallChancePoints += droppable.dropChancePoints;
            droppable.chanceThreshold = droppableItemsOverallChancePoints;
            Debug.Log($"droppableItems[{i}].chanceThreshold: {droppableItems[i].chanceThreshold}");
        }


    }
    /* private void FixedUpdate()
     {
         if (/*isServer && /state == KevinStates.DroppingItems)
         {
             DropRoutine();
         }
     }*/

    [Server]
    private IEnumerator ItemDropRoutine()
    {
        //HARDCODED: wait times are arbitrary
        state = KevinStates.DroppingItems;//TODO: Should we change state here..?
        navAgent.enabled = true;
        ChangeDestination();//TODO: might need more than that
        animator.SetBool("IsWalking", true);

        while (state == KevinStates.DroppingItems) //TODO: Hmm maybe first condition should do with GameManager's state
        {
            Vector3 dropPoint = dropPoints[nextDropPointIndex].position;//TODO: We could cache this for performance but what if drop point is supposed to be dynamic?
            float squaredDistanceFromDestination = Vector3.SqrMagnitude(myTransform.position - dropPoint);
            if (squaredDistanceFromDestination < REQUIRED_DROP_POINT_DISTANCE)//HARDCODED
            {
                DropItem();
                //NOTE: This wait is here mainly to ensure that OverlapSphere performed in ChangeDestination 
                //detects the thing we just dropped...
                yield return new WaitForSeconds(0.1f);
                while (!ChangeDestination())
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }
            yield return new WaitForSeconds(0.1f);
        }

    }

    [Server]
    public void StartDropRoutine()
    {
        StartCoroutine(ItemDropRoutine());
    }

    [Server]
    private void DropItem()
    {
        int chance = Random.Range(0, droppableItemsOverallChancePoints);
        Debug.Log($"chance = {chance}");
        Spawnables spawnable = Spawnables.Null;
        for (int i = 0; i < droppableItems.Length; i++)
        {
            if(chance < droppableItems[i].chanceThreshold)
            {
                spawnable = droppableItems[i].spawnable;
                break;
            }
        }
        if(spawnable == Spawnables.Null)
        {
            Debug.LogError("Thine chance model did not work after all!");
            return;
        }
        //int droppedItemIndex = Random.Range(0, allowedDroppedItems.Length);
        Vector3 itemPosition = dropPoints[nextDropPointIndex].position; //myTransform.position;// + (myTransform.forward * -1.1f);//HARDCODED
        Spawner.Spawn(spawnable, itemPosition, Quaternion.identity);
    }

    [Server]
    private bool ChangeDestination()
    {
        Debug.Log("ChangeDestination");
        bool foundValidDestination = false;
        //TODO: We could use a more efficient and elegant solution for monitoring available drop points. 
        availableDropPointIndexes.Clear();
        for (int i = 0; i < dropPoints.Length; i++)
        {
            Vector3 dropPoint = dropPoints[i].position;
            int colliderCount = Physics.OverlapSphereNonAlloc(dropPoint, availableDropPointCheckRadius ,availableDropPointCheckColliders);
            if(colliderCount == availableDropPointCheckColliders.Length)
            {
                Debug.LogWarning("colliderCount == availableDropPointCheckColliders.Length");
            }
            for (int j = 0; j < colliderCount; j++)
            {
                if(availableDropPointCheckColliders[j].GetComponentInParent<PickUp>() != null)
                {
                    goto Continue;
                }
            }
            availableDropPointIndexes.Add(i);
            Continue:;

        }

        Debug.Log("availableDropPointIndexes.Count: " + availableDropPointIndexes.Count);

        if (availableDropPointIndexes.Count > 0)
        {
            nextDropPointIndex = availableDropPointIndexes[Random.Range(0, availableDropPointIndexes.Count)];
            navAgent.SetDestination(dropPoints[nextDropPointIndex].position);
            foundValidDestination = true;
            /*Debug.Log(navAgent.hasPath);
            Debug.Log(navAgent.remainingDistance);*/
        }

        return foundValidDestination;
    }

    [Server]
    public void Spin(Vector3 lookAtPoint)
    {
        StartCoroutine(SpinCoroutine(lookAtPoint));
    }

    [Server]
    public IEnumerator SpinCoroutine(Vector3 lookAtPoint)
    {
        networkAnimator.SetTrigger("PointAt");
        yield return new WaitForSeconds(0.5f);

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
