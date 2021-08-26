using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.AI;

namespace HashtagChampion
{
    public class Kevin : MonoBehaviour
    {
        private enum KevinStates : byte
        {
            Idle, ChoosingTagger, DroppingItems, Laughing
        }
        private enum KevinAnimationStates : int
        {
            Idle = 0, Point = 1, Walk = 2, Laugh = 3,
        }

        private KevinStates state;
        private string previousRoutine;
        #region First Tagger Picking:
        [SerializeField] private AnimationCurve rotationCurve;
        [SerializeField] private int extraCompleteRotations;
        #endregion
        [SerializeField] private Transform dropPointsParent;
        private Transform[] dropPoints;
        private List<int> availableDropPointIndexes = new List<int>();
        private float availableDropPointCheckRadius = 1.5f;
        private Collider[] availableDropPointCheckColliders = new Collider[16];
        private int nextDropPointIndex;
        private const float REQUIRED_DROP_POINT_DISTANCE = (1f * 1f);
        [SerializeField] private NavMeshAgent navAgent;
        private Transform myTransform;
        [SerializeField] private DroppableItem[] droppableItems;
        private int droppableItemsOverallChancePoints;
        [SerializeField] private Animator animator;
        //[SerializeField] private NetworkAnimator networkAnimator;
        private static Kevin instance;

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
            if (NetworkServer.active)
            {
                if (instance == null)
                {
                    instance = this;

                    // allowedDroppedItems = new Spawnables[] { Spawnables.HealthPickup, Spawnables.FootballPickup/*, Spawnables.Trap*/ };
                }
                else
                {
                    Destroy(this);
                    Debug.LogError("There can only be ONE Kevin Rubin!");
                }
            }
            else
            {
                Destroy(navAgent);
                Destroy(this);
            }
        }

        public void Initialise()
        {
            myTransform = transform;
            dropPoints = new Transform[dropPointsParent.childCount];
            for (int i = 0; i < dropPoints.Length; i++)
            {
                dropPoints[i] = dropPointsParent.GetChild(i);
            }
            InitialiseDroppableItems();
            previousRoutine = nameof(IdleRoutine);
            StartCoroutine(previousRoutine);
        }

        private void InitialiseDroppableItems()
        {
            //Debug.Log("InitialiseDroppableItems");
            droppableItemsOverallChancePoints = 0;
            for (int i = 0; i < droppableItems.Length; i++)
            {
                ref DroppableItem droppable = ref droppableItems[i];
                if (droppable.dropChancePoints <= 0)
                {
                    // Debug.LogError("droppable has an invalid value for dropChancePoints, correcting...");
                    droppable.dropChancePoints = 1;
                }
                droppableItemsOverallChancePoints += droppable.dropChancePoints;
                droppable.chanceThreshold = droppableItemsOverallChancePoints;
                //Debug.Log($"droppableItems[{i}].chanceThreshold: {droppableItems[i].chanceThreshold}");
            }
        }

        public void SpawnInitialPickups()
        {
            int initialPickups = TagNetworkManager.RoomManager.settings.initialPickups;
            if (initialPickups > 0)
            {
                int dropPointsCount = dropPoints.Length;
                if (initialPickups > dropPointsCount)
                {
                    Debug.LogWarning("initialPickups > dropPointsCount, it will be reduced to dropPointsCount since we do not allow things to be spawn on top of each other");
                    initialPickups = dropPointsCount;
                }
                //NOTE: This may not be a very efficient way of doing this but we do it once a game.
                List<int> availableDropPointIndexes = new List<int>(dropPointsCount);
                for (int i = 0; i < dropPointsCount; i++)
                {
                    availableDropPointIndexes.Add(i);
                }
                for (int i = 0; i < initialPickups; i++)
                {
                    int randomIndex = Random.Range(0, availableDropPointIndexes.Count);
                    Vector3 dropPoint = dropPoints[availableDropPointIndexes[randomIndex]].position;
                    DropItem(dropPoint);
                    availableDropPointIndexes.RemoveAt(randomIndex);
                }
            }

        }

        private IEnumerator ItemDropRoutine()
        {
            previousRoutine = nameof(ItemDropRoutine);
            //HARDCODED: wait times are arbitrary
            state = KevinStates.DroppingItems;//TODO: Should we change state here..?
            navAgent.enabled = true;
            //NOTE/TODO: Kevin will change his destination if his journey was interrupted by laughter. is this what we want?
            ChangeDestination();//TODO: might need more than that
                                // animator.SetBool("IsWalking", true);
            animator.SetInteger(AnimatorParameters.State, (int)KevinAnimationStates.Walk);

            while (state == KevinStates.DroppingItems) //TODO: Hmm maybe first condition should do with GameManager's state
            {
                Vector3 dropPoint = dropPoints[nextDropPointIndex].position;//TODO: We could cache this for performance but what if drop point is supposed to be dynamic?
                float squaredDistanceFromDestination = Vector3.SqrMagnitude(myTransform.position - dropPoint);
                if (squaredDistanceFromDestination < REQUIRED_DROP_POINT_DISTANCE)
                {
                    DropItem(dropPoint);
                    //NOTE: This wait is here mainly to ensure that OverlapSphere performed in ChangeDestination 
                    //detects the thing we just dropped...
                    //TODO: cache WaitForSeconds
                    yield return new WaitForSeconds(0.1f);
                    while (!ChangeDestination())
                    {
                        yield return new WaitForSeconds(0.5f);
                    }
                }
                yield return new WaitForSeconds(0.1f);
            }

        }

