using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class PlayerController : NetworkBehaviour
{
   /* private enum PlayerActions
    {
        InputMovement,AutomaticMovement;
    }*/
    [SerializeField] private CharacterController characterController;
    private Transform myTransform;
    private Joystick joystick;

    [SerializeField] private float maxMovementSpeed;
    [SerializeField] private float maxRotationSpeed;
    private Quaternion desiredRotation;
    private Vector3 velocity;
    [SerializeField] private float gravity = -9.8f;
    private const float MIN_SQUARED_WAYPOINT_DISTANCE =0.05f;
    #region Path Finding:

    private NavMeshPath pathCache;
    private Vector3[] pathWayPoints;
    private int pathWayPointsCount;
    private Vector3 currentWayPoint;
    private int currentPathIndex;
    private InteractableAccessPoint desiredInteractableAccessPoint;
    private bool PathFindingIsActive
    {
        get
        {
            return (desiredInteractableAccessPoint != null);
        }
    }
    #endregion
    [SerializeField] private Transform cameraAnchor;
     private Camera camera;

    [SerializeField] private Animator animator;
    private Liftable liftable;// TODO: make it more generic;
    [SerializeField] private Transform grabbingHand;
    //private bool isWalking;

    private void Start()
    {
        if (isLocalPlayer)
        {
            myTransform = transform;
            pathCache = new NavMeshPath();
            pathWayPoints = new Vector3[16];
            joystick = FindObjectOfType<Joystick>();
            CharacterCamera characterCamera = FindObjectOfType<CharacterCamera>();
            characterCamera.Initialise(myTransform, cameraAnchor.localPosition);
            camera = characterCamera.GetComponent<Camera>();
        }


    }

    private void Update()
    {
        if (isLocalPlayer)
        {
            float deltaTime = Time.deltaTime;

            if (Input.GetMouseButtonDown(0))
            {
                Interactable interactable = FindInteractable();
                if (interactable != null)
                {
                    InteractableAccessPoint accessPoint =
                        interactable.GetClosestAccessPoint(myTransform.position);
                    ActivateNavMesh(accessPoint);
                }
            }
        }
    }

    private void FixedUpdate()
    {
        if (isLocalPlayer)
        {
            float deltaTime = Time.fixedDeltaTime;
            HandleMovement(ref deltaTime);
        }
    }

    private Interactable FindInteractable()
    {
        Interactable interactable = null;
        Vector2 mousePosition = Input.mousePosition;
        Ray ray = camera.ScreenPointToRay(mousePosition);
        RaycastHit raycastHit;
        
        if (Physics.Raycast(ray, out raycastHit))
        {
            interactable = raycastHit.collider.GetComponentInParent<Interactable>();
            if (interactable != null)
            {
                Debug.Log("Interactable found!");
            }
        }

        return interactable;
    }

    private void ActivateNavMesh(InteractableAccessPoint interactableAccessPoint)
    {
        desiredInteractableAccessPoint = interactableAccessPoint;
        
        NavMesh.CalculatePath(myTransform.position, interactableAccessPoint.myTransform.position, NavMesh.AllAreas, pathCache);
        pathWayPointsCount = pathCache.corners.Length;
        if (pathWayPoints.Length < pathWayPointsCount)
        {
            Debug.LogError("pathWayPoints.Length < pathCache.corners.Length!");
            return;
        }
        else
        {

            for (int i = 0; i < pathWayPointsCount; i++)
            {
                pathWayPoints[i] = pathCache.corners[i];
            }
            currentPathIndex = 0;
            currentWayPoint = pathWayPoints[currentPathIndex];
        }
    }

    private void DeactivateNavMesh()
    {
        desiredInteractableAccessPoint = null;
    }

    private void HandleMovement(ref float deltaTime)
    {

        float verticalInput = joystick.Vertical;
        float horizontalInput = joystick.Horizontal;
        Vector3 movement = new Vector3();
        //Debug.Log($"Vertical: {verticalInput.ToString("f2")}  + Horizontal: {horizontalInput.ToString("f2")}");

        if (verticalInput != 0 || horizontalInput != 0)
        {
            Vector3 inputVector3 = new Vector3(horizontalInput, 0, verticalInput);
            desiredRotation = Quaternion.LookRotation(inputVector3);

            movement = inputVector3 * maxMovementSpeed;// * deltaTime;
            DeactivateNavMesh();
           /* if (!isWalking)
            {
                animator.SetBool("IsWalking",true);
                isWalking = true;
            }*/
            //Debug.Log($"movement magnitude: {movement.magnitude}"); Magnitude seems good!
        }

        #region Gravity
        /* if (navMeshAgent..isGrounded)
         {
             velocity.y = 0;
         }
         else
         {
             velocity.y += gravity * deltaTime;
         }*/
        #endregion

        if (PathFindingIsActive)
        {
            Vector3 difference = (currentWayPoint - myTransform.position);
            float squaredDistance = Vector3.SqrMagnitude(difference);

            if (squaredDistance < MIN_SQUARED_WAYPOINT_DISTANCE)
            {
                currentPathIndex++;

                if (currentPathIndex >= pathWayPointsCount)
                {
                    desiredRotation = desiredInteractableAccessPoint.myTransform.rotation;
                    //desiredInteractableAccessPoint.Interact(this);
                    uint id = desiredInteractableAccessPoint.GetInterractable().netId;
                    CmdTryInteract(id);
                    DeactivateNavMesh();
                }
                else
                {
                    currentWayPoint = pathWayPoints[currentPathIndex];
                }
            }
            else
            {

                movement = difference.normalized * maxMovementSpeed;// * deltaTime;
                velocity.x = movement.x;
                velocity.z = movement.z;

                difference.y = 0;
                desiredRotation = Quaternion.LookRotation(difference);
            }
        }

        velocity.x = movement.x;
        velocity.z = movement.z;

        characterController.Move(velocity * deltaTime);
        myTransform.rotation = Quaternion.RotateTowards
            (myTransform.rotation, desiredRotation, maxRotationSpeed * deltaTime);

        float velocitySquaredMagnitude = velocity.sqrMagnitude;
        float maxVelocitySquaredMagnitude = maxMovementSpeed * maxMovementSpeed;//TODO: Pre-Calculate
        //Debug.Log($"velocitySquaredMagnitude: {velocitySquaredMagnitude.ToString("f2")}");
        animator.SetFloat("Speed", velocitySquaredMagnitude / maxVelocitySquaredMagnitude);
       // Debug.Log($"navMeshVelocity: {navMeshAgent.velocity}");
    }

    [Command]
    private void CmdTryInteract(uint interactableID)
    {
        //desiredInteractableAccessPoint.Interact(this);
        Debug.Log("CmdTryInteract");

        Interactable interactable = NetworkIdentity.spawned[interactableID].GetComponent<Interactable>();
        float distance = Vector3.Distance(myTransform.position, interactable.transform.position);
        if (distance < 2)//TODO: Add serious check
        {
            Debug.Log("interactable.Interact(/*this*/);");
            interactable.Interact(/*this*/);
        }
        else
        {
            Debug.LogWarning($"distance = {distance.ToString("f3")}, illegal interaction");

        }
    }

    public void Lift(Liftable liftable)
    {
        animator.SetTrigger("Lift");
        this.liftable = liftable;      
    }

    public void GrabLiftable()
    {
        liftable.Graphics.SetParent(grabbingHand);
    }

    public void ReleaseLiftable()
    {
        liftable.Graphics.SetParent(grabbingHand);
    }
}