        public static void TryLaughAt(Transform embarrassmentTransform)
        {
            //HARDCODING AHEAD:
            //TODO: Maybe do a raycast to make sure Kevin can actually see the event?

            float distanceFromEmbarrassment =
                Vector3.Distance(instance.myTransform.position, embarrassmentTransform.position);
            float maxDistance = 4f;
            if (distanceFromEmbarrassment < maxDistance)
            {
                instance.StopAllCoroutines();
                // instance.StopCoroutine(nameof(LaughAtRoutine));
                instance.StartCoroutine(instance.LaughAtRoutine(embarrassmentTransform, Random.Range(2.5f, 4f)));
            }
        }

        private IEnumerator LaughAtRoutine(Transform embarrassmentTransform, float embarrassmentDuration)
        {
            /*if(previousRoutine != null)
            {
                StopCoroutine(previousRoutine);
            }*/

            //NOTE/TODO: this is written to be played simultanuasly with ItemDropRoutine.
            //Kevin is meant to return to it once he stops laughing. Might be a good idea to independentise these routines in the future
            state = KevinStates.Laughing;
            navAgent.enabled = false;
            animator.SetInteger(AnimatorParameters.State, (int)KevinAnimationStates.Laugh);
            float timePassed = 0;
            float rotationSpeed = navAgent.angularSpeed;
            while (timePassed < embarrassmentDuration)
            {
                //TODO: Is this not too costly?
                float deltaTime = Time.deltaTime;
                Vector3 embarrassmentPosition = embarrassmentTransform.position;
                Vector3 myPosition = myTransform.position;
                embarrassmentPosition.y = 0;
                myPosition.y = 0;
                Vector3 forward = (embarrassmentPosition - myPosition).normalized;
                Quaternion destinationRotation = Quaternion.LookRotation(forward);
                myTransform.rotation =
                    Quaternion.RotateTowards(myTransform.rotation, destinationRotation, rotationSpeed * deltaTime);
                timePassed += deltaTime;//TODO: use smaller steps for performance>?
                yield return null;
            }
            if (previousRoutine != null)
            {
                StartCoroutine(previousRoutine);
            }
        }

        [Server]
        public void StartDropRoutine()
        {

            StartCoroutine(ItemDropRoutine());
        }

        [Server]
        private void DropItem(Vector3 dropPosition)
        {
            //TODO: דיויד, וודא שההגרלה יוצאת כמו שמצופה והחלף את המודל אם יש צורך
            int chance = Random.Range(0, droppableItemsOverallChancePoints);
            //Debug.Log($"chance = {chance}");
            Spawnables spawnable = Spawnables.Null;
            for (int i = 0; i < droppableItems.Length; i++)
            {
                if (chance < droppableItems[i].chanceThreshold)
                {
                    spawnable = droppableItems[i].spawnable;
                    break;
                }
            }
            if (spawnable == Spawnables.Null)
            {
                Debug.LogError("Thine chance model did not work after all!");
                return;
            }
            //int droppedItemIndex = Random.Range(0, allowedDroppedItems.Length);
            // Vector3 itemPosition = dropPoints[nextDropPointIndex].position; //myTransform.position;// + (myTransform.forward * -1.1f);//HARDCODED
            TagNetworkManager.Spawner.Spawn(spawnable, dropPosition, Quaternion.identity, null);
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
                int colliderCount = Physics.OverlapSphereNonAlloc(dropPoint, availableDropPointCheckRadius, availableDropPointCheckColliders);
                if (colliderCount == availableDropPointCheckColliders.Length)
                {
                    Debug.LogWarning("colliderCount == availableDropPointCheckColliders.Length");
                }
                for (int j = 0; j < colliderCount; j++)
                {
                    if (availableDropPointCheckColliders[j].GetComponentInParent<PickUp>() != null)
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
        public IEnumerator SpinCoroutine(Vector3 lookAtPoint)
        {
            state = KevinStates.ChoosingTagger;

            animator.SetInteger(AnimatorParameters.State, (int)KevinAnimationStates.Point);
            //networkAnimator.SetTrigger("PointAt");
            yield return new WaitForSeconds(0.25f);
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

        private IEnumerator IdleRoutine()
        {
            state = KevinStates.Idle;
            navAgent.enabled = false;
            animator.SetInteger(AnimatorParameters.State, (int)KevinAnimationStates.Idle);
            WaitForSeconds waitForSeconds = new WaitForSeconds(2f);
            while (state == KevinStates.Idle)
            {
                //Debug.Log("Kevin is Idle");
                yield return waitForSeconds;
            }
        }

    }

}
